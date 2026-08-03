using System;
using Game;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace StateMachine
{
	public class StartGameState : State
	{
		private const int MAIN_MENU_SCENE_BUILD_INDEX = 1;

		private readonly GameContext _context;

		public override bool IsDone { get; protected set; }
		public override State NextState { get; protected set; }

		private AsyncOperation _loadOperation;

		public StartGameState(GameContext context)
		{
			_context = context;
		}

		public override void OnEnter()
		{
			IsDone = false;
			NextState = null;

			_loadOperation = SceneManager.LoadSceneAsync(MAIN_MENU_SCENE_BUILD_INDEX);
			if (_loadOperation == null)
			{
				throw new InvalidOperationException($"Failed to load main menu scene at build index {MAIN_MENU_SCENE_BUILD_INDEX}.");
			}

			_loadOperation.completed += OnMainMenuSceneLoaded;
		}

		public override void OnExit()
		{
			if (_loadOperation == null)
				return;

			_loadOperation.completed -= OnMainMenuSceneLoaded;
			_loadOperation = null;
		}

		public override void Update() { }

		private void OnMainMenuSceneLoaded(AsyncOperation _)
		{
			_loadOperation.completed -= OnMainMenuSceneLoaded;
			_loadOperation = null;

			NextState = new MainMenuState(_context);
			IsDone = true;
		}
	}
}
