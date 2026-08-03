using System;

namespace Relic
{
	public sealed class RelicInstance
	{
		public RelicSO Definition { get; }
		public int Level { get; private set; }
		public bool IsMaxLevel => Level >= Definition.MaxLevel;

		public RelicInstance(RelicSO definition)
		{
			Definition = definition ?? throw new ArgumentNullException(nameof(definition));
			Level = 1;
		}

		public bool TryLevelUp()
		{
			if (IsMaxLevel)
				return false;

			Level++;
			return true;
		}
	}
}
