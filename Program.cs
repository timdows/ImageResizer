using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizer
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            if (args.Length != 3)
            {
                Console.WriteLine("Needed arguments:\r\n1. Input directory\r\n2. Save directory\r\n3. MaxSize");
                return;
            }

            var inputDirectory = args[0].ToString();
            var saveDirectory = args[1].ToString();
            if (!int.TryParse(args[2], out var maxSize))
            {
                Console.WriteLine($"Could not parse int maxSize");
            }

            Console.WriteLine($"1. Input directory: {inputDirectory}\r\n2. Save directory: {saveDirectory}\r\n3. MaxSize: {maxSize}");

            var files = Directory.GetFiles(inputDirectory, "*.jpg", SearchOption.AllDirectories).ToList();
            files.AddRange(Directory.GetFiles(inputDirectory, "*.png", SearchOption.AllDirectories));
            files = files.OrderBy(item => item).ToList();

            //FixDates(files);

            Console.WriteLine($"Found {files.Count} images to be scaled");

            var stopwatch = Stopwatch.StartNew();
            var totalToProcess = files.Count;
            var processedCount = 0;

            Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 8 }, (file, state, index) =>
            {
                var message = ScaleImage(file, saveDirectory, maxSize);
                Interlocked.Increment(ref processedCount);
                Console.WriteLine($"{processedCount}/{totalToProcess}, {index} {message}");
            });


            stopwatch.Stop();
            Console.WriteLine($"{files.Count} images scaled after {stopwatch.ElapsedMilliseconds}");
        }

        private static void FixDates(List<string> files)
        {
            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                var creationTime = fileInfo.CreationTime;
                var lastWriteTime = fileInfo.LastWriteTime;

                if (creationTime > DateTime.Today || lastWriteTime > DateTime.Today)
                {
                    var fix = DateTime.Today;
                    File.SetCreationTime(file, fix);
                    File.SetLastWriteTime(file, fix);
                }
            }
        }


        private static string ScaleImage(string imagePath, string saveDirectory, int maxSize)
        {
            var fileName = Path.GetFileName(imagePath);
            var imageDirectoryName = Path.GetFileName(Path.GetDirectoryName(imagePath));
            var saveDirectoryWithSubdir = Path.Combine(saveDirectory, imageDirectoryName);
            Directory.CreateDirectory(saveDirectoryWithSubdir);

            var savePath = Path.Combine(saveDirectoryWithSubdir, fileName);
            var stopwatch = Stopwatch.StartNew();

            using (var image = Image.Load(imagePath))
            {
                var ratioX = (double)maxSize / image.Width;
                var ratioY = (double)maxSize / image.Height;
                var ratio = Math.Min(ratioX, ratioY);

                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);

                if (newHeight > image.Height)
                {
                    newHeight = image.Height;
                }
                if (newWidth > image.Width)
                {
                    newWidth = image.Width;
                }

                image.Mutate(x => x
                    .Resize(newWidth, newHeight)
                    .AutoOrient());
                image.Save(savePath);
            }

            var fileInfo = new FileInfo(imagePath);
            var creationTime = fileInfo.CreationTime;
            var lastWriteTime = fileInfo.LastWriteTime;

            File.SetCreationTime(savePath, creationTime);
            File.SetLastWriteTime(savePath, lastWriteTime);

            stopwatch.Stop();
            return $"On thread {Thread.CurrentThread.ManagedThreadId} after {stopwatch.ElapsedMilliseconds} done with {imagePath}";
        }
    }
}
