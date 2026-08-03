using System.Collections.Generic;
using Relic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
	public class BagOfHoldingUI : MonoBehaviour, IDropHandler
	{
		[SerializeField] private RelicBagItemUI ItemPrefab;
		[SerializeField] private Transform ItemContainer;

		private RelicBag _bag;
		private readonly Dictionary<RelicInstance, RelicBagItemUI> _itemViews = new();


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

		public RelicBagItemUI GetItemView(RelicInstance instance)
		{
			return _itemViews.GetValueOrDefault(instance);
		}
	}
}
