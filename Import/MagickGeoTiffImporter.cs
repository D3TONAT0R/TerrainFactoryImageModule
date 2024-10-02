using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using ImageMagick;

namespace TerrainFactory.Modules.Bitmaps.Import
{
	public static class MagickGeoTiffImporter
	{

		public static ElevationData Import(string importPath, params string[] args)
		{
			MagickNET.Initialize();

			var task = new NExifTool.Reader.ExifReader(new NExifTool.ExifToolOptions()).ReadExifAsync(importPath);
			task.RunSynchronously();
			ConsoleOutput.WriteLine(task.Result.ToString());

			using(var input = new MagickImage(importPath))
			{
				//TODO: ImageMagick does not load exif data (is null)
				var exif = input.GetExifProfile();

				int imgWidth = input.Width;
				int imgHeight = input.Height;
				int depth = input.Depth / 8;

				float cellSize;
				var scaleTag = exif.GetValue(ExifTag.PixelScale);
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
				var tiepointData = exif.GetValue(ExifTag.ModelTiePoint);
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
					ConsoleOutput.WriteWarning("Coordinates not defined, assuming position of (0,0).");
					lowerCornerCoordinate = Vector2.Zero;
				}

				float? nodataValue = null;
				var nodataValueString = exif.GetValue(ExifTag.GDALNoData)?.Value;
				if(nodataValueString != null)
				{
					nodataValue = float.Parse(nodataValueString);
				}

				var data = new ElevationData(imgWidth, imgHeight, importPath)
				{
					CellSize = cellSize,
					LowerCornerPosition = lowerCornerCoordinate
				};
				if(nodataValue.HasValue)
				{
					data.NoDataValue = nodataValue.Value;
				}

				var pixels = input.GetPixelsUnsafe();
				for(int y = 0; y < imgHeight; y++)
				{
					for(int x = 0; x < imgWidth; x++)
					{
						float elevation = pixels[x, imgHeight - y - 1].GetChannel(0) / 65536f;
						data.SetHeightAt(x, y, elevation);
					}
				}

				data.RecalculateElevationRange(true);
				return data;
			}
		}

		private static void Log(object sender, LogEventArgs e)
		{
			ConsoleOutput.WriteLine(e.Message);
		}
	}
}
