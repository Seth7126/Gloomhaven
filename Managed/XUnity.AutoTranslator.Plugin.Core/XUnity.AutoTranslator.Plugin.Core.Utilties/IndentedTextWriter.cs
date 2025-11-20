using System.IO;

namespace XUnity.AutoTranslator.Plugin.Core.Utilties;

internal class IndentedTextWriter
{
	private readonly TextWriter _writer;

	private readonly char _indent;

	public int Indent { get; set; }

	public IndentedTextWriter(TextWriter writer, char indent)
	{
		_writer = writer;
		_indent = indent;
	}

	public void WriteLine(string line)
	{
		_writer.Write(new string(_indent, Indent));
		_writer.WriteLine(line);
	}
}
