namespace Manatee.Trello.Rest;

public class RestFile
{
	public const string ParameterKey = "file";

	public string FileName { get; set; }

	public byte[] ContentBytes { get; set; }
}
