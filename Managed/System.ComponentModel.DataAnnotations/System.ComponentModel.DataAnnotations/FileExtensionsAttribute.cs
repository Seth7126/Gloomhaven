using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace System.ComponentModel.DataAnnotations;

/// <summary>Validates file name extensions.</summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public sealed class FileExtensionsAttribute : DataTypeAttribute
{
	private string _extensions;

	/// <summary>Gets or sets the file name extensions.</summary>
	/// <returns>The file name extensions, or the default file extensions (".png", ".jpg", ".jpeg", and ".gif") if the property is not set.</returns>
	public string Extensions
	{
		get
		{
			if (!string.IsNullOrWhiteSpace(_extensions))
			{
				return _extensions;
			}
			return "png,jpg,jpeg,gif";
		}
		set
		{
			_extensions = value;
		}
	}

	private string ExtensionsFormatted => ExtensionsParsed.Aggregate((string left, string right) => left + ", " + right);

	private string ExtensionsNormalized => Extensions.Replace(" ", "").Replace(".", "").ToLowerInvariant();

	private IEnumerable<string> ExtensionsParsed => from e in ExtensionsNormalized.Split(',')
		select "." + e;

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.FileExtensionsAttribute" /> class.</summary>
	public FileExtensionsAttribute()
		: base(DataType.Upload)
	{
		base.DefaultErrorMessage = "The {0} field only accepts files with the following extensions: {1}";
	}

	/// <summary>Applies formatting to an error message, based on the data field where the error occurred.</summary>
	/// <returns>The formatted error message.</returns>
	/// <param name="name">The name of the field that caused the validation failure.</param>
	public override string FormatErrorMessage(string name)
	{
		return string.Format(CultureInfo.CurrentCulture, base.ErrorMessageString, name, ExtensionsFormatted);
	}

	/// <summary>Checks that the specified file name extension or extensions is valid.</summary>
	/// <returns>true if the file name extension is valid; otherwise, false.</returns>
	/// <param name="value">A comma delimited list of valid file extensions.</param>
	public override bool IsValid(object value)
	{
		if (value == null)
		{
			return true;
		}
		if (value is string fileName)
		{
			return ValidateExtension(fileName);
		}
		return false;
	}

	private bool ValidateExtension(string fileName)
	{
		try
		{
			return ExtensionsParsed.Contains(Path.GetExtension(fileName).ToLowerInvariant());
		}
		catch (ArgumentException)
		{
			return false;
		}
	}
}
