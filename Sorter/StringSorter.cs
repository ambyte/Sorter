using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Sorter
{
    public class StringSorter
    {
        private string _tempFolder;

        public StringSorter()
        {
            _tempFolder = "sorter";
        }

        public void SortFile(string filePath)
        {
            var splittedFiles = SplitFile(filePath);
            var sortedFiles = SortSplittedFiles(splittedFiles);
            MergeSortedFiles(filePath, sortedFiles);
        }

        private List<string> SplitFile(string inputFile)
        {
            if (Directory.Exists(_tempFolder))
            {
                Directory.Delete(_tempFolder);
            }
            Directory.CreateDirectory(_tempFolder);
            var unsortedChunkFiles = new List<string>();
            const int chunkSize = 10 * 1024 * 1024;
            byte[] buffer = new byte[chunkSize];
            List<byte> extraBuffer = new List<byte>();

            using (Stream input = File.OpenRead(inputFile))
            {
                int index = 0;
                while (input.Position < input.Length)
                {
                    string unsortedFilePath = Path.Combine(_tempFolder, $"unsorted{index}");
                    using (Stream output = File.Create(unsortedFilePath))
                    {
                        int chunkBytesRead = 0;
                        while (chunkBytesRead < chunkSize)
                        {
                            int bytesRead = input.Read(buffer, chunkBytesRead, chunkSize - chunkBytesRead);
                            if (bytesRead == 0)
                            {
                                break;
                            }
                            chunkBytesRead += bytesRead;
                        }
                        byte extraByte = buffer[chunkSize - 1];
                        while (extraByte != '\n')
                        {
                            int flag = input.ReadByte();
                            if (flag == -1)
                            {
                                break;
                            }
                            extraByte = (byte)flag;
                            extraBuffer.Add(extraByte);
                        }
                        output.Write(buffer, 0, chunkBytesRead);
                        if (extraBuffer.Count > 0)
                        {
                            output.Write(extraBuffer.ToArray(), 0, extraBuffer.Count);
                        }
                        extraBuffer.Clear();
                    }
                    unsortedChunkFiles.Add(unsortedFilePath);
                    index++;
                }
            }
            return unsortedChunkFiles;
        }

        private List<string> SortSplittedFiles(List<string> splittedFiles)
        {
            using var files = new BlockingCollection<string>();
            var taskList = new List<Task>();
            for (int i = 0; i < splittedFiles.Count; i++)
{
                taskList.Add(SortBlock(splittedFiles[i], i, files));
                if (i + 1 % 8 == 0)
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
                var reader = new StreamReader(filePath);
                string[] strings = reader.ReadToEnd().Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
                var listData = strings.Select(x => new Record(x)).ToList();
                listData.Sort();
                var sortedFileName = Path.Combine(_tempFolder, $"sorted{index}");
                var writer = new StreamWriter(sortedFileName);
                writer.Write(string.Join(Environment.NewLine, listData));
                listData = null;                
                reader.Close();
                writer.Close();
                ts.Add(sortedFileName);
            });
            task.Start();
            return task;
        }

        private static void MergeSortedFiles(string originalFilePath, List<string> sortedFiles)
        {
            File.Delete(originalFilePath);
            List<StreamReader> readers = new List<StreamReader>();
            List<Record> layer = new List<Record>(readers.Count);
            foreach (string file in sortedFiles)
            {
                var reader = new StreamReader(File.OpenRead(file));
                readers.Add(reader);
                layer.Add(new Record(reader.ReadLine()));
            }
            var writter = new StreamWriter(originalFilePath);
            int Id = 0;
            while (readers.Count > 0)
            {
                var min = layer.Min();
                Id = layer.IndexOf(min);
                var line = readers[Id].ReadLine();
                if (line == null)
                {
                    layer.RemoveAt(Id);
                    readers[Id].Close();
                    readers.RemoveAt(Id);
                }
                else
                {
                    layer[Id] = new Record(line);

                }
                writter.WriteLine(min);
            }

            writter.Close();
            foreach (string file in sortedFiles)
            {
                File.Delete(file);
            }
        }

    }
}