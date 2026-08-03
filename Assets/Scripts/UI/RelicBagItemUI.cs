using Relic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	[RequireComponent(typeof(Image))]
	public class RelicBagItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		[SerializeField] private Image IconImage;
		[SerializeField] private Image RarityBorderImage;
		[SerializeField] private TMP_Text LevelText;

		[Header("Rarity Colors (index matches RelicRarity enum)")]
		[SerializeField] private Color[] RarityColors =
		{
			new(0.60f, 0.60f, 0.60f), // Common
			new(0.20f, 0.45f, 1.00f), // Rare
			new(0.60f, 0.10f, 0.90f), // Epic
			new(1.00f, 0.60f, 0.10f), // Legendary
		};

		public RelicInstance Instance { get; private set; }

		public void Bind(RelicInstance instance)
		{
			Instance = instance;

			if (IconImage != null)
			{
				IconImage.sprite = instance.Definition.Icon;
				IconImage.enabled = instance.Definition.Icon != null;
			}

			if (RarityBorderImage != null)
			{
				int i = (int)instance.Definition.Rarity;
				RarityBorderImage.color = i >= 0 && i < RarityColors.Length ? RarityColors[i] : Color.grey;
			}

			if (LevelText != null)
				LevelText.text = instance.Level > 1 ? $"Lv{instance.Level}" : string.Empty;
		}


		public void OnBeginDrag(PointerEventData eventData)
		{
			RelicDragHandler.Instance?.StartDragFromBag(Instance, eventData);
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
