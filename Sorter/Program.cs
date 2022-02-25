using System;
using System.Diagnostics;
using System.IO;

namespace Sorter
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to Sorter!");
            Console.WriteLine("Please choose:");
            Console.WriteLine("1. Generate new file");
            Console.WriteLine("2. Inter the path for existing file");
            var input = Console.ReadLine();
            if (!int.TryParse(input, out int enteredNumber))
            {
                return;
            }
            switch (enteredNumber)
            {
                case 1:
                    GenerateFile();
                    break;
                case 2:
                    GetFilePath();
                    break;                
            }
        }

        public static void GenerateFile()
        {
            Console.Clear();
            Console.Write("Input file size in GB: ");

            var rows = Console.ReadLine();

            if (!double.TryParse(rows, out double fileSizeGb))
            {
                return;
            }

            Console.WriteLine("Generating in progress...");
            FileGenerator.Generate(fileSizeGb);

            Console.Clear();
            Console.WriteLine("Generating is complete. Press any key to continue.");
            Console.ReadKey();
            StartSorting("GeneratedFile.txt");
        }

        public static void GetFilePath()
        {
            Console.Clear();
            Console.Write("Input file path: ");

            var path = Console.ReadLine();

            if (!File.Exists(path))
            {
                Console.Write("The file does not exist. Press any key to continue.");
                Console.ReadKey();
                GetFilePath();
            }
            StartSorting(path);
        }

        public static void StartSorting(string filePath)
        {
            Console.Clear();
            Console.WriteLine("Sorting in progress...");
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            StringSorter sorter = new StringSorter();
            sorter.SortFile(filePath);
            Console.Clear();
            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            Console.WriteLine($"Sorting is complete. Press any key to continue. Duration minutes: {ts.TotalMinutes}");
            Console.ReadKey();
        }
    }
}
