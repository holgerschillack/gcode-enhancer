using Gcode.Utils;
using System;
using System.Collections.Generic;

namespace gcode_enhancer
{
    internal class Program
    {
        static double shiftX = 0.0;
        static double shiftY = 0.0;

        private static void Main(string[] args)
        {
            Console.WriteLine($"Got {args.Length} file(s).\n");

            if (args.Length != 0)
            {
                var fileList = new List<string>();

                foreach (var arg in args)
                {
                    fileList.Add(arg);
                }

                ProcessInputFiles(fileList);
            }

            Console.Write("\nExiting...\n");
        }

        private static void ProcessInputFiles(IEnumerable<string> fileList)
        {

            List<string> processFiles = new List<string>();

            foreach (var file in fileList)
            {
                if (file.IndexOf(".top.", StringComparison.Ordinal) == -1)
                {
                    processFiles.Add((file));
                }
                else
                {
                    System.IO.File.Delete(file);
                }
            }

            CleanGCode(processFiles);

            foreach (var file in processFiles)
            {
                if (file.IndexOf("bot.mill", StringComparison.Ordinal) != -1) // looking for milling file to get shift values
                {
                    Console.WriteLine($"\nFound milling file: {file}\n");
                    var millFile = new List<string>(System.IO.File.ReadAllLines(file));
                    if (FindMinMax(millFile))
                    {
                        Console.WriteLine($"SHIFT Values: SHIFT X: {Math.Abs(shiftX)}, SHIFT Y: {Math.Abs(shiftY)}\n");
                        ShiftGCode(fileList);
                    }
                    else
                    {
                        Console.WriteLine("Could not find MIN/MAX values in milling file. Just cleaning GCode...");
                    }
                }
            }
        }

        private static void CleanGCode(IEnumerable<string> fileList)
        {
            Console.WriteLine("Cleaning GCode...\n");
            foreach (var file in fileList)
            {
                Console.Write($"{file}...");
                var gcodeFile = System.IO.File.ReadAllLines(file);
                List<string> newGcodeFile = new List<string>();

                if (file.IndexOf("bot.mill", StringComparison.Ordinal) != -1 || file.IndexOf("bot.etch", StringComparison.Ordinal) != -1 || file.IndexOf("bot.drill", StringComparison.Ordinal) != -1 || file.IndexOf("bot.text", StringComparison.Ordinal) != -1)
                {
                    bool jumpFlag = false;
                    foreach (var line in gcodeFile)
                    {
                        bool deleteFlag = false;

                        var newLine = line;

                        if (newLine.IndexOf("(", StringComparison.Ordinal) >= 0) deleteFlag = true;
                        if (newLine.Trim() == "") deleteFlag = true;
                        if (newLine.IndexOf("M02", StringComparison.Ordinal) >= 0) deleteFlag = true;
                        if (newLine.IndexOf("M05", StringComparison.Ordinal) >= 0)
                        {
                            deleteFlag = true;
                            jumpFlag = true;
                        }

                        if (newLine.IndexOf("M03", StringComparison.Ordinal) >= 0)
                        {
                            deleteFlag = true;
                            jumpFlag = false;
                        }

                        if (newLine.IndexOf("G00 Z") >= 0) newLine += " F200 ";
                        if (newLine.IndexOf("G00 X") >= 0) newLine += " F1000 ";

                        if (!deleteFlag && !jumpFlag) newGcodeFile.Add(newLine);
                    }

                    System.IO.File.WriteAllLines(file, newGcodeFile);
                }
                Console.Write(" Done.\n");
            }
        }

        private static void ShiftGCode(IEnumerable<string> fileList)
        {
            Console.WriteLine("Shift GCode Files...\n");
            foreach (var file in fileList)
            {
                Console.Write($"{file}...");
                var gcodeFile = System.IO.File.ReadAllLines(file);
                List<string> newGcodeFile = new List<string>();

                for (int i = 0; i < gcodeFile.Length && i < 6; i++) // just copy the first 6 lines
                {
                    newGcodeFile.Add(gcodeFile[i]);
                }

                for (int i = 6; i < gcodeFile.Length; i++)
                {
                    var gcodeConverted = GcodeParser.ToGCode(gcodeFile[i]);

                    gcodeConverted.X -= shiftX;
                    gcodeConverted.Y -= shiftY;

                    newGcodeFile.Add(gcodeConverted.ToStringCommand());
                }
                System.IO.File.WriteAllLines(file, newGcodeFile);
                Console.Write(" Done.\n");
            }
        }

        private static bool FindMinMax(List<string> millFile)
        {
            List<double> xList = new List<double>();
            List<double> yList = new List<double>();

            for (int i = 10; i < millFile.Count; i++)
            {
                var gcodeConverted = GcodeParser.ToGCode(millFile[i]);
                if (gcodeConverted.X != null && gcodeConverted.Y != null)
                {
                    xList.Add(Convert.ToDouble(gcodeConverted.X));
                    yList.Add(Convert.ToDouble(gcodeConverted.Y));
                }
            }

            xList.Sort((a, b) => Math.Abs(a).CompareTo(Math.Abs(b)));
            yList.Sort((a, b) => Math.Abs(a).CompareTo(Math.Abs(b)));

            if (xList.Count > 1 && yList.Count > 1)
            {
                shiftX = xList[xList.Count - 1];
                shiftY = yList[0];
                return true;
            }
            return false;
        }
    }
}
