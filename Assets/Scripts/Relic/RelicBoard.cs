using System;
using System.Collections.Generic;
using UnityEngine;

namespace Relic
{
	/// <summary>
	/// Pure board model for relic placement and merging.
	/// </summary>
	/// <remarks>
	/// Construct it with board dimensions and manipulate it through the placement helpers.
	/// </remarks>
	public sealed class RelicBoard
	{
		private RelicInstance[,] _cells;
		private readonly HashSet<RelicInstance> _placedRelics = new();

		/// <summary>
		/// Fired when a cell changes.
		/// </summary>
		public event Action<Vector2Int> onCellChanged;
		/// <summary>
		/// Fired when the board size changes.
		/// </summary>
		public event Action onBoardResized;

		/// <summary>
		/// Board width in cells.
		/// </summary>
		public int Columns { get; private set; }
		/// <summary>
		/// Board height in cells.
		/// </summary>
		public int Rows { get; private set; }

		/// <summary>
		/// Relics currently placed on the board.
		/// </summary>
		public IReadOnlyCollection<RelicInstance> PlacedRelics => _placedRelics;

		/// <summary>
		/// Creates a board with the specified size.
		/// </summary>
		/// <param name="columns">Board width.</param>
		/// <param name="rows">Board height.</param>
		public RelicBoard(int columns, int rows)
		{
			if (columns <= 0) throw new ArgumentOutOfRangeException(nameof(columns));
			if (rows <= 0) throw new ArgumentOutOfRangeException(nameof(rows));
			Columns = columns;
			Rows = rows;
			_cells = new RelicInstance[columns, rows];
		}

		/// <summary>
		/// Checks whether a coordinate is within the board bounds.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <returns>True if the coordinate is valid.</returns>
		public bool IsInBounds(int x, int y)
		{
			return x >= 0 && x < Columns && y >= 0 && y < Rows;
		}
		public bool IsInBounds(Vector2Int pos)
		{
			return IsInBounds(pos.x, pos.y);
		}

		/// <summary>
		/// Gets the occupant at a cell coordinate.
		/// </summary>
		/// <param name="x">X coordinate.</param>
		/// <param name="y">Y coordinate.</param>
		/// <returns>The occupant, or null.</returns>
		public RelicInstance GetCell(int x, int y)
		{
			return IsInBounds(x, y) ? _cells[x, y] : null;
		}
		public RelicInstance GetCell(Vector2Int pos)
		{
			return GetCell(pos.x, pos.y);
		}

		/// <summary>
		/// Checks whether a shape can be placed at the anchor.
		/// </summary>
		/// <param name="shape">Shape to test.</param>
		/// <param name="anchor">Anchor position.</param>
		/// <returns>True if the shape fits and all cells are free.</returns>
		public bool CanPlace(RelicShape shape, Vector2Int anchor)
		{
			if (shape == null || !shape.IsValid) return false;
			foreach (Vector2Int offset in shape.Cells)
			{
				Vector2Int pos = anchor + offset;
				if (!IsInBounds(pos)) return false;
				if (_cells[pos.x, pos.y] != null) return false;
			}

			return true;
		}

		/// <summary>
		/// Checks whether a shape can be placed while ignoring one instance.
		/// </summary>
		/// <param name="shape">Shape to test.</param>
		/// <param name="anchor">Anchor position.</param>
		/// <param name="ignoreInstance">Instance to ignore while testing overlap.</param>
		/// <returns>True if the shape fits.</returns>
		public bool CanPlaceIgnoring(RelicShape shape, Vector2Int anchor, RelicInstance ignoreInstance)
		{
			if (shape == null || !shape.IsValid) return false;
			foreach (Vector2Int offset in shape.Cells)
			{
				Vector2Int pos = anchor + offset;
				if (!IsInBounds(pos)) return false;
				RelicInstance occupant = _cells[pos.x, pos.y];
				if (occupant != null && occupant != ignoreInstance) return false;
			}

			return true;
		}

		/// <summary>
		/// Places an instance on the board.
		/// </summary>
		/// <param name="instance">Relic instance to place.</param>
		/// <param name="shape">Placement shape.</param>
		/// <param name="anchor">Placement anchor.</param>
		/// <returns>True when placement succeeded.</returns>
		public bool TryPlace(RelicInstance instance, RelicShape shape, Vector2Int anchor)
		{
			if (instance == null || !CanPlace(shape, anchor)) return false;

			foreach (Vector2Int offset in shape.Cells)
			{
				Vector2Int pos = anchor + offset;
				_cells[pos.x, pos.y] = instance;
				onCellChanged?.Invoke(pos);
			}

			instance.SetPlaced(anchor);
			_placedRelics.Add(instance);
			return true;
		}

		/// <summary>
		/// Removes an instance from the board.
		/// </summary>
		/// <param name="instance">Instance to remove.</param>
		/// <returns>True when the instance was found and cleared.</returns>
		public bool Remove(RelicInstance instance)
		{
			if (instance == null) return false;

			bool removed = false;
			for (int x = 0; x < Columns; x++)
			{
				for (int y = 0; y < Rows; y++)
				{
					if (_cells[x, y] != instance) continue;
					_cells[x, y] = null;
					onCellChanged?.Invoke(new Vector2Int(x, y));
					removed = true;
				}
			}

			if (!removed)
				return false;

			_placedRelics.Remove(instance);
			instance.ClearPlaced();
			return true;
		}

		/// <summary>
		/// Expands the board size.
		/// </summary>
		/// <param name="newColumns">New board width.</param>
		/// <param name="newRows">New board height.</param>
		public void Resize(int newColumns, int newRows)
		{
			if (newColumns < Columns || newRows < Rows)
				throw new InvalidOperationException("Board cannot be shrunk while relics may be placed.");

			RelicInstance[,] newCells = new RelicInstance[newColumns, newRows];
			for (int x = 0; x < Columns; x++)
			{
				for (int y = 0; y < Rows; y++)
				{
					newCells[x, y] = _cells[x, y];
				}
			}

			Columns = newColumns;
			Rows = newRows;
			_cells = newCells;
			onBoardResized?.Invoke();
		}
	}
}
