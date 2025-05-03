using TerrainFactory;
using TerrainFactory.Export;
using TerrainFactory.Formats;
using TerrainFactory.Util;
using TerrainFactory.Modules.Bitmaps;
using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace TerrainFactory.Modules.Bitmaps.Formats
{
	public abstract class HeightmapFormatBase : FileFormat
	{
		public abstract bool Is16BitFormat { get; }

		protected override ElevationData ImportFile(string importPath, params string[] args)
		{
			return HeightmapImporter.Import(importPath, args);
		}

		public override void ModifyFileName(ExportTask task, FileNameBuilder nameBuilder)
		{
			nameBuilder.suffix = "height";
		}

		protected override bool ExportFile(string path, ExportTask task)
		{
			var img = ImageGenerator.CreateHeightMap(task.data, Is16BitFormat);
			img.Write(path, Is16BitFormat ? ImageMagick.MagickFormat.Png48 : ImageMagick.MagickFormat.Png24);
			return true;
		}
	}
}
