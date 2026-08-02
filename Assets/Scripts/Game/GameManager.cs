using StateMachine;
using UnityEngine;

namespace Game
{
	public class GameManager : MonoBehaviour
	{
		private GameStateManager _gameStateManager;

		private void Awake()
		{
			DontDestroyOnLoad(this.gameObject);

			int refreshRate = Mathf.RoundToInt((float)Screen.currentResolution.refreshRateRatio.value);
			if (refreshRate <= 0)
			{
				refreshRate = 60;
			}

			Application.targetFrameRate = Application.isMobilePlatform
				? Mathf.Min(refreshRate, 60)
				: refreshRate;

			_gameStateManager = new GameStateManager(new StartGameState());
		}

		private void Update()
		{
			_gameStateManager.Update();
		}
	}
}
