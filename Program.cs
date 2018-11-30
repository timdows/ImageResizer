using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ImageResizer
{
    public class Program
    {
        public static async Task Main(string[] args)
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

            var files = Directory.GetFiles(inputDirectory, "*.jpg", SearchOption.AllDirectories);

            List<Task> backgroundTasks = new List<Task>();
            foreach (var file in files)
            {
                var task = Task.Run(() => ScaleImage(file, saveDirectory, maxSize));
                backgroundTasks.Add(task);
            }

            await Task.WhenAll(backgroundTasks);
        }

        public static async Task ScaleImage(string imagePath, string saveDirectory, int maxSize)
        {
            var fileName = Path.GetFileName(imagePath);
            var savePath = Path.Combine(saveDirectory, fileName);
            var stopwatch = Stopwatch.StartNew();

            using (Image<Rgba32> image = Image.Load(imagePath))
            {
                var ratioX = (double)maxSize / image.Width;
                var ratioY = (double)maxSize / image.Height;
                var ratio = Math.Min(ratioX, ratioY);

                var newWidth = (int)(image.Width * ratio);
                var newHeight = (int)(image.Height * ratio);

                image.Mutate(x => x
                    .Resize(newWidth, newHeight)
                    .AutoOrient());
                image.Save(savePath);
            }

            stopwatch.Stop();
            Console.WriteLine($"Done after {stopwatch.ElapsedMilliseconds} with {imagePath}");
        }
    }
}
