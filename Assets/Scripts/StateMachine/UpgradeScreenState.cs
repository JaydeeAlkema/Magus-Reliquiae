using System;
using System.Collections.Generic;
using Game;
using Relic;
using UnityEngine;

namespace StateMachine
{
	/// <summary>
	/// Upgrade-screen state.
	/// </summary>
	/// <remarks>
	/// It owns offer generation, selection, and exit flow. Enter it through the state machine, not through scene wiring.
	/// </remarks>
	public class UpgradeScreenState : State
	{
		private readonly GameContext _context;
		private bool _offerSelected;

		/// <summary>
		/// True when the state is finished.
		/// </summary>
		public override bool IsDone { get; protected set; }
		/// <summary>
		/// The next state after the upgrade screen exits.
		/// </summary>
		public override State NextState { get; protected set; }

		/// <summary>
		/// Offer list presented to the player.
		/// </summary>
		public IReadOnlyList<RelicSO> Offers { get; }

		/// <summary>
		/// Fired when offers are ready to show.
		/// </summary>
		public event Action<IReadOnlyList<RelicSO>> onOffersReady;
		/// <summary>
		/// Fired after an offer is selected.
		/// </summary>
		public event Action onOfferSelected;
		/// <summary>
		/// Fired while the state is exiting.
		/// </summary>
		public event Action onExiting;

		/// <summary>
		/// Creates the upgrade-screen state.
		/// </summary>
		/// <param name="context">Shared game context.</param>
		/// <param name="offers">Generated relic offers.</param>
		public UpgradeScreenState(GameContext context, IReadOnlyList<RelicSO> offers)
		{
			_context = context;
			Offers = offers ?? Array.Empty<RelicSO>();
		}

		/// <summary>
		/// Prepares the upgrade screen and pauses gameplay.
		/// </summary>
		public override void OnEnter()
		{
			IsDone = false;
			NextState = null;
			_offerSelected = false;

			Time.timeScale = 0f;

			if (_context.Player != null)
				_context.Player.Relics.IsInteractionLocked = false;

			onOffersReady?.Invoke(Offers);

			StateMachineLog.Log($"Entering Upgrade Screen State — {Offers.Count} offer(s) ready.");
		}

		/// <summary>
		/// Restores gameplay time and interaction lock state.
		/// </summary>
		public override void OnExit()
		{
			Time.timeScale = 1f;

			if (_context.Player != null)
				_context.Player.Relics.IsInteractionLocked = true;

			onExiting?.Invoke();
			StateMachineLog.Log("Exiting Upgrade Screen State.");
		}

		/// <summary>
		/// No per-frame work is required.
		/// </summary>
		public override void Update() { }

		/// <summary>
		/// Applies the selected relic and requests exit.
		/// </summary>
		/// <param name="selected">Selected relic.</param>
		public void SelectOffer(RelicSO selected)
		{
			if (_offerSelected || selected == null) return;
			if (_context.Player == null)
			{
				Debug.LogWarning("[UpgradeScreenState] SelectOffer called but Player is null.");
				return;
			}

			_offerSelected = true;
			_context.Player.Relics.AcquireToBag(selected);
			onOfferSelected?.Invoke();
		}

		/// <summary>
		/// Dismisses the upgrade screen without selecting an offer.
		/// </summary>
		public void Dismiss()
		{
			NextState = new GameplayState(_context);
			IsDone = true;
		}
	}
}
