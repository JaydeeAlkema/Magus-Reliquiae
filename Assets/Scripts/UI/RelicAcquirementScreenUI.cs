using System;
using System.Collections.Generic;
using Relic;
using StateMachine;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class RelicAcquirementScreenUI : MonoBehaviour
	{
		[SerializeField] private RelicCardUI[] Cards;
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

		public void SetUpgradeState(UpgradeScreenState state)
		{
			_upgradeState = state;
		}

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

		public void Hide()
		{
			this.gameObject.SetActive(false);
		}

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
