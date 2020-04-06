# ActiveDesktopPlus
A simple app that lets you pin windows to your desktop and use fullscreen programs as interactive wallpapers.

## How do I use this?
0. Make sure desktop icons are turned off. These get in the way otherwise.
1. Focus the app, hold B and hover over the titlebar of the app you wish to send to the desktop.
2. Click on "Send to Desktop"
3. If it worked, the app should now appear under "Current Apps" and also have the Aero Basic theme.
4. Select a window from the list on the "Current Apps" tab then use the Pin/Unpin buttons to toggle its borders and the ability to move the window.
5. Alternatively if your app supports a fullscreen mode, enabling it should work fine.

## What apps can I use this with?
This utility should work with most applications. It's not compatible with UWP apps or anything that draws its own titlebar though. Some good apps to try are browsers, video players such as Windows Media Player or VLC, or terminal emulators such as Windows Terminal or ConHost. Since fullscreen apps are drawn behind the taskbar, [TranslucentTB](https://github.com/TranslucentTB/TranslucentTB) works wonders to help improve the overall appearance.

## Does it break easily?
Yes, it was written by me using my terrible approach to problem-solving. However regardless of this there are still plenty of bugs that need to be ironed out. At the moment, you can use this to child any element of any window to the desktop, including text boxes, buttons, sections of the taskbar etc., all with varying degrees of hilarity. However this can occasionally cause the tool, Explorer or other apps to crash. Be careful, if you break something it's your own fault.

## Why can't I do `x` with it?
Because I've not added that feature. Feel free to complain at me or add it yourself though. It's also worth noting that I've only tested this on Windows 10 version 2004, I cannot guarantee compatibility on any other version. Microsoft are weird and keep moving around the order that the desktop windows are in. If it doesn't work, file an issue or something.
