namespace System.ComponentModel.DataAnnotations;

/// <summary>Specifies the column that is displayed in the referred table as a foreign-key column.</summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class DisplayColumnAttribute : Attribute
{
	/// <summary>Gets the name of the column to use as the display field.</summary>
	/// <returns>The name of the display column.</returns>
	public string DisplayColumn { get; private set; }

	/// <summary>Gets the name of the column to use for sorting.</summary>
	/// <returns>The name of the sort column.</returns>
	public string SortColumn { get; private set; }

	/// <summary>Gets a value that indicates whether to sort in descending or ascending order.</summary>
	/// <returns>true if the column will be sorted in descending order; otherwise, false.</returns>
	public bool SortDescending { get; private set; }

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.DisplayColumnAttribute" /> class by using the specified column. </summary>
	/// <param name="displayColumn">The name of the column to use as the display column.</param>
	public DisplayColumnAttribute(string displayColumn)
		: this(displayColumn, null)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.DisplayColumnAttribute" /> class by using the specified display and sort columns. </summary>
	/// <param name="displayColumn">The name of the column to use as the display column.</param>
	/// <param name="sortColumn">The name of the column to use for sorting.</param>
	public DisplayColumnAttribute(string displayColumn, string sortColumn)
		: this(displayColumn, sortColumn, sortDescending: false)
	{
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.ComponentModel.DataAnnotations.DisplayColumnAttribute" /> class by using the specified display column, and the specified sort column and sort order. </summary>
	/// <param name="displayColumn">The name of the column to use as the display column.</param>
	/// <param name="sortColumn">The name of the column to use for sorting.</param>
	/// <param name="sortDescending">true to sort in descending order; otherwise, false. The default is false.</param>
	public DisplayColumnAttribute(string displayColumn, string sortColumn, bool sortDescending)
	{
		DisplayColumn = displayColumn;
		SortColumn = sortColumn;
		SortDescending = sortDescending;
	}
}
