# TikSync C# / .NET SDK

**TikTok Live SDK for C# / .NET** - Real-time chat, gifts, likes, follows & viewer events.

[![NuGet](https://img.shields.io/nuget/v/TikSync.svg)](https://www.nuget.org/packages/TikSync)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Discord](https://img.shields.io/discord/1487514051427700886?label=Discord&logo=discord&logoColor=white)](https://discord.gg/2RkymdBNa7)

[Website](https://tik-sync.com) - [Documentation](https://tik-sync.com/docs) - [Pricing](https://tik-sync.com/pricing)

---

## Why TikSync?

- **No Puppeteer/Chromium** - Pure Rust signing engine, no browser dependency
- **Fast** - Sub-millisecond signature generation
- **Production-ready** - Used by 50+ TikTok Live streamers daily
- **Built-in reliability** - Auto-reconnection and error handling
- **6 SDKs, 1 API** - Same design across JS, Python, Go, Rust, Java, C#

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

## All SDKs

| Language | Package | Install |
|----------|---------|---------|
| JavaScript | [npm](https://www.npmjs.com/package/tiksync) | `npm install tiksync` |
| Python | [PyPI](https://pypi.org/project/tiksync/) | `pip install tiksync` |
| Go | [go.dev](https://pkg.go.dev/github.com/tiksync/tiksync-go) | `go get github.com/tiksync/tiksync-go` |
| Rust | [crates.io](https://crates.io/crates/tiksync) | `cargo add tiksync` |
| Java | [Maven Central](https://central.sonatype.com/artifact/io.github.0xwolfsync/tiksync) | See docs |
| C# | [NuGet](https://www.nuget.org/packages/TikSync) | `dotnet add package TikSync` |

## Community

- [Discord](https://discord.gg/2RkymdBNa7) - Get help and chat with other developers
- [Documentation](https://tik-sync.com/docs) - Full API reference
- [Blog](https://tik-sync.com/blog) - Technical deep-dives
- [Status](https://tik-sync.com/status) - Service uptime

## License

MIT - Built by [TikSync](https://tik-sync.com)
