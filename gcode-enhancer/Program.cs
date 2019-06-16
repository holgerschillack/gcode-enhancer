using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace gcode_enhancer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine($"Habe {args.Length} Dateipfade erkannt.");
            var fileList = new List<string>();
            ;
            foreach (var arg in args)
            {
                fileList.Add(arg);
                Console.WriteLine(arg);
            }
            ReadInputFiles(fileList);
            Console.ReadLine();
        }

        private static void ReadInputFiles(IEnumerable<string> fileList)
        {
            var minX = 1000.0;
            var maxX = 0.0;
            var minY = 1000.0;
            var maxY = 0.0;

            foreach (var file in fileList)
            {
                if (file.IndexOf("bot.mill", StringComparison.Ordinal) != -1) // Found milling file for x,y size shift information
                {
                    Console.WriteLine("Milling Datei erkannt!!!\n");
                    var millFile = cleanMillFile(file);
                    var i = 0;
                    foreach (var line in millFile)
                    {
                        // check for min/max Y,X
                        i++;
                        if (i > 6) // erst ab Zeile 7 überprüfen
                        {
                            if (line.IndexOf("X", StringComparison.Ordinal) >= 0 && line.IndexOf("X", StringComparison.Ordinal) >= 0)
                            {
                                // found line with X and Y coords, now looking for min/max values
                                // X
                                string xPattern = @"[X].?\d?\d?\d?[.]\d{1,}";
                                Match mX = Regex.Match(line, xPattern, RegexOptions.IgnoreCase);
                                var xLine = mX.Value;
                                string xPatternSec = @"\d?\d?\d?[.]\d{1,}";
                                Match mXSec = Regex.Match(xLine, xPatternSec, RegexOptions.IgnoreCase);
                                xLine = mXSec.Value;

                                // Y
                                string yPattern = @"[Y].?\d?\d?\d?[.]\d{1,}";
                                Match mY = Regex.Match(line, yPattern, RegexOptions.IgnoreCase);
                                var yLine = mY.Value;
                                string yPatternSec = @"\d?\d?\d?[.]\d{1,}";
                                Match mYSec = Regex.Match(yLine, yPatternSec, RegexOptions.IgnoreCase);
                                yLine = mYSec.Value;
                                xLine = xLine.Replace(".", ",");
                                yLine = yLine.Replace(".", ",");
                                if (Convert.ToDouble(xLine) > maxX) maxX = Convert.ToDouble(xLine);
                                if (Convert.ToDouble(xLine) < minX) minX = Convert.ToDouble(xLine);
                                if (Convert.ToDouble(yLine) > maxY) maxY = Convert.ToDouble(yLine);
                                if (Convert.ToDouble(yLine) < minY) minY = Convert.ToDouble(yLine);

                            }
                        }
                    }

                    Console.WriteLine("MIN/MAX-WERTE:");
                    Console.WriteLine($"\nMIN X: {minX}");
                    Console.WriteLine($"MIN Y: {minY}");
                    Console.WriteLine($"MAX X: {maxX}");
                    Console.WriteLine($"MAX Y: {maxY}\n");

                }
            }


            foreach (var file in fileList) // if maxX, maxY != 0
            {
                // MILL FILE
                if (file.IndexOf("bot.mill", StringComparison.Ordinal) != -1)
                {
                    Console.WriteLine("Milling Datei...\n");
                    var newMillFile = cleanMillFile(file);

                    newMillFile = shiftFile(newMillFile, maxX - minX, maxY - minY);

                    Console.WriteLine("NEW MILL FILE: \n");
                    foreach (var line in newMillFile)
                    {
                        Console.WriteLine(line);
                    }

                    // Export new milling file
                }
            }

        }

        private static List<string> cleanMillFile(string file)
        {
            var millFile = System.IO.File.ReadAllLines(file);
            List<string> newMillFile = new List<string>();
            foreach (var line in millFile)
            {
                bool problemFound = false;
                var newLine = line;

                if (newLine.IndexOf("(", StringComparison.Ordinal) >= 0) problemFound = true;
                if (newLine.Trim() == "") problemFound = true;
                if (newLine.IndexOf("M02", StringComparison.Ordinal) >= 0) problemFound = true;
                if (newLine.IndexOf("M03", StringComparison.Ordinal) >= 0) problemFound = true;
                if (newLine.IndexOf("M05", StringComparison.Ordinal) >= 0) problemFound = true;

                if (newLine.IndexOf("G00 Z") >= 0) newLine += " F200 ";
                if (newLine.IndexOf("G00 X") >= 0) newLine += " F1000 ";

                if (!problemFound) newMillFile.Add(newLine);
            }

            return newMillFile;
        }

        private static List<string> shiftFile(List<string> data, double X, double Y)
        {
            var i = 0;
            foreach (var line in data)
            {
                i++;
                if (i > 6) // erst ab Zeile 7 überprüfen
                {
                    if (line.IndexOf("X", StringComparison.Ordinal) >= 0 &&
                        line.IndexOf("X", StringComparison.Ordinal) >= 0)
                    {
                        // hier muss X und Y angepasst werden

                    }
                }
            }

            return data;
        }
    }
}
