using ImageMagick;
using System;

namespace TerrainFactory.Modules.Bitmaps
{
	public static class ColorUtil
	{
		[ThreadStatic]
		private static float[] channels = new float[4];

		public static void CreateColorGrayscale(float v, float[] output)
		{
			v = MathUtils.Clamp01(v);
			ushort u = (ushort)(v * ushort.MaxValue);
			output[0] = u;
			output[1] = u;
			output[2] = u;
			output[3] = ushort.MaxValue;
		}

		public static void CreateColor(float r, float g, float b, float[] output)
		{
			output[0] = (ushort)(r * ushort.MaxValue);
			output[1] = (ushort)(g * ushort.MaxValue);
			output[2] = (ushort)(b * ushort.MaxValue);
			output[3] = ushort.MaxValue;
		}

		public static void SetPixel(MagickImage img, IPixelCollection<float> pixels, int x, int y, IMagickColor<float> color, float opacity)
		{
			if(channels == null) channels = new float[4];
			y = (int)img.Height - y - 1;
			if(x < 0 || x >= img.Width || y < 0 || y >= img.Height) return;
			var src = pixels.GetPixel(x, y).ToColor();
			var col = Lerp(src, color, opacity);
			channels[0] = col.R / 255f;
			channels[1] = col.G / 255f;
			channels[2] = col.B / 255f;
			channels[3] = col.A / 255f;
			pixels.SetPixel(x, y, channels);
		}

		public static MagickColor Lerp(IMagickColor<float> ca, IMagickColor<float> cb, float t)
		{
			byte r = (byte)(ca.R + (cb.R - ca.R) * t);
			byte g = (byte)(ca.G + (cb.G - ca.G) * t);
			byte b = (byte)(ca.B + (cb.B - ca.B) * t);
			return MagickColor.FromRgba(r, g, b, 255);
		}
	}
}
