using System.Collections.Generic;
using Relic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	/// <summary>
	///     Drag-and-drop coordinator for relic UI.
	/// </summary>
	/// <remarks>
	///     Add it to the UI root, assign the canvas and ghost prefab, then initialize it with
	///     <see cref="PlayerRelicManager" />.
	/// </remarks>
	public class RelicDragHandler : MonoBehaviour
	{
		/// <summary>
		///     Singleton instance used by the bag and board cells.
		/// </summary>
		public static RelicDragHandler Instance { get; private set; }

		/// <summary>
		///     Canvas used to spawn the drag ghost.
		/// </summary>
		[SerializeField] private Canvas RootCanvas;
		/// <summary>
		///     Prefab used for the drag ghost.
		/// </summary>
		[SerializeField] private RectTransform GhostPrefab;
		/// <summary>
		///     Ghost size in UI pixels.
		/// </summary>
		[SerializeField] private Vector2 GhostSize = new(80f, 80f);

		private PlayerRelicManager _relicManager;
		private RelicBoardUI _boardUI;

		private RelicInstance _draggingInstance;
		private bool _draggingFromBoard;

		private RectTransform _ghost;
		private GraphicRaycaster _raycaster;
		private Vector2Int? _lastPreviewAnchor;

		private void Awake()
		{
			if (Instance != null && Instance != this)
			{
				Destroy(this.gameObject);
				return;
			}

			Instance = this;
		}

		private void OnDestroy()
		{
			if (Instance == this) Instance = null;
		}

		/// <summary>
		///     Initializes the drag handler with runtime references.
		/// </summary>
		/// <param name="relicManager">Relic manager for board operations.</param>
		/// <param name="boardUI">Board view.</param>
		/// <param name="bagUI">Bag view.</param>
		public void Initialize(PlayerRelicManager relicManager, RelicBoardUI boardUI, BagOfHoldingUI bagUI)
		{
			_relicManager = relicManager;
			_boardUI = boardUI;
			_raycaster = RootCanvas != null
				? RootCanvas.GetComponent<GraphicRaycaster>()
				: FindAnyObjectByType<GraphicRaycaster>();
		}

		/// <summary>
		///     Starts dragging from the bag view.
		/// </summary>
		/// <param name="instance">Dragged relic instance.</param>
		/// <param name="eventData">Pointer event payload.</param>
		public void StartDragFromBag(RelicInstance instance, PointerEventData eventData)
		{
			if (instance == null || !CanInteract()) return;

			_draggingInstance = instance;
			_draggingFromBoard = false;
			CreateGhost(instance.Definition.Icon, eventData.position);
		}

		/// <summary>
		///     Starts dragging from the board view.
		/// </summary>
		/// <param name="instance">Dragged relic instance.</param>
		/// <param name="eventData">Pointer event payload.</param>
		public void StartDragFromBoard(RelicInstance instance, PointerEventData eventData)
		{
			if (instance == null || !CanInteract()) return;

			_draggingInstance = instance;
			_draggingFromBoard = true;
			CreateGhost(instance.Definition.Icon, eventData.position);
		}

		/// <summary>
		///     Updates ghost position and board preview.
		/// </summary>
		/// <param name="eventData">Pointer event payload.</param>
		public void UpdateDrag(PointerEventData eventData)
		{
			if (_ghost == null || _draggingInstance == null) return;

			MoveGhostToPointer(eventData.position);
			UpdateBoardPreview(eventData);
		}

		/// <summary>
		///     Ends the current drag interaction.
		/// </summary>
		/// <param name="eventData">Pointer event payload.</param>
		public void EndDrag(PointerEventData eventData)
		{
			_boardUI?.ClearPlacementPreview();
			_lastPreviewAnchor = null;

			DestroyGhost();
			_draggingInstance = null;
		}

		/// <summary>
		///     Handles a drop onto a board cell.
		/// </summary>
		/// <param name="targetCell">Cell under the pointer.</param>
		public void HandleBoardDrop(RelicCellUI targetCell)
		{
			if (_draggingInstance == null || _relicManager == null) return;

			Vector2Int anchor = targetCell.GridPosition;

			if (_draggingFromBoard)
			{
				return;
			}

			RelicInstance occupant = _relicManager.Board.GetCell(anchor);

			if (occupant == null)
			{
				bool placed = _relicManager.PlaceOnBoard(_draggingInstance, anchor);
				if (!placed)
				{
					Debug.Log("[RelicDragHandler] PlaceOnBoard failed — shape may not fit at this anchor.");
				}
			}
			else if (occupant.AnchorPosition == anchor &&
			         occupant.Definition == _draggingInstance.Definition)
			{
				MergeResult result = _relicManager.TryMergeOnBoard(_draggingInstance, occupant);
				LogMergeResult(result);
			}
		}

		/// <summary>
		///     Handles a drop back into the bag.
		/// </summary>
		public void HandleBagDrop()
		{
			if (_draggingInstance == null || !_draggingFromBoard || _relicManager == null) return;

			_relicManager.RemoveFromBoard(_draggingInstance);
		}

		private void UpdateBoardPreview(PointerEventData eventData)
		{
			if (_boardUI == null) return;

			RelicCellUI cellUnder = FindCellUnderPointer(eventData);
			if (cellUnder == null)
			{
				if (!_lastPreviewAnchor.HasValue)
					return;

				_boardUI.ClearPlacementPreview();
				_lastPreviewAnchor = null;

				return;
			}

			Vector2Int anchor = cellUnder.GridPosition;
			if (_lastPreviewAnchor == anchor) return;

			_lastPreviewAnchor = anchor;
			RelicShape shape = _draggingInstance.Definition.GetShape(_draggingInstance.Level);

			bool isValid = DeterminePreviewValidity(anchor, shape);
			_boardUI.ShowPlacementPreview(shape, anchor, isValid);
		}

		private bool DeterminePreviewValidity(Vector2Int anchor, RelicShape shape)
		{
			if (_relicManager == null) return false;

			RelicInstance occupant = _relicManager.Board.GetCell(anchor);

			if (occupant != null &&
			    occupant.AnchorPosition == anchor &&
			    occupant.Definition == _draggingInstance.Definition &&
			    !occupant.IsMaxLevel)
				return true;

			// Valid placement: all shape cells empty and in bounds
			return occupant == null && _relicManager.Board.CanPlace(shape, anchor);

		}

		private void CreateGhost(Sprite sprite, Vector2 screenPos)
		{
			if (GhostPrefab == null || RootCanvas == null) return;

			_ghost = Instantiate(GhostPrefab, RootCanvas.transform);
			_ghost.SetAsLastSibling();
			_ghost.sizeDelta = GhostSize;

			Image ghostImg = _ghost.GetComponent<Image>();
			if (ghostImg != null)
			{
				ghostImg.sprite = sprite;
				ghostImg.enabled = sprite != null;
				ghostImg.raycastTarget = false;
			}

			MoveGhostToPointer(screenPos);
		}

		private void MoveGhostToPointer(Vector2 screenPos)
		{
			if (_ghost == null || RootCanvas == null) return;

			UnityEngine.Camera cam = RootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : RootCanvas.worldCamera;
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
				    RootCanvas.transform as RectTransform,
				    screenPos, cam,
				    out Vector2 localPoint))
			{
				_ghost.localPosition = localPoint;
			}
		}

		private void DestroyGhost()
		{
			if (_ghost == null) return;
			Destroy(_ghost.gameObject);
			_ghost = null;
		}

		private RelicCellUI FindCellUnderPointer(PointerEventData eventData)
		{
			if (_raycaster == null) return null;

			List<RaycastResult> results = new();
			_raycaster.Raycast(eventData, results);

			foreach (RaycastResult hit in results)
			{
				RelicCellUI cell = hit.gameObject.GetComponent<RelicCellUI>();
				if (cell != null) return cell;
			}

			return null;
		}

		private bool CanInteract()
		{
			return _relicManager != null && !_relicManager.IsInteractionLocked;
		}

		private static void LogMergeResult(MergeResult result)
		{
			switch (result)
			{
				case MergeResult.SuccessButShapeConflict:
					Debug.Log("[RelicDragHandler] Merge succeeded but upgraded shape didn't fit — relic moved to bag.");
					break;
				case MergeResult.AlreadyMaxLevel:
					Debug.Log("[RelicDragHandler] Cannot merge: relic is already at max level.");
					break;
				case MergeResult.Success:
					break;
				default:
					Debug.LogWarning($"[RelicDragHandler] Unexpected merge result: {result}");
					break;
			}
		}
	}
}
