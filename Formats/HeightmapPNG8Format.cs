using TerrainFactory;
using TerrainFactory.Export;
using TerrainFactory.Formats;

namespace TerrainFactory.Modules.Images.Formats
{
	public class HeightmapPNG8Format : HeightmapFormatBase
	{
		public override string Identifier => "PNG";
		public override string ReadableName => "PNG Heightmap (8 Bit)";
		public override string CommandKey => "png";
		public override string Description => ReadableName;
		public override string Extension => "png";
		public override FileSupportFlags SupportedActions => FileSupportFlags.ImportAndExport;

		protected override bool ExportFile(string path, ExportTask task)
		{
			var gen = new ImageGeneratorMagick(task.data, ImageType.Heightmap8, task.data.LowPoint, task.data.HighPoint);
			gen.WriteFile(path, ImageMagick.MagickFormat.Png24);
			return true;
		}
	}
}
