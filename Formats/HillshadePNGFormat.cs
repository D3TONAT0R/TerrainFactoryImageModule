﻿using TerrainFactory;
using TerrainFactory.Export;
using TerrainFactory.Formats;
using TerrainFactory.Util;

namespace TerrainFactory.Modules.Bitmaps.Formats
{
	public class HillshadePNGFormat : FileFormat
	{
		public override string Identifier => "PNG_HS";
		public override string ReadableName => "PNG Hillshade Map";
		public override string Command => "png-hs";
		public override string Description => ReadableName;
		public override string Extension => "png";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Export;

		protected override bool ExportFile(string path, ExportTask task)
		{
			var img = ImageGenerator.CreateHillshadeMap(task.data);
			img.Write(path, ImageMagick.MagickFormat.Png24);
			return true;
		}

		public override void ModifyFileName(ExportTask task, FileNameBuilder nameBuilder)
		{
			nameBuilder.suffix = "hillshade";
		}
	}
}
