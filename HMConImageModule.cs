using TerrainFactory;
using TerrainFactory.Export;
using TerrainFactory.Formats;
using TerrainFactory.Import;
using TerrainFactory.Modules.Images.Formats;
using System;
using System.Collections.Generic;

namespace TerrainFactory.Modules.Images
{

	public class TerrainFactoryImageModule : TerrainFactoryModule
	{
		public override string ModuleID => "ImageModule";
		public override string ModuleName => "Image Importer/Exporter";
		public override string ModuleVersion => "1.0";

		public override void RegisterFormats(List<FileFormat> registry)
		{
			registry.Add(new HeightmapPNG8Format());
			registry.Add(new HeightmapPNG16Format());
			registry.Add(new HeightmapJPGFormat());
			//registry.Add(new HeightmapTIFFFormat());
			registry.Add(new NormalPNGFormat());
			registry.Add(new HillshadePNGFormat());
			registry.Add(new HeightmapGeoTIFFFormat());
		}

		public override IEnumerable<Type> GetCommandDefiningTypes()
		{
			yield return typeof(ImageCommands);
		}
	}
}
