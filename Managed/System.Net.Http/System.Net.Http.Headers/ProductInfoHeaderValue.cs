using System.Collections.Generic;

namespace System.Net.Http.Headers;

/// <summary>Represents a value which can either be a product or a comment in a User-Agent header.</summary>
public class ProductInfoHeaderValue : ICloneable
{
	/// <summary>Gets the comment from the <see cref="T:System.Net.Http.Headers.ProductInfoHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.The comment value this <see cref="T:System.Net.Http.Headers.ProductInfoHeaderValue" />.</returns>
	public string Comment { get; private set; }

	/// <summary>Gets the product from the <see cref="T:System.Net.Http.Headers.ProductInfoHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.ProductHeaderValue" />.The product value from this <see cref="T:System.Net.Http.Headers.ProductInfoHeaderValue" />.</returns>
	public ProductHeaderValue Product { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.ProductInfoHeaderValue" /> class.</summary>
	/// <param name="product">A <see cref="T:System.Net.Http.Headers.ProductInfoHeaderValue" /> object used to initialize the new instance.</param>
	public ProductInfoHeaderValue(ProductHeaderValue product)
	{
		if (product == null)
		{
			throw new ArgumentNullException();
		}
		Product = product;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.ProductInfoHeaderValue" /> class.</summary>
	/// <param name="comment">A comment value.</param>
	public ProductInfoHeaderValue(string comment)
	{
		Parser.Token.CheckComment(comment);
		Comment = comment;
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Net.Http.Headers.ProductInfoHeaderValue" /> class.</summary>
	/// <param name="productName">The product name value.</param>
	/// <param name="productVersion">The product version value.</param>
	public ProductInfoHeaderValue(string productName, string productVersion)
	{
		Product = new ProductHeaderValue(productName, productVersion);
	}

	private ProductInfoHeaderValue()
	{
	}

	/// <summary>Creates a new object that is a copy of the current <see cref="T:System.Net.Http.Headers.ProductInfoHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Object" />.A copy of the current instance.</returns>
	object ICloneable.Clone()
	{
		return MemberwiseClone();
	}

	/// <summary>Determines whether the specified <see cref="T:System.Object" /> is equal to the current <see cref="T:System.Net.Http.Headers.ProductInfoHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if the specified <see cref="T:System.Object" /> is equal to the current object; otherwise, false.</returns>
	/// <param name="obj">The object to compare with the current object.</param>
	public override bool Equals(object obj)
	{
		if (!(obj is ProductInfoHeaderValue productInfoHeaderValue))
		{
			return false;
		}
		if (Product == null)
		{
			return productInfoHeaderValue.Comment == Comment;
		}
		return Product.Equals(productInfoHeaderValue.Product);
	}

	/// <summary>Serves as a hash function for an <see cref="T:System.Net.Http.Headers.ProductInfoHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.Int32" />.A hash code for the current object.</returns>
	public override int GetHashCode()
	{
		if (Product == null)
		{
			return Comment.GetHashCode();
		}
		return Product.GetHashCode();
	}

	/// <summary>Converts a string to an <see cref="T:System.Net.Http.Headers.ProductInfoHeaderValue" /> instance.</summary>
	/// <returns>Returns <see cref="T:System.Net.Http.Headers.ProductInfoHeaderValue" />.An <see cref="T:System.Net.Http.Headers.ProductInfoHeaderValue" /> instance.</returns>
	/// <param name="input">A string that represents product info header value information.</param>
	/// <exception cref="T:System.ArgumentNullException">
	///   <paramref name="input" /> is a null reference.</exception>
	/// <exception cref="T:System.FormatException">
	///   <paramref name="input" /> is not valid product info header value information.</exception>
	public static ProductInfoHeaderValue Parse(string input)
	{
		if (TryParse(input, out var parsedValue))
		{
			return parsedValue;
		}
		throw new FormatException(input);
	}

	/// <summary>Determines whether a string is valid <see cref="T:System.Net.Http.Headers.ProductInfoHeaderValue" /> information.</summary>
	/// <returns>Returns <see cref="T:System.Boolean" />.true if <paramref name="input" /> is valid <see cref="T:System.Net.Http.Headers.ProductInfoHeaderValue" /> information; otherwise, false.</returns>
	/// <param name="input">The string to validate.</param>
	/// <param name="parsedValue">The <see cref="T:System.Net.Http.Headers.ProductInfoHeaderValue" /> version of the string.</param>
	public static bool TryParse(string input, out ProductInfoHeaderValue parsedValue)
	{
		parsedValue = null;
		Lexer lexer = new Lexer(input);
		if (!TryParseElement(lexer, out parsedValue) || parsedValue == null)
		{
			return false;
		}
		if ((Token.Type)lexer.Scan() != Token.Type.End)
		{
			parsedValue = null;
			return false;
		}
		return true;
	}

	internal static bool TryParse(string input, int minimalCount, out List<ProductInfoHeaderValue> result)
	{
		List<ProductInfoHeaderValue> list = new List<ProductInfoHeaderValue>();
		Lexer lexer = new Lexer(input);
		result = null;
		while (true)
		{
			if (!TryParseElement(lexer, out var parsedValue))
			{
				return false;
			}
			if (parsedValue == null)
			{
				if (list != null && minimalCount <= list.Count)
				{
					result = list;
					return true;
				}
				return false;
			}
			list.Add(parsedValue);
			switch (lexer.PeekChar())
			{
			case 9:
			case 32:
				goto IL_004e;
			case -1:
				if (minimalCount <= list.Count)
				{
					result = list;
					return true;
				}
				break;
			}
			break;
			IL_004e:
			lexer.EatChar();
		}
		return false;
	}

	private static bool TryParseElement(Lexer lexer, out ProductInfoHeaderValue parsedValue)
	{
		parsedValue = null;
		if (lexer.ScanCommentOptional(out var value, out var readToken))
		{
			if (value == null)
			{
				return false;
			}
			parsedValue = new ProductInfoHeaderValue();
			parsedValue.Comment = value;
			return true;
		}
		if ((Token.Type)readToken == Token.Type.End)
		{
			return true;
		}
		if ((Token.Type)readToken != Token.Type.Token)
		{
			return false;
		}
		ProductHeaderValue productHeaderValue = new ProductHeaderValue();
		productHeaderValue.Name = lexer.GetStringValue(readToken);
		int position = lexer.Position;
		readToken = lexer.Scan();
		if ((Token.Type)readToken == Token.Type.SeparatorSlash)
		{
			readToken = lexer.Scan();
			if ((Token.Type)readToken != Token.Type.Token)
			{
				return false;
			}
			productHeaderValue.Version = lexer.GetStringValue(readToken);
		}
		else
		{
			lexer.Position = position;
		}
		parsedValue = new ProductInfoHeaderValue(productHeaderValue);
		return true;
	}

	/// <summary>Returns a string that represents the current <see cref="T:System.Net.Http.Headers.ProductInfoHeaderValue" /> object.</summary>
	/// <returns>Returns <see cref="T:System.String" />.A string that represents the current object.</returns>
	public override string ToString()
	{
		if (Product == null)
		{
			return Comment;
		}
		return Product.ToString();
	}
}
