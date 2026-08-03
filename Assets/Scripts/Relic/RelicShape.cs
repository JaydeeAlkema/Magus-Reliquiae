using System;
using System.Collections.Generic;
using UnityEngine;

namespace Relic
{
	[Serializable]
	public class RelicShape
	{
		public Vector2Int[] Cells = Array.Empty<Vector2Int>();

		public static RelicShape Default => new()
		{
			Cells = new[]
			{
				Vector2Int.zero,
			},
		};

		public bool IsValid => Cells != null && Cells.Length > 0;
		public int CellCount => Cells?.Length ?? 0;

		public IEnumerable<Vector2Int> GetWorldCells(Vector2Int anchor)
		{
			if (Cells == null) yield break;
			foreach (Vector2Int offset in Cells)
			{
				yield return anchor + offset;
			}
		}

		public bool ContainsOffset(Vector2Int offset)
		{
			if (Cells == null) return false;
			foreach (Vector2Int cell in Cells)
			{
				if (cell == offset) return true;
			}

			return false;
		}
	}
}
