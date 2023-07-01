using HMCon;
using HMCon.Export;
using HMCon.Formats;
using HMCon.Util;
using HMConImage;
using System;
using System.Collections.Generic;
using System.Text;

namespace HMConImage.Formats
{
	public abstract class HeightmapFormatBase : FileFormat
	{

		protected override HeightData ImportFile(string importPath, params string[] args)
		{
			return HeightmapImporter.Import(importPath, args);
		}

		public override void ModifyFileName(ExportTask task, FileNameBuilder nameBuilder)
		{
			nameBuilder.suffix = "height";
		}
	}
}
