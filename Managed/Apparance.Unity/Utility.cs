using UnityEngine;

public class Utility
{
	private string s_OldLine = "";

	public void Print(string line)
	{
		if (line != s_OldLine)
		{
			Debug.Log(line);
			s_OldLine = line;
		}
	}
}
