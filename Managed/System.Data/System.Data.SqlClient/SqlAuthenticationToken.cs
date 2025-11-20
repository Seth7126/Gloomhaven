using System.Runtime.CompilerServices;
using Unity;

namespace System.Data.SqlClient;

public class SqlAuthenticationToken
{
	public string AccessToken
	{
		[CompilerGenerated]
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return null;
		}
	}

	public DateTimeOffset ExpiresOn
	{
		[CompilerGenerated]
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(DateTimeOffset);
		}
	}

	public SqlAuthenticationToken(string accessToken, DateTimeOffset expiresOn)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
