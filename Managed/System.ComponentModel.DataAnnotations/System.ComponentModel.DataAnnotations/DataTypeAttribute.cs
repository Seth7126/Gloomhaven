using System.Globalization;

namespace System.ComponentModel.DataAnnotations;

/// <summary>Specifies the name of an additional type to associate with a data field.</summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class DataTypeAttribute : ValidationAttribute
{
	private static string[] _dataTypeStrings = Enum.GetNames(typeof(DataType));

	/// <summary>Gets the type that is associated with the data field.</summary>
	/// <returns>One of the <see cref="T:System.ComponentModel.DataAnnotations.DataType" /> values.</returns>
	public DataType DataType { get; private set; }

	/// <summary>Gets the name of custom field template that is associated with the data field.</summary>
	/// <returns>The name of the custom field template that is associated with the data field.</returns>
	public string CustomDataType { get; private set; }

	/// <summary>Gets a data-field display format.</summary>
	/// <returns>The data-field display format.</returns>
	public DisplayFormatAttribute DisplayFormat { get; protected set; }

	/// <summary>Returns the name of the type that is associated with the data field.</summary>
	/// <returns>The name of the type associated with the data field.</returns>
	public virtual string GetDataTypeName()
	{
		EnsureValidDataType();
		if (DataType == DataType.Custom)
		{
			return CustomDataType;
		}
		return _dataTypeStrings[(int)DataType];
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.DataTypeTypeAttribute" /> class by using the specified type name.</summary>
	/// <param name="dataType">The name of the type to associate with the data field.</param>
	public DataTypeAttribute(DataType dataType)
	{
		DataType = dataType;
		switch (dataType)
		{
		case DataType.Date:
			DisplayFormat = new DisplayFormatAttribute();
			DisplayFormat.DataFormatString = "{0:d}";
			DisplayFormat.ApplyFormatInEditMode = true;
			break;
		case DataType.Time:
			DisplayFormat = new DisplayFormatAttribute();
			DisplayFormat.DataFormatString = "{0:t}";
			DisplayFormat.ApplyFormatInEditMode = true;
			break;
		case DataType.Currency:
			DisplayFormat = new DisplayFormatAttribute();
			DisplayFormat.DataFormatString = "{0:C}";
			break;
		case DataType.Duration:
		case DataType.PhoneNumber:
			break;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.DataTypeTypeAttribute" /> class by using the specified field template name.</summary>
	/// <param name="customDataType">The name of the custom field template to associate with the data field.</param>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="customDataType" /> is null or an empty string (""). </exception>
	public DataTypeAttribute(string customDataType)
		: this(DataType.Custom)
	{
		CustomDataType = customDataType;
	}

	/// <summary>Checks that the value of the data field is valid.</summary>
	/// <returns>true always.</returns>
	/// <param name="value">The data field value to validate.</param>
	public override bool IsValid(object value)
	{
		EnsureValidDataType();
		return true;
	}

	private void EnsureValidDataType()
	{
		if (DataType == DataType.Custom && string.IsNullOrEmpty(CustomDataType))
		{
			throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, "The custom DataType string cannot be null or empty."));
		}
	}
}
