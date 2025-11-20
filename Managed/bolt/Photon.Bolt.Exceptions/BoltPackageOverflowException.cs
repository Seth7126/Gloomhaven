namespace Photon.Bolt.Exceptions;

public class BoltPackageOverflowException : BoltException
{
	public BoltPackageOverflowException()
		: base("Package Overflow: the remote client may be hanging or slow to receive data. Remote Connection: {0}")
	{
	}
}
