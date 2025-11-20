namespace ICSharpCode.SharpZipLib.GZip;

internal class GZipException : SharpZipBaseException
{
	public GZipException()
	{
	}

	public GZipException(string message)
		: base(message)
	{
	}
}
