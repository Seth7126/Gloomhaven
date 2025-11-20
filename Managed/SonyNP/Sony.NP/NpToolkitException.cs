using System;

namespace Sony.NP;

public class NpToolkitException : Exception
{
	internal APIResultTypes resultType = APIResultTypes.Error;

	internal string filename;

	internal int lineNumber;

	internal int sceErrorCode;

	public APIResultTypes ResultType => resultType;

	public string Filename => filename;

	public int LineNumber => lineNumber;

	public int SceErrorCode => sceErrorCode;

	public string ExtendedMessage
	{
		get
		{
			string text = Message;
			if (sceErrorCode != 0)
			{
				text = text + " (Sce : 0x" + sceErrorCode.ToString("X") + " ) ";
			}
			if (filename != null && filename.Length > 0)
			{
				object obj = text;
				text = string.Concat(obj, " ( ", filename, " : Line = ", lineNumber, " ) ");
			}
			return text;
		}
	}

	public NpToolkitException()
	{
	}

	public NpToolkitException(string message)
		: base(message)
	{
	}

	public NpToolkitException(string message, Exception inner)
		: base(message, inner)
	{
	}

	internal NpToolkitException(APIResult apiResult)
		: base(apiResult.message)
	{
		resultType = apiResult.apiResult;
		filename = apiResult.filename;
		lineNumber = apiResult.lineNumber;
		sceErrorCode = apiResult.sceErrorCode;
	}
}
