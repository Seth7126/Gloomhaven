using Unity;

namespace System.Data.SqlClient;

public abstract class SqlAuthenticationInitializer
{
	protected SqlAuthenticationInitializer()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	public abstract void Initialize();
}
