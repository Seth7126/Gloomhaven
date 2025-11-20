using System;
using System.ComponentModel;

namespace XUnity.AutoTranslator.Plugin.Core.Web.Internal;

public class XUnityUploadFileCompletedEventArgs : AsyncCompletedEventArgs
{
	private byte[] result;

	public byte[] Result => result;

	internal XUnityUploadFileCompletedEventArgs(byte[] result, Exception error, bool cancelled, object userState)
		: base(error, cancelled, userState)
	{
		this.result = result;
	}
}
