using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.TextCore.LowLevel;

namespace UnityEngine.TextCore.Text;

[Serializable]
public class FontFeatureTable
{
	[SerializeField]
	internal List<GlyphPairAdjustmentRecord> m_GlyphPairAdjustmentRecords;

	internal Dictionary<uint, GlyphPairAdjustmentRecord> m_GlyphPairAdjustmentRecordLookup;

	internal List<GlyphPairAdjustmentRecord> glyphPairAdjustmentRecords
	{
		get
		{
			return m_GlyphPairAdjustmentRecords;
		}
		set
		{
			m_GlyphPairAdjustmentRecords = value;
		}
	}

	public FontFeatureTable()
	{
		m_GlyphPairAdjustmentRecords = new List<GlyphPairAdjustmentRecord>();
		m_GlyphPairAdjustmentRecordLookup = new Dictionary<uint, GlyphPairAdjustmentRecord>();
	}

	public void SortGlyphPairAdjustmentRecords()
	{
		if (m_GlyphPairAdjustmentRecords.Count > 0)
		{
			m_GlyphPairAdjustmentRecords = (from s in m_GlyphPairAdjustmentRecords
				orderby s.firstAdjustmentRecord.glyphIndex, s.secondAdjustmentRecord.glyphIndex
				select s).ToList();
		}
	}
}
