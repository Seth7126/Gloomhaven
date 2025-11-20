using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Unity;

namespace System.Data.SqlClient;

public class SqlEnclaveAttestationParameters
{
	public ECDiffieHellmanCng ClientDiffieHellmanKey
	{
		[CompilerGenerated]
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	public int Protocol
	{
		[CompilerGenerated]
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(int);
		}
	}

	public SqlEnclaveAttestationParameters(int protocol, byte[] input, ECDiffieHellmanCng clientDiffieHellmanKey)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	public byte[] GetInput()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}
}
