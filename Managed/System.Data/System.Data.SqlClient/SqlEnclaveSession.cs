using System.Runtime.CompilerServices;
using Unity;

namespace System.Data.SqlClient;

public class SqlEnclaveSession
{
	public long SessionId
	{
		[CompilerGenerated]
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(long);
		}
	}

	public SqlEnclaveSession(byte[] sessionKey, long sessionId)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	public byte[] GetSessionKey()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return null;
	}
}
