using ImageMagick;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace TerrainFactory.Modules.Images
{
	public static class ColorUtil
	{
		public static float[] CreateColorGrayscale(float v)
		{
			ushort u = (ushort)(v * ushort.MaxValue);
			return new float[] { u, u, u, ushort.MaxValue };
		}

		public static float[] CreateColor(float r, float g, float b)
		{
			ushort ur = (ushort)(r * ushort.MaxValue);
			ushort ug = (ushort)(g * ushort.MaxValue);
			ushort ub = (ushort)(b * ushort.MaxValue);
			return new float[] { ur, ug, ub, ushort.MaxValue };
		}

		public static ushort[] CreateColorUShort(Color color)
		{
			ushort ur = (ushort)(color.R * ushort.MaxValue);
			ushort ug = (ushort)(color.G * ushort.MaxValue);
			ushort ub = (ushort)(color.B * ushort.MaxValue);
			ushort ua = (ushort)(color.A * ushort.MaxValue);
			return new ushort[] { ur, ug, ub, ua };
		}

		public static float[] CreateColorFloat(Color color)
		{
			ushort ur = (ushort)(color.R * ushort.MaxValue);
			ushort ug = (ushort)(color.G * ushort.MaxValue);
			ushort ub = (ushort)(color.B * ushort.MaxValue);
			ushort ua = (ushort)(color.A * ushort.MaxValue);
			return new float[] { ur, ug, ub, ua };
		}

		public static MagickColor CreateMagickColor(Color color)
		{
			ushort ur = (ushort)(color.R * ushort.MaxValue);
			ushort ug = (ushort)(color.G * ushort.MaxValue);
			ushort ub = (ushort)(color.B * ushort.MaxValue);
			ushort ua = (ushort)(color.A * ushort.MaxValue);
			return new MagickColor(ur, ug, ub, ua);
		}

		public static void SetPixel(MagickImage img, IPixelCollection<float> pixels, int x, int y, Color color, float opacity)
		{
			y = img.Height - y - 1;
			if(x < 0 || x >= img.Width || y < 0 || y >= img.Height) return;
			Color src = pixels.GetPixel(x, y).ToColor().ToColor();
			pixels.SetPixel(x, y, CreateColorFloat(Lerp(src, color, opacity)));
		}

		public static Color Lerp(Color ca, Color cb, float t)
		{
			byte r = (byte)(ca.R + (cb.R - ca.R) * t);
			byte g = (byte)(ca.G + (cb.G - ca.G) * t);
			byte b = (byte)(ca.B + (cb.B - ca.B) * t);
			return Color.FromArgb(255, r, g, b);
		}
	}
}
