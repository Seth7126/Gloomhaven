using System.Collections.Generic;

namespace FrameTimeLogger;

public class SimpleCSVConverter : ICSVConverter
{
	public string ConvertToString<T>(IEnumerable<Column<T>> columns)
	{
		string text = string.Empty;
		int num = 0;
		foreach (Column<T> column in columns)
		{
			text = text + column.Name + ";";
			if (num < column.Values.Length)
			{
				num = column.Values.Length;
			}
		}
		for (int i = 0; i < num; i++)
		{
			text += "\n";
			foreach (Column<T> column2 in columns)
			{
				string text2 = ((column2.Values.Length > i) ? column2.Values[i].ToString() : "");
				text = text + text2 + ";";
			}
		}
		return text;
	}
}
