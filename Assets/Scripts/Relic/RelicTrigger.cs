namespace Relic
{
	/// <summary>
	///     Payload for relic trigger events.
	/// </summary>
	public readonly struct RelicTrigger
	{
		/// <summary>
		///     Event type.
		/// </summary>
		public readonly RelicTriggerEvent EventType;
		/// <summary>
		///     Integer payload.
		/// </summary>
		public readonly int IntValue;
		/// <summary>
		///     Float payload.
		/// </summary>
		public readonly float FloatValue;

		/// <summary>
		///     Creates a trigger payload.
		/// </summary>
		/// <param name="eventType">Trigger type.</param>
		/// <param name="intValue">Integer payload.</param>
		/// <param name="floatValue">Float payload.</param>
		public RelicTrigger(RelicTriggerEvent eventType, int intValue = 0, float floatValue = 0f)
		{
			EventType = eventType;
			IntValue = intValue;
			FloatValue = floatValue;
		}
	}
}
