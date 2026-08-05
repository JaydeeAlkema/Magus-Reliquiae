using Game;
using StateMachine;
using UnityEngine;

namespace UI
{
	/// <summary>
	///     Wires relic UI panels to the game and upgrade state.
	/// </summary>
	public class RelicUICoordinator : MonoBehaviour
	{
		/// <summary>
		///     Relic offer screen.
		/// </summary>
		[SerializeField] private RelicAcquirementScreenUI AcquirementScreen;
		/// <summary>
		///     Inventory overlay screen.
		/// </summary>
		[SerializeField] private InventoryOverlayUI InventoryOverlay;
		/// <summary>
		///     Drag handler singleton reference.
		/// </summary>
		[SerializeField] private RelicDragHandler DragHandler;

		private GameManager _gameManager;
		private UpgradeScreenState _currentUpgradeState;

		private void Start()
		{
			if (AcquirementScreen == null)
				AcquirementScreen = GetComponentInChildren<RelicAcquirementScreenUI>(true);

			if (InventoryOverlay == null)
				InventoryOverlay = GetComponentInChildren<InventoryOverlayUI>(true);

			if (DragHandler == null)
				DragHandler = GetComponentInChildren<RelicDragHandler>(true);

			_gameManager = FindAnyObjectByType<GameManager>();
			if (_gameManager == null)
			{
				Debug.LogError("[RelicUICoordinator] GameManager not found in scene.");
				return;
			}

			Player.Player player = FindAnyObjectByType<Player.Player>();
			if (player != null)
			{
				if (InventoryOverlay != null)
				{
					InventoryOverlay.BoardUI.Initialize(player.Relics.Board);
					InventoryOverlay.BagUI.Initialize(player.Relics.Bag);
				}

				if (DragHandler != null)
					DragHandler.Initialize(player.Relics, InventoryOverlay?.BoardUI, InventoryOverlay?.BagUI);
			}
			else
			{
				Debug.LogWarning("[RelicUICoordinator] Player not found — board/bag UI not initialized.");
			}

			_gameManager.StateManager.onStateChanged += OnStateChanged;

			if (_gameManager.StateManager.CurrentState is UpgradeScreenState upgradeState)
				ConnectUpgradeState(upgradeState);
		}

		private void OnDestroy()
		{
			if (_gameManager != null)
				_gameManager.StateManager.onStateChanged -= OnStateChanged;

			DisconnectUpgradeState();
		}

		private void OnStateChanged(State previous, State current)
		{
			DisconnectUpgradeState();

			if (current is UpgradeScreenState upgradeState)
				ConnectUpgradeState(upgradeState);
		}

		private void ConnectUpgradeState(UpgradeScreenState state)
		{
			_currentUpgradeState = state;
			AcquirementScreen.SetUpgradeState(state);

			state.onOffersReady += AcquirementScreen.Show;
			state.onOfferSelected += OnOfferSelected;
			state.onExiting += OnUpgradeExiting;
		}

		private void DisconnectUpgradeState()
		{
			if (_currentUpgradeState == null) return;

			_currentUpgradeState.onOffersReady -= AcquirementScreen.Show;
			_currentUpgradeState.onOfferSelected -= OnOfferSelected;
			_currentUpgradeState.onExiting -= OnUpgradeExiting;
			_currentUpgradeState = null;
		}

		private void OnOfferSelected()
		{
			UpgradeScreenState captured = _currentUpgradeState;
			AcquirementScreen.ShowManageBoardButton(() => { InventoryOverlay.Show(captured); });
		}

		private void OnUpgradeExiting()
		{
			AcquirementScreen.Hide();
			InventoryOverlay.Hide();
		}
	}
}
