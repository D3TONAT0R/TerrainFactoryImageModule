using TerrainFactory;
using TerrainFactory.Export;
using TerrainFactory.Formats;
using TerrainFactory.Modules.Images.Formats;
using ImageMagick;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace TerrainFactory.Modules.Images {
	public class Previewer {

		public static readonly (int size, Color col)[] allGrids = new (int, Color)[] {
			(10,Color.Black),
			(50,Color.DarkCyan),
			(100,Color.DarkBlue),
			(500,Color.DarkGreen),
			(1000,Color.Yellow),
		};

		public static void OpenDataPreview(Worksheet sheet, bool heightmap) {

			var data = sheet.ApplyModificationChain(sheet.CurrentData);

			var exporter = new ImageGeneratorMagick(data, heightmap ? ImageType.Heightmap8 : ImageType.CombinedPreview, data.lowPoint, data.highPoint);
			MakeGrid(exporter.GetImage(), data.offsetFromSource);
			string path = Path.GetTempPath() + Guid.NewGuid().ToString() + ".png";
			exporter.WriteFile(path, ImageMagick.MagickFormat.Png24);
			var p = new Process {
				StartInfo = new ProcessStartInfo(path) {
					UseShellExecute = true
				}
			};
			p.Start();
		}

		private static void MakeGrid(MagickImage img, (int x, int y) offsetFromSource) {
			int dim = MinDim(img);
			if(dim < 50) return;
			Queue<(int size, Color col)> grids = new Queue<(int size, Color col)>();
			foreach(var g in allGrids) {
				if(Range(dim, g.size * 2, g.size * 20)) grids.Enqueue(g);
			}
			int i = 0;
			while(grids.Count > 0) {
				float opacity = 1f;// (float)Math.Pow(1f / grids.Count, 2);
				var (size, col) = grids.Dequeue();
				DrawGrid(img, size, col, opacity, i == 0, offsetFromSource);
				DrawGridLegend(img, size, col, i);
				i++;
			}
		}

		private static bool Range(int i, int min, int max) {
			return i >= min && i < max;
		}

		private static void DrawGrid(MagickImage img, int size, Color color, float opacity, bool drawCoords, (int x, int y) offsetFromSource) {
			var pixels = img.GetPixels();
			//vertical lines
			for(int x = 0; x < img.Width; x++) {
				if((x - offsetFromSource.x) % size == 0) {
					for(int y = 0; y < img.Height; y++) {
						ColorUtil.SetPixel(img, pixels, x, y, color, opacity);
					}
					if(drawCoords && x > 20) {
						int tx = x;
						int ty = img.Height - 2;
						DrawString(img, (x + offsetFromSource.x).ToString(), color, ref tx, ref ty);
					}
				}
			}
			//horizontal lines
			for(int y = 0; y < img.Height; y++) {
				if((y - offsetFromSource.y) % size == 0) {
					for(int x = 0; x < img.Width; x++) {
						ColorUtil.SetPixel(img, pixels, x, y, color, opacity);
					}
					if(drawCoords && y > 20) {
						int tx = 2;
						int ty = img.Height - y - 1;
						DrawString(img, (y + offsetFromSource.y).ToString(), color, ref tx, ref ty);
					}
				}
			}
		}

		private static void DrawGridLegend(MagickImage img, int size, Color color, int index) {
			//Draw info text in the corner
			int x = 2;
			int y = (int)(2 + index * (SystemFonts.MessageBoxFont.Size + 2));
			DrawString(img, size.ToString(), color, ref x, ref y);
		}

		private static void DrawString(MagickImage img, string str, Color color, ref int x, ref int y) {
			if(MinDim(img) > 200) {
				var drawables = new Drawables().FillColor(new MagickColor(ColorUtil.CreateMagickColor(color))).Text(x, y, str);
				drawables.Draw(img);
				//g.DrawString(str, SystemFonts.MessageBoxFont, new SolidBrush(color), new PointF(x, y));
			} else {
				PixelFont.DrawString(img, str, ref x, ref y, color, 1);
			}
		}

		private static int MinDim(MagickImage img) {
			if(img.Width < img.Height) return img.Width;
			else return img.Height;
		}
	}
}