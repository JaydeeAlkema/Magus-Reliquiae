namespace Relic
{
	public interface IRelicBehavior
	{
		void OnAcquired(RelicInstance instance, RelicRuntimeContext context);
		void OnRemoved(RelicInstance instance, RelicRuntimeContext context);
		void OnLevelChanged(RelicInstance instance, int previousLevel, RelicRuntimeContext context);
		void OnTick(RelicInstance instance, float deltaTime, RelicRuntimeContext context);
		void OnTrigger(RelicInstance instance, RelicTrigger trigger, RelicRuntimeContext context);
	}
}
