using TerrainFactory;
using TerrainFactory.Export;
using TerrainFactory.Formats;
using TerrainFactory.Util;

namespace TerrainFactory.Modules.Bitmaps.Formats
{
	public class NormalPNGFormat : FileFormat
	{
		public override string Identifier => "PNG_NM";
		public override string ReadableName => "PNG Normal Map";
		public override string Command => "png-nm";
		public override string Description => ReadableName;
		public override string Extension => "png";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Export;

		protected override bool ExportFile(string path, ExportTask task)
		{
			var img = ImageGenerator.CreateNormalMap(task.data, false);
			img.Write(path, ImageMagick.MagickFormat.Png24);
			return true;
		}

		public override void ModifyFileName(ExportTask task, FileNameBuilder nameBuilder)
		{
			nameBuilder.suffix = "normal";
		}
	}
}
