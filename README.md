# TikSync .NET SDK

**TikTok Live SDK for C# / .NET** — Real-time chat, gifts, likes, follows & viewer events.

```csharp
using TikSync;

var client = new TikSyncClient("charlidamelio", "your_api_key");

client.On("chat", data => {
    Console.WriteLine($"[{data["uniqueId"]}] {data["comment"]}");
});

client.On("gift", data => {
    Console.WriteLine($"{data["uniqueId"]} sent {data["giftName"]}");
});

await client.ConnectAsync();
```

## Installation

### NuGet
```bash
dotnet add package TikSync
```

### Package Manager
```powershell
Install-Package TikSync
```

## Unity

Works with Unity 2021.3+ using .NET Standard 2.1. Add the NuGet package or copy `TikSyncClient.cs` directly.

## License

MIT — built by [SyncLive](https://synclive.fr) | [tik-sync.com](https://tik-sync.com)
