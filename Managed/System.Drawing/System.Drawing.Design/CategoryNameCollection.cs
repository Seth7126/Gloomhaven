using System.Collections;

namespace System.Drawing.Design;

/// <summary>Represents a collection of category name strings.</summary>
public sealed class CategoryNameCollection : ReadOnlyCollectionBase
{
	/// <summary>Gets the category name at the specified index.</summary>
	/// <returns>The category name at the specified index.</returns>
	/// <param name="index">The index of the collection element to access. </param>
	public string this[int index] => (string)base.InnerList[index];

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Design.CategoryNameCollection" /> class using the specified collection.</summary>
	/// <param name="value">A <see cref="T:System.Drawing.Design.CategoryNameCollection" /> that contains the names to initialize the collection values to. </param>
	public CategoryNameCollection(CategoryNameCollection value)
	{
		base.InnerList.AddRange(value);
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Design.CategoryNameCollection" /> class using the specified array of names.</summary>
	/// <param name="value">An array of strings that contains the names of the categories to initialize the collection values to. </param>
	public CategoryNameCollection(string[] value)
	{
		base.InnerList.AddRange(value);
	}

	/// <summary>Indicates whether the specified category is contained in the collection.</summary>
	/// <returns>true if the specified category is contained in the collection; otherwise, false.</returns>
	/// <param name="value">The string to check for in the collection. </param>
	public bool Contains(string value)
	{
		return base.InnerList.Contains(value);
	}

	/// <summary>Copies the collection elements to the specified array at the specified index.</summary>
	/// <param name="array">The array to copy to. </param>
	/// <param name="index">The index of the destination array at which to begin copying. </param>
	public void CopyTo(string[] array, int index)
	{
		base.InnerList.CopyTo(array, index);
	}

	/// <summary>Gets the index of the specified value.</summary>
	/// <returns>The index in the collection, or null if the string does not exist in the collection.</returns>
	/// <param name="value">The category name to retrieve the index of in the collection. </param>
	public int IndexOf(string value)
	{
		return base.InnerList.IndexOf(value);
	}
}
