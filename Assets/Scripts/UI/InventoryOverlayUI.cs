using StateMachine;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	/// <summary>
	///     Overlay that combines the relic board and bag views.
	/// </summary>
	/// <remarks>
	///     Put this on the inventory overlay prefab and wire the board, bag, and close button.
	/// </remarks>
	public class InventoryOverlayUI : MonoBehaviour
	{
		/// <summary>
		///     Board view shown in the overlay.
		/// </summary>
		[SerializeField] private RelicBoardUI _boardUI;
		/// <summary>
		///     Bag view shown in the overlay.
		/// </summary>
		[SerializeField] private BagOfHoldingUI _bagUI;
		/// <summary>
		///     Button used to close the overlay.
		/// </summary>
		[SerializeField] private Button CloseButton;

		private UpgradeScreenState _upgradeState;

		/// <summary>
		///     Board UI reference.
		/// </summary>
		public RelicBoardUI BoardUI => _boardUI;
		/// <summary>
		///     Bag UI reference.
		/// </summary>
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

		/// <summary>
		///     Shows the overlay and binds the current upgrade state.
		/// </summary>
		/// <param name="upgradeState">Active upgrade state.</param>
		public void Show(UpgradeScreenState upgradeState)
		{
			_upgradeState = upgradeState;
			this.gameObject.SetActive(true);
		}

		/// <summary>
		///     Hides the overlay and clears the active state.
		/// </summary>
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
