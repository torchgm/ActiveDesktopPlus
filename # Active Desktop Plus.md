# Active Desktop Plus
A simple app that lets you pin windows, videos and webpages to your desktop and use fullscreen programs as interactive wallpapers.

<img src="/img/1.png" width="250"> <img src="/img/2.png" width="250"> <img src="/img/3.png" width="250">

## What does this do?
If you've ever looked into customising your desktop wallpaper further than what Windows allows, you may have come across a feature called "Active Desktop". It allowed you to use webpages and files as your wallpaper or overlay small windows on top of your desktop that stayed behind everything else. ADP attempts to reintroduce and supercharge this feature, allowing for any app, webpage or video you want to be kept on the desktop either as a full wallpaper or a small widget.

## What apps can I use this with?
This utility should work with most applications. It's generally not compatible with UWP apps or anything that draws its own titlebar though. Some good apps to try are browsers, video players such as Windows Media Player or VLC (if you need more control than the built-in video player), or terminal emulators such as Windows Terminal. Since fullscreen apps are drawn behind the taskbar, [TranslucentTB](https://github.com/TranslucentTB/TranslucentTB) works wonders to help improve the overall appearance.

# How do I use ADP?

ADP's user interface is currently split into two sections; "Current apps" and "Saved apps". Both of these sections provide a key piece of functionality. Under "Current apps", you can view,  manage and close any apps currently on the desktop, as well as send more to the desktop. From "Saved apps", you can add custom apps to a quick-launch list, giving them flags, exact positions and sizes, have them run on startup and even set videos as your wallpaper.

## Current apps overview



## Saved apps overview
A saved app will automatically be opened alongside ADP and sent to the desktop. There are eight options that you can save right now:
### Command Line
The path to the application to be started. Can accept anything relying on the PATH variable.
### Flags
Any flags, arguments or switches you want to apply to the app at launch.
### X & Y
Specifies the location the window should be placed, with `0, 0` being the top-left of your primary monitor. Note that Windows inverts the Y axis, so down the screen is more
### Height & Width
Specifies the new size of the window.
### Friendly Name
What the application appears as in the Saved Apps list.
### Wait Time
How long ADP should wait for the application to have opened in milliseconds (default 1000). If this is is too low then the application's main window may open after ADP tries to send it to the desktop, so increase this if you have issues getting an app to work, or decrease this if you feel it stays open too long.

The only required value is `Command Line`, everything else can be left as-is if not needed and it will be ignored. Some apps may simply not work when they're saved due to how they handle their main window. It's worth experimenting to see what does and doesn't work. I will probably add more options and the ability to save an app that's already running in the future. The configuration file is located at `%appdata%/ActiveDesktopPlus/saved.json`.
## Does it break easily?
Yes, it was written by me using my terrible approach to problem-solving. However regardless of this there are still plenty of bugs that need to be ironed out. At the moment, you can use this to child any element of any window to the desktop, including text boxes, buttons, sections of the taskbar etc., all with varying degrees of hilarity. However this can occasionally cause the tool, Explorer or other apps to crash. Be careful, if you break something it's your own fault.

## Why can't I do `x` with it?
Because I've not added that feature. Feel free to complain at me or add it yourself though. It's also worth noting that I've only tested this on Windows 10 version 2004, I cannot guarantee compatibility on any other version. Microsoft are weird and keep moving around the order that the desktop windows are in. If it doesn't work, file an issue or something.