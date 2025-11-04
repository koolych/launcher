<p align="center">
 <h2 align="center">ClassicCounter Launcher</h2>
 <p align="center">
   Launcher for ClassicCounter with Discord RPC, Auto-Updates and More!
   <br/>
   Written in C# using .NET 8.
 </p>
</p>

[![Downloads][downloads-shield]][downloads-url]
[![Stars][stars-shield]][stars-url]
[![Issues][issues-shield]][issues-url]
[![MIT License][license-shield]][license-url]

> [!IMPORTANT]
> .NET Runtime 8 is required to run the launcher. Download it from [**here**](https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/runtime-8.0.11-windows-x64-installer).

## Arguments
- `--debug-mode` - Enables debug mode, prints additional info.
- `--skip-updates` - Skips checking for launcher updates.
- `--skip-validating` - Skips validating patches.
- `--validate-all` - Validates all game files.
- `--patch-only` - Will only check for patches, won't open the game.
- `--disable-rpc` - Disables Discord RPC.
- `--gc` - Launches the Game with custom Game Coordinator.
- `--install-dependencies` - Launches setup process for required Game dependencies.

> [!CAUTION]
> **Using `--skip-updates` or `--skip-validating` is NOT recommended!**  
> **An outdated launcher or patches might cause issues.**

## Packages Used
- [CSGSI](https://github.com/rakijah/CSGSI) by [rakijah](https://github.com/rakijah)
- [DiscordRichPresence](https://github.com/Lachee/discord-rpc-csharp) by [Lachee](https://github.com/Lachee)
- [Downloader](https://github.com/bezzad/Downloader) by [bezzad](https://github.com/bezzad)
- [Refit](https://github.com/reactiveui/refit) by [ReactiveUI](https://github.com/reactiveui)
- [Spectre.Console](https://github.com/spectreconsole/spectre.console) by [Spectre Console](https://github.com/spectreconsole)
- [Gameloop.Vdf](https://github.com/shravan2x/Gameloop.Vdf) by [shravan2x](https://github.com/shravan2x)

[downloads-shield]: https://img.shields.io/github/downloads/koolych/launcher/total.svg?style=for-the-badge
[downloads-url]: https://github.com/koolych/launcher/releases/latest
[stars-shield]: https://img.shields.io/github/stars/koolych/launcher.svg?style=for-the-badge
[stars-url]: https://github.com/koolych/launcher/stargazers
[issues-shield]: https://img.shields.io/github/issues/koolych/launcher.svg?style=for-the-badge
[issues-url]: https://github.com/koolych/launcher/issues
[license-shield]: https://img.shields.io/github/license/koolych/launcher.svg?style=for-the-badge
[license-url]: https://github.com/koolych/launcher/blob/main/LICENSE.txt
