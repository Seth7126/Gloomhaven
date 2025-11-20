namespace System.Data;

/// <summary>Defines the extension methods to the <see cref="T:System.Data.DataRow" /> class. This is a static class.   </summary>
public static class DataRowExtensions
{
	private static class UnboxT<T>
	{
		internal static readonly Converter<object, T> s_unbox = Create();

		private static Converter<object, T> Create()
		{
			if (default(T) == null)
			{
				return ReferenceOrNullableField;
			}
			return ValueField;
		}

		private static T ReferenceOrNullableField(object value)
		{
			if (DBNull.Value != value)
			{
				return (T)value;
			}
			return default(T);
		}

		private static T ValueField(object value)
		{
			if (DBNull.Value == value)
			{
				throw DataSetUtil.InvalidCast($"Cannot cast DBNull. Value to type '{typeof(T).ToString()}'. Please use a nullable type.");
			}
			return (T)value;
		}
	}

	/// <summary>Provides strongly-typed access to each of the column values in the specified row. The <see cref="M:System.Data.DataRowExtensions.Field``1(System.Data.DataRow,System.String)" /> method also supports nullable types. </summary>
	/// <returns>The value, of type <paramref name="T" />, of the <see cref="T:System.Data.DataColumn" /> specified by <paramref name="columnName" />.</returns>
	/// <param name="row">The input <see cref="T:System.Data.DataRow" />, which acts as the this instance for the extension method.</param>
	/// <param name="columnName">The name of the column to return the value of.</param>
	/// <typeparam name="T">A generic parameter that specifies the return type of the column.</typeparam>
	/// <exception cref="T:System.InvalidCastException">The value type of the underlying column could not be cast to the type specified by the generic parameter, <paramref name="T" />.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">The column specified by <paramref name="columnName" /> does not occur in the <see cref="T:System.Data.DataTable" /> that the <see cref="T:System.Data.DataRow" /> is a part of.</exception>
	/// <exception cref="T:System.NullReferenceException">A null value was assigned to a non-nullable type.</exception>
	public static T Field<T>(this DataRow row, string columnName)
	{
		DataSetUtil.CheckArgumentNull(row, "row");
		return UnboxT<T>.s_unbox(row[columnName]);
	}

	/// <summary>Provides strongly-typed access to each of the column values in the specified row. The <see cref="M:System.Data.DataRowExtensions.Field``1(System.Data.DataRow,System.Data.DataColumn)" /> method also supports nullable types. </summary>
	/// <returns>The value, of type <paramref name="T" />, of the <see cref="T:System.Data.DataColumn" /> specified by <paramref name="column" />.</returns>
	/// <param name="row">The input <see cref="T:System.Data.DataRow" />, which acts as the this instance for the extension method.</param>
	/// <param name="column">The input <see cref="T:System.Data.DataColumn" /> object that specifies the column to return the value of.</param>
	/// <typeparam name="T">A generic parameter that specifies the return type of the column.</typeparam>
	/// <exception cref="T:System.InvalidCastException">The value type of the underlying column could not be cast to the type specified by the generic parameter, <paramref name="T" />.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">The column specified by <paramref name="column" /> does not occur in the <see cref="T:System.Data.DataTable" /> that the <see cref="T:System.Data.DataRow" /> is a part of.</exception>
	/// <exception cref="T:System.NullReferenceException">A null value was assigned to a non-nullable type.</exception>
	public static T Field<T>(this DataRow row, DataColumn column)
	{
		DataSetUtil.CheckArgumentNull(row, "row");
		return UnboxT<T>.s_unbox(row[column]);
	}

	/// <summary>Provides strongly-typed access to each of the column values in the specified row. The <see cref="M:System.Data.DataRowExtensions.Field``1(System.Data.DataRow,System.Int32)" /> method also supports nullable types. </summary>
	/// <returns>The value, of type <paramref name="T" />, of the <see cref="T:System.Data.DataColumn" /> specified by <paramref name="columnIndex" />.</returns>
	/// <param name="row">The input <see cref="T:System.Data.DataRow" />, which acts as the this instance for the extension method.</param>
	/// <param name="columnIndex">The column index.</param>
	/// <typeparam name="T">A generic parameter that specifies the return type of the column.</typeparam>
	/// <exception cref="T:System.InvalidCastException">The value type of the underlying column could not be cast to the type specified by the generic parameter, <paramref name="T" />.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">The column specified by <paramref name="ordinal" /> does not exist in the <see cref="T:System.Data.DataTable" /> that the <see cref="T:System.Data.DataRow" /> is a part of.</exception>
	/// <exception cref="T:System.NullReferenceException">A null value was assigned to a non-nullable type.</exception>
	public static T Field<T>(this DataRow row, int columnIndex)
	{
		DataSetUtil.CheckArgumentNull(row, "row");
		return UnboxT<T>.s_unbox(row[columnIndex]);
	}

	/// <summary>Provides strongly-typed access to each of the column values in the specified row. The <see cref="M:System.Data.DataRowExtensions.Field``1(System.Data.DataRow,System.Int32,System.Data.DataRowVersion)" /> method also supports nullable types. </summary>
	/// <returns>The value, of type <paramref name="T" />, of the <see cref="T:System.Data.DataColumn" /> specified by <paramref name="ordinal" /> and <paramref name="version" />.</returns>
	/// <param name="row">The input <see cref="T:System.Data.DataRow" />, which acts as the this instance for the extension method.</param>
	/// <param name="columnIndex">The zero-based ordinal of the column to return the value of.</param>
	/// <param name="version">A <see cref="T:System.Data.DataRowVersion" /> enumeration that specifies the version of the column value to return, such as Current or Original version.</param>
	/// <typeparam name="T">A generic parameter that specifies the return type of the column.</typeparam>
	/// <exception cref="T:System.InvalidCastException">The value type of the underlying column could not be cast to the type specified by the generic parameter, <paramref name="T" />.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">The column specified by <paramref name="ordinal" /> does not exist in the <see cref="T:System.Data.DataTable" /> that the <see cref="T:System.Data.DataRow" /> is a part of.</exception>
	/// <exception cref="T:System.NullReferenceException">A null value was assigned to a non-nullable type.</exception>
	public static T Field<T>(this DataRow row, int columnIndex, DataRowVersion version)
	{
		DataSetUtil.CheckArgumentNull(row, "row");
		return UnboxT<T>.s_unbox(row[columnIndex, version]);
	}

	/// <summary>Provides strongly-typed access to each of the column values in the specified row. The <see cref="M:System.Data.DataRowExtensions.Field``1(System.Data.DataRow,System.String,System.Data.DataRowVersion)" /> method also supports nullable types. </summary>
	/// <returns>The value, of type <paramref name="T" />, of the <see cref="T:System.Data.DataColumn" /> specified by <paramref name="columnName" /> and <paramref name="version" />.</returns>
	/// <param name="row">The input <see cref="T:System.Data.DataRow" />, which acts as the this instance for the extension method.</param>
	/// <param name="columnName">The name of the column to return the value of.</param>
	/// <param name="version">A <see cref="T:System.Data.DataRowVersion" /> enumeration that specifies the version of the column value to return, such as Current or Original version.</param>
	/// <typeparam name="T">A generic parameter that specifies the return type of the column.</typeparam>
	/// <exception cref="T:System.InvalidCastException">The value type of the underlying column could not be cast to the type specified by the generic parameter, <paramref name="T" />.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">The column specified by <paramref name="columnName" /> does not exist in the <see cref="T:System.Data.DataTable" /> that the <see cref="T:System.Data.DataRow" /> is a part of.</exception>
	/// <exception cref="T:System.NullReferenceException">A null value was assigned to a non-nullable type.</exception>
	public static T Field<T>(this DataRow row, string columnName, DataRowVersion version)
	{
		DataSetUtil.CheckArgumentNull(row, "row");
		return UnboxT<T>.s_unbox(row[columnName, version]);
	}

	/// <summary>Provides strongly-typed access to each of the column values in the specified row. The <see cref="M:System.Data.DataRowExtensions.Field``1(System.Data.DataRow,System.Data.DataColumn,System.Data.DataRowVersion)" /> method also supports nullable types. </summary>
	/// <returns>The value, of type <paramref name="T" />, of the <see cref="T:System.Data.DataColumn" /> specified by <paramref name="column" /> and <paramref name="version" />.</returns>
	/// <param name="row">The input <see cref="T:System.Data.DataRow" />, which acts as the this instance for the extension method.</param>
	/// <param name="column">The input <see cref="T:System.Data.DataColumn" /> object that specifies the column to return the value of.</param>
	/// <param name="version">A <see cref="T:System.Data.DataRowVersion" /> enumeration that specifies the version of the column value to return, such as Current or Original version.</param>
	/// <typeparam name="T">A generic parameter that specifies the return type of the column.</typeparam>
	/// <exception cref="T:System.InvalidCastException">The value type of the underlying column could not be cast to the type specified by the generic parameter, <paramref name="T" />.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">The column specified by <paramref name="column" /> does not exist in the <see cref="T:System.Data.DataTable" /> that the <see cref="T:System.Data.DataRow" /> is a part of.</exception>
	/// <exception cref="T:System.NullReferenceException">A null value was assigned to a non-nullable type.</exception>
	public static T Field<T>(this DataRow row, DataColumn column, DataRowVersion version)
	{
		DataSetUtil.CheckArgumentNull(row, "row");
		return UnboxT<T>.s_unbox(row[column, version]);
	}

	/// <summary>Sets a new value for the specified column in the <see cref="T:System.Data.DataRow" /> the method is called on. The <see cref="M:System.Data.DataRowExtensions.SetField``1(System.Data.DataRow,System.Int32,``0)" /> method also supports nullable types. </summary>
	/// <param name="row">The input <see cref="T:System.Data.DataRow" />, which acts as the this instance for the extension method.</param>
	/// <param name="columnIndex">The zero-based ordinal of the column to set the value of.</param>
	/// <param name="value">The new row value for the specified column, of type <paramref name="T" />.</param>
	/// <typeparam name="T">A generic parameter that specifies the value type of the column.</typeparam>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">Occurs when attempting to set a value on a deleted row.</exception>
	/// <exception cref="T:System.IndexOutOfRangeException">The <paramref name="ordinal" /> argument is out of range.</exception>
	/// <exception cref="T:System.InvalidCastException">The value type of the underlying column could be not cast to the type specified by the generic parameter, <paramref name="T" />.</exception>
	public static void SetField<T>(this DataRow row, int columnIndex, T value)
	{
		DataSetUtil.CheckArgumentNull(row, "row");
		row[columnIndex] = ((object)value) ?? DBNull.Value;
	}

	/// <summary>Sets a new value for the specified column in the <see cref="T:System.Data.DataRow" />. The <see cref="M:System.Data.DataRowExtensions.SetField``1(System.Data.DataRow,System.String,``0)" /> method also supports nullable types. </summary>
	/// <param name="row">The input <see cref="T:System.Data.DataRow" />, which acts as the this instance for the extension method.</param>
	/// <param name="columnName">The name of the column to set the value of.</param>
	/// <param name="value">The new row value for the specified column, of type <paramref name="T" />.</param>
	/// <typeparam name="T">A generic parameter that specifies the value type of the column.</typeparam>
	/// <exception cref="T:System.ArgumentException">The column specified by <paramref name="columnName" /> cannot be found.</exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">Occurs when attempting to set a value on a deleted row.</exception>
	/// <exception cref="T:System.InvalidCastException">The value type of the underlying column could not be cast to the type specified by the generic parameter, <paramref name="T" />.</exception>
	public static void SetField<T>(this DataRow row, string columnName, T value)
	{
		DataSetUtil.CheckArgumentNull(row, "row");
		row[columnName] = ((object)value) ?? DBNull.Value;
	}

	/// <summary>Sets a new value for the specified column in the <see cref="T:System.Data.DataRow" />. The <see cref="M:System.Data.DataRowExtensions.SetField``1(System.Data.DataRow,System.Data.DataColumn,``0)" /> method also supports nullable types. </summary>
	/// <param name="row">The input <see cref="T:System.Data.DataRow" />, which acts as the this instance for the extension method.</param>
	/// <param name="column">The input <see cref="T:System.Data.DataColumn" /> specifies which row value to retrieve.</param>
	/// <param name="value">The new row value for the specified column, of type <paramref name="T" />.</param>
	/// <typeparam name="T">A generic parameter that specifies the value type of the column.</typeparam>
	/// <exception cref="T:System.ArgumentException">The column specified by <paramref name="column" /> cannot be found.</exception>
	/// <exception cref="T:System.ArgumentNullException">The <paramref name="column" /> is null.</exception>
	/// <exception cref="T:System.Data.DeletedRowInaccessibleException">Occurs when attempting to set a value on a deleted row.</exception>
	/// <exception cref="T:System.InvalidCastException">The value type of the underlying column could not be cast to the type specified by the generic parameter, <paramref name="T" />.</exception>
	public static void SetField<T>(this DataRow row, DataColumn column, T value)
	{
		DataSetUtil.CheckArgumentNull(row, "row");
		row[column] = ((object)value) ?? DBNull.Value;
	}
}
