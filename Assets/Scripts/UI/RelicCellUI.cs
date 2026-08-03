using Relic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	[RequireComponent(typeof(Image))]
	public class RelicCellUI : MonoBehaviour,
		IDropHandler,
		IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[Header("Child References")]
		[SerializeField] private Image BackgroundImage;
		[SerializeField] private Image RelicIconImage;
		[SerializeField] private GameObject LevelBadge;
		[SerializeField] private TMP_Text LevelText;

		[Header("Cell Colors")]
		[SerializeField] private Color EmptyCellColor = new(0.15f, 0.15f, 0.15f, 0.8f);
		[SerializeField] private Color FilledCellColor = new(0.3f, 0.3f, 0.3f, 0.9f);
		[SerializeField] private Color PreviewValidColor = new(0.2f, 0.8f, 0.2f, 0.55f);
		[SerializeField] private Color PreviewInvalidColor = new(0.8f, 0.2f, 0.2f, 0.55f);

		[Header("Rarity Colors (index matches RelicRarity enum)")]
		[SerializeField] private Color[] RarityColors =
		{
			new(0.60f, 0.60f, 0.60f), // Common
			new(0.20f, 0.45f, 1.00f), // Rare
			new(0.60f, 0.10f, 0.90f), // Epic
			new(1.00f, 0.60f, 0.10f), // Legendary
		};


		public Vector2Int GridPosition { get; private set; }

		private RelicBoardUI _boardUI;
		private RelicInstance _occupant;
		private bool _isAnchor;
		private bool _inPreview;
		private Color _previewColor;


		public void Initialize(Vector2Int gridPos, RelicBoardUI boardUI)
		{
			GridPosition = gridPos;
			_boardUI = boardUI;
			Refresh(null, false);
		}


		public void Refresh(RelicInstance occupant, bool isAnchor)
		{
			_occupant = occupant;
			_isAnchor = isAnchor;
			UpdateVisuals();
		}


		public void SetPreview(bool isValid)
		{
			_inPreview = true;
			_previewColor = isValid ? PreviewValidColor : PreviewInvalidColor;
			UpdateVisuals();
		}

		public void ClearPreview()
		{
			if (!_inPreview) return;
			_inPreview = false;
			UpdateVisuals();
		}

		private void UpdateVisuals()
		{
			if (_inPreview)
			{
				SetBackground(_previewColor);
				SetIcon(null, false);
				SetLevelBadge(false, 0);
				return;
			}

			if (_occupant == null)
			{
				SetBackground(EmptyCellColor);
				SetIcon(null, false);
				SetLevelBadge(false, 0);
				return;
			}

			if (_isAnchor)
			{
				SetBackground(GetRarityColor(_occupant.Definition.Rarity));
				SetIcon(_occupant.Definition.Icon, _occupant.Definition.Icon != null);
				SetLevelBadge(_occupant.Definition.MaxLevel > 1, _occupant.Level);
			}
			else
			{
				SetBackground(FilledCellColor);
				SetIcon(null, false);
				SetLevelBadge(false, 0);
			}
		}

		private void SetBackground(Color color)
		{
			if (BackgroundImage != null) BackgroundImage.color = color;
		}

		private void SetIcon(Sprite sprite, bool visible)
		{
			if (RelicIconImage == null) return;
			RelicIconImage.enabled = visible;
			if (visible) RelicIconImage.sprite = sprite;
		}

		private void SetLevelBadge(bool visible, int level)
		{
			if (LevelBadge != null) LevelBadge.SetActive(visible);
			if (LevelText != null && visible) LevelText.text = level.ToString();
		}

		private Color GetRarityColor(RelicRarity rarity)
		{
			int i = (int)rarity;
			return i >= 0 && i < RarityColors.Length ? RarityColors[i] : Color.grey;
		}

		public void OnDrop(PointerEventData eventData)
		{
			RelicDragHandler.Instance?.HandleBoardDrop(this);
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (_occupant == null || !_isAnchor) return;
			RelicDragHandler.Instance?.StartDragFromBoard(_occupant, eventData);
		}

		public void OnDrag(PointerEventData eventData)
		{
			RelicDragHandler.Instance?.UpdateDrag(eventData);
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			RelicDragHandler.Instance?.EndDrag(eventData);
		}
	}
}
