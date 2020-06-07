# ActiveDesktopPlus
A simple app that lets you pin windows to your desktop and use fullscreen programs as interactive wallpapers.

<img src="/img/1.png" width="250"> <img src="/img/2.png" width="250"> <img src="/img/3.png" width="250">

## How do I use this?
0. Make sure desktop icons are turned off. These get in the way otherwise.
1. Focus the app, hold B and hover over the titlebar of the app you wish to send to the desktop.
2. Click on "Send to Desktop"
3. If it worked, the app should now appear under "Current Apps" and also have the Aero Basic theme.
4. Select a window from the list on the "Current Apps" tab then use the Pin/Unpin buttons to toggle its borders and the ability to move the window.
5. Alternatively if your app supports a fullscreen mode, enabling it should work fine.

## What apps can I use this with?
This utility should work with most applications. It's not compatible with UWP apps or anything that draws its own titlebar though. Some good apps to try are browsers, video players such as Windows Media Player or VLC, or terminal emulators such as Windows Terminal or ConHost. Since fullscreen apps are drawn behind the taskbar, [TranslucentTB](https://github.com/TranslucentTB/TranslucentTB) works wonders to help improve the overall appearance.

## What's all this nonsense about saving apps?
A saved app will automatically be opened alongside ADP and sent to the desktop. There are eight options that you can save right now:
### Command Line
The path to the application to be started. Can accept anything relying on the PATH variable.
### Flags
Any flags, arguments or switches you want to apply to the app at launch.
### X & Y
Specifies the location the window should be placed. Does not consistently work yet.
### Height & Width
Specifies the new size of the window. Does not consistently work yet.
### Friendly Name
What the application appears as in the Saved Apps list.
### Wait Time
How long ADP should wait for the application to have opened in milliseconds (default 1000). If this is is too low then the application's main window may open after ADP tries to send it to the desktop, so increase this if you have issues getting an app to work.

The only required value is `Command Line`, everything else can be left as-is if not needed and it will be ignored. Some apps may simply not work when they're saved due to how they handle their main window. It's worth experimenting to see what does and doesn't work. I will probably add more options and the ability to save an app that's already running in the future. The configuration file is located at `%appdata%/ActiveDesktopPlus/saved.cti` one app per-line in the format `commandline§x§y§h§w§flags§name§time`.

## Does it break easily?
Yes, it was written by me using my terrible approach to problem-solving. However regardless of this there are still plenty of bugs that need to be ironed out. At the moment, you can use this to child any element of any window to the desktop, including text boxes, buttons, sections of the taskbar etc., all with varying degrees of hilarity. However this can occasionally cause the tool, Explorer or other apps to crash. Be careful, if you break something it's your own fault.

## Why can't I do `x` with it?
Because I've not added that feature. Feel free to complain at me or add it yourself though. It's also worth noting that I've only tested this on Windows 10 version 2004, I cannot guarantee compatibility on any other version. Microsoft are weird and keep moving around the order that the desktop windows are in. If it doesn't work, file an issue or something.
