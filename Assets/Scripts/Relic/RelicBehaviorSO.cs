using UnityEngine;

namespace Relic
{
	/// <summary>
	/// Base ScriptableObject for relic behaviors.
	/// </summary>
	/// <remarks>
	/// Derive from it, then create assets that can build runtime <see cref="IRelicBehavior"/> instances.
	/// </remarks>
	public abstract class RelicBehaviorSO : ScriptableObject
	{
		/// <summary>
		/// Creates the runtime relic behavior.
		/// </summary>
		/// <returns>A behavior instance.</returns>
		public abstract IRelicBehavior CreateBehavior();
	}
}
