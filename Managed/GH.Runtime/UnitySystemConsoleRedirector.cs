#define ENABLE_LOGS
using System;
using System.IO;
using System.Text;

public static class UnitySystemConsoleRedirector
{
	private class UnityTextWriter : TextWriter
	{
		private StringBuilder buffer = new StringBuilder();

		public override Encoding Encoding => Encoding.Default;

		public override void Flush()
		{
			Debug.Log(buffer.ToString());
			buffer.Length = 0;
		}

		public override void Write(string value)
		{
			buffer.Append(value);
			if (value != null)
			{
				int length = value.Length;
				if (length > 0 && value[length - 1] == '\n')
				{
					Flush();
				}
			}
		}

		public override void Write(char value)
		{
			buffer.Append(value);
			if (value == '\n')
			{
				Flush();
			}
		}

		public override void Write(char[] value, int index, int count)
		{
			Write(new string(value, index, count));
		}
	}

	public static void Redirect()
	{
		Console.SetOut(new UnityTextWriter());
	}
}
