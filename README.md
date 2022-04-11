<h1 align="center">BrackeysBot Core Plugin</h1>
<p align="center"><img src="https://avatars.githubusercontent.com/u/102218313?s=200&v=4"></p>
<p align="center"><i>The core plugin for <a href="https://github.com/oliverbooth/BrackeysBot">BrackeysBot</a> which offers fundamental and common functionality.</i></p>
<p align="center">
<a href="https://www.nuget.org/packages/BrackeysBot.Core/"><img src="https://img.shields.io/nuget/v/BrackeysBot.Core.API?label=stable%20build"></a>
<a href="https://www.nuget.org/packages/BrackeysBot.Core/"><img src="https://img.shields.io/nuget/vpre/BrackeysBot.Core.API?label=nightly%20build"></a>
<a href="https://github.com/BrackeysBot/BrackeysBot.Core/actions?query=workflow%3A%22.NET%22"><img src="https://img.shields.io/github/workflow/status/BrackeysBot/BrackeysBot.Core/.NET" alt="GitHub Workflow Status" title="GitHub Workflow Status"></a>
<a href="https://github.com/BrackeysBot/BrackeysBot.Core/issues"><img src="https://img.shields.io/github/issues/BrackeysBot/BrackeysBot.Core" alt="GitHub Issues" title="GitHub Issues"></a>
<a href="https://github.com/BrackeysBot/BrackeysBot.Core/blob/main/LICENSE.md"><img src="https://img.shields.io/github/license/BrackeysBot/BrackeysBot.Core" alt="MIT License" title="MIT License"></a>
</p>

## About
BrackeysBot.Core is the core plugin for [BrackeysBot](https://github.com/oliverbooth/BrackeysBot/). It provides a shared API which is used by a variety of other plugins, and implements functionality to allow administrators to load and unload other plugins dynamically - except for itself. The core plugin should not and cannot unload itself, as this would lead to the inability to reload itself.

## Contributing
Contributions are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for details.

## License
This plugin is under the [MIT License](LICENSE.md).

## Disclaimer
This plugin is tailored for use within the [Brackeys Discord server](https://discord.gg/brackeys). While this plugin is open source and you are free to use it in your own servers, you accept responsibility for any mishaps which may arise from the use of this software. Use at your own risk.
