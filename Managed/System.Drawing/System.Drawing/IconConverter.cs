using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace System.Drawing;

/// <summary>Converts an <see cref="T:System.Drawing.Icon" /> object from one data type to another. Access this class through the <see cref="T:System.ComponentModel.TypeDescriptor" /> object.</summary>
/// <filterpriority>1</filterpriority>
public class IconConverter : ExpandableObjectConverter
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.IconConverter" /> class.</summary>
	public IconConverter()
	{
	}

	/// <summary>Determines whether this <see cref="T:System.Drawing.IconConverter" /> can convert an instance of a specified type to an <see cref="T:System.Drawing.Icon" />, using the specified context.</summary>
	/// <returns>This method returns true if this <see cref="T:System.Drawing.IconConverter" /> can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="sourceType">A <see cref="T:System.Type" /> that specifies the type you want to convert from. </param>
	/// <filterpriority>1</filterpriority>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(byte[]))
		{
			return true;
		}
		return false;
	}

	/// <summary>Determines whether this <see cref="T:System.Drawing.IconConverter" /> can convert an <see cref="T:System.Drawing.Icon" /> to an instance of a specified type, using the specified context.</summary>
	/// <returns>This method returns true if this <see cref="T:System.Drawing.IconConverter" /> can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="destinationType">A <see cref="T:System.Type" /> that specifies the type you want to convert to. </param>
	/// <filterpriority>1</filterpriority>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(byte[]) || destinationType == typeof(string))
		{
			return true;
		}
		return false;
	}

	/// <summary>Converts a specified object to an <see cref="T:System.Drawing.Icon" />.</summary>
	/// <returns>If this method succeeds, it returns the <see cref="T:System.Drawing.Icon" /> that it created by converting the specified object. Otherwise, it throws an exception.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> that holds information about a specific culture. </param>
	/// <param name="value">The <see cref="T:System.Object" /> to be converted. </param>
	/// <exception cref="T:System.NotSupportedException">The conversion could not be performed.</exception>
	/// <filterpriority>1</filterpriority>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (!(value is byte[] buffer))
		{
			return base.ConvertFrom(context, culture, value);
		}
		return new Icon(new MemoryStream(buffer));
	}

	/// <summary>Converts an <see cref="T:System.Drawing.Icon" /> (or an object that can be cast to an <see cref="T:System.Drawing.Icon" />) to a specified type.</summary>
	/// <returns>This method returns the converted object.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> object that specifies formatting conventions used by a particular culture. </param>
	/// <param name="value">The object to convert. This object should be of type icon or some type that can be cast to <see cref="T:System.Drawing.Icon" />. </param>
	/// <param name="destinationType">The type to convert the icon to. </param>
	/// <exception cref="T:System.NotSupportedException">The conversion could not be performed.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (value is Icon && destinationType == typeof(string))
		{
			return value.ToString();
		}
		if (value == null && destinationType == typeof(string))
		{
			return "(none)";
		}
		if (CanConvertTo(null, destinationType))
		{
			using (MemoryStream memoryStream = new MemoryStream())
			{
				((Icon)value).Save(memoryStream);
				return memoryStream.ToArray();
			}
		}
		return new NotSupportedException("IconConverter can not convert from " + value.GetType());
	}
}
