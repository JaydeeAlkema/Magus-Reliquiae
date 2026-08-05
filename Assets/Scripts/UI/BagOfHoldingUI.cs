using System.Collections.Generic;
using Relic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
	/// <summary>
	///     UI view for the player's relic bag.
	/// </summary>
	/// <remarks>
	///     Assign the item prefab and container in the bag UI prefab, then initialize it with a <see cref="RelicBag" />.
	/// </remarks>
	public class BagOfHoldingUI : MonoBehaviour, IDropHandler
	{
		/// <summary>
		///     Prefab used for bag item entries.
		/// </summary>
		[SerializeField] private RelicBagItemUI ItemPrefab;
		/// <summary>
		///     Parent transform that receives instantiated entries.
		/// </summary>
		[SerializeField] private Transform ItemContainer;

		private RelicBag _bag;
		private readonly Dictionary<RelicInstance, RelicBagItemUI> _itemViews = new();


		/// <summary>
		///     Binds this view to a relic bag.
		/// </summary>
		/// <param name="bag">Bag to display.</param>
		public void Initialize(RelicBag bag)
		{
			if (_bag != null)
			{
				_bag.onRelicAdded -= OnRelicAdded;
				_bag.onRelicRemoved -= OnRelicRemoved;
			}

			_bag = bag;
			bag.onRelicAdded += OnRelicAdded;
			bag.onRelicRemoved += OnRelicRemoved;
			Rebuild();
		}

		private void OnDestroy()
		{
			if (_bag == null) return;
			_bag.onRelicAdded -= OnRelicAdded;
			_bag.onRelicRemoved -= OnRelicRemoved;
		}


		private void OnRelicAdded(RelicInstance instance)
		{
			SpawnItem(instance);
		}

		private void OnRelicRemoved(RelicInstance instance)
		{
			if (!_itemViews.TryGetValue(instance, out RelicBagItemUI view)) return;
			_itemViews.Remove(instance);
			Destroy(view.gameObject);
		}


		/// <summary>
		///     Handles relic bag drop interactions.
		/// </summary>
		/// <param name="eventData">Pointer event payload.</param>
		public void OnDrop(PointerEventData eventData)
		{
			RelicDragHandler.Instance?.HandleBagDrop();
		}


		private void Rebuild()
		{
			foreach (RelicBagItemUI view in _itemViews.Values)
			{
				Destroy(view.gameObject);
			}

			_itemViews.Clear();

			if (_bag == null) return;
			foreach (RelicInstance instance in _bag.Relics)
			{
				SpawnItem(instance);
			}
		}

		private void SpawnItem(RelicInstance instance)
		{
			if (ItemPrefab == null || ItemContainer == null) return;
			RelicBagItemUI item = Instantiate(ItemPrefab, ItemContainer);
			item.Bind(instance);
			_itemViews[instance] = item;
		}

		/// <summary>
		///     Returns the instantiated view for a relic instance.
		/// </summary>
		/// <param name="instance">Relic instance to look up.</param>
		/// <returns>The matching item view, or null.</returns>
		public RelicBagItemUI GetItemView(RelicInstance instance)
		{
			return _itemViews.GetValueOrDefault(instance);
		}
	}
}
