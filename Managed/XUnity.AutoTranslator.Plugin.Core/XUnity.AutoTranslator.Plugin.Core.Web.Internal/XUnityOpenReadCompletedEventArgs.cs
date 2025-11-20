using System;
using System.ComponentModel;
using System.IO;

namespace XUnity.AutoTranslator.Plugin.Core.Web.Internal;

public class XUnityOpenReadCompletedEventArgs : AsyncCompletedEventArgs
{
	private Stream result;

	public Stream Result
	{
		get
		{
			RaiseExceptionIfNecessary();
			return result;
		}
	}

	internal XUnityOpenReadCompletedEventArgs(Stream result, Exception error, bool cancelled, object userState)
		: base(error, cancelled, userState)
	{
		this.result = result;
	}
}
