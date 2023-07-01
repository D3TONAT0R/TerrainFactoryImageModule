﻿using HMCon;
using HMCon.Export;
using HMCon.Formats;
using HMCon.Import;
using HMConImage.Formats;
using System;
using System.Collections.Generic;

namespace HMConImage
{

	public class HMConImageModule : HMConModule
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
