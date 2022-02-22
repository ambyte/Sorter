using System;
using System.IO;

namespace Sorter
{
    public class FileGenerator
    {
        public static void Generate(int fileSizeGb, string filePath = "GeneratedFile.txt")
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            var strCompanyArray = new[]
            {
                "CellStar",
                "The Boeing",
                "Boise Cascade",
                "Apple Computer",
                "Arrow Electronics",
                "Avon Products",
                "Mellon Financial",
                "Novellus Systems",
                "Paccar",
                "Questar",
                "Raytheon",
                "Reliant Energy",
                "Sovereign Bancorp",
                "Spherion",
                "Tellabs",
                "Toro",
                "Trinity Industries",
                "Sierra Health Services",
                "Staff Leasing",
                "Harris ",
                "Hercules",
                "Fleetwood Enterprises",
                "Ecolab ",
            };
            var strSufficsArray = new[]
            {
                "Company",
                "Inc",
                "Corporation",
                "Co"
            };
            var random = new Random();
            long totalBytes = 0;
            using (var writer = new StreamWriter(new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192)))
            {
                while (totalBytes / 1073741824.0 < fileSizeGb)
                {
                    var text = strCompanyArray[random.Next(strCompanyArray.Length)] + " " + strSufficsArray[random.Next(strSufficsArray.Length)];
                    var number = random.Next();
                    var strLine = number + ". " + text;
                    writer.WriteLine(strLine);
                    totalBytes += strLine.Length + 2;
                }
            }
        }
    }
}