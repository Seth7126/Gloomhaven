namespace ICSharpCode.SharpZipLib.Tar;

internal class TarException : SharpZipBaseException
{
	public TarException()
	{
	}

	public TarException(string message)
		: base(message)
	{
	}
}
