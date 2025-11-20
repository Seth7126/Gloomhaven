using System.IO;
using System.Runtime.Serialization;
using System.Security;

namespace System;

/// <summary>The exception that is thrown when the file image of a dynamic link library (DLL) or an executable program is invalid. </summary>
/// <filterpriority>2</filterpriority>
[Serializable]
public class BadImageFormatException : SystemException
{
	private string _fileName;

	private string _fusionLog;

	/// <summary>Gets the error message and the name of the file that caused this exception.</summary>
	/// <returns>A string containing the error message and the name of the file that caused this exception.</returns>
	/// <filterpriority>2</filterpriority>
	public override string Message
	{
		get
		{
			SetMessageField();
			return _message;
		}
	}

	/// <summary>Gets the name of the file that causes this exception.</summary>
	/// <returns>The name of the file with the invalid image, or a null reference if no file name was passed to the constructor for the current instance.</returns>
	/// <filterpriority>2</filterpriority>
	public string FileName => _fileName;

	/// <summary>Gets the log file that describes why an assembly load failed.</summary>
	/// <returns>A String containing errors reported by the assembly cache.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	public string FusionLog => _fusionLog;

	/// <summary>Initializes a new instance of the <see cref="T:System.BadImageFormatException" /> class.</summary>
	public BadImageFormatException()
		: base("Format of the executable (.exe) or library (.dll) is invalid.")
	{
		base.HResult = -2147024885;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.BadImageFormatException" /> class with a specified error message.</summary>
	/// <param name="message">The message that describes the error. </param>
	public BadImageFormatException(string message)
		: base(message)
	{
		base.HResult = -2147024885;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.BadImageFormatException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not a null reference, the current exception is raised in a catch block that handles the inner exception. </param>
	public BadImageFormatException(string message, Exception inner)
		: base(message, inner)
	{
		base.HResult = -2147024885;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.BadImageFormatException" /> class with a specified error message and file name.</summary>
	/// <param name="message">A message that describes the error. </param>
	/// <param name="fileName">The full name of the file with the invalid image. </param>
	public BadImageFormatException(string message, string fileName)
		: base(message)
	{
		base.HResult = -2147024885;
		_fileName = fileName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.BadImageFormatException" /> class with a specified error message and a reference to the inner exception that is the cause of this exception.</summary>
	/// <param name="message">The error message that explains the reason for the exception. </param>
	/// <param name="fileName">The full name of the file with the invalid image. </param>
	/// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner" /> parameter is not null, the current exception is raised in a catch block that handles the inner exception. </param>
	public BadImageFormatException(string message, string fileName, Exception inner)
		: base(message, inner)
	{
		base.HResult = -2147024885;
		_fileName = fileName;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.BadImageFormatException" /> class with serialized data.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
	protected BadImageFormatException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_fileName = info.GetString("BadImageFormat_FileName");
		_fusionLog = info.GetString("BadImageFormat_FusionLog");
	}

	/// <summary>Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> object with the file name, assembly cache log, and additional exception information.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
	/// <exception cref="T:System.Security.SecurityException">The caller does not have the required permission. </exception>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	[SecurityCritical]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("BadImageFormat_FileName", _fileName, typeof(string));
		info.AddValue("BadImageFormat_FusionLog", _fusionLog, typeof(string));
	}

	private void SetMessageField()
	{
		if (_message == null)
		{
			if (_fileName == null && base.HResult == -2146233088)
			{
				_message = "Format of the executable (.exe) or library (.dll) is invalid.";
			}
			else
			{
				_message = FileLoadException.FormatFileLoadExceptionMessage(_fileName, base.HResult);
			}
		}
	}

	/// <summary>Returns the fully qualified name of this exception and possibly the error message, the name of the inner exception, and the stack trace.</summary>
	/// <returns>A string containing the fully qualified name of this exception and possibly the error message, the name of the inner exception, and the stack trace.</returns>
	/// <filterpriority>2</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="ControlEvidence, ControlPolicy" />
	/// </PermissionSet>
	public override string ToString()
	{
		string text = GetType().ToString() + ": " + Message;
		if (_fileName != null && _fileName.Length != 0)
		{
			text = text + Environment.NewLine + SR.Format("File name: '{0}'", _fileName);
		}
		if (base.InnerException != null)
		{
			text = text + " ---> " + base.InnerException.ToString();
		}
		if (StackTrace != null)
		{
			text = text + Environment.NewLine + StackTrace;
		}
		if (_fusionLog != null)
		{
			if (text == null)
			{
				text = " ";
			}
			text += Environment.NewLine;
			text += Environment.NewLine;
			text += _fusionLog;
		}
		return text;
	}
}
