using TerrainFactory;
using System;
using System.Collections.Generic;
using System.Text;
using TerrainFactory.Modules.Bitmaps.Import;

namespace TerrainFactory.Modules.Bitmaps.Formats
{
	public class HeightmapGeoTIFFFormat : HeightmapFormatBase
	{
		public override string Identifier => "GEOTIFF";
		public override string ReadableName => "GeoTIFF";
		public override string CommandKey => "geotif";
		public override string Description => "GeoTIFF (32 Bit)";
		public override string Extension => "tif";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Import;

		public override bool Is16BitFormat => false;

		protected override ElevationData ImportFile(string importPath, params string[] args)
		{
			return MagickGeoTiffImporter.Import(importPath, args);
		}
	}
}
