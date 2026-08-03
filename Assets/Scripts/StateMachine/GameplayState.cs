using System.Collections.Generic;
using Game;
using Relic;
using UnityEngine;

namespace StateMachine
{
	public class GameplayState : State
	{
		private const int OFFER_COUNT = 3;

		private readonly GameContext _context;
		private Player.Player _player;
		private bool _levelUpPending;

		public override bool IsDone { get; protected set; }
		public override State NextState { get; protected set; }

		public GameplayState(GameContext context)
		{
			_context = context;
		}

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

			if (!_player)
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

		public override void OnExit()
		{
			if (_player)
				_player.XP.onLevelUp -= OnLevelUp;

			StateMachineLog.Log("Exiting Gameplay State");
		}

		public override void Update()
		{
			if (!_levelUpPending || !_player) return;

			_levelUpPending = false;

			RelicOfferGenerator generator = new(_context.RelicCatalogue, _context.RarityWeights);
			List<RelicSO> offers = generator.GenerateOffers(OFFER_COUNT);

			NextState = new UpgradeScreenState(_context, offers);
			IsDone = true;
		}

		private void OnLevelUp(int newLevel)
		{
			// Defer transition to Update so it happens at a safe point in the frame
			_levelUpPending = true;
		}
	}
}
