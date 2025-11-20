namespace System.Data;

/// <summary>Specifies how query command results are applied to the row being updated.</summary>
/// <filterpriority>2</filterpriority>
public enum UpdateRowSource
{
	/// <summary>Any returned parameters or rows are ignored.</summary>
	None,
	/// <summary>Output parameters are mapped to the changed row in the <see cref="T:System.Data.DataSet" />.</summary>
	OutputParameters,
	/// <summary>The data in the first returned row is mapped to the changed row in the <see cref="T:System.Data.DataSet" />.</summary>
	FirstReturnedRecord,
	/// <summary>Both the output parameters and the first returned row are mapped to the changed row in the <see cref="T:System.Data.DataSet" />.</summary>
	Both
}
