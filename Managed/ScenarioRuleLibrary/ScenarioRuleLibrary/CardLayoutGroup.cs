using System.Collections.Generic;

namespace ScenarioRuleLibrary;

public class CardLayoutGroup
{
	public enum GroupTypes
	{
		Row,
		Column,
		Consume,
		Element,
		Summon,
		Augment,
		Duration,
		XP,
		Song,
		Doom
	}

	public enum DataTypes
	{
		None,
		RowCollection,
		ColumnCollection,
		RowData,
		Consume,
		Element,
		Summon,
		Augment,
		Duration,
		XP,
		Song,
		Doom
	}

	public class ColumnAttributes
	{
		public int[] ColumnWidths { get; private set; }

		public int ColumnSpacing { get; private set; }

		public ColumnAttributes()
		{
			ColumnSpacing = 0;
			ColumnWidths = new int[0];
		}

		public ColumnAttributes(int[] colWidths, int colSpacing)
		{
			ColumnWidths = colWidths;
			ColumnSpacing = colSpacing;
		}
	}

	public class RowAttributes
	{
		public bool AutoSize { get; set; }

		public int AutoSizeMin { get; set; }

		public int AutoSizeMax { get; set; }

		public int CharSpacing { get; set; }

		public int WordSpacing { get; set; }

		public int LineSpacing { get; set; }

		public int ParSpacing { get; set; }

		public string Alignment { get; set; }

		public bool Wrapping { get; set; }

		public string Overflow { get; set; }

		public RowAttributes(bool autosize = false, int autosizemin = 0, int autosizemax = 0, int charspacing = 0, int wordspacing = 0, int linespacing = 0, int parspacing = 0, string alignment = "Center", bool wrapping = true, string overflow = "Overflow")
		{
			AutoSize = autosize;
			AutoSizeMin = autosizemin;
			AutoSizeMax = autosizemax;
			CharSpacing = charspacing;
			WordSpacing = wordspacing;
			LineSpacing = linespacing;
			ParSpacing = parspacing;
			Alignment = alignment;
			Wrapping = wrapping;
			Overflow = overflow;
		}
	}

	public class SummonLayout
	{
		public CAbility SummonAbility { get; private set; }

		public string SummonName { get; private set; }

		public CardLayout SpecialText { get; private set; }

		public bool HideSummonBoxes { get; private set; }

		public SummonLayout(CAbility summonAbility, string summonName, CardLayout specialText, bool hideSummonBoxes)
		{
			SummonAbility = summonAbility;
			SummonName = summonName;
			SpecialText = specialText;
			HideSummonBoxes = hideSummonBoxes;
		}
	}

	public class AugmentLayout
	{
		public string AugmentIcon { get; private set; }

		public string DiscardText { get; private set; }

		public CardLayout AugmentAbilityLayout { get; private set; }

		public CardLayout NormalContentLayout { get; private set; }

		public AugmentLayout(string augmentIcon, string discardText, CardLayout augmentAbilityLayout, CardLayout normalContentLayout)
		{
			AugmentIcon = augmentIcon;
			DiscardText = discardText;
			AugmentAbilityLayout = augmentAbilityLayout;
			NormalContentLayout = normalContentLayout;
		}
	}

	public class DoomLayout
	{
		public CardLayout DoomAbilityLayout { get; private set; }

		public string ReminderTextOverride { get; private set; }

		public string DoomIcon { get; private set; }

		public string DiscardText { get; private set; }

		public DoomLayout(string doomIcon, string discardText, CardLayout doomAbilityLayout, string reminderTextOverride)
		{
			DoomAbilityLayout = doomAbilityLayout;
			ReminderTextOverride = reminderTextOverride;
			DoomIcon = doomIcon;
			DiscardText = discardText;
		}
	}

	public class SongLayout
	{
		public string SongIcon { get; private set; }

		public string DiscardText { get; private set; }

		public CardLayout SongAbilityLayout { get; private set; }

		public SongLayout(string songIcon, string discardText, CardLayout songAbilityLayout)
		{
			SongIcon = songIcon;
			DiscardText = discardText;
			SongAbilityLayout = songAbilityLayout;
		}
	}

	public bool IsCommand;

	public GroupTypes GroupType { get; private set; }

	public DataTypes DataType { get; private set; }

	public List<CardLayoutGroup> Collection { get; private set; }

	public CardLayoutRow Data { get; private set; }

	public AbilityConsume Consume { get; private set; }

	public SummonLayout Summon { get; private set; }

	public AugmentLayout Augment { get; private set; }

	public DoomLayout Doom { get; private set; }

	public SongLayout Song { get; private set; }

	public List<ElementInfusionBoardManager.EElement> Elements { get; private set; }

	public List<int> XP { get; private set; }

	public CActiveBonus.EActiveBonusDurationType Duration { get; private set; }

	public ColumnAttributes ColAtt { get; private set; }

	public RowAttributes RowAtt { get; private set; }

	public void CreateRowCollection(List<CardLayoutGroup> collection)
	{
		DataType = DataTypes.RowCollection;
		Collection = collection;
	}

	public void CreateColumnCollection(List<CardLayoutGroup> collection, ColumnAttributes columnAttributes)
	{
		DataType = DataTypes.ColumnCollection;
		Collection = collection;
		ColAtt = columnAttributes;
	}

	public void CreateRowData(CardLayoutRow rowData, RowAttributes rowAttributes)
	{
		DataType = DataTypes.RowData;
		Data = rowData;
		RowAtt = rowAttributes;
	}

	public void CreateConsumeData(AbilityConsume consume)
	{
		DataType = DataTypes.Consume;
		Consume = consume;
	}

	public void CreateElementData(List<ElementInfusionBoardManager.EElement> elements)
	{
		DataType = DataTypes.Element;
		Elements = elements;
	}

	public void CreateSummonData(SummonLayout summonLayout)
	{
		DataType = DataTypes.Summon;
		Summon = summonLayout;
	}

	public void CreateAugmentData(AugmentLayout augmentLayout)
	{
		DataType = DataTypes.Augment;
		Augment = augmentLayout;
	}

	public void CreateDoomData(DoomLayout doomLayout)
	{
		DataType = DataTypes.Doom;
		Doom = doomLayout;
	}

	public void CreateSongData(SongLayout songLayout)
	{
		DataType = DataTypes.Song;
		Song = songLayout;
	}

	public void CreateDurationData(CActiveBonus.EActiveBonusDurationType duration)
	{
		DataType = DataTypes.Duration;
		Duration = duration;
	}

	public void CreateXPData(List<int> xp)
	{
		DataType = DataTypes.XP;
		XP = xp;
	}

	public CardLayoutGroup(GroupTypes groupTypes)
	{
		GroupType = groupTypes;
	}

	public CardLayoutGroup(CardLayoutGroup group)
	{
		GroupType = group.GroupType;
		DataType = group.DataType;
		Collection = group.Collection;
		RowAtt = group.RowAtt;
		ColAtt = group.ColAtt;
		Data = group.Data;
	}
}
