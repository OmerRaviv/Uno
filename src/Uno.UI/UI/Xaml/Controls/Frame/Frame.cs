﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Uno.Disposables;
using System.Text;
using Uno.Extensions;
using System.Collections.Specialized;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Animation;
using Uno;
using Windows.UI.Xaml.Media;

namespace Windows.UI.Xaml.Controls
{
	public partial class Frame : ContentControl
	{
		private string _navigationState;

		public Frame()
		{
			var backStack = new ObservableCollection<PageStackEntry>();
			var forwardStack = new ObservableCollection<PageStackEntry>();

			backStack.CollectionChanged += (s, e) => CanGoBack = BackStack.Any();
			forwardStack.CollectionChanged += (s, e) => CanGoForward = ForwardStack.Any();

			BackStack = backStack;
			ForwardStack = forwardStack;
		}

		internal PageStackEntry CurrentEntry { get; set; }

#region BackStackDepth DependencyProperty

		public int BackStackDepth
		{
			get { return (int)GetValue(BackStackDepthProperty); }
			private set { this.SetValue(BackStackDepthProperty, value); }
		}

		// Using a DependencyProperty as the backing store for BackStackDepth.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BackStackDepthProperty =
			DependencyProperty.Register("BackStackDepth", typeof(int), typeof(Frame), new PropertyMetadata(0, (s, e) => ((Frame)s)?.OnBackStackDepthChanged(e)));


		protected virtual void OnBackStackDepthChanged(DependencyPropertyChangedEventArgs e)
		{

		}

#endregion

#region BackStack DependencyProperty

		public IList<PageStackEntry> BackStack
		{
			get { return (IList<PageStackEntry>)GetValue(BackStackProperty); }
			set { SetValue(BackStackProperty, value); }
		}

		// Using a DependencyProperty as the backing store for BackStack.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty BackStackProperty =
			DependencyProperty.Register("BackStack", typeof(IList<PageStackEntry>), typeof(Frame), new PropertyMetadata(null, (s, e) => ((Frame)s)?.OnBackStackChanged(e)));

		private void OnBackStackChanged(DependencyPropertyChangedEventArgs e)
		{
		}

#endregion

#region CacheSize DependencyProperty

		public int CacheSize
		{
			get { return (int)GetValue(CacheSizeProperty); }
			set { SetValue(CacheSizeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CacheSize.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CacheSizeProperty =
			DependencyProperty.Register("CacheSize", typeof(int), typeof(Frame), new PropertyMetadata(0, (s, e) => ((Frame)s)?.OnCacheSizeChanged(e)));


		private void OnCacheSizeChanged(DependencyPropertyChangedEventArgs e)
		{
		}

#endregion

#region CanGoBack DependencyProperty

		public bool CanGoBack
		{
			get { return (bool)GetValue(CanGoBackProperty); }
			private set { SetValue(CanGoBackProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CanGoBack.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CanGoBackProperty =
			DependencyProperty.Register("CanGoBack", typeof(bool), typeof(Frame), new PropertyMetadata(false, (s, e) => ((Frame)s)?.OnCanGoBackChanged(e)));


		private void OnCanGoBackChanged(DependencyPropertyChangedEventArgs e)
		{
		}

#endregion

#region CanGoForward DependencyProperty

		public bool CanGoForward
		{
			get { return (bool)GetValue(CanGoForwardProperty); }
			private set { SetValue(CanGoForwardProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CanGoForward.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CanGoForwardProperty =
			DependencyProperty.Register("CanGoForward", typeof(bool), typeof(Frame), new PropertyMetadata(true, (s, e) => ((Frame)s)?.OnCanGoForwardChanged(e)));


		private void OnCanGoForwardChanged(DependencyPropertyChangedEventArgs e)
		{

		}

#endregion

#region CurrentSourcePageType DependencyProperty

		public Type CurrentSourcePageType
		{
			get { return (Type)GetValue(CurrentSourcePageTypeProperty); }
			private set { SetValue(CurrentSourcePageTypeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for CurrentSourcePageType.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty CurrentSourcePageTypeProperty =
			DependencyProperty.Register("CurrentSourcePageType", typeof(Type), typeof(Frame), new PropertyMetadata(null, (s, e) => ((Frame)s)?.OnCurrentSourcePageTypeChanged(e)));


		private void OnCurrentSourcePageTypeChanged(DependencyPropertyChangedEventArgs e)
		{

		}

#endregion

#region ForwardStack DependencyProperty

		public IList<PageStackEntry> ForwardStack
		{
			get { return (IList<PageStackEntry>)GetValue(ForwardStackProperty); }
			private set { SetValue(ForwardStackProperty, value); }
		}

		// Using a DependencyProperty as the backing store for ForwardStack.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty ForwardStackProperty =
			DependencyProperty.Register("ForwardStack", typeof(IList<PageStackEntry>), typeof(Frame), new PropertyMetadata(null, (s, e) => ((Frame)s)?.OnForwardStackChanged(e)));


		private void OnForwardStackChanged(DependencyPropertyChangedEventArgs e)
		{
		}

#endregion

#region SourcePageType DependencyProperty

		public Type SourcePageType
		{
			get { return (Type)GetValue(SourcePageTypeProperty); }
			private set { SetValue(SourcePageTypeProperty, value); }
		}

		// Using a DependencyProperty as the backing store for SourcePageType.  This enables animation, styling, binding, etc...
		public static readonly DependencyProperty SourcePageTypeProperty =
			DependencyProperty.Register("SourcePageType", typeof(Type), typeof(Frame), new PropertyMetadata(null, (s, e) => ((Frame)s)?.OnSourcePageTypeChanged(e)));

		private void OnSourcePageTypeChanged(DependencyPropertyChangedEventArgs e)
		{
		}

#endregion

		public event NavigatedEventHandler Navigated;

		public event NavigatingCancelEventHandler Navigating;

		public event NavigationFailedEventHandler NavigationFailed;

		public event NavigationStoppedEventHandler NavigationStopped;

		public string GetNavigationState() => _navigationState;

		public void GoBack() => GoBack(null);

		public void GoBack(NavigationTransitionInfo transitionInfoOverride)
		{
			if (CanGoBack)
			{

				var entry = BackStack.Last();
				if (transitionInfoOverride != null)
				{
					entry.NavigationTransitionInfo = transitionInfoOverride;
				}
				else
				{
					// Fallback to the page forward navigation transition info
					entry.NavigationTransitionInfo = CurrentEntry.NavigationTransitionInfo;
				}

				InnerNavigate(entry, NavigationMode.Back);
			}
		}

		public void GoForward()
		{
			if (CanGoForward)
			{
				InnerNavigate(ForwardStack.Last(), NavigationMode.Forward);
			}
		}

		public bool Navigate(Type sourcePageType) => Navigate(sourcePageType, null, null);

		public bool Navigate(Type sourcePageType, object parameter) => Navigate(sourcePageType, parameter, null);

		public bool Navigate(Type sourcePageType, object parameter, NavigationTransitionInfo infoOverride)
		{
			var entry = new PageStackEntry(sourcePageType, parameter, infoOverride);
			return InnerNavigate(entry, NavigationMode.New);
		}

		private bool InnerNavigate(PageStackEntry entry, NavigationMode mode)
		{
			try
			{
				// Navigating
				var navigatingFromArgs = new NavigatingCancelEventArgs(
					mode,
					entry.NavigationTransitionInfo,
					entry.Parameter,
					entry.SourcePageType
				);

				Navigating?.Invoke(this, navigatingFromArgs);

				CurrentEntry?.Instance.OnNavigatingFrom(navigatingFromArgs);

				if (navigatingFromArgs.Cancel)
				{
					NavigationStopped?.Invoke(this, new NavigationEventArgs(
						entry.Instance,
						mode,
						entry.NavigationTransitionInfo,
						entry.Parameter,
						entry.SourcePageType,
						null
					));
					return false;
				}

				// Navigate
				var previousEntry = CurrentEntry;
				CurrentEntry = entry;

				if (CurrentEntry.Instance == null)
				{
					var page = CreatePageInstance(entry.SourcePageType);
					if (page == null)
					{
						return false;
					}

					page.Frame = this;
					CurrentEntry.Instance = page;
				}

				Content = CurrentEntry.Instance;

				switch (mode)
				{
					case NavigationMode.New:
						ForwardStack.Clear();
						if (previousEntry != null)
						{
							BackStack.Add(previousEntry);
						}
						break;
					case NavigationMode.Back:
						ForwardStack.Add(previousEntry);
						BackStack.Remove(CurrentEntry);
						break;
					case NavigationMode.Forward:
						BackStack.Add(previousEntry);
						ForwardStack.Remove(CurrentEntry);
						break;
					case NavigationMode.Refresh:
						break;
				}

				// Navigated
				var navigationEvent = new NavigationEventArgs(
					CurrentEntry.Instance,
					mode,
					entry.NavigationTransitionInfo,
					entry.Parameter,
					entry.SourcePageType,
					null
				);

				previousEntry?.Instance.OnNavigatedFrom(navigationEvent);
				CurrentEntry.Instance.OnNavigatedTo(navigationEvent);
				Navigated?.Invoke(this, navigationEvent);

				VisualTreeHelper.CloseAllPopups();

				return true;
			}
			catch (Exception exception)
			{
				NavigationFailed?.Invoke(this, new NavigationFailedEventArgs(entry.SourcePageType, exception));
				return false;
			}
		}

		public void SetNavigationState(string navigationState) => _navigationState = navigationState;

		private Page CreatePageInstance(Type sourcePageType)
		{
			if (Uno.UI.DataBinding.BindingPropertyHelper.BindableMetadataProvider != null)
			{
				var bindableType = Uno.UI.DataBinding.BindingPropertyHelper.BindableMetadataProvider.GetBindableTypeByType(sourcePageType);

				if (bindableType != null)
				{
					return bindableType.CreateInstance()() as Page;
				}
			}

			return Activator.CreateInstance(sourcePageType) as Page;
		}
	}
}