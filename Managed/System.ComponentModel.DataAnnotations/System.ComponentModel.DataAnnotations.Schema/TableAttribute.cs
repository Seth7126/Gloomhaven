using System.Globalization;

namespace System.ComponentModel.DataAnnotations.Schema;

/// <summary>Specifies the database table that a class is mapped to.</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class TableAttribute : Attribute
{
	private readonly string _name;

	private string _schema;

	/// <summary>Gets the name of the table the class is mapped to.</summary>
	/// <returns>The name of the table the class is mapped to.</returns>
	public string Name => _name;

	/// <summary>Gets or sets the schema of the table the class is mapped to.</summary>
	/// <returns>The schema of the table the class is mapped to.</returns>
	public string Schema
	{
		get
		{
			return _schema;
		}
		set
		{
			if (string.IsNullOrWhiteSpace(value))
			{
				throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The argument '{0}' cannot be null, empty or contain only white space.", "value"));
			}
			_schema = value;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.Schema.TableAttribute" /> class using the specified name of the table.</summary>
	/// <param name="name">The name of the table.</param>
	public TableAttribute(string name)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, "The argument '{0}' cannot be null, empty or contain only white space.", "name"));
		}
		_name = name;
	}
}
