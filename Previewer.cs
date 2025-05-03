using ImageMagick;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ImageMagick.Drawing;

namespace TerrainFactory.Modules.Bitmaps
{
	public class Previewer {

		public static readonly (int size, MagickColor col)[] allGrids = new (int, MagickColor)[] {
			(10,MagickColors.Black),
			(50,MagickColors.DarkCyan),
			(100,MagickColors.DarkBlue),
			(500,MagickColors.DarkGreen),
			(1000,MagickColors.Yellow),
		};

		public static void OpenDataPreview(Project sheet, bool heightmap) {

			sheet.InputData.LoadIfRequired();
			var data = sheet.ApplyModificationChain(sheet.InputData.Current);

			var img = ImageGenerator.CreateImage(data, heightmap ? ImageType.Heightmap8 : ImageType.CombinedPreview);
			MakeGrid(img, data.offsetFromSource);
			string path = Path.GetTempPath() + Guid.NewGuid() + ".png";
			img.Write(path, MagickFormat.Png24);
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
			Queue<(int size, MagickColor col)> grids = new Queue<(int size, MagickColor col)>();
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

		private static void DrawGrid(MagickImage img, int size, MagickColor color, float opacity, bool drawCoords, (int x, int y) offsetFromSource) {
			var pixels = img.GetPixels();
			//vertical lines
			for(int x = 0; x < img.Width; x++) {
				if((x - offsetFromSource.x) % size == 0) {
					for(int y = 0; y < img.Height; y++) {
						ColorUtil.SetPixel(img, pixels, x, y, color, opacity);
					}
					if(drawCoords && x > 20) {
						int tx = x;
						int ty = (int)img.Height - 2;
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
						int ty = (int)img.Height - y - 1;
						DrawString(img, (y + offsetFromSource.y).ToString(), color, ref tx, ref ty);
					}
				}
			}
		}

		private static void DrawGridLegend(MagickImage img, int size, MagickColor color, int index) {
			//Draw info text in the corner
			int x = 2;
			int y = (int)(2 + index * 16);
			DrawString(img, size.ToString(), color, ref x, ref y);
		}

		private static void DrawString(MagickImage img, string str, MagickColor color, ref int x, ref int y) {
			if(MinDim(img) > 200) {
				var drawables = new Drawables().FillColor(new MagickColor(color)).Text(x, y, str);
				drawables.Draw(img);
				//g.DrawString(str, SystemFonts.MessageBoxFont, new SolidBrush(color), new PointF(x, y));
			} else {
				PixelFont.DrawString(img, str, ref x, ref y, color, 1);
			}
		}

		private static int MinDim(MagickImage img) {
			if(img.Width < img.Height) return (int)img.Width;
			else return (int)img.Height;
		}
	}
}