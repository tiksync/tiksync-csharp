# TikSync C# / .NET SDK

**TikTok Live SDK for C# / .NET** - Real-time chat, gifts, likes, follows & viewer events.

[Website](https://tik-sync.com) - [Documentation](https://tik-sync.com/docs) - [Pricing](https://tik-sync.com/pricing)

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
