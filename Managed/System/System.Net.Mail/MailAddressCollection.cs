using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace System.Net.Mail;

/// <summary>Store e-mail addresses that are associated with an e-mail message.</summary>
public class MailAddressCollection : Collection<MailAddress>
{
	/// <summary>Initializes an empty instance of the <see cref="T:System.Net.Mail.MailAddressCollection" /> class.</summary>
	public MailAddressCollection()
	{
	}

	/// <summary>Add a list of e-mail addresses to the collection.</summary>
	/// <param name="addresses">The e-mail addresses to add to the <see cref="T:System.Net.Mail.MailAddressCollection" />. Multiple e-mail addresses must be separated with a comma character (","). </param>
	/// <exception cref="T:System.ArgumentNullException">The<paramref name=" addresses" /> parameter is null.</exception>
	/// <exception cref="T:System.ArgumentException">The<paramref name=" addresses" /> parameter is an empty string.</exception>
	/// <exception cref="T:System.FormatException">The<paramref name=" addresses" /> parameter contains an e-mail address that is invalid or not supported. </exception>
	public void Add(string addresses)
	{
		if (addresses == null)
		{
			throw new ArgumentNullException("addresses");
		}
		if (addresses == string.Empty)
		{
			throw new ArgumentException(global::SR.Format("The parameter '{0}' cannot be an empty string.", "addresses"), "addresses");
		}
		ParseValue(addresses);
	}

	/// <summary>Replaces the element at the specified index.</summary>
	/// <param name="index">The index of the e-mail address element to be replaced.</param>
	/// <param name="item">An e-mail address that will replace the element in the collection.</param>
	/// <exception cref="T:System.ArgumentNullException">The<paramref name=" item" /> parameter is null.</exception>
	protected override void SetItem(int index, MailAddress item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		base.SetItem(index, item);
	}

	/// <summary>Inserts an e-mail address into the <see cref="T:System.Net.Mail.MailAddressCollection" />, at the specified location.</summary>
	/// <param name="index">The location at which to insert the e-mail address that is specified by <paramref name="item" />.</param>
	/// <param name="item">The e-mail address to be inserted into the collection.</param>
	/// <exception cref="T:System.ArgumentNullException">The<paramref name=" item" /> parameter is null.</exception>
	protected override void InsertItem(int index, MailAddress item)
	{
		if (item == null)
		{
			throw new ArgumentNullException("item");
		}
		base.InsertItem(index, item);
	}

	internal void ParseValue(string addresses)
	{
		IList<MailAddress> list = MailAddressParser.ParseMultipleAddresses(addresses);
		for (int i = 0; i < list.Count; i++)
		{
			Add(list[i]);
		}
	}

	/// <summary>Returns a string representation of the e-mail addresses in this <see cref="T:System.Net.Mail.MailAddressCollection" /> object.</summary>
	/// <returns>A <see cref="T:System.String" /> containing the e-mail addresses in this collection.</returns>
	public override string ToString()
	{
		bool flag = true;
		StringBuilder stringBuilder = new StringBuilder();
		using (IEnumerator<MailAddress> enumerator = GetEnumerator())
		{
			while (enumerator.MoveNext())
			{
				MailAddress current = enumerator.Current;
				if (!flag)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(current.ToString());
				flag = false;
			}
		}
		return stringBuilder.ToString();
	}

	internal string Encode(int charsConsumed, bool allowUnicode)
	{
		string text = string.Empty;
		using IEnumerator<MailAddress> enumerator = GetEnumerator();
		while (enumerator.MoveNext())
		{
			MailAddress current = enumerator.Current;
			text = ((!string.IsNullOrEmpty(text)) ? (text + ", " + current.Encode(1, allowUnicode)) : current.Encode(charsConsumed, allowUnicode));
		}
		return text;
	}
}
