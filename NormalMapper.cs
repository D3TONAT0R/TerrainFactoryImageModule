using TerrainFactory;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace TerrainFactory.Modules.Bitmaps
{
	public static class NormalMapper
	{

		const double Rad2Deg = 180f / Math.PI;

		public static Vector3[,] CalculateNormals(ElevationData grid, bool sharpMode)
		{
			if (sharpMode)
			{
				var normals = new Vector3[grid.CellCountX, grid.CellCountY];
				for (int x = 0; x < grid.CellCountX - 1; x++)
				{
					for (int y = 0; y < grid.CellCountY - 1; y++)
					{
						float ll = grid.GetElevationAtCellClamped(x, y);
						float lr = grid.GetElevationAtCellClamped(x + 1, y);
						float ul = grid.GetElevationAtCellClamped(x, y + 1);
						float ur = grid.GetElevationAtCellClamped(x + 1, y + 1);
						float nrmX = (GetSlope(lr, ll, grid.CellSize) + GetSlope(ur, ul, grid.CellSize)) / 2f;
						float nrmY = (GetSlope(ul, ll, grid.CellSize) + GetSlope(ur, lr, grid.CellSize)) / 2f;
						float power = Math.Abs(nrmX) + Math.Abs(nrmY);
						if (power > 1)
						{
							nrmX /= power;
							nrmY /= power;
						}
						float nrmZ = 1f - power;
						normals[x, y] = Vector3.Normalize(new Vector3(nrmX, nrmY, nrmZ));
					}
				}
				return normals;
			}
			else
			{
				var normals = new Vector3[grid.CellCountX, grid.CellCountY];
				for (int x = 0; x < grid.CellCountX; x++)
				{
					for (int y = 0; y < grid.CellCountY; y++)
					{
						float m = grid.GetElevationAtCellClamped(x, y);
						float r = GetSlope(grid.GetElevationAtCellClamped(x + 1, y), m, grid.CellSize);
						float l = GetSlope(m, grid.GetElevationAtCellClamped(x - 1, y), grid.CellSize);
						float u = GetSlope(grid.GetElevationAtCellClamped(x, y + 1), m, grid.CellSize);
						float d = GetSlope(m, grid.GetElevationAtCellClamped(x, y - 1), grid.CellSize);
						float nrmX = (r + l) / 2f;
						float nrmY = (u + d) / 2f;
						float power = Math.Abs(nrmX) + Math.Abs(nrmY);
						if (power > 1)
						{
							nrmX /= power;
							nrmY /= power;
						}
						float nrmZ = 1f - power;
						normals[x, y] = Vector3.Normalize(new Vector3(nrmX, nrmY, nrmZ));
					}
				}
				return normals;
			}
		}

		private static float GetSlope(float from, float to, float gridSpacing)
		{
			float hdiff = to - from;
			return (float)(Math.Atan(hdiff / gridSpacing) * Rad2Deg / 90f);
		}
	}
}
