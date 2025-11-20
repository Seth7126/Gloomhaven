namespace System.ComponentModel.DataAnnotations;

/// <summary>Represents an enumeration of the data types associated with data fields and parameters. </summary>
public enum DataType
{
	/// <summary>Represents a custom data type.</summary>
	Custom,
	/// <summary>Represents an instant in time, expressed as a date and time of day.</summary>
	DateTime,
	/// <summary>Represents a date value.</summary>
	Date,
	/// <summary>Represents a time value.</summary>
	Time,
	/// <summary>Represents a continuous time during which an object exists.</summary>
	Duration,
	/// <summary>Represents a phone number value.</summary>
	PhoneNumber,
	/// <summary>Represents a currency value.</summary>
	Currency,
	/// <summary>Represents text that is displayed.</summary>
	Text,
	/// <summary>Represents an HTML file.</summary>
	Html,
	/// <summary>Represents multi-line text.</summary>
	MultilineText,
	/// <summary>Represents an e-mail address.</summary>
	EmailAddress,
	/// <summary>Represent a password value.</summary>
	Password,
	/// <summary>Represents a URL value.</summary>
	Url,
	/// <summary>Represents a URL to an image.</summary>
	ImageUrl,
	/// <summary>Represents a credit card number.</summary>
	CreditCard,
	/// <summary>Represents a postal code.</summary>
	PostalCode,
	/// <summary>Represents file upload data type.</summary>
	Upload
}
