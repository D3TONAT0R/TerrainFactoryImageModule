using TerrainFactory;
using TerrainFactory.Commands;
using TerrainFactory.Export;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory.Modules.Bitmaps
{
	public static class ImageCommands
	{
		[Command("preview", "", "Previews the grid data in an image")]
		public static bool ExecPreviewCmd(Project sheet, string[] args)
		{
			OpenPreview(sheet, false);
			return true;
		}

		[Command("preview-hm", "", "Previews the grid data in a heightmap")]
		public static bool ExecHMPreviewCmd(Project sheet, string[] args)
		{
			OpenPreview(sheet, true);
			return true;
		}

		private static void OpenPreview(Project sheet, bool heightmap)
		{
			ConsoleOutput.WriteLine("Opening preview...");
			Previewer.OpenDataPreview(sheet, heightmap);
		}
	}
}
