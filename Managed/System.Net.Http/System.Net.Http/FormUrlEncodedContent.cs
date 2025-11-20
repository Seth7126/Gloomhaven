using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Text;

namespace System.Net.Http;

/// <summary>A container for name/value tuples encoded using application/x-www-form-urlencoded MIME type.</summary>
public class FormUrlEncodedContent : ByteArrayContent
{
	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.FormUrlEncodedContent" /> class with a specific collection of name/value pairs.</summary>
	/// <param name="nameValueCollection">A collection of name/value pairs.</param>
	public FormUrlEncodedContent(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
		: base(EncodeContent(nameValueCollection))
	{
		base.Headers.ContentType = new MediaTypeHeaderValue("application/x-www-form-urlencoded");
	}

	private static byte[] EncodeContent(IEnumerable<KeyValuePair<string, string>> nameValueCollection)
	{
		if (nameValueCollection == null)
		{
			throw new ArgumentNullException("nameValueCollection");
		}
		List<byte> list = new List<byte>();
		foreach (KeyValuePair<string, string> item in nameValueCollection)
		{
			if (list.Count != 0)
			{
				list.Add(38);
			}
			byte[] array = SerializeValue(item.Key);
			if (array != null)
			{
				list.AddRange(array);
			}
			list.Add(61);
			array = SerializeValue(item.Value);
			if (array != null)
			{
				list.AddRange(array);
			}
		}
		return list.ToArray();
	}

	private static byte[] SerializeValue(string value)
	{
		if (value == null)
		{
			return null;
		}
		value = Uri.EscapeDataString(value).Replace("%20", "+");
		return Encoding.ASCII.GetBytes(value);
	}
}
