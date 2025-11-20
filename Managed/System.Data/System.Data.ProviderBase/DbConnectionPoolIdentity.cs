using System.Data.SqlClient;
using System.Security.Principal;

namespace System.Data.ProviderBase;

[Serializable]
internal sealed class DbConnectionPoolIdentity
{
	private static DbConnectionPoolIdentity s_lastIdentity = null;

	public static readonly DbConnectionPoolIdentity NoIdentity = new DbConnectionPoolIdentity(string.Empty, isRestricted: false, isNetwork: true);

	private readonly string _sidString;

	private readonly bool _isRestricted;

	private readonly bool _isNetwork;

	private readonly int _hashCode;

	internal bool IsRestricted => _isRestricted;

	internal static DbConnectionPoolIdentity GetCurrent()
	{
		if (!TdsParserStateObjectFactory.UseManagedSNI)
		{
			return GetCurrentNative();
		}
		return GetCurrentManaged();
	}

	private static DbConnectionPoolIdentity GetCurrentNative()
	{
		DbConnectionPoolIdentity result;
		using (WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent())
		{
			IntPtr token = windowsIdentity.AccessToken.DangerousGetHandle();
			bool flag = windowsIdentity.User.IsWellKnown(WellKnownSidType.NetworkSid);
			string value = windowsIdentity.User.Value;
			bool flag2 = Win32NativeMethods.IsTokenRestrictedWrapper(token);
			DbConnectionPoolIdentity dbConnectionPoolIdentity = s_lastIdentity;
			result = ((dbConnectionPoolIdentity == null || !(dbConnectionPoolIdentity._sidString == value) || dbConnectionPoolIdentity._isRestricted != flag2 || dbConnectionPoolIdentity._isNetwork != flag) ? new DbConnectionPoolIdentity(value, flag2, flag) : dbConnectionPoolIdentity);
		}
		s_lastIdentity = result;
		return result;
	}

	private DbConnectionPoolIdentity(string sidString, bool isRestricted, bool isNetwork)
	{
		_sidString = sidString;
		_isRestricted = isRestricted;
		_isNetwork = isNetwork;
		_hashCode = sidString?.GetHashCode() ?? 0;
	}

	public override bool Equals(object value)
	{
		bool flag = this == NoIdentity || this == value;
		if (!flag && value != null)
		{
			DbConnectionPoolIdentity dbConnectionPoolIdentity = (DbConnectionPoolIdentity)value;
			flag = _sidString == dbConnectionPoolIdentity._sidString && _isRestricted == dbConnectionPoolIdentity._isRestricted && _isNetwork == dbConnectionPoolIdentity._isNetwork;
		}
		return flag;
	}

	public override int GetHashCode()
	{
		return _hashCode;
	}

	internal static DbConnectionPoolIdentity GetCurrentManaged()
	{
		string sidString = ((!string.IsNullOrWhiteSpace(Environment.UserDomainName)) ? (Environment.UserDomainName + "\\") : "") + Environment.UserName;
		bool isNetwork = false;
		bool isRestricted = false;
		return new DbConnectionPoolIdentity(sidString, isRestricted, isNetwork);
	}
}
