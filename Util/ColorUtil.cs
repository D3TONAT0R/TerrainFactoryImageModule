using ImageMagick;

namespace TerrainFactory.Modules.Bitmaps
{
	public static class ColorUtil
	{
		private static float[] channels = new float[4];

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

		public static void SetPixel(MagickImage img, IPixelCollection<float> pixels, int x, int y, IMagickColor<float> color, float opacity)
		{
			y = img.Height - y - 1;
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
