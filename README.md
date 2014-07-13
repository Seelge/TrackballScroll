TrackballScroll
===============
**Allow scrolling with a trackball without a scroll wheel by using a low level mouse hook.**

Changes the behaviour of both X-Buttons (typically buttons 3 and 4) to
- scrolling, i.e. vertical mouse wheel events, when an X-Button is pressed and the trackball is moved
- middle button click, when an X-Button is pressed and released without trackball movement

Requirements:
- a trackball or mouse with X-Buttons
- a Microsoft Windows operating system

This software has been tested with a *Logitech Trackman Marble*(tm) and *Microsoft Windows 8.1* and *7*.
When using this program with a driver software that allows customization of the button behavior, make sure to set the X-Buttons to default behavior. E.g. with a *Logitech Trackman Marble*, make sure to set the buttons 3 and 4 to `default` button behaviour and not `back`/`Universal Scroll`.

The program displays an empty console window while it is running.
