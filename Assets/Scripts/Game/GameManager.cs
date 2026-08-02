using StateMachine;
using UnityEngine;

namespace Game
{
	public class GameManager : MonoBehaviour
	{
		private GameState _gameState;

		private void Awake()
		{
			_gameState = new GameState(new MainMenuState());

			Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
		}

		private void Update()
		{
			_gameState.Update();
		}
	}
}
