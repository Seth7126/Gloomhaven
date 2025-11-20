namespace Photon.Bolt.Exceptions;

public class BoltAssertFailedException : BoltException
{
	public BoltAssertFailedException(string msg = "")
		: base("Assert Failed: {0}", msg)
	{
	}
}
