using System;
using System.Data.Common;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.SqlServer.Server;

/// <summary>Thrown when SQL Server or the ADO.NET <see cref="N:System.Data.SqlClient" /> provider detects an invalid user-defined type (UDT). </summary>
[Serializable]
public sealed class InvalidUdtException : SystemException
{
	private class HResults
	{
		internal const int InvalidUdt = -2146232009;
	}

	internal InvalidUdtException()
	{
		base.HResult = -2146232009;
	}

	internal InvalidUdtException(string message)
		: base(message)
	{
		base.HResult = -2146232009;
	}

	internal InvalidUdtException(string message, Exception innerException)
		: base(message, innerException)
	{
		base.HResult = -2146232009;
	}

	private InvalidUdtException(SerializationInfo si, StreamingContext sc)
		: base(si, sc)
	{
	}

	/// <summary>Streams all the <see cref="T:Microsoft.SqlServer.Server.InvalidUdtException" /> properties into the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> class for the given <see cref="T:System.Runtime.Serialization.StreamingContext" />.</summary>
	/// <param name="si">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object.</param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> object.</param>
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo si, StreamingContext context)
	{
		base.GetObjectData(si, context);
	}

	internal static InvalidUdtException Create(Type udtType, string resourceReason)
	{
		string text = Res.GetString(resourceReason);
		InvalidUdtException ex = new InvalidUdtException(Res.GetString("'{0}' is an invalid user defined type, reason: {1}.", udtType.FullName, text));
		ADP.TraceExceptionAsReturnValue(ex);
		return ex;
	}
}
