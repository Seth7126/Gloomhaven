namespace System.Drawing.Printing;

/// <summary>Standard paper sources.</summary>
public enum PaperSourceKind
{
	/// <summary>The upper bin of a printer (or the default bin, if the printer only has one bin).</summary>
	Upper = 1,
	/// <summary>The lower bin of a printer.</summary>
	Lower = 2,
	/// <summary>The middle bin of a printer.</summary>
	Middle = 3,
	/// <summary>Manually fed paper.</summary>
	Manual = 4,
	/// <summary>An envelope.</summary>
	Envelope = 5,
	/// <summary>Manually fed envelope.</summary>
	ManualFeed = 6,
	/// <summary>Automatically fed paper.</summary>
	AutomaticFeed = 7,
	/// <summary>A tractor feed.</summary>
	TractorFeed = 8,
	/// <summary>Small-format paper.</summary>
	SmallFormat = 9,
	/// <summary>Large-format paper.</summary>
	LargeFormat = 10,
	/// <summary>The printer's large-capacity bin.</summary>
	LargeCapacity = 11,
	/// <summary>A paper cassette.</summary>
	Cassette = 14,
	/// <summary>The printer's default input bin.</summary>
	FormSource = 15,
	/// <summary>A printer-specific paper source.</summary>
	Custom = 257
}
