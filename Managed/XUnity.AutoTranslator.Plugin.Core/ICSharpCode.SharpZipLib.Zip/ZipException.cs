namespace ICSharpCode.SharpZipLib.Zip;

internal class ZipException : SharpZipBaseException
{
	public ZipException()
	{
	}

	public ZipException(string msg)
		: base(msg)
	{
	}
}
