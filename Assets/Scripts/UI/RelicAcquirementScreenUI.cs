using System;
using System.Collections.Generic;
using Relic;
using StateMachine;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	/// <summary>
	/// Relic reward selection screen.
	/// </summary>
	/// <remarks>
	/// Put this on the acquirement screen prefab and wire the card array and manage-board button.
	/// </remarks>
	public class RelicAcquirementScreenUI : MonoBehaviour
	{
		/// <summary>
		/// Card views used to present relic offers.
		/// </summary>
		[SerializeField] private RelicCardUI[] Cards;
		/// <summary>
		/// Optional button that opens the relic board.
		/// </summary>
		[SerializeField] private Button ManageBoardButton;

		private UpgradeScreenState _upgradeState;
		private Action _onManageBoardTapped;

		private void Awake()
		{
			this.gameObject.SetActive(false);

			if (Cards == null || Cards.Length == 0)
				Cards = GetComponentsInChildren<RelicCardUI>(true);

			if (ManageBoardButton == null)
				ManageBoardButton = GetComponentInChildren<Button>(true);

			if (ManageBoardButton == null)
				return;
			
			ManageBoardButton.gameObject.SetActive(false);
			ManageBoardButton.onClick.AddListener(OnManageBoardTapped);
		}

		/// <summary>
		/// Stores the active upgrade state.
		/// </summary>
		/// <param name="state">Upgrade state to connect.</param>
		public void SetUpgradeState(UpgradeScreenState state)
		{
			_upgradeState = state;
		}

		/// <summary>
		/// Shows relic offers in the card slots.
		/// </summary>
		/// <param name="offers">Relics to present.</param>
		public void Show(IReadOnlyList<RelicSO> offers)
		{
			this.gameObject.SetActive(true);

			if (ManageBoardButton)
				ManageBoardButton.gameObject.SetActive(false);

			if (Cards == null || Cards.Length == 0)
				Cards = GetComponentsInChildren<RelicCardUI>(true);

			for (int i = 0; i < Cards.Length; i++)
			{
				if (i < offers.Count)
				{
					Cards[i].gameObject.SetActive(true);
					Cards[i].Bind(offers[i], OnCardSelected);
				}
				else
				{
					Cards[i].gameObject.SetActive(false);
				}
			}
		}

		/// <summary>
		/// Hides the offer screen.
		/// </summary>
		public void Hide()
		{
			this.gameObject.SetActive(false);
		}

		/// <summary>
		/// Shows the manage-board button and stores its callback.
		/// </summary>
		/// <param name="onTapped">Callback for the button.</param>
		public void ShowManageBoardButton(Action onTapped)
		{
			_onManageBoardTapped = onTapped;

			foreach (RelicCardUI card in Cards)
			{
				card.SetInteractable(false);
			}

			if (ManageBoardButton != null)
				ManageBoardButton.gameObject.SetActive(true);
		}

		private void OnCardSelected(RelicSO relic)
		{
			_upgradeState?.SelectOffer(relic);
		}

		private void OnManageBoardTapped()
		{
			_onManageBoardTapped?.Invoke();
		}
	}
}
