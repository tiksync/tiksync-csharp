<p align="center">
  <img src="https://raw.githubusercontent.com/tiksync/.github/main/profile/logo-96.png" width="60" alt="TikSync" />
</p>

<h1 align="center">TikSync C# / .NET SDK</h1>

<p align="center">
  <strong>TikTok Live SDK for C# / .NET</strong> - Real-time chat, gifts, likes, follows & viewer events.<br>
  <a href="https://tik-sync.com">Website</a> - <a href="https://tik-sync.com/docs">Documentation</a> - <a href="https://tik-sync.com/pricing">Pricing</a>
</p>

---

## Installation

```bash
dotnet add package TikSync
```

Or via NuGet Package Manager:
```
Install-Package TikSync
```

## Quick Start

```csharp
using TikSync;

var client = new TikSyncClient("username", "your_api_key");

client.On("chat", data => {
    Console.WriteLine($"[{data["uniqueId"]}] {data["comment"]}");
});

client.On("gift", data => {
    Console.WriteLine($"{data["uniqueId"]} sent {data["giftName"]}");
});

await client.ConnectAsync();
```

Requires .NET 6+. Works with Unity 2021.3+.

## Events

| Event | Description |
|-------|-------------|
| `connected` | Connected to stream |
| `chat` | Chat message received |
| `gift` | Gift received (with diamond count, streak info) |
| `like` | Likes received |
| `follow` | New follower |
| `share` | Stream shared |
| `member` | User joined the stream |
| `roomUser` | Viewer count update |
| `streamEnd` | Stream ended |
| `disconnected` | Disconnected |
| `error` | Connection error |

## Get Started

1. Sign up at [tik-sync.com](https://tik-sync.com)
2. Create a free API key in your dashboard
3. Install the SDK and start building

Free tier available. See [pricing](https://tik-sync.com/pricing) for details.

## License

MIT - Built by [0xwolfsync](https://tik-sync.com)
