namespace XUnity.AutoTranslator.Plugin;

internal class SceneInformation
{
	public int Id { get; set; }

	public string Name { get; set; }

	public SceneInformation(int id, string name)
	{
		Id = id;
		Name = name;
	}
}
