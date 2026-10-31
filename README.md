# StatBot [<img src="https://img.shields.io/vso/build/moonraven/5557531f-8a79-4c7b-bde1-75757f001741/5.svg?style=flat-square"/>]
A Discord Bot which logs in the HydraIRC complient log format so that mIRCStats will work. Slightly modified parser can be found in the mIRCStats parser folder. 

Download: [![GitHub release](https://img.shields.io/github/release/mjheijster/statbot.svg?style=flat-square)](https://github.com/MJHeijster/StatBot/releases/latest)

# Setup
Compile (or download) and place in a location you want the software in. Configure the application through the appsettings.json file. More information can be found here about the token: https://github.com/reactiflux/discord-irc/wiki/Creating-a-discord-bot-&-getting-a-token

**Important**: when logged into the portal, go to "Bot" in the left menu and enable "Message Content Intent". In my short test, I had to restart the bot, but that might not be needed. I understand that it was announced a year ago, but I also wasn't aware of it.

You can either let the bot log to channelname.log or channelid.log. You can figure out what channel belongs to a channelid by opening the log and checking the first line, or by writing \#channelname in Discord. It will show something like <#296923759988703232> where 296923759988703232 is the id.

If you use StatBot to generate the stats through GeneratorFile, make sure to run StatBot through your own account or one that has the FTP credentials uploaded if you use FTP. The FTP credentials are encrypted in mIRCStats, so other accounts will not be able to upload.

# Tips
If a channel is renamed and the bot is set to log to channelname.log, you can use a script to move the old and new log files into a folder. If you specify the channel name in the first log (for this example, 1.log) through the following line: 

"*** Now talking in #channelname"

mIRCStats will recognize that name.

Example batch script for combining multiple logs:

> - mkdir C:\Discord\StatBot\277418737798479872\general
> - copy "C:\Discord\StatBot\277418737798479872\general.log" "C:\Discord\StatBot\277418737798479872\general\1.log" /Y
> - copy "C:\Discord\StatBot\277418737798479872\the-toy-factory.log" "C:\Discord\StatBot\277418737798479872\general\2.log" /Y
> - START /WAIT C:\Discord\StatBot\mircstats\mircstats.exe -cfg general.cfg -log "C:\Discord\StatBot\277418737798479872\general\\*.log" -html Html\general.html

The logs are stored in a folder per server (the server id, 277418737798479872 in the example above). By default these folders are created in the folder StatBot runs from. Set LogPath in appsettings.json to store them somewhere else.

# Running on Linux
StatBot runs on Linux as well as Windows. The configuration tool (StatBotConfiguration) is Windows only, so on Linux edit appsettings.json by hand.

1. Install the [.NET 8 runtime](https://learn.microsoft.com/dotnet/core/install/linux).
2. Build it: `dotnet publish StatBot/StatBot.csproj -c Release -r linux-x64 --self-contained false -o ~/statbot`
3. Configure `~/statbot/appsettings.json`. Use Linux paths (e.g. `/home/user/statbot/mircstats`), not Windows paths with backslashes. StatBot shows a warning at startup if a path contains a backslash.
4. Run it from its folder, since the database is stored relative to it: `cd ~/statbot && dotnet StatBot.dll`

mIRCStats is a Windows program, but it runs under [Wine](https://www.winehq.org/). To let StatBot generate the stats, point GeneratorFile to an executable script (`chmod +x generate-stats.sh`), for example:

```sh
#!/bin/sh
cd /home/user/statbot/mircstats
wine mircstats.exe -cfg general.cfg -log "Z:\\home\\user\\statbot\\277418737798479872\\general.log" -html Html/general.html
```


# Notes
Want to help or need help? Join the discord server https://discord.gg/SFYVQNE , create an issue or create a pull request.

You can also donate at https://www.paypal.me/mjheijster.

Font used for the icon: Archicoco
