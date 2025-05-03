using ImageMagick;
using System;
using System.Numerics;
using System.Threading.Tasks;
using TerrainFactory.Util;

namespace TerrainFactory.Modules.Bitmaps
{
	public static class ImageGenerator
	{
		[ThreadStatic]
		private static float[] pixelChannels;

		public static MagickImage CreateImage(ElevationData data, ImageType type)
		{
			switch (type)
			{
				case ImageType.Heightmap8:
					return CreateHeightMap(data, false);
				case ImageType.Heightmap16:
					return CreateHeightMap(data, true);
				case ImageType.Normalmap:
					return CreateNormalMap(data, false);
				case ImageType.Hillshade:
					return CreateHillshadeMap(data);
				case ImageType.CombinedPreview:
					return CreateHillshadeMap(data, heightmapBlend: 0.5f);
				default:
					throw new NotImplementedException();
			}
		}

		public static MagickImage CreateHeightMap(ElevationData data, bool is16bit)
		{
			var img = NewImage(data.CellCountX, data.CellCountY, is16bit ? MagickFormat.Png48 : MagickFormat.Png24);
			int h = (int)img.Height;
			var range = data.GrayscaleRange;
			ForEachPixel(img, (pixels, x, y) =>
			{
				float v = GetHeightmapLuminance(data, x, y, range);
				ColorUtil.CreateColorGrayscale(v, pixelChannels);
				pixels.SetPixel(x, h - y - 1, pixelChannels);
			});
			return img;
		}

		public static MagickImage CreateNormalMap(ElevationData data, bool sharp)
		{
			int width = data.CellCountX;
			int height = data.CellCountY;
			if(sharp) width--;
			if(sharp) height--;
			var img = NewImage(width, height);
			var normals = NormalMapper.CalculateNormals(data, sharp);

			ForEachPixel(img, (pixels, x, y) =>
			{
				Vector3 nrm = normals[x, y];
				float r = 0.5f + nrm.X / 2f;
				float g = 0.5f + nrm.Y / 2f;
				float b = 0.5f + nrm.Z / 2f;
				ColorUtil.CreateColor(r, g, b, pixelChannels);
				pixels.SetPixel(x, (int)img.Height - y - 1, pixelChannels);
			});
			return img;
		}

		public static MagickImage CreateHillshadeMap(ElevationData data, float sunYawDegrees = 60, float sunPitchDegrees = 40, float intensity = 0.8f, float heightmapBlend = 0f)
		{
			var normals = NormalMapper.CalculateNormals(data, true);
			var img = NewImage(data.CellCountX, data.CellCountY);
			Vector3 sunNormal = RotationToNormal(sunYawDegrees, sunPitchDegrees);
			var grayscaleRange = data.GrayscaleRange;

			int h = (int)img.Height;
			ForEachPixel(img, (pixels, x, y) =>
			{
				Vector3 nrm = normals[x, y];
				float luminance = -Vector3.Dot(nrm, sunNormal);
				luminance = luminance * 0.5f * intensity + 0.5f;
				if(heightmapBlend > 0)
				{
					float hmLuminance = GetHeightmapLuminance(data, x, y, grayscaleRange) * 1.6f;
					luminance *= MathUtils.Lerp(1f, hmLuminance, heightmapBlend);
				}
				ColorUtil.CreateColorGrayscale(luminance, pixelChannels);
				pixels.SetPixel(x, h - y - 1, pixelChannels);
			});
			return img;
		}

		public static MagickImage GenerateCompositeMap(ElevationData data, MagickImage baseMap, float heightmapIntensity, float hillshadeIntensity)
		{
			MagickImage result;
			if(baseMap == null)
			{
				result = new MagickImage(MagickColors.Gray, (uint)data.CellCountX, (uint)data.CellCountY);
			}
			else
			{
				result = baseMap;
			}
			if(heightmapIntensity > 0)
			{
				var hm = CreateHeightMap(data, false);
				result = OverlayBlend(result, hm, heightmapIntensity);
			}
			if(hillshadeIntensity > 0)
			{
				var hs = CreateHillshadeMap(data);
				result = OverlayBlend(result, hs, hillshadeIntensity);
			}
			return result;
		}

		private static MagickImage NewImage(int width, int height, MagickFormat format = MagickFormat.Png24)
		{
			var img = new MagickImage(MagickColors.Black, (uint)width, (uint)height);
			img.Format = format;
			return img;
		}

		private static MagickImage OverlayBlend(MagickImage a, MagickImage b, float strength)
		{
			MagickImage result = new MagickImage(MagickColors.Black, a.Width, a.Height);
			var pixelsA = a.GetPixels();
			var pixelsB = b.GetPixels();
			var resultPixels = result.GetPixels();
			float[] channels = new float[4];
			for(int y = 0; y < a.Height; y++)
			{
				for(int x = 0; x < a.Width; x++)
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

		[ThreadStatic]
		private static float[] a;
		[ThreadStatic]
		private static float[] b;
		[ThreadStatic]
		private static float[] r;

		private static IMagickColor<float> OverlayBlend(IMagickColor<float> ca, IMagickColor<float> cb, float strength)
		{
			if(a == null) a = new float[3];
			if(b == null) b = new float[3];
			if(r == null) r = new float[3];
			a[0] = ca.R / 255f;
			a[1] = ca.G / 255f;
			a[2] = ca.B / 255f;
			b[0] = cb.R / 255f;
			b[1] = cb.G / 255f;
			b[2] = cb.B / 255f;
			for(int i = 0; i < 3; i++)
			{
				r[i] = MathUtils.Clamp01(a[i] + (b[i] - 0.5f) * strength * 2f);
			}
			return MagickColor.FromRgb((byte)(r[0] * 255), (byte)(r[1] * 255), (byte)(r[2] * 255));
		}

		private static float GetHeightmapLuminance(ElevationData data, int cx, int cy, Range colorRange)
		{
			float h = data.GetElevationAtCellUnchecked(cx, cy);
			return colorRange.InverseLerpClamped(h);
		}

		private static void ForEachPixel(MagickImage img, Action<IPixelCollection<float>, int, int> action)
		{
			var pixels = img.GetPixelsUnsafe();
			int w = (int)img.Width;
			int h = (int)img.Height;
			Parallel.For(0, h, y =>
			{
				if(pixelChannels == null) pixelChannels = new float[4];
				for(int x = 0; x < w; x++)
				{
					action(pixels, x, y);
				}
			});
		}


		private static Vector3 RotationToNormal(float yawDegrees, float pitchDegrees)
		{
			const float DEG_TO_RAD = (float)Math.PI / 180f;

			float pitchRad = DEG_TO_RAD * -pitchDegrees;
			float yawRad = DEG_TO_RAD * (yawDegrees + 90f);

			var dy = (float)Math.Sin(pitchRad);
			var dx = (float)Math.Sin(yawRad) * (float)Math.Cos(pitchRad);
			var dz = (float)Math.Cos(yawRad) * (float)Math.Cos(pitchRad);

			return new Vector3(dx, dy, dz);
		}
	}
}
