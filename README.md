# WinAuto
WinAuto is small C# library for simulating basic **user input**(mouse and keyboard), **image needle-in-haystack search**, **clipboard handling** and some **basic system API functions**. 
  
All of this makes it ideal for macroing and automatization(games for example).

## Features
- Simulate keyboard and mouse input
- Image pattern searching(using needle images) on images(screenshots etc)
- Set and get Clipboard text
- A few basic WinAPI methods to take a screenshot, focus window or block user input
- Basic tests to show functionality

## Performance
Image search is multithreaded, which makes it pretty fast. For example, finding spell icon(44x45px) on some random WoW screenshot(1920x1080) takes around 130ms on my I5-7300HQ(4 cores).
It certainly can't handle these things in realtime, but it's still significantly faster than previous versions(could take up to 3 seconds, hehe).

## Third party tools
WinAuto wraps [InputSimulator](https://github.com/michaelnoonan/inputsimulator) library. It is possible I will rewrite it once to add "native" support.

## Documentation
See tests and in-code XML docs. It is really simple to learn and use.

## Licence
The MIT License (MIT)

Copyright (c) 2016-2017 Tomas Bosek bosektom@gmail.com

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
