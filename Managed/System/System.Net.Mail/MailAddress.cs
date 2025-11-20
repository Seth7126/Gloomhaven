using System.Globalization;
using System.Net.Mime;
using System.Text;

namespace System.Net.Mail;

/// <summary>Represents the address of an electronic mail sender or recipient.</summary>
public class MailAddress
{
	private readonly Encoding _displayNameEncoding;

	private readonly string _displayName;

	private readonly string _userName;

	private readonly string _host;

	private static readonly EncodedStreamFactory s_encoderFactory = new EncodedStreamFactory();

	/// <summary>Gets the display name composed from the display name and address information specified when this instance was created.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the display name; otherwise, <see cref="F:System.String.Empty" /> ("") if no display name information was specified when this instance was created.</returns>
	public string DisplayName => _displayName;

	/// <summary>Gets the user information from the address specified when this instance was created.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the user name portion of the <see cref="P:System.Net.Mail.MailAddress.Address" />.</returns>
	public string User => _userName;

	/// <summary>Gets the host portion of the address specified when this instance was created.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the name of the host computer that accepts e-mail for the <see cref="P:System.Net.Mail.MailAddress.User" /> property.</returns>
	public string Host => _host;

	/// <summary>Gets the e-mail address specified when this instance was created.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the e-mail address.</returns>
	public string Address => _userName + "@" + _host;

	private string SmtpAddress => "<" + Address + ">";

	internal MailAddress(string displayName, string userName, string domain)
	{
		_host = domain;
		_userName = userName;
		_displayName = displayName;
		_displayNameEncoding = Encoding.GetEncoding("utf-8");
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mail.MailAddress" /> class using the specified address. </summary>
	/// <param name="address">A <see cref="T:System.String" /> that contains an e-mail address.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="address" /> is <see cref="F:System.String.Empty" /> ("").</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="address" /> is not in a recognized format.</exception>
	public MailAddress(string address)
		: this(address, (string)null, (Encoding)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mail.MailAddress" /> class using the specified address and display name.</summary>
	/// <param name="address">A <see cref="T:System.String" /> that contains an e-mail address.</param>
	/// <param name="displayName">A <see cref="T:System.String" /> that contains the display name associated with <paramref name="address" />. This parameter can be null.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="address" /> is <see cref="F:System.String.Empty" /> ("").</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="address" /> is not in a recognized format.-or-<paramref name="address" /> contains non-ASCII characters.</exception>
	public MailAddress(string address, string displayName)
		: this(address, displayName, (Encoding)null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Mail.MailAddress" /> class using the specified address, display name, and encoding.</summary>
	/// <param name="address">A <see cref="T:System.String" /> that contains an e-mail address.</param>
	/// <param name="displayName">A <see cref="T:System.String" /> that contains the display name associated with <paramref name="address" />.</param>
	/// <param name="displayNameEncoding">The <see cref="T:System.Text.Encoding" /> that defines the character set used for <paramref name="displayName" />.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="address" /> is null.-or-<paramref name="displayName" /> is null.</exception>
	/// <exception cref="T:System.ArgumentException">
	///   <paramref name="address" /> is <see cref="F:System.String.Empty" /> ("").-or-<paramref name="displayName" /> is <see cref="F:System.String.Empty" /> ("").</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="address" /> is not in a recognized format.-or-<paramref name="address" /> contains non-ASCII characters.</exception>
	public MailAddress(string address, string displayName, Encoding displayNameEncoding)
	{
		if (address == null)
		{
			throw new ArgumentNullException("address");
		}
		if (address == string.Empty)
		{
			throw new ArgumentException(global::SR.Format("The parameter '{0}' cannot be an empty string.", "address"), "address");
		}
		_displayNameEncoding = displayNameEncoding ?? Encoding.GetEncoding("utf-8");
		_displayName = displayName ?? string.Empty;
		if (!string.IsNullOrEmpty(_displayName))
		{
			_displayName = MailAddressParser.NormalizeOrThrow(_displayName);
			if (_displayName.Length >= 2 && _displayName[0] == '"' && _displayName[_displayName.Length - 1] == '"')
			{
				_displayName = _displayName.Substring(1, _displayName.Length - 2);
			}
		}
		MailAddress mailAddress = MailAddressParser.ParseAddress(address);
		_host = mailAddress._host;
		_userName = mailAddress._userName;
		if (string.IsNullOrEmpty(_displayName))
		{
			_displayName = mailAddress._displayName;
		}
	}

	private string GetUser(bool allowUnicode)
	{
		if (!allowUnicode && !MimeBasePart.IsAscii(_userName, permitCROrLF: true))
		{
			throw new SmtpException(global::SR.Format("The client or server is only configured for E-mail addresses with ASCII local-parts: {0}.", Address));
		}
		return _userName;
	}

	private string GetHost(bool allowUnicode)
	{
		string text = _host;
		if (!allowUnicode && !MimeBasePart.IsAscii(text, permitCROrLF: true))
		{
			IdnMapping idnMapping = new IdnMapping();
			try
			{
				text = idnMapping.GetAscii(text);
			}
			catch (ArgumentException innerException)
			{
				throw new SmtpException(global::SR.Format("The address has an invalid host name: {0}.", Address), innerException);
			}
		}
		return text;
	}

	private string GetAddress(bool allowUnicode)
	{
		return GetUser(allowUnicode) + "@" + GetHost(allowUnicode);
	}

	internal string GetSmtpAddress(bool allowUnicode)
	{
		return "<" + GetAddress(allowUnicode) + ">";
	}

	/// <summary>Returns a string representation of this instance.</summary>
	/// <returns>A <see cref="T:System.String" /> that contains the contents of this <see cref="T:System.Net.Mail.MailAddress" />.</returns>
	public override string ToString()
	{
		if (string.IsNullOrEmpty(DisplayName))
		{
			return Address;
		}
		return "\"" + DisplayName + "\" " + SmtpAddress;
	}

	/// <summary>Compares two mail addresses.</summary>
	/// <returns>true if the two mail addresses are equal; otherwise, false.</returns>
	/// <param name="value">A <see cref="T:System.Net.Mail.MailAddress" /> instance to compare to the current instance.</param>
	public override bool Equals(object value)
	{
		if (value == null)
		{
			return false;
		}
		return ToString().Equals(value.ToString(), StringComparison.InvariantCultureIgnoreCase);
	}

	/// <summary>Returns a hash value for a mail address.</summary>
	/// <returns>An integer hash value.</returns>
	public override int GetHashCode()
	{
		return ToString().GetHashCode();
	}

	internal string Encode(int charsConsumed, bool allowUnicode)
	{
		string empty = string.Empty;
		if (!string.IsNullOrEmpty(_displayName))
		{
			if (MimeBasePart.IsAscii(_displayName, permitCROrLF: false) || allowUnicode)
			{
				empty = "\"" + _displayName + "\"";
			}
			else
			{
				IEncodableStream encoderForHeader = s_encoderFactory.GetEncoderForHeader(_displayNameEncoding, useBase64Encoding: false, charsConsumed);
				byte[] bytes = _displayNameEncoding.GetBytes(_displayName);
				encoderForHeader.EncodeBytes(bytes, 0, bytes.Length);
				empty = encoderForHeader.GetEncodedString();
			}
			return empty + " " + GetSmtpAddress(allowUnicode);
		}
		return GetAddress(allowUnicode);
	}
}
