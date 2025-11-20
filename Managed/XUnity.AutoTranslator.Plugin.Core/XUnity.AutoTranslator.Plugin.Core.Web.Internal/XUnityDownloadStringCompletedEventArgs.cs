using System;
using System.ComponentModel;

namespace XUnity.AutoTranslator.Plugin.Core.Web.Internal;

public class XUnityDownloadStringCompletedEventArgs : AsyncCompletedEventArgs
{
	private string result;

	public string Result
	{
		get
		{
			RaiseExceptionIfNecessary();
			return result;
		}
	}

	internal XUnityDownloadStringCompletedEventArgs(string result, Exception error, bool cancelled, object userState)
		: base(error, cancelled, userState)
	{
		this.result = result;
	}
}
