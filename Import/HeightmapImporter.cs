using TerrainFactory.Util;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ImageMagick;

namespace TerrainFactory.Modules.Bitmaps
{
	public static class HeightmapImporter
	{

		const string progString = "Importing heightmap";

		public enum ColorChannel
		{
			Red,
			Green,
			Blue,
			Alpha,
			CombinedBrightness
		}

		public static ElevationData Import(string importPath, params string[] args)
		{
			ColorChannel? channel = null;
			if (args.TryGetArgument("channel", out string v))
			{
				v = v.ToUpper();
				if (v == "R") channel = ColorChannel.Red;
				else if (v == "G") channel = ColorChannel.Green;
				else if (v == "B") channel = ColorChannel.Blue;
				else if (v == "A") channel = ColorChannel.Alpha;
				else if (v == "C") channel = ColorChannel.CombinedBrightness;
			}
			if (args.TryGetArgument("bytes"))
			{
				return ImportHeightmap256(importPath, channel ?? ColorChannel.Red);
			}
			else
			{
				return ImportHeightmap(importPath, 0, 1, channel ?? ColorChannel.CombinedBrightness);
			}
		}

		public static ElevationData ImportHeightmap(string filepath, float? low, float? high, ColorChannel channel = ColorChannel.CombinedBrightness)
		{
			return ImportHeightmap(filepath,
				(ElevationData d, int x, int y, IMagickColor<float> c) =>
				{
					d.SetHeightAt(x, y, GetValue(c, channel));
				},
				(ElevationData d) =>
				{
					d.RecalculateElevationRange(false);
					d.CustomBlackPoint = low;
					d.CustomWhitePoint = high;
				}
			);
		}

		public static ElevationData ImportHeightmap256(string filepath, ColorChannel channel = ColorChannel.CombinedBrightness)
		{
			return ImportHeightmap(filepath,
				(ElevationData d, int x, int y, IMagickColor<float> c) =>
				{
					d.SetHeightAt(x, y, GetValueRaw(c, channel));
				},
				(ElevationData d) =>
				{
					d.RecalculateElevationRange(false);
					d.CustomBlackPoint = 0;
					d.CustomWhitePoint = 255;
				}
			);
		}

		private static ElevationData ImportHeightmap(string filepath, Action<ElevationData, int, int, IMagickColor<float>> iterator, Action<ElevationData> finalizer)
		{
			ConsoleOutput.UpdateProgressBar(progString, 0);
			FileStream stream = File.Open(filepath, FileMode.Open);
			var image = new MagickImage(stream);
			stream.Dispose();
			ConsoleOutput.UpdateProgressBar(progString, 0.5f);
			ElevationData heightData = new ElevationData((int)image.Width, (int)image.Height, filepath);
			heightData.CellSize = 1;

			uint width = image.Width;
			uint height = image.Height;
			int depth = 4;
			var pixels = image.GetPixels();

			int progress = 0;

			Parallel.For(0, (int)width, x =>
			{
				for (int y = 0; y < height; y++)
				{
					var c = pixels.GetPixel(x, y).ToColor();
					iterator(heightData, x, (int)(height - y - 1), c);
				}
				progress++;
				ConsoleOutput.UpdateProgressBar(progString, 0.5f + progress / (float)width * 0.5f);
			}
			);
			image.Dispose();
			stream.Close();

			finalizer(heightData);

			return heightData;
		}

		public static byte[,] ImportHeightmapRaw(string filepath, int offsetX, int offsetY, int width, int height, ColorChannel channel = ColorChannel.Red)
		{
			FileStream stream = File.Open(filepath, FileMode.Open);
			var image = new MagickImage(stream);
			var pixels = image.GetPixels();
			byte[,] arr = new byte[width, height];
			for (int x = 0; x < width; x++)
			{
				for (int y = 0; y < height; y++)
				{
					var c = pixels.GetPixel(offsetX + x, offsetY + y).ToColor();
					arr[x, height - 1 - y] = GetValueRaw(c, channel);
				}
			}
			return arr;
		}

		public static byte GetValueRaw(IMagickColor<float> c, ColorChannel channel)
		{
			if (channel == ColorChannel.Red)
			{
				return (byte)(c.R * 255);
			}
			else if (channel == ColorChannel.Green)
			{
				return (byte)(c.G * 255);
			}
			else if (channel == ColorChannel.Blue)
			{
				return (byte)(c.B * 255);
			}
			else if (channel == ColorChannel.Alpha)
			{
				return (byte)(c.A * 255);
			}
			else
			{
				return (byte)(c.R * 255);
			}
		}

		public static float GetValue(IMagickColor<float> c, ColorChannel channel)
		{
			if (channel == ColorChannel.Red)
			{
				return c.R / ushort.MaxValue;
			}
			else if (channel == ColorChannel.Green)
			{
				return c.G / ushort.MaxValue;
			}
			else if (channel == ColorChannel.Blue)
			{
				return c.B / ushort.MaxValue;
			}
			else if (channel == ColorChannel.Alpha)
			{
				return c.A / ushort.MaxValue;
			}
			else
			{
				return c.R / ushort.MaxValue;
			}
		}
	}
}
