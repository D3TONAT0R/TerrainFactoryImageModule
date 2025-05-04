namespace TerrainFactory.Modules.Bitmaps.Formats
{
	public class HeightmapPNG16Format : HeightmapFormatBase
	{
		public override string Identifier => "PNG16";
		public override string ReadableName => "PNG Heightmap (16 Bit)";
		public override string Command => "png16";
		public override string Description => ReadableName;
		public override string Extension => "png";
		public override FileSupportFlags SupportedActions => FileSupportFlags.ImportAndExport;

		public override bool Is16BitFormat => true;
	}
}
