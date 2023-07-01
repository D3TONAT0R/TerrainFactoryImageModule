using BitMiracle.LibTiff.Classic;
using HMCon;
using HMCon.Import;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMConImage.Import
{
	public static class GeoTiffImporter
	{

		public static HeightData Import(string importPath, params string[] args)
		{
			using(var input = Tiff.Open(importPath, "r"))
			{
				int imgWidth = input.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
				int imgHeight = input.GetField(TiffTag.IMAGELENGTH)[0].ToInt();
				int depth = input.GetField(TiffTag.BITSPERSAMPLE)[0].ToInt() / 8;

				float cellSize = 1f;
				var scaleTag = input.GetField(TiffTag.GEOTIFF_MODELPIXELSCALETAG);
				if(scaleTag != null && scaleTag.Length > 0)
				{
					int tagLength = scaleTag[0].ToInt();
					double[] scales = scaleTag[1].ToDoubleArray();
					cellSize = (float)scales[0];
				}
				ConsoleOutput.WriteLine("cellSize: " + cellSize);

				var data = new HeightData(imgWidth, imgHeight, importPath);
				data.cellSize = cellSize;

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
									data.SetHeight(xMin + x, (imgHeight - 1) - (yMin + y), height);
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

				data.isValid = true;
				data.RecalculateValues(true);
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
	}
}
