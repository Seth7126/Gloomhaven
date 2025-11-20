namespace System.Drawing.Printing;

/// <summary>Specifies the standard paper sizes.</summary>
public enum PaperKind
{
	/// <summary>The paper size is defined by the user.</summary>
	Custom = 0,
	/// <summary>Letter paper (8.5 in. by 11 in.).</summary>
	Letter = 1,
	/// <summary>Legal paper (8.5 in. by 14 in.).</summary>
	Legal = 5,
	/// <summary>A4 paper (210 mm by 297 mm).</summary>
	A4 = 9,
	/// <summary>C paper (17 in. by 22 in.).</summary>
	CSheet = 24,
	/// <summary>D paper (22 in. by 34 in.).</summary>
	DSheet = 25,
	/// <summary>E paper (34 in. by 44 in.).</summary>
	ESheet = 26,
	/// <summary>Letter small paper (8.5 in. by 11 in.).</summary>
	LetterSmall = 2,
	/// <summary>Tabloid paper (11 in. by 17 in.).</summary>
	Tabloid = 3,
	/// <summary>Ledger paper (17 in. by 11 in.).</summary>
	Ledger = 4,
	/// <summary>Statement paper (5.5 in. by 8.5 in.).</summary>
	Statement = 6,
	/// <summary>Executive paper (7.25 in. by 10.5 in.).</summary>
	Executive = 7,
	/// <summary>A3 paper (297 mm by 420 mm).</summary>
	A3 = 8,
	/// <summary>A4 small paper (210 mm by 297 mm).</summary>
	A4Small = 10,
	/// <summary>A5 paper (148 mm by 210 mm).</summary>
	A5 = 11,
	/// <summary>B4 paper (250 mm by 353 mm).</summary>
	B4 = 12,
	/// <summary>B5 paper (176 mm by 250 mm).</summary>
	B5 = 13,
	/// <summary>Folio paper (8.5 in. by 13 in.).</summary>
	Folio = 14,
	/// <summary>Quarto paper (215 mm by 275 mm).</summary>
	Quarto = 15,
	/// <summary>Standard paper (10 in. by 14 in.).</summary>
	Standard10x14 = 16,
	/// <summary>Standard paper (11 in. by 17 in.).</summary>
	Standard11x17 = 17,
	/// <summary>Note paper (8.5 in. by 11 in.).</summary>
	Note = 18,
	/// <summary>#9 envelope (3.875 in. by 8.875 in.).</summary>
	Number9Envelope = 19,
	/// <summary>#10 envelope (4.125 in. by 9.5 in.).</summary>
	Number10Envelope = 20,
	/// <summary>#11 envelope (4.5 in. by 10.375 in.).</summary>
	Number11Envelope = 21,
	/// <summary>#12 envelope (4.75 in. by 11 in.).</summary>
	Number12Envelope = 22,
	/// <summary>#14 envelope (5 in. by 11.5 in.).</summary>
	Number14Envelope = 23,
	/// <summary>DL envelope (110 mm by 220 mm).</summary>
	DLEnvelope = 27,
	/// <summary>C5 envelope (162 mm by 229 mm).</summary>
	C5Envelope = 28,
	/// <summary>C3 envelope (324 mm by 458 mm).</summary>
	C3Envelope = 29,
	/// <summary>C4 envelope (229 mm by 324 mm).</summary>
	C4Envelope = 30,
	/// <summary>C6 envelope (114 mm by 162 mm).</summary>
	C6Envelope = 31,
	/// <summary>C65 envelope (114 mm by 229 mm).</summary>
	C65Envelope = 32,
	/// <summary>B4 envelope (250 mm by 353 mm).</summary>
	B4Envelope = 33,
	/// <summary>B5 envelope (176 mm by 250 mm).</summary>
	B5Envelope = 34,
	/// <summary>B6 envelope (176 mm by 125 mm).</summary>
	B6Envelope = 35,
	/// <summary>Italy envelope (110 mm by 230 mm).</summary>
	ItalyEnvelope = 36,
	/// <summary>Monarch envelope (3.875 in. by 7.5 in.).</summary>
	MonarchEnvelope = 37,
	/// <summary>6 3/4 envelope (3.625 in. by 6.5 in.).</summary>
	PersonalEnvelope = 38,
	/// <summary>US standard fanfold (14.875 in. by 11 in.).</summary>
	USStandardFanfold = 39,
	/// <summary>German standard fanfold (8.5 in. by 12 in.).</summary>
	GermanStandardFanfold = 40,
	/// <summary>German legal fanfold (8.5 in. by 13 in.).</summary>
	GermanLegalFanfold = 41,
	/// <summary>ISO B4 (250 mm by 353 mm).</summary>
	IsoB4 = 42,
	/// <summary>Japanese postcard (100 mm by 148 mm).</summary>
	JapanesePostcard = 43,
	/// <summary>Standard paper (9 in. by 11 in.).</summary>
	Standard9x11 = 44,
	/// <summary>Standard paper (10 in. by 11 in.).</summary>
	Standard10x11 = 45,
	/// <summary>Standard paper (15 in. by 11 in.).</summary>
	Standard15x11 = 46,
	/// <summary>Invitation envelope (220 mm by 220 mm).</summary>
	InviteEnvelope = 47,
	/// <summary>Letter extra paper (9.275 in. by 12 in.). This value is specific to the PostScript driver and is used only by Linotronic printers in order to conserve paper.</summary>
	LetterExtra = 50,
	/// <summary>Legal extra paper (9.275 in. by 15 in.). This value is specific to the PostScript driver and is used only by Linotronic printers in order to conserve paper.</summary>
	LegalExtra = 51,
	/// <summary>Tabloid extra paper (11.69 in. by 18 in.). This value is specific to the PostScript driver and is used only by Linotronic printers in order to conserve paper.</summary>
	TabloidExtra = 52,
	/// <summary>A4 extra paper (236 mm by 322 mm). This value is specific to the PostScript driver and is used only by Linotronic printers to help save paper.</summary>
	A4Extra = 53,
	/// <summary>Letter transverse paper (8.275 in. by 11 in.).</summary>
	LetterTransverse = 54,
	/// <summary>A4 transverse paper (210 mm by 297 mm).</summary>
	A4Transverse = 55,
	/// <summary>Letter extra transverse paper (9.275 in. by 12 in.).</summary>
	LetterExtraTransverse = 56,
	/// <summary>SuperA/SuperA/A4 paper (227 mm by 356 mm).</summary>
	APlus = 57,
	/// <summary>SuperB/SuperB/A3 paper (305 mm by 487 mm).</summary>
	BPlus = 58,
	/// <summary>Letter plus paper (8.5 in. by 12.69 in.).</summary>
	LetterPlus = 59,
	/// <summary>A4 plus paper (210 mm by 330 mm).</summary>
	A4Plus = 60,
	/// <summary>A5 transverse paper (148 mm by 210 mm).</summary>
	A5Transverse = 61,
	/// <summary>JIS B5 transverse paper (182 mm by 257 mm).</summary>
	B5Transverse = 62,
	/// <summary>A3 extra paper (322 mm by 445 mm).</summary>
	A3Extra = 63,
	/// <summary>A5 extra paper (174 mm by 235 mm).</summary>
	A5Extra = 64,
	/// <summary>ISO B5 extra paper (201 mm by 276 mm).</summary>
	B5Extra = 65,
	/// <summary>A2 paper (420 mm by 594 mm).</summary>
	A2 = 66,
	/// <summary>A3 transverse paper (297 mm by 420 mm).</summary>
	A3Transverse = 67,
	/// <summary>A3 extra transverse paper (322 mm by 445 mm).</summary>
	A3ExtraTransverse = 68,
	/// <summary>Japanese double postcard (200 mm by 148 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	JapaneseDoublePostcard = 69,
	/// <summary>A6 paper (105 mm by 148 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	A6 = 70,
	/// <summary>Japanese Kaku #2 envelope. Requires Windows 98, Windows NT 4.0, or later.</summary>
	JapaneseEnvelopeKakuNumber2 = 71,
	/// <summary>Japanese Kaku #3 envelope. Requires Windows 98, Windows NT 4.0, or later.</summary>
	JapaneseEnvelopeKakuNumber3 = 72,
	/// <summary>Japanese Chou #3 envelope. Requires Windows 98, Windows NT 4.0, or later.</summary>
	JapaneseEnvelopeChouNumber3 = 73,
	/// <summary>Japanese Chou #4 envelope. Requires Windows 98, Windows NT 4.0, or later.</summary>
	JapaneseEnvelopeChouNumber4 = 74,
	/// <summary>Letter rotated paper (11 in. by 8.5 in.).</summary>
	LetterRotated = 75,
	/// <summary>A3 rotated paper (420 mm by 297 mm).</summary>
	A3Rotated = 76,
	/// <summary>A4 rotated paper (297 mm by 210 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	A4Rotated = 77,
	/// <summary>A5 rotated paper (210 mm by 148 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	A5Rotated = 78,
	/// <summary>JIS B4 rotated paper (364 mm by 257 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	B4JisRotated = 79,
	/// <summary>JIS B5 rotated paper (257 mm by 182 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	B5JisRotated = 80,
	/// <summary>Japanese rotated postcard (148 mm by 100 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	JapanesePostcardRotated = 81,
	/// <summary>Japanese rotated double postcard (148 mm by 200 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	JapaneseDoublePostcardRotated = 82,
	/// <summary>A6 rotated paper (148 mm by 105 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	A6Rotated = 83,
	/// <summary>Japanese rotated Kaku #2 envelope. Requires Windows 98, Windows NT 4.0, or later.</summary>
	JapaneseEnvelopeKakuNumber2Rotated = 84,
	/// <summary>Japanese rotated Kaku #3 envelope. Requires Windows 98, Windows NT 4.0, or later.</summary>
	JapaneseEnvelopeKakuNumber3Rotated = 85,
	/// <summary>Japanese rotated Chou #3 envelope. Requires Windows 98, Windows NT 4.0, or later.</summary>
	JapaneseEnvelopeChouNumber3Rotated = 86,
	/// <summary>Japanese rotated Chou #4 envelope. Requires Windows 98, Windows NT 4.0, or later.</summary>
	JapaneseEnvelopeChouNumber4Rotated = 87,
	/// <summary>JIS B6 paper (128 mm by 182 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	B6Jis = 88,
	/// <summary>JIS B6 rotated paper (182 mm by 128 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	B6JisRotated = 89,
	/// <summary>Standard paper (12 in. by 11 in.). Requires Windows 98, Windows NT 4.0, or later.</summary>
	Standard12x11 = 90,
	/// <summary>Japanese You #4 envelope. Requires Windows 98, Windows NT 4.0, or later.</summary>
	JapaneseEnvelopeYouNumber4 = 91,
	/// <summary>Japanese You #4 rotated envelope. Requires Windows 98, Windows NT 4.0, or later.</summary>
	JapaneseEnvelopeYouNumber4Rotated = 92,
	/// <summary>People's Republic of China 16K paper (146 mm by 215 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	Prc16K = 93,
	/// <summary>People's Republic of China 32K paper (97 mm by 151 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	Prc32K = 94,
	/// <summary>People's Republic of China 32K big paper (97 mm by 151 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	Prc32KBig = 95,
	/// <summary>People's Republic of China #1 envelope (102 mm by 165 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber1 = 96,
	/// <summary>People's Republic of China #2 envelope (102 mm by 176 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber2 = 97,
	/// <summary>People's Republic of China #3 envelope (125 mm by 176 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber3 = 98,
	/// <summary>People's Republic of China #4 envelope (110 mm by 208 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber4 = 99,
	/// <summary>People's Republic of China #5 envelope (110 mm by 220 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber5 = 100,
	/// <summary>People's Republic of China #6 envelope (120 mm by 230 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber6 = 101,
	/// <summary>People's Republic of China #7 envelope (160 mm by 230 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber7 = 102,
	/// <summary>People's Republic of China #8 envelope (120 mm by 309 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber8 = 103,
	/// <summary>People's Republic of China #9 envelope (229 mm by 324 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber9 = 104,
	/// <summary>People's Republic of China #10 envelope (324 mm by 458 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber10 = 105,
	/// <summary>People's Republic of China 16K rotated paper (146 mm by 215 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	Prc16KRotated = 106,
	/// <summary>People's Republic of China 32K rotated paper (97 mm by 151 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	Prc32KRotated = 107,
	/// <summary>People's Republic of China 32K big rotated paper (97 mm by 151 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	Prc32KBigRotated = 108,
	/// <summary>People's Republic of China #1 rotated envelope (165 mm by 102 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber1Rotated = 109,
	/// <summary>People's Republic of China #2 rotated envelope (176 mm by 102 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber2Rotated = 110,
	/// <summary>People's Republic of China #3 rotated envelope (176 mm by 125 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber3Rotated = 111,
	/// <summary>People's Republic of China #4 rotated envelope (208 mm by 110 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber4Rotated = 112,
	/// <summary>People's Republic of China Envelope #5 rotated envelope (220 mm by 110 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber5Rotated = 113,
	/// <summary>People's Republic of China #6 rotated envelope (230 mm by 120 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber6Rotated = 114,
	/// <summary>People's Republic of China #7 rotated envelope (230 mm by 160 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber7Rotated = 115,
	/// <summary>People's Republic of China #8 rotated envelope (309 mm by 120 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber8Rotated = 116,
	/// <summary>People's Republic of China #9 rotated envelope (324 mm by 229 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber9Rotated = 117,
	/// <summary>People's Republic of China #10 rotated envelope (458 mm by 324 mm). Requires Windows 98, Windows NT 4.0, or later.</summary>
	PrcEnvelopeNumber10Rotated = 118
}
