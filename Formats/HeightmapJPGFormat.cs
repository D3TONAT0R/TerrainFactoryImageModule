using TerrainFactory;
using TerrainFactory.Formats;

namespace TerrainFactory.Modules.Bitmaps.Formats
{
	public class HeightmapJPGFormat : HeightmapFormatBase
	{
		public override string Identifier => "JPG";
		public override string ReadableName => "JPG Heightmap";
		public override string CommandKey => "jpg";
		public override string Description => ReadableName;
		public override string Extension => "jpg";
		public override FileSupportFlags SupportedActions => FileSupportFlags.Import;

		public override bool Is16BitFormat => false;
	}
}
