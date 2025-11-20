using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace System.Drawing;

/// <summary>Converts colors from one data type to another. Access this class through the <see cref="T:System.ComponentModel.TypeDescriptor" />.</summary>
/// <filterpriority>1</filterpriority>
public class ColorConverter : TypeConverter
{
	private class ColorComparer : IComparer
	{
		public int Compare(object left, object right)
		{
			Color color = (Color)left;
			Color color2 = (Color)right;
			return string.Compare(color.Name, color2.Name, ignoreCase: false, CultureInfo.InvariantCulture);
		}
	}

	private static string ColorConstantsLock = "colorConstants";

	private static Hashtable colorConstants;

	private static string SystemColorConstantsLock = "systemColorConstants";

	private static Hashtable systemColorConstants;

	private static string ValuesLock = "values";

	private static StandardValuesCollection values;

	private static Hashtable Colors
	{
		get
		{
			if (colorConstants == null)
			{
				lock (ColorConstantsLock)
				{
					if (colorConstants == null)
					{
						Hashtable hash = new Hashtable(StringComparer.OrdinalIgnoreCase);
						FillConstants(hash, typeof(Color));
						colorConstants = hash;
					}
				}
			}
			return colorConstants;
		}
	}

	private static Hashtable SystemColors
	{
		get
		{
			if (systemColorConstants == null)
			{
				lock (SystemColorConstantsLock)
				{
					if (systemColorConstants == null)
					{
						Hashtable hash = new Hashtable(StringComparer.OrdinalIgnoreCase);
						FillConstants(hash, typeof(SystemColors));
						systemColorConstants = hash;
					}
				}
			}
			return systemColorConstants;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.ColorConverter" /> class.</summary>
	public ColorConverter()
	{
	}

	/// <summary>Determines if this converter can convert an object in the given source type to the native type of the converter.</summary>
	/// <returns>true if this object can perform the conversion; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. You can use this object to get additional information about the environment from which this converter is being invoked. </param>
	/// <param name="sourceType">The type from which you want to convert. </param>
	/// <filterpriority>1</filterpriority>
	public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
	{
		if (sourceType == typeof(string))
		{
			return true;
		}
		return base.CanConvertFrom(context, sourceType);
	}

	/// <summary>Returns a value indicating whether this converter can convert an object to the given destination type using the context.</summary>
	/// <returns>true if this converter can perform the operation; otherwise, false.</returns>
	/// <param name="context">An <see cref="T:System.ComponentModel.ITypeDescriptorContext" /> that provides a format context. </param>
	/// <param name="destinationType">A <see cref="T:System.Type" /> that represents the type to which you want to convert. </param>
	/// <filterpriority>1</filterpriority>
	public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
	{
		if (destinationType == typeof(InstanceDescriptor))
		{
			return true;
		}
		return base.CanConvertTo(context, destinationType);
	}

	internal static object GetNamedColor(string name)
	{
		object obj = null;
		obj = Colors[name];
		if (obj != null)
		{
			return obj;
		}
		return SystemColors[name];
	}

	/// <summary>Converts the given object to the converter's native type.</summary>
	/// <returns>An <see cref="T:System.Object" /> representing the converted value.</returns>
	/// <param name="context">A <see cref="T:System.ComponentModel.TypeDescriptor" /> that provides a format context. You can use this object to get additional information about the environment from which this converter is being invoked. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> that specifies the culture to represent the color. </param>
	/// <param name="value">The object to convert. </param>
	/// <exception cref="T:System.ArgumentException">The conversion cannot be performed.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
	{
		if (value is string text)
		{
			object obj = null;
			string text2 = text.Trim();
			if (text2.Length == 0)
			{
				obj = Color.Empty;
			}
			else
			{
				obj = GetNamedColor(text2);
				if (obj == null)
				{
					if (culture == null)
					{
						culture = CultureInfo.CurrentCulture;
					}
					char c = culture.TextInfo.ListSeparator[0];
					bool flag = true;
					TypeConverter converter = TypeDescriptor.GetConverter(typeof(int));
					if (text2.IndexOf(c) == -1)
					{
						if (text2.Length >= 2 && (text2[0] == '\'' || text2[0] == '"') && text2[0] == text2[text2.Length - 1])
						{
							obj = Color.FromName(text2.Substring(1, text2.Length - 2));
							flag = false;
						}
						else if ((text2.Length == 7 && text2[0] == '#') || (text2.Length == 8 && (text2.StartsWith("0x") || text2.StartsWith("0X"))) || (text2.Length == 8 && (text2.StartsWith("&h") || text2.StartsWith("&H"))))
						{
							obj = Color.FromArgb(-16777216 | (int)converter.ConvertFromString(context, culture, text2));
						}
					}
					if (obj == null)
					{
						string[] array = text2.Split(new char[1] { c });
						int[] array2 = new int[array.Length];
						for (int i = 0; i < array2.Length; i++)
						{
							array2[i] = (int)converter.ConvertFromString(context, culture, array[i]);
						}
						switch (array2.Length)
						{
						case 1:
							obj = Color.FromArgb(array2[0]);
							break;
						case 3:
							obj = Color.FromArgb(array2[0], array2[1], array2[2]);
							break;
						case 4:
							obj = Color.FromArgb(array2[0], array2[1], array2[2], array2[3]);
							break;
						}
						flag = true;
					}
					if (obj != null && flag)
					{
						int num = ((Color)obj).ToArgb();
						foreach (Color value2 in Colors.Values)
						{
							if (value2.ToArgb() == num)
							{
								obj = value2;
								break;
							}
						}
					}
				}
				if (obj == null)
				{
					throw new ArgumentException(global::SR.Format("Color '{0}' is not valid.", text2));
				}
			}
			return obj;
		}
		return base.ConvertFrom(context, culture, value);
	}

	/// <summary>Converts the specified object to another type. </summary>
	/// <returns>An <see cref="T:System.Object" /> representing the converted value.</returns>
	/// <param name="context">A formatter context. Use this object to extract additional information about the environment from which this converter is being invoked. Always check whether this value is null. Also, properties on the context object may return null. </param>
	/// <param name="culture">A <see cref="T:System.Globalization.CultureInfo" /> that specifies the culture to represent the color. </param>
	/// <param name="value">The object to convert. </param>
	/// <param name="destinationType">The type to convert the object to. </param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="destinationtype" /> is null.</exception>
	/// <exception cref="T:System.NotSupportedException">The conversion cannot be performed.</exception>
	/// <filterpriority>1</filterpriority>
	/// <PermissionSet>
	///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
	/// </PermissionSet>
	public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
	{
		if (destinationType == null)
		{
			throw new ArgumentNullException("destinationType");
		}
		if (value is Color)
		{
			if (destinationType == typeof(string))
			{
				Color color = (Color)value;
				if (color == Color.Empty)
				{
					return string.Empty;
				}
				if (color.IsKnownColor)
				{
					return color.Name;
				}
				if (color.IsNamedColor)
				{
					return "'" + color.Name + "'";
				}
				if (culture == null)
				{
					culture = CultureInfo.CurrentCulture;
				}
				string separator = culture.TextInfo.ListSeparator + " ";
				TypeConverter converter = TypeDescriptor.GetConverter(typeof(int));
				int num = 0;
				string[] array;
				if (color.A < byte.MaxValue)
				{
					array = new string[4];
					array[num++] = converter.ConvertToString(context, culture, color.A);
				}
				else
				{
					array = new string[3];
				}
				array[num++] = converter.ConvertToString(context, culture, color.R);
				array[num++] = converter.ConvertToString(context, culture, color.G);
				array[num++] = converter.ConvertToString(context, culture, color.B);
				return string.Join(separator, array);
			}
			if (destinationType == typeof(InstanceDescriptor))
			{
				MemberInfo memberInfo = null;
				object[] arguments = null;
				Color color2 = (Color)value;
				if (color2.IsEmpty)
				{
					memberInfo = typeof(Color).GetField("Empty");
				}
				else if (color2.IsSystemColor)
				{
					memberInfo = typeof(SystemColors).GetProperty(color2.Name);
				}
				else if (color2.IsKnownColor)
				{
					memberInfo = typeof(Color).GetProperty(color2.Name);
				}
				else if (color2.A != byte.MaxValue)
				{
					memberInfo = typeof(Color).GetMethod("FromArgb", new Type[4]
					{
						typeof(int),
						typeof(int),
						typeof(int),
						typeof(int)
					});
					arguments = new object[4] { color2.A, color2.R, color2.G, color2.B };
				}
				else if (color2.IsNamedColor)
				{
					memberInfo = typeof(Color).GetMethod("FromName", new Type[1] { typeof(string) });
					arguments = new object[1] { color2.Name };
				}
				else
				{
					memberInfo = typeof(Color).GetMethod("FromArgb", new Type[3]
					{
						typeof(int),
						typeof(int),
						typeof(int)
					});
					arguments = new object[3] { color2.R, color2.G, color2.B };
				}
				if (memberInfo != null)
				{
					return new InstanceDescriptor(memberInfo, arguments);
				}
				return null;
			}
		}
		return base.ConvertTo(context, culture, value, destinationType);
	}

	private static void FillConstants(Hashtable hash, Type enumType)
	{
		MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.Static;
		PropertyInfo[] properties = enumType.GetProperties();
		foreach (PropertyInfo propertyInfo in properties)
		{
			if (propertyInfo.PropertyType == typeof(Color))
			{
				MethodInfo getMethod = propertyInfo.GetGetMethod();
				if (getMethod != null && (getMethod.Attributes & methodAttributes) == methodAttributes)
				{
					object[] index = null;
					hash[propertyInfo.Name] = propertyInfo.GetValue(null, index);
				}
			}
		}
	}

	/// <summary>Retrieves a collection containing a set of standard values for the data type for which this validator is designed. This will return null if the data type does not support a standard set of values.</summary>
	/// <returns>A collection containing null or a standard set of valid values. The default implementation always returns null.</returns>
	/// <param name="context">A formatter context. Use this object to extract additional information about the environment from which this converter is being invoked. Always check whether this value is null. Also, properties on the context object may return null. </param>
	/// <filterpriority>1</filterpriority>
	public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
	{
		if (values == null)
		{
			lock (ValuesLock)
			{
				if (values == null)
				{
					ArrayList arrayList = new ArrayList();
					arrayList.AddRange(Colors.Values);
					arrayList.AddRange(SystemColors.Values);
					int num = arrayList.Count;
					for (int i = 0; i < num - 1; i++)
					{
						for (int j = i + 1; j < num; j++)
						{
							if (arrayList[i].Equals(arrayList[j]))
							{
								arrayList.RemoveAt(j);
								num--;
								j--;
							}
						}
					}
					arrayList.Sort(0, arrayList.Count, new ColorComparer());
					values = new StandardValuesCollection(arrayList.ToArray());
				}
			}
		}
		return values;
	}

	/// <summary>Determines if this object supports a standard set of values that can be chosen from a list.</summary>
	/// <returns>true if <see cref="Overload:System.Drawing.ColorConverter.GetStandardValues" /> must be called to find a common set of values the object supports; otherwise, false.</returns>
	/// <param name="context">A <see cref="T:System.ComponentModel.TypeDescriptor" /> through which additional context can be provided. </param>
	/// <filterpriority>1</filterpriority>
	public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
	{
		return true;
	}
}
