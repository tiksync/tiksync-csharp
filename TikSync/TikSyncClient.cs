using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TikSync
{
    public class TikSyncClient
    {
        private const string DefaultApiUrl = "https://api.tik-sync.com";

        private readonly string _uniqueId;
        private readonly string _apiKey;
        private readonly string _apiUrl;
        private readonly int _maxReconnectAttempts;
        private readonly int _reconnectDelay;
        private readonly ConcurrentDictionary<string, List<Action<Dictionary<string, object>>>> _listeners = new();
        private ClientWebSocket _ws;
        private bool _running;
        private int _reconnectCount;
        private CancellationTokenSource _cts;
        private readonly SecurityCoreTikSync _security;
        private readonly NativeCoreTikSync _native;

        public TikSyncClient(string uniqueId, string apiKey, string apiUrl = DefaultApiUrl, int maxReconnectAttempts = 3, int reconnectDelay = 5000)
        {
            _uniqueId = uniqueId;
            _apiKey = apiKey;
            _apiUrl = apiUrl;
            _maxReconnectAttempts = maxReconnectAttempts;
            _reconnectDelay = reconnectDelay;
            _security = !string.IsNullOrEmpty(apiKey) ? new SecurityCoreTikSync(apiKey) : null;
            if (NativeCoreTikSync.IsAvailable && !string.IsNullOrEmpty(apiKey))
            {
                try { _native = new NativeCoreTikSync(apiKey); } catch { }
            }
        }

        public TikSyncClient On(string eventName, Action<Dictionary<string, object>> handler)
        {
            _listeners.GetOrAdd(eventName, _ => new List<Action<Dictionary<string, object>>>()).Add(handler);
            return this;
        }

        private void Emit(string eventName, Dictionary<string, object> data)
        {
            if (_listeners.TryGetValue(eventName, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    Task.Run(() => handler(data));
                }
            }
        }

        public async Task ConnectAsync()
        {
            _running = true;
            _reconnectCount = 0;
            _cts = new CancellationTokenSource();
            await ConnectWsAsync();
        }

        private async Task ConnectWsAsync()
        {
            var wsUrl = _apiUrl.Replace("https://", "wss://").Replace("http://", "ws://");
            var uri = new Uri($"{wsUrl}/v1/connect?uniqueId={_uniqueId}");

            _ws = new ClientWebSocket();
            if (!string.IsNullOrEmpty(_apiKey))
            {
                _ws.Options.SetRequestHeader("x-api-key", _apiKey);
            }
            if (_native != null)
            {
                foreach (var h in _native.GetConnectHeaders())
                    _ws.Options.SetRequestHeader(h.Key, h.Value);
            }
            else if (_security != null)
            {
                foreach (var h in _security.GetConnectHeaders())
                    _ws.Options.SetRequestHeader(h.Key, h.Value);
            }

            try
            {
                await _ws.ConnectAsync(uri, _cts.Token);
                _reconnectCount = 0;
                Console.WriteLine($"[TikSync] Connected to {_uniqueId}");
                Emit("connected", new Dictionary<string, object> { { "uniqueId", _uniqueId } });

                var buffer = new byte[4096];
                var messageBuffer = new StringBuilder();

                while (_ws.State == WebSocketState.Open && !_cts.Token.IsCancellationRequested)
                {
                    var result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cts.Token);

                    if (result.MessageType == WebSocketMessageType.Close)
                        break;

                    messageBuffer.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                    if (result.EndOfMessage)
                    {
                        var message = messageBuffer.ToString();
                        messageBuffer.Clear();

                        try
                        {
                            var eventObj = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(message);
                            var type = eventObj.ContainsKey("type") ? eventObj["type"].GetString() : "unknown";
                            var data = new Dictionary<string, object>();

                            if (eventObj.ContainsKey("data") && eventObj["data"].ValueKind == JsonValueKind.Object)
                            {
                                foreach (var prop in eventObj["data"].EnumerateObject())
                                    data[prop.Name] = prop.Value.ToString();
                            }

                            Emit(type, data);
                        }
                        catch { }
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (WebSocketException ex)
            {
                if (_running)
                    Emit("error", new Dictionary<string, object> { { "error", ex.Message } });
            }
            catch (Exception ex)
            {
                Emit("error", new Dictionary<string, object> { { "error", ex.Message } });
            }
            finally
            {
                try { _ws?.Dispose(); } catch { }
                _ws = null;
            }

            if (_running)
                await TryReconnectAsync();
        }

        private async Task TryReconnectAsync()
        {
            if (_reconnectCount >= _maxReconnectAttempts)
            {
                Emit("disconnected", new Dictionary<string, object> { { "reason", "max_reconnect" } });
                return;
            }

            _reconnectCount++;
            var delay = _reconnectDelay * _reconnectCount;
            Console.WriteLine($"[TikSync] Reconnecting in {delay}ms (attempt {_reconnectCount}/{_maxReconnectAttempts})");
            await Task.Delay(delay);
            await ConnectWsAsync();
        }

        public async Task DisconnectAsync()
        {
            _running = false;
            _cts?.Cancel();
            try
            {
                if (_ws?.State == WebSocketState.Open)
                    await _ws.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "disconnect", CancellationToken.None);
            }
            catch { }
            try { _ws?.Dispose(); } catch { }
            _ws = null;
            Emit("disconnected", new Dictionary<string, object> { { "reason", "manual" } });
        }
    }
}
