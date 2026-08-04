using Relic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	/// <summary>
	/// Grid view for relic placement.
	/// </summary>
	/// <remarks>
	/// Attach this to the board root, assign the cell prefab and grid, then initialize it with a <see cref="RelicBoard"/>.
	/// </remarks>
	public class RelicBoardUI : MonoBehaviour
	{
		/// <summary>
		/// Cell prefab used to build the board grid.
		/// </summary>
		[SerializeField] private RelicCellUI CellPrefab;
		/// <summary>
		/// Grid layout that hosts the cells.
		/// </summary>
		[SerializeField] private GridLayoutGroup Grid;

		private RelicCellUI[,] _cells;

		/// <summary>
		/// Active relic board.
		/// </summary>
		public RelicBoard Board { get; private set; }

		/// <summary>
		/// Binds the board view to a relic board.
		/// </summary>
		/// <param name="board">Board to display.</param>
		public void Initialize(RelicBoard board)
		{
			if (Board != null)
			{
				Board.onCellChanged -= HandleCellChanged;
				Board.onBoardResized -= RebuildGrid;
			}

			Board = board;
			board.onCellChanged += HandleCellChanged;
			board.onBoardResized += RebuildGrid;
			RebuildGrid();
		}

		private void OnDestroy()
		{
			if (Board == null) return;
			Board.onCellChanged -= HandleCellChanged;
			Board.onBoardResized -= RebuildGrid;
		}

		private void RebuildGrid()
		{
			if (Grid == null || CellPrefab == null) return;

			foreach (Transform child in Grid.transform)
			{
				Destroy(child.gameObject);
			}

			_cells = new RelicCellUI[Board.Columns, Board.Rows];

			for (int y = 0; y < Board.Rows; y++)
			{
				for (int x = 0; x < Board.Columns; x++)
				{
					RelicCellUI cell = Instantiate(CellPrefab, Grid.transform);
					cell.Initialize(new Vector2Int(x, y), this);
					_cells[x, y] = cell;
				}
			}

			RefreshAll();
		}

		private void HandleCellChanged(Vector2Int pos)
		{
			if (_cells == null) return;
			RelicCellUI cell = GetCell(pos);
			if (cell == null) return;

			RelicInstance inst = Board.GetCell(pos);
			bool isAnchor = inst != null && inst.AnchorPosition == pos;
			cell.Refresh(inst, isAnchor);
		}

		private void RefreshAll()
		{
			if (_cells == null) return;
			for (int y = 0; y < Board.Rows; y++)
			{
				for (int x = 0; x < Board.Columns; x++)
				{
					Vector2Int pos = new(x, y);
					RelicInstance inst = Board.GetCell(pos);
					bool isAnchor = inst != null && inst.AnchorPosition == pos;
					_cells[x, y].Refresh(inst, isAnchor);
				}
			}
		}

		/// <summary>
		/// Returns the cell at the given coordinates.
		/// </summary>
		/// <param name="x">Grid X coordinate.</param>
		/// <param name="y">Grid Y coordinate.</param>
		/// <returns>The matching cell, or null.</returns>
		public RelicCellUI GetCell(int x, int y)
		{
			if (_cells == null || !Board.IsInBounds(x, y)) return null;
			return _cells[x, y];
		}

		/// <summary>
		/// Returns the cell at the given grid position.
		/// </summary>
		/// <param name="pos">Grid position.</param>
		/// <returns>The matching cell, or null.</returns>
		public RelicCellUI GetCell(Vector2Int pos)
		{
			return GetCell(pos.x, pos.y);
		}

		/// <summary>
		/// Shows preview state for a potential relic placement.
		/// </summary>
		/// <param name="shape">Relic shape being previewed.</param>
		/// <param name="anchor">Preview anchor position.</param>
		/// <param name="isValid">Whether the placement is valid.</param>
		public void ShowPlacementPreview(RelicShape shape, Vector2Int anchor, bool isValid)
		{
			ClearPlacementPreview();
			if (shape == null || !shape.IsValid) return;

			foreach (Vector2Int offset in shape.Cells)
			{
				RelicCellUI cell = GetCell(anchor + offset);
				cell?.SetPreview(isValid);
			}
		}

		/// <summary>
		/// Clears all placement preview highlights.
		/// </summary>
		public void ClearPlacementPreview()
		{
			if (_cells == null) return;
			for (int y = 0; y < Board.Rows; y++)
			{
				for (int x = 0; x < Board.Columns; x++)
				{
					_cells[x, y].ClearPreview();
				}
			}
		}
	}
}
