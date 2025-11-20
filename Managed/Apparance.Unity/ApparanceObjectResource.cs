using System;
using System.Text;
using UnityEngine;

[Serializable]
public class ApparanceObjectResource
{
	public string Name;

	public UnityEngine.Object Object;

	public string Description;

	public bool Copy;

	public string ID;

	public int Variant;

	public int Variants;

	[NonSerialized]
	public bool IsNew;

	[NonSerialized]
	public string OrderedName;

	public bool IsVariant => Variant > 0;

	public bool SupportsCopy
	{
		get
		{
			if (Object != null && Object is Material)
			{
				return true;
			}
			return false;
		}
	}

	public bool UpdateName(string category)
	{
		string name = Name;
		if (string.IsNullOrWhiteSpace(ID))
		{
			if (string.IsNullOrWhiteSpace(category))
			{
				ID = Name;
			}
			else if (Name.StartsWith(category, ignoreCase: true, null) && (Name.Length == category.Length || Name[category.Length] == '.'))
			{
				ID = Name.Substring(category.Length + 1);
			}
			else
			{
				ID = Name;
			}
			Variant = 0;
		}
		if (string.IsNullOrWhiteSpace(ID))
		{
			ID = "unnamed";
		}
		int num = ID.IndexOf('#');
		int result = 0;
		int num2;
		string text;
		if (num != -1 && int.TryParse(ID.Substring(num + 1), out result))
		{
			num2 = ((result > 0) ? 1 : 0);
			if (num2 != 0)
			{
				text = ID.Substring(0, num);
				goto IL_00f2;
			}
		}
		else
		{
			num2 = 0;
		}
		text = ID;
		goto IL_00f2;
		IL_00f2:
		string value = text;
		if (num2 != 0)
		{
			ID = ID.Substring(0, num);
			Variant = result;
		}
		StringBuilder stringBuilder = new StringBuilder();
		OrderedName = null;
		stringBuilder.Append(category);
		if (stringBuilder.Length > 0)
		{
			stringBuilder.Append('.');
		}
		stringBuilder.Append(value);
		OrderedName = stringBuilder.ToString();
		if (Variant > 0)
		{
			stringBuilder.Append('#');
			string text2 = Variant.ToString();
			OrderedName = stringBuilder.ToString();
			int num3 = ((Variant > 0) ? ((int)Math.Log10(Variant)) : 0);
			int num4 = ((Variants > 0) ? ((int)Math.Log10(Variants)) : 0) - num3;
			if (num4 > 0)
			{
				OrderedName += new string('0', num4);
			}
			OrderedName += text2;
			stringBuilder.Append(text2);
		}
		Name = stringBuilder.ToString();
		return Name != name;
	}
}
