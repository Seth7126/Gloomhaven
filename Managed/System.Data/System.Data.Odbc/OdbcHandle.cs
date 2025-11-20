using System.Data.Common;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace System.Data.Odbc;

internal abstract class OdbcHandle : SafeHandle
{
	private ODBC32.SQL_HANDLE _handleType;

	private OdbcHandle _parentHandle;

	internal ODBC32.SQL_HANDLE HandleType => _handleType;

	public override bool IsInvalid => IntPtr.Zero == handle;

	protected OdbcHandle(ODBC32.SQL_HANDLE handleType, OdbcHandle parentHandle)
		: base(IntPtr.Zero, ownsHandle: true)
	{
		_handleType = handleType;
		bool success = false;
		ODBC32.RetCode retCode = ODBC32.RetCode.SUCCESS;
		RuntimeHelpers.PrepareConstrainedRegions();
		try
		{
			switch (handleType)
			{
			case ODBC32.SQL_HANDLE.ENV:
				retCode = global::Interop.Odbc.SQLAllocHandle(handleType, IntPtr.Zero, out handle);
				break;
			case ODBC32.SQL_HANDLE.DBC:
			case ODBC32.SQL_HANDLE.STMT:
				parentHandle.DangerousAddRef(ref success);
				retCode = global::Interop.Odbc.SQLAllocHandle(handleType, parentHandle, out handle);
				break;
			}
		}
		finally
		{
			if (success && (uint)(handleType - 2) <= 1u)
			{
				if (IntPtr.Zero != handle)
				{
					_parentHandle = parentHandle;
				}
				else
				{
					parentHandle.DangerousRelease();
				}
			}
		}
		if (ADP.PtrZero == handle || retCode != ODBC32.RetCode.SUCCESS)
		{
			throw ODBC.CantAllocateEnvironmentHandle(retCode);
		}
	}

	internal OdbcHandle(OdbcStatementHandle parentHandle, ODBC32.SQL_ATTR attribute)
		: base(IntPtr.Zero, ownsHandle: true)
	{
		_handleType = ODBC32.SQL_HANDLE.DESC;
		bool success = false;
		RuntimeHelpers.PrepareConstrainedRegions();
		ODBC32.RetCode statementAttribute;
		try
		{
			parentHandle.DangerousAddRef(ref success);
			statementAttribute = parentHandle.GetStatementAttribute(attribute, out handle, out var _);
		}
		finally
		{
			if (success)
			{
				if (IntPtr.Zero != handle)
				{
					_parentHandle = parentHandle;
				}
				else
				{
					parentHandle.DangerousRelease();
				}
			}
		}
		if (ADP.PtrZero == handle)
		{
			throw ODBC.FailedToGetDescriptorHandle(statementAttribute);
		}
	}

	protected override bool ReleaseHandle()
	{
		IntPtr intPtr = handle;
		handle = IntPtr.Zero;
		if (IntPtr.Zero != intPtr)
		{
			ODBC32.SQL_HANDLE handleType = HandleType;
			switch (handleType)
			{
			case ODBC32.SQL_HANDLE.ENV:
			case ODBC32.SQL_HANDLE.DBC:
			case ODBC32.SQL_HANDLE.STMT:
				global::Interop.Odbc.SQLFreeHandle(handleType, intPtr);
				break;
			}
		}
		OdbcHandle parentHandle = _parentHandle;
		_parentHandle = null;
		if (parentHandle != null)
		{
			parentHandle.DangerousRelease();
			parentHandle = null;
		}
		return true;
	}

	internal ODBC32.RetCode GetDiagnosticField(out string sqlState)
	{
		StringBuilder stringBuilder = new StringBuilder(6);
		short StringLength;
		ODBC32.RetCode retCode = global::Interop.Odbc.SQLGetDiagFieldW(HandleType, this, 1, 4, stringBuilder, checked((short)(2 * stringBuilder.Capacity)), out StringLength);
		ODBC.TraceODBC(3, "SQLGetDiagFieldW", retCode);
		if (retCode == ODBC32.RetCode.SUCCESS || retCode == ODBC32.RetCode.SUCCESS_WITH_INFO)
		{
			sqlState = stringBuilder.ToString();
		}
		else
		{
			sqlState = ADP.StrEmpty;
		}
		return retCode;
	}

	internal ODBC32.RetCode GetDiagnosticRecord(short record, out string sqlState, StringBuilder message, out int nativeError, out short cchActual)
	{
		StringBuilder stringBuilder = new StringBuilder(5);
		ODBC32.RetCode retCode = global::Interop.Odbc.SQLGetDiagRecW(HandleType, this, record, stringBuilder, out nativeError, message, checked((short)message.Capacity), out cchActual);
		ODBC.TraceODBC(3, "SQLGetDiagRecW", retCode);
		if (retCode == ODBC32.RetCode.SUCCESS || retCode == ODBC32.RetCode.SUCCESS_WITH_INFO)
		{
			sqlState = stringBuilder.ToString();
		}
		else
		{
			sqlState = ADP.StrEmpty;
		}
		return retCode;
	}
}
