#pragma warning disable 108 // new keyword hiding
#pragma warning disable 114 // new keyword hiding
namespace Windows.ApplicationModel.Email
{
	#if __ANDROID__ || __IOS__ || NET46 || __WASM__
	#if __ANDROID__ || __IOS__ || NET46 || __WASM__
	[global::Uno.NotImplemented]
	#endif
	public   enum EmailMailboxSmimeSigningAlgorithm 
	{
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		Any,
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		Sha1,
		#endif
		#if __ANDROID__ || __IOS__ || NET46 || __WASM__
		MD5,
		#endif
	}
	#endif
}
