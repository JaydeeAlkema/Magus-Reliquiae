using System;
using Relic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class RelicCardUI : MonoBehaviour
	{
		[SerializeField] private Image IconImage;
		[SerializeField] private TMP_Text NameText;
		[SerializeField] private TMP_Text DescriptionText;
		[SerializeField] private Image RarityBorderImage;
		[SerializeField] private Button SelectButton;

		[Header("Rarity Colors")]
		[SerializeField] private Color CommonColor = new(0.65f, 0.65f, 0.65f);
		[SerializeField] private Color RareColor = new(0.2f, 0.45f, 1f);
		[SerializeField] private Color EpicColor = new(0.6f, 0.1f, 0.9f);
		[SerializeField] private Color LegendaryColor = new(1f, 0.6f, 0.1f);

		private Action<RelicSO> _onSelected;
		private RelicSO _boundRelic;

		private void Awake()
		{
			if (SelectButton != null)
				SelectButton.onClick.AddListener(OnButtonClicked);
		}

		public void Bind(RelicSO relic, Action<RelicSO> onSelected)
		{
			_boundRelic = relic;
			_onSelected = onSelected;

			if (IconImage)
			{
				IconImage.sprite = relic.Icon;
				IconImage.enabled = relic.Icon != null;
			}

			if (NameText)
				NameText.text = relic.DisplayName;

			if (DescriptionText)
			{
				RelicLevelData levelData = relic.GetLevelData(1);
				DescriptionText.text = levelData != null && !string.IsNullOrEmpty(levelData.Description)
					? levelData.Description
					: relic.Description;
			}

			if (RarityBorderImage)
				RarityBorderImage.color = GetRarityColor(relic.Rarity);

			SetInteractable(true);
		}

		public void SetInteractable(bool interactable)
		{
			if (SelectButton)
				SelectButton.interactable = interactable;
		}

		private void OnButtonClicked()
		{
			_onSelected?.Invoke(_boundRelic);
		}

		private Color GetRarityColor(RelicRarity rarity)
		{
			return rarity switch
			{
				RelicRarity.Common => CommonColor,
				RelicRarity.Rare => RareColor,
				RelicRarity.Epic => EpicColor,
				RelicRarity.Legendary => LegendaryColor,
				_ => CommonColor,
			};
		}
	}
}
