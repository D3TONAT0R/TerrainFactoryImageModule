using HMCon;
using HMCon.Export;
using HMCon.Formats;

namespace HMConImage.Formats
{
	public class HeightmapPNG16Format : HeightmapFormatBase
	{
		public override string Identifier => "PNG16";
		public override string ReadableName => "PNG Heightmap (16 Bit)";
		public override string CommandKey => "png16";
		public override string Description => ReadableName;
		public override string Extension => "png";
		public override FileSupportFlags SupportedActions => FileSupportFlags.ImportAndExport;

		protected override bool ExportFile(string path, ExportTask task)
		{
			var gen = new ImageGeneratorMagick(task.data, ImageType.Heightmap16, task.data.lowPoint, task.data.highPoint);
			gen.WriteFile(path, ImageMagick.MagickFormat.Png48);
			return true;
		}
	}
}
