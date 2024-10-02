using TerrainFactory;
using TerrainFactory.Export;
using TerrainFactory.Formats;
using TerrainFactory.Import;
using System;
using System.Collections.Generic;
using TerrainFactory.Modules.Bitmaps.Formats;

namespace TerrainFactory.Modules.Bitmaps
{

	public class TerrainFactoryImageModule : TerrainFactoryModule
	{
		public override string ModuleID => "ImageModule";
		public override string ModuleName => "Image Importer/Exporter";
		public override string ModuleVersion => "1.0";

		public override void Initialize()
		{
			SupportedFormats.Add(new HeightmapPNG8Format());
			SupportedFormats.Add(new HeightmapPNG16Format());
			SupportedFormats.Add(new HeightmapJPGFormat());
			//SupportedFormats.Add(new HeightmapTIFFFormat());
			SupportedFormats.Add(new NormalPNGFormat());
			SupportedFormats.Add(new HillshadePNGFormat());
			SupportedFormats.Add(new HeightmapGeoTIFFFormat());
			CommandDefiningTypes.Add(typeof(ImageCommands));
		}
	}
}
