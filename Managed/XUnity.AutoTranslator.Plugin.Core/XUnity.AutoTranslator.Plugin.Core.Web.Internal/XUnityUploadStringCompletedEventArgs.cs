using System;
using System.ComponentModel;

namespace XUnity.AutoTranslator.Plugin.Core.Web.Internal;

public class XUnityUploadStringCompletedEventArgs : AsyncCompletedEventArgs
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

	internal XUnityUploadStringCompletedEventArgs(string result, Exception error, bool cancelled, object userState)
		: base(error, cancelled, userState)
	{
		this.result = result;
	}
}
