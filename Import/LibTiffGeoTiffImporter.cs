using BitMiracle.LibTiff.Classic;
using TerrainFactory;
using TerrainFactory.Import;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TerrainFactory.Modules.Images.Import
{
	public static class LibTiffGeoTiffImporter
	{

		public static ElevationData Import(string importPath, params string[] args)
		{
			Tiff.SetTagExtender(GeoTiffTagExtender);
			using(var input = Tiff.Open(importPath, "r"))
			{
				int imgWidth = input.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
				int imgHeight = input.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
				int depth = input.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt() / 8;

				float cellSize;
				var scaleTag = input.GetField(TiffTag.GEOTIFF_MODELPIXELSCALETAG);
				if(scaleTag != null && scaleTag.Length > 0)
				{
					double[] scales = scaleTag[0].ToDoubleArray();
					cellSize = (float)scales[0];
				}
				else
				{
					ConsoleOutput.WriteWarning("Pixel scale not defined, assuming scale of 1.0.");
					cellSize = 1f;
				}

				Vector2 lowerCornerCoordinate;
				var tiepointData = input.GetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG)?[0].ToDoubleArray();
				if(tiepointData != null && tiepointData.Length > 0)
				{
					float pixelX = (float)tiepointData[0];
					float pixelY = (float)tiepointData[1];
					float coordX = (float)tiepointData[3];
					float coordY = (float)tiepointData[4];
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
				var nodataValueString = input.GetField((TiffTag)42113)?[0].ToString();
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

				if(input.IsTiled())
				{
					int splitX = input.GetField(TiffTag.TILEWIDTH)[0].ToInt();
					int splitY = input.GetField(TiffTag.TILELENGTH)[0].ToInt();
					int tilesX = (int)Math.Ceiling(imgWidth / (float)splitX);
					int tilesY = (int)Math.Ceiling(imgHeight / (float)splitY);

					for(int ty = 0; ty < tilesY; ty++)
					{
						for(int tx = 0; tx < tilesX; tx++)
						{
							int xMin = tx * splitX;
							int yMin = ty * splitY;
							int xMax = Math.Min(xMin + splitX - 1, imgWidth - 1);
							int yMax = Math.Min(yMin + splitY - 1, imgHeight - 1);
							//int xMax = xMin + splitX;
							//int yMax = yMin + splitY;
							int tileWidth = xMax - xMin;
							int tileHeight = yMax - yMin;
							byte[] rawData = new byte[splitX * splitY * depth];

							input.ReadTile(rawData, 0, tx * splitX, ty * splitY, 0, 0);

							for(int y = 0; y <= tileHeight; y++)
							{
								for(int x = 0; x <= tileWidth; x++)
								{
									int i = (y * splitX + x) * depth;
									float height;
									if(depth == 4)
									{
										height = BitConverter.ToSingle(new byte[] { rawData[i], rawData[i + 1], rawData[i + 2], rawData[i + 3] }, 0);
									}
									else
									{
										throw new NotImplementedException();
									}
									data.SetHeightAt(xMin + x, (imgHeight - 1) - (yMin + y), height);
								}
							}
						}
					}
				}
				else
				{
					throw new NotImplementedException();
					/*
					for(int i = 0; i < input.NumberOfStrips(); i++)
					{
						offset += input.ReadRawStrip(i, rawData, offset, (int)input.RawStripSize(i));
					}
					*/
				}
				Tiff.SetTagExtender(null);

				data.RecalculateElevationRange(true);
				return data;

				/*

				//Working example
				byte[] rawData = new byte[512 * 512 * 4];
				input.ReadTile(rawData, 0, 0, 0, 0, 0);
				float[,] hm = new float[512, 512];
				for(int y = 0; y < 512; y++)
				{
					for(int x = 0; x < 512; x++)
					{
						int i = (y * 512 + x) * 4;
						//hm[x, y] = BitConverter.ToSingle(new byte[] { rawData[i + 3], rawData[i + 2], rawData[i + 1], rawData[i] }, 0);
						hm[x, 511 - y] = BitConverter.ToSingle(new byte[] { rawData[i], rawData[i + 1], rawData[i + 2], rawData[i + 3] }, 0);
					}
				}
				var data = new HeightData(hm, 1f) { isValid = true };
				data.RecalculateValues(true);
				return data;

				*/
			}
		}

		public static void GeoTiffTagExtender(Tiff tif)
		{
			//short tiePointCount = (short)(tif.GetField(TiffTag.GEOTIFF_MODELTIEPOINTTAG)?[0].ToInt() ?? -1);
			var fieldInfo = new TiffFieldInfo[]
			{
				new TiffFieldInfo((TiffTag)33550, 3, 3, TiffType.DOUBLE, FieldBit.Custom, true, false, "ModelPixelScaleTag"),
				new TiffFieldInfo((TiffTag)33922, 6, 6, TiffType.DOUBLE, FieldBit.Custom, true, false, "ModelTiepointTag"),
				new TiffFieldInfo((TiffTag)34735, -1, -1, TiffType.SHORT, FieldBit.Custom, true, false, "GeoKeyDirectoryTag"),
				new TiffFieldInfo((TiffTag)34736, -1, -1, TiffType.DOUBLE, FieldBit.Custom, true, false, "GeoDoubleParamsTag"),
				new TiffFieldInfo((TiffTag)34737, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "GeoAsciiParamsTag"),

				new TiffFieldInfo((TiffTag)42112, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "GDAL_METADATA"),
				new TiffFieldInfo((TiffTag)42113, -1, -1, TiffType.ASCII, FieldBit.Custom, true, false, "GDAL_NODATA")
			};
			tif.MergeFieldInfo(fieldInfo, fieldInfo.Length);
		}
	}
}
