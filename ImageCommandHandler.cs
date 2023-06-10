using HMCon;
using HMCon.Export;
using HMCon.Util;
using System;
using System.Collections.Generic;

namespace HMConImage {
	public class ImageCommandHandler : HMConCommandHandler {
		public override void AddCommands(List<ConsoleCommand> list) {
			list.Add(new ConsoleCommand("preview", "", "Previews the grid data in an image", HandlePreviewCmd));
			list.Add(new ConsoleCommand("preview-hm", "", "Previews the grid data in a heightmap", HandleHMPreviewCmd));
		}

		private bool HandlePreviewCmd(Worksheet sheet, string[] args) {
			OpenPreview(sheet, false);
			return true;
		}

		private bool HandleHMPreviewCmd(Worksheet sheet, string[] args) {
			OpenPreview(sheet, true);
			return true;
		}

		private void OpenPreview(Worksheet sheet, bool heightmap) {
			ConsoleOutput.WriteLine("Opening preview...");
			Previewer.OpenDataPreview(sheet, heightmap);
		}
	}
}
