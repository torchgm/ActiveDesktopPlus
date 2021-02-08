# Active Desktop Plus
A simple app that lets you pin windows, videos and webpages to your desktop and use fullscreen programs as interactive wallpapers.

<img src="/img/Bing.png" width="250"> <img src="/img/Terminal.png" width="250"> <img src="/img/Shader.png" width="250">

### What does this do?
If you've ever looked into customising your desktop wallpaper further than what Windows allows, you may have come across a feature called "Active Desktop". It allowed you to use webpages and files as your wallpaper or overlay small windows on top of your desktop that stayed behind everything else. ADP attempts to reintroduce and supercharge this feature, allowing for any app, webpage or video you want to be kept on the desktop either as a full wallpaper or a small widget.

### What apps can I use this with?
This utility should work with most applications. It's generally not compatible with UWP apps or anything that draws its own titlebar though. Some good apps to try are browsers, video players such as Windows Media Player or VLC (if you need more control than the built-in video player), or terminal emulators such as Windows Terminal. Since fullscreen apps are drawn behind the taskbar, [TranslucentTB](https://github.com/TranslucentTB/TranslucentTB) works wonders to help improve the overall appearance.

# How do I use ADP?
To start, simply drag and drop an MP4, EXE or URL (for example from the address bar of your browser) onto ADP's main window, select a monitor, then click apply. It'll automatically configure itself as your wallpaper and set it to run when ADP starts. If you use a YouTube or ShaderToy URL, they'll automatically play muted, fullscreen and on loop. From this point, the ADP window can be safely closed and it will minimise to the notification area.

# What if I want to customise it?
If you wish for more control, you can select the Edit Further option, which will allow you to customise your wallpaper in more depth, including spanning it across all monitors, setting whether or not it should open with ADP, toggling the ability to interact with the wallpaper and lots more.

# What other instructions might I need?
ADP can be used in many different ways, you might find the following sets of instructions useful:

### Sending an application to the desktop
0. With ADP open and the focused window, hold <kbd>Ctrl</kbd>  and move the mouse cursor over the titlebar of the window you wish to send to the desktop.
1. Release <kbd>Ctrl</kbd>, then click the "Send To Desktop" button.
2. Press <kbd>Win</kbd>+<kbd>D</kbd> to go to the desktop, you should see the app on the desktop.

### Setting a video or webpage as your wallpaper manually
0. In ADP, click on the "Use Video" until the Command Line box reflects what you want to set.
1. Type the path to the video or webpage in the box provided.
2. If you have multiple monitors, use the "Select Monitor" button to choose the monitor you wish to have the wallpaper on, or select Span if you want it to span all your monitors.
3. Check "Start locked" (which will prevent you from closing the wallpaper accidentally via <kbd>Alt</kbd>+<kbd>F4</kbd> or interacting with it in other potentially unwanted ways), and "Start with ADP" so that it runs whenever ADP is opened.
4. Click "Add to saved apps".
5. Find it in the list of saved apps, select it and click "Launch".

# What properties can I change about an app?
The following properties are all available to change for any app in the config file as well as directly from the UI.

### Command Line
The path to the application to be started. Can accept anything relying on the PATH variable. Setting it to `MEDIA` or `WEB` will change the type of content the wallpaper consists of.
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
### Start Locked
Starts the application with interaction disabled.
### Start with ADP
Starts the app at the same time ADP starts.
### Start Pinned
Starts ADP without borders, and "pinned" to the desktop.
### Fix Alignment
Attempts to fix the position of an app if it appears partially or fully off-screen (Only useful on some multi-monitor setups, see "Does it break easily?" below)

When adding an app under the "Saved Apps" secion, the only required value is `Command Line`, everything else can be left as-is if not needed and it will be ignored. Videos (`MEDIA`) and webpages (`WEB`) will require a path to the content in the Flags box. Some apps may simply not work when they're saved due to how they handle their main window. It's worth experimenting to see what does and doesn't work. The configuration file is located at `%appdata%/ActiveDesktopPlus/saved.json`.

### Does it break easily?
I've tried to make it as stable as possible and have for the most-part succeeded. However, it was written by me using my terrible approach to problem-solving so there's a good chance it might just explode your PC. At the moment, you can use this to child any element of any window to the desktop, including text boxes, buttons, sections of the taskbar etc., all with varying degrees of hilarity. However this can occasionally cause the tool, Explorer or other apps to crash. Be careful, if you break something it's your own fault. Furthermore, it's worth noting you may run into issues with some fullscreen apps if you use a multi-monitor setup where primary monitor's top-left corner is not the top-left corner of the entire desktop area (press <kbd>PrtSc</kbd>, paste into an image editor, and if the top-left corner of your primary monitor isn't in the top-left corner of the image then this may apply to you). As such, the Fix Alignment options exist, but these may not work for all apps. I'm looking into ways around that.

### Why can't I do `x` with it?
Because I've not added that feature. Feel free to complain at me or add it yourself though. I cannot guarantee compatibility with versions of Windows older than 10 2004. Microsoft are weird and keep moving around the order that the desktop windows are in. If it doesn't work, file an issue or something.
