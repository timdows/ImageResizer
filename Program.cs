using ImageSharp;
using ImageSharp.Formats;
using ImageSharp.PixelFormats;
using System;
using System.IO;

namespace ImageResizer
{
    public class Program
    {
        public static void Main(string[] args)
        {
			if (args.Length > 0 && args[0] != null)
			{
				var directory = args[0].ToString();
				Console.WriteLine($"Using directory {directory}");

				var files = Directory.GetFiles(directory, "*.jpg", SearchOption.AllDirectories);
				var current = 0;

				foreach (var file in files)
				{
					Console.WriteLine($"{current++}/{files.Length} - Processing {file}");
					ScaleImage(file);
				}
			}
		}

		public static void ScaleImage(string imagePath)
		{
			// Image.Load(string path) is a shortcut for our default type. Other pixel formats use Image.Load<TPixel>(string path))
			using (Image<Rgba32> image = Image.Load(imagePath))
			{
				var maxSize = 800;

				var ratioX = (double)maxSize / image.Width;
				var ratioY = (double)maxSize / image.Height;
				var ratio = Math.Min(ratioX, ratioY);

				var newWidth = (int)(image.Width * ratio);
				var newHeight = (int)(image.Height * ratio);

				image.Resize(newWidth, newHeight)
					.Save(imagePath); // automatic encoder selected based on extension.
			}
		}
	}
}
