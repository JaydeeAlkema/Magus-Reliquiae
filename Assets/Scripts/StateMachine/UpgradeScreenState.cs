using System;
using System.Collections.Generic;
using Game;
using Relic;
using UnityEngine;

namespace StateMachine
{
	public class UpgradeScreenState : State
	{
		private readonly GameContext _context;
		private bool _offerSelected;

		public override bool IsDone { get; protected set; }
		public override State NextState { get; protected set; }

		public IReadOnlyList<RelicSO> Offers { get; }

		public event Action<IReadOnlyList<RelicSO>> onOffersReady;
		public event Action onOfferSelected;
		public event Action onExiting;

		public UpgradeScreenState(GameContext context, IReadOnlyList<RelicSO> offers)
		{
			_context = context;
			Offers = offers ?? Array.Empty<RelicSO>();
		}

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

		public override void OnExit()
		{
			Time.timeScale = 1f;

			if (_context.Player != null)
				_context.Player.Relics.IsInteractionLocked = true;

			onExiting?.Invoke();
			StateMachineLog.Log("Exiting Upgrade Screen State.");
		}

		public override void Update() { }

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

		public void Dismiss()
		{
			NextState = new GameplayState(_context);
			IsDone = true;
		}
	}
}
