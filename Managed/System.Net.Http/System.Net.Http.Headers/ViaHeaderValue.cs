using System.Collections.Generic;

namespace System.Net.Http.Headers;

/// <summary>Represents the value of a Via header.</summary>
public class ViaHeaderValue : ICloneable
{
	/// <summary>Gets the comment field used to identify the software of the recipient proxy or gateway.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The comment field used to identify the software of the recipient proxy or gateway.</returns>
	public string Comment { get; private set; }

	/// <summary>Gets the protocol name of the received protocol.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The protocol name.</returns>
	public string ProtocolName { get; private set; }

	/// <summary>Gets the protocol version of the received protocol.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The protocol version.</returns>
	public string ProtocolVersion { get; private set; }

	/// <summary>Gets the host and port that the request or response was received by.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The host and port that the request or response was received by.</returns>
	public string ReceivedBy { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.ViaHeaderValue" /> class.</summary>
	/// <param name="protocolVersion">The protocol version of the received protocol.</param>
	/// <param name="receivedBy">The host and port that the request or response was received by.</param>
	public ViaHeaderValue(string protocolVersion, string receivedBy)
	{
		Parser.Token.Check(protocolVersion);
		Parser.Uri.Check(receivedBy);
		ProtocolVersion = protocolVersion;
		ReceivedBy = receivedBy;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.ViaHeaderValue" /> class.</summary>
	/// <param name="protocolVersion">The protocol version of the received protocol.</param>
	/// <param name="receivedBy">The host and port that the request or response was received by.</param>
	/// <param name="protocolName">The protocol name of the received protocol.</param>
	public ViaHeaderValue(string protocolVersion, string receivedBy, string protocolName)
		: this(protocolVersion, receivedBy)
	{
		if (!string.IsNullOrEmpty(protocolName))
		{
			Parser.Token.Check(protocolName);
			ProtocolName = protocolName;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.ViaHeaderValue" /> class.</summary>
	/// <param name="protocolVersion">The protocol version of the received protocol.</param>
	/// <param name="receivedBy">The host and port that the request or response was received by.</param>
	/// <param name="protocolName">The protocol name of the received protocol.</param>
	/// <param name="comment">The comment field used to identify the software of the recipient proxy or gateway.</param>
	public ViaHeaderValue(string protocolVersion, string receivedBy, string protocolName, string comment)
		: this(protocolVersion, receivedBy, protocolName)
	{
		if (!string.IsNullOrEmpty(comment))
		{
			Parser.Token.CheckComment(comment);
			Comment = comment;
		}
	}

	private ViaHeaderValue()
	{
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Net.Http.Headers.ViaHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Object" />.A copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		return MemberwiseClone();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Net.Http.Headers.ViaHeaderValue" />object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <see cref="T:System.Object" /> is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is ViaHeaderValue viaHeaderValue))
		{
			return false;
		}
		if (string.Equals(viaHeaderValue.Comment, Comment, StringComparison.Ordinal) && string.Equals(viaHeaderValue.ProtocolName, ProtocolName, StringComparison.OrdinalIgnoreCase) && string.Equals(viaHeaderValue.ProtocolVersion, ProtocolVersion, StringComparison.OrdinalIgnoreCase))
		{
			return string.Equals(viaHeaderValue.ReceivedBy, ReceivedBy, StringComparison.OrdinalIgnoreCase);
		}
		return false;
	}

	/// <summary>Serves as a hash function for an <see cref="T:System.Net.Http.Headers.ViaHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.Returns a hash code for the current object.</returns>
	public override int GetHashCode()
	{
		int hashCode = ProtocolVersion.ToLowerInvariant().GetHashCode();
		hashCode ^= ReceivedBy.ToLowerInvariant().GetHashCode();
		if (!string.IsNullOrEmpty(ProtocolName))
		{
			hashCode ^= ProtocolName.ToLowerInvariant().GetHashCode();
		}
		if (!string.IsNullOrEmpty(Comment))
		{
			hashCode ^= Comment.GetHashCode();
		}
		return hashCode;
	}

	/// <summary>Converts a string to an <see cref="T:System.Net.Http.Headers.ViaHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.ViaHeaderValue" />.An <see cref="T:System.Net.Http.Headers.ViaHeaderValue" /> instance.</returns>
	/// <param name="input">A string that represents via header value information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is a null reference.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not valid via header value information.</exception>
	public static ViaHeaderValue Parse(string input)
	{
		if (TryParse(input, out var parsedValue))
		{
			return parsedValue;
		}
		throw new FormatException(input);
	}

	/// <summary>Determines whether a string is valid <see cref="T:System.Net.Http.Headers.ViaHeaderValue" /> information.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="input" /> is valid <see cref="T:System.Net.Http.Headers.ViaHeaderValue" /> information; otherwise, false.</returns>
	/// <param name="input">The string to validate.</param>
	/// <param name="parsedValue">The <see cref="T:System.Net.Http.Headers.ViaHeaderValue" /> version of the string.</param>
	public static bool TryParse(string input, out ViaHeaderValue parsedValue)
	{
		if (TryParseElement(new Lexer(input), out parsedValue, out var t) && (Token.Type)t == Token.Type.End)
		{
			return true;
		}
		parsedValue = null;
		return false;
	}

	internal static bool TryParse(string input, int minimalCount, out List<ViaHeaderValue> result)
	{
		return CollectionParser.TryParse(input, minimalCount, (ElementTryParser<ViaHeaderValue>)TryParseElement, out result);
	}

	private static bool TryParseElement(Lexer lexer, out ViaHeaderValue parsedValue, out Token t)
	{
		parsedValue = null;
		t = lexer.Scan();
		if ((Token.Type)t != Token.Type.Token)
		{
			return false;
		}
		Token token = lexer.Scan();
		ViaHeaderValue viaHeaderValue = new ViaHeaderValue();
		if ((Token.Type)token == Token.Type.SeparatorSlash)
		{
			token = lexer.Scan();
			if ((Token.Type)token != Token.Type.Token)
			{
				return false;
			}
			viaHeaderValue.ProtocolName = lexer.GetStringValue(t);
			viaHeaderValue.ProtocolVersion = lexer.GetStringValue(token);
			token = lexer.Scan();
		}
		else
		{
			viaHeaderValue.ProtocolVersion = lexer.GetStringValue(t);
		}
		if ((Token.Type)token != Token.Type.Token)
		{
			return false;
		}
		if (lexer.PeekChar() == 58)
		{
			lexer.EatChar();
			t = lexer.Scan();
			if ((Token.Type)t != Token.Type.Token)
			{
				return false;
			}
		}
		else
		{
			t = token;
		}
		viaHeaderValue.ReceivedBy = lexer.GetStringValue(token, t);
		if (lexer.ScanCommentOptional(out var value, out t))
		{
			t = lexer.Scan();
		}
		viaHeaderValue.Comment = value;
		parsedValue = viaHeaderValue;
		return true;
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.ViaHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		string text = ((ProtocolName != null) ? (ProtocolName + "/" + ProtocolVersion + " " + ReceivedBy) : (ProtocolVersion + " " + ReceivedBy));
		if (Comment == null)
		{
			return text;
		}
		return text + " " + Comment;
	}
}
