namespace System.Data.Common;

internal class DbProviderFactoryConfigSection
{
	private Type factType;

	private string name;

	private string invariantName;

	private string description;

	private string assemblyQualifiedName;

	public string Name => name;

	public string InvariantName => invariantName;

	public string Description => description;

	public string AssemblyQualifiedName => assemblyQualifiedName;

	public DbProviderFactoryConfigSection(Type FactoryType, string FactoryName, string FactoryDescription)
	{
		try
		{
			factType = FactoryType;
			name = FactoryName;
			invariantName = factType.Namespace.ToString();
			description = FactoryDescription;
			assemblyQualifiedName = factType.AssemblyQualifiedName.ToString();
		}
		catch
		{
			factType = null;
			name = string.Empty;
			invariantName = string.Empty;
			description = string.Empty;
			assemblyQualifiedName = string.Empty;
		}
	}

	public DbProviderFactoryConfigSection(string FactoryName, string FactoryInvariantName, string FactoryDescription, string FactoryAssemblyQualifiedName)
	{
		factType = null;
		name = FactoryName;
		invariantName = FactoryInvariantName;
		description = FactoryDescription;
		assemblyQualifiedName = FactoryAssemblyQualifiedName;
	}

	public bool IsNull()
	{
		if (factType == null && invariantName == string.Empty)
		{
			return true;
		}
		return false;
	}
}
