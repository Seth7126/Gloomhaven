using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace System.Net.Http.Headers;

/// <summary>Represents the value of the Content-Disposition header.</summary>
public class ContentDispositionHeaderValue : ICloneable
{
	private string dispositionType;

	private List<NameValueHeaderValue> parameters;

	/// <summary>The date at which   the file was created.</summary>
	/// <returns>Returns <see cref="T:System.DateTimeOffset" />.The file creation date.</returns>
	public DateTimeOffset? CreationDate
	{
		get
		{
			return GetDateValue("creation-date");
		}
		set
		{
			SetDateValue("creation-date", value);
		}
	}

	/// <summary>The disposition type for a content body part.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The disposition type. </returns>
	public string DispositionType
	{
		get
		{
			return dispositionType;
		}
		set
		{
			Parser.Token.Check(value);
			dispositionType = value;
		}
	}

	/// <summary>A suggestion for how to construct a filename for   storing the message payload to be used if the entity is   detached and stored in a separate file.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A suggested filename.</returns>
	public string FileName
	{
		get
		{
			string text = FindParameter("filename");
			if (text == null)
			{
				return null;
			}
			return DecodeValue(text, extendedNotation: false);
		}
		set
		{
			if (value != null)
			{
				value = EncodeBase64Value(value);
			}
			SetValue("filename", value);
		}
	}

	/// <summary>A suggestion for how to construct filenames for   storing message payloads to be used if the entities are    detached and stored in a separate files.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A suggested filename of the form filename*.</returns>
	public string FileNameStar
	{
		get
		{
			string text = FindParameter("filename*");
			if (text == null)
			{
				return null;
			}
			return DecodeValue(text, extendedNotation: true);
		}
		set
		{
			if (value != null)
			{
				value = EncodeRFC5987(value);
			}
			SetValue("filename*", value);
		}
	}

	/// <summary>The date at   which the file was last modified. </summary>
	/// <returns>Returns <see cref="T:System.DateTimeOffset" />.The file modification date.</returns>
	public DateTimeOffset? ModificationDate
	{
		get
		{
			return GetDateValue("modification-date");
		}
		set
		{
			SetDateValue("modification-date", value);
		}
	}

	/// <summary>The name for a content body part.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The name for the content body part.</returns>
	public string Name
	{
		get
		{
			string text = FindParameter("name");
			if (text == null)
			{
				return null;
			}
			return DecodeValue(text, extendedNotation: false);
		}
		set
		{
			if (value != null)
			{
				value = EncodeBase64Value(value);
			}
			SetValue("name", value);
		}
	}

	/// <summary>A set of parameters included the Content-Disposition header.</summary>
	/// <returns>Returns <see cref="T:System.Collections.Generic.ICollection`1" />.A collection of parameters. </returns>
	public ICollection<NameValueHeaderValue> Parameters => parameters ?? (parameters = new List<NameValueHeaderValue>());

	/// <summary>The date the file was last read.</summary>
	/// <returns>Returns <see cref="T:System.DateTimeOffset" />.The last read date.</returns>
	public DateTimeOffset? ReadDate
	{
		get
		{
			return GetDateValue("read-date");
		}
		set
		{
			SetDateValue("read-date", value);
		}
	}

	/// <summary>The approximate size, in bytes, of the file. </summary>
	/// <returns>Returns <see cref="T:System.Int64" />.The approximate size, in bytes.</returns>
	public long? Size
	{
		get
		{
			if (Parser.Long.TryParse(FindParameter("size"), out var result))
			{
				return result;
			}
			return null;
		}
		set
		{
			if (!value.HasValue)
			{
				SetValue("size", null);
				return;
			}
			if (value < 0)
			{
				throw new ArgumentOutOfRangeException("value");
			}
			SetValue("size", value.Value.ToString(CultureInfo.InvariantCulture));
		}
	}

	private ContentDispositionHeaderValue()
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" /> class.</summary>
	/// <param name="dispositionType">A string that contains a <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" />.</param>
	public ContentDispositionHeaderValue(string dispositionType)
	{
		DispositionType = dispositionType;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" /> class.</summary>
	/// <param name="source">A <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" />. </param>
	protected ContentDispositionHeaderValue(ContentDispositionHeaderValue source)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		dispositionType = source.dispositionType;
		if (source.parameters == null)
		{
			return;
		}
		foreach (NameValueHeaderValue parameter in source.parameters)
		{
			Parameters.Add(new NameValueHeaderValue(parameter));
		}
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" />  instance.</summary>
	/// <returns>Returns <see cref="T:System.Object" />.A copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		return new ContentDispositionHeaderValue(this);
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" />  object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <see cref="T:System.Object" /> is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object.</param>
	public override bool Equals(object obj)
	{
		if (obj is ContentDispositionHeaderValue contentDispositionHeaderValue && string.Equals(contentDispositionHeaderValue.dispositionType, dispositionType, StringComparison.OrdinalIgnoreCase))
		{
			return contentDispositionHeaderValue.parameters.SequenceEqual(parameters);
		}
		return false;
	}

	private string FindParameter(string name)
	{
		if (parameters == null)
		{
			return null;
		}
		foreach (NameValueHeaderValue parameter in parameters)
		{
			if (string.Equals(parameter.Name, name, StringComparison.OrdinalIgnoreCase))
			{
				return parameter.Value;
			}
		}
		return null;
	}

	private DateTimeOffset? GetDateValue(string name)
	{
		string text = FindParameter(name);
		if (text == null || text == null)
		{
			return null;
		}
		if (text.Length < 3)
		{
			return null;
		}
		if (text[0] == '"')
		{
			text = text.Substring(1, text.Length - 2);
		}
		if (Lexer.TryGetDateValue(text, out var value))
		{
			return value;
		}
		return null;
	}

	private static string EncodeBase64Value(string value)
	{
		bool flag = value.Length > 1 && value[0] == '"' && value[value.Length - 1] == '"';
		if (flag)
		{
			value = value.Substring(1, value.Length - 2);
		}
		for (int i = 0; i < value.Length; i++)
		{
			if (value[i] > '\u007f')
			{
				Encoding uTF = Encoding.UTF8;
				return $"\"=?{uTF.WebName}?B?{Convert.ToBase64String(uTF.GetBytes(value))}?=\"";
			}
		}
		if (flag || !Lexer.IsValidToken(value))
		{
			return "\"" + value + "\"";
		}
		return value;
	}

	private static string EncodeRFC5987(string value)
	{
		Encoding uTF = Encoding.UTF8;
		StringBuilder stringBuilder = new StringBuilder(value.Length + 11);
		stringBuilder.Append(uTF.WebName);
		stringBuilder.Append('\'');
		stringBuilder.Append('\'');
		foreach (char c in value)
		{
			if (c > '\u007f')
			{
				byte[] bytes = uTF.GetBytes(new char[1] { c });
				foreach (byte b in bytes)
				{
					stringBuilder.Append('%');
					stringBuilder.Append(b.ToString("X2"));
				}
			}
			else if (!Lexer.IsValidCharacter(c) || c == '*' || c == '?' || c == '%')
			{
				stringBuilder.Append(Uri.HexEscape(c));
			}
			else
			{
				stringBuilder.Append(c);
			}
		}
		return stringBuilder.ToString();
	}

	private static string DecodeValue(string value, bool extendedNotation)
	{
		if (value.Length < 2)
		{
			return value;
		}
		string[] array;
		Encoding encoding;
		if (value[0] == '"')
		{
			array = value.Split('?');
			if (array.Length != 5 || array[0] != "\"=" || array[4] != "=\"" || (array[2] != "B" && array[2] != "b"))
			{
				return value;
			}
			try
			{
				encoding = Encoding.GetEncoding(array[1]);
				return encoding.GetString(Convert.FromBase64String(array[3]));
			}
			catch
			{
				return value;
			}
		}
		if (!extendedNotation)
		{
			return value;
		}
		array = value.Split('\'');
		if (array.Length != 3)
		{
			return null;
		}
		try
		{
			encoding = Encoding.GetEncoding(array[0]);
		}
		catch
		{
			return null;
		}
		value = array[2];
		if (value.IndexOf('%') < 0)
		{
			return value;
		}
		StringBuilder stringBuilder = new StringBuilder();
		byte[] array2 = null;
		int num = 0;
		int index = 0;
		while (index < value.Length)
		{
			char c = value[index];
			if (c == '%')
			{
				char c2 = c;
				c = Uri.HexUnescape(value, ref index);
				if (c != c2)
				{
					if (array2 == null)
					{
						array2 = new byte[value.Length - index + 1];
					}
					array2[num++] = (byte)c;
					continue;
				}
			}
			else
			{
				index++;
			}
			if (num != 0)
			{
				stringBuilder.Append(encoding.GetChars(array2, 0, num));
				num = 0;
			}
			stringBuilder.Append(c);
		}
		if (num != 0)
		{
			stringBuilder.Append(encoding.GetChars(array2, 0, num));
		}
		return stringBuilder.ToString();
	}

	/// <summary>Serves as a hash function for an  <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return dispositionType.ToLowerInvariant().GetHashCode() ^ HashCodeCalculator.Calculate(parameters);
	}

	/// <summary>Converts a string to an <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" />  instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" />.An <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" />  instance.</returns>
	/// <param name="input">A string that represents content disposition header value information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is a null reference.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not valid content disposition header value information.</exception>
	public static ContentDispositionHeaderValue Parse(string input)
	{
		if (TryParse(input, out var parsedValue))
		{
			return parsedValue;
		}
		throw new FormatException(input);
	}

	private void SetDateValue(string key, DateTimeOffset? value)
	{
		SetValue(key, (!value.HasValue) ? null : ("\"" + value.Value.ToString("r", CultureInfo.InvariantCulture) + "\""));
	}

	private void SetValue(string key, string value)
	{
		if (parameters == null)
		{
			parameters = new List<NameValueHeaderValue>();
		}
		parameters.SetValue(key, value);
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		return dispositionType + CollectionExtensions.ToString(parameters);
	}

	/// <summary>Determines whether a string is valid <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" /> information.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="input" /> is valid <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" /> information; otherwise, false.</returns>
	/// <param name="input">The string to validate.</param>
	/// <param name="parsedValue">The <see cref="T:System.Net.Http.Headers.ContentDispositionHeaderValue" /> version of the string.</param>
	public static bool TryParse(string input, out ContentDispositionHeaderValue parsedValue)
	{
		parsedValue = null;
		Lexer lexer = new Lexer(input);
		Token token = lexer.Scan();
		if (token.Kind != Token.Type.Token)
		{
			return false;
		}
		List<NameValueHeaderValue> result = null;
		string stringValue = lexer.GetStringValue(token);
		token = lexer.Scan();
		switch (token.Kind)
		{
		case Token.Type.SeparatorSemicolon:
			if (!NameValueHeaderValue.TryParseParameters(lexer, out result, out token) || (Token.Type)token != Token.Type.End)
			{
				return false;
			}
			break;
		default:
			return false;
		case Token.Type.End:
			break;
		}
		parsedValue = new ContentDispositionHeaderValue
		{
			dispositionType = stringValue,
			parameters = result
		};
		return true;
	}
}
