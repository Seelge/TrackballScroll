TrackballScroll
===============
**Allow scrolling with a trackball without scroll wheel by using a low level mouse hook.**

Changes the behaviour of both X-Buttons (typically buttons 3 and 4) to
- Scrolling, i.e. vertical/horizontal mouse wheel events, when an X-Button is pressed and the trackball is moved vertically/horizontally
- Middle button click, when an X-Button is pressed and released without trackball movement

###### Requirements
- A trackball or mouse with X-Buttons
- A Microsoft Windows operating system with .NET 4.5.2

This software has been tested with a *Logitech Marble Trackball*(tm) and *Microsoft Windows 10*.

###### Download the latest release
https://github.com/Seelge/TrackballScroll/releases/latest

###### Run the program
- Execute `TrackballScroll.exe`, no installation is necessary. A console window is displayed while the program is running, close it to disable the mouse hook.
- When using this program with a driver software that allows customization of the button behavior, make sure to set the X-Buttons to default behavior. E.g. with a *Logitech Trackman Marble*, make sure to set the buttons 3 and 4 to `default` button behaviour and not `back`/`Universal Scroll`.
- The lines scrolled per wheel event are determined by the *Microsoft Windows* mouse wheel settings.

###### Compile the source code
- Clone the repository from the github page or download the latest source code archive
- Open the solution with *Microsoft Visual Studio 2015*.
- Change the build type to `Release`. The program works in both 64 and 32 bit programs.
- Build the solution

###### Version history
- v2.1.1 fixes issues #8, #9, #10. Instead of calculating the scaled coordinates, both original and scaled coordinates are memorized.
- v2.1.0 fixes issue #5 with high dpi scaling. Note: The app must be restartet after changing the scaling factor in windows.
- v2.0.1 Added an application icon and moved all strings to a resource file.
- v2.0.0 The application is accessible through a systray-icon, the console window is gone. Converted from C++ to C#.
- v1.1.0 Add horizontal scroll 
- v1.0.1 Add info to the console window
