using System.Collections.Generic;
using Game;
using Relic;
using UnityEngine;

namespace StateMachine
{
	/// <summary>
	///     Primary in-game state.
	/// </summary>
	/// <remarks>
	///     It owns gameplay setup, updates, and the transition to upgrade screens.
	/// </remarks>
	public class GameplayState : State
	{
		private const int OFFER_COUNT = 3;

		private readonly GameContext _context;
		private Player.Player _player;
		private bool _levelUpPending;

		/// <summary>
		///     True when the state is finished.
		/// </summary>
		public override bool IsDone { get; protected set; }
		/// <summary>
		///     The next state after a level-up offer is ready.
		/// </summary>
		public override State NextState { get; protected set; }

		/// <summary>
		///     Creates the gameplay state.
		/// </summary>
		/// <param name="context">Shared game context.</param>
		public GameplayState(GameContext context)
		{
			_context = context;
		}

		/// <summary>
		///     Binds the player and enables gameplay-specific hooks.
		/// </summary>
		public override void OnEnter()
		{
			IsDone = false;
			NextState = null;
			_levelUpPending = false;

			_player = _context.Player;
			if (!_player)
			{
				_player = Object.FindAnyObjectByType<Player.Player>();
				_context.Player = _player;
			}

			if (_player)
			{
				_player.XP.onLevelUp += OnLevelUp;
				_player.Relics.IsInteractionLocked = true;
			}
			else
			{
				Debug.LogWarning("[GameplayState] No Player found in scene.");
			}

			StateMachineLog.Log("Entering Gameplay State");
		}

		/// <summary>
		///     Cleans up gameplay hooks.
		/// </summary>
		public override void OnExit()
		{
			if (_player)
				_player.XP.onLevelUp -= OnLevelUp;

			StateMachineLog.Log("Exiting Gameplay State");
		}

		/// <summary>
		///     Waits for a pending level-up transition.
		/// </summary>
		public override void Update()
		{
			if (!_levelUpPending || !_player) return;

			_levelUpPending = false;

			RelicOfferGenerator generator = new(_context.RelicCatalogue, _context.RarityWeights);
			List<RelicSO> offers = generator.GenerateOffers(OFFER_COUNT);

			NextState = new UpgradeScreenState(_context, offers);
			IsDone = true;
		}

		/// <summary>
		///     Defers the upgrade transition until the next safe update point.
		/// </summary>
		/// <param name="newLevel">New player level.</param>
		private void OnLevelUp(int newLevel)
		{
			// Defer transition to Update so it happens at a safe point in the frame
			_levelUpPending = true;
		}
	}
}
