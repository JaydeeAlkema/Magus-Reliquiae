namespace Relic
{
	/// <summary>
	/// Runtime contract for relic behavior logic.
	/// </summary>
	/// <remarks>
	/// Implement this in code or return it from a <see cref="RelicBehaviorSO"/> asset.
	/// </remarks>
	public interface IRelicBehavior
	{
		/// <summary>
		/// Called when the relic is acquired.
		/// </summary>
		void OnAcquired(RelicInstance instance, RelicRuntimeContext context);
		/// <summary>
		/// Called when the relic is removed.
		/// </summary>
		void OnRemoved(RelicInstance instance, RelicRuntimeContext context);
		/// <summary>
		/// Called when the relic levels up.
		/// </summary>
		void OnLevelChanged(RelicInstance instance, int previousLevel, RelicRuntimeContext context);
		/// <summary>
		/// Called every tick while the relic is active.
		/// </summary>
		void OnTick(RelicInstance instance, float deltaTime, RelicRuntimeContext context);
		/// <summary>
		/// Called when a relic trigger is published.
		/// </summary>
		void OnTrigger(RelicInstance instance, RelicTrigger trigger, RelicRuntimeContext context);
	}
}
