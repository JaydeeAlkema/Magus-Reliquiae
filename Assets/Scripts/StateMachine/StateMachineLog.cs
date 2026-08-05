using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace StateMachine
{
	/// <summary>
	///     Conditional logger for the state machine.
	/// </summary>
	/// <remarks>
	///     Use this for editor and development diagnostics instead of unconditional <see cref="Debug.Log" /> calls.
	/// </remarks>
	internal static class StateMachineLog
	{
		/// <summary>
		///     Logs a state-machine message in editor and development builds.
		/// </summary>
		/// <param name="message">Message to log.</param>
		[Conditional("UNITY_EDITOR")]
		[Conditional("DEVELOPMENT_BUILD")]
		public static void Log(string message)
		{
			Debug.Log(message);
		}

		/// <summary>
		///     Logs a state-machine error in editor and development builds.
		/// </summary>
		/// <param name="message">Message to log.</param>
		[Conditional("UNITY_EDITOR")]
		[Conditional("DEVELOPMENT_BUILD")]
		public static void LogError(string message)
		{
			Debug.LogError(message);
		}
	}
}
