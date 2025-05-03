namespace TerrainFactory.Modules.Bitmaps.Formats
{
	public class HeightmapPNG8Format : HeightmapFormatBase
	{
		public override string Identifier => "PNG";
		public override string ReadableName => "PNG Heightmap (8 Bit)";
		public override string CommandKey => "png";
		public override string Description => ReadableName;
		public override string Extension => "png";
		public override FileSupportFlags SupportedActions => FileSupportFlags.ImportAndExport;

		public override bool Is16BitFormat => false;
	}
}
