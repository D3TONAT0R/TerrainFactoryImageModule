using TerrainFactory;
using TerrainFactory.Export;
using TerrainFactory.Formats;
using TerrainFactory.Util;
using TerrainFactory.Modules.Images;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory.Modules.Images.Formats
{
	public abstract class HeightmapFormatBase : FileFormat
	{

		protected override ElevationData ImportFile(string importPath, params string[] args)
		{
			return HeightmapImporter.Import(importPath, args);
		}

		public override void ModifyFileName(ExportTask task, FileNameBuilder nameBuilder)
		{
			nameBuilder.suffix = "height";
		}
	}
}
