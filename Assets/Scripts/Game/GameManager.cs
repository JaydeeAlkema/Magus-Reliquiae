using UnityEngine;

namespace Game
{
	public class GameManager : MonoBehaviour
	{
		private void Awake()
		{
			Application.targetFrameRate = (int)Screen.currentResolution.refreshRateRatio.value;
			Debug.Log($"Target frame rate set to {Application.targetFrameRate} FPS");
		}
	}
}
