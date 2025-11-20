namespace ICSharpCode.SharpZipLib.Tar;

internal class InvalidHeaderException : TarException
{
	public InvalidHeaderException()
	{
	}

	public InvalidHeaderException(string msg)
		: base(msg)
	{
	}
}
