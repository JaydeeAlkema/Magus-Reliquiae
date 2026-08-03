using StateMachine;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class InventoryOverlayUI : MonoBehaviour
	{
		[SerializeField] private RelicBoardUI _boardUI;
		[SerializeField] private BagOfHoldingUI _bagUI;
		[SerializeField] private Button CloseButton;

		private UpgradeScreenState _upgradeState;

		public RelicBoardUI BoardUI => _boardUI;
		public BagOfHoldingUI BagUI => _bagUI;

		private void Awake()
		{
			this.gameObject.SetActive(false);

			if (_boardUI == null)
				_boardUI = GetComponentInChildren<RelicBoardUI>(true);

			if (_bagUI == null)
				_bagUI = GetComponentInChildren<BagOfHoldingUI>(true);

			if (CloseButton == null)
				CloseButton = GetComponentInChildren<Button>(true);

			if (CloseButton != null)
				CloseButton.onClick.AddListener(OnCloseClicked);
		}

		public void Show(UpgradeScreenState upgradeState)
		{
			_upgradeState = upgradeState;
			this.gameObject.SetActive(true);
		}

		public void Hide()
		{
			this.gameObject.SetActive(false);
			_upgradeState = null;
		}

		private void OnCloseClicked()
		{
			_upgradeState?.Dismiss();
		}
	}
}
