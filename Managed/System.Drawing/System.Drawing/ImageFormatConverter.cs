using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Drawing.Imaging;
using System.Globalization;
using System.Reflection;

namespace System.Drawing;

/// <summary>
///   <see cref="T:System.Drawing.ImageFormatConverter" /> is a class that can be used to convert <see cref="T:System.Drawing.Imaging.ImageFormat" /> objects from one data type to another. Access this class through the <see cref="T:System.ComponentModel.TypeDescriptor" /> object.</summary>
/// <filterpriority>1</filterpriority>
public class ImageFormatConverter : TypeConverter
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.ImageFormatConverter" /> class.</summary>
	public ImageFormatConverter()
	{
	}

	/// <summary>Indicates whether this converter can convert an object in the specified source type to the native type of the converter.</summary>
	/// <returns>This method returns true if this object can perform the conversion.</returns>
	/// <param name="context">A formatter context. This object can be used to get more information about the environment this converter is being called from. This may be null, so you should always check. Also, properties on the context object may also return null. </param>
	/// <param name="sourceType">The type you want to convert from. </param>
	/// <filterpriority>1</filterpriority>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Gets a value indicating whether this converter can convert an object to the specified destination type using the context.</summary>
	/// <returns>This method returns true if this object can perform the conversion.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that specifies the context for this type conversion. </param>
	/// <param name="destinationType">The <see cref="T:System.Type" /> that represents the type to which you want to convert this <see cref="T:System.Drawing.Imaging.ImageFormat" /> object. </param>
	/// <filterpriority>1</filterpriority>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(string))
		{
			return true;
		}
		if (destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <summary>Converts the specified object to an <see cref="T:System.Drawing.Imaging.ImageFormat" /> object.</summary>
	/// <returns>The converted object.</returns>
	/// <param name="context">A formatter context. This object can be used to get more information about the environment this converter is being called from. This may be null, so you should always check. Also, properties on the context object may also return null. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> object that specifies formatting conventions for a particular culture. </param>
	/// <param name="value">The object to convert. </param>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be completed.</exception>
	/// <filterpriority>1</filterpriority>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (!(value is string text))
		{
			return base.ConvertFrom(context, culture, value);
		}
		if (text[0] == '[')
		{
			if (text.Equals(ImageFormat.Bmp.ToString()))
			{
				return ImageFormat.Bmp;
			}
			if (text.Equals(ImageFormat.Emf.ToString()))
			{
				return ImageFormat.Emf;
			}
			if (text.Equals(ImageFormat.Exif.ToString()))
			{
				return ImageFormat.Exif;
			}
			if (text.Equals(ImageFormat.Gif.ToString()))
			{
				return ImageFormat.Gif;
			}
			if (text.Equals(ImageFormat.Icon.ToString()))
			{
				return ImageFormat.Icon;
			}
			if (text.Equals(ImageFormat.Jpeg.ToString()))
			{
				return ImageFormat.Jpeg;
			}
			if (text.Equals(ImageFormat.MemoryBmp.ToString()))
			{
				return ImageFormat.MemoryBmp;
			}
			if (text.Equals(ImageFormat.Png.ToString()))
			{
				return ImageFormat.Png;
			}
			if (text.Equals(ImageFormat.Tiff.ToString()))
			{
				return ImageFormat.Tiff;
			}
			if (text.Equals(ImageFormat.Wmf.ToString()))
			{
				return ImageFormat.Wmf;
			}
		}
		else
		{
			if (string.Compare(text, "Bmp", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return ImageFormat.Bmp;
			}
			if (string.Compare(text, "Emf", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return ImageFormat.Emf;
			}
			if (string.Compare(text, "Exif", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return ImageFormat.Exif;
			}
			if (string.Compare(text, "Gif", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return ImageFormat.Gif;
			}
			if (string.Compare(text, "Icon", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return ImageFormat.Icon;
			}
			if (string.Compare(text, "Jpeg", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return ImageFormat.Jpeg;
			}
			if (string.Compare(text, "MemoryBmp", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return ImageFormat.MemoryBmp;
			}
			if (string.Compare(text, "Png", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return ImageFormat.Png;
			}
			if (string.Compare(text, "Tiff", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return ImageFormat.Tiff;
			}
			if (string.Compare(text, "Wmf", StringComparison.OrdinalIgnoreCase) == 0)
			{
				return ImageFormat.Wmf;
			}
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Converts the specified object to the specified type.</summary>
	/// <returns>The converted object.</returns>
	/// <param name="context">A formatter context. This object can be used to get more information about the environment this converter is being called from. This may be null, so you should always check. Also, properties on the context object may also return null. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> object that specifies formatting conventions for a particular culture. </param>
	/// <param name="value">The object to convert. </param>
	/// <param name="destinationType">The type to convert the object to. </param>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be completed.</exception>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationType" /> is null.</exception>
	/// <filterpriority>1</filterpriority>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (value is ImageFormat)
		{
			ImageFormat imageFormat = (ImageFormat)value;
			string text = null;
			if (imageFormat.Guid.Equals(ImageFormat.Bmp.Guid))
			{
				text = "Bmp";
			}
			else if (imageFormat.Guid.Equals(ImageFormat.Emf.Guid))
			{
				text = "Emf";
			}
			else if (imageFormat.Guid.Equals(ImageFormat.Exif.Guid))
			{
				text = "Exif";
			}
			else if (imageFormat.Guid.Equals(ImageFormat.Gif.Guid))
			{
				text = "Gif";
			}
			else if (imageFormat.Guid.Equals(ImageFormat.Icon.Guid))
			{
				text = "Icon";
			}
			else if (imageFormat.Guid.Equals(ImageFormat.Jpeg.Guid))
			{
				text = "Jpeg";
			}
			else if (imageFormat.Guid.Equals(ImageFormat.MemoryBmp.Guid))
			{
				text = "MemoryBmp";
			}
			else if (imageFormat.Guid.Equals(ImageFormat.Png.Guid))
			{
				text = "Png";
			}
			else if (imageFormat.Guid.Equals(ImageFormat.Tiff.Guid))
			{
				text = "Tiff";
			}
			else if (imageFormat.Guid.Equals(ImageFormat.Wmf.Guid))
			{
				text = "Wmf";
			}
			if (destinationType == typeof(string))
			{
				if (text == null)
				{
					return imageFormat.ToString();
				}
				return text;
			}
			if (destinationType == typeof(InstanceDescriptor))
			{
				if (text != null)
				{
					return new InstanceDescriptor(typeof(ImageFormat).GetTypeInfo().GetProperty(text), null);
				}
				return new InstanceDescriptor(typeof(ImageFormat).GetTypeInfo().GetConstructor(new Type[1] { typeof(Guid) }), new object[1] { imageFormat.Guid });
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Gets a collection that contains a set of standard values for the data type this validator is designed for. Returns null if the data type does not support a standard set of values.</summary>
	/// <returns>A collection that contains a standard set of valid values, or null. The default implementation always returns null.</returns>
	/// <param name="context">A formatter context. This object can be used to get more information about the environment this converter is being called from. This may be null, so you should always check. Also, properties on the context object may also return null. </param>
	/// <filterpriority>1</filterpriority>
	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		return new StandardValuesCollection(new ImageFormat[10]
		{
			ImageFormat.MemoryBmp,
			ImageFormat.Bmp,
			ImageFormat.Emf,
			ImageFormat.Wmf,
			ImageFormat.Gif,
			ImageFormat.Jpeg,
			ImageFormat.Png,
			ImageFormat.Tiff,
			ImageFormat.Exif,
			ImageFormat.Icon
		});
	}

	/// <summary>Indicates whether this object supports a standard set of values that can be picked from a list.</summary>
	/// <returns>This method returns true if the <see cref="Overload:System.Drawing.ImageFormatConverter.GetStandardValues" /> method should be called to find a common set of values the object supports.</returns>
	/// <param name="context">A type descriptor through which additional context can be provided. </param>
	/// <filterpriority>1</filterpriority>
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return true;
	}
}
