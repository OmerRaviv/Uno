﻿using Uno.UI.Controls;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uno.Extensions;
using System.Collections.Specialized;
using Uno.Extensions.Specialized;
using System.Diagnostics;
using Uno.UI;
using Uno.Disposables;
using Windows.UI.Xaml.Data;

namespace Windows.UI.Xaml.Controls.Primitives
{
	public partial class Selector : ItemsControl
	{
		public event SelectionChangedEventHandler SelectionChanged;

		private readonly SerialDisposable _collectionViewSubscription = new SerialDisposable();

		/// <summary>
		/// This is always true for <see cref="FlipView"/> and <see cref="ComboBox"/>, and depends on the value of <see cref="ListViewBase.SelectionMode"/> for <see cref="ListViewBase"/>.
		/// </summary>
		internal virtual bool IsSingleSelection => true;

		public Selector()
		{

		}

		public static readonly DependencyProperty SelectedItemProperty =
		DependencyProperty.Register(
			"SelectedItem",
			typeof(object),
			typeof(Selector),
			new PropertyMetadata(
				defaultValue: null,
				propertyChangedCallback: (s, e) => (s as Selector).OnSelectedItemChanged(e.OldValue, e.NewValue)
			)
		);

		public object SelectedItem
		{
			get { return (object)this.GetValue(SelectedItemProperty); }
			set { this.SetValue(SelectedItemProperty, value); }
		}

		internal virtual void OnSelectedItemChanged(object oldSelectedItem, object selectedItem)
		{
			var wasSelectionUnset = oldSelectedItem == null && (!GetItems()?.Contains(null) ?? false);
			var isSelectionUnset = false;
			if (!GetItems()?.Contains(selectedItem) ?? false)
			{
				if (selectedItem == null)
				{
					isSelectionUnset = true;
				}
				else
				{
					//Prevent SelectedItem being set to an invalid value
					SelectedItem = oldSelectedItem;
					return;
				}
			}

			// If SelectedIndex is -1 and SelectedItem is being changed from non-null to null, this indicates that we're desetting 
			// SelectedItem, not setting a null inside the collection as selected. Little edge case there. (Note that this relies 
			// on user interactions setting SelectedIndex which then sets SelectedItem.)
			if (SelectedIndex == -1 && selectedItem == null)
			{
				isSelectionUnset = true;
			}

			var newIndex = IndexFromItem(selectedItem);
			if (SelectedIndex != newIndex)
			{
				SelectedIndex = newIndex;
			}

			InvokeSelectionChanged(wasSelectionUnset ? new object[] { } : new[] { oldSelectedItem },
				isSelectionUnset ? new object[] { } : new[] { selectedItem }
			);
			OnSelectedItemChangedPartial(oldSelectedItem, selectedItem);
		}

		partial void OnSelectedItemChangedPartial(object oldSelectedItem, object selectedItem);

		internal void InvokeSelectionChanged(object[] removedItems, object[] addedItems)
		{
			SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(removedItems, addedItems));
		}

		public int SelectedIndex
		{
			get { return (int)this.GetValue(SelectedIndexProperty); }
			set { this.SetValue(SelectedIndexProperty, value); }
		}

		// Using a DependencyProperty as the backing store for SelectedIndex.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SelectedIndexProperty =
			DependencyProperty.Register("SelectedIndex", typeof(int), typeof(Selector), new PropertyMetadata(-1,
				(s, e) => (s as Selector).OnSelectedIndexChanged((int)e.OldValue, (int)e.NewValue)
			)
		);

		private void OnSelectedIndexChanged(int oldSelectedIndex, int newSelectedIndex)
		{
			var newSelectedItem = ItemFromIndex(newSelectedIndex);

			if (ItemsSource is ICollectionView collectionView)
			{
				collectionView.MoveCurrentToPosition(newSelectedIndex);
				//TODO: we should check if CurrentPosition actually changes, and set SelectedIndex back if not.
			}
			if (!object.Equals(SelectedItem, newSelectedItem))
			{
				SelectedItem = newSelectedItem;
			}

			SelectedIndexPath = GetIndexPathFromIndex(SelectedIndex);
		}

		/// <summary>
		/// The selected index as an <see cref="IndexPath"/> of (group, group position), where group=0 if the source is ungrouped.
		/// </summary>
		private IndexPath? SelectedIndexPath { get; set; }

		protected override void OnItemsSourceChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnItemsSourceChanged(e);
			if (ItemsSource is ICollectionView collectionView)
			{
				_collectionViewSubscription.Disposable = Disposable.Create(() => collectionView.CurrentChanged -= OnCollectionViewCurrentChanged);
				collectionView.CurrentChanged += OnCollectionViewCurrentChanged;
				SelectedIndex = collectionView.CurrentPosition;
			}
			else
			{
				_collectionViewSubscription.Disposable = null;
				SelectedIndex = -1;
			}
		}

		private void OnCollectionViewCurrentChanged(object sender, object e)
		{
			if (IsSingleSelection)
			{
				if (ItemsSource is ICollectionView collectionView)
				{
					SelectedIndex = collectionView.CurrentPosition;
				}
			}
		}

		protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
		{
			base.PrepareContainerForItemOverride(element, item);

			if (element is SelectorItem selectorItem)
			{
				selectorItem.IsSelected = IsSelected(IndexFromContainer(element));
			}

			PrepareContainerForItemOverridePartial(element, item);
		}

		protected override void ClearContainerForItemOverride(DependencyObject element, object item)
		{
			base.ClearContainerForItemOverride(element, item);

			ClearContainerForItemOverridePartial(element, item);
		}

		partial void PrepareContainerForItemOverridePartial(DependencyObject element, object item);
		partial void ClearContainerForItemOverridePartial(DependencyObject element, object item);

		internal virtual bool IsSelected(int index)
		{
			return SelectedIndex == index;
		}

		internal override void OnItemsSourceSingleCollectionChanged(object sender, NotifyCollectionChangedEventArgs c, int section)
		{
			// Get SelectedIndex before calling base, as a quick workaround for side-effect in FlipView.Android that changes the SelectedIndex, to avoid incrementing it twice
			var selectedIndexToSet = SelectedIndex;
			base.OnItemsSourceSingleCollectionChanged(sender, c, section);
			switch (c.Action)
			{
				case NotifyCollectionChangedAction.Add:

					//Advance SelectedIndex if items are being inserted before it
					var newIndex = GetIndexFromIndexPath(IndexPath.FromRowSection(c.NewStartingIndex, section));
					if (selectedIndexToSet >= newIndex)
					{
						selectedIndexToSet += c.NewItems.Count;
						SelectedIndex = selectedIndexToSet;
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					{
						var oldIndex = GetIndexFromIndexPath(IndexPath.FromRowSection(c.OldStartingIndex, section));
						if (selectedIndexToSet >= oldIndex && selectedIndexToSet < oldIndex + c.OldItems.Count)
						{
							//Deset if selected item is being removed
							SelectedIndex = -1;
						}
						else if (selectedIndexToSet >= oldIndex + c.OldItems.Count)
						{
							//Decrement SelectedIndex if items are removed before it
							selectedIndexToSet -= c.OldItems.Count;
							SelectedIndex = selectedIndexToSet;
						}
					}
					break;
				case NotifyCollectionChangedAction.Replace:
					{
						var oldIndex = GetIndexFromIndexPath(IndexPath.FromRowSection(c.OldStartingIndex, section));
						if (selectedIndexToSet >= oldIndex && selectedIndexToSet < oldIndex + c.OldItems.Count)
						{
							//Deset if selected item is being replaced
							SelectedIndex = -1;
						}
					}
					break;
				case NotifyCollectionChangedAction.Reset:
					if (SelectedIndexPath?.Section == section)
					{
						SelectedIndex = -1;
					}
					break;
			}
			//TODO: handle other cases (PBI #27502)
		}

		internal override void OnItemsSourceGroupsChanged(object sender, NotifyCollectionChangedEventArgs c)
		{
			base.OnItemsSourceGroupsChanged(sender, c);

			if (SelectedIndexPath == null)
			{
				return;
			}
			var selectedIndexPath = SelectedIndexPath.Value;

			switch (c.Action)
			{
				case NotifyCollectionChangedAction.Add:
					var newIndexPath = IndexPath.FromRowSection(row: 0, section: c.NewStartingIndex);
					if (selectedIndexPath >= newIndexPath)
					{
						//Advance SelectedIndex if groups are being inserted before it
						var newSelectedIndex = SelectedIndex;
						for (int i = c.NewStartingIndex; i < c.NewStartingIndex + c.NewItems.Count; i++)
						{
							newSelectedIndex += GetGroupCount(i);
						}
						SelectedIndex = newSelectedIndex;
					}
					break;
				case NotifyCollectionChangedAction.Remove:
					{
						var oldIndexPath = IndexPath.FromRowSection(row: 0, section: c.OldStartingIndex);
						var oldIndexPathLast = IndexPath.FromRowSection(row: int.MaxValue, section: c.OldStartingIndex + c.OldItems.Count);
						if (selectedIndexPath >= oldIndexPath && selectedIndexPath < oldIndexPathLast)
						{
							//Deset if selected item is in group being removed
							SelectedIndex = -1;
						}
						else if (selectedIndexPath >= oldIndexPathLast)
						{
							//Decrement SelectedIndex if groups are removed before it
							var newSelectedIndex = SelectedIndex;
							for (int i = c.OldStartingIndex; i < c.OldStartingIndex + c.OldItems.Count; i++)
							{
								newSelectedIndex -= GetGroupCount(i);
							}
							SelectedIndex = newSelectedIndex;
						}
					}
					break;
				case NotifyCollectionChangedAction.Replace:
					{
						var oldIndexPath = IndexPath.FromRowSection(row: 0, section: c.OldStartingIndex);
						var oldIndexPathLast = IndexPath.FromRowSection(row: int.MaxValue, section: c.OldStartingIndex + c.OldItems.Count);
						if (selectedIndexPath >= oldIndexPath && selectedIndexPath < oldIndexPathLast)
						{
							//Deset if selected item is in group being replaced
							SelectedIndex = -1;
						}
					}
					break;
				case NotifyCollectionChangedAction.Reset:
					SelectedIndex = -1;
					break;
			}
		}

		internal void OnItemClicked(SelectorItem selectorItem) => OnItemClicked(IndexFromContainer(selectorItem));

		internal virtual void OnItemClicked(int clickedIndex) { }
	}
}