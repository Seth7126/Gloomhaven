using Unity;

namespace System.Data.SqlClient;

public class SqlClientLogger
{
	public bool IsLoggingEnabled
	{
		get
		{
			Unity.ThrowStub.ThrowNotSupportedException();
			return default(bool);
		}
	}

	public SqlClientLogger()
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	public bool LogAssert(bool value, string type, string method, string message)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
		return default(bool);
	}

	public void LogError(string type, string method, string message)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}

	public void LogInfo(string type, string method, string message)
	{
		Unity.ThrowStub.ThrowNotSupportedException();
	}
}
