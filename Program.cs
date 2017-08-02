using ImageSharp;
using System;
using System.IO;

namespace ImageResizer
{
	public class Program
	{
		public static void Main(string[] args)
		{
			if (args.Length != 2)
			{
				Console.WriteLine($"Missing directory and/or maxSize arguments");
				return;
			}

			var directory = args[0].ToString();
			if (!int.TryParse(args[1], out var maxSize))
			{
				Console.WriteLine($"Could not parse int maxSize");
			}

			Console.WriteLine($"Using directory {directory} and maxSize: {maxSize}");

			var files = Directory.GetFiles(directory, "*.jpg", SearchOption.AllDirectories);
			var current = 0;

			foreach (var file in files)
			{
				Console.WriteLine($"{current++}/{files.Length} - Processing {file}");
				ScaleImage(file, maxSize);
			}
		}

		public static void ScaleImage(string imagePath, int maxSize)
		{
			// Image.Load(string path) is a shortcut for our default type. Other pixel formats use Image.Load<TPixel>(string path))
			using (Image<Rgba32> image = Image.Load(imagePath))
			{
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
