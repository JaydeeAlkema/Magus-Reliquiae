namespace Relic
{
	public readonly struct RelicTrigger
	{
		public readonly RelicTriggerEvent EventType;
		public readonly int IntValue;
		public readonly float FloatValue;

		public RelicTrigger(RelicTriggerEvent eventType, int intValue = 0, float floatValue = 0f)
		{
			EventType = eventType;
			IntValue = intValue;
			FloatValue = floatValue;
		}
	}
}
