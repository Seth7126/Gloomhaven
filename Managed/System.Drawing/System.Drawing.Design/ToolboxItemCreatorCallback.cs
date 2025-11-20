namespace System.Drawing.Design;

/// <summary>Provides a callback mechanism that can create a <see cref="T:System.Drawing.Design.ToolboxItem" />.</summary>
/// <returns>The deserialized <see cref="T:System.Drawing.Design.ToolboxItem" /> object specified by <paramref name="serializedObject" />.</returns>
/// <param name="serializedObject">The object which contains the data to create a <see cref="T:System.Drawing.Design.ToolboxItem" /> for. </param>
/// <param name="format">The name of the clipboard data format to create a <see cref="T:System.Drawing.Design.ToolboxItem" /> for. </param>
public delegate ToolboxItem ToolboxItemCreatorCallback(object serializedObject, string format);
