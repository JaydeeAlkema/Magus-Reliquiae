using UnityEngine.SceneManagement;

namespace StateMachine
{
	public class MainMenuState : State
	{
		public override bool IsDone { get; protected set; }
		public override State NextState { get; protected set; } = new GameplayState();

		public override void OnEnter()
		{
			int mainMenuSceneBuildIndex = SceneManager.GetSceneByName("MainMenu").buildIndex;
			SceneManager.LoadSceneAsync(mainMenuSceneBuildIndex);
		}

		public override void OnExit() { }

		public override void Update() { }
	}
}
