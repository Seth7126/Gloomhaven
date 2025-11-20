using System;
using System.Collections.Generic;
using System.Text;

namespace ExIni;

public class IniComment
{
	public List<string> Comments { get; set; }

	public IniComment()
	{
		Comments = new List<string>();
	}

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < Comments.Count; i++)
		{
			string text = Comments[i];
			string value = ((i < Comments.Count - 1) ? (";" + text + Environment.NewLine) : (";" + text));
			stringBuilder.Append(value);
		}
		return stringBuilder.ToString();
	}

	public void Append(params string[] comments)
	{
		Comments.AddRange(comments);
	}
}
