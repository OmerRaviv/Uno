#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.Media.Protection.PlayReady
{
	#if __ANDROID__ || __IOS__ || NET46 || __WASM__
	#if __ANDROID__ || __IOS__ || NET46 || __WASM__
	[global::Uno.NotImplemented]
	#endif
	public   enum PlayReadyITADataFormat 
	{
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		SerializedProperties,
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		SerializedProperties_WithContentProtectionWrapper,
		#endif
	}
	#endif
}
