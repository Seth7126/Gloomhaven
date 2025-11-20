using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace System.Drawing.Printing;

/// <summary>Represents the exception that is thrown when you try to access a printer using printer settings that are not valid.</summary>
[Serializable]
public class InvalidPrinterException : SystemException
{
	private PrinterSettings _settings;

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.InvalidPrinterException" /> class with serialized data.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	/// <exception cref="T:System.Runtime.Serialization.SerializationException">The class name is null or <see cref="P:System.Exception.HResult" /> is 0. </exception>
	protected InvalidPrinterException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
		_settings = (PrinterSettings)info.GetValue("settings", typeof(PrinterSettings));
	}

	/// <summary>Overridden. Sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.</summary>
	/// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown. </param>
	/// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="info" /> is null. </exception>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Read="*AllFiles*" PathDiscovery="*AllFiles*" />
	///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="SerializationFormatter" />
	///   <IPermission class="System.Drawing.Printing.PrintingPermission, System.Drawing, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" version="1" Unrestricted="true" />
	/// </PermissionSet>
	[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
	public override void GetObjectData(SerializationInfo info, StreamingContext context)
	{
		base.GetObjectData(info, context);
		info.AddValue("settings", _settings);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.InvalidPrinterException" /> class.</summary>
	/// <param name="settings">A <see cref="T:System.Drawing.Printing.PrinterSettings" /> that specifies the settings for a printer. </param>
	public InvalidPrinterException(PrinterSettings settings)
		: base(GenerateMessage(settings))
	{
		_settings = settings;
	}

	private static string GenerateMessage(PrinterSettings settings)
	{
		if (settings.IsDefaultPrinter)
		{
			return global::SR.Format("No printers are installed.");
		}
		try
		{
			return global::SR.Format("Settings to access printer '{0}' are not valid.", settings.PrinterName);
		}
		catch (SecurityException)
		{
			return global::SR.Format("Settings to access printer '{0}' are not valid.", global::SR.Format("(printer name protected due to security restrictions)"));
		}
	}
}
