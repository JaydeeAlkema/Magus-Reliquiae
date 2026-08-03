using System;
using System.Collections.Generic;

namespace Relic
{
	public sealed class RelicBag
	{
		private readonly List<RelicInstance> _instances = new();

		public event Action<RelicInstance> onRelicAdded;
		public event Action<RelicInstance> onRelicRemoved;

		public IReadOnlyList<RelicInstance> Relics => _instances;
		public int Count => _instances.Count;

		public void Add(RelicInstance instance)
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			_instances.Add(instance);
			onRelicAdded?.Invoke(instance);
		}

		public bool Remove(RelicInstance instance)
		{
			if (!_instances.Remove(instance)) return false;
			onRelicRemoved?.Invoke(instance);
			return true;
		}

		public bool Contains(RelicInstance instance)
		{
			return _instances.Contains(instance);
		}

		public void Clear()
		{
			for (int i = _instances.Count - 1; i >= 0; i--)
			{
				RelicInstance inst = _instances[i];
				_instances.RemoveAt(i);
				onRelicRemoved?.Invoke(inst);
			}
		}
	}
}
