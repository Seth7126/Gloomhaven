using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace System.Drawing;

/// <summary>Converts <see cref="T:System.Drawing.SizeF" /> objects from one type to another.</summary>
public class SizeFConverter : TypeConverter
{
	private static readonly string[] s_propertySort = new string[2] { "Width", "Height" };

	/// <summary>Returns a value indicating whether the converter can convert from the type specified to the <see cref="T:System.Drawing.SizeF" /> type, using the specified context.</summary>
	/// <returns>true to indicate the conversion can be performed; otherwise, false. </returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> through which additional context can be supplied.</param>
	/// <param name="sourceType">A <see cref="T:System.Type" /> the represents the type you wish to convert from.</param>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Returns a value indicating whether the <see cref="T:System.Drawing.SizeFConverter" /> can convert a <see cref="T:System.Drawing.SizeF" /> to the specified type.</summary>
	/// <returns>true if this converter can perform the conversion otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> through which additional context can be supplied.</param>
	/// <param name="destinationType">A <see cref="T:System.Type" /> that represents the type you want to convert from.</param>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	/// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">The <see cref="T:System.Globalization.CultureInfo" /> to use as the current culture. </param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert. </param>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string text)
		{
			string text2 = text.Trim();
			if (text2.Length == 0)
			{
				return null;
			}
			if (culture == null)
			{
				culture = CultureInfo.CurrentCulture;
			}
			char separator = culture.TextInfo.ListSeparator[0];
			string[] array = text2.Split(separator);
			float[] array2 = new float[array.Length];
			TypeConverter converter = TypeDescriptor.GetConverter(typeof(float));
			for (int i = 0; i < array2.Length; i++)
			{
				array2[i] = (float)converter.ConvertFromString(context, culture, array[i]);
			}
			if (array2.Length == 2)
			{
				return new SizeF(array2[0], array2[1]);
			}
			throw new ArgumentException(global::SR.Format("Text \"{0}\" cannot be parsed. The expected text format is \"{1}\".", text2, "Width,Height"));
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <returns>An <see cref="T:System.Object" /> that represents the converted value.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" />. If null is passed, the current culture is assumed. </param>
	/// <param name="value">The <see cref="T:System.Object" /> to convert. </param>
	/// <param name="destinationType">The <see cref="T:System.Type" /> to convert the <paramref name="value" /> parameter to. </param>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (destinationType == typeof(string) && value is SizeF sizeF)
		{
			if (culture == null)
			{
				culture = CultureInfo.CurrentCulture;
			}
			string separator = culture.TextInfo.ListSeparator + " ";
			TypeConverter converter = TypeDescriptor.GetConverter(typeof(float));
			string[] array = new string[2];
			int num = 0;
			array[num++] = converter.ConvertToString(context, culture, sizeF.Width);
			array[num++] = converter.ConvertToString(context, culture, sizeF.Height);
			return string.Join(separator, array);
		}
		if (destinationType == typeof(InstanceDescriptor) && value is SizeF sizeF2)
		{
			ConstructorInfo constructor = typeof(SizeF).GetConstructor(new Type[2]
			{
				typeof(float),
				typeof(float)
			});
			if (constructor != null)
			{
				return new InstanceDescriptor(constructor, new object[2] { sizeF2.Width, sizeF2.Height });
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	/// <summary>Creates an instance of a <see cref="T:System.Drawing.SizeF" /> with the specified property values using the specified context.</summary>
	/// <returns>An <see cref="T:System.Object" /> representing the new <see cref="T:System.Drawing.SizeF" />, or null if the object cannot be created.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> through which additional context can be supplied.</param>
	/// <param name="propertyValues">An <see cref="T:System.Collections.IDictionary" /> containing property names and values.</param>
	public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
	{
		if (propertyValues == null)
		{
			throw new ArgumentNullException("propertyValues");
		}
		object obj = propertyValues["Width"];
		object obj2 = propertyValues["Height"];
		if (obj == null || obj2 == null || !(obj is float) || !(obj2 is float))
		{
			throw new ArgumentException(global::SR.Format("IDictionary parameter contains at least one entry that is not valid. Ensure all values are consistent with the object's properties."));
		}
		return new SizeF((float)obj, (float)obj2);
	}

	/// <summary>Returns a value indicating whether changing a value on this object requires a call to the <see cref="Overload:System.Drawing.SizeFConverter.CreateInstance" /> method to create a new value.</summary>
	/// <returns>Always returns true.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. This may be null.</param>
	public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	/// <summary>Retrieves a set of properties for the <see cref="T:System.Drawing.SizeF" /> type using the specified context and attributes.</summary>
	/// <returns>A <see cref="T:System.ComponentModel.PropertyDescriptorCollection" /> containing the properties.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> through which additional context can be supplied.</param>
	/// <param name="value">The <see cref="T:System.Object" /> to return properties for.</param>
	/// <param name="attributes">An array of <see cref="T:System.Attribute" /> objects that describe the properties.</param>
	public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
	{
		return TypeDescriptor.GetProperties(typeof(SizeF), attributes).Sort(s_propertySort);
	}

	/// <summary>Returns whether the <see cref="T:System.Drawing.SizeF" /> type supports properties.</summary>
	/// <returns>Always returns true.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> through which additional context can be supplied.</param>
	public override bool GetPropertiesSupported(ITypeDescriptorContext context)
	{
		return true;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.SizeFConverter" /> class.</summary>
	public SizeFConverter()
	{
	}
}
