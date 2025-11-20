namespace System.Drawing.Printing;

/// <summary>Provides data for the <see cref="E:System.Drawing.Printing.PrintDocument.QueryPageSettings" /> event.</summary>
public class QueryPageSettingsEventArgs : PrintEventArgs
{
	private PageSettings _pageSettings;

	internal bool PageSettingsChanged;

	/// <summary>Gets or sets the page settings for the page to be printed.</summary>
	/// <returns>The page settings for the page to be printed.</returns>
	public PageSettings PageSettings
	{
		get
		{
			PageSettingsChanged = true;
			return _pageSettings;
		}
		set
		{
			if (value == null)
			{
				value = new PageSettings();
			}
			_pageSettings = value;
			PageSettingsChanged = true;
		}
	}

	/// <summary>Initializes a new instance of the <see cref="T:System.Drawing.Printing.QueryPageSettingsEventArgs" /> class.</summary>
	/// <param name="pageSettings">The page settings for the page to be printed. </param>
	public QueryPageSettingsEventArgs(PageSettings pageSettings)
	{
		_pageSettings = pageSettings;
	}
}
