TrackballScroll
===============
**Allow scrolling with a trackball without scroll wheel by using a low level mouse hook.**

Changes the behaviour of both X-Buttons (typically buttons 3 and 4) to
- Scrolling, i.e. vertical mouse wheel events, when an X-Button is pressed and the trackball is moved
- Middle button click, when an X-Button is pressed and released without trackball movement

######Requirements
- A trackball or mouse with X-Buttons
- A Microsoft Windows operating system

This software has been tested with a *Logitech Trackman Marble*(tm) and *Microsoft Windows 8.1* and *7*.

######Compiling
- Clone the repository
- Open the solution with *Microsoft Visual Studio 2013*. If you are using an older version, create a new solution and add the missing files.
- Change the build type to `Release` and `x64`. The program also works in both 64 and 32 bit programs.
- Build the solution

######Running
- Execute the program. An empty console window is displayed while it is running.
- When using this program with a driver software that allows customization of the button behavior, make sure to set the X-Buttons to default behavior. E.g. with a *Logitech Trackman Marble*, make sure to set the buttons 3 and 4 to `default` button behaviour and not `back`/`Universal Scroll`.
- The lines scrolled per wheel event are determined by the *Microsoft Windows* mouse wheel settings.
