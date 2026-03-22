using TikSync;

var client = new TikSyncClient("croc_mi", "sl_int_synclive_electron_prod_2026", apiUrl: "https://api.synclive.fr");
var count = 0;
var cts = new CancellationTokenSource();

client.On("connected", data => Console.WriteLine("[C#] connected: OK"));
client.On("roomInfo", data => Console.WriteLine("[C#] roomInfo: OK"));
client.On("chat", data => {
    Console.WriteLine($"[C#] chat: OK - {(data.ContainsKey("uniqueId") ? data["uniqueId"] : "?")}");
    if (++count >= 3) cts.Cancel();
});
client.On("gift", data => Console.WriteLine("[C#] gift: OK"));
client.On("member", data => Console.WriteLine("[C#] member: OK"));
client.On("roomUser", data => Console.WriteLine("[C#] roomUser: OK"));
client.On("error", data => {
    Console.WriteLine($"[C#] ERROR: {(data.ContainsKey("error") ? data["error"] : "unknown")}");
    cts.Cancel();
});
client.On("disconnected", data => {
    Console.WriteLine($"[C#] disconnected: {(data.ContainsKey("reason") ? data["reason"] : "?")}");
    cts.Cancel();
});

Console.WriteLine("Connecting to croc_mi...");
_ = client.ConnectAsync();

try { await Task.Delay(15000, cts.Token); } catch (OperationCanceledException) { }
await client.DisconnectAsync();
