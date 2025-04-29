using ImageMagick;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace TerrainFactory.Modules.Bitmaps
{
	class ImageGeneratorMagick
	{

		public enum BitDepth
		{
			Depth8,
			Depth16,
			Depth32
		}

		public MagickImage Image { get; private set; }

		ImageType imageType;
		ElevationData data;
		float lowValue;
		float highValue;

		[ThreadStatic]
		private static float[] pixelChannels;

		public ImageGeneratorMagick(ElevationData heightData, ImageType type, float blackValue, float whiteValue)
		{
			data = heightData;
			imageType = type;
			lowValue = blackValue;
			highValue = whiteValue;
			if(type == ImageType.Heightmap8) MakeHeightmap(false);
			else if(type == ImageType.Heightmap16) MakeHeightmap(true);
			else if(type == ImageType.Normalmap) MakeNormalmap(false);
			else if(type == ImageType.Hillshade) MakeHillshademap();
			else if(type == ImageType.CombinedPreview) MakeHillshademap(heightmapBlend: 0.5f);
			else throw new NotImplementedException();
		}

		public void WriteFile(string filename, MagickFormat format)
		{
			Image.Write(filename, format);
		}

		private void MakeHeightmap(bool is16bit)
		{
			Image = CreateImage(0, is16bit ? MagickFormat.Png48 : MagickFormat.Png24);
			int h = (int)Image.Height;
			ForEachPixel(Image, (pixels, x, y) =>
			{
				float v = GetHeightmapLuminance(x, y);
				ColorUtil.CreateColorGrayscale(v, pixelChannels);
				pixels.SetPixel(x, h - y - 1, pixelChannels);
			});
		}

		private float GetHeightmapLuminance(int x, int y)
		{
			return MathUtils.Clamp01(MathUtils.InverseLerp(lowValue, highValue, data.GetElevationAtCellUnchecked(x, y)));
		}

		private void MakeNormalmap(bool sharp)
		{
			Image = CreateImage(sharp ? -1 : 0);
			var normals = NormalMapper.CalculateNormals(data, sharp);

			ForEachPixel(Image, (pixels, x, y) =>
			{
				Vector3 nrm = normals[x, y];
				float r = 0.5f + nrm.X / 2f;
				float g = 0.5f + nrm.Y / 2f;
				float b = 0.5f + nrm.Z / 2f;
				ColorUtil.CreateColor(r, g, b, pixelChannels);
				pixels.SetPixel(x, (int)Image.Height - y - 1, pixelChannels);
			});
		}

		private void MakeHillshademap(float intensity = 0.8f, float heightmapBlend = 0f)
		{
			MakeHillshademap(60, 40, intensity, heightmapBlend);
		}

		private void MakeHillshademap(float sunYawDegrees, float sunPitchDegrees, float intensity, float heightmapBlend)
		{
			var normals = NormalMapper.CalculateNormals(data, true);
			Image = CreateImage();
			Vector3 sunNormal = RotationToNormal(sunYawDegrees, sunPitchDegrees);

			int h = (int)Image.Height;
			ForEachPixel(Image, (pixels, x, y) =>
			{
				Vector3 nrm = normals[x, y];
				float luminance = -Vector3.Dot(nrm, sunNormal);
				luminance = luminance * 0.5f * intensity + 0.5f;
				if(heightmapBlend > 0)
				{
					float hmLuminance = GetHeightmapLuminance(x, y) * 1.6f;
					luminance *= MathUtils.Lerp(1f, hmLuminance, heightmapBlend);
				}
				ColorUtil.CreateColorGrayscale(luminance, pixelChannels);
				pixels.SetPixel(x, h - y - 1, pixelChannels);
			});
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

		public const float Deg2Rad = (float)Math.PI / 180f;
		public const float Rad2Deg = 180f / (float)Math.PI;

		private Vector3 RotationToNormal(float yawDegrees, float pitchDegrees)
		{
			float pitchRad = Deg2Rad * -pitchDegrees;
			float yawRad = Deg2Rad * (yawDegrees + 90f);

			var dy = (float)Math.Sin(pitchRad);
			var dx = (float)Math.Sin(yawRad) * (float)Math.Cos(pitchRad);
			var dz = (float)Math.Cos(yawRad) * (float)Math.Cos(pitchRad);

			return new Vector3(dx, dy, dz);
		}

		private MagickImage CreateImage(int width, int height, MagickFormat format = MagickFormat.Png24)
		{
			var img = new MagickImage(MagickColors.Black, (uint)width, (uint)height);
			img.Format = format;
			return img;
		}

		private MagickImage CreateImage(int sizeOffset = 0, MagickFormat format = MagickFormat.Png24)
		{
			return CreateImage(data.CellCountX + sizeOffset, data.CellCountY + sizeOffset, format);
		}
	}
}
