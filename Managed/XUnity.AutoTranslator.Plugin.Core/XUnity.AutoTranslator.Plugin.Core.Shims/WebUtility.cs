using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace XUnity.AutoTranslator.Plugin.Core.Shims;

internal static class WebUtility
{
	private static class HtmlEntities
	{
		private static string[] _entitiesList = new string[253]
		{
			"\"-quot", "&-amp", "'-apos", "<-lt", ">-gt", "\u00a0-nbsp", "¡-iexcl", "¢-cent", "£-pound", "¤-curren",
			"¥-yen", "¦-brvbar", "§-sect", "\u00a8-uml", "©-copy", "ª-ordf", "«-laquo", "¬-not", "\u00ad-shy", "®-reg",
			"\u00af-macr", "°-deg", "±-plusmn", "²-sup2", "³-sup3", "\u00b4-acute", "µ-micro", "¶-para", "·-middot", "\u00b8-cedil",
			"¹-sup1", "º-ordm", "»-raquo", "¼-frac14", "½-frac12", "¾-frac34", "¿-iquest", "À-Agrave", "Á-Aacute", "Â-Acirc",
			"Ã-Atilde", "Ä-Auml", "Å-Aring", "Æ-AElig", "Ç-Ccedil", "È-Egrave", "É-Eacute", "Ê-Ecirc", "Ë-Euml", "Ì-Igrave",
			"Í-Iacute", "Î-Icirc", "Ï-Iuml", "Ð-ETH", "Ñ-Ntilde", "Ò-Ograve", "Ó-Oacute", "Ô-Ocirc", "Õ-Otilde", "Ö-Ouml",
			"×-times", "Ø-Oslash", "Ù-Ugrave", "Ú-Uacute", "Û-Ucirc", "Ü-Uuml", "Ý-Yacute", "Þ-THORN", "ß-szlig", "à-agrave",
			"á-aacute", "â-acirc", "ã-atilde", "ä-auml", "å-aring", "æ-aelig", "ç-ccedil", "è-egrave", "é-eacute", "ê-ecirc",
			"ë-euml", "ì-igrave", "í-iacute", "î-icirc", "ï-iuml", "ð-eth", "ñ-ntilde", "ò-ograve", "ó-oacute", "ô-ocirc",
			"õ-otilde", "ö-ouml", "÷-divide", "ø-oslash", "ù-ugrave", "ú-uacute", "û-ucirc", "ü-uuml", "ý-yacute", "þ-thorn",
			"ÿ-yuml", "Œ-OElig", "œ-oelig", "Š-Scaron", "š-scaron", "Ÿ-Yuml", "ƒ-fnof", "ˆ-circ", "\u02dc-tilde", "Α-Alpha",
			"Β-Beta", "Γ-Gamma", "Δ-Delta", "Ε-Epsilon", "Ζ-Zeta", "Η-Eta", "Θ-Theta", "Ι-Iota", "Κ-Kappa", "Λ-Lambda",
			"Μ-Mu", "Ν-Nu", "Ξ-Xi", "Ο-Omicron", "Π-Pi", "Ρ-Rho", "Σ-Sigma", "Τ-Tau", "Υ-Upsilon", "Φ-Phi",
			"Χ-Chi", "Ψ-Psi", "Ω-Omega", "α-alpha", "β-beta", "γ-gamma", "δ-delta", "ε-epsilon", "ζ-zeta", "η-eta",
			"θ-theta", "ι-iota", "κ-kappa", "λ-lambda", "μ-mu", "ν-nu", "ξ-xi", "ο-omicron", "π-pi", "ρ-rho",
			"ς-sigmaf", "σ-sigma", "τ-tau", "υ-upsilon", "φ-phi", "χ-chi", "ψ-psi", "ω-omega", "ϑ-thetasym", "ϒ-upsih",
			"ϖ-piv", "\u2002-ensp", "\u2003-emsp", "\u2009-thinsp", "\u200c-zwnj", "\u200d-zwj", "\u200e-lrm", "\u200f-rlm", "–-ndash", "—-mdash",
			"‘-lsquo", "’-rsquo", "‚-sbquo", "“-ldquo", "”-rdquo", "„-bdquo", "†-dagger", "‡-Dagger", "•-bull", "…-hellip",
			"‰-permil", "′-prime", "″-Prime", "‹-lsaquo", "›-rsaquo", "‾-oline", "⁄-frasl", "€-euro", "ℑ-image", "℘-weierp",
			"ℜ-real", "™-trade", "ℵ-alefsym", "←-larr", "↑-uarr", "→-rarr", "↓-darr", "↔-harr", "↵-crarr", "⇐-lArr",
			"⇑-uArr", "⇒-rArr", "⇓-dArr", "⇔-hArr", "∀-forall", "∂-part", "∃-exist", "∅-empty", "∇-nabla", "∈-isin",
			"∉-notin", "∋-ni", "∏-prod", "∑-sum", "−-minus", "∗-lowast", "√-radic", "∝-prop", "∞-infin", "∠-ang",
			"∧-and", "∨-or", "∩-cap", "∪-cup", "∫-int", "∴-there4", "∼-sim", "≅-cong", "≈-asymp", "≠-ne",
			"≡-equiv", "≤-le", "≥-ge", "⊂-sub", "⊃-sup", "⊄-nsub", "⊆-sube", "⊇-supe", "⊕-oplus", "⊗-otimes",
			"⊥-perp", "⋅-sdot", "⌈-lceil", "⌉-rceil", "⌊-lfloor", "⌋-rfloor", "〈-lang", "〉-rang", "◊-loz", "♠-spades",
			"♣-clubs", "♥-hearts", "♦-diams"
		};

		private static Dictionary<string, char> _lookupTable = GenerateLookupTable();

		private static Dictionary<string, char> GenerateLookupTable()
		{
			Dictionary<string, char> dictionary = new Dictionary<string, char>(StringComparer.Ordinal);
			string[] entitiesList = _entitiesList;
			foreach (string text in entitiesList)
			{
				dictionary.Add(text.Substring(2), text[0]);
			}
			return dictionary;
		}

		public static char Lookup(string entity)
		{
			_lookupTable.TryGetValue(entity, out var value);
			return value;
		}
	}

	private const char HIGH_SURROGATE_START = '\ud800';

	private const char LOW_SURROGATE_START = '\udc00';

	private const char LOW_SURROGATE_END = '\udfff';

	private const int UNICODE_PLANE00_END = 65535;

	private const int UNICODE_PLANE01_START = 65536;

	private const int UNICODE_PLANE16_END = 1114111;

	private static readonly char[] _htmlEntityEndingChars = new char[2] { ';', '&' };

	private static bool StringRequiresHtmlDecoding(string s)
	{
		return s.IndexOf('&') >= 0;
	}

	private unsafe static int IndexOfHtmlEncodingChars(string s, int startPos)
	{
		int num = s.Length - startPos;
		fixed (char* ptr = s)
		{
			char* ptr2 = ptr + startPos;
			while (num > 0)
			{
				switch (*ptr2)
				{
				case '"':
				case '&':
				case '\'':
				case '<':
				case '>':
					return s.Length - num;
				}
				ptr2++;
				num--;
			}
		}
		return -1;
	}

	public static string HtmlEncode(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}
		if (IndexOfHtmlEncodingChars(value, 0) == -1)
		{
			return value;
		}
		StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		HtmlEncode(value, stringWriter);
		return stringWriter.ToString();
	}

	public unsafe static void HtmlEncode(string value, TextWriter output)
	{
		if (value == null)
		{
			return;
		}
		if (output == null)
		{
			throw new ArgumentNullException("output");
		}
		int num = IndexOfHtmlEncodingChars(value, 0);
		if (num == -1)
		{
			output.Write(value);
			return;
		}
		int num2 = value.Length - num;
		fixed (char* ptr = value)
		{
			char* ptr2 = ptr;
			while (num-- > 0)
			{
				output.Write(*(ptr2++));
			}
			while (num2 > 0)
			{
				char c = *ptr2;
				if (c <= '>')
				{
					switch (c)
					{
					case '<':
						output.Write("&lt;");
						break;
					case '>':
						output.Write("&gt;");
						break;
					case '"':
						output.Write("&quot;");
						break;
					case '\'':
						output.Write("&#39;");
						break;
					case '&':
						output.Write("&amp;");
						break;
					default:
						output.Write(c);
						break;
					}
				}
				else
				{
					output.Write(c);
				}
				num2--;
				ptr2++;
			}
		}
	}

	public static string HtmlDecode(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return value;
		}
		if (!StringRequiresHtmlDecoding(value))
		{
			return value;
		}
		StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
		HtmlDecode(value, stringWriter);
		return stringWriter.ToString();
	}

	private static void ConvertSmpToUtf16(uint smpChar, out char leadingSurrogate, out char trailingSurrogate)
	{
		int num = (int)(smpChar - 65536);
		leadingSurrogate = (char)(num / 1024 + 55296);
		trailingSurrogate = (char)(num % 1024 + 56320);
	}

	public static void HtmlDecode(string value, TextWriter output)
	{
		if (value == null)
		{
			return;
		}
		if (output == null)
		{
			throw new ArgumentNullException("output");
		}
		if (!StringRequiresHtmlDecoding(value))
		{
			output.Write(value);
			return;
		}
		int length = value.Length;
		for (int i = 0; i < length; i++)
		{
			char c = value[i];
			if (c == '&')
			{
				int num = value.IndexOfAny(_htmlEntityEndingChars, i + 1);
				if (num > 0 && value[num] == ';')
				{
					string text = value.Substring(i + 1, num - i - 1);
					if (text.Length > 1 && text[0] == '#')
					{
						uint result;
						bool flag = ((text[1] != 'x' && text[1] != 'X') ? uint.TryParse(text.Substring(1), NumberStyles.Integer, NumberFormatInfo.InvariantInfo, out result) : uint.TryParse(text.Substring(2), NumberStyles.AllowHexSpecifier, NumberFormatInfo.InvariantInfo, out result));
						if (flag)
						{
							flag = 0 < result && result <= 65535;
						}
						if (flag)
						{
							if (result <= 65535)
							{
								output.Write((char)result);
							}
							else
							{
								ConvertSmpToUtf16(result, out var leadingSurrogate, out var trailingSurrogate);
								output.Write(leadingSurrogate);
								output.Write(trailingSurrogate);
							}
							i = num;
							continue;
						}
					}
					else
					{
						i = num;
						char c2 = HtmlEntities.Lookup(text);
						if (c2 == '\0')
						{
							output.Write('&');
							output.Write(text);
							output.Write(';');
							continue;
						}
						c = c2;
					}
				}
			}
			output.Write(c);
		}
	}
}
