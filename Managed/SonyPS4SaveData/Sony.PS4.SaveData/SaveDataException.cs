using System;

namespace Sony.PS4.SaveData;

public class SaveDataException : Exception
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

	public SaveDataException()
	{
	}

	public SaveDataException(string message)
		: base(message)
	{
	}

	public SaveDataException(string message, Exception inner)
		: base(message, inner)
	{
	}

	internal SaveDataException(APIResult apiResult)
		: base(apiResult.message)
	{
		resultType = apiResult.apiResult;
		filename = apiResult.filename;
		lineNumber = apiResult.lineNumber;
		sceErrorCode = apiResult.sceErrorCode;
	}
}
