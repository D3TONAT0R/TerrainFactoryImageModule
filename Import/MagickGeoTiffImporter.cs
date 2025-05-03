using System;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ExifLibrary;
using ImageMagick;

namespace TerrainFactory.Modules.Bitmaps.Import
{
	public static class MagickGeoTiffImporter
	{

		public static ElevationData Import(string importPath, params string[] args)
		{
			MagickNET.Initialize();

			var exifImage = ExifLibrary.ImageFile.FromFile(importPath);

			using(var input = new MagickImage(importPath))
			{
				uint imgWidth = input.Width;
				uint imgHeight = input.Height;
				uint depth = input.Depth / 8;

				float cellSize;
				var scaleTag = exifImage.Properties.Get<ExifDoubleArray>((ExifLibrary.ExifTag)133550);
				if(scaleTag != null && scaleTag.Value.Length > 0)
				{
					cellSize = (float)scaleTag.Value[0];
					if(scaleTag.Value.Length > 1 && scaleTag.Value[1] != scaleTag.Value[0])
					{
						ConsoleOutput.WriteWarning("Non-uniform pixel scale detected, this is not supported.");
					}
				}
				else
				{
					ConsoleOutput.WriteWarning("Pixel scale not defined, assuming scale of 1.0.");
					cellSize = 1f;
				}

				Vector2 lowerCornerCoordinate;
				var tiepointData = exifImage.Properties.Get<ExifDoubleArray>((ExifLibrary.ExifTag)133922);
				if(tiepointData != null && tiepointData.Value.Length > 0)
				{
					float pixelX = (float)tiepointData.Value[0];
					float pixelY = (float)tiepointData.Value[1];
					float coordX = (float)tiepointData.Value[3];
					float coordY = (float)tiepointData.Value[4];
					lowerCornerCoordinate = new Vector2(coordX, coordY);
					lowerCornerCoordinate.X -= pixelX * cellSize;
					lowerCornerCoordinate.Y -= pixelY * cellSize;
				}
				else
				{
					ConsoleOutput.WriteWarning("Tiepoints not found, assuming position of (0,0).");
					lowerCornerCoordinate = Vector2.Zero;
				}

				float? nodataValue = null;
				var nodataValueString = exifImage.Properties.Get<ExifEncodedString>((ExifLibrary.ExifTag)142113)?.Value;
				if(nodataValueString != null)
				{
					nodataValue = float.Parse(nodataValueString);
				}

				var data = new ElevationData((int)imgWidth, (int)imgHeight, importPath)
				{
					CellSize = cellSize,
					LowerCornerPosition = lowerCornerCoordinate
				};

				var pixels = input.GetPixelsUnsafe();
				for(int y = 0; y < imgHeight; y++)
				{
					for(int x = 0; x < imgWidth; x++)
					{
						float elevation = pixels[x, (int)imgHeight - y - 1].GetChannel(0) / 65536f;
						if(IsNoData(elevation, nodataValue))
						{
							elevation = ElevationData.NODATA_VALUE;
						}
						data.SetHeightAt(x, y, elevation);
					}
				}

				data.RecalculateElevationRange(true);
				return data;
			}
		}

		private static bool IsNoData(float v, float? nd)
		{
			if(float.IsNaN(v)) return true;
			if(nd.HasValue)
			{
				return Math.Abs(v - nd.Value) < 0.01f;
			}
			return false;
		}

		public static ElevationData Import(Stream stream, params string[] args)
		{
			MagickNET.Initialize();

			var exifImage = ExifLibrary.ImageFile.FromStream(stream);
			stream.Position = 0;

			using(var input = new MagickImage(stream))
			{
				uint imgWidth = input.Width;
				uint imgHeight = input.Height;
				uint depth = input.Depth / 8;

				float cellSize;
				var scaleTag = exifImage.Properties.Get<ExifDoubleArray>((ExifLibrary.ExifTag)133550);
				if(scaleTag != null && scaleTag.Value.Length > 0)
				{
					cellSize = (float)scaleTag.Value[0];
					if(scaleTag.Value.Length > 1 && scaleTag.Value[1] != scaleTag.Value[0])
					{
						ConsoleOutput.WriteWarning("Non-uniform pixel scale detected, this is not supported.");
					}
				}
				else
				{
					ConsoleOutput.WriteWarning("Pixel scale not defined, assuming scale of 1.0.");
					cellSize = 1f;
				}

				Vector2 lowerCornerCoordinate;
				var tiepointData = exifImage.Properties.Get<ExifDoubleArray>((ExifLibrary.ExifTag)133922);
				if(tiepointData != null && tiepointData.Value.Length > 0)
				{
					float pixelX = (float)tiepointData.Value[0];
					float pixelY = (float)tiepointData.Value[1];
					float coordX = (float)tiepointData.Value[3];
					float coordY = (float)tiepointData.Value[4];
					lowerCornerCoordinate = new Vector2(coordX, coordY);
					lowerCornerCoordinate.X -= pixelX * cellSize;
					lowerCornerCoordinate.Y -= pixelY * cellSize;
				}
				else
				{
					ConsoleOutput.WriteWarning("Tiepoints not found, assuming position of (0,0).");
					lowerCornerCoordinate = Vector2.Zero;
				}

				float? nodataValue = null;
				var nodataValueString = exifImage.Properties.Get<ExifEncodedString>((ExifLibrary.ExifTag)142113)?.Value;
				if(nodataValueString != null)
				{
					nodataValue = float.Parse(nodataValueString);
				}

				var data = new ElevationData((int)imgWidth, (int)imgHeight)
				{
					CellSize = cellSize,
					LowerCornerPosition = lowerCornerCoordinate
				};

				var pixels = input.GetPixelsUnsafe();
				for(int y = 0; y < imgHeight; y++)
				{
					for(int x = 0; x < imgWidth; x++)
					{
						float elevation = pixels[x, (int)imgHeight - y - 1].GetChannel(0) / 65536f;
						if(IsNoData(elevation, nodataValue))
						{
							elevation = ElevationData.NODATA_VALUE;
						}
						data.SetHeightAt(x, y, elevation);
					}
				}

				data.RecalculateElevationRange(true);
				return data;
			}
		}
	}
}
