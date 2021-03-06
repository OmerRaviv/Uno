﻿#if NETSTANDARD2_0
#pragma warning disable CS0067, CS0414
#endif

#if XAMARIN || __WASM__
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Uno.Extensions;

namespace Windows.UI.Xaml.Controls
{
	public partial class WebView : Control
	{
		private const string BlankUrl = "about:blank";
		private static readonly Uri BlankUri = new Uri(BlankUrl);

		private object _internalSource;
		private bool _isLoaded;
		private string _invokeScriptResponse = string.Empty;

		public WebView()
		{
		}

		#region CanGoBack

		public bool CanGoBack
		{
			get { return (bool)GetValue(CanGoBackProperty); }
			private set { SetValue(CanGoBackProperty, value); }
		}

		public static readonly DependencyProperty CanGoBackProperty =
			DependencyProperty.Register("CanGoBack", typeof(bool), typeof(WebView), new PropertyMetadata(false));

		#endregion

		#region CanGoForward

		public bool CanGoForward
		{
			get { return (bool)GetValue(CanGoForwardProperty); }
			private set { SetValue(CanGoForwardProperty, value); }
		}

		public static readonly DependencyProperty CanGoForwardProperty =
			DependencyProperty.Register("CanGoForward", typeof(bool), typeof(WebView), new PropertyMetadata(false));

		#endregion

		#region Source

		public Uri Source
		{
			get { return (Uri)GetValue(SourceProperty); }
			set { SetValue(SourceProperty, value); }
		}

		public static readonly DependencyProperty SourceProperty =
			DependencyProperty.Register("Source", typeof(Uri), typeof(WebView), new PropertyMetadata(null,
				(s, e) => ((WebView)s)?.Navigate((Uri)e.NewValue)));

		#endregion

		#region IsScrollEnabled
		public bool IsScrollEnabled
		{
			get { return (bool)GetValue(IsScrollEnabledProperty); }
			set { SetValue(IsScrollEnabledProperty, value); }
		}

		public static readonly DependencyProperty IsScrollEnabledProperty =
			DependencyProperty.Register("IsScrollEnabled", typeof(bool), typeof(WebView), new PropertyMetadata(true,
				(s, e) => ((WebView)s)?.OnScrollEnabledChangedPartial((bool)e.NewValue)));

		partial void OnScrollEnabledChangedPartial(bool scrollingEnabled);
		#endregion

		public event TypedEventHandler<WebView, WebViewNavigationStartingEventArgs> NavigationStarting;
		public event TypedEventHandler<WebView, WebViewNavigationCompletedEventArgs> NavigationCompleted;
		public event TypedEventHandler<WebView, WebViewNewWindowRequestedEventArgs> NewWindowRequested;
		public event TypedEventHandler<WebView, WebViewUnsupportedUriSchemeIdentifiedEventArgs> UnsupportedUriSchemeIdentified;

		//Remove pragma when implemented for Android
#pragma warning disable 0067
		public event WebViewNavigationFailedEventHandler NavigationFailed;
#pragma warning restore 0067

		public void GoBack()
		{
			GoBackPartial();
		}

		public void GoForward()
		{
			GoForwardPartial();
		}

		public void Navigate(Uri uri)
		{
			this.SetInternalSource(uri ?? BlankUri);
		}

		//
		// Summary:
		//     Loads the specified HTML content as a new document.
		//
		// Parameters:
		//   text:
		//     The HTML content to display in the WebView control.
		public void NavigateToString(string text)
		{
			this.SetInternalSource(text ?? "");
		}

		public void NavigateWithHttpRequestMessage(HttpRequestMessage requestMessage)
		{
			if (requestMessage?.RequestUri == null)
			{
				throw new ArgumentException("Invalid request message. It does not have a RequestUri.");
			}

			SetInternalSource(requestMessage);
		}

		public void Stop()
		{
			StopPartial();
		}

		partial void GoBackPartial();
		partial void GoForwardPartial();
		partial void NavigatePartial(Uri uri);
		partial void NavigateToStringPartial(string text);
		partial void NavigateWithHttpRequestMessagePartial(HttpRequestMessage requestMessage);
		partial void StopPartial();

		protected override void OnLoaded()
		{
			base.OnLoaded();

			_isLoaded = true;
		}

		private void SetInternalSource(object source)
		{
			_internalSource = source;

			this.UpdateFromInternalSource();
		}

		private void UpdateFromInternalSource()
		{
			var uri = _internalSource as Uri;
			if (uri != null)
			{
				NavigatePartial(uri);
				return;
			}

			var html = _internalSource as string;
			if (html != null)
			{
				NavigateToStringPartial(html);
			}

			var message = _internalSource as HttpRequestMessage;
			if (message != null)
			{
				NavigateWithHttpRequestMessagePartial(message);
			}
		}

		private static string ConcatenateJavascriptArguments(string[] arguments)
		{
			var argument = string.Empty;
			if (arguments != null && arguments.Any())
			{
				argument = string.Join(",", arguments);
			}

			return argument;
		}

		internal void OnUnsupportedUriSchemeIdentified(WebViewUnsupportedUriSchemeIdentifiedEventArgs args)
		{
			UnsupportedUriSchemeIdentified?.Invoke(this, args);
		}

		internal bool GetIsHistoryEntryValid(string url) => !url.IsNullOrWhiteSpace() && !url.Equals(BlankUrl, StringComparison.OrdinalIgnoreCase);
	}
}
#endif