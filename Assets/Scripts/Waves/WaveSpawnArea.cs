using System;
using System.Collections.Generic;
using UnityEngine;

namespace Waves
{
	public class WaveSpawnArea : MonoBehaviour
	{
		[Header("Spawn Area")]
		[SerializeField] private int MinCells;
		[SerializeField] private Vector2Int CellSize;
		[SerializeField] private WaveSpawnAreaExpandDirection ExpandDirection;
		[SerializeField] private float SpawnOffset = 1f;
		[SerializeField] private UnityEngine.Camera TargetCamera;

		[Header("Debugging")]
		[SerializeField] private bool DrawGizmos;

		private Dictionary<(int, int), bool> _cells;
		private Vector2Int _spawnArea;
		private UnityEngine.Camera _cachedCamera;
		private float _cachedOrthographicSize = -1f;
		private float _cachedAspect = -1f;
		private int _cachedMinCells = -1;
		private Vector2Int _cachedCellSize = new(-1, -1);
		private WaveSpawnAreaExpandDirection _cachedExpandDirection;

		private void Awake()
		{
			_cells = new Dictionary<(int, int), bool>();
			RefreshArea(forceResize: true);
		}

		private void LateUpdate()
		{
			if (NeedsResize())
				Resize();

			Reposition();
		}

		private void OnValidate()
		{
			if (_cells == null)
				_cells = new Dictionary<(int, int), bool>();

			RefreshArea(forceResize: true);
		}

		private void RefreshArea(bool forceResize)
		{
			if (forceResize)
				Resize(force: true);

			Reposition();
		}

		private bool NeedsResize()
		{
			if (_cachedCamera != TargetCamera)
				return true;

			if (_cachedMinCells != MinCells || _cachedCellSize != CellSize || _cachedExpandDirection != ExpandDirection)
				return true;

			if (!TargetCamera || !TargetCamera.orthographic)
				return false;

			return !Mathf.Approximately(_cachedOrthographicSize, TargetCamera.orthographicSize) ||
			       !Mathf.Approximately(_cachedAspect, TargetCamera.aspect);
		}

		private void Reposition()
		{
			if (!TargetCamera)
				return;

			if (!TargetCamera.orthographic)
				return;

			float halfViewHeight = TargetCamera.orthographicSize;
			float halfViewWidth = halfViewHeight * TargetCamera.aspect;

			Vector3 cameraPosition = TargetCamera.transform.position;
			Vector3 currentPosition = this.transform.position;

			float halfCellWidth = Mathf.Max(1, CellSize.x) * 0.5f;
			float halfCellHeight = Mathf.Max(1, CellSize.y) * 0.5f;
			int cols = Mathf.Max(1, _spawnArea.x / Mathf.Max(1, CellSize.x));
			int rows = Mathf.Max(1, _spawnArea.y / Mathf.Max(1, CellSize.y));
			float orthogonalOffsetX = -((cols - 1) * Mathf.Max(1, CellSize.x)) * 0.5f;
			float orthogonalOffsetY = -((rows - 1) * Mathf.Max(1, CellSize.y)) * 0.5f;

			float cameraPositionX = cameraPosition.x;
			float cameraPositionY = cameraPosition.y;
			float currentPositionZ = currentPosition.z;
			this.transform.position = ExpandDirection switch
			{
				WaveSpawnAreaExpandDirection.Up => new Vector3(cameraPositionX + orthogonalOffsetX, cameraPositionY + halfViewHeight + halfCellHeight + SpawnOffset, currentPositionZ),
				WaveSpawnAreaExpandDirection.Down => new Vector3(cameraPositionX + orthogonalOffsetX, cameraPositionY - halfViewHeight - halfCellHeight - SpawnOffset, currentPositionZ),
				WaveSpawnAreaExpandDirection.Left => new Vector3(cameraPositionX - halfViewWidth - halfCellWidth - SpawnOffset, cameraPositionY + orthogonalOffsetY, currentPositionZ),
				_ => new Vector3(cameraPositionX + halfViewWidth + halfCellWidth + SpawnOffset, cameraPositionY + orthogonalOffsetY, currentPositionZ),
			};
		}

		private void Resize(bool force = false)
		{
			int cellWidth = Mathf.Max(1, CellSize.x);
			int cellHeight = Mathf.Max(1, CellSize.y);
			int minCells = Mathf.Max(1, MinCells);

			float fullViewWidth = 0f;
			float fullViewHeight = 0f;

			if (TargetCamera && TargetCamera.orthographic)
			{
				fullViewHeight = TargetCamera.orthographicSize * 2f;
				fullViewWidth = fullViewHeight * TargetCamera.aspect;
			}

			bool expandVertically = ExpandDirection is WaveSpawnAreaExpandDirection.Up or WaveSpawnAreaExpandDirection.Down;

			int floorToIntWidth = Mathf.FloorToInt(fullViewWidth / cellWidth);
			int floorToIntHeight = Mathf.FloorToInt(fullViewHeight / cellHeight);

			int cols = expandVertically ? Mathf.Max(1, floorToIntWidth) : minCells;
			int rows = expandVertically ? minCells : Mathf.Max(1, floorToIntHeight);

			Vector2Int newSpawnArea = new(cols * cellWidth, rows * cellHeight);
			if (!force && _spawnArea == newSpawnArea)
				return;

			_spawnArea = newSpawnArea;
			_cachedCamera = TargetCamera;
			_cachedOrthographicSize = TargetCamera && TargetCamera.orthographic ? TargetCamera.orthographicSize : -1f;
			_cachedAspect = TargetCamera && TargetCamera.orthographic ? TargetCamera.aspect : -1f;
			_cachedMinCells = MinCells;
			_cachedCellSize = CellSize;
			_cachedExpandDirection = ExpandDirection;
			PopulateArea();
		}

		private void PopulateArea()
		{
			int cols = Mathf.Max(1, _spawnArea.x / Mathf.Max(1, CellSize.x));
			int rows = Mathf.Max(1, _spawnArea.y / Mathf.Max(1, CellSize.y));

			_cells.Clear();

			for (int y = 0; y < rows; y++)
			{
				for (int x = 0; x < cols; x++)
				{
					int cellX = x;
					int cellY = y;

					switch (ExpandDirection)
					{
						case WaveSpawnAreaExpandDirection.Down:
							cellY = -y;
							break;

						case WaveSpawnAreaExpandDirection.Left:
							cellX = -x;
							break;

						case WaveSpawnAreaExpandDirection.Up:
						case WaveSpawnAreaExpandDirection.Right:
							break;

						default:
							throw new ArgumentOutOfRangeException();
					}

					_cells[(cellX, cellY)] = false;
				}
			}
		}

		private void OnDrawGizmos()
		{
			if (!DrawGizmos)
				return;

			Gizmos.color = Color.yellow;
			Gizmos.DrawWireCube(this.transform.position, new Vector3(CellSize.x, CellSize.y, 0));

			if (_cells == null || _cells.Count == 0)
				return;

			Gizmos.color = Color.blue;
			foreach (KeyValuePair<(int, int), bool> cell in _cells)
			{
				Vector2Int cellPosition = new(cell.Key.Item1 * CellSize.x, cell.Key.Item2 * CellSize.y);
				Gizmos.DrawWireCube(this.transform.position + new Vector3(cellPosition.x, cellPosition.y, 0), new Vector3(CellSize.x, CellSize.y, 0));
			}

		}
	}
}
