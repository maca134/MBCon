# MBCon - [Download](https://github.com/maca134/MBCon/releases) #

MBCon is a C# (.NET) [BattlEye](http://www.battleye.com/) RCON client for ARMA 2/3 servers. It uses a simple plugin system to make it easy to extend. (BattleNET is included at the moment as there is a missing BE command, this will be changed to nuget soon).

### Included Plugins ###

- Console: Simply outputs stuff to a console window.
- Logger: Logs various BE events to logs.
- PlayerCheck: Can be used as a "global" bans list or a whitelister, using a file, http request or mysql database.
- BEFilterMonitor: Watches bans.txt and filters and reloads them when changed.
- RestartMessage: A simple plugin to do restart messages.
- ScheduledTasks: Perform BE commands at certain times.
- SimpleMessages: Sends messages to server at certain intervals.
- WebLogger: Sends logs to a URL.
- WebRcon: A very experimental plugin to allow access to a web-based RCON client.

### Basic Usage ###

Edit the main config.ini, pointing to the active servers be config and start mbcon.exe.

### Donate ###

If you like this app and use it, please consider donating via [PayPal](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=95G5FZ8PSW63W) or becoming a [Patreon](https://www.patreon.com/maca134).
