using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sorter
{
    public class StringSorter
    {
        private const long MaxBlockSizeInBytes = 40000000;
        private string _tempFolder;

        public StringSorter()
        {
            _tempFolder = Path.Combine(Path.GetTempPath(), "sorter");
        }

        public void SortFile(string filePath)
        {
            var splittedFiles = SplitFile(filePath);
            var sortedFiles = SortBlocks(splittedFiles);
            var fileResult = MergeBlocks(sortedFiles);
            ReplaceOriginalFileWithSorted(filePath, fileResult);
        }

        private List<string> SplitFile(string filePath)
        {
            var files = new List<string>();
            if (Directory.Exists(_tempFolder)) Directory.Delete(_tempFolder, true);
            Directory.CreateDirectory(_tempFolder);
            long sizeOfBlockInBytes = 0;
            using var reader = new StreamReader(filePath);
            string line = reader.ReadLine();
            var sb = new StringBuilder();
            while (line != null)
            {

                sizeOfBlockInBytes += line.Length;
                if (sizeOfBlockInBytes > MaxBlockSizeInBytes)
                {
                    files.Add(CreateNewBlock(sb, files.Count));
                    sb.Clear();
                    sizeOfBlockInBytes = line.Length;
                }
                sb.AppendLine(line);
                line = reader.ReadLine();
            }
            if (sb.Length > 0)
            {
                files.Add(CreateNewBlock(sb, files.Count));
            }
            return files;
        }

        private string CreateNewBlock(StringBuilder sb, int count)
        {
            var filePath = $"{_tempFolder}/block{count}.txt";            
            using StreamWriter file = new StreamWriter(filePath);
            file.Write(sb);
            return filePath;
        }

        private List<string> SortBlocks(List<string> splittedFiles)
        {
            using var files = new BlockingCollection<string>();
            var taskList = new List<Task>();
            for (int i = 0; i < splittedFiles.Count; i++)
{
                taskList.Add(SortBlock(splittedFiles[i], i, files));
                if (i % 4 == 0)
                {
                    Task.WaitAll(taskList.ToArray());
                    taskList.Clear();
                }
            }
            if (taskList.Count>0)
            {
                Task.WaitAll(taskList.ToArray());
            }
            foreach (var file in splittedFiles)
            {
                File.Delete(file);
            }
            return files.ToList();
        }        

        private Task SortBlock(string filePath, int index, BlockingCollection<string> ts)
        {
            var task = new Task(() =>
            {
                using var reader = new StreamReader(filePath);
                string[] strings = reader.ReadToEnd().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                var listData = strings.Select(x => new Record(x)).ToList();
                listData.Sort();
                var sortedFileName = $"{_tempFolder}/sorted{index}.txt";            
                using var writer = new StreamWriter(sortedFileName);
                writer.Write(string.Join(Environment.NewLine, listData));
                ts.Add(sortedFileName);
            });
            task.Start();
            return task;
        }        

        private string MergeBlocks(List<string> files)
        {
            int mergeGeneration = 1;
            List<Task> taskList = new List<Task>(); ;
            while (files.Count > 1)
            {
                int filesToDeleteCount = files.Count;
                for (int i = 0; i < filesToDeleteCount; i++)
                {
                    if (i + 1 >= filesToDeleteCount)
                    {
                        filesToDeleteCount--;
                        break;
                    }
                    taskList.Add(MergeTwoFiles(files[i], files[i + 1], $"{_tempFolder}/{mergeGeneration}merge{i}.txt"));
                    files.Add($"{_tempFolder}/{mergeGeneration}merge{i}.txt");
                    i++;
                    mergeGeneration++;
                }
                Task.WaitAll(taskList.ToArray());
                taskList.Clear();
                DeleteUnnecessaryFiles(filesToDeleteCount, files);
            }
            return files[0];
        }


        private Task MergeTwoFiles(string firstFilePath, string secondFilePath, string outputFileName)
        {
            var task = new Task(() =>
            {
                using var writer = new StreamWriter(outputFileName);
                using var firstReader = new StreamReader(firstFilePath);
                using var secondReader = new StreamReader(secondFilePath);

                string leftString = firstReader.ReadLine();
                string rightString = secondReader.ReadLine();

                while (!String.IsNullOrEmpty(leftString) && !String.IsNullOrEmpty(rightString))
                {
                    if (new Record(leftString).CompareTo(new Record(rightString)) < 0)
                    {
                        writer.WriteLine(leftString);
                        leftString = firstReader.ReadLine();
                    }
                    else
                    {
                        writer.WriteLine(rightString);
                        rightString = secondReader.ReadLine();
                    }
                }

                while (!String.IsNullOrEmpty(leftString))
                {
                    writer.WriteLine(leftString);
                    leftString = firstReader.ReadLine();
                }

                while (!String.IsNullOrEmpty(rightString))
                {
                    writer.WriteLine(rightString);
                    rightString = secondReader.ReadLine();
                }

            });
            task.Start();
            return task;
        }

        private void DeleteUnnecessaryFiles(int filesToDeleteCount, List<string> files)
        {
            for (int i = 0; i < filesToDeleteCount; i++)
            {
                File.Delete(files[i]);
            }
            files.RemoveRange(0, filesToDeleteCount);
        }

        private void ReplaceOriginalFileWithSorted(string originalFilePath, string sortedFilePath)
        {
            File.Delete(originalFilePath);
            File.Move(sortedFilePath,originalFilePath);
        }
    }
}