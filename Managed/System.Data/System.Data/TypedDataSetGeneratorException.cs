using System.Collections;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace System.Data;

/// <summary>The exception that is thrown when a name conflict occurs while generating a strongly typed <see cref="T:System.Data.DataSet" />. </summary>
/// <filterpriority>1</filterpriority>
[Serializable]
public class TypedDataSetGeneratorException : DataException
{
	private ArrayList errorList;

	private string KEY_ARRAYCOUNT = "KEY_ARRAYCOUNT";

	private string KEY_ARRAYVALUES = "KEY_ARRAYVALUES";

	/// <summary>Gets a dynamic list of generated errors.</summary>
	/// <returns>
	///   <see cref="T:System.Collections.ArrayList" /> object.</returns>
	/// <filterpriority>2</filterpriority>
	public ArrayList ErrorList => errorList;

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.TypedDataSetGeneratorException" /> class using the specified serialization information and streaming context.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object. </param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure. </param>
	protected TypedDataSetGeneratorException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		int num = (int)info.GetValue(KEY_ARRAYCOUNT, typeof(int));
		if (num > 0)
		{
			errorList = new ArrayList();
			for (int i = 0; i < num; i++)
			{
				errorList.Add(info.GetValue(KEY_ARRAYVALUES + i, typeof(string)));
			}
		}
		else
		{
			errorList = null;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.TypedDataSetGeneratorException" /> class.</summary>
	public TypedDataSetGeneratorException()
	{
		errorList = null;
		base.HResult = -2146232021;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.TypedDataSetGeneratorException" /> class with the specified string. </summary>
	/// <param name="message">The string to display when the exception is thrown.</param>
	public TypedDataSetGeneratorException(string message)
		: base(message)
	{
		base.HResult = -2146232021;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.TypedDataSetGeneratorException" /> class with the specified string and inner exception. </summary>
	/// <param name="message">The string to display when the exception is thrown.</param>
	/// <param name="innerException">A reference to an inner exception.</param>
	public TypedDataSetGeneratorException(string message, Exception innerException)
		: base(message, innerException)
	{
		base.HResult = -2146232021;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Data.TypedDataSetGeneratorException" /> class.</summary>
	/// <param name="list">
	///   <see cref="T:System.Collections.ArrayList" /> object containing a dynamic list of exceptions. </param>
	public TypedDataSetGeneratorException(ArrayList list)
		: this()
	{
		errorList = list;
		base.HResult = -2146232021;
	}

	/// <summary>Implements the ISerializable interface and returns the data needed to serialize the <see cref="T:System.Data.TypedDataSetGeneratorException" /> object.</summary>
	/// <param name="info">A <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object. </param>
	/// <param name="context">A <see cref="T:System.Runtime.Serialization.StreamingContext" /> structure. </param>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, SerializationFormatter" />
	/// </PermissionSet>
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		if (errorList != null)
		{
			info.AddValue(KEY_ARRAYCOUNT, errorList.Count);
			for (int i = 0; i < errorList.Count; i++)
			{
				info.AddValue(KEY_ARRAYVALUES + i, errorList[i].ToString());
			}
		}
		else
		{
			info.AddValue(KEY_ARRAYCOUNT, 0);
		}
	}
}
