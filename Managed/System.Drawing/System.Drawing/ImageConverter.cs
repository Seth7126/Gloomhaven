using System.ComponentModel;
using System.Globalization;
using System.IO;

namespace System.Drawing;

/// <summary>
///   <see cref="T:System.Drawing.ImageConverter" />  is a class that can be used to convert <see cref="T:System.Drawing.Image" /> objects from one data type to another. Access this class through the <see cref="T:System.ComponentModel.TypeDescriptor" /> object.</summary>
/// <filterpriority>1</filterpriority>
public class ImageConverter : TypeConverter
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.ImageConverter" /> class.</summary>
	public ImageConverter()
	{
	}

	/// <summary>Determines whether this <see cref="T:System.Drawing.ImageConverter" /> can convert an instance of a specified type to an <see cref="T:System.Drawing.Image" />, using the specified context.</summary>
	/// <returns>This method returns true if this <see cref="T:System.Drawing.ImageConverter" /> can perform the conversion; otherwise, false.</returns>
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

	/// <summary>Determines whether this <see cref="T:System.Drawing.ImageConverter" /> can convert an <see cref="T:System.Drawing.Image" /> to an instance of a specified type, using the specified context.</summary>
	/// <returns>This method returns true if this <see cref="T:System.Drawing.ImageConverter" /> can perform the conversion; otherwise, false.</returns>
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

	/// <summary>Converts a specified object to an <see cref="T:System.Drawing.Image" />.</summary>
	/// <returns>If this method succeeds, it returns the <see cref="T:System.Drawing.Image" /> that it created by converting the specified object. Otherwise, it throws an exception.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> that holds information about a specific culture. </param>
	/// <param name="value">The <see cref="T:System.Object" /> to be converted. </param>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be completed.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	///   <IPermission class="System.Security.Permissions.ReflectionPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="MemberAccess" />
	/// </PermissionSet>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (!(value is byte[] buffer))
		{
			return base.ConvertFrom(context, culture, value);
		}
		return Image.FromStream(new MemoryStream(buffer));
	}

	/// <summary>Converts an <see cref="T:System.Drawing.Image" /> (or an object that can be cast to an <see cref="T:System.Drawing.Image" />) to the specified type.</summary>
	/// <returns>This method returns the converted object.</returns>
	/// <param name="context">A formatter context. This object can be used to get more information about the environment this converter is being called from. This may be null, so you should always check. Also, properties on the context object may also return null. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> object that specifies formatting conventions used by a particular culture. </param>
	/// <param name="value">The <see cref="T:System.Drawing.Image" /> to convert. </param>
	/// <param name="destinationType">The <see cref="T:System.Type" /> to convert the <see cref="T:System.Drawing.Image" /> to. </param>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be completed.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (value == null)
		{
			return "(none)";
		}
		if (value is Image)
		{
			if (destinationType == typeof(string))
			{
				return value.ToString();
			}
			if (CanConvertTo(null, destinationType))
			{
				using (MemoryStream memoryStream = new MemoryStream())
				{
					((Image)value).Save(memoryStream, ((Image)value).RawFormat);
					return memoryStream.ToArray();
				}
			}
		}
		throw new NotSupportedException(global::Locale.GetText("ImageConverter can not convert from type '{0}'.", value.GetType()));
	}

	/// <summary>Gets the set of properties for this type.</summary>
	/// <returns>The set of properties that should be exposed for this data type. If no properties should be exposed, this can return null. The default implementation always returns null.</returns>
	/// <param name="context">A type descriptor through which additional context can be provided. </param>
	/// <param name="value">The value of the object to get the properties for. </param>
	/// <param name="attributes">An array of <see cref="T:System.Attribute" /> objects that describe the properties.</param>
	/// <filterpriority>1</filterpriority>
	public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
	{
		return TypeDescriptor.GetProperties(typeof(Image), attributes);
	}

	/// <summary>Indicates whether this object supports properties. By default, this is false.</summary>
	/// <returns>This method returns true if the <see cref="Overload:System.Drawing.ImageConverter.GetProperties" /> method should be called to find the properties of this object.</returns>
	/// <param name="context">A type descriptor through which additional context can be provided. </param>
	/// <filterpriority>1</filterpriority>
	public override bool GetPropertiesSupported(ITypeDescriptorContext context)
	{
		return true;
	}
}
