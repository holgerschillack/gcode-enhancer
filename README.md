# gcode-enhancer

Shift GCode coordinates to positive XY-plane and improve generated GCode from pcb-gcode eagle plugin.

## Problem and solution

When making PCB layouts with eagle to export them with the PCB-Gcode extension, the bottom side needs to be mirrored. In the export the mirroring results in a mirroring at the x-axis, so the x-values become negative, although the mirrored layout is correct.
These negative x-values were inconvenient for me, so I wrote a script that shifts the different scripts (mill, drill, etch, text) to starting at (0, 0) and have only positive values in X and Y. Also some other GCode improvements are made. e.g the moving od the spindle between drilling was totally slow, so I sped that up a bit. And there were according to the circuit different drill bit sizes (e.g. 0.7, 0.9, 1.2mm) so the programm wanted to make a tool change - I deleted that as well as some comments with paranthesis in the GCode.

I also made a registry entry template to register the application to the context menu of windows. So you can just click on the files and convert them in one click. The full path of the application needs to be inserted.

## 3D Printer

I use an old 3D-Printer (Vellemann K8200) for the pcb jobs. The accuracy is good, the extruder mount was replaced by a mount for a Dremel 4000.
Drilling, etching and milling is done with relatively cheap china VHM bits (0.8mm/1.2mm drill, 1.5mm milling bit, 20° engraving tool)
I'm etching with Repetier-Host on my laptop connected per USB-cable to my printer. In the start script I control an I/O pin that (over a relais) switches the dremel on and off.

## This App

It's a Dotnet Core 2.2 commandline app and can be built under windows with e.g. "dotnet publish -c Release -r win10-x64". Probably you need to have the dotnet core SDK/CLI installed for building.

The App takes space-seperated filenames as parameter.

It basically shifts the gcode to the other side of the x-axis and removes any X/Y margins. So if you execute it twice, the gcode will land on the other side again :)
For my usage it's best practice to delete the top-side layout files, which are always generated in my version of the PCB-GCODE script, so be aware that every executed file that is not an bottom-layout will be deleted. You can just comment it out for your purposes.

This App is not tested, has no unit tests and is just a simple script that does it's work.

## Pre-built

I have included a built version for Windows x64 in the pre-build folder, if you just want to see what the script is doing to your GCode.
