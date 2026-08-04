using System;
using System.Collections.Generic;

namespace Relic
{
	/// <summary>
	/// Runtime bag for unequipped relics.
	/// </summary>
	/// <remarks>
	/// Construct it inside <see cref="PlayerRelicManager"/> and mutate it only through the add/remove API.
	/// </remarks>
	public sealed class RelicBag
	{
		private readonly List<RelicInstance> _instances = new();

		/// <summary>
		/// Fired when a relic is added.
		/// </summary>
		public event Action<RelicInstance> onRelicAdded;
		/// <summary>
		/// Fired when a relic is removed.
		/// </summary>
		public event Action<RelicInstance> onRelicRemoved;

		/// <summary>
		/// Current relic list.
		/// </summary>
		public IReadOnlyList<RelicInstance> Relics => _instances;
		/// <summary>
		/// Current bag size.
		/// </summary>
		public int Count => _instances.Count;

		/// <summary>
		/// Adds a relic to the bag.
		/// </summary>
		/// <param name="instance">Relic to add.</param>
		public void Add(RelicInstance instance)
		{
			if (instance == null) throw new ArgumentNullException(nameof(instance));
			_instances.Add(instance);
			onRelicAdded?.Invoke(instance);
		}

		/// <summary>
		/// Removes a relic from the bag.
		/// </summary>
		/// <param name="instance">Relic to remove.</param>
		/// <returns>True when the relic was removed.</returns>
		public bool Remove(RelicInstance instance)
		{
			if (!_instances.Remove(instance)) return false;
			onRelicRemoved?.Invoke(instance);
			return true;
		}

		/// <summary>
		/// Checks whether the bag contains a relic.
		/// </summary>
		/// <param name="instance">Relic to check.</param>
		/// <returns>True when the relic is present.</returns>
		public bool Contains(RelicInstance instance)
		{
			return _instances.Contains(instance);
		}

		/// <summary>
		/// Removes every relic from the bag.
		/// </summary>
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
