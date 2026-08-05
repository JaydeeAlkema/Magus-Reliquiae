using Relic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	/// <summary>
	///     Draggable relic entry shown inside the bag UI.
	/// </summary>
	/// <remarks>
	///     Use this on the bag item prefab and wire icon, rarity border, and level text references.
	/// </remarks>
	[RequireComponent(typeof(Image))]
	public class RelicBagItemUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
	{
		/// <summary>
		///     Display icon for the relic.
		/// </summary>
		[SerializeField] private Image IconImage;
		/// <summary>
		///     Border tinted by rarity.
		/// </summary>
		[SerializeField] private Image RarityBorderImage;
		/// <summary>
		///     Optional level label.
		/// </summary>
		[SerializeField] private TMP_Text LevelText;

		[Header("Rarity Colors (index matches RelicRarity enum)")]
		/// <summary>
		/// Border colors indexed by rarity.
		/// </summary>
		[SerializeField] private Color[] RarityColors =
		{
			new(0.60f, 0.60f, 0.60f), // Common
			new(0.20f, 0.45f, 1.00f), // Rare
			new(0.60f, 0.10f, 0.90f), // Epic
			new(1.00f, 0.60f, 0.10f), // Legendary
		};

		/// <summary>
		///     Bound relic instance.
		/// </summary>
		public RelicInstance Instance { get; private set; }

		/// <summary>
		///     Binds a relic instance to the view.
		/// </summary>
		/// <param name="instance">Relic instance to display.</param>
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


		/// <summary>
		///     Begins dragging the relic from the bag.
		/// </summary>
		/// <param name="eventData">Pointer event payload.</param>
		public void OnBeginDrag(PointerEventData eventData)
		{
			RelicDragHandler.Instance?.StartDragFromBag(Instance, eventData);
		}

		/// <summary>
		///     Updates the drag interaction.
		/// </summary>
		/// <param name="eventData">Pointer event payload.</param>
		public void OnDrag(PointerEventData eventData)
		{
			RelicDragHandler.Instance?.UpdateDrag(eventData);
		}

		/// <summary>
		///     Ends the drag interaction.
		/// </summary>
		/// <param name="eventData">Pointer event payload.</param>
		public void OnEndDrag(PointerEventData eventData)
		{
			RelicDragHandler.Instance?.EndDrag(eventData);
		}
	}
}
