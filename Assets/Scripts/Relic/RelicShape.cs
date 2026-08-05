using System;
using System.Collections.Generic;
using UnityEngine;

namespace Relic
{
	/// <summary>
	///     Serializable shape helper used by relic placement.
	/// </summary>
	/// <remarks>
	///     Built from relic level data; keep it as data, not a scene component.
	/// </remarks>
	[Serializable]
	public class RelicShape
	{
		/// <summary>
		///     Cells occupied relative to the anchor.
		/// </summary>
		public Vector2Int[] Cells = Array.Empty<Vector2Int>();

		/// <summary>
		///     Default single-cell shape.
		/// </summary>
		public static RelicShape Default => new()
		{
			Cells = new[]
			{
				Vector2Int.zero,
			},
		};

		/// <summary>
		///     True when the shape contains at least one cell.
		/// </summary>
		public bool IsValid => Cells != null && Cells.Length > 0;
		/// <summary>
		///     Number of cells in the shape.
		/// </summary>
		public int CellCount => Cells?.Length ?? 0;

		/// <summary>
		///     Enumerates world cells for the shape at the given anchor.
		/// </summary>
		/// <param name="anchor">Anchor position.</param>
		/// <returns>World-space cell positions.</returns>
		public IEnumerable<Vector2Int> GetWorldCells(Vector2Int anchor)
		{
			if (Cells == null) yield break;
			foreach (Vector2Int offset in Cells)
			{
				yield return anchor + offset;
			}
		}

		/// <summary>
		///     Checks whether the shape contains the given offset.
		/// </summary>
		/// <param name="offset">Relative offset to check.</param>
		/// <returns>True if the offset is present.</returns>
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
