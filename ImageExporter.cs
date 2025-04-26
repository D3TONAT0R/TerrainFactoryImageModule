using ImageMagick;
using System;

namespace TerrainFactory.Modules.Bitmaps
{
	public static class ImageExporter
	{

		public static MagickImage GenerateCompositeMap(ElevationData data, MagickImage baseMap, float heightmapIntensity, float hillshadeIntensity)
		{
			MagickImage result;
			if (baseMap == null)
			{
				result = new MagickImage(MagickColors.Gray, data.CellCountX, data.CellCountY);
			}
			else
			{
				result = baseMap;
			}
			if (heightmapIntensity > 0)
			{
				var hm = new ImageGeneratorMagick(data, ImageType.Heightmap8, data.LowPoint, data.HighPoint).Image;
				result = OverlayBlend(result, hm, heightmapIntensity);
			}
			if (hillshadeIntensity > 0)
			{
				var hs = new ImageGeneratorMagick(data, ImageType.Hillshade, data.LowPoint, data.HighPoint).Image;
				result = OverlayBlend(result, hs, hillshadeIntensity);
			}
			return result;
		}

		private static MagickImage OverlayBlend(MagickImage a, MagickImage b, float strength)
		{
			MagickImage result = new MagickImage(MagickColors.Black, a.Width, a.Height);
			var pixelsA = a.GetPixels();
			var pixelsB = b.GetPixels();
			var resultPixels = result.GetPixels();
			float[] channels = new float[4];
			for (int y = 0; y < a.Height; y++)
			{
				for (int x = 0; x < a.Width; x++)
				{
					var ca = pixelsA.GetPixel(x, y).ToColor();
					var cb = pixelsB.GetPixel(x, y).ToColor();
					var cr = OverlayBlend(ca, cb, strength);
					channels[0] = cr.R / 255f;
					channels[1] = cr.G / 255f;
					channels[2] = cr.B / 255f;
					channels[3] = cr.A / 255f;
					resultPixels.SetPixel(x, y, channels);
				}
			}
			return result;
		}

		private static IMagickColor<float> OverlayBlend(IMagickColor<float> ca, IMagickColor<float> cb, float strength)
		{
			float[] a = new float[] { ca.R / 255f, ca.G / 255f, ca.B / 255f };
			float[] b = new float[] { cb.R / 255f, cb.G / 255f, cb.B / 255f };
			float[] r = new float[3];
			for (int i = 0; i < 3; i++)
			{
				if (b[i] > 0.5f)
				{
					r[i] = a[i] + (b[i] - 0.5f) * strength * 2f;
				}
				else
				{
					r[i] = a[i] + (b[i] - 0.5f) * strength * 2f;
				}
				r[i] = Math.Max(0, Math.Min(1, r[i]));
			}
			return MagickColor.FromRgb((byte)(r[0] * 255), (byte)(r[1] * 255), (byte)(r[2] * 255));
		}
	}
}
