using TerrainFactory;
using TerrainFactory.Modules.Images.Import;
using System;
using System.Collections.Generic;
using System.Text;

namespace TerrainFactory.Modules.Images.Formats
{
	public class HeightmapGeoTIFFFormat : HeightmapFormatBase
	{
		public override string Identifier => "GEOTIFF";
		public override string ReadableName => "GeoTIFF";
		public override string CommandKey => "geotif";
		public override string Description => "GeoTIFF (32 Bit)";
		public override string Extension => "tif";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Import;

		protected override HeightData ImportFile(string importPath, params string[] args)
		{
			return GeoTiffImporter.Import(importPath, args);
		}
	}
}
