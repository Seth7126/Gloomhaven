using System.Collections.Generic;

namespace System.Net.Http.Headers;

/// <summary>Represents an entity-tag header value.</summary>
public class EntityTagHeaderValue : ICloneable
{
	private static readonly EntityTagHeaderValue any = new EntityTagHeaderValue
	{
		Tag = "*"
	};

	/// <summary>Gets the entity-tag header value.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.EntityTagHeaderValue" />.</returns>
	public static EntityTagHeaderValue Any => any;

	/// <summary>Gets whether the entity-tag is prefaced by a weakness indicator.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the entity-tag is prefaced by a weakness indicator; otherwise, false.</returns>
	public bool IsWeak { get; internal set; }

	/// <summary>Gets the opaque quoted string. </summary>
	/// <returns>Returns <see cref="T:System.String" />.An opaque quoted string.</returns>
	public string Tag { get; internal set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.EntityTagHeaderValue" /> class.</summary>
	/// <param name="tag">A string that contains an <see cref="T:System.Net.Http.Headers.EntityTagHeaderValue" />.</param>
	public EntityTagHeaderValue(string tag)
	{
		Parser.Token.CheckQuotedString(tag);
		Tag = tag;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.EntityTagHeaderValue" /> class.</summary>
	/// <param name="tag">A string that contains an  <see cref="T:System.Net.Http.Headers.EntityTagHeaderValue" />.</param>
	/// <param name="isWeak">A value that indicates if this entity-tag header is a weak validator. If the entity-tag header is weak validator, then <paramref name="isWeak" /> should be set to true. If the entity-tag header is a strong validator, then <paramref name="isWeak" /> should be set to false.</param>
	public EntityTagHeaderValue(string tag, bool isWeak)
		: this(tag)
	{
		IsWeak = isWeak;
	}

	internal EntityTagHeaderValue()
	{
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Net.Http.Headers.EntityTagHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Object" />.A copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		return MemberwiseClone();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Net.Http.Headers.EntityTagHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <see cref="T:System.Object" /> is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object.</param>
	public override bool Equals(object obj)
	{
		if (obj is EntityTagHeaderValue entityTagHeaderValue && entityTagHeaderValue.Tag == Tag)
		{
			return string.Equals(entityTagHeaderValue.Tag, Tag, StringComparison.Ordinal);
		}
		return false;
	}

	/// <summary>Serves as a hash function for an <see cref="T:System.Net.Http.Headers.EntityTagHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		return IsWeak.GetHashCode() ^ Tag.GetHashCode();
	}

	/// <summary>Converts a string to an <see cref="T:System.Net.Http.Headers.EntityTagHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.EntityTagHeaderValue" />.An <see cref="T:System.Net.Http.Headers.EntityTagHeaderValue" /> instance.</returns>
	/// <param name="input">A string that represents entity tag header value information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is a null reference.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not valid entity tag header value information.</exception>
	public static EntityTagHeaderValue Parse(string input)
	{
		if (TryParse(input, out var parsedValue))
		{
			return parsedValue;
		}
		throw new FormatException(input);
	}

	/// <summary>Determines whether a string is valid <see cref="T:System.Net.Http.Headers.EntityTagHeaderValue" /> information.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="input" /> is valid <see cref="T:System.Net.Http.Headers.EntityTagHeaderValue" /> information; otherwise, false.</returns>
	/// <param name="input">The string to validate.</param>
	/// <param name="parsedValue">The <see cref="T:System.Net.Http.Headers.EntityTagHeaderValue" /> version of the string.</param>
	public static bool TryParse(string input, out EntityTagHeaderValue parsedValue)
	{
		if (TryParseElement(new Lexer(input), out parsedValue, out var t) && (Token.Type)t == Token.Type.End)
		{
			return true;
		}
		parsedValue = null;
		return false;
	}

	private static bool TryParseElement(Lexer lexer, out EntityTagHeaderValue parsedValue, out Token t)
	{
		parsedValue = null;
		t = lexer.Scan();
		bool isWeak = false;
		if ((Token.Type)t == Token.Type.Token)
		{
			string stringValue = lexer.GetStringValue(t);
			if (stringValue == "*")
			{
				parsedValue = any;
				t = lexer.Scan();
				return true;
			}
			if (stringValue != "W" || lexer.PeekChar() != 47)
			{
				return false;
			}
			isWeak = true;
			lexer.EatChar();
			t = lexer.Scan();
		}
		if ((Token.Type)t != Token.Type.QuotedString)
		{
			return false;
		}
		parsedValue = new EntityTagHeaderValue();
		parsedValue.Tag = lexer.GetStringValue(t);
		parsedValue.IsWeak = isWeak;
		t = lexer.Scan();
		return true;
	}

	internal static bool TryParse(string input, int minimalCount, out List<EntityTagHeaderValue> result)
	{
		return CollectionParser.TryParse(input, minimalCount, (ElementTryParser<EntityTagHeaderValue>)TryParseElement, out result);
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.EntityTagHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		if (!IsWeak)
		{
			return Tag;
		}
		return "W/" + Tag;
	}
}
