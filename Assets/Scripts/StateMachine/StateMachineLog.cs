using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace StateMachine
{
	internal static class StateMachineLog
	{
		[Conditional("UNITY_EDITOR")]
		[Conditional("DEVELOPMENT_BUILD")]
		public static void Log(string message)
		{
			Debug.Log(message);
		}

		[Conditional("UNITY_EDITOR")]
		[Conditional("DEVELOPMENT_BUILD")]
		public static void LogError(string message)
		{
			Debug.LogError(message);
		}
	}
}
