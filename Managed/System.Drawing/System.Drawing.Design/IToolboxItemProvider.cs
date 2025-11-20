namespace System.Drawing.Design;

/// <summary>Exposes a collection of toolbox items.</summary>
public interface IToolboxItemProvider
{
	/// <summary>Gets a collection of <see cref="T:System.Drawing.Design.ToolboxItem" /> objects.</summary>
	/// <returns>A collection of <see cref="T:System.Drawing.Design.ToolboxItem" /> objects.</returns>
	ToolboxItemCollection Items { get; }
}
