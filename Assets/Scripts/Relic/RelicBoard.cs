using System;
using System.Collections.Generic;
using UnityEngine;

namespace Relic
{
	public sealed class RelicBoard
	{
		private RelicInstance[,] _cells;
		private readonly HashSet<RelicInstance> _placedRelics = new();

		public event Action<Vector2Int> onCellChanged;
		public event Action onBoardResized;

		public int Columns { get; private set; }
		public int Rows { get; private set; }

		public IReadOnlyCollection<RelicInstance> PlacedRelics => _placedRelics;

		public RelicBoard(int columns, int rows)
		{
			if (columns <= 0) throw new ArgumentOutOfRangeException(nameof(columns));
			if (rows <= 0) throw new ArgumentOutOfRangeException(nameof(rows));
			Columns = columns;
			Rows = rows;
			_cells = new RelicInstance[columns, rows];
		}

		public bool IsInBounds(int x, int y)
		{
			return x >= 0 && x < Columns && y >= 0 && y < Rows;
		}
		public bool IsInBounds(Vector2Int pos)
		{
			return IsInBounds(pos.x, pos.y);
		}

		public RelicInstance GetCell(int x, int y)
		{
			return IsInBounds(x, y) ? _cells[x, y] : null;
		}
		public RelicInstance GetCell(Vector2Int pos)
		{
			return GetCell(pos.x, pos.y);
		}

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
