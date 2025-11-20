using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Manatee.Trello;

public static class Emojis
{
	private static readonly object _lock = new object();

	private static Dictionary<string, Emoji> _lookup;

	public static readonly Emoji Grinning = new Emoji("1F600", "GRINNING FACE", "\ud83d\ude00", "grinning", new string[1] { "grinning" }, ":D", null, "Smileys & People", 30, 24, SkinVariationType.None, "grinning face", new string[3] { "face", "grin", "grinning face" });

	public static readonly Emoji Grin = new Emoji("1F601", "GRINNING FACE WITH SMILING EYES", "\ud83d\ude01", "grin", new string[1] { "grin" }, null, null, "Smileys & People", 30, 25, SkinVariationType.None, "beaming face with smiling eyes", new string[5] { "beaming face with smiling eyes", "eye", "face", "grin", "smile" });

	public static readonly Emoji Joy = new Emoji("1F602", "FACE WITH TEARS OF JOY", "\ud83d\ude02", "joy", new string[1] { "joy" }, null, null, "Smileys & People", 30, 26, SkinVariationType.None, "face with tears of joy", new string[5] { "face", "face with tears of joy", "joy", "laugh", "tear" });

	public static readonly Emoji RollingOnTheFloorLaughing = new Emoji("1F923", "ROLLING ON THE FLOOR LAUGHING", "\ud83e\udd23", "rolling_on_the_floor_laughing", new string[1] { "rolling_on_the_floor_laughing" }, null, null, "Smileys & People", 38, 26, SkinVariationType.None, "rolling on the floor laughing", new string[5] { "face", "floor", "laugh", "rolling", "rolling on the floor laughing" });

	public static readonly Emoji Smiley = new Emoji("1F603", "SMILING FACE WITH OPEN MOUTH", "\ud83d\ude03", "smiley", new string[1] { "smiley" }, ":)", new string[2] { "=)", "=-)" }, "Smileys & People", 30, 27, SkinVariationType.None, "grinning face with big eyes", new string[5] { "face", "grinning face with big eyes", "mouth", "open", "smile" });

	public static readonly Emoji Smile = new Emoji("1F604", "SMILING FACE WITH OPEN MOUTH AND SMILING EYES", "\ud83d\ude04", "smile", new string[1] { "smile" }, ":)", new string[4] { "C:", "c:", ":D", ":-D" }, "Smileys & People", 30, 28, SkinVariationType.None, "grinning face with smiling eyes", new string[6] { "eye", "face", "grinning face with smiling eyes", "mouth", "open", "smile" });

	public static readonly Emoji SweatSmile = new Emoji("1F605", "SMILING FACE WITH OPEN MOUTH AND COLD SWEAT", "\ud83d\ude05", "sweat_smile", new string[1] { "sweat_smile" }, null, null, "Smileys & People", 30, 29, SkinVariationType.None, "grinning face with sweat", new string[6] { "cold", "face", "grinning face with sweat", "open", "smile", "sweat" });

	public static readonly Emoji Laughing = new Emoji("1F606", "SMILING FACE WITH OPEN MOUTH AND TIGHTLY-CLOSED EYES", "\ud83d\ude06", "laughing", new string[2] { "laughing", "satisfied" }, null, new string[2] { ":>", ":->" }, "Smileys & People", 30, 30, SkinVariationType.None, "grinning squinting face", new string[7] { "face", "grinning squinting face", "laugh", "mouth", "open", "satisfied", "smile" });

	public static readonly Emoji Wink = new Emoji("1F609", "WINKING FACE", "\ud83d\ude09", "wink", new string[1] { "wink" }, ";)", new string[2] { ";)", ";-)" }, "Smileys & People", 30, 33, SkinVariationType.None, "winking face", new string[3] { "face", "wink", "winking face" });

	public static readonly Emoji Blush = new Emoji("1F60A", "SMILING FACE WITH SMILING EYES", "\ud83d\ude0a", "blush", new string[1] { "blush" }, ":)", null, "Smileys & People", 30, 34, SkinVariationType.None, "smiling face with smiling eyes", new string[5] { "blush", "eye", "face", "smile", "smiling face with smiling eyes" });

	public static readonly Emoji Yum = new Emoji("1F60B", "FACE SAVOURING DELICIOUS FOOD", "\ud83d\ude0b", "yum", new string[1] { "yum" }, null, null, "Smileys & People", 30, 35, SkinVariationType.None, "face savoring food", new string[7] { "delicious", "face", "face savoring food", "savouring", "smile", "um", "yum" });

	public static readonly Emoji Sunglasses = new Emoji("1F60E", "SMILING FACE WITH SUNGLASSES", "\ud83d\ude0e", "sunglasses", new string[1] { "sunglasses" }, null, new string[1] { "8)" }, "Smileys & People", 30, 38, SkinVariationType.None, "smiling face with sunglasses", new string[10] { "bright", "cool", "eye", "eyewear", "face", "glasses", "smile", "smiling face with sunglasses", "sun", "sunglasses" });

	public static readonly Emoji HeartEyes = new Emoji("1F60D", "SMILING FACE WITH HEART-SHAPED EYES", "\ud83d\ude0d", "heart_eyes", new string[1] { "heart_eyes" }, null, null, "Smileys & People", 30, 37, SkinVariationType.None, "smiling face with heart-eyes", new string[5] { "eye", "face", "love", "smile", "smiling face with heart-eyes" });

	public static readonly Emoji KissingHeart = new Emoji("1F618", "FACE THROWING A KISS", "\ud83d\ude18", "kissing_heart", new string[1] { "kissing_heart" }, null, new string[2] { ":*", ":-*" }, "Smileys & People", 30, 48, SkinVariationType.None, "face blowing a kiss", new string[3] { "face", "face blowing a kiss", "kiss" });

	public static readonly Emoji Kissing = new Emoji("1F617", "KISSING FACE", "\ud83d\ude17", "kissing", new string[1] { "kissing" }, null, null, "Smileys & People", 30, 47, SkinVariationType.None, "kissing face", new string[3] { "face", "kiss", "kissing face" });

	public static readonly Emoji KissingSmilingEyes = new Emoji("1F619", "KISSING FACE WITH SMILING EYES", "\ud83d\ude19", "kissing_smiling_eyes", new string[1] { "kissing_smiling_eyes" }, null, null, "Smileys & People", 30, 49, SkinVariationType.None, "kissing face with smiling eyes", new string[5] { "eye", "face", "kiss", "kissing face with smiling eyes", "smile" });

	public static readonly Emoji KissingClosedEyes = new Emoji("1F61A", "KISSING FACE WITH CLOSED EYES", "\ud83d\ude1a", "kissing_closed_eyes", new string[1] { "kissing_closed_eyes" }, null, null, "Smileys & People", 30, 50, SkinVariationType.None, "kissing face with closed eyes", new string[5] { "closed", "eye", "face", "kiss", "kissing face with closed eyes" });

	public static readonly Emoji Relaxed = new Emoji("263A-FE0F", "WHITE SMILING FACE", "☺\ufe0f", "relaxed", new string[1] { "relaxed" }, null, null, "Smileys & People", 47, 41, SkinVariationType.None, null, null);

	public static readonly Emoji SlightlySmilingFace = new Emoji("1F642", "SLIGHTLY SMILING FACE", "\ud83d\ude42", "slightly_smiling_face", new string[1] { "slightly_smiling_face" }, null, new string[3] { ":)", "(:", ":-)" }, "Smileys & People", 31, 38, SkinVariationType.None, "slightly smiling face", new string[3] { "face", "slightly smiling face", "smile" });

	public static readonly Emoji HuggingFace = new Emoji("1F917", "HUGGING FACE", "\ud83e\udd17", "hugging_face", new string[1] { "hugging_face" }, null, null, "Smileys & People", 37, 31, SkinVariationType.None, "hugging face", new string[3] { "face", "hug", "hugging" });

	public static readonly Emoji StarStruck = new Emoji("1F929", "GRINNING FACE WITH STAR EYES", "\ud83e\udd29", "star-struck", new string[2] { "star-struck", "grinning_face_with_star_eyes" }, null, null, "Smileys & People", 38, 49, SkinVariationType.None, "star-struck", new string[5] { "eyes", "face", "grinning", "star", "star-struck" });

	public static readonly Emoji ThinkingFace = new Emoji("1F914", "THINKING FACE", "\ud83e\udd14", "thinking_face", new string[1] { "thinking_face" }, null, null, "Smileys & People", 37, 28, SkinVariationType.None, "thinking face", new string[2] { "face", "thinking" });

	public static readonly Emoji FaceWithRaisedEyebrow = new Emoji("1F928", "FACE WITH ONE EYEBROW RAISED", "\ud83e\udd28", "face_with_raised_eyebrow", new string[2] { "face_with_raised_eyebrow", "face_with_one_eyebrow_raised" }, null, null, "Smileys & People", 38, 48, SkinVariationType.None, "face with raised eyebrow", new string[3] { "distrust", "face with raised eyebrow", "skeptic" });

	public static readonly Emoji NeutralFace = new Emoji("1F610", "NEUTRAL FACE", "\ud83d\ude10", "neutral_face", new string[1] { "neutral_face" }, null, new string[2] { ":|", ":-|" }, "Smileys & People", 30, 40, SkinVariationType.None, "neutral face", new string[3] { "deadpan", "face", "neutral" });

	public static readonly Emoji Expressionless = new Emoji("1F611", "EXPRESSIONLESS FACE", "\ud83d\ude11", "expressionless", new string[1] { "expressionless" }, null, null, "Smileys & People", 30, 41, SkinVariationType.None, "expressionless face", new string[4] { "expressionless", "face", "inexpressive", "unexpressive" });

	public static readonly Emoji NoMouth = new Emoji("1F636", "FACE WITHOUT MOUTH", "\ud83d\ude36", "no_mouth", new string[1] { "no_mouth" }, null, null, "Smileys & People", 31, 26, SkinVariationType.None, "face without mouth", new string[5] { "face", "face without mouth", "mouth", "quiet", "silent" });

	public static readonly Emoji FaceWithRollingEyes = new Emoji("1F644", "FACE WITH ROLLING EYES", "\ud83d\ude44", "face_with_rolling_eyes", new string[1] { "face_with_rolling_eyes" }, null, null, "Smileys & People", 31, 40, SkinVariationType.None, "face with rolling eyes", new string[4] { "eyes", "face", "face with rolling eyes", "rolling" });

	public static readonly Emoji Smirk = new Emoji("1F60F", "SMIRKING FACE", "\ud83d\ude0f", "smirk", new string[1] { "smirk" }, null, null, "Smileys & People", 30, 39, SkinVariationType.None, "smirking face", new string[3] { "face", "smirk", "smirking face" });

	public static readonly Emoji Persevere = new Emoji("1F623", "PERSEVERING FACE", "\ud83d\ude23", "persevere", new string[1] { "persevere" }, null, null, "Smileys & People", 31, 7, SkinVariationType.None, "persevering face", new string[3] { "face", "persevere", "persevering face" });

	public static readonly Emoji DisappointedRelieved = new Emoji("1F625", "DISAPPOINTED BUT RELIEVED FACE", "\ud83d\ude25", "disappointed_relieved", new string[1] { "disappointed_relieved" }, null, null, "Smileys & People", 31, 9, SkinVariationType.None, "sad but relieved face", new string[5] { "disappointed", "face", "relieved", "sad but relieved face", "whew" });

	public static readonly Emoji OpenMouth = new Emoji("1F62E", "FACE WITH OPEN MOUTH", "\ud83d\ude2e", "open_mouth", new string[1] { "open_mouth" }, null, new string[4] { ":o", ":-o", ":O", ":-O" }, "Smileys & People", 31, 18, SkinVariationType.None, "face with open mouth", new string[5] { "face", "face with open mouth", "mouth", "open", "sympathy" });

	public static readonly Emoji ZipperMouthFace = new Emoji("1F910", "ZIPPER-MOUTH FACE", "\ud83e\udd10", "zipper_mouth_face", new string[1] { "zipper_mouth_face" }, null, null, "Smileys & People", 37, 24, SkinVariationType.None, "zipper-mouth face", new string[4] { "face", "mouth", "zipper", "zipper-mouth face" });

	public static readonly Emoji Hushed = new Emoji("1F62F", "HUSHED FACE", "\ud83d\ude2f", "hushed", new string[1] { "hushed" }, null, null, "Smileys & People", 31, 19, SkinVariationType.None, "hushed face", new string[4] { "face", "hushed", "stunned", "surprised" });

	public static readonly Emoji Sleepy = new Emoji("1F62A", "SLEEPY FACE", "\ud83d\ude2a", "sleepy", new string[1] { "sleepy" }, null, null, "Smileys & People", 31, 14, SkinVariationType.None, "sleepy face", new string[3] { "face", "sleep", "sleepy face" });

	public static readonly Emoji TiredFace = new Emoji("1F62B", "TIRED FACE", "\ud83d\ude2b", "tired_face", new string[1] { "tired_face" }, null, null, "Smileys & People", 31, 15, SkinVariationType.None, "tired face", new string[2] { "face", "tired" });

	public static readonly Emoji Sleeping = new Emoji("1F634", "SLEEPING FACE", "\ud83d\ude34", "sleeping", new string[1] { "sleeping" }, null, null, "Smileys & People", 31, 24, SkinVariationType.None, "sleeping face", new string[4] { "face", "sleep", "sleeping face", "zzz" });

	public static readonly Emoji Relieved = new Emoji("1F60C", "RELIEVED FACE", "\ud83d\ude0c", "relieved", new string[1] { "relieved" }, null, null, "Smileys & People", 30, 36, SkinVariationType.None, "relieved face", new string[2] { "face", "relieved" });

	public static readonly Emoji StuckOutTongue = new Emoji("1F61B", "FACE WITH STUCK-OUT TONGUE", "\ud83d\ude1b", "stuck_out_tongue", new string[1] { "stuck_out_tongue" }, ":p", new string[6] { ":p", ":-p", ":P", ":-P", ":b", ":-b" }, "Smileys & People", 30, 51, SkinVariationType.None, "face with tongue", new string[3] { "face", "face with tongue", "tongue" });

	public static readonly Emoji StuckOutTongueWinkingEye = new Emoji("1F61C", "FACE WITH STUCK-OUT TONGUE AND WINKING EYE", "\ud83d\ude1c", "stuck_out_tongue_winking_eye", new string[1] { "stuck_out_tongue_winking_eye" }, ";p", new string[6] { ";p", ";-p", ";b", ";-b", ";P", ";-P" }, "Smileys & People", 31, 0, SkinVariationType.None, "winking face with tongue", new string[6] { "eye", "face", "joke", "tongue", "wink", "winking face with tongue" });

	public static readonly Emoji StuckOutTongueClosedEyes = new Emoji("1F61D", "FACE WITH STUCK-OUT TONGUE AND TIGHTLY-CLOSED EYES", "\ud83d\ude1d", "stuck_out_tongue_closed_eyes", new string[1] { "stuck_out_tongue_closed_eyes" }, null, null, "Smileys & People", 31, 1, SkinVariationType.None, "squinting face with tongue", new string[6] { "eye", "face", "horrible", "squinting face with tongue", "taste", "tongue" });

	public static readonly Emoji DroolingFace = new Emoji("1F924", "DROOLING FACE", "\ud83e\udd24", "drooling_face", new string[1] { "drooling_face" }, null, null, "Smileys & People", 38, 27, SkinVariationType.None, "drooling face", new string[2] { "drooling", "face" });

	public static readonly Emoji Unamused = new Emoji("1F612", "UNAMUSED FACE", "\ud83d\ude12", "unamused", new string[1] { "unamused" }, ":(", null, "Smileys & People", 30, 42, SkinVariationType.None, "unamused face", new string[3] { "face", "unamused", "unhappy" });

	public static readonly Emoji Sweat = new Emoji("1F613", "FACE WITH COLD SWEAT", "\ud83d\ude13", "sweat", new string[1] { "sweat" }, null, null, "Smileys & People", 30, 43, SkinVariationType.None, "downcast face with sweat", new string[4] { "cold", "downcast face with sweat", "face", "sweat" });

	public static readonly Emoji Pensive = new Emoji("1F614", "PENSIVE FACE", "\ud83d\ude14", "pensive", new string[1] { "pensive" }, null, null, "Smileys & People", 30, 44, SkinVariationType.None, "pensive face", new string[3] { "dejected", "face", "pensive" });

	public static readonly Emoji Confused = new Emoji("1F615", "CONFUSED FACE", "\ud83d\ude15", "confused", new string[1] { "confused" }, null, new string[4] { ":\\", ":-\\", ":/", ":-/" }, "Smileys & People", 30, 45, SkinVariationType.None, "confused face", new string[2] { "confused", "face" });

	public static readonly Emoji UpsideDownFace = new Emoji("1F643", "UPSIDE-DOWN FACE", "\ud83d\ude43", "upside_down_face", new string[1] { "upside_down_face" }, null, null, "Smileys & People", 31, 39, SkinVariationType.None, "upside-down face", new string[2] { "face", "upside-down" });

	public static readonly Emoji MoneyMouthFace = new Emoji("1F911", "MONEY-MOUTH FACE", "\ud83e\udd11", "money_mouth_face", new string[1] { "money_mouth_face" }, null, null, "Smileys & People", 37, 25, SkinVariationType.None, "money-mouth face", new string[4] { "face", "money", "money-mouth face", "mouth" });

	public static readonly Emoji Astonished = new Emoji("1F632", "ASTONISHED FACE", "\ud83d\ude32", "astonished", new string[1] { "astonished" }, null, null, "Smileys & People", 31, 22, SkinVariationType.None, "astonished face", new string[4] { "astonished", "face", "shocked", "totally" });

	public static readonly Emoji WhiteFrowningFace = new Emoji("2639-FE0F", null, "☹\ufe0f", "white_frowning_face", new string[1] { "white_frowning_face" }, null, null, "Smileys & People", 47, 40, SkinVariationType.None, null, null);

	public static readonly Emoji SlightlyFrowningFace = new Emoji("1F641", "SLIGHTLY FROWNING FACE", "\ud83d\ude41", "slightly_frowning_face", new string[1] { "slightly_frowning_face" }, null, null, "Smileys & People", 31, 37, SkinVariationType.None, "slightly frowning face", new string[3] { "face", "frown", "slightly frowning face" });

	public static readonly Emoji Confounded = new Emoji("1F616", "CONFOUNDED FACE", "\ud83d\ude16", "confounded", new string[1] { "confounded" }, null, null, "Smileys & People", 30, 46, SkinVariationType.None, "confounded face", new string[2] { "confounded", "face" });

	public static readonly Emoji Disappointed = new Emoji("1F61E", "DISAPPOINTED FACE", "\ud83d\ude1e", "disappointed", new string[1] { "disappointed" }, ":(", new string[3] { "):", ":(", ":-(" }, "Smileys & People", 31, 2, SkinVariationType.None, "disappointed face", new string[2] { "disappointed", "face" });

	public static readonly Emoji Worried = new Emoji("1F61F", "WORRIED FACE", "\ud83d\ude1f", "worried", new string[1] { "worried" }, null, null, "Smileys & People", 31, 3, SkinVariationType.None, "worried face", new string[2] { "face", "worried" });

	public static readonly Emoji Triumph = new Emoji("1F624", "FACE WITH LOOK OF TRIUMPH", "\ud83d\ude24", "triumph", new string[1] { "triumph" }, null, null, "Smileys & People", 31, 8, SkinVariationType.None, "face with steam from nose", new string[4] { "face", "face with steam from nose", "triumph", "won" });

	public static readonly Emoji Cry = new Emoji("1F622", "CRYING FACE", "\ud83d\ude22", "cry", new string[1] { "cry" }, ":'(", new string[1] { ":'(" }, "Smileys & People", 31, 6, SkinVariationType.None, "crying face", new string[5] { "cry", "crying face", "face", "sad", "tear" });

	public static readonly Emoji Sob = new Emoji("1F62D", "LOUDLY CRYING FACE", "\ud83d\ude2d", "sob", new string[1] { "sob" }, ":'(", null, "Smileys & People", 31, 17, SkinVariationType.None, "loudly crying face", new string[6] { "cry", "face", "loudly crying face", "sad", "sob", "tear" });

	public static readonly Emoji Frowning = new Emoji("1F626", "FROWNING FACE WITH OPEN MOUTH", "\ud83d\ude26", "frowning", new string[1] { "frowning" }, null, null, "Smileys & People", 31, 10, SkinVariationType.None, "frowning face with open mouth", new string[5] { "face", "frown", "frowning face with open mouth", "mouth", "open" });

	public static readonly Emoji Anguished = new Emoji("1F627", "ANGUISHED FACE", "\ud83d\ude27", "anguished", new string[1] { "anguished" }, null, new string[1] { "D:" }, "Smileys & People", 31, 11, SkinVariationType.None, "anguished face", new string[2] { "anguished", "face" });

	public static readonly Emoji Fearful = new Emoji("1F628", "FEARFUL FACE", "\ud83d\ude28", "fearful", new string[1] { "fearful" }, null, null, "Smileys & People", 31, 12, SkinVariationType.None, "fearful face", new string[4] { "face", "fear", "fearful", "scared" });

	public static readonly Emoji Weary = new Emoji("1F629", "WEARY FACE", "\ud83d\ude29", "weary", new string[1] { "weary" }, null, null, "Smileys & People", 31, 13, SkinVariationType.None, "weary face", new string[3] { "face", "tired", "weary" });

	public static readonly Emoji ExplodingHead = new Emoji("1F92F", "SHOCKED FACE WITH EXPLODING HEAD", "\ud83e\udd2f", "exploding_head", new string[2] { "exploding_head", "shocked_face_with_exploding_head" }, null, null, "Smileys & People", 39, 3, SkinVariationType.None, "exploding head", new string[2] { "exploding head", "shocked" });

	public static readonly Emoji Grimacing = new Emoji("1F62C", "GRIMACING FACE", "\ud83d\ude2c", "grimacing", new string[1] { "grimacing" }, null, null, "Smileys & People", 31, 16, SkinVariationType.None, "grimacing face", new string[3] { "face", "grimace", "grimacing face" });

	public static readonly Emoji ColdSweat = new Emoji("1F630", "FACE WITH OPEN MOUTH AND COLD SWEAT", "\ud83d\ude30", "cold_sweat", new string[1] { "cold_sweat" }, null, null, "Smileys & People", 31, 20, SkinVariationType.None, "anxious face with sweat", new string[8] { "anxious face with sweat", "blue", "cold", "face", "mouth", "open", "rushed", "sweat" });

	public static readonly Emoji Scream = new Emoji("1F631", "FACE SCREAMING IN FEAR", "\ud83d\ude31", "scream", new string[1] { "scream" }, null, null, "Smileys & People", 31, 21, SkinVariationType.None, "face screaming in fear", new string[7] { "face", "face screaming in fear", "fear", "fearful", "munch", "scared", "scream" });

	public static readonly Emoji Flushed = new Emoji("1F633", "FLUSHED FACE", "\ud83d\ude33", "flushed", new string[1] { "flushed" }, null, null, "Smileys & People", 31, 23, SkinVariationType.None, "flushed face", new string[3] { "dazed", "face", "flushed" });

	public static readonly Emoji ZanyFace = new Emoji("1F92A", "GRINNING FACE WITH ONE LARGE AND ONE SMALL EYE", "\ud83e\udd2a", "zany_face", new string[2] { "zany_face", "grinning_face_with_one_large_and_one_small_eye" }, null, null, "Smileys & People", 38, 50, SkinVariationType.None, "zany face", new string[5] { "eye", "goofy", "large", "small", "zany face" });

	public static readonly Emoji DizzyFace = new Emoji("1F635", "DIZZY FACE", "\ud83d\ude35", "dizzy_face", new string[1] { "dizzy_face" }, null, null, "Smileys & People", 31, 25, SkinVariationType.None, "dizzy face", new string[2] { "dizzy", "face" });

	public static readonly Emoji Rage = new Emoji("1F621", "POUTING FACE", "\ud83d\ude21", "rage", new string[1] { "rage" }, null, null, "Smileys & People", 31, 5, SkinVariationType.None, "pouting face", new string[6] { "angry", "face", "mad", "pouting", "rage", "red" });

	public static readonly Emoji Angry = new Emoji("1F620", "ANGRY FACE", "\ud83d\ude20", "angry", new string[1] { "angry" }, null, new string[2] { ">:(", ">:-(" }, "Smileys & People", 31, 4, SkinVariationType.None, "angry face", new string[3] { "angry", "face", "mad" });

	public static readonly Emoji FaceWithSymbolsOnMouth = new Emoji("1F92C", "SERIOUS FACE WITH SYMBOLS COVERING MOUTH", "\ud83e\udd2c", "face_with_symbols_on_mouth", new string[2] { "face_with_symbols_on_mouth", "serious_face_with_symbols_covering_mouth" }, null, null, "Smileys & People", 39, 0, SkinVariationType.None, "face with symbols on mouth", new string[2] { "face with symbols on mouth", "swearing" });

	public static readonly Emoji Mask = new Emoji("1F637", "FACE WITH MEDICAL MASK", "\ud83d\ude37", "mask", new string[1] { "mask" }, null, null, "Smileys & People", 31, 27, SkinVariationType.None, "face with medical mask", new string[7] { "cold", "doctor", "face", "face with medical mask", "mask", "medicine", "sick" });

	public static readonly Emoji FaceWithThermometer = new Emoji("1F912", "FACE WITH THERMOMETER", "\ud83e\udd12", "face_with_thermometer", new string[1] { "face_with_thermometer" }, null, null, "Smileys & People", 37, 26, SkinVariationType.None, "face with thermometer", new string[5] { "face", "face with thermometer", "ill", "sick", "thermometer" });

	public static readonly Emoji FaceWithHeadBandage = new Emoji("1F915", "FACE WITH HEAD-BANDAGE", "\ud83e\udd15", "face_with_head_bandage", new string[1] { "face_with_head_bandage" }, null, null, "Smileys & People", 37, 29, SkinVariationType.None, "face with head-bandage", new string[5] { "bandage", "face", "face with head-bandage", "hurt", "injury" });

	public static readonly Emoji NauseatedFace = new Emoji("1F922", "NAUSEATED FACE", "\ud83e\udd22", "nauseated_face", new string[1] { "nauseated_face" }, null, null, "Smileys & People", 38, 25, SkinVariationType.None, "nauseated face", new string[3] { "face", "nauseated", "vomit" });

	public static readonly Emoji FaceVomiting = new Emoji("1F92E", "FACE WITH OPEN MOUTH VOMITING", "\ud83e\udd2e", "face_vomiting", new string[2] { "face_vomiting", "face_with_open_mouth_vomiting" }, null, null, "Smileys & People", 39, 2, SkinVariationType.None, "face vomiting", new string[3] { "face vomiting", "sick", "vomit" });

	public static readonly Emoji SneezingFace = new Emoji("1F927", "SNEEZING FACE", "\ud83e\udd27", "sneezing_face", new string[1] { "sneezing_face" }, null, null, "Smileys & People", 38, 47, SkinVariationType.None, "sneezing face", new string[4] { "face", "gesundheit", "sneeze", "sneezing face" });

	public static readonly Emoji Innocent = new Emoji("1F607", "SMILING FACE WITH HALO", "\ud83d\ude07", "innocent", new string[1] { "innocent" }, null, null, "Smileys & People", 30, 31, SkinVariationType.None, "smiling face with halo", new string[8] { "angel", "face", "fairy tale", "fantasy", "halo", "innocent", "smile", "smiling face with halo" });

	public static readonly Emoji FaceWithCowboyHat = new Emoji("1F920", "FACE WITH COWBOY HAT", "\ud83e\udd20", "face_with_cowboy_hat", new string[1] { "face_with_cowboy_hat" }, null, null, "Smileys & People", 38, 23, SkinVariationType.None, "cowboy hat face", new string[4] { "cowboy", "cowgirl", "face", "hat" });

	public static readonly Emoji LyingFace = new Emoji("1F925", "LYING FACE", "\ud83e\udd25", "lying_face", new string[1] { "lying_face" }, null, null, "Smileys & People", 38, 28, SkinVariationType.None, "lying face", new string[4] { "face", "lie", "lying face", "pinocchio" });

	public static readonly Emoji ShushingFace = new Emoji("1F92B", "FACE WITH FINGER COVERING CLOSED LIPS", "\ud83e\udd2b", "shushing_face", new string[2] { "shushing_face", "face_with_finger_covering_closed_lips" }, null, null, "Smileys & People", 38, 51, SkinVariationType.None, "shushing face", new string[3] { "quiet", "shush", "shushing face" });

	public static readonly Emoji FaceWithHandOverMouth = new Emoji("1F92D", "SMILING FACE WITH SMILING EYES AND HAND COVERING MOUTH", "\ud83e\udd2d", "face_with_hand_over_mouth", new string[2] { "face_with_hand_over_mouth", "smiling_face_with_smiling_eyes_and_hand_covering_mouth" }, null, null, "Smileys & People", 39, 1, SkinVariationType.None, "face with hand over mouth", new string[2] { "face with hand over mouth", "whoops" });

	public static readonly Emoji FaceWithMonocle = new Emoji("1F9D0", "FACE WITH MONOCLE", "\ud83e\uddd0", "face_with_monocle", new string[1] { "face_with_monocle" }, null, null, "Smileys & People", 42, 49, SkinVariationType.None, "face with monocle", new string[2] { "face with monocle", "stuffy" });

	public static readonly Emoji NerdFace = new Emoji("1F913", "NERD FACE", "\ud83e\udd13", "nerd_face", new string[1] { "nerd_face" }, null, null, "Smileys & People", 37, 27, SkinVariationType.None, "nerd face", new string[3] { "face", "geek", "nerd" });

	public static readonly Emoji SmilingImp = new Emoji("1F608", "SMILING FACE WITH HORNS", "\ud83d\ude08", "smiling_imp", new string[1] { "smiling_imp" }, null, null, "Smileys & People", 30, 32, SkinVariationType.None, "smiling face with horns", new string[6] { "face", "fairy tale", "fantasy", "horns", "smile", "smiling face with horns" });

	public static readonly Emoji Imp = new Emoji("1F47F", "IMP", "\ud83d\udc7f", "imp", new string[1] { "imp" }, null, null, "Smileys & People", 22, 51, SkinVariationType.None, "angry face with horns", new string[7] { "angry face with horns", "demon", "devil", "face", "fairy tale", "fantasy", "imp" });

	public static readonly Emoji ClownFace = new Emoji("1F921", "CLOWN FACE", "\ud83e\udd21", "clown_face", new string[1] { "clown_face" }, null, null, "Smileys & People", 38, 24, SkinVariationType.None, "clown face", new string[2] { "clown", "face" });

	public static readonly Emoji JapaneseOgre = new Emoji("1F479", "JAPANESE OGRE", "\ud83d\udc79", "japanese_ogre", new string[1] { "japanese_ogre" }, null, null, "Smileys & People", 22, 40, SkinVariationType.None, "ogre", new string[6] { "creature", "face", "fairy tale", "fantasy", "monster", "ogre" });

	public static readonly Emoji JapaneseGoblin = new Emoji("1F47A", "JAPANESE GOBLIN", "\ud83d\udc7a", "japanese_goblin", new string[1] { "japanese_goblin" }, null, null, "Smileys & People", 22, 41, SkinVariationType.None, "goblin", new string[6] { "creature", "face", "fairy tale", "fantasy", "goblin", "monster" });

	public static readonly Emoji Skull = new Emoji("1F480", "SKULL", "\ud83d\udc80", "skull", new string[1] { "skull" }, null, null, "Smileys & People", 23, 0, SkinVariationType.None, "skull", new string[5] { "death", "face", "fairy tale", "monster", "skull" });

	public static readonly Emoji SkullAndCrossbones = new Emoji("2620-FE0F", null, "☠\ufe0f", "skull_and_crossbones", new string[1] { "skull_and_crossbones" }, null, null, "Smileys & People", 47, 32, SkinVariationType.None, null, null);

	public static readonly Emoji Ghost = new Emoji("1F47B", "GHOST", "\ud83d\udc7b", "ghost", new string[1] { "ghost" }, null, null, "Smileys & People", 22, 42, SkinVariationType.None, "ghost", new string[6] { "creature", "face", "fairy tale", "fantasy", "ghost", "monster" });

	public static readonly Emoji Alien = new Emoji("1F47D", "EXTRATERRESTRIAL ALIEN", "\ud83d\udc7d", "alien", new string[1] { "alien" }, null, null, "Smileys & People", 22, 49, SkinVariationType.None, "alien", new string[8] { "alien", "creature", "extraterrestrial", "face", "fairy tale", "fantasy", "monster", "ufo" });

	public static readonly Emoji SpaceInvader = new Emoji("1F47E", "ALIEN MONSTER", "\ud83d\udc7e", "space_invader", new string[1] { "space_invader" }, null, null, "Smileys & People", 22, 50, SkinVariationType.None, "alien monster", new string[8] { "alien", "creature", "extraterrestrial", "face", "fairy tale", "fantasy", "monster", "ufo" });

	public static readonly Emoji RobotFace = new Emoji("1F916", "ROBOT FACE", "\ud83e\udd16", "robot_face", new string[1] { "robot_face" }, null, null, "Smileys & People", 37, 30, SkinVariationType.None, "robot face", new string[3] { "face", "monster", "robot" });

	public static readonly Emoji Hankey = new Emoji("1F4A9", "PILE OF POO", "\ud83d\udca9", "hankey", new string[3] { "hankey", "poop", "shit" }, null, null, "Smileys & People", 25, 15, SkinVariationType.None, "pile of poo", new string[7] { "comic", "dung", "face", "monster", "pile of poo", "poo", "poop" });

	public static readonly Emoji SmileyCat = new Emoji("1F63A", "SMILING CAT FACE WITH OPEN MOUTH", "\ud83d\ude3a", "smiley_cat", new string[1] { "smiley_cat" }, null, null, "Smileys & People", 31, 30, SkinVariationType.None, "grinning cat face", new string[6] { "cat", "face", "grinning cat face", "mouth", "open", "smile" });

	public static readonly Emoji SmileCat = new Emoji("1F638", "GRINNING CAT FACE WITH SMILING EYES", "\ud83d\ude38", "smile_cat", new string[1] { "smile_cat" }, null, null, "Smileys & People", 31, 28, SkinVariationType.None, "grinning cat face with smiling eyes", new string[6] { "cat", "eye", "face", "grin", "grinning cat face with smiling eyes", "smile" });

	public static readonly Emoji JoyCat = new Emoji("1F639", "CAT FACE WITH TEARS OF JOY", "\ud83d\ude39", "joy_cat", new string[1] { "joy_cat" }, null, null, "Smileys & People", 31, 29, SkinVariationType.None, "cat face with tears of joy", new string[5] { "cat", "cat face with tears of joy", "face", "joy", "tear" });

	public static readonly Emoji HeartEyesCat = new Emoji("1F63B", "SMILING CAT FACE WITH HEART-SHAPED EYES", "\ud83d\ude3b", "heart_eyes_cat", new string[1] { "heart_eyes_cat" }, null, null, "Smileys & People", 31, 31, SkinVariationType.None, "smiling cat face with heart-eyes", new string[6] { "cat", "eye", "face", "love", "smile", "smiling cat face with heart-eyes" });

	public static readonly Emoji SmirkCat = new Emoji("1F63C", "CAT FACE WITH WRY SMILE", "\ud83d\ude3c", "smirk_cat", new string[1] { "smirk_cat" }, null, null, "Smileys & People", 31, 32, SkinVariationType.None, "cat face with wry smile", new string[6] { "cat", "cat face with wry smile", "face", "ironic", "smile", "wry" });

	public static readonly Emoji KissingCat = new Emoji("1F63D", "KISSING CAT FACE WITH CLOSED EYES", "\ud83d\ude3d", "kissing_cat", new string[1] { "kissing_cat" }, null, null, "Smileys & People", 31, 33, SkinVariationType.None, "kissing cat face", new string[5] { "cat", "eye", "face", "kiss", "kissing cat face" });

	public static readonly Emoji ScreamCat = new Emoji("1F640", "WEARY CAT FACE", "\ud83d\ude40", "scream_cat", new string[1] { "scream_cat" }, null, null, "Smileys & People", 31, 36, SkinVariationType.None, "weary cat face", new string[5] { "cat", "face", "oh", "surprised", "weary" });

	public static readonly Emoji CryingCatFace = new Emoji("1F63F", "CRYING CAT FACE", "\ud83d\ude3f", "crying_cat_face", new string[1] { "crying_cat_face" }, null, null, "Smileys & People", 31, 35, SkinVariationType.None, "crying cat face", new string[6] { "cat", "cry", "crying cat face", "face", "sad", "tear" });

	public static readonly Emoji PoutingCat = new Emoji("1F63E", "POUTING CAT FACE", "\ud83d\ude3e", "pouting_cat", new string[1] { "pouting_cat" }, null, null, "Smileys & People", 31, 34, SkinVariationType.None, "pouting cat face", new string[3] { "cat", "face", "pouting" });

	public static readonly Emoji SeeNoEvil = new Emoji("1F648", "SEE-NO-EVIL MONKEY", "\ud83d\ude48", "see_no_evil", new string[1] { "see_no_evil" }, null, null, "Smileys & People", 32, 43, SkinVariationType.None, "see-no-evil monkey", new string[10] { "evil", "face", "forbidden", "gesture", "monkey", "no", "not", "prohibited", "see", "see-no-evil monkey" });

	public static readonly Emoji HearNoEvil = new Emoji("1F649", "HEAR-NO-EVIL MONKEY", "\ud83d\ude49", "hear_no_evil", new string[1] { "hear_no_evil" }, null, null, "Smileys & People", 32, 44, SkinVariationType.None, "hear-no-evil monkey", new string[10] { "evil", "face", "forbidden", "gesture", "hear", "hear-no-evil monkey", "monkey", "no", "not", "prohibited" });

	public static readonly Emoji SpeakNoEvil = new Emoji("1F64A", "SPEAK-NO-EVIL MONKEY", "\ud83d\ude4a", "speak_no_evil", new string[1] { "speak_no_evil" }, null, null, "Smileys & People", 32, 45, SkinVariationType.None, "speak-no-evil monkey", new string[10] { "evil", "face", "forbidden", "gesture", "monkey", "no", "not", "prohibited", "speak", "speak-no-evil monkey" });

	public static readonly Emoji Baby = new Emoji("1F476", "BABY", "\ud83d\udc76", "baby", new string[1] { "baby" }, null, null, "Smileys & People", 22, 10, SkinVariationType.None, "baby", new string[2] { "baby", "young" });

	public static readonly Emoji Baby_Light = new Emoji("1F476-1F3FB", "BABY", "\ud83d\udc76\ud83c\udffb", "baby", new string[1] { "baby" }, null, null, "Smileys & People", 22, 11, SkinVariationType.Light, "baby", new string[2] { "baby", "young" });

	public static readonly Emoji Baby_MediumLight = new Emoji("1F476-1F3FC", "BABY", "\ud83d\udc76\ud83c\udffc", "baby", new string[1] { "baby" }, null, null, "Smileys & People", 22, 12, SkinVariationType.MediumLight, "baby", new string[2] { "baby", "young" });

	public static readonly Emoji Baby_Medium = new Emoji("1F476-1F3FD", "BABY", "\ud83d\udc76\ud83c\udffd", "baby", new string[1] { "baby" }, null, null, "Smileys & People", 22, 13, SkinVariationType.Medium, "baby", new string[2] { "baby", "young" });

	public static readonly Emoji Baby_MediumDark = new Emoji("1F476-1F3FE", "BABY", "\ud83d\udc76\ud83c\udffe", "baby", new string[1] { "baby" }, null, null, "Smileys & People", 22, 14, SkinVariationType.MediumDark, "baby", new string[2] { "baby", "young" });

	public static readonly Emoji Baby_Dark = new Emoji("1F476-1F3FF", "BABY", "\ud83d\udc76\ud83c\udfff", "baby", new string[1] { "baby" }, null, null, "Smileys & People", 22, 15, SkinVariationType.Dark, "baby", new string[2] { "baby", "young" });

	public static readonly Emoji Child = new Emoji("1F9D2", "CHILD", "\ud83e\uddd2", "child", new string[1] { "child" }, null, null, "Smileys & People", 43, 4, SkinVariationType.None, "child", new string[3] { "child", "gender-neutral", "young" });

	public static readonly Emoji Child_Light = new Emoji("1F9D2-1F3FB", "CHILD", "\ud83e\uddd2\ud83c\udffb", "child", new string[1] { "child" }, null, null, "Smileys & People", 43, 5, SkinVariationType.Light, "child", new string[3] { "child", "gender-neutral", "young" });

	public static readonly Emoji Child_MediumLight = new Emoji("1F9D2-1F3FC", "CHILD", "\ud83e\uddd2\ud83c\udffc", "child", new string[1] { "child" }, null, null, "Smileys & People", 43, 6, SkinVariationType.MediumLight, "child", new string[3] { "child", "gender-neutral", "young" });

	public static readonly Emoji Child_Medium = new Emoji("1F9D2-1F3FD", "CHILD", "\ud83e\uddd2\ud83c\udffd", "child", new string[1] { "child" }, null, null, "Smileys & People", 43, 7, SkinVariationType.Medium, "child", new string[3] { "child", "gender-neutral", "young" });

	public static readonly Emoji Child_MediumDark = new Emoji("1F9D2-1F3FE", "CHILD", "\ud83e\uddd2\ud83c\udffe", "child", new string[1] { "child" }, null, null, "Smileys & People", 43, 8, SkinVariationType.MediumDark, "child", new string[3] { "child", "gender-neutral", "young" });

	public static readonly Emoji Child_Dark = new Emoji("1F9D2-1F3FF", "CHILD", "\ud83e\uddd2\ud83c\udfff", "child", new string[1] { "child" }, null, null, "Smileys & People", 43, 9, SkinVariationType.Dark, "child", new string[3] { "child", "gender-neutral", "young" });

	public static readonly Emoji Boy = new Emoji("1F466", "BOY", "\ud83d\udc66", "boy", new string[1] { "boy" }, null, null, "Smileys & People", 15, 42, SkinVariationType.None, "boy", new string[2] { "boy", "young" });

	public static readonly Emoji Boy_Light = new Emoji("1F466-1F3FB", "BOY", "\ud83d\udc66\ud83c\udffb", "boy", new string[1] { "boy" }, null, null, "Smileys & People", 15, 43, SkinVariationType.Light, "boy", new string[2] { "boy", "young" });

	public static readonly Emoji Boy_MediumLight = new Emoji("1F466-1F3FC", "BOY", "\ud83d\udc66\ud83c\udffc", "boy", new string[1] { "boy" }, null, null, "Smileys & People", 15, 44, SkinVariationType.MediumLight, "boy", new string[2] { "boy", "young" });

	public static readonly Emoji Boy_Medium = new Emoji("1F466-1F3FD", "BOY", "\ud83d\udc66\ud83c\udffd", "boy", new string[1] { "boy" }, null, null, "Smileys & People", 15, 45, SkinVariationType.Medium, "boy", new string[2] { "boy", "young" });

	public static readonly Emoji Boy_MediumDark = new Emoji("1F466-1F3FE", "BOY", "\ud83d\udc66\ud83c\udffe", "boy", new string[1] { "boy" }, null, null, "Smileys & People", 15, 46, SkinVariationType.MediumDark, "boy", new string[2] { "boy", "young" });

	public static readonly Emoji Boy_Dark = new Emoji("1F466-1F3FF", "BOY", "\ud83d\udc66\ud83c\udfff", "boy", new string[1] { "boy" }, null, null, "Smileys & People", 15, 47, SkinVariationType.Dark, "boy", new string[2] { "boy", "young" });

	public static readonly Emoji Girl = new Emoji("1F467", "GIRL", "\ud83d\udc67", "girl", new string[1] { "girl" }, null, null, "Smileys & People", 15, 48, SkinVariationType.None, "girl", new string[4] { "girl", "Virgo", "young", "zodiac" });

	public static readonly Emoji Girl_Light = new Emoji("1F467-1F3FB", "GIRL", "\ud83d\udc67\ud83c\udffb", "girl", new string[1] { "girl" }, null, null, "Smileys & People", 15, 49, SkinVariationType.Light, "girl", new string[4] { "girl", "Virgo", "young", "zodiac" });

	public static readonly Emoji Girl_MediumLight = new Emoji("1F467-1F3FC", "GIRL", "\ud83d\udc67\ud83c\udffc", "girl", new string[1] { "girl" }, null, null, "Smileys & People", 15, 50, SkinVariationType.MediumLight, "girl", new string[4] { "girl", "Virgo", "young", "zodiac" });

	public static readonly Emoji Girl_Medium = new Emoji("1F467-1F3FD", "GIRL", "\ud83d\udc67\ud83c\udffd", "girl", new string[1] { "girl" }, null, null, "Smileys & People", 15, 51, SkinVariationType.Medium, "girl", new string[4] { "girl", "Virgo", "young", "zodiac" });

	public static readonly Emoji Girl_MediumDark = new Emoji("1F467-1F3FE", "GIRL", "\ud83d\udc67\ud83c\udffe", "girl", new string[1] { "girl" }, null, null, "Smileys & People", 16, 0, SkinVariationType.MediumDark, "girl", new string[4] { "girl", "Virgo", "young", "zodiac" });

	public static readonly Emoji Girl_Dark = new Emoji("1F467-1F3FF", "GIRL", "\ud83d\udc67\ud83c\udfff", "girl", new string[1] { "girl" }, null, null, "Smileys & People", 16, 1, SkinVariationType.Dark, "girl", new string[4] { "girl", "Virgo", "young", "zodiac" });

	public static readonly Emoji Adult = new Emoji("1F9D1", "ADULT", "\ud83e\uddd1", "adult", new string[1] { "adult" }, null, null, "Smileys & People", 42, 50, SkinVariationType.None, "adult", new string[2] { "adult", "gender-neutral" });

	public static readonly Emoji Adult_Light = new Emoji("1F9D1-1F3FB", "ADULT", "\ud83e\uddd1\ud83c\udffb", "adult", new string[1] { "adult" }, null, null, "Smileys & People", 42, 51, SkinVariationType.Light, "adult", new string[2] { "adult", "gender-neutral" });

	public static readonly Emoji Adult_MediumLight = new Emoji("1F9D1-1F3FC", "ADULT", "\ud83e\uddd1\ud83c\udffc", "adult", new string[1] { "adult" }, null, null, "Smileys & People", 43, 0, SkinVariationType.MediumLight, "adult", new string[2] { "adult", "gender-neutral" });

	public static readonly Emoji Adult_Medium = new Emoji("1F9D1-1F3FD", "ADULT", "\ud83e\uddd1\ud83c\udffd", "adult", new string[1] { "adult" }, null, null, "Smileys & People", 43, 1, SkinVariationType.Medium, "adult", new string[2] { "adult", "gender-neutral" });

	public static readonly Emoji Adult_MediumDark = new Emoji("1F9D1-1F3FE", "ADULT", "\ud83e\uddd1\ud83c\udffe", "adult", new string[1] { "adult" }, null, null, "Smileys & People", 43, 2, SkinVariationType.MediumDark, "adult", new string[2] { "adult", "gender-neutral" });

	public static readonly Emoji Adult_Dark = new Emoji("1F9D1-1F3FF", "ADULT", "\ud83e\uddd1\ud83c\udfff", "adult", new string[1] { "adult" }, null, null, "Smileys & People", 43, 3, SkinVariationType.Dark, "adult", new string[2] { "adult", "gender-neutral" });

	public static readonly Emoji Man = new Emoji("1F468", "MAN", "\ud83d\udc68", "man", new string[1] { "man" }, null, null, "Smileys & People", 18, 11, SkinVariationType.None, "man", new string[1] { "man" });

	public static readonly Emoji Man_Light = new Emoji("1F468-1F3FB", "MAN", "\ud83d\udc68\ud83c\udffb", "man", new string[1] { "man" }, null, null, "Smileys & People", 18, 12, SkinVariationType.Light, "man", new string[1] { "man" });

	public static readonly Emoji Man_MediumLight = new Emoji("1F468-1F3FC", "MAN", "\ud83d\udc68\ud83c\udffc", "man", new string[1] { "man" }, null, null, "Smileys & People", 18, 13, SkinVariationType.MediumLight, "man", new string[1] { "man" });

	public static readonly Emoji Man_Medium = new Emoji("1F468-1F3FD", "MAN", "\ud83d\udc68\ud83c\udffd", "man", new string[1] { "man" }, null, null, "Smileys & People", 18, 14, SkinVariationType.Medium, "man", new string[1] { "man" });

	public static readonly Emoji Man_MediumDark = new Emoji("1F468-1F3FE", "MAN", "\ud83d\udc68\ud83c\udffe", "man", new string[1] { "man" }, null, null, "Smileys & People", 18, 15, SkinVariationType.MediumDark, "man", new string[1] { "man" });

	public static readonly Emoji Man_Dark = new Emoji("1F468-1F3FF", "MAN", "\ud83d\udc68\ud83c\udfff", "man", new string[1] { "man" }, null, null, "Smileys & People", 18, 16, SkinVariationType.Dark, "man", new string[1] { "man" });

	public static readonly Emoji BlondHairedMan = new Emoji("1F471-200D-2642-FE0F", null, "\ud83d\udc71\u200d♂\ufe0f", "blond-haired-man", new string[1] { "blond-haired-man" }, null, null, "Smileys & People", 21, 14, SkinVariationType.None, "blond-haired man", new string[4] { "blond", "blond-haired person", "blond-haired man", "man" });

	public static readonly Emoji BlondHairedMan_Light = new Emoji("1F471-1F3FB-200D-2642-FE0F", null, "\ud83d\udc71\ud83c\udffb\u200d♂\ufe0f", "blond-haired-man", new string[1] { "blond-haired-man" }, null, null, "Smileys & People", 21, 15, SkinVariationType.Light, "blond-haired man", new string[4] { "blond", "blond-haired person", "blond-haired man", "man" });

	public static readonly Emoji BlondHairedMan_MediumLight = new Emoji("1F471-1F3FC-200D-2642-FE0F", null, "\ud83d\udc71\ud83c\udffc\u200d♂\ufe0f", "blond-haired-man", new string[1] { "blond-haired-man" }, null, null, "Smileys & People", 21, 16, SkinVariationType.MediumLight, "blond-haired man", new string[4] { "blond", "blond-haired person", "blond-haired man", "man" });

	public static readonly Emoji BlondHairedMan_Medium = new Emoji("1F471-1F3FD-200D-2642-FE0F", null, "\ud83d\udc71\ud83c\udffd\u200d♂\ufe0f", "blond-haired-man", new string[1] { "blond-haired-man" }, null, null, "Smileys & People", 21, 17, SkinVariationType.Medium, "blond-haired man", new string[4] { "blond", "blond-haired person", "blond-haired man", "man" });

	public static readonly Emoji BlondHairedMan_MediumDark = new Emoji("1F471-1F3FE-200D-2642-FE0F", null, "\ud83d\udc71\ud83c\udffe\u200d♂\ufe0f", "blond-haired-man", new string[1] { "blond-haired-man" }, null, null, "Smileys & People", 21, 18, SkinVariationType.MediumDark, "blond-haired man", new string[4] { "blond", "blond-haired person", "blond-haired man", "man" });

	public static readonly Emoji BlondHairedMan_Dark = new Emoji("1F471-1F3FF-200D-2642-FE0F", null, "\ud83d\udc71\ud83c\udfff\u200d♂\ufe0f", "blond-haired-man", new string[1] { "blond-haired-man" }, null, null, "Smileys & People", 21, 19, SkinVariationType.Dark, "blond-haired man", new string[4] { "blond", "blond-haired person", "blond-haired man", "man" });

	public static readonly Emoji BeardedPerson = new Emoji("1F9D4", "BEARDED PERSON", "\ud83e\uddd4", "bearded_person", new string[1] { "bearded_person" }, null, null, "Smileys & People", 43, 16, SkinVariationType.None, "bearded person", new string[2] { "beard", "bearded person" });

	public static readonly Emoji BeardedPerson_Light = new Emoji("1F9D4-1F3FB", "BEARDED PERSON", "\ud83e\uddd4\ud83c\udffb", "bearded_person", new string[1] { "bearded_person" }, null, null, "Smileys & People", 43, 17, SkinVariationType.Light, "bearded person", new string[2] { "beard", "bearded person" });

	public static readonly Emoji BeardedPerson_MediumLight = new Emoji("1F9D4-1F3FC", "BEARDED PERSON", "\ud83e\uddd4\ud83c\udffc", "bearded_person", new string[1] { "bearded_person" }, null, null, "Smileys & People", 43, 18, SkinVariationType.MediumLight, "bearded person", new string[2] { "beard", "bearded person" });

	public static readonly Emoji BeardedPerson_Medium = new Emoji("1F9D4-1F3FD", "BEARDED PERSON", "\ud83e\uddd4\ud83c\udffd", "bearded_person", new string[1] { "bearded_person" }, null, null, "Smileys & People", 43, 19, SkinVariationType.Medium, "bearded person", new string[2] { "beard", "bearded person" });

	public static readonly Emoji BeardedPerson_MediumDark = new Emoji("1F9D4-1F3FE", "BEARDED PERSON", "\ud83e\uddd4\ud83c\udffe", "bearded_person", new string[1] { "bearded_person" }, null, null, "Smileys & People", 43, 20, SkinVariationType.MediumDark, "bearded person", new string[2] { "beard", "bearded person" });

	public static readonly Emoji BeardedPerson_Dark = new Emoji("1F9D4-1F3FF", "BEARDED PERSON", "\ud83e\uddd4\ud83c\udfff", "bearded_person", new string[1] { "bearded_person" }, null, null, "Smileys & People", 43, 21, SkinVariationType.Dark, "bearded person", new string[2] { "beard", "bearded person" });

	public static readonly Emoji Woman = new Emoji("1F469", "WOMAN", "\ud83d\udc69", "woman", new string[1] { "woman" }, null, null, "Smileys & People", 20, 23, SkinVariationType.None, "woman", new string[1] { "woman" });

	public static readonly Emoji Woman_Light = new Emoji("1F469-1F3FB", "WOMAN", "\ud83d\udc69\ud83c\udffb", "woman", new string[1] { "woman" }, null, null, "Smileys & People", 20, 24, SkinVariationType.Light, "woman", new string[1] { "woman" });

	public static readonly Emoji Woman_MediumLight = new Emoji("1F469-1F3FC", "WOMAN", "\ud83d\udc69\ud83c\udffc", "woman", new string[1] { "woman" }, null, null, "Smileys & People", 20, 25, SkinVariationType.MediumLight, "woman", new string[1] { "woman" });

	public static readonly Emoji Woman_Medium = new Emoji("1F469-1F3FD", "WOMAN", "\ud83d\udc69\ud83c\udffd", "woman", new string[1] { "woman" }, null, null, "Smileys & People", 20, 26, SkinVariationType.Medium, "woman", new string[1] { "woman" });

	public static readonly Emoji Woman_MediumDark = new Emoji("1F469-1F3FE", "WOMAN", "\ud83d\udc69\ud83c\udffe", "woman", new string[1] { "woman" }, null, null, "Smileys & People", 20, 27, SkinVariationType.MediumDark, "woman", new string[1] { "woman" });

	public static readonly Emoji Woman_Dark = new Emoji("1F469-1F3FF", "WOMAN", "\ud83d\udc69\ud83c\udfff", "woman", new string[1] { "woman" }, null, null, "Smileys & People", 20, 28, SkinVariationType.Dark, "woman", new string[1] { "woman" });

	public static readonly Emoji BlondHairedWoman = new Emoji("1F471-200D-2640-FE0F", null, "\ud83d\udc71\u200d♀\ufe0f", "blond-haired-woman", new string[1] { "blond-haired-woman" }, null, null, "Smileys & People", 21, 8, SkinVariationType.None, "blond-haired woman", new string[5] { "blond", "blond-haired person", "blond-haired woman", "blonde", "woman" });

	public static readonly Emoji BlondHairedWoman_Light = new Emoji("1F471-1F3FB-200D-2640-FE0F", null, "\ud83d\udc71\ud83c\udffb\u200d♀\ufe0f", "blond-haired-woman", new string[1] { "blond-haired-woman" }, null, null, "Smileys & People", 21, 9, SkinVariationType.Light, "blond-haired woman", new string[5] { "blond", "blond-haired person", "blond-haired woman", "blonde", "woman" });

	public static readonly Emoji BlondHairedWoman_MediumLight = new Emoji("1F471-1F3FC-200D-2640-FE0F", null, "\ud83d\udc71\ud83c\udffc\u200d♀\ufe0f", "blond-haired-woman", new string[1] { "blond-haired-woman" }, null, null, "Smileys & People", 21, 10, SkinVariationType.MediumLight, "blond-haired woman", new string[5] { "blond", "blond-haired person", "blond-haired woman", "blonde", "woman" });

	public static readonly Emoji BlondHairedWoman_Medium = new Emoji("1F471-1F3FD-200D-2640-FE0F", null, "\ud83d\udc71\ud83c\udffd\u200d♀\ufe0f", "blond-haired-woman", new string[1] { "blond-haired-woman" }, null, null, "Smileys & People", 21, 11, SkinVariationType.Medium, "blond-haired woman", new string[5] { "blond", "blond-haired person", "blond-haired woman", "blonde", "woman" });

	public static readonly Emoji BlondHairedWoman_MediumDark = new Emoji("1F471-1F3FE-200D-2640-FE0F", null, "\ud83d\udc71\ud83c\udffe\u200d♀\ufe0f", "blond-haired-woman", new string[1] { "blond-haired-woman" }, null, null, "Smileys & People", 21, 12, SkinVariationType.MediumDark, "blond-haired woman", new string[5] { "blond", "blond-haired person", "blond-haired woman", "blonde", "woman" });

	public static readonly Emoji BlondHairedWoman_Dark = new Emoji("1F471-1F3FF-200D-2640-FE0F", null, "\ud83d\udc71\ud83c\udfff\u200d♀\ufe0f", "blond-haired-woman", new string[1] { "blond-haired-woman" }, null, null, "Smileys & People", 21, 13, SkinVariationType.Dark, "blond-haired woman", new string[5] { "blond", "blond-haired person", "blond-haired woman", "blonde", "woman" });

	public static readonly Emoji OlderAdult = new Emoji("1F9D3", "OLDER ADULT", "\ud83e\uddd3", "older_adult", new string[1] { "older_adult" }, null, null, "Smileys & People", 43, 10, SkinVariationType.None, "older adult", new string[3] { "gender-neutral", "old", "older adult" });

	public static readonly Emoji OlderAdult_Light = new Emoji("1F9D3-1F3FB", "OLDER ADULT", "\ud83e\uddd3\ud83c\udffb", "older_adult", new string[1] { "older_adult" }, null, null, "Smileys & People", 43, 11, SkinVariationType.Light, "older adult", new string[3] { "gender-neutral", "old", "older adult" });

	public static readonly Emoji OlderAdult_MediumLight = new Emoji("1F9D3-1F3FC", "OLDER ADULT", "\ud83e\uddd3\ud83c\udffc", "older_adult", new string[1] { "older_adult" }, null, null, "Smileys & People", 43, 12, SkinVariationType.MediumLight, "older adult", new string[3] { "gender-neutral", "old", "older adult" });

	public static readonly Emoji OlderAdult_Medium = new Emoji("1F9D3-1F3FD", "OLDER ADULT", "\ud83e\uddd3\ud83c\udffd", "older_adult", new string[1] { "older_adult" }, null, null, "Smileys & People", 43, 13, SkinVariationType.Medium, "older adult", new string[3] { "gender-neutral", "old", "older adult" });

	public static readonly Emoji OlderAdult_MediumDark = new Emoji("1F9D3-1F3FE", "OLDER ADULT", "\ud83e\uddd3\ud83c\udffe", "older_adult", new string[1] { "older_adult" }, null, null, "Smileys & People", 43, 14, SkinVariationType.MediumDark, "older adult", new string[3] { "gender-neutral", "old", "older adult" });

	public static readonly Emoji OlderAdult_Dark = new Emoji("1F9D3-1F3FF", "OLDER ADULT", "\ud83e\uddd3\ud83c\udfff", "older_adult", new string[1] { "older_adult" }, null, null, "Smileys & People", 43, 15, SkinVariationType.Dark, "older adult", new string[3] { "gender-neutral", "old", "older adult" });

	public static readonly Emoji OlderMan = new Emoji("1F474", "OLDER MAN", "\ud83d\udc74", "older_man", new string[1] { "older_man" }, null, null, "Smileys & People", 21, 50, SkinVariationType.None, "old man", new string[2] { "man", "old" });

	public static readonly Emoji OlderMan_Light = new Emoji("1F474-1F3FB", "OLDER MAN", "\ud83d\udc74\ud83c\udffb", "older_man", new string[1] { "older_man" }, null, null, "Smileys & People", 21, 51, SkinVariationType.Light, "old man", new string[2] { "man", "old" });

	public static readonly Emoji OlderMan_MediumLight = new Emoji("1F474-1F3FC", "OLDER MAN", "\ud83d\udc74\ud83c\udffc", "older_man", new string[1] { "older_man" }, null, null, "Smileys & People", 22, 0, SkinVariationType.MediumLight, "old man", new string[2] { "man", "old" });

	public static readonly Emoji OlderMan_Medium = new Emoji("1F474-1F3FD", "OLDER MAN", "\ud83d\udc74\ud83c\udffd", "older_man", new string[1] { "older_man" }, null, null, "Smileys & People", 22, 1, SkinVariationType.Medium, "old man", new string[2] { "man", "old" });

	public static readonly Emoji OlderMan_MediumDark = new Emoji("1F474-1F3FE", "OLDER MAN", "\ud83d\udc74\ud83c\udffe", "older_man", new string[1] { "older_man" }, null, null, "Smileys & People", 22, 2, SkinVariationType.MediumDark, "old man", new string[2] { "man", "old" });

	public static readonly Emoji OlderMan_Dark = new Emoji("1F474-1F3FF", "OLDER MAN", "\ud83d\udc74\ud83c\udfff", "older_man", new string[1] { "older_man" }, null, null, "Smileys & People", 22, 3, SkinVariationType.Dark, "old man", new string[2] { "man", "old" });

	public static readonly Emoji OlderWoman = new Emoji("1F475", "OLDER WOMAN", "\ud83d\udc75", "older_woman", new string[1] { "older_woman" }, null, null, "Smileys & People", 22, 4, SkinVariationType.None, "old woman", new string[2] { "old", "woman" });

	public static readonly Emoji OlderWoman_Light = new Emoji("1F475-1F3FB", "OLDER WOMAN", "\ud83d\udc75\ud83c\udffb", "older_woman", new string[1] { "older_woman" }, null, null, "Smileys & People", 22, 5, SkinVariationType.Light, "old woman", new string[2] { "old", "woman" });

	public static readonly Emoji OlderWoman_MediumLight = new Emoji("1F475-1F3FC", "OLDER WOMAN", "\ud83d\udc75\ud83c\udffc", "older_woman", new string[1] { "older_woman" }, null, null, "Smileys & People", 22, 6, SkinVariationType.MediumLight, "old woman", new string[2] { "old", "woman" });

	public static readonly Emoji OlderWoman_Medium = new Emoji("1F475-1F3FD", "OLDER WOMAN", "\ud83d\udc75\ud83c\udffd", "older_woman", new string[1] { "older_woman" }, null, null, "Smileys & People", 22, 7, SkinVariationType.Medium, "old woman", new string[2] { "old", "woman" });

	public static readonly Emoji OlderWoman_MediumDark = new Emoji("1F475-1F3FE", "OLDER WOMAN", "\ud83d\udc75\ud83c\udffe", "older_woman", new string[1] { "older_woman" }, null, null, "Smileys & People", 22, 8, SkinVariationType.MediumDark, "old woman", new string[2] { "old", "woman" });

	public static readonly Emoji OlderWoman_Dark = new Emoji("1F475-1F3FF", "OLDER WOMAN", "\ud83d\udc75\ud83c\udfff", "older_woman", new string[1] { "older_woman" }, null, null, "Smileys & People", 22, 9, SkinVariationType.Dark, "old woman", new string[2] { "old", "woman" });

	public static readonly Emoji MaleDoctor = new Emoji("1F468-200D-2695-FE0F", null, "\ud83d\udc68\u200d⚕\ufe0f", "male-doctor", new string[1] { "male-doctor" }, null, null, "Smileys & People", 17, 43, SkinVariationType.None, "man health worker", new string[6] { "doctor", "healthcare", "man", "man health worker", "nurse", "therapist" });

	public static readonly Emoji MaleDoctor_Light = new Emoji("1F468-1F3FB-200D-2695-FE0F", null, "\ud83d\udc68\ud83c\udffb\u200d⚕\ufe0f", "male-doctor", new string[1] { "male-doctor" }, null, null, "Smileys & People", 17, 44, SkinVariationType.Light, "man health worker", new string[6] { "doctor", "healthcare", "man", "man health worker", "nurse", "therapist" });

	public static readonly Emoji MaleDoctor_MediumLight = new Emoji("1F468-1F3FC-200D-2695-FE0F", null, "\ud83d\udc68\ud83c\udffc\u200d⚕\ufe0f", "male-doctor", new string[1] { "male-doctor" }, null, null, "Smileys & People", 17, 45, SkinVariationType.MediumLight, "man health worker", new string[6] { "doctor", "healthcare", "man", "man health worker", "nurse", "therapist" });

	public static readonly Emoji MaleDoctor_Medium = new Emoji("1F468-1F3FD-200D-2695-FE0F", null, "\ud83d\udc68\ud83c\udffd\u200d⚕\ufe0f", "male-doctor", new string[1] { "male-doctor" }, null, null, "Smileys & People", 17, 46, SkinVariationType.Medium, "man health worker", new string[6] { "doctor", "healthcare", "man", "man health worker", "nurse", "therapist" });

	public static readonly Emoji MaleDoctor_MediumDark = new Emoji("1F468-1F3FE-200D-2695-FE0F", null, "\ud83d\udc68\ud83c\udffe\u200d⚕\ufe0f", "male-doctor", new string[1] { "male-doctor" }, null, null, "Smileys & People", 17, 47, SkinVariationType.MediumDark, "man health worker", new string[6] { "doctor", "healthcare", "man", "man health worker", "nurse", "therapist" });

	public static readonly Emoji MaleDoctor_Dark = new Emoji("1F468-1F3FF-200D-2695-FE0F", null, "\ud83d\udc68\ud83c\udfff\u200d⚕\ufe0f", "male-doctor", new string[1] { "male-doctor" }, null, null, "Smileys & People", 17, 48, SkinVariationType.Dark, "man health worker", new string[6] { "doctor", "healthcare", "man", "man health worker", "nurse", "therapist" });

	public static readonly Emoji FemaleDoctor = new Emoji("1F469-200D-2695-FE0F", null, "\ud83d\udc69\u200d⚕\ufe0f", "female-doctor", new string[1] { "female-doctor" }, null, null, "Smileys & People", 20, 1, SkinVariationType.None, "woman health worker", new string[6] { "doctor", "healthcare", "nurse", "therapist", "woman", "woman health worker" });

	public static readonly Emoji FemaleDoctor_Light = new Emoji("1F469-1F3FB-200D-2695-FE0F", null, "\ud83d\udc69\ud83c\udffb\u200d⚕\ufe0f", "female-doctor", new string[1] { "female-doctor" }, null, null, "Smileys & People", 20, 2, SkinVariationType.Light, "woman health worker", new string[6] { "doctor", "healthcare", "nurse", "therapist", "woman", "woman health worker" });

	public static readonly Emoji FemaleDoctor_MediumLight = new Emoji("1F469-1F3FC-200D-2695-FE0F", null, "\ud83d\udc69\ud83c\udffc\u200d⚕\ufe0f", "female-doctor", new string[1] { "female-doctor" }, null, null, "Smileys & People", 20, 3, SkinVariationType.MediumLight, "woman health worker", new string[6] { "doctor", "healthcare", "nurse", "therapist", "woman", "woman health worker" });

	public static readonly Emoji FemaleDoctor_Medium = new Emoji("1F469-1F3FD-200D-2695-FE0F", null, "\ud83d\udc69\ud83c\udffd\u200d⚕\ufe0f", "female-doctor", new string[1] { "female-doctor" }, null, null, "Smileys & People", 20, 4, SkinVariationType.Medium, "woman health worker", new string[6] { "doctor", "healthcare", "nurse", "therapist", "woman", "woman health worker" });

	public static readonly Emoji FemaleDoctor_MediumDark = new Emoji("1F469-1F3FE-200D-2695-FE0F", null, "\ud83d\udc69\ud83c\udffe\u200d⚕\ufe0f", "female-doctor", new string[1] { "female-doctor" }, null, null, "Smileys & People", 20, 5, SkinVariationType.MediumDark, "woman health worker", new string[6] { "doctor", "healthcare", "nurse", "therapist", "woman", "woman health worker" });

	public static readonly Emoji FemaleDoctor_Dark = new Emoji("1F469-1F3FF-200D-2695-FE0F", null, "\ud83d\udc69\ud83c\udfff\u200d⚕\ufe0f", "female-doctor", new string[1] { "female-doctor" }, null, null, "Smileys & People", 20, 6, SkinVariationType.Dark, "woman health worker", new string[6] { "doctor", "healthcare", "nurse", "therapist", "woman", "woman health worker" });

	public static readonly Emoji MaleStudent = new Emoji("1F468-200D-1F393", null, "\ud83d\udc68\u200d\ud83c\udf93", "male-student", new string[1] { "male-student" }, null, null, "Smileys & People", 16, 14, SkinVariationType.None, null, null);

	public static readonly Emoji MaleStudent_Light = new Emoji("1F468-1F3FB-200D-1F393", null, "\ud83d\udc68\ud83c\udffb\u200d\ud83c\udf93", "male-student", new string[1] { "male-student" }, null, null, "Smileys & People", 16, 15, SkinVariationType.Light, null, null);

	public static readonly Emoji MaleStudent_MediumLight = new Emoji("1F468-1F3FC-200D-1F393", null, "\ud83d\udc68\ud83c\udffc\u200d\ud83c\udf93", "male-student", new string[1] { "male-student" }, null, null, "Smileys & People", 16, 16, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji MaleStudent_Medium = new Emoji("1F468-1F3FD-200D-1F393", null, "\ud83d\udc68\ud83c\udffd\u200d\ud83c\udf93", "male-student", new string[1] { "male-student" }, null, null, "Smileys & People", 16, 17, SkinVariationType.Medium, null, null);

	public static readonly Emoji MaleStudent_MediumDark = new Emoji("1F468-1F3FE-200D-1F393", null, "\ud83d\udc68\ud83c\udffe\u200d\ud83c\udf93", "male-student", new string[1] { "male-student" }, null, null, "Smileys & People", 16, 18, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji MaleStudent_Dark = new Emoji("1F468-1F3FF-200D-1F393", null, "\ud83d\udc68\ud83c\udfff\u200d\ud83c\udf93", "male-student", new string[1] { "male-student" }, null, null, "Smileys & People", 16, 19, SkinVariationType.Dark, null, null);

	public static readonly Emoji FemaleStudent = new Emoji("1F469-200D-1F393", null, "\ud83d\udc69\u200d\ud83c\udf93", "female-student", new string[1] { "female-student" }, null, null, "Smileys & People", 18, 29, SkinVariationType.None, null, null);

	public static readonly Emoji FemaleStudent_Light = new Emoji("1F469-1F3FB-200D-1F393", null, "\ud83d\udc69\ud83c\udffb\u200d\ud83c\udf93", "female-student", new string[1] { "female-student" }, null, null, "Smileys & People", 18, 30, SkinVariationType.Light, null, null);

	public static readonly Emoji FemaleStudent_MediumLight = new Emoji("1F469-1F3FC-200D-1F393", null, "\ud83d\udc69\ud83c\udffc\u200d\ud83c\udf93", "female-student", new string[1] { "female-student" }, null, null, "Smileys & People", 18, 31, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji FemaleStudent_Medium = new Emoji("1F469-1F3FD-200D-1F393", null, "\ud83d\udc69\ud83c\udffd\u200d\ud83c\udf93", "female-student", new string[1] { "female-student" }, null, null, "Smileys & People", 18, 32, SkinVariationType.Medium, null, null);

	public static readonly Emoji FemaleStudent_MediumDark = new Emoji("1F469-1F3FE-200D-1F393", null, "\ud83d\udc69\ud83c\udffe\u200d\ud83c\udf93", "female-student", new string[1] { "female-student" }, null, null, "Smileys & People", 18, 33, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji FemaleStudent_Dark = new Emoji("1F469-1F3FF-200D-1F393", null, "\ud83d\udc69\ud83c\udfff\u200d\ud83c\udf93", "female-student", new string[1] { "female-student" }, null, null, "Smileys & People", 18, 34, SkinVariationType.Dark, null, null);

	public static readonly Emoji MaleTeacher = new Emoji("1F468-200D-1F3EB", null, "\ud83d\udc68\u200d\ud83c\udfeb", "male-teacher", new string[1] { "male-teacher" }, null, null, "Smileys & People", 16, 32, SkinVariationType.None, null, null);

	public static readonly Emoji MaleTeacher_Light = new Emoji("1F468-1F3FB-200D-1F3EB", null, "\ud83d\udc68\ud83c\udffb\u200d\ud83c\udfeb", "male-teacher", new string[1] { "male-teacher" }, null, null, "Smileys & People", 16, 33, SkinVariationType.Light, null, null);

	public static readonly Emoji MaleTeacher_MediumLight = new Emoji("1F468-1F3FC-200D-1F3EB", null, "\ud83d\udc68\ud83c\udffc\u200d\ud83c\udfeb", "male-teacher", new string[1] { "male-teacher" }, null, null, "Smileys & People", 16, 34, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji MaleTeacher_Medium = new Emoji("1F468-1F3FD-200D-1F3EB", null, "\ud83d\udc68\ud83c\udffd\u200d\ud83c\udfeb", "male-teacher", new string[1] { "male-teacher" }, null, null, "Smileys & People", 16, 35, SkinVariationType.Medium, null, null);

	public static readonly Emoji MaleTeacher_MediumDark = new Emoji("1F468-1F3FE-200D-1F3EB", null, "\ud83d\udc68\ud83c\udffe\u200d\ud83c\udfeb", "male-teacher", new string[1] { "male-teacher" }, null, null, "Smileys & People", 16, 36, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji MaleTeacher_Dark = new Emoji("1F468-1F3FF-200D-1F3EB", null, "\ud83d\udc68\ud83c\udfff\u200d\ud83c\udfeb", "male-teacher", new string[1] { "male-teacher" }, null, null, "Smileys & People", 16, 37, SkinVariationType.Dark, null, null);

	public static readonly Emoji FemaleTeacher = new Emoji("1F469-200D-1F3EB", null, "\ud83d\udc69\u200d\ud83c\udfeb", "female-teacher", new string[1] { "female-teacher" }, null, null, "Smileys & People", 18, 47, SkinVariationType.None, null, null);

	public static readonly Emoji FemaleTeacher_Light = new Emoji("1F469-1F3FB-200D-1F3EB", null, "\ud83d\udc69\ud83c\udffb\u200d\ud83c\udfeb", "female-teacher", new string[1] { "female-teacher" }, null, null, "Smileys & People", 18, 48, SkinVariationType.Light, null, null);

	public static readonly Emoji FemaleTeacher_MediumLight = new Emoji("1F469-1F3FC-200D-1F3EB", null, "\ud83d\udc69\ud83c\udffc\u200d\ud83c\udfeb", "female-teacher", new string[1] { "female-teacher" }, null, null, "Smileys & People", 18, 49, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji FemaleTeacher_Medium = new Emoji("1F469-1F3FD-200D-1F3EB", null, "\ud83d\udc69\ud83c\udffd\u200d\ud83c\udfeb", "female-teacher", new string[1] { "female-teacher" }, null, null, "Smileys & People", 18, 50, SkinVariationType.Medium, null, null);

	public static readonly Emoji FemaleTeacher_MediumDark = new Emoji("1F469-1F3FE-200D-1F3EB", null, "\ud83d\udc69\ud83c\udffe\u200d\ud83c\udfeb", "female-teacher", new string[1] { "female-teacher" }, null, null, "Smileys & People", 18, 51, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji FemaleTeacher_Dark = new Emoji("1F469-1F3FF-200D-1F3EB", null, "\ud83d\udc69\ud83c\udfff\u200d\ud83c\udfeb", "female-teacher", new string[1] { "female-teacher" }, null, null, "Smileys & People", 19, 0, SkinVariationType.Dark, null, null);

	public static readonly Emoji MaleJudge = new Emoji("1F468-200D-2696-FE0F", null, "\ud83d\udc68\u200d⚖\ufe0f", "male-judge", new string[1] { "male-judge" }, null, null, "Smileys & People", 17, 49, SkinVariationType.None, "man judge", new string[4] { "justice", "man", "man judge", "scales" });

	public static readonly Emoji MaleJudge_Light = new Emoji("1F468-1F3FB-200D-2696-FE0F", null, "\ud83d\udc68\ud83c\udffb\u200d⚖\ufe0f", "male-judge", new string[1] { "male-judge" }, null, null, "Smileys & People", 17, 50, SkinVariationType.Light, "man judge", new string[4] { "justice", "man", "man judge", "scales" });

	public static readonly Emoji MaleJudge_MediumLight = new Emoji("1F468-1F3FC-200D-2696-FE0F", null, "\ud83d\udc68\ud83c\udffc\u200d⚖\ufe0f", "male-judge", new string[1] { "male-judge" }, null, null, "Smileys & People", 17, 51, SkinVariationType.MediumLight, "man judge", new string[4] { "justice", "man", "man judge", "scales" });

	public static readonly Emoji MaleJudge_Medium = new Emoji("1F468-1F3FD-200D-2696-FE0F", null, "\ud83d\udc68\ud83c\udffd\u200d⚖\ufe0f", "male-judge", new string[1] { "male-judge" }, null, null, "Smileys & People", 18, 0, SkinVariationType.Medium, "man judge", new string[4] { "justice", "man", "man judge", "scales" });

	public static readonly Emoji MaleJudge_MediumDark = new Emoji("1F468-1F3FE-200D-2696-FE0F", null, "\ud83d\udc68\ud83c\udffe\u200d⚖\ufe0f", "male-judge", new string[1] { "male-judge" }, null, null, "Smileys & People", 18, 1, SkinVariationType.MediumDark, "man judge", new string[4] { "justice", "man", "man judge", "scales" });

	public static readonly Emoji MaleJudge_Dark = new Emoji("1F468-1F3FF-200D-2696-FE0F", null, "\ud83d\udc68\ud83c\udfff\u200d⚖\ufe0f", "male-judge", new string[1] { "male-judge" }, null, null, "Smileys & People", 18, 2, SkinVariationType.Dark, "man judge", new string[4] { "justice", "man", "man judge", "scales" });

	public static readonly Emoji FemaleJudge = new Emoji("1F469-200D-2696-FE0F", null, "\ud83d\udc69\u200d⚖\ufe0f", "female-judge", new string[1] { "female-judge" }, null, null, "Smileys & People", 20, 7, SkinVariationType.None, "woman judge", new string[3] { "judge", "scales", "woman" });

	public static readonly Emoji FemaleJudge_Light = new Emoji("1F469-1F3FB-200D-2696-FE0F", null, "\ud83d\udc69\ud83c\udffb\u200d⚖\ufe0f", "female-judge", new string[1] { "female-judge" }, null, null, "Smileys & People", 20, 8, SkinVariationType.Light, "woman judge", new string[3] { "judge", "scales", "woman" });

	public static readonly Emoji FemaleJudge_MediumLight = new Emoji("1F469-1F3FC-200D-2696-FE0F", null, "\ud83d\udc69\ud83c\udffc\u200d⚖\ufe0f", "female-judge", new string[1] { "female-judge" }, null, null, "Smileys & People", 20, 9, SkinVariationType.MediumLight, "woman judge", new string[3] { "judge", "scales", "woman" });

	public static readonly Emoji FemaleJudge_Medium = new Emoji("1F469-1F3FD-200D-2696-FE0F", null, "\ud83d\udc69\ud83c\udffd\u200d⚖\ufe0f", "female-judge", new string[1] { "female-judge" }, null, null, "Smileys & People", 20, 10, SkinVariationType.Medium, "woman judge", new string[3] { "judge", "scales", "woman" });

	public static readonly Emoji FemaleJudge_MediumDark = new Emoji("1F469-1F3FE-200D-2696-FE0F", null, "\ud83d\udc69\ud83c\udffe\u200d⚖\ufe0f", "female-judge", new string[1] { "female-judge" }, null, null, "Smileys & People", 20, 11, SkinVariationType.MediumDark, "woman judge", new string[3] { "judge", "scales", "woman" });

	public static readonly Emoji FemaleJudge_Dark = new Emoji("1F469-1F3FF-200D-2696-FE0F", null, "\ud83d\udc69\ud83c\udfff\u200d⚖\ufe0f", "female-judge", new string[1] { "female-judge" }, null, null, "Smileys & People", 20, 12, SkinVariationType.Dark, "woman judge", new string[3] { "judge", "scales", "woman" });

	public static readonly Emoji MaleFarmer = new Emoji("1F468-200D-1F33E", null, "\ud83d\udc68\u200d\ud83c\udf3e", "male-farmer", new string[1] { "male-farmer" }, null, null, "Smileys & People", 16, 2, SkinVariationType.None, null, null);

	public static readonly Emoji MaleFarmer_Light = new Emoji("1F468-1F3FB-200D-1F33E", null, "\ud83d\udc68\ud83c\udffb\u200d\ud83c\udf3e", "male-farmer", new string[1] { "male-farmer" }, null, null, "Smileys & People", 16, 3, SkinVariationType.Light, null, null);

	public static readonly Emoji MaleFarmer_MediumLight = new Emoji("1F468-1F3FC-200D-1F33E", null, "\ud83d\udc68\ud83c\udffc\u200d\ud83c\udf3e", "male-farmer", new string[1] { "male-farmer" }, null, null, "Smileys & People", 16, 4, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji MaleFarmer_Medium = new Emoji("1F468-1F3FD-200D-1F33E", null, "\ud83d\udc68\ud83c\udffd\u200d\ud83c\udf3e", "male-farmer", new string[1] { "male-farmer" }, null, null, "Smileys & People", 16, 5, SkinVariationType.Medium, null, null);

	public static readonly Emoji MaleFarmer_MediumDark = new Emoji("1F468-1F3FE-200D-1F33E", null, "\ud83d\udc68\ud83c\udffe\u200d\ud83c\udf3e", "male-farmer", new string[1] { "male-farmer" }, null, null, "Smileys & People", 16, 6, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji MaleFarmer_Dark = new Emoji("1F468-1F3FF-200D-1F33E", null, "\ud83d\udc68\ud83c\udfff\u200d\ud83c\udf3e", "male-farmer", new string[1] { "male-farmer" }, null, null, "Smileys & People", 16, 7, SkinVariationType.Dark, null, null);

	public static readonly Emoji FemaleFarmer = new Emoji("1F469-200D-1F33E", null, "\ud83d\udc69\u200d\ud83c\udf3e", "female-farmer", new string[1] { "female-farmer" }, null, null, "Smileys & People", 18, 17, SkinVariationType.None, null, null);

	public static readonly Emoji FemaleFarmer_Light = new Emoji("1F469-1F3FB-200D-1F33E", null, "\ud83d\udc69\ud83c\udffb\u200d\ud83c\udf3e", "female-farmer", new string[1] { "female-farmer" }, null, null, "Smileys & People", 18, 18, SkinVariationType.Light, null, null);

	public static readonly Emoji FemaleFarmer_MediumLight = new Emoji("1F469-1F3FC-200D-1F33E", null, "\ud83d\udc69\ud83c\udffc\u200d\ud83c\udf3e", "female-farmer", new string[1] { "female-farmer" }, null, null, "Smileys & People", 18, 19, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji FemaleFarmer_Medium = new Emoji("1F469-1F3FD-200D-1F33E", null, "\ud83d\udc69\ud83c\udffd\u200d\ud83c\udf3e", "female-farmer", new string[1] { "female-farmer" }, null, null, "Smileys & People", 18, 20, SkinVariationType.Medium, null, null);

	public static readonly Emoji FemaleFarmer_MediumDark = new Emoji("1F469-1F3FE-200D-1F33E", null, "\ud83d\udc69\ud83c\udffe\u200d\ud83c\udf3e", "female-farmer", new string[1] { "female-farmer" }, null, null, "Smileys & People", 18, 21, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji FemaleFarmer_Dark = new Emoji("1F469-1F3FF-200D-1F33E", null, "\ud83d\udc69\ud83c\udfff\u200d\ud83c\udf3e", "female-farmer", new string[1] { "female-farmer" }, null, null, "Smileys & People", 18, 22, SkinVariationType.Dark, null, null);

	public static readonly Emoji MaleCook = new Emoji("1F468-200D-1F373", null, "\ud83d\udc68\u200d\ud83c\udf73", "male-cook", new string[1] { "male-cook" }, null, null, "Smileys & People", 16, 8, SkinVariationType.None, null, null);

	public static readonly Emoji MaleCook_Light = new Emoji("1F468-1F3FB-200D-1F373", null, "\ud83d\udc68\ud83c\udffb\u200d\ud83c\udf73", "male-cook", new string[1] { "male-cook" }, null, null, "Smileys & People", 16, 9, SkinVariationType.Light, null, null);

	public static readonly Emoji MaleCook_MediumLight = new Emoji("1F468-1F3FC-200D-1F373", null, "\ud83d\udc68\ud83c\udffc\u200d\ud83c\udf73", "male-cook", new string[1] { "male-cook" }, null, null, "Smileys & People", 16, 10, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji MaleCook_Medium = new Emoji("1F468-1F3FD-200D-1F373", null, "\ud83d\udc68\ud83c\udffd\u200d\ud83c\udf73", "male-cook", new string[1] { "male-cook" }, null, null, "Smileys & People", 16, 11, SkinVariationType.Medium, null, null);

	public static readonly Emoji MaleCook_MediumDark = new Emoji("1F468-1F3FE-200D-1F373", null, "\ud83d\udc68\ud83c\udffe\u200d\ud83c\udf73", "male-cook", new string[1] { "male-cook" }, null, null, "Smileys & People", 16, 12, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji MaleCook_Dark = new Emoji("1F468-1F3FF-200D-1F373", null, "\ud83d\udc68\ud83c\udfff\u200d\ud83c\udf73", "male-cook", new string[1] { "male-cook" }, null, null, "Smileys & People", 16, 13, SkinVariationType.Dark, null, null);

	public static readonly Emoji FemaleCook = new Emoji("1F469-200D-1F373", null, "\ud83d\udc69\u200d\ud83c\udf73", "female-cook", new string[1] { "female-cook" }, null, null, "Smileys & People", 18, 23, SkinVariationType.None, null, null);

	public static readonly Emoji FemaleCook_Light = new Emoji("1F469-1F3FB-200D-1F373", null, "\ud83d\udc69\ud83c\udffb\u200d\ud83c\udf73", "female-cook", new string[1] { "female-cook" }, null, null, "Smileys & People", 18, 24, SkinVariationType.Light, null, null);

	public static readonly Emoji FemaleCook_MediumLight = new Emoji("1F469-1F3FC-200D-1F373", null, "\ud83d\udc69\ud83c\udffc\u200d\ud83c\udf73", "female-cook", new string[1] { "female-cook" }, null, null, "Smileys & People", 18, 25, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji FemaleCook_Medium = new Emoji("1F469-1F3FD-200D-1F373", null, "\ud83d\udc69\ud83c\udffd\u200d\ud83c\udf73", "female-cook", new string[1] { "female-cook" }, null, null, "Smileys & People", 18, 26, SkinVariationType.Medium, null, null);

	public static readonly Emoji FemaleCook_MediumDark = new Emoji("1F469-1F3FE-200D-1F373", null, "\ud83d\udc69\ud83c\udffe\u200d\ud83c\udf73", "female-cook", new string[1] { "female-cook" }, null, null, "Smileys & People", 18, 27, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji FemaleCook_Dark = new Emoji("1F469-1F3FF-200D-1F373", null, "\ud83d\udc69\ud83c\udfff\u200d\ud83c\udf73", "female-cook", new string[1] { "female-cook" }, null, null, "Smileys & People", 18, 28, SkinVariationType.Dark, null, null);

	public static readonly Emoji MaleMechanic = new Emoji("1F468-200D-1F527", null, "\ud83d\udc68\u200d\ud83d\udd27", "male-mechanic", new string[1] { "male-mechanic" }, null, null, "Smileys & People", 17, 19, SkinVariationType.None, null, null);

	public static readonly Emoji MaleMechanic_Light = new Emoji("1F468-1F3FB-200D-1F527", null, "\ud83d\udc68\ud83c\udffb\u200d\ud83d\udd27", "male-mechanic", new string[1] { "male-mechanic" }, null, null, "Smileys & People", 17, 20, SkinVariationType.Light, null, null);

	public static readonly Emoji MaleMechanic_MediumLight = new Emoji("1F468-1F3FC-200D-1F527", null, "\ud83d\udc68\ud83c\udffc\u200d\ud83d\udd27", "male-mechanic", new string[1] { "male-mechanic" }, null, null, "Smileys & People", 17, 21, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji MaleMechanic_Medium = new Emoji("1F468-1F3FD-200D-1F527", null, "\ud83d\udc68\ud83c\udffd\u200d\ud83d\udd27", "male-mechanic", new string[1] { "male-mechanic" }, null, null, "Smileys & People", 17, 22, SkinVariationType.Medium, null, null);

	public static readonly Emoji MaleMechanic_MediumDark = new Emoji("1F468-1F3FE-200D-1F527", null, "\ud83d\udc68\ud83c\udffe\u200d\ud83d\udd27", "male-mechanic", new string[1] { "male-mechanic" }, null, null, "Smileys & People", 17, 23, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji MaleMechanic_Dark = new Emoji("1F468-1F3FF-200D-1F527", null, "\ud83d\udc68\ud83c\udfff\u200d\ud83d\udd27", "male-mechanic", new string[1] { "male-mechanic" }, null, null, "Smileys & People", 17, 24, SkinVariationType.Dark, null, null);

	public static readonly Emoji FemaleMechanic = new Emoji("1F469-200D-1F527", null, "\ud83d\udc69\u200d\ud83d\udd27", "female-mechanic", new string[1] { "female-mechanic" }, null, null, "Smileys & People", 19, 29, SkinVariationType.None, null, null);

	public static readonly Emoji FemaleMechanic_Light = new Emoji("1F469-1F3FB-200D-1F527", null, "\ud83d\udc69\ud83c\udffb\u200d\ud83d\udd27", "female-mechanic", new string[1] { "female-mechanic" }, null, null, "Smileys & People", 19, 30, SkinVariationType.Light, null, null);

	public static readonly Emoji FemaleMechanic_MediumLight = new Emoji("1F469-1F3FC-200D-1F527", null, "\ud83d\udc69\ud83c\udffc\u200d\ud83d\udd27", "female-mechanic", new string[1] { "female-mechanic" }, null, null, "Smileys & People", 19, 31, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji FemaleMechanic_Medium = new Emoji("1F469-1F3FD-200D-1F527", null, "\ud83d\udc69\ud83c\udffd\u200d\ud83d\udd27", "female-mechanic", new string[1] { "female-mechanic" }, null, null, "Smileys & People", 19, 32, SkinVariationType.Medium, null, null);

	public static readonly Emoji FemaleMechanic_MediumDark = new Emoji("1F469-1F3FE-200D-1F527", null, "\ud83d\udc69\ud83c\udffe\u200d\ud83d\udd27", "female-mechanic", new string[1] { "female-mechanic" }, null, null, "Smileys & People", 19, 33, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji FemaleMechanic_Dark = new Emoji("1F469-1F3FF-200D-1F527", null, "\ud83d\udc69\ud83c\udfff\u200d\ud83d\udd27", "female-mechanic", new string[1] { "female-mechanic" }, null, null, "Smileys & People", 19, 34, SkinVariationType.Dark, null, null);

	public static readonly Emoji MaleFactoryWorker = new Emoji("1F468-200D-1F3ED", null, "\ud83d\udc68\u200d\ud83c\udfed", "male-factory-worker", new string[1] { "male-factory-worker" }, null, null, "Smileys & People", 16, 38, SkinVariationType.None, null, null);

	public static readonly Emoji MaleFactoryWorker_Light = new Emoji("1F468-1F3FB-200D-1F3ED", null, "\ud83d\udc68\ud83c\udffb\u200d\ud83c\udfed", "male-factory-worker", new string[1] { "male-factory-worker" }, null, null, "Smileys & People", 16, 39, SkinVariationType.Light, null, null);

	public static readonly Emoji MaleFactoryWorker_MediumLight = new Emoji("1F468-1F3FC-200D-1F3ED", null, "\ud83d\udc68\ud83c\udffc\u200d\ud83c\udfed", "male-factory-worker", new string[1] { "male-factory-worker" }, null, null, "Smileys & People", 16, 40, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji MaleFactoryWorker_Medium = new Emoji("1F468-1F3FD-200D-1F3ED", null, "\ud83d\udc68\ud83c\udffd\u200d\ud83c\udfed", "male-factory-worker", new string[1] { "male-factory-worker" }, null, null, "Smileys & People", 16, 41, SkinVariationType.Medium, null, null);

	public static readonly Emoji MaleFactoryWorker_MediumDark = new Emoji("1F468-1F3FE-200D-1F3ED", null, "\ud83d\udc68\ud83c\udffe\u200d\ud83c\udfed", "male-factory-worker", new string[1] { "male-factory-worker" }, null, null, "Smileys & People", 16, 42, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji MaleFactoryWorker_Dark = new Emoji("1F468-1F3FF-200D-1F3ED", null, "\ud83d\udc68\ud83c\udfff\u200d\ud83c\udfed", "male-factory-worker", new string[1] { "male-factory-worker" }, null, null, "Smileys & People", 16, 43, SkinVariationType.Dark, null, null);

	public static readonly Emoji FemaleFactoryWorker = new Emoji("1F469-200D-1F3ED", null, "\ud83d\udc69\u200d\ud83c\udfed", "female-factory-worker", new string[1] { "female-factory-worker" }, null, null, "Smileys & People", 19, 1, SkinVariationType.None, null, null);

	public static readonly Emoji FemaleFactoryWorker_Light = new Emoji("1F469-1F3FB-200D-1F3ED", null, "\ud83d\udc69\ud83c\udffb\u200d\ud83c\udfed", "female-factory-worker", new string[1] { "female-factory-worker" }, null, null, "Smileys & People", 19, 2, SkinVariationType.Light, null, null);

	public static readonly Emoji FemaleFactoryWorker_MediumLight = new Emoji("1F469-1F3FC-200D-1F3ED", null, "\ud83d\udc69\ud83c\udffc\u200d\ud83c\udfed", "female-factory-worker", new string[1] { "female-factory-worker" }, null, null, "Smileys & People", 19, 3, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji FemaleFactoryWorker_Medium = new Emoji("1F469-1F3FD-200D-1F3ED", null, "\ud83d\udc69\ud83c\udffd\u200d\ud83c\udfed", "female-factory-worker", new string[1] { "female-factory-worker" }, null, null, "Smileys & People", 19, 4, SkinVariationType.Medium, null, null);

	public static readonly Emoji FemaleFactoryWorker_MediumDark = new Emoji("1F469-1F3FE-200D-1F3ED", null, "\ud83d\udc69\ud83c\udffe\u200d\ud83c\udfed", "female-factory-worker", new string[1] { "female-factory-worker" }, null, null, "Smileys & People", 19, 5, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji FemaleFactoryWorker_Dark = new Emoji("1F469-1F3FF-200D-1F3ED", null, "\ud83d\udc69\ud83c\udfff\u200d\ud83c\udfed", "female-factory-worker", new string[1] { "female-factory-worker" }, null, null, "Smileys & People", 19, 6, SkinVariationType.Dark, null, null);

	public static readonly Emoji MaleOfficeWorker = new Emoji("1F468-200D-1F4BC", null, "\ud83d\udc68\u200d\ud83d\udcbc", "male-office-worker", new string[1] { "male-office-worker" }, null, null, "Smileys & People", 17, 13, SkinVariationType.None, null, null);

	public static readonly Emoji MaleOfficeWorker_Light = new Emoji("1F468-1F3FB-200D-1F4BC", null, "\ud83d\udc68\ud83c\udffb\u200d\ud83d\udcbc", "male-office-worker", new string[1] { "male-office-worker" }, null, null, "Smileys & People", 17, 14, SkinVariationType.Light, null, null);

	public static readonly Emoji MaleOfficeWorker_MediumLight = new Emoji("1F468-1F3FC-200D-1F4BC", null, "\ud83d\udc68\ud83c\udffc\u200d\ud83d\udcbc", "male-office-worker", new string[1] { "male-office-worker" }, null, null, "Smileys & People", 17, 15, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji MaleOfficeWorker_Medium = new Emoji("1F468-1F3FD-200D-1F4BC", null, "\ud83d\udc68\ud83c\udffd\u200d\ud83d\udcbc", "male-office-worker", new string[1] { "male-office-worker" }, null, null, "Smileys & People", 17, 16, SkinVariationType.Medium, null, null);

	public static readonly Emoji MaleOfficeWorker_MediumDark = new Emoji("1F468-1F3FE-200D-1F4BC", null, "\ud83d\udc68\ud83c\udffe\u200d\ud83d\udcbc", "male-office-worker", new string[1] { "male-office-worker" }, null, null, "Smileys & People", 17, 17, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji MaleOfficeWorker_Dark = new Emoji("1F468-1F3FF-200D-1F4BC", null, "\ud83d\udc68\ud83c\udfff\u200d\ud83d\udcbc", "male-office-worker", new string[1] { "male-office-worker" }, null, null, "Smileys & People", 17, 18, SkinVariationType.Dark, null, null);

	public static readonly Emoji FemaleOfficeWorker = new Emoji("1F469-200D-1F4BC", null, "\ud83d\udc69\u200d\ud83d\udcbc", "female-office-worker", new string[1] { "female-office-worker" }, null, null, "Smileys & People", 19, 23, SkinVariationType.None, null, null);

	public static readonly Emoji FemaleOfficeWorker_Light = new Emoji("1F469-1F3FB-200D-1F4BC", null, "\ud83d\udc69\ud83c\udffb\u200d\ud83d\udcbc", "female-office-worker", new string[1] { "female-office-worker" }, null, null, "Smileys & People", 19, 24, SkinVariationType.Light, null, null);

	public static readonly Emoji FemaleOfficeWorker_MediumLight = new Emoji("1F469-1F3FC-200D-1F4BC", null, "\ud83d\udc69\ud83c\udffc\u200d\ud83d\udcbc", "female-office-worker", new string[1] { "female-office-worker" }, null, null, "Smileys & People", 19, 25, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji FemaleOfficeWorker_Medium = new Emoji("1F469-1F3FD-200D-1F4BC", null, "\ud83d\udc69\ud83c\udffd\u200d\ud83d\udcbc", "female-office-worker", new string[1] { "female-office-worker" }, null, null, "Smileys & People", 19, 26, SkinVariationType.Medium, null, null);

	public static readonly Emoji FemaleOfficeWorker_MediumDark = new Emoji("1F469-1F3FE-200D-1F4BC", null, "\ud83d\udc69\ud83c\udffe\u200d\ud83d\udcbc", "female-office-worker", new string[1] { "female-office-worker" }, null, null, "Smileys & People", 19, 27, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji FemaleOfficeWorker_Dark = new Emoji("1F469-1F3FF-200D-1F4BC", null, "\ud83d\udc69\ud83c\udfff\u200d\ud83d\udcbc", "female-office-worker", new string[1] { "female-office-worker" }, null, null, "Smileys & People", 19, 28, SkinVariationType.Dark, null, null);

	public static readonly Emoji MaleScientist = new Emoji("1F468-200D-1F52C", null, "\ud83d\udc68\u200d\ud83d\udd2c", "male-scientist", new string[1] { "male-scientist" }, null, null, "Smileys & People", 17, 25, SkinVariationType.None, null, null);

	public static readonly Emoji MaleScientist_Light = new Emoji("1F468-1F3FB-200D-1F52C", null, "\ud83d\udc68\ud83c\udffb\u200d\ud83d\udd2c", "male-scientist", new string[1] { "male-scientist" }, null, null, "Smileys & People", 17, 26, SkinVariationType.Light, null, null);

	public static readonly Emoji MaleScientist_MediumLight = new Emoji("1F468-1F3FC-200D-1F52C", null, "\ud83d\udc68\ud83c\udffc\u200d\ud83d\udd2c", "male-scientist", new string[1] { "male-scientist" }, null, null, "Smileys & People", 17, 27, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji MaleScientist_Medium = new Emoji("1F468-1F3FD-200D-1F52C", null, "\ud83d\udc68\ud83c\udffd\u200d\ud83d\udd2c", "male-scientist", new string[1] { "male-scientist" }, null, null, "Smileys & People", 17, 28, SkinVariationType.Medium, null, null);

	public static readonly Emoji MaleScientist_MediumDark = new Emoji("1F468-1F3FE-200D-1F52C", null, "\ud83d\udc68\ud83c\udffe\u200d\ud83d\udd2c", "male-scientist", new string[1] { "male-scientist" }, null, null, "Smileys & People", 17, 29, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji MaleScientist_Dark = new Emoji("1F468-1F3FF-200D-1F52C", null, "\ud83d\udc68\ud83c\udfff\u200d\ud83d\udd2c", "male-scientist", new string[1] { "male-scientist" }, null, null, "Smileys & People", 17, 30, SkinVariationType.Dark, null, null);

	public static readonly Emoji FemaleScientist = new Emoji("1F469-200D-1F52C", null, "\ud83d\udc69\u200d\ud83d\udd2c", "female-scientist", new string[1] { "female-scientist" }, null, null, "Smileys & People", 19, 35, SkinVariationType.None, null, null);

	public static readonly Emoji FemaleScientist_Light = new Emoji("1F469-1F3FB-200D-1F52C", null, "\ud83d\udc69\ud83c\udffb\u200d\ud83d\udd2c", "female-scientist", new string[1] { "female-scientist" }, null, null, "Smileys & People", 19, 36, SkinVariationType.Light, null, null);

	public static readonly Emoji FemaleScientist_MediumLight = new Emoji("1F469-1F3FC-200D-1F52C", null, "\ud83d\udc69\ud83c\udffc\u200d\ud83d\udd2c", "female-scientist", new string[1] { "female-scientist" }, null, null, "Smileys & People", 19, 37, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji FemaleScientist_Medium = new Emoji("1F469-1F3FD-200D-1F52C", null, "\ud83d\udc69\ud83c\udffd\u200d\ud83d\udd2c", "female-scientist", new string[1] { "female-scientist" }, null, null, "Smileys & People", 19, 38, SkinVariationType.Medium, null, null);

	public static readonly Emoji FemaleScientist_MediumDark = new Emoji("1F469-1F3FE-200D-1F52C", null, "\ud83d\udc69\ud83c\udffe\u200d\ud83d\udd2c", "female-scientist", new string[1] { "female-scientist" }, null, null, "Smileys & People", 19, 39, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji FemaleScientist_Dark = new Emoji("1F469-1F3FF-200D-1F52C", null, "\ud83d\udc69\ud83c\udfff\u200d\ud83d\udd2c", "female-scientist", new string[1] { "female-scientist" }, null, null, "Smileys & People", 19, 40, SkinVariationType.Dark, null, null);

	public static readonly Emoji MaleTechnologist = new Emoji("1F468-200D-1F4BB", null, "\ud83d\udc68\u200d\ud83d\udcbb", "male-technologist", new string[1] { "male-technologist" }, null, null, "Smileys & People", 17, 7, SkinVariationType.None, null, null);

	public static readonly Emoji MaleTechnologist_Light = new Emoji("1F468-1F3FB-200D-1F4BB", null, "\ud83d\udc68\ud83c\udffb\u200d\ud83d\udcbb", "male-technologist", new string[1] { "male-technologist" }, null, null, "Smileys & People", 17, 8, SkinVariationType.Light, null, null);

	public static readonly Emoji MaleTechnologist_MediumLight = new Emoji("1F468-1F3FC-200D-1F4BB", null, "\ud83d\udc68\ud83c\udffc\u200d\ud83d\udcbb", "male-technologist", new string[1] { "male-technologist" }, null, null, "Smileys & People", 17, 9, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji MaleTechnologist_Medium = new Emoji("1F468-1F3FD-200D-1F4BB", null, "\ud83d\udc68\ud83c\udffd\u200d\ud83d\udcbb", "male-technologist", new string[1] { "male-technologist" }, null, null, "Smileys & People", 17, 10, SkinVariationType.Medium, null, null);

	public static readonly Emoji MaleTechnologist_MediumDark = new Emoji("1F468-1F3FE-200D-1F4BB", null, "\ud83d\udc68\ud83c\udffe\u200d\ud83d\udcbb", "male-technologist", new string[1] { "male-technologist" }, null, null, "Smileys & People", 17, 11, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji MaleTechnologist_Dark = new Emoji("1F468-1F3FF-200D-1F4BB", null, "\ud83d\udc68\ud83c\udfff\u200d\ud83d\udcbb", "male-technologist", new string[1] { "male-technologist" }, null, null, "Smileys & People", 17, 12, SkinVariationType.Dark, null, null);

	public static readonly Emoji FemaleTechnologist = new Emoji("1F469-200D-1F4BB", null, "\ud83d\udc69\u200d\ud83d\udcbb", "female-technologist", new string[1] { "female-technologist" }, null, null, "Smileys & People", 19, 17, SkinVariationType.None, null, null);

	public static readonly Emoji FemaleTechnologist_Light = new Emoji("1F469-1F3FB-200D-1F4BB", null, "\ud83d\udc69\ud83c\udffb\u200d\ud83d\udcbb", "female-technologist", new string[1] { "female-technologist" }, null, null, "Smileys & People", 19, 18, SkinVariationType.Light, null, null);

	public static readonly Emoji FemaleTechnologist_MediumLight = new Emoji("1F469-1F3FC-200D-1F4BB", null, "\ud83d\udc69\ud83c\udffc\u200d\ud83d\udcbb", "female-technologist", new string[1] { "female-technologist" }, null, null, "Smileys & People", 19, 19, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji FemaleTechnologist_Medium = new Emoji("1F469-1F3FD-200D-1F4BB", null, "\ud83d\udc69\ud83c\udffd\u200d\ud83d\udcbb", "female-technologist", new string[1] { "female-technologist" }, null, null, "Smileys & People", 19, 20, SkinVariationType.Medium, null, null);

	public static readonly Emoji FemaleTechnologist_MediumDark = new Emoji("1F469-1F3FE-200D-1F4BB", null, "\ud83d\udc69\ud83c\udffe\u200d\ud83d\udcbb", "female-technologist", new string[1] { "female-technologist" }, null, null, "Smileys & People", 19, 21, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji FemaleTechnologist_Dark = new Emoji("1F469-1F3FF-200D-1F4BB", null, "\ud83d\udc69\ud83c\udfff\u200d\ud83d\udcbb", "female-technologist", new string[1] { "female-technologist" }, null, null, "Smileys & People", 19, 22, SkinVariationType.Dark, null, null);

	public static readonly Emoji MaleSinger = new Emoji("1F468-200D-1F3A4", null, "\ud83d\udc68\u200d\ud83c\udfa4", "male-singer", new string[1] { "male-singer" }, null, null, "Smileys & People", 16, 20, SkinVariationType.None, null, null);

	public static readonly Emoji MaleSinger_Light = new Emoji("1F468-1F3FB-200D-1F3A4", null, "\ud83d\udc68\ud83c\udffb\u200d\ud83c\udfa4", "male-singer", new string[1] { "male-singer" }, null, null, "Smileys & People", 16, 21, SkinVariationType.Light, null, null);

	public static readonly Emoji MaleSinger_MediumLight = new Emoji("1F468-1F3FC-200D-1F3A4", null, "\ud83d\udc68\ud83c\udffc\u200d\ud83c\udfa4", "male-singer", new string[1] { "male-singer" }, null, null, "Smileys & People", 16, 22, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji MaleSinger_Medium = new Emoji("1F468-1F3FD-200D-1F3A4", null, "\ud83d\udc68\ud83c\udffd\u200d\ud83c\udfa4", "male-singer", new string[1] { "male-singer" }, null, null, "Smileys & People", 16, 23, SkinVariationType.Medium, null, null);

	public static readonly Emoji MaleSinger_MediumDark = new Emoji("1F468-1F3FE-200D-1F3A4", null, "\ud83d\udc68\ud83c\udffe\u200d\ud83c\udfa4", "male-singer", new string[1] { "male-singer" }, null, null, "Smileys & People", 16, 24, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji MaleSinger_Dark = new Emoji("1F468-1F3FF-200D-1F3A4", null, "\ud83d\udc68\ud83c\udfff\u200d\ud83c\udfa4", "male-singer", new string[1] { "male-singer" }, null, null, "Smileys & People", 16, 25, SkinVariationType.Dark, null, null);

	public static readonly Emoji FemaleSinger = new Emoji("1F469-200D-1F3A4", null, "\ud83d\udc69\u200d\ud83c\udfa4", "female-singer", new string[1] { "female-singer" }, null, null, "Smileys & People", 18, 35, SkinVariationType.None, null, null);

	public static readonly Emoji FemaleSinger_Light = new Emoji("1F469-1F3FB-200D-1F3A4", null, "\ud83d\udc69\ud83c\udffb\u200d\ud83c\udfa4", "female-singer", new string[1] { "female-singer" }, null, null, "Smileys & People", 18, 36, SkinVariationType.Light, null, null);

	public static readonly Emoji FemaleSinger_MediumLight = new Emoji("1F469-1F3FC-200D-1F3A4", null, "\ud83d\udc69\ud83c\udffc\u200d\ud83c\udfa4", "female-singer", new string[1] { "female-singer" }, null, null, "Smileys & People", 18, 37, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji FemaleSinger_Medium = new Emoji("1F469-1F3FD-200D-1F3A4", null, "\ud83d\udc69\ud83c\udffd\u200d\ud83c\udfa4", "female-singer", new string[1] { "female-singer" }, null, null, "Smileys & People", 18, 38, SkinVariationType.Medium, null, null);

	public static readonly Emoji FemaleSinger_MediumDark = new Emoji("1F469-1F3FE-200D-1F3A4", null, "\ud83d\udc69\ud83c\udffe\u200d\ud83c\udfa4", "female-singer", new string[1] { "female-singer" }, null, null, "Smileys & People", 18, 39, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji FemaleSinger_Dark = new Emoji("1F469-1F3FF-200D-1F3A4", null, "\ud83d\udc69\ud83c\udfff\u200d\ud83c\udfa4", "female-singer", new string[1] { "female-singer" }, null, null, "Smileys & People", 18, 40, SkinVariationType.Dark, null, null);

	public static readonly Emoji MaleArtist = new Emoji("1F468-200D-1F3A8", null, "\ud83d\udc68\u200d\ud83c\udfa8", "male-artist", new string[1] { "male-artist" }, null, null, "Smileys & People", 16, 26, SkinVariationType.None, null, null);

	public static readonly Emoji MaleArtist_Light = new Emoji("1F468-1F3FB-200D-1F3A8", null, "\ud83d\udc68\ud83c\udffb\u200d\ud83c\udfa8", "male-artist", new string[1] { "male-artist" }, null, null, "Smileys & People", 16, 27, SkinVariationType.Light, null, null);

	public static readonly Emoji MaleArtist_MediumLight = new Emoji("1F468-1F3FC-200D-1F3A8", null, "\ud83d\udc68\ud83c\udffc\u200d\ud83c\udfa8", "male-artist", new string[1] { "male-artist" }, null, null, "Smileys & People", 16, 28, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji MaleArtist_Medium = new Emoji("1F468-1F3FD-200D-1F3A8", null, "\ud83d\udc68\ud83c\udffd\u200d\ud83c\udfa8", "male-artist", new string[1] { "male-artist" }, null, null, "Smileys & People", 16, 29, SkinVariationType.Medium, null, null);

	public static readonly Emoji MaleArtist_MediumDark = new Emoji("1F468-1F3FE-200D-1F3A8", null, "\ud83d\udc68\ud83c\udffe\u200d\ud83c\udfa8", "male-artist", new string[1] { "male-artist" }, null, null, "Smileys & People", 16, 30, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji MaleArtist_Dark = new Emoji("1F468-1F3FF-200D-1F3A8", null, "\ud83d\udc68\ud83c\udfff\u200d\ud83c\udfa8", "male-artist", new string[1] { "male-artist" }, null, null, "Smileys & People", 16, 31, SkinVariationType.Dark, null, null);

	public static readonly Emoji FemaleArtist = new Emoji("1F469-200D-1F3A8", null, "\ud83d\udc69\u200d\ud83c\udfa8", "female-artist", new string[1] { "female-artist" }, null, null, "Smileys & People", 18, 41, SkinVariationType.None, null, null);

	public static readonly Emoji FemaleArtist_Light = new Emoji("1F469-1F3FB-200D-1F3A8", null, "\ud83d\udc69\ud83c\udffb\u200d\ud83c\udfa8", "female-artist", new string[1] { "female-artist" }, null, null, "Smileys & People", 18, 42, SkinVariationType.Light, null, null);

	public static readonly Emoji FemaleArtist_MediumLight = new Emoji("1F469-1F3FC-200D-1F3A8", null, "\ud83d\udc69\ud83c\udffc\u200d\ud83c\udfa8", "female-artist", new string[1] { "female-artist" }, null, null, "Smileys & People", 18, 43, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji FemaleArtist_Medium = new Emoji("1F469-1F3FD-200D-1F3A8", null, "\ud83d\udc69\ud83c\udffd\u200d\ud83c\udfa8", "female-artist", new string[1] { "female-artist" }, null, null, "Smileys & People", 18, 44, SkinVariationType.Medium, null, null);

	public static readonly Emoji FemaleArtist_MediumDark = new Emoji("1F469-1F3FE-200D-1F3A8", null, "\ud83d\udc69\ud83c\udffe\u200d\ud83c\udfa8", "female-artist", new string[1] { "female-artist" }, null, null, "Smileys & People", 18, 45, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji FemaleArtist_Dark = new Emoji("1F469-1F3FF-200D-1F3A8", null, "\ud83d\udc69\ud83c\udfff\u200d\ud83c\udfa8", "female-artist", new string[1] { "female-artist" }, null, null, "Smileys & People", 18, 46, SkinVariationType.Dark, null, null);

	public static readonly Emoji MalePilot = new Emoji("1F468-200D-2708-FE0F", null, "\ud83d\udc68\u200d✈\ufe0f", "male-pilot", new string[1] { "male-pilot" }, null, null, "Smileys & People", 18, 3, SkinVariationType.None, "man pilot", new string[3] { "man", "pilot", "plane" });

	public static readonly Emoji MalePilot_Light = new Emoji("1F468-1F3FB-200D-2708-FE0F", null, "\ud83d\udc68\ud83c\udffb\u200d✈\ufe0f", "male-pilot", new string[1] { "male-pilot" }, null, null, "Smileys & People", 18, 4, SkinVariationType.Light, "man pilot", new string[3] { "man", "pilot", "plane" });

	public static readonly Emoji MalePilot_MediumLight = new Emoji("1F468-1F3FC-200D-2708-FE0F", null, "\ud83d\udc68\ud83c\udffc\u200d✈\ufe0f", "male-pilot", new string[1] { "male-pilot" }, null, null, "Smileys & People", 18, 5, SkinVariationType.MediumLight, "man pilot", new string[3] { "man", "pilot", "plane" });

	public static readonly Emoji MalePilot_Medium = new Emoji("1F468-1F3FD-200D-2708-FE0F", null, "\ud83d\udc68\ud83c\udffd\u200d✈\ufe0f", "male-pilot", new string[1] { "male-pilot" }, null, null, "Smileys & People", 18, 6, SkinVariationType.Medium, "man pilot", new string[3] { "man", "pilot", "plane" });

	public static readonly Emoji MalePilot_MediumDark = new Emoji("1F468-1F3FE-200D-2708-FE0F", null, "\ud83d\udc68\ud83c\udffe\u200d✈\ufe0f", "male-pilot", new string[1] { "male-pilot" }, null, null, "Smileys & People", 18, 7, SkinVariationType.MediumDark, "man pilot", new string[3] { "man", "pilot", "plane" });

	public static readonly Emoji MalePilot_Dark = new Emoji("1F468-1F3FF-200D-2708-FE0F", null, "\ud83d\udc68\ud83c\udfff\u200d✈\ufe0f", "male-pilot", new string[1] { "male-pilot" }, null, null, "Smileys & People", 18, 8, SkinVariationType.Dark, "man pilot", new string[3] { "man", "pilot", "plane" });

	public static readonly Emoji FemalePilot = new Emoji("1F469-200D-2708-FE0F", null, "\ud83d\udc69\u200d✈\ufe0f", "female-pilot", new string[1] { "female-pilot" }, null, null, "Smileys & People", 20, 13, SkinVariationType.None, "woman pilot", new string[3] { "pilot", "plane", "woman" });

	public static readonly Emoji FemalePilot_Light = new Emoji("1F469-1F3FB-200D-2708-FE0F", null, "\ud83d\udc69\ud83c\udffb\u200d✈\ufe0f", "female-pilot", new string[1] { "female-pilot" }, null, null, "Smileys & People", 20, 14, SkinVariationType.Light, "woman pilot", new string[3] { "pilot", "plane", "woman" });

	public static readonly Emoji FemalePilot_MediumLight = new Emoji("1F469-1F3FC-200D-2708-FE0F", null, "\ud83d\udc69\ud83c\udffc\u200d✈\ufe0f", "female-pilot", new string[1] { "female-pilot" }, null, null, "Smileys & People", 20, 15, SkinVariationType.MediumLight, "woman pilot", new string[3] { "pilot", "plane", "woman" });

	public static readonly Emoji FemalePilot_Medium = new Emoji("1F469-1F3FD-200D-2708-FE0F", null, "\ud83d\udc69\ud83c\udffd\u200d✈\ufe0f", "female-pilot", new string[1] { "female-pilot" }, null, null, "Smileys & People", 20, 16, SkinVariationType.Medium, "woman pilot", new string[3] { "pilot", "plane", "woman" });

	public static readonly Emoji FemalePilot_MediumDark = new Emoji("1F469-1F3FE-200D-2708-FE0F", null, "\ud83d\udc69\ud83c\udffe\u200d✈\ufe0f", "female-pilot", new string[1] { "female-pilot" }, null, null, "Smileys & People", 20, 17, SkinVariationType.MediumDark, "woman pilot", new string[3] { "pilot", "plane", "woman" });

	public static readonly Emoji FemalePilot_Dark = new Emoji("1F469-1F3FF-200D-2708-FE0F", null, "\ud83d\udc69\ud83c\udfff\u200d✈\ufe0f", "female-pilot", new string[1] { "female-pilot" }, null, null, "Smileys & People", 20, 18, SkinVariationType.Dark, "woman pilot", new string[3] { "pilot", "plane", "woman" });

	public static readonly Emoji MaleAstronaut = new Emoji("1F468-200D-1F680", null, "\ud83d\udc68\u200d\ud83d\ude80", "male-astronaut", new string[1] { "male-astronaut" }, null, null, "Smileys & People", 17, 31, SkinVariationType.None, null, null);

	public static readonly Emoji MaleAstronaut_Light = new Emoji("1F468-1F3FB-200D-1F680", null, "\ud83d\udc68\ud83c\udffb\u200d\ud83d\ude80", "male-astronaut", new string[1] { "male-astronaut" }, null, null, "Smileys & People", 17, 32, SkinVariationType.Light, null, null);

	public static readonly Emoji MaleAstronaut_MediumLight = new Emoji("1F468-1F3FC-200D-1F680", null, "\ud83d\udc68\ud83c\udffc\u200d\ud83d\ude80", "male-astronaut", new string[1] { "male-astronaut" }, null, null, "Smileys & People", 17, 33, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji MaleAstronaut_Medium = new Emoji("1F468-1F3FD-200D-1F680", null, "\ud83d\udc68\ud83c\udffd\u200d\ud83d\ude80", "male-astronaut", new string[1] { "male-astronaut" }, null, null, "Smileys & People", 17, 34, SkinVariationType.Medium, null, null);

	public static readonly Emoji MaleAstronaut_MediumDark = new Emoji("1F468-1F3FE-200D-1F680", null, "\ud83d\udc68\ud83c\udffe\u200d\ud83d\ude80", "male-astronaut", new string[1] { "male-astronaut" }, null, null, "Smileys & People", 17, 35, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji MaleAstronaut_Dark = new Emoji("1F468-1F3FF-200D-1F680", null, "\ud83d\udc68\ud83c\udfff\u200d\ud83d\ude80", "male-astronaut", new string[1] { "male-astronaut" }, null, null, "Smileys & People", 17, 36, SkinVariationType.Dark, null, null);

	public static readonly Emoji FemaleAstronaut = new Emoji("1F469-200D-1F680", null, "\ud83d\udc69\u200d\ud83d\ude80", "female-astronaut", new string[1] { "female-astronaut" }, null, null, "Smileys & People", 19, 41, SkinVariationType.None, null, null);

	public static readonly Emoji FemaleAstronaut_Light = new Emoji("1F469-1F3FB-200D-1F680", null, "\ud83d\udc69\ud83c\udffb\u200d\ud83d\ude80", "female-astronaut", new string[1] { "female-astronaut" }, null, null, "Smileys & People", 19, 42, SkinVariationType.Light, null, null);

	public static readonly Emoji FemaleAstronaut_MediumLight = new Emoji("1F469-1F3FC-200D-1F680", null, "\ud83d\udc69\ud83c\udffc\u200d\ud83d\ude80", "female-astronaut", new string[1] { "female-astronaut" }, null, null, "Smileys & People", 19, 43, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji FemaleAstronaut_Medium = new Emoji("1F469-1F3FD-200D-1F680", null, "\ud83d\udc69\ud83c\udffd\u200d\ud83d\ude80", "female-astronaut", new string[1] { "female-astronaut" }, null, null, "Smileys & People", 19, 44, SkinVariationType.Medium, null, null);

	public static readonly Emoji FemaleAstronaut_MediumDark = new Emoji("1F469-1F3FE-200D-1F680", null, "\ud83d\udc69\ud83c\udffe\u200d\ud83d\ude80", "female-astronaut", new string[1] { "female-astronaut" }, null, null, "Smileys & People", 19, 45, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji FemaleAstronaut_Dark = new Emoji("1F469-1F3FF-200D-1F680", null, "\ud83d\udc69\ud83c\udfff\u200d\ud83d\ude80", "female-astronaut", new string[1] { "female-astronaut" }, null, null, "Smileys & People", 19, 46, SkinVariationType.Dark, null, null);

	public static readonly Emoji MaleFirefighter = new Emoji("1F468-200D-1F692", null, "\ud83d\udc68\u200d\ud83d\ude92", "male-firefighter", new string[1] { "male-firefighter" }, null, null, "Smileys & People", 17, 37, SkinVariationType.None, null, null);

	public static readonly Emoji MaleFirefighter_Light = new Emoji("1F468-1F3FB-200D-1F692", null, "\ud83d\udc68\ud83c\udffb\u200d\ud83d\ude92", "male-firefighter", new string[1] { "male-firefighter" }, null, null, "Smileys & People", 17, 38, SkinVariationType.Light, null, null);

	public static readonly Emoji MaleFirefighter_MediumLight = new Emoji("1F468-1F3FC-200D-1F692", null, "\ud83d\udc68\ud83c\udffc\u200d\ud83d\ude92", "male-firefighter", new string[1] { "male-firefighter" }, null, null, "Smileys & People", 17, 39, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji MaleFirefighter_Medium = new Emoji("1F468-1F3FD-200D-1F692", null, "\ud83d\udc68\ud83c\udffd\u200d\ud83d\ude92", "male-firefighter", new string[1] { "male-firefighter" }, null, null, "Smileys & People", 17, 40, SkinVariationType.Medium, null, null);

	public static readonly Emoji MaleFirefighter_MediumDark = new Emoji("1F468-1F3FE-200D-1F692", null, "\ud83d\udc68\ud83c\udffe\u200d\ud83d\ude92", "male-firefighter", new string[1] { "male-firefighter" }, null, null, "Smileys & People", 17, 41, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji MaleFirefighter_Dark = new Emoji("1F468-1F3FF-200D-1F692", null, "\ud83d\udc68\ud83c\udfff\u200d\ud83d\ude92", "male-firefighter", new string[1] { "male-firefighter" }, null, null, "Smileys & People", 17, 42, SkinVariationType.Dark, null, null);

	public static readonly Emoji FemaleFirefighter = new Emoji("1F469-200D-1F692", null, "\ud83d\udc69\u200d\ud83d\ude92", "female-firefighter", new string[1] { "female-firefighter" }, null, null, "Smileys & People", 19, 47, SkinVariationType.None, null, null);

	public static readonly Emoji FemaleFirefighter_Light = new Emoji("1F469-1F3FB-200D-1F692", null, "\ud83d\udc69\ud83c\udffb\u200d\ud83d\ude92", "female-firefighter", new string[1] { "female-firefighter" }, null, null, "Smileys & People", 19, 48, SkinVariationType.Light, null, null);

	public static readonly Emoji FemaleFirefighter_MediumLight = new Emoji("1F469-1F3FC-200D-1F692", null, "\ud83d\udc69\ud83c\udffc\u200d\ud83d\ude92", "female-firefighter", new string[1] { "female-firefighter" }, null, null, "Smileys & People", 19, 49, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji FemaleFirefighter_Medium = new Emoji("1F469-1F3FD-200D-1F692", null, "\ud83d\udc69\ud83c\udffd\u200d\ud83d\ude92", "female-firefighter", new string[1] { "female-firefighter" }, null, null, "Smileys & People", 19, 50, SkinVariationType.Medium, null, null);

	public static readonly Emoji FemaleFirefighter_MediumDark = new Emoji("1F469-1F3FE-200D-1F692", null, "\ud83d\udc69\ud83c\udffe\u200d\ud83d\ude92", "female-firefighter", new string[1] { "female-firefighter" }, null, null, "Smileys & People", 19, 51, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji FemaleFirefighter_Dark = new Emoji("1F469-1F3FF-200D-1F692", null, "\ud83d\udc69\ud83c\udfff\u200d\ud83d\ude92", "female-firefighter", new string[1] { "female-firefighter" }, null, null, "Smileys & People", 20, 0, SkinVariationType.Dark, null, null);

	public static readonly Emoji MalePoliceOfficer = new Emoji("1F46E-200D-2642-FE0F", null, "\ud83d\udc6e\u200d♂\ufe0f", "male-police-officer", new string[1] { "male-police-officer" }, null, null, "Smileys & People", 20, 39, SkinVariationType.None, "man police officer", new string[4] { "cop", "officer", "police", "man" });

	public static readonly Emoji MalePoliceOfficer_Light = new Emoji("1F46E-1F3FB-200D-2642-FE0F", null, "\ud83d\udc6e\ud83c\udffb\u200d♂\ufe0f", "male-police-officer", new string[1] { "male-police-officer" }, null, null, "Smileys & People", 20, 40, SkinVariationType.Light, "man police officer", new string[4] { "cop", "officer", "police", "man" });

	public static readonly Emoji MalePoliceOfficer_MediumLight = new Emoji("1F46E-1F3FC-200D-2642-FE0F", null, "\ud83d\udc6e\ud83c\udffc\u200d♂\ufe0f", "male-police-officer", new string[1] { "male-police-officer" }, null, null, "Smileys & People", 20, 41, SkinVariationType.MediumLight, "man police officer", new string[4] { "cop", "officer", "police", "man" });

	public static readonly Emoji MalePoliceOfficer_Medium = new Emoji("1F46E-1F3FD-200D-2642-FE0F", null, "\ud83d\udc6e\ud83c\udffd\u200d♂\ufe0f", "male-police-officer", new string[1] { "male-police-officer" }, null, null, "Smileys & People", 20, 42, SkinVariationType.Medium, "man police officer", new string[4] { "cop", "officer", "police", "man" });

	public static readonly Emoji MalePoliceOfficer_MediumDark = new Emoji("1F46E-1F3FE-200D-2642-FE0F", null, "\ud83d\udc6e\ud83c\udffe\u200d♂\ufe0f", "male-police-officer", new string[1] { "male-police-officer" }, null, null, "Smileys & People", 20, 43, SkinVariationType.MediumDark, "man police officer", new string[4] { "cop", "officer", "police", "man" });

	public static readonly Emoji MalePoliceOfficer_Dark = new Emoji("1F46E-1F3FF-200D-2642-FE0F", null, "\ud83d\udc6e\ud83c\udfff\u200d♂\ufe0f", "male-police-officer", new string[1] { "male-police-officer" }, null, null, "Smileys & People", 20, 44, SkinVariationType.Dark, "man police officer", new string[4] { "cop", "officer", "police", "man" });

	public static readonly Emoji FemalePoliceOfficer = new Emoji("1F46E-200D-2640-FE0F", null, "\ud83d\udc6e\u200d♀\ufe0f", "female-police-officer", new string[1] { "female-police-officer" }, null, null, "Smileys & People", 20, 33, SkinVariationType.None, "woman police officer", new string[4] { "cop", "officer", "police", "woman" });

	public static readonly Emoji FemalePoliceOfficer_Light = new Emoji("1F46E-1F3FB-200D-2640-FE0F", null, "\ud83d\udc6e\ud83c\udffb\u200d♀\ufe0f", "female-police-officer", new string[1] { "female-police-officer" }, null, null, "Smileys & People", 20, 34, SkinVariationType.Light, "woman police officer", new string[4] { "cop", "officer", "police", "woman" });

	public static readonly Emoji FemalePoliceOfficer_MediumLight = new Emoji("1F46E-1F3FC-200D-2640-FE0F", null, "\ud83d\udc6e\ud83c\udffc\u200d♀\ufe0f", "female-police-officer", new string[1] { "female-police-officer" }, null, null, "Smileys & People", 20, 35, SkinVariationType.MediumLight, "woman police officer", new string[4] { "cop", "officer", "police", "woman" });

	public static readonly Emoji FemalePoliceOfficer_Medium = new Emoji("1F46E-1F3FD-200D-2640-FE0F", null, "\ud83d\udc6e\ud83c\udffd\u200d♀\ufe0f", "female-police-officer", new string[1] { "female-police-officer" }, null, null, "Smileys & People", 20, 36, SkinVariationType.Medium, "woman police officer", new string[4] { "cop", "officer", "police", "woman" });

	public static readonly Emoji FemalePoliceOfficer_MediumDark = new Emoji("1F46E-1F3FE-200D-2640-FE0F", null, "\ud83d\udc6e\ud83c\udffe\u200d♀\ufe0f", "female-police-officer", new string[1] { "female-police-officer" }, null, null, "Smileys & People", 20, 37, SkinVariationType.MediumDark, "woman police officer", new string[4] { "cop", "officer", "police", "woman" });

	public static readonly Emoji FemalePoliceOfficer_Dark = new Emoji("1F46E-1F3FF-200D-2640-FE0F", null, "\ud83d\udc6e\ud83c\udfff\u200d♀\ufe0f", "female-police-officer", new string[1] { "female-police-officer" }, null, null, "Smileys & People", 20, 38, SkinVariationType.Dark, "woman police officer", new string[4] { "cop", "officer", "police", "woman" });

	public static readonly Emoji SleuthOrSpy = new Emoji("1F575-FE0F", null, "\ud83d\udd75\ufe0f", "sleuth_or_spy", new string[1] { "sleuth_or_spy" }, null, null, "Smileys & People", 29, 11, SkinVariationType.None, null, null);

	public static readonly Emoji SleuthOrSpy_Light = new Emoji("1F575-1F3FB", null, "\ud83d\udd75\ud83c\udffb", "sleuth_or_spy", new string[1] { "sleuth_or_spy" }, null, null, "Smileys & People", 29, 12, SkinVariationType.Light, null, null);

	public static readonly Emoji SleuthOrSpy_MediumLight = new Emoji("1F575-1F3FC", null, "\ud83d\udd75\ud83c\udffc", "sleuth_or_spy", new string[1] { "sleuth_or_spy" }, null, null, "Smileys & People", 29, 13, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji SleuthOrSpy_Medium = new Emoji("1F575-1F3FD", null, "\ud83d\udd75\ud83c\udffd", "sleuth_or_spy", new string[1] { "sleuth_or_spy" }, null, null, "Smileys & People", 29, 14, SkinVariationType.Medium, null, null);

	public static readonly Emoji SleuthOrSpy_MediumDark = new Emoji("1F575-1F3FE", null, "\ud83d\udd75\ud83c\udffe", "sleuth_or_spy", new string[1] { "sleuth_or_spy" }, null, null, "Smileys & People", 29, 15, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji SleuthOrSpy_Dark = new Emoji("1F575-1F3FF", null, "\ud83d\udd75\ud83c\udfff", "sleuth_or_spy", new string[1] { "sleuth_or_spy" }, null, null, "Smileys & People", 29, 16, SkinVariationType.Dark, null, null);

	public static readonly Emoji MaleDetective = new Emoji("1F575-FE0F-200D-2642-FE0F", null, "\ud83d\udd75\ufe0f\u200d♂\ufe0f", "male-detective", new string[1] { "male-detective" }, null, null, "Smileys & People", 29, 5, SkinVariationType.None, null, null);

	public static readonly Emoji MaleDetective_Light = new Emoji("1F575-1F3FB-200D-2642-FE0F", null, "\ud83d\udd75\ud83c\udffb\u200d♂\ufe0f", "male-detective", new string[1] { "male-detective" }, null, null, "Smileys & People", 29, 6, SkinVariationType.Light, null, null);

	public static readonly Emoji MaleDetective_MediumLight = new Emoji("1F575-1F3FC-200D-2642-FE0F", null, "\ud83d\udd75\ud83c\udffc\u200d♂\ufe0f", "male-detective", new string[1] { "male-detective" }, null, null, "Smileys & People", 29, 7, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji MaleDetective_Medium = new Emoji("1F575-1F3FD-200D-2642-FE0F", null, "\ud83d\udd75\ud83c\udffd\u200d♂\ufe0f", "male-detective", new string[1] { "male-detective" }, null, null, "Smileys & People", 29, 8, SkinVariationType.Medium, null, null);

	public static readonly Emoji MaleDetective_MediumDark = new Emoji("1F575-1F3FE-200D-2642-FE0F", null, "\ud83d\udd75\ud83c\udffe\u200d♂\ufe0f", "male-detective", new string[1] { "male-detective" }, null, null, "Smileys & People", 29, 9, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji MaleDetective_Dark = new Emoji("1F575-1F3FF-200D-2642-FE0F", null, "\ud83d\udd75\ud83c\udfff\u200d♂\ufe0f", "male-detective", new string[1] { "male-detective" }, null, null, "Smileys & People", 29, 10, SkinVariationType.Dark, null, null);

	public static readonly Emoji FemaleDetective = new Emoji("1F575-FE0F-200D-2640-FE0F", null, "\ud83d\udd75\ufe0f\u200d♀\ufe0f", "female-detective", new string[1] { "female-detective" }, null, null, "Smileys & People", 28, 51, SkinVariationType.None, null, null);

	public static readonly Emoji FemaleDetective_Light = new Emoji("1F575-1F3FB-200D-2640-FE0F", null, "\ud83d\udd75\ud83c\udffb\u200d♀\ufe0f", "female-detective", new string[1] { "female-detective" }, null, null, "Smileys & People", 29, 0, SkinVariationType.Light, null, null);

	public static readonly Emoji FemaleDetective_MediumLight = new Emoji("1F575-1F3FC-200D-2640-FE0F", null, "\ud83d\udd75\ud83c\udffc\u200d♀\ufe0f", "female-detective", new string[1] { "female-detective" }, null, null, "Smileys & People", 29, 1, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji FemaleDetective_Medium = new Emoji("1F575-1F3FD-200D-2640-FE0F", null, "\ud83d\udd75\ud83c\udffd\u200d♀\ufe0f", "female-detective", new string[1] { "female-detective" }, null, null, "Smileys & People", 29, 2, SkinVariationType.Medium, null, null);

	public static readonly Emoji FemaleDetective_MediumDark = new Emoji("1F575-1F3FE-200D-2640-FE0F", null, "\ud83d\udd75\ud83c\udffe\u200d♀\ufe0f", "female-detective", new string[1] { "female-detective" }, null, null, "Smileys & People", 29, 3, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji FemaleDetective_Dark = new Emoji("1F575-1F3FF-200D-2640-FE0F", null, "\ud83d\udd75\ud83c\udfff\u200d♀\ufe0f", "female-detective", new string[1] { "female-detective" }, null, null, "Smileys & People", 29, 4, SkinVariationType.Dark, null, null);

	public static readonly Emoji MaleGuard = new Emoji("1F482-200D-2642-FE0F", null, "\ud83d\udc82\u200d♂\ufe0f", "male-guard", new string[1] { "male-guard" }, null, null, "Smileys & People", 23, 25, SkinVariationType.None, "man guard", new string[2] { "guard", "man" });

	public static readonly Emoji MaleGuard_Light = new Emoji("1F482-1F3FB-200D-2642-FE0F", null, "\ud83d\udc82\ud83c\udffb\u200d♂\ufe0f", "male-guard", new string[1] { "male-guard" }, null, null, "Smileys & People", 23, 26, SkinVariationType.Light, "man guard", new string[2] { "guard", "man" });

	public static readonly Emoji MaleGuard_MediumLight = new Emoji("1F482-1F3FC-200D-2642-FE0F", null, "\ud83d\udc82\ud83c\udffc\u200d♂\ufe0f", "male-guard", new string[1] { "male-guard" }, null, null, "Smileys & People", 23, 27, SkinVariationType.MediumLight, "man guard", new string[2] { "guard", "man" });

	public static readonly Emoji MaleGuard_Medium = new Emoji("1F482-1F3FD-200D-2642-FE0F", null, "\ud83d\udc82\ud83c\udffd\u200d♂\ufe0f", "male-guard", new string[1] { "male-guard" }, null, null, "Smileys & People", 23, 28, SkinVariationType.Medium, "man guard", new string[2] { "guard", "man" });

	public static readonly Emoji MaleGuard_MediumDark = new Emoji("1F482-1F3FE-200D-2642-FE0F", null, "\ud83d\udc82\ud83c\udffe\u200d♂\ufe0f", "male-guard", new string[1] { "male-guard" }, null, null, "Smileys & People", 23, 29, SkinVariationType.MediumDark, "man guard", new string[2] { "guard", "man" });

	public static readonly Emoji MaleGuard_Dark = new Emoji("1F482-1F3FF-200D-2642-FE0F", null, "\ud83d\udc82\ud83c\udfff\u200d♂\ufe0f", "male-guard", new string[1] { "male-guard" }, null, null, "Smileys & People", 23, 30, SkinVariationType.Dark, "man guard", new string[2] { "guard", "man" });

	public static readonly Emoji FemaleGuard = new Emoji("1F482-200D-2640-FE0F", null, "\ud83d\udc82\u200d♀\ufe0f", "female-guard", new string[1] { "female-guard" }, null, null, "Smileys & People", 23, 19, SkinVariationType.None, "woman guard", new string[2] { "guard", "woman" });

	public static readonly Emoji FemaleGuard_Light = new Emoji("1F482-1F3FB-200D-2640-FE0F", null, "\ud83d\udc82\ud83c\udffb\u200d♀\ufe0f", "female-guard", new string[1] { "female-guard" }, null, null, "Smileys & People", 23, 20, SkinVariationType.Light, "woman guard", new string[2] { "guard", "woman" });

	public static readonly Emoji FemaleGuard_MediumLight = new Emoji("1F482-1F3FC-200D-2640-FE0F", null, "\ud83d\udc82\ud83c\udffc\u200d♀\ufe0f", "female-guard", new string[1] { "female-guard" }, null, null, "Smileys & People", 23, 21, SkinVariationType.MediumLight, "woman guard", new string[2] { "guard", "woman" });

	public static readonly Emoji FemaleGuard_Medium = new Emoji("1F482-1F3FD-200D-2640-FE0F", null, "\ud83d\udc82\ud83c\udffd\u200d♀\ufe0f", "female-guard", new string[1] { "female-guard" }, null, null, "Smileys & People", 23, 22, SkinVariationType.Medium, "woman guard", new string[2] { "guard", "woman" });

	public static readonly Emoji FemaleGuard_MediumDark = new Emoji("1F482-1F3FE-200D-2640-FE0F", null, "\ud83d\udc82\ud83c\udffe\u200d♀\ufe0f", "female-guard", new string[1] { "female-guard" }, null, null, "Smileys & People", 23, 23, SkinVariationType.MediumDark, "woman guard", new string[2] { "guard", "woman" });

	public static readonly Emoji FemaleGuard_Dark = new Emoji("1F482-1F3FF-200D-2640-FE0F", null, "\ud83d\udc82\ud83c\udfff\u200d♀\ufe0f", "female-guard", new string[1] { "female-guard" }, null, null, "Smileys & People", 23, 24, SkinVariationType.Dark, "woman guard", new string[2] { "guard", "woman" });

	public static readonly Emoji MaleConstructionWorker = new Emoji("1F477-200D-2642-FE0F", null, "\ud83d\udc77\u200d♂\ufe0f", "male-construction-worker", new string[1] { "male-construction-worker" }, null, null, "Smileys & People", 22, 22, SkinVariationType.None, "man construction worker", new string[4] { "construction", "hat", "worker", "man" });

	public static readonly Emoji MaleConstructionWorker_Light = new Emoji("1F477-1F3FB-200D-2642-FE0F", null, "\ud83d\udc77\ud83c\udffb\u200d♂\ufe0f", "male-construction-worker", new string[1] { "male-construction-worker" }, null, null, "Smileys & People", 22, 23, SkinVariationType.Light, "man construction worker", new string[4] { "construction", "hat", "worker", "man" });

	public static readonly Emoji MaleConstructionWorker_MediumLight = new Emoji("1F477-1F3FC-200D-2642-FE0F", null, "\ud83d\udc77\ud83c\udffc\u200d♂\ufe0f", "male-construction-worker", new string[1] { "male-construction-worker" }, null, null, "Smileys & People", 22, 24, SkinVariationType.MediumLight, "man construction worker", new string[4] { "construction", "hat", "worker", "man" });

	public static readonly Emoji MaleConstructionWorker_Medium = new Emoji("1F477-1F3FD-200D-2642-FE0F", null, "\ud83d\udc77\ud83c\udffd\u200d♂\ufe0f", "male-construction-worker", new string[1] { "male-construction-worker" }, null, null, "Smileys & People", 22, 25, SkinVariationType.Medium, "man construction worker", new string[4] { "construction", "hat", "worker", "man" });

	public static readonly Emoji MaleConstructionWorker_MediumDark = new Emoji("1F477-1F3FE-200D-2642-FE0F", null, "\ud83d\udc77\ud83c\udffe\u200d♂\ufe0f", "male-construction-worker", new string[1] { "male-construction-worker" }, null, null, "Smileys & People", 22, 26, SkinVariationType.MediumDark, "man construction worker", new string[4] { "construction", "hat", "worker", "man" });

	public static readonly Emoji MaleConstructionWorker_Dark = new Emoji("1F477-1F3FF-200D-2642-FE0F", null, "\ud83d\udc77\ud83c\udfff\u200d♂\ufe0f", "male-construction-worker", new string[1] { "male-construction-worker" }, null, null, "Smileys & People", 22, 27, SkinVariationType.Dark, "man construction worker", new string[4] { "construction", "hat", "worker", "man" });

	public static readonly Emoji FemaleConstructionWorker = new Emoji("1F477-200D-2640-FE0F", null, "\ud83d\udc77\u200d♀\ufe0f", "female-construction-worker", new string[1] { "female-construction-worker" }, null, null, "Smileys & People", 22, 16, SkinVariationType.None, "woman construction worker", new string[4] { "construction", "hat", "worker", "woman" });

	public static readonly Emoji FemaleConstructionWorker_Light = new Emoji("1F477-1F3FB-200D-2640-FE0F", null, "\ud83d\udc77\ud83c\udffb\u200d♀\ufe0f", "female-construction-worker", new string[1] { "female-construction-worker" }, null, null, "Smileys & People", 22, 17, SkinVariationType.Light, "woman construction worker", new string[4] { "construction", "hat", "worker", "woman" });

	public static readonly Emoji FemaleConstructionWorker_MediumLight = new Emoji("1F477-1F3FC-200D-2640-FE0F", null, "\ud83d\udc77\ud83c\udffc\u200d♀\ufe0f", "female-construction-worker", new string[1] { "female-construction-worker" }, null, null, "Smileys & People", 22, 18, SkinVariationType.MediumLight, "woman construction worker", new string[4] { "construction", "hat", "worker", "woman" });

	public static readonly Emoji FemaleConstructionWorker_Medium = new Emoji("1F477-1F3FD-200D-2640-FE0F", null, "\ud83d\udc77\ud83c\udffd\u200d♀\ufe0f", "female-construction-worker", new string[1] { "female-construction-worker" }, null, null, "Smileys & People", 22, 19, SkinVariationType.Medium, "woman construction worker", new string[4] { "construction", "hat", "worker", "woman" });

	public static readonly Emoji FemaleConstructionWorker_MediumDark = new Emoji("1F477-1F3FE-200D-2640-FE0F", null, "\ud83d\udc77\ud83c\udffe\u200d♀\ufe0f", "female-construction-worker", new string[1] { "female-construction-worker" }, null, null, "Smileys & People", 22, 20, SkinVariationType.MediumDark, "woman construction worker", new string[4] { "construction", "hat", "worker", "woman" });

	public static readonly Emoji FemaleConstructionWorker_Dark = new Emoji("1F477-1F3FF-200D-2640-FE0F", null, "\ud83d\udc77\ud83c\udfff\u200d♀\ufe0f", "female-construction-worker", new string[1] { "female-construction-worker" }, null, null, "Smileys & People", 22, 21, SkinVariationType.Dark, "woman construction worker", new string[4] { "construction", "hat", "worker", "woman" });

	public static readonly Emoji Prince = new Emoji("1F934", "PRINCE", "\ud83e\udd34", "prince", new string[1] { "prince" }, null, null, "Smileys & People", 39, 28, SkinVariationType.None, "prince", new string[1] { "prince" });

	public static readonly Emoji Prince_Light = new Emoji("1F934-1F3FB", "PRINCE", "\ud83e\udd34\ud83c\udffb", "prince", new string[1] { "prince" }, null, null, "Smileys & People", 39, 29, SkinVariationType.Light, "prince", new string[1] { "prince" });

	public static readonly Emoji Prince_MediumLight = new Emoji("1F934-1F3FC", "PRINCE", "\ud83e\udd34\ud83c\udffc", "prince", new string[1] { "prince" }, null, null, "Smileys & People", 39, 30, SkinVariationType.MediumLight, "prince", new string[1] { "prince" });

	public static readonly Emoji Prince_Medium = new Emoji("1F934-1F3FD", "PRINCE", "\ud83e\udd34\ud83c\udffd", "prince", new string[1] { "prince" }, null, null, "Smileys & People", 39, 31, SkinVariationType.Medium, "prince", new string[1] { "prince" });

	public static readonly Emoji Prince_MediumDark = new Emoji("1F934-1F3FE", "PRINCE", "\ud83e\udd34\ud83c\udffe", "prince", new string[1] { "prince" }, null, null, "Smileys & People", 39, 32, SkinVariationType.MediumDark, "prince", new string[1] { "prince" });

	public static readonly Emoji Prince_Dark = new Emoji("1F934-1F3FF", "PRINCE", "\ud83e\udd34\ud83c\udfff", "prince", new string[1] { "prince" }, null, null, "Smileys & People", 39, 33, SkinVariationType.Dark, "prince", new string[1] { "prince" });

	public static readonly Emoji Princess = new Emoji("1F478", "PRINCESS", "\ud83d\udc78", "princess", new string[1] { "princess" }, null, null, "Smileys & People", 22, 34, SkinVariationType.None, "princess", new string[3] { "fairy tale", "fantasy", "princess" });

	public static readonly Emoji Princess_Light = new Emoji("1F478-1F3FB", "PRINCESS", "\ud83d\udc78\ud83c\udffb", "princess", new string[1] { "princess" }, null, null, "Smileys & People", 22, 35, SkinVariationType.Light, "princess", new string[3] { "fairy tale", "fantasy", "princess" });

	public static readonly Emoji Princess_MediumLight = new Emoji("1F478-1F3FC", "PRINCESS", "\ud83d\udc78\ud83c\udffc", "princess", new string[1] { "princess" }, null, null, "Smileys & People", 22, 36, SkinVariationType.MediumLight, "princess", new string[3] { "fairy tale", "fantasy", "princess" });

	public static readonly Emoji Princess_Medium = new Emoji("1F478-1F3FD", "PRINCESS", "\ud83d\udc78\ud83c\udffd", "princess", new string[1] { "princess" }, null, null, "Smileys & People", 22, 37, SkinVariationType.Medium, "princess", new string[3] { "fairy tale", "fantasy", "princess" });

	public static readonly Emoji Princess_MediumDark = new Emoji("1F478-1F3FE", "PRINCESS", "\ud83d\udc78\ud83c\udffe", "princess", new string[1] { "princess" }, null, null, "Smileys & People", 22, 38, SkinVariationType.MediumDark, "princess", new string[3] { "fairy tale", "fantasy", "princess" });

	public static readonly Emoji Princess_Dark = new Emoji("1F478-1F3FF", "PRINCESS", "\ud83d\udc78\ud83c\udfff", "princess", new string[1] { "princess" }, null, null, "Smileys & People", 22, 39, SkinVariationType.Dark, "princess", new string[3] { "fairy tale", "fantasy", "princess" });

	public static readonly Emoji ManWearingTurban = new Emoji("1F473-200D-2642-FE0F", null, "\ud83d\udc73\u200d♂\ufe0f", "man-wearing-turban", new string[1] { "man-wearing-turban" }, null, null, "Smileys & People", 21, 38, SkinVariationType.None, "man wearing turban", new string[4] { "person wearing turban", "turban", "man", "man wearing turban" });

	public static readonly Emoji ManWearingTurban_Light = new Emoji("1F473-1F3FB-200D-2642-FE0F", null, "\ud83d\udc73\ud83c\udffb\u200d♂\ufe0f", "man-wearing-turban", new string[1] { "man-wearing-turban" }, null, null, "Smileys & People", 21, 39, SkinVariationType.Light, "man wearing turban", new string[4] { "person wearing turban", "turban", "man", "man wearing turban" });

	public static readonly Emoji ManWearingTurban_MediumLight = new Emoji("1F473-1F3FC-200D-2642-FE0F", null, "\ud83d\udc73\ud83c\udffc\u200d♂\ufe0f", "man-wearing-turban", new string[1] { "man-wearing-turban" }, null, null, "Smileys & People", 21, 40, SkinVariationType.MediumLight, "man wearing turban", new string[4] { "person wearing turban", "turban", "man", "man wearing turban" });

	public static readonly Emoji ManWearingTurban_Medium = new Emoji("1F473-1F3FD-200D-2642-FE0F", null, "\ud83d\udc73\ud83c\udffd\u200d♂\ufe0f", "man-wearing-turban", new string[1] { "man-wearing-turban" }, null, null, "Smileys & People", 21, 41, SkinVariationType.Medium, "man wearing turban", new string[4] { "person wearing turban", "turban", "man", "man wearing turban" });

	public static readonly Emoji ManWearingTurban_MediumDark = new Emoji("1F473-1F3FE-200D-2642-FE0F", null, "\ud83d\udc73\ud83c\udffe\u200d♂\ufe0f", "man-wearing-turban", new string[1] { "man-wearing-turban" }, null, null, "Smileys & People", 21, 42, SkinVariationType.MediumDark, "man wearing turban", new string[4] { "person wearing turban", "turban", "man", "man wearing turban" });

	public static readonly Emoji ManWearingTurban_Dark = new Emoji("1F473-1F3FF-200D-2642-FE0F", null, "\ud83d\udc73\ud83c\udfff\u200d♂\ufe0f", "man-wearing-turban", new string[1] { "man-wearing-turban" }, null, null, "Smileys & People", 21, 43, SkinVariationType.Dark, "man wearing turban", new string[4] { "person wearing turban", "turban", "man", "man wearing turban" });

	public static readonly Emoji WomanWearingTurban = new Emoji("1F473-200D-2640-FE0F", null, "\ud83d\udc73\u200d♀\ufe0f", "woman-wearing-turban", new string[1] { "woman-wearing-turban" }, null, null, "Smileys & People", 21, 32, SkinVariationType.None, "woman wearing turban", new string[4] { "person wearing turban", "turban", "woman", "woman wearing turban" });

	public static readonly Emoji WomanWearingTurban_Light = new Emoji("1F473-1F3FB-200D-2640-FE0F", null, "\ud83d\udc73\ud83c\udffb\u200d♀\ufe0f", "woman-wearing-turban", new string[1] { "woman-wearing-turban" }, null, null, "Smileys & People", 21, 33, SkinVariationType.Light, "woman wearing turban", new string[4] { "person wearing turban", "turban", "woman", "woman wearing turban" });

	public static readonly Emoji WomanWearingTurban_MediumLight = new Emoji("1F473-1F3FC-200D-2640-FE0F", null, "\ud83d\udc73\ud83c\udffc\u200d♀\ufe0f", "woman-wearing-turban", new string[1] { "woman-wearing-turban" }, null, null, "Smileys & People", 21, 34, SkinVariationType.MediumLight, "woman wearing turban", new string[4] { "person wearing turban", "turban", "woman", "woman wearing turban" });

	public static readonly Emoji WomanWearingTurban_Medium = new Emoji("1F473-1F3FD-200D-2640-FE0F", null, "\ud83d\udc73\ud83c\udffd\u200d♀\ufe0f", "woman-wearing-turban", new string[1] { "woman-wearing-turban" }, null, null, "Smileys & People", 21, 35, SkinVariationType.Medium, "woman wearing turban", new string[4] { "person wearing turban", "turban", "woman", "woman wearing turban" });

	public static readonly Emoji WomanWearingTurban_MediumDark = new Emoji("1F473-1F3FE-200D-2640-FE0F", null, "\ud83d\udc73\ud83c\udffe\u200d♀\ufe0f", "woman-wearing-turban", new string[1] { "woman-wearing-turban" }, null, null, "Smileys & People", 21, 36, SkinVariationType.MediumDark, "woman wearing turban", new string[4] { "person wearing turban", "turban", "woman", "woman wearing turban" });

	public static readonly Emoji WomanWearingTurban_Dark = new Emoji("1F473-1F3FF-200D-2640-FE0F", null, "\ud83d\udc73\ud83c\udfff\u200d♀\ufe0f", "woman-wearing-turban", new string[1] { "woman-wearing-turban" }, null, null, "Smileys & People", 21, 37, SkinVariationType.Dark, "woman wearing turban", new string[4] { "person wearing turban", "turban", "woman", "woman wearing turban" });

	public static readonly Emoji ManWithGuaPiMao = new Emoji("1F472", "MAN WITH GUA PI MAO", "\ud83d\udc72", "man_with_gua_pi_mao", new string[1] { "man_with_gua_pi_mao" }, null, null, "Smileys & People", 21, 26, SkinVariationType.None, "man with Chinese cap", new string[4] { "gua pi mao", "hat", "man", "man with Chinese cap" });

	public static readonly Emoji ManWithGuaPiMao_Light = new Emoji("1F472-1F3FB", "MAN WITH GUA PI MAO", "\ud83d\udc72\ud83c\udffb", "man_with_gua_pi_mao", new string[1] { "man_with_gua_pi_mao" }, null, null, "Smileys & People", 21, 27, SkinVariationType.Light, "man with Chinese cap", new string[4] { "gua pi mao", "hat", "man", "man with Chinese cap" });

	public static readonly Emoji ManWithGuaPiMao_MediumLight = new Emoji("1F472-1F3FC", "MAN WITH GUA PI MAO", "\ud83d\udc72\ud83c\udffc", "man_with_gua_pi_mao", new string[1] { "man_with_gua_pi_mao" }, null, null, "Smileys & People", 21, 28, SkinVariationType.MediumLight, "man with Chinese cap", new string[4] { "gua pi mao", "hat", "man", "man with Chinese cap" });

	public static readonly Emoji ManWithGuaPiMao_Medium = new Emoji("1F472-1F3FD", "MAN WITH GUA PI MAO", "\ud83d\udc72\ud83c\udffd", "man_with_gua_pi_mao", new string[1] { "man_with_gua_pi_mao" }, null, null, "Smileys & People", 21, 29, SkinVariationType.Medium, "man with Chinese cap", new string[4] { "gua pi mao", "hat", "man", "man with Chinese cap" });

	public static readonly Emoji ManWithGuaPiMao_MediumDark = new Emoji("1F472-1F3FE", "MAN WITH GUA PI MAO", "\ud83d\udc72\ud83c\udffe", "man_with_gua_pi_mao", new string[1] { "man_with_gua_pi_mao" }, null, null, "Smileys & People", 21, 30, SkinVariationType.MediumDark, "man with Chinese cap", new string[4] { "gua pi mao", "hat", "man", "man with Chinese cap" });

	public static readonly Emoji ManWithGuaPiMao_Dark = new Emoji("1F472-1F3FF", "MAN WITH GUA PI MAO", "\ud83d\udc72\ud83c\udfff", "man_with_gua_pi_mao", new string[1] { "man_with_gua_pi_mao" }, null, null, "Smileys & People", 21, 31, SkinVariationType.Dark, "man with Chinese cap", new string[4] { "gua pi mao", "hat", "man", "man with Chinese cap" });

	public static readonly Emoji PersonWithHeadscarf = new Emoji("1F9D5", "PERSON WITH HEADSCARF", "\ud83e\uddd5", "person_with_headscarf", new string[1] { "person_with_headscarf" }, null, null, "Smileys & People", 43, 22, SkinVariationType.None, "woman with headscarf", new string[5] { "headscarf", "hijab", "mantilla", "tichel", "woman with headscarf" });

	public static readonly Emoji PersonWithHeadscarf_Light = new Emoji("1F9D5-1F3FB", "PERSON WITH HEADSCARF", "\ud83e\uddd5\ud83c\udffb", "person_with_headscarf", new string[1] { "person_with_headscarf" }, null, null, "Smileys & People", 43, 23, SkinVariationType.Light, "woman with headscarf", new string[5] { "headscarf", "hijab", "mantilla", "tichel", "woman with headscarf" });

	public static readonly Emoji PersonWithHeadscarf_MediumLight = new Emoji("1F9D5-1F3FC", "PERSON WITH HEADSCARF", "\ud83e\uddd5\ud83c\udffc", "person_with_headscarf", new string[1] { "person_with_headscarf" }, null, null, "Smileys & People", 43, 24, SkinVariationType.MediumLight, "woman with headscarf", new string[5] { "headscarf", "hijab", "mantilla", "tichel", "woman with headscarf" });

	public static readonly Emoji PersonWithHeadscarf_Medium = new Emoji("1F9D5-1F3FD", "PERSON WITH HEADSCARF", "\ud83e\uddd5\ud83c\udffd", "person_with_headscarf", new string[1] { "person_with_headscarf" }, null, null, "Smileys & People", 43, 25, SkinVariationType.Medium, "woman with headscarf", new string[5] { "headscarf", "hijab", "mantilla", "tichel", "woman with headscarf" });

	public static readonly Emoji PersonWithHeadscarf_MediumDark = new Emoji("1F9D5-1F3FE", "PERSON WITH HEADSCARF", "\ud83e\uddd5\ud83c\udffe", "person_with_headscarf", new string[1] { "person_with_headscarf" }, null, null, "Smileys & People", 43, 26, SkinVariationType.MediumDark, "woman with headscarf", new string[5] { "headscarf", "hijab", "mantilla", "tichel", "woman with headscarf" });

	public static readonly Emoji PersonWithHeadscarf_Dark = new Emoji("1F9D5-1F3FF", "PERSON WITH HEADSCARF", "\ud83e\uddd5\ud83c\udfff", "person_with_headscarf", new string[1] { "person_with_headscarf" }, null, null, "Smileys & People", 43, 27, SkinVariationType.Dark, "woman with headscarf", new string[5] { "headscarf", "hijab", "mantilla", "tichel", "woman with headscarf" });

	public static readonly Emoji ManInTuxedo = new Emoji("1F935", "MAN IN TUXEDO", "\ud83e\udd35", "man_in_tuxedo", new string[1] { "man_in_tuxedo" }, null, null, "Smileys & People", 39, 34, SkinVariationType.None, "man in tuxedo", new string[4] { "groom", "man", "man in tuxedo", "tuxedo" });

	public static readonly Emoji ManInTuxedo_Light = new Emoji("1F935-1F3FB", "MAN IN TUXEDO", "\ud83e\udd35\ud83c\udffb", "man_in_tuxedo", new string[1] { "man_in_tuxedo" }, null, null, "Smileys & People", 39, 35, SkinVariationType.Light, "man in tuxedo", new string[4] { "groom", "man", "man in tuxedo", "tuxedo" });

	public static readonly Emoji ManInTuxedo_MediumLight = new Emoji("1F935-1F3FC", "MAN IN TUXEDO", "\ud83e\udd35\ud83c\udffc", "man_in_tuxedo", new string[1] { "man_in_tuxedo" }, null, null, "Smileys & People", 39, 36, SkinVariationType.MediumLight, "man in tuxedo", new string[4] { "groom", "man", "man in tuxedo", "tuxedo" });

	public static readonly Emoji ManInTuxedo_Medium = new Emoji("1F935-1F3FD", "MAN IN TUXEDO", "\ud83e\udd35\ud83c\udffd", "man_in_tuxedo", new string[1] { "man_in_tuxedo" }, null, null, "Smileys & People", 39, 37, SkinVariationType.Medium, "man in tuxedo", new string[4] { "groom", "man", "man in tuxedo", "tuxedo" });

	public static readonly Emoji ManInTuxedo_MediumDark = new Emoji("1F935-1F3FE", "MAN IN TUXEDO", "\ud83e\udd35\ud83c\udffe", "man_in_tuxedo", new string[1] { "man_in_tuxedo" }, null, null, "Smileys & People", 39, 38, SkinVariationType.MediumDark, "man in tuxedo", new string[4] { "groom", "man", "man in tuxedo", "tuxedo" });

	public static readonly Emoji ManInTuxedo_Dark = new Emoji("1F935-1F3FF", "MAN IN TUXEDO", "\ud83e\udd35\ud83c\udfff", "man_in_tuxedo", new string[1] { "man_in_tuxedo" }, null, null, "Smileys & People", 39, 39, SkinVariationType.Dark, "man in tuxedo", new string[4] { "groom", "man", "man in tuxedo", "tuxedo" });

	public static readonly Emoji BrideWithVeil = new Emoji("1F470", "BRIDE WITH VEIL", "\ud83d\udc70", "bride_with_veil", new string[1] { "bride_with_veil" }, null, null, "Smileys & People", 21, 2, SkinVariationType.None, "bride with veil", new string[4] { "bride", "bride with veil", "veil", "wedding" });

	public static readonly Emoji BrideWithVeil_Light = new Emoji("1F470-1F3FB", "BRIDE WITH VEIL", "\ud83d\udc70\ud83c\udffb", "bride_with_veil", new string[1] { "bride_with_veil" }, null, null, "Smileys & People", 21, 3, SkinVariationType.Light, "bride with veil", new string[4] { "bride", "bride with veil", "veil", "wedding" });

	public static readonly Emoji BrideWithVeil_MediumLight = new Emoji("1F470-1F3FC", "BRIDE WITH VEIL", "\ud83d\udc70\ud83c\udffc", "bride_with_veil", new string[1] { "bride_with_veil" }, null, null, "Smileys & People", 21, 4, SkinVariationType.MediumLight, "bride with veil", new string[4] { "bride", "bride with veil", "veil", "wedding" });

	public static readonly Emoji BrideWithVeil_Medium = new Emoji("1F470-1F3FD", "BRIDE WITH VEIL", "\ud83d\udc70\ud83c\udffd", "bride_with_veil", new string[1] { "bride_with_veil" }, null, null, "Smileys & People", 21, 5, SkinVariationType.Medium, "bride with veil", new string[4] { "bride", "bride with veil", "veil", "wedding" });

	public static readonly Emoji BrideWithVeil_MediumDark = new Emoji("1F470-1F3FE", "BRIDE WITH VEIL", "\ud83d\udc70\ud83c\udffe", "bride_with_veil", new string[1] { "bride_with_veil" }, null, null, "Smileys & People", 21, 6, SkinVariationType.MediumDark, "bride with veil", new string[4] { "bride", "bride with veil", "veil", "wedding" });

	public static readonly Emoji BrideWithVeil_Dark = new Emoji("1F470-1F3FF", "BRIDE WITH VEIL", "\ud83d\udc70\ud83c\udfff", "bride_with_veil", new string[1] { "bride_with_veil" }, null, null, "Smileys & People", 21, 7, SkinVariationType.Dark, "bride with veil", new string[4] { "bride", "bride with veil", "veil", "wedding" });

	public static readonly Emoji PregnantWoman = new Emoji("1F930", "PREGNANT WOMAN", "\ud83e\udd30", "pregnant_woman", new string[1] { "pregnant_woman" }, null, null, "Smileys & People", 39, 4, SkinVariationType.None, "pregnant woman", new string[2] { "pregnant", "woman" });

	public static readonly Emoji PregnantWoman_Light = new Emoji("1F930-1F3FB", "PREGNANT WOMAN", "\ud83e\udd30\ud83c\udffb", "pregnant_woman", new string[1] { "pregnant_woman" }, null, null, "Smileys & People", 39, 5, SkinVariationType.Light, "pregnant woman", new string[2] { "pregnant", "woman" });

	public static readonly Emoji PregnantWoman_MediumLight = new Emoji("1F930-1F3FC", "PREGNANT WOMAN", "\ud83e\udd30\ud83c\udffc", "pregnant_woman", new string[1] { "pregnant_woman" }, null, null, "Smileys & People", 39, 6, SkinVariationType.MediumLight, "pregnant woman", new string[2] { "pregnant", "woman" });

	public static readonly Emoji PregnantWoman_Medium = new Emoji("1F930-1F3FD", "PREGNANT WOMAN", "\ud83e\udd30\ud83c\udffd", "pregnant_woman", new string[1] { "pregnant_woman" }, null, null, "Smileys & People", 39, 7, SkinVariationType.Medium, "pregnant woman", new string[2] { "pregnant", "woman" });

	public static readonly Emoji PregnantWoman_MediumDark = new Emoji("1F930-1F3FE", "PREGNANT WOMAN", "\ud83e\udd30\ud83c\udffe", "pregnant_woman", new string[1] { "pregnant_woman" }, null, null, "Smileys & People", 39, 8, SkinVariationType.MediumDark, "pregnant woman", new string[2] { "pregnant", "woman" });

	public static readonly Emoji PregnantWoman_Dark = new Emoji("1F930-1F3FF", "PREGNANT WOMAN", "\ud83e\udd30\ud83c\udfff", "pregnant_woman", new string[1] { "pregnant_woman" }, null, null, "Smileys & People", 39, 9, SkinVariationType.Dark, "pregnant woman", new string[2] { "pregnant", "woman" });

	public static readonly Emoji BreastFeeding = new Emoji("1F931", "BREAST-FEEDING", "\ud83e\udd31", "breast-feeding", new string[1] { "breast-feeding" }, null, null, "Smileys & People", 39, 10, SkinVariationType.None, "breast-feeding", new string[4] { "baby", "breast", "breast-feeding", "nursing" });

	public static readonly Emoji BreastFeeding_Light = new Emoji("1F931-1F3FB", "BREAST-FEEDING", "\ud83e\udd31\ud83c\udffb", "breast-feeding", new string[1] { "breast-feeding" }, null, null, "Smileys & People", 39, 11, SkinVariationType.Light, "breast-feeding", new string[4] { "baby", "breast", "breast-feeding", "nursing" });

	public static readonly Emoji BreastFeeding_MediumLight = new Emoji("1F931-1F3FC", "BREAST-FEEDING", "\ud83e\udd31\ud83c\udffc", "breast-feeding", new string[1] { "breast-feeding" }, null, null, "Smileys & People", 39, 12, SkinVariationType.MediumLight, "breast-feeding", new string[4] { "baby", "breast", "breast-feeding", "nursing" });

	public static readonly Emoji BreastFeeding_Medium = new Emoji("1F931-1F3FD", "BREAST-FEEDING", "\ud83e\udd31\ud83c\udffd", "breast-feeding", new string[1] { "breast-feeding" }, null, null, "Smileys & People", 39, 13, SkinVariationType.Medium, "breast-feeding", new string[4] { "baby", "breast", "breast-feeding", "nursing" });

	public static readonly Emoji BreastFeeding_MediumDark = new Emoji("1F931-1F3FE", "BREAST-FEEDING", "\ud83e\udd31\ud83c\udffe", "breast-feeding", new string[1] { "breast-feeding" }, null, null, "Smileys & People", 39, 14, SkinVariationType.MediumDark, "breast-feeding", new string[4] { "baby", "breast", "breast-feeding", "nursing" });

	public static readonly Emoji BreastFeeding_Dark = new Emoji("1F931-1F3FF", "BREAST-FEEDING", "\ud83e\udd31\ud83c\udfff", "breast-feeding", new string[1] { "breast-feeding" }, null, null, "Smileys & People", 39, 15, SkinVariationType.Dark, "breast-feeding", new string[4] { "baby", "breast", "breast-feeding", "nursing" });

	public static readonly Emoji Angel = new Emoji("1F47C", "BABY ANGEL", "\ud83d\udc7c", "angel", new string[1] { "angel" }, null, null, "Smileys & People", 22, 43, SkinVariationType.None, "baby angel", new string[5] { "angel", "baby", "face", "fairy tale", "fantasy" });

	public static readonly Emoji Angel_Light = new Emoji("1F47C-1F3FB", "BABY ANGEL", "\ud83d\udc7c\ud83c\udffb", "angel", new string[1] { "angel" }, null, null, "Smileys & People", 22, 44, SkinVariationType.Light, "baby angel", new string[5] { "angel", "baby", "face", "fairy tale", "fantasy" });

	public static readonly Emoji Angel_MediumLight = new Emoji("1F47C-1F3FC", "BABY ANGEL", "\ud83d\udc7c\ud83c\udffc", "angel", new string[1] { "angel" }, null, null, "Smileys & People", 22, 45, SkinVariationType.MediumLight, "baby angel", new string[5] { "angel", "baby", "face", "fairy tale", "fantasy" });

	public static readonly Emoji Angel_Medium = new Emoji("1F47C-1F3FD", "BABY ANGEL", "\ud83d\udc7c\ud83c\udffd", "angel", new string[1] { "angel" }, null, null, "Smileys & People", 22, 46, SkinVariationType.Medium, "baby angel", new string[5] { "angel", "baby", "face", "fairy tale", "fantasy" });

	public static readonly Emoji Angel_MediumDark = new Emoji("1F47C-1F3FE", "BABY ANGEL", "\ud83d\udc7c\ud83c\udffe", "angel", new string[1] { "angel" }, null, null, "Smileys & People", 22, 47, SkinVariationType.MediumDark, "baby angel", new string[5] { "angel", "baby", "face", "fairy tale", "fantasy" });

	public static readonly Emoji Angel_Dark = new Emoji("1F47C-1F3FF", "BABY ANGEL", "\ud83d\udc7c\ud83c\udfff", "angel", new string[1] { "angel" }, null, null, "Smileys & People", 22, 48, SkinVariationType.Dark, "baby angel", new string[5] { "angel", "baby", "face", "fairy tale", "fantasy" });

	public static readonly Emoji Santa = new Emoji("1F385", "FATHER CHRISTMAS", "\ud83c\udf85", "santa", new string[1] { "santa" }, null, null, "Smileys & People", 8, 19, SkinVariationType.None, "Santa Claus", new string[6] { "celebration", "Christmas", "claus", "father", "santa", "Santa Claus" });

	public static readonly Emoji Santa_Light = new Emoji("1F385-1F3FB", "FATHER CHRISTMAS", "\ud83c\udf85\ud83c\udffb", "santa", new string[1] { "santa" }, null, null, "Smileys & People", 8, 20, SkinVariationType.Light, "Santa Claus", new string[6] { "celebration", "Christmas", "claus", "father", "santa", "Santa Claus" });

	public static readonly Emoji Santa_MediumLight = new Emoji("1F385-1F3FC", "FATHER CHRISTMAS", "\ud83c\udf85\ud83c\udffc", "santa", new string[1] { "santa" }, null, null, "Smileys & People", 8, 21, SkinVariationType.MediumLight, "Santa Claus", new string[6] { "celebration", "Christmas", "claus", "father", "santa", "Santa Claus" });

	public static readonly Emoji Santa_Medium = new Emoji("1F385-1F3FD", "FATHER CHRISTMAS", "\ud83c\udf85\ud83c\udffd", "santa", new string[1] { "santa" }, null, null, "Smileys & People", 8, 22, SkinVariationType.Medium, "Santa Claus", new string[6] { "celebration", "Christmas", "claus", "father", "santa", "Santa Claus" });

	public static readonly Emoji Santa_MediumDark = new Emoji("1F385-1F3FE", "FATHER CHRISTMAS", "\ud83c\udf85\ud83c\udffe", "santa", new string[1] { "santa" }, null, null, "Smileys & People", 8, 23, SkinVariationType.MediumDark, "Santa Claus", new string[6] { "celebration", "Christmas", "claus", "father", "santa", "Santa Claus" });

	public static readonly Emoji Santa_Dark = new Emoji("1F385-1F3FF", "FATHER CHRISTMAS", "\ud83c\udf85\ud83c\udfff", "santa", new string[1] { "santa" }, null, null, "Smileys & People", 8, 24, SkinVariationType.Dark, "Santa Claus", new string[6] { "celebration", "Christmas", "claus", "father", "santa", "Santa Claus" });

	public static readonly Emoji MrsClaus = new Emoji("1F936", "MOTHER CHRISTMAS", "\ud83e\udd36", "mrs_claus", new string[2] { "mrs_claus", "mother_christmas" }, null, null, "Smileys & People", 39, 40, SkinVariationType.None, "Mrs. Claus", new string[6] { "celebration", "Christmas", "claus", "mother", "Mrs.", "Mrs. Claus" });

	public static readonly Emoji MrsClaus_Light = new Emoji("1F936-1F3FB", "MOTHER CHRISTMAS", "\ud83e\udd36\ud83c\udffb", "mrs_claus", new string[2] { "mrs_claus", "mother_christmas" }, null, null, "Smileys & People", 39, 41, SkinVariationType.Light, "Mrs. Claus", new string[6] { "celebration", "Christmas", "claus", "mother", "Mrs.", "Mrs. Claus" });

	public static readonly Emoji MrsClaus_MediumLight = new Emoji("1F936-1F3FC", "MOTHER CHRISTMAS", "\ud83e\udd36\ud83c\udffc", "mrs_claus", new string[2] { "mrs_claus", "mother_christmas" }, null, null, "Smileys & People", 39, 42, SkinVariationType.MediumLight, "Mrs. Claus", new string[6] { "celebration", "Christmas", "claus", "mother", "Mrs.", "Mrs. Claus" });

	public static readonly Emoji MrsClaus_Medium = new Emoji("1F936-1F3FD", "MOTHER CHRISTMAS", "\ud83e\udd36\ud83c\udffd", "mrs_claus", new string[2] { "mrs_claus", "mother_christmas" }, null, null, "Smileys & People", 39, 43, SkinVariationType.Medium, "Mrs. Claus", new string[6] { "celebration", "Christmas", "claus", "mother", "Mrs.", "Mrs. Claus" });

	public static readonly Emoji MrsClaus_MediumDark = new Emoji("1F936-1F3FE", "MOTHER CHRISTMAS", "\ud83e\udd36\ud83c\udffe", "mrs_claus", new string[2] { "mrs_claus", "mother_christmas" }, null, null, "Smileys & People", 39, 44, SkinVariationType.MediumDark, "Mrs. Claus", new string[6] { "celebration", "Christmas", "claus", "mother", "Mrs.", "Mrs. Claus" });

	public static readonly Emoji MrsClaus_Dark = new Emoji("1F936-1F3FF", "MOTHER CHRISTMAS", "\ud83e\udd36\ud83c\udfff", "mrs_claus", new string[2] { "mrs_claus", "mother_christmas" }, null, null, "Smileys & People", 39, 45, SkinVariationType.Dark, "Mrs. Claus", new string[6] { "celebration", "Christmas", "claus", "mother", "Mrs.", "Mrs. Claus" });

	public static readonly Emoji FemaleMage = new Emoji("1F9D9-200D-2640-FE0F", null, "\ud83e\uddd9\u200d♀\ufe0f", "female_mage", new string[1] { "female_mage" }, null, null, "Smileys & People", 44, 30, SkinVariationType.None, "woman mage", new string[6] { "mage", "sorcerer", "sorceress", "witch", "wizard", "woman mage" });

	public static readonly Emoji FemaleMage_Light = new Emoji("1F9D9-1F3FB-200D-2640-FE0F", null, "\ud83e\uddd9\ud83c\udffb\u200d♀\ufe0f", "female_mage", new string[1] { "female_mage" }, null, null, "Smileys & People", 44, 31, SkinVariationType.Light, "woman mage", new string[6] { "mage", "sorcerer", "sorceress", "witch", "wizard", "woman mage" });

	public static readonly Emoji FemaleMage_MediumLight = new Emoji("1F9D9-1F3FC-200D-2640-FE0F", null, "\ud83e\uddd9\ud83c\udffc\u200d♀\ufe0f", "female_mage", new string[1] { "female_mage" }, null, null, "Smileys & People", 44, 32, SkinVariationType.MediumLight, "woman mage", new string[6] { "mage", "sorcerer", "sorceress", "witch", "wizard", "woman mage" });

	public static readonly Emoji FemaleMage_Medium = new Emoji("1F9D9-1F3FD-200D-2640-FE0F", null, "\ud83e\uddd9\ud83c\udffd\u200d♀\ufe0f", "female_mage", new string[1] { "female_mage" }, null, null, "Smileys & People", 44, 33, SkinVariationType.Medium, "woman mage", new string[6] { "mage", "sorcerer", "sorceress", "witch", "wizard", "woman mage" });

	public static readonly Emoji FemaleMage_MediumDark = new Emoji("1F9D9-1F3FE-200D-2640-FE0F", null, "\ud83e\uddd9\ud83c\udffe\u200d♀\ufe0f", "female_mage", new string[1] { "female_mage" }, null, null, "Smileys & People", 44, 34, SkinVariationType.MediumDark, "woman mage", new string[6] { "mage", "sorcerer", "sorceress", "witch", "wizard", "woman mage" });

	public static readonly Emoji FemaleMage_Dark = new Emoji("1F9D9-1F3FF-200D-2640-FE0F", null, "\ud83e\uddd9\ud83c\udfff\u200d♀\ufe0f", "female_mage", new string[1] { "female_mage" }, null, null, "Smileys & People", 44, 35, SkinVariationType.Dark, "woman mage", new string[6] { "mage", "sorcerer", "sorceress", "witch", "wizard", "woman mage" });

	public static readonly Emoji MaleMage = new Emoji("1F9D9-200D-2642-FE0F", null, "\ud83e\uddd9\u200d♂\ufe0f", "male_mage", new string[1] { "male_mage" }, null, null, "Smileys & People", 44, 36, SkinVariationType.None, "man mage", new string[6] { "mage", "sorcerer", "sorceress", "witch", "wizard", "man mage" });

	public static readonly Emoji MaleMage_Light = new Emoji("1F9D9-1F3FB-200D-2642-FE0F", null, "\ud83e\uddd9\ud83c\udffb\u200d♂\ufe0f", "male_mage", new string[1] { "male_mage" }, null, null, "Smileys & People", 44, 37, SkinVariationType.Light, "man mage", new string[6] { "mage", "sorcerer", "sorceress", "witch", "wizard", "man mage" });

	public static readonly Emoji MaleMage_MediumLight = new Emoji("1F9D9-1F3FC-200D-2642-FE0F", null, "\ud83e\uddd9\ud83c\udffc\u200d♂\ufe0f", "male_mage", new string[1] { "male_mage" }, null, null, "Smileys & People", 44, 38, SkinVariationType.MediumLight, "man mage", new string[6] { "mage", "sorcerer", "sorceress", "witch", "wizard", "man mage" });

	public static readonly Emoji MaleMage_Medium = new Emoji("1F9D9-1F3FD-200D-2642-FE0F", null, "\ud83e\uddd9\ud83c\udffd\u200d♂\ufe0f", "male_mage", new string[1] { "male_mage" }, null, null, "Smileys & People", 44, 39, SkinVariationType.Medium, "man mage", new string[6] { "mage", "sorcerer", "sorceress", "witch", "wizard", "man mage" });

	public static readonly Emoji MaleMage_MediumDark = new Emoji("1F9D9-1F3FE-200D-2642-FE0F", null, "\ud83e\uddd9\ud83c\udffe\u200d♂\ufe0f", "male_mage", new string[1] { "male_mage" }, null, null, "Smileys & People", 44, 40, SkinVariationType.MediumDark, "man mage", new string[6] { "mage", "sorcerer", "sorceress", "witch", "wizard", "man mage" });

	public static readonly Emoji MaleMage_Dark = new Emoji("1F9D9-1F3FF-200D-2642-FE0F", null, "\ud83e\uddd9\ud83c\udfff\u200d♂\ufe0f", "male_mage", new string[1] { "male_mage" }, null, null, "Smileys & People", 44, 41, SkinVariationType.Dark, "man mage", new string[6] { "mage", "sorcerer", "sorceress", "witch", "wizard", "man mage" });

	public static readonly Emoji FemaleFairy = new Emoji("1F9DA-200D-2640-FE0F", null, "\ud83e\uddda\u200d♀\ufe0f", "female_fairy", new string[1] { "female_fairy" }, null, null, "Smileys & People", 44, 48, SkinVariationType.None, "woman fairy", new string[5] { "fairy", "Oberon", "Puck", "Titania", "woman fairy" });

	public static readonly Emoji FemaleFairy_Light = new Emoji("1F9DA-1F3FB-200D-2640-FE0F", null, "\ud83e\uddda\ud83c\udffb\u200d♀\ufe0f", "female_fairy", new string[1] { "female_fairy" }, null, null, "Smileys & People", 44, 49, SkinVariationType.Light, "woman fairy", new string[5] { "fairy", "Oberon", "Puck", "Titania", "woman fairy" });

	public static readonly Emoji FemaleFairy_MediumLight = new Emoji("1F9DA-1F3FC-200D-2640-FE0F", null, "\ud83e\uddda\ud83c\udffc\u200d♀\ufe0f", "female_fairy", new string[1] { "female_fairy" }, null, null, "Smileys & People", 44, 50, SkinVariationType.MediumLight, "woman fairy", new string[5] { "fairy", "Oberon", "Puck", "Titania", "woman fairy" });

	public static readonly Emoji FemaleFairy_Medium = new Emoji("1F9DA-1F3FD-200D-2640-FE0F", null, "\ud83e\uddda\ud83c\udffd\u200d♀\ufe0f", "female_fairy", new string[1] { "female_fairy" }, null, null, "Smileys & People", 44, 51, SkinVariationType.Medium, "woman fairy", new string[5] { "fairy", "Oberon", "Puck", "Titania", "woman fairy" });

	public static readonly Emoji FemaleFairy_MediumDark = new Emoji("1F9DA-1F3FE-200D-2640-FE0F", null, "\ud83e\uddda\ud83c\udffe\u200d♀\ufe0f", "female_fairy", new string[1] { "female_fairy" }, null, null, "Smileys & People", 45, 0, SkinVariationType.MediumDark, "woman fairy", new string[5] { "fairy", "Oberon", "Puck", "Titania", "woman fairy" });

	public static readonly Emoji FemaleFairy_Dark = new Emoji("1F9DA-1F3FF-200D-2640-FE0F", null, "\ud83e\uddda\ud83c\udfff\u200d♀\ufe0f", "female_fairy", new string[1] { "female_fairy" }, null, null, "Smileys & People", 45, 1, SkinVariationType.Dark, "woman fairy", new string[5] { "fairy", "Oberon", "Puck", "Titania", "woman fairy" });

	public static readonly Emoji MaleFairy = new Emoji("1F9DA-200D-2642-FE0F", null, "\ud83e\uddda\u200d♂\ufe0f", "male_fairy", new string[1] { "male_fairy" }, null, null, "Smileys & People", 45, 2, SkinVariationType.None, "man fairy", new string[5] { "fairy", "Oberon", "Puck", "Titania", "man fairy" });

	public static readonly Emoji MaleFairy_Light = new Emoji("1F9DA-1F3FB-200D-2642-FE0F", null, "\ud83e\uddda\ud83c\udffb\u200d♂\ufe0f", "male_fairy", new string[1] { "male_fairy" }, null, null, "Smileys & People", 45, 3, SkinVariationType.Light, "man fairy", new string[5] { "fairy", "Oberon", "Puck", "Titania", "man fairy" });

	public static readonly Emoji MaleFairy_MediumLight = new Emoji("1F9DA-1F3FC-200D-2642-FE0F", null, "\ud83e\uddda\ud83c\udffc\u200d♂\ufe0f", "male_fairy", new string[1] { "male_fairy" }, null, null, "Smileys & People", 45, 4, SkinVariationType.MediumLight, "man fairy", new string[5] { "fairy", "Oberon", "Puck", "Titania", "man fairy" });

	public static readonly Emoji MaleFairy_Medium = new Emoji("1F9DA-1F3FD-200D-2642-FE0F", null, "\ud83e\uddda\ud83c\udffd\u200d♂\ufe0f", "male_fairy", new string[1] { "male_fairy" }, null, null, "Smileys & People", 45, 5, SkinVariationType.Medium, "man fairy", new string[5] { "fairy", "Oberon", "Puck", "Titania", "man fairy" });

	public static readonly Emoji MaleFairy_MediumDark = new Emoji("1F9DA-1F3FE-200D-2642-FE0F", null, "\ud83e\uddda\ud83c\udffe\u200d♂\ufe0f", "male_fairy", new string[1] { "male_fairy" }, null, null, "Smileys & People", 45, 6, SkinVariationType.MediumDark, "man fairy", new string[5] { "fairy", "Oberon", "Puck", "Titania", "man fairy" });

	public static readonly Emoji MaleFairy_Dark = new Emoji("1F9DA-1F3FF-200D-2642-FE0F", null, "\ud83e\uddda\ud83c\udfff\u200d♂\ufe0f", "male_fairy", new string[1] { "male_fairy" }, null, null, "Smileys & People", 45, 7, SkinVariationType.Dark, "man fairy", new string[5] { "fairy", "Oberon", "Puck", "Titania", "man fairy" });

	public static readonly Emoji FemaleVampire = new Emoji("1F9DB-200D-2640-FE0F", null, "\ud83e\udddb\u200d♀\ufe0f", "female_vampire", new string[1] { "female_vampire" }, null, null, "Smileys & People", 45, 14, SkinVariationType.None, "woman vampire", new string[4] { "Dracula", "undead", "vampire", "woman vampire" });

	public static readonly Emoji FemaleVampire_Light = new Emoji("1F9DB-1F3FB-200D-2640-FE0F", null, "\ud83e\udddb\ud83c\udffb\u200d♀\ufe0f", "female_vampire", new string[1] { "female_vampire" }, null, null, "Smileys & People", 45, 15, SkinVariationType.Light, "woman vampire", new string[4] { "Dracula", "undead", "vampire", "woman vampire" });

	public static readonly Emoji FemaleVampire_MediumLight = new Emoji("1F9DB-1F3FC-200D-2640-FE0F", null, "\ud83e\udddb\ud83c\udffc\u200d♀\ufe0f", "female_vampire", new string[1] { "female_vampire" }, null, null, "Smileys & People", 45, 16, SkinVariationType.MediumLight, "woman vampire", new string[4] { "Dracula", "undead", "vampire", "woman vampire" });

	public static readonly Emoji FemaleVampire_Medium = new Emoji("1F9DB-1F3FD-200D-2640-FE0F", null, "\ud83e\udddb\ud83c\udffd\u200d♀\ufe0f", "female_vampire", new string[1] { "female_vampire" }, null, null, "Smileys & People", 45, 17, SkinVariationType.Medium, "woman vampire", new string[4] { "Dracula", "undead", "vampire", "woman vampire" });

	public static readonly Emoji FemaleVampire_MediumDark = new Emoji("1F9DB-1F3FE-200D-2640-FE0F", null, "\ud83e\udddb\ud83c\udffe\u200d♀\ufe0f", "female_vampire", new string[1] { "female_vampire" }, null, null, "Smileys & People", 45, 18, SkinVariationType.MediumDark, "woman vampire", new string[4] { "Dracula", "undead", "vampire", "woman vampire" });

	public static readonly Emoji FemaleVampire_Dark = new Emoji("1F9DB-1F3FF-200D-2640-FE0F", null, "\ud83e\udddb\ud83c\udfff\u200d♀\ufe0f", "female_vampire", new string[1] { "female_vampire" }, null, null, "Smileys & People", 45, 19, SkinVariationType.Dark, "woman vampire", new string[4] { "Dracula", "undead", "vampire", "woman vampire" });

	public static readonly Emoji MaleVampire = new Emoji("1F9DB-200D-2642-FE0F", null, "\ud83e\udddb\u200d♂\ufe0f", "male_vampire", new string[1] { "male_vampire" }, null, null, "Smileys & People", 45, 20, SkinVariationType.None, "man vampire", new string[4] { "Dracula", "undead", "vampire", "man vampire" });

	public static readonly Emoji MaleVampire_Light = new Emoji("1F9DB-1F3FB-200D-2642-FE0F", null, "\ud83e\udddb\ud83c\udffb\u200d♂\ufe0f", "male_vampire", new string[1] { "male_vampire" }, null, null, "Smileys & People", 45, 21, SkinVariationType.Light, "man vampire", new string[4] { "Dracula", "undead", "vampire", "man vampire" });

	public static readonly Emoji MaleVampire_MediumLight = new Emoji("1F9DB-1F3FC-200D-2642-FE0F", null, "\ud83e\udddb\ud83c\udffc\u200d♂\ufe0f", "male_vampire", new string[1] { "male_vampire" }, null, null, "Smileys & People", 45, 22, SkinVariationType.MediumLight, "man vampire", new string[4] { "Dracula", "undead", "vampire", "man vampire" });

	public static readonly Emoji MaleVampire_Medium = new Emoji("1F9DB-1F3FD-200D-2642-FE0F", null, "\ud83e\udddb\ud83c\udffd\u200d♂\ufe0f", "male_vampire", new string[1] { "male_vampire" }, null, null, "Smileys & People", 45, 23, SkinVariationType.Medium, "man vampire", new string[4] { "Dracula", "undead", "vampire", "man vampire" });

	public static readonly Emoji MaleVampire_MediumDark = new Emoji("1F9DB-1F3FE-200D-2642-FE0F", null, "\ud83e\udddb\ud83c\udffe\u200d♂\ufe0f", "male_vampire", new string[1] { "male_vampire" }, null, null, "Smileys & People", 45, 24, SkinVariationType.MediumDark, "man vampire", new string[4] { "Dracula", "undead", "vampire", "man vampire" });

	public static readonly Emoji MaleVampire_Dark = new Emoji("1F9DB-1F3FF-200D-2642-FE0F", null, "\ud83e\udddb\ud83c\udfff\u200d♂\ufe0f", "male_vampire", new string[1] { "male_vampire" }, null, null, "Smileys & People", 45, 25, SkinVariationType.Dark, "man vampire", new string[4] { "Dracula", "undead", "vampire", "man vampire" });

	public static readonly Emoji Mermaid = new Emoji("1F9DC-200D-2640-FE0F", null, "\ud83e\udddc\u200d♀\ufe0f", "mermaid", new string[1] { "mermaid" }, null, null, "Smileys & People", 45, 32, SkinVariationType.None, "mermaid", new string[4] { "mermaid", "merman", "merperson", "merwoman" });

	public static readonly Emoji Mermaid_Light = new Emoji("1F9DC-1F3FB-200D-2640-FE0F", null, "\ud83e\udddc\ud83c\udffb\u200d♀\ufe0f", "mermaid", new string[1] { "mermaid" }, null, null, "Smileys & People", 45, 33, SkinVariationType.Light, "mermaid", new string[4] { "mermaid", "merman", "merperson", "merwoman" });

	public static readonly Emoji Mermaid_MediumLight = new Emoji("1F9DC-1F3FC-200D-2640-FE0F", null, "\ud83e\udddc\ud83c\udffc\u200d♀\ufe0f", "mermaid", new string[1] { "mermaid" }, null, null, "Smileys & People", 45, 34, SkinVariationType.MediumLight, "mermaid", new string[4] { "mermaid", "merman", "merperson", "merwoman" });

	public static readonly Emoji Mermaid_Medium = new Emoji("1F9DC-1F3FD-200D-2640-FE0F", null, "\ud83e\udddc\ud83c\udffd\u200d♀\ufe0f", "mermaid", new string[1] { "mermaid" }, null, null, "Smileys & People", 45, 35, SkinVariationType.Medium, "mermaid", new string[4] { "mermaid", "merman", "merperson", "merwoman" });

	public static readonly Emoji Mermaid_MediumDark = new Emoji("1F9DC-1F3FE-200D-2640-FE0F", null, "\ud83e\udddc\ud83c\udffe\u200d♀\ufe0f", "mermaid", new string[1] { "mermaid" }, null, null, "Smileys & People", 45, 36, SkinVariationType.MediumDark, "mermaid", new string[4] { "mermaid", "merman", "merperson", "merwoman" });

	public static readonly Emoji Mermaid_Dark = new Emoji("1F9DC-1F3FF-200D-2640-FE0F", null, "\ud83e\udddc\ud83c\udfff\u200d♀\ufe0f", "mermaid", new string[1] { "mermaid" }, null, null, "Smileys & People", 45, 37, SkinVariationType.Dark, "mermaid", new string[4] { "mermaid", "merman", "merperson", "merwoman" });

	public static readonly Emoji Merman = new Emoji("1F9DC-200D-2642-FE0F", null, "\ud83e\udddc\u200d♂\ufe0f", "merman", new string[1] { "merman" }, null, null, "Smileys & People", 45, 38, SkinVariationType.None, "merman", new string[5] { "mermaid", "merman", "merperson", "merwoman", "Triton" });

	public static readonly Emoji Merman_Light = new Emoji("1F9DC-1F3FB-200D-2642-FE0F", null, "\ud83e\udddc\ud83c\udffb\u200d♂\ufe0f", "merman", new string[1] { "merman" }, null, null, "Smileys & People", 45, 39, SkinVariationType.Light, "merman", new string[5] { "mermaid", "merman", "merperson", "merwoman", "Triton" });

	public static readonly Emoji Merman_MediumLight = new Emoji("1F9DC-1F3FC-200D-2642-FE0F", null, "\ud83e\udddc\ud83c\udffc\u200d♂\ufe0f", "merman", new string[1] { "merman" }, null, null, "Smileys & People", 45, 40, SkinVariationType.MediumLight, "merman", new string[5] { "mermaid", "merman", "merperson", "merwoman", "Triton" });

	public static readonly Emoji Merman_Medium = new Emoji("1F9DC-1F3FD-200D-2642-FE0F", null, "\ud83e\udddc\ud83c\udffd\u200d♂\ufe0f", "merman", new string[1] { "merman" }, null, null, "Smileys & People", 45, 41, SkinVariationType.Medium, "merman", new string[5] { "mermaid", "merman", "merperson", "merwoman", "Triton" });

	public static readonly Emoji Merman_MediumDark = new Emoji("1F9DC-1F3FE-200D-2642-FE0F", null, "\ud83e\udddc\ud83c\udffe\u200d♂\ufe0f", "merman", new string[1] { "merman" }, null, null, "Smileys & People", 45, 42, SkinVariationType.MediumDark, "merman", new string[5] { "mermaid", "merman", "merperson", "merwoman", "Triton" });

	public static readonly Emoji Merman_Dark = new Emoji("1F9DC-1F3FF-200D-2642-FE0F", null, "\ud83e\udddc\ud83c\udfff\u200d♂\ufe0f", "merman", new string[1] { "merman" }, null, null, "Smileys & People", 45, 43, SkinVariationType.Dark, "merman", new string[5] { "mermaid", "merman", "merperson", "merwoman", "Triton" });

	public static readonly Emoji FemaleElf = new Emoji("1F9DD-200D-2640-FE0F", null, "\ud83e\udddd\u200d♀\ufe0f", "female_elf", new string[1] { "female_elf" }, null, null, "Smileys & People", 45, 50, SkinVariationType.None, "woman elf", new string[3] { "elf", "magical", "woman elf" });

	public static readonly Emoji FemaleElf_Light = new Emoji("1F9DD-1F3FB-200D-2640-FE0F", null, "\ud83e\udddd\ud83c\udffb\u200d♀\ufe0f", "female_elf", new string[1] { "female_elf" }, null, null, "Smileys & People", 45, 51, SkinVariationType.Light, "woman elf", new string[3] { "elf", "magical", "woman elf" });

	public static readonly Emoji FemaleElf_MediumLight = new Emoji("1F9DD-1F3FC-200D-2640-FE0F", null, "\ud83e\udddd\ud83c\udffc\u200d♀\ufe0f", "female_elf", new string[1] { "female_elf" }, null, null, "Smileys & People", 46, 0, SkinVariationType.MediumLight, "woman elf", new string[3] { "elf", "magical", "woman elf" });

	public static readonly Emoji FemaleElf_Medium = new Emoji("1F9DD-1F3FD-200D-2640-FE0F", null, "\ud83e\udddd\ud83c\udffd\u200d♀\ufe0f", "female_elf", new string[1] { "female_elf" }, null, null, "Smileys & People", 46, 1, SkinVariationType.Medium, "woman elf", new string[3] { "elf", "magical", "woman elf" });

	public static readonly Emoji FemaleElf_MediumDark = new Emoji("1F9DD-1F3FE-200D-2640-FE0F", null, "\ud83e\udddd\ud83c\udffe\u200d♀\ufe0f", "female_elf", new string[1] { "female_elf" }, null, null, "Smileys & People", 46, 2, SkinVariationType.MediumDark, "woman elf", new string[3] { "elf", "magical", "woman elf" });

	public static readonly Emoji FemaleElf_Dark = new Emoji("1F9DD-1F3FF-200D-2640-FE0F", null, "\ud83e\udddd\ud83c\udfff\u200d♀\ufe0f", "female_elf", new string[1] { "female_elf" }, null, null, "Smileys & People", 46, 3, SkinVariationType.Dark, "woman elf", new string[3] { "elf", "magical", "woman elf" });

	public static readonly Emoji MaleElf = new Emoji("1F9DD-200D-2642-FE0F", null, "\ud83e\udddd\u200d♂\ufe0f", "male_elf", new string[1] { "male_elf" }, null, null, "Smileys & People", 46, 4, SkinVariationType.None, "man elf", new string[3] { "elf", "magical", "man elf" });

	public static readonly Emoji MaleElf_Light = new Emoji("1F9DD-1F3FB-200D-2642-FE0F", null, "\ud83e\udddd\ud83c\udffb\u200d♂\ufe0f", "male_elf", new string[1] { "male_elf" }, null, null, "Smileys & People", 46, 5, SkinVariationType.Light, "man elf", new string[3] { "elf", "magical", "man elf" });

	public static readonly Emoji MaleElf_MediumLight = new Emoji("1F9DD-1F3FC-200D-2642-FE0F", null, "\ud83e\udddd\ud83c\udffc\u200d♂\ufe0f", "male_elf", new string[1] { "male_elf" }, null, null, "Smileys & People", 46, 6, SkinVariationType.MediumLight, "man elf", new string[3] { "elf", "magical", "man elf" });

	public static readonly Emoji MaleElf_Medium = new Emoji("1F9DD-1F3FD-200D-2642-FE0F", null, "\ud83e\udddd\ud83c\udffd\u200d♂\ufe0f", "male_elf", new string[1] { "male_elf" }, null, null, "Smileys & People", 46, 7, SkinVariationType.Medium, "man elf", new string[3] { "elf", "magical", "man elf" });

	public static readonly Emoji MaleElf_MediumDark = new Emoji("1F9DD-1F3FE-200D-2642-FE0F", null, "\ud83e\udddd\ud83c\udffe\u200d♂\ufe0f", "male_elf", new string[1] { "male_elf" }, null, null, "Smileys & People", 46, 8, SkinVariationType.MediumDark, "man elf", new string[3] { "elf", "magical", "man elf" });

	public static readonly Emoji MaleElf_Dark = new Emoji("1F9DD-1F3FF-200D-2642-FE0F", null, "\ud83e\udddd\ud83c\udfff\u200d♂\ufe0f", "male_elf", new string[1] { "male_elf" }, null, null, "Smileys & People", 46, 9, SkinVariationType.Dark, "man elf", new string[3] { "elf", "magical", "man elf" });

	public static readonly Emoji FemaleGenie = new Emoji("1F9DE-200D-2640-FE0F", null, "\ud83e\uddde\u200d♀\ufe0f", "female_genie", new string[1] { "female_genie" }, null, null, "Smileys & People", 46, 16, SkinVariationType.None, "woman genie", new string[3] { "djinn", "genie", "woman genie" });

	public static readonly Emoji MaleGenie = new Emoji("1F9DE-200D-2642-FE0F", null, "\ud83e\uddde\u200d♂\ufe0f", "male_genie", new string[1] { "male_genie" }, null, null, "Smileys & People", 46, 17, SkinVariationType.None, "man genie", new string[3] { "djinn", "genie", "man genie" });

	public static readonly Emoji FemaleZombie = new Emoji("1F9DF-200D-2640-FE0F", null, "\ud83e\udddf\u200d♀\ufe0f", "female_zombie", new string[1] { "female_zombie" }, null, null, "Smileys & People", 46, 19, SkinVariationType.None, "woman zombie", new string[4] { "undead", "walking dead", "zombie", "woman zombie" });

	public static readonly Emoji MaleZombie = new Emoji("1F9DF-200D-2642-FE0F", null, "\ud83e\udddf\u200d♂\ufe0f", "male_zombie", new string[1] { "male_zombie" }, null, null, "Smileys & People", 46, 20, SkinVariationType.None, "man zombie", new string[4] { "undead", "walking dead", "zombie", "man zombie" });

	public static readonly Emoji ManFrowning = new Emoji("1F64D-200D-2642-FE0F", null, "\ud83d\ude4d\u200d♂\ufe0f", "man-frowning", new string[1] { "man-frowning" }, null, null, "Smileys & People", 33, 24, SkinVariationType.None, "man frowning", new string[5] { "frown", "gesture", "person frowning", "frowning", "man" });

	public static readonly Emoji ManFrowning_Light = new Emoji("1F64D-1F3FB-200D-2642-FE0F", null, "\ud83d\ude4d\ud83c\udffb\u200d♂\ufe0f", "man-frowning", new string[1] { "man-frowning" }, null, null, "Smileys & People", 33, 25, SkinVariationType.Light, "man frowning", new string[5] { "frown", "gesture", "person frowning", "frowning", "man" });

	public static readonly Emoji ManFrowning_MediumLight = new Emoji("1F64D-1F3FC-200D-2642-FE0F", null, "\ud83d\ude4d\ud83c\udffc\u200d♂\ufe0f", "man-frowning", new string[1] { "man-frowning" }, null, null, "Smileys & People", 33, 26, SkinVariationType.MediumLight, "man frowning", new string[5] { "frown", "gesture", "person frowning", "frowning", "man" });

	public static readonly Emoji ManFrowning_Medium = new Emoji("1F64D-1F3FD-200D-2642-FE0F", null, "\ud83d\ude4d\ud83c\udffd\u200d♂\ufe0f", "man-frowning", new string[1] { "man-frowning" }, null, null, "Smileys & People", 33, 27, SkinVariationType.Medium, "man frowning", new string[5] { "frown", "gesture", "person frowning", "frowning", "man" });

	public static readonly Emoji ManFrowning_MediumDark = new Emoji("1F64D-1F3FE-200D-2642-FE0F", null, "\ud83d\ude4d\ud83c\udffe\u200d♂\ufe0f", "man-frowning", new string[1] { "man-frowning" }, null, null, "Smileys & People", 33, 28, SkinVariationType.MediumDark, "man frowning", new string[5] { "frown", "gesture", "person frowning", "frowning", "man" });

	public static readonly Emoji ManFrowning_Dark = new Emoji("1F64D-1F3FF-200D-2642-FE0F", null, "\ud83d\ude4d\ud83c\udfff\u200d♂\ufe0f", "man-frowning", new string[1] { "man-frowning" }, null, null, "Smileys & People", 33, 29, SkinVariationType.Dark, "man frowning", new string[5] { "frown", "gesture", "person frowning", "frowning", "man" });

	public static readonly Emoji WomanFrowning = new Emoji("1F64D-200D-2640-FE0F", null, "\ud83d\ude4d\u200d♀\ufe0f", "woman-frowning", new string[1] { "woman-frowning" }, null, null, "Smileys & People", 33, 18, SkinVariationType.None, "woman frowning", new string[5] { "frown", "gesture", "person frowning", "frowning", "woman" });

	public static readonly Emoji WomanFrowning_Light = new Emoji("1F64D-1F3FB-200D-2640-FE0F", null, "\ud83d\ude4d\ud83c\udffb\u200d♀\ufe0f", "woman-frowning", new string[1] { "woman-frowning" }, null, null, "Smileys & People", 33, 19, SkinVariationType.Light, "woman frowning", new string[5] { "frown", "gesture", "person frowning", "frowning", "woman" });

	public static readonly Emoji WomanFrowning_MediumLight = new Emoji("1F64D-1F3FC-200D-2640-FE0F", null, "\ud83d\ude4d\ud83c\udffc\u200d♀\ufe0f", "woman-frowning", new string[1] { "woman-frowning" }, null, null, "Smileys & People", 33, 20, SkinVariationType.MediumLight, "woman frowning", new string[5] { "frown", "gesture", "person frowning", "frowning", "woman" });

	public static readonly Emoji WomanFrowning_Medium = new Emoji("1F64D-1F3FD-200D-2640-FE0F", null, "\ud83d\ude4d\ud83c\udffd\u200d♀\ufe0f", "woman-frowning", new string[1] { "woman-frowning" }, null, null, "Smileys & People", 33, 21, SkinVariationType.Medium, "woman frowning", new string[5] { "frown", "gesture", "person frowning", "frowning", "woman" });

	public static readonly Emoji WomanFrowning_MediumDark = new Emoji("1F64D-1F3FE-200D-2640-FE0F", null, "\ud83d\ude4d\ud83c\udffe\u200d♀\ufe0f", "woman-frowning", new string[1] { "woman-frowning" }, null, null, "Smileys & People", 33, 22, SkinVariationType.MediumDark, "woman frowning", new string[5] { "frown", "gesture", "person frowning", "frowning", "woman" });

	public static readonly Emoji WomanFrowning_Dark = new Emoji("1F64D-1F3FF-200D-2640-FE0F", null, "\ud83d\ude4d\ud83c\udfff\u200d♀\ufe0f", "woman-frowning", new string[1] { "woman-frowning" }, null, null, "Smileys & People", 33, 23, SkinVariationType.Dark, "woman frowning", new string[5] { "frown", "gesture", "person frowning", "frowning", "woman" });

	public static readonly Emoji ManPouting = new Emoji("1F64E-200D-2642-FE0F", null, "\ud83d\ude4e\u200d♂\ufe0f", "man-pouting", new string[1] { "man-pouting" }, null, null, "Smileys & People", 33, 42, SkinVariationType.None, "man pouting", new string[4] { "gesture", "person pouting", "pouting", "man" });

	public static readonly Emoji ManPouting_Light = new Emoji("1F64E-1F3FB-200D-2642-FE0F", null, "\ud83d\ude4e\ud83c\udffb\u200d♂\ufe0f", "man-pouting", new string[1] { "man-pouting" }, null, null, "Smileys & People", 33, 43, SkinVariationType.Light, "man pouting", new string[4] { "gesture", "person pouting", "pouting", "man" });

	public static readonly Emoji ManPouting_MediumLight = new Emoji("1F64E-1F3FC-200D-2642-FE0F", null, "\ud83d\ude4e\ud83c\udffc\u200d♂\ufe0f", "man-pouting", new string[1] { "man-pouting" }, null, null, "Smileys & People", 33, 44, SkinVariationType.MediumLight, "man pouting", new string[4] { "gesture", "person pouting", "pouting", "man" });

	public static readonly Emoji ManPouting_Medium = new Emoji("1F64E-1F3FD-200D-2642-FE0F", null, "\ud83d\ude4e\ud83c\udffd\u200d♂\ufe0f", "man-pouting", new string[1] { "man-pouting" }, null, null, "Smileys & People", 33, 45, SkinVariationType.Medium, "man pouting", new string[4] { "gesture", "person pouting", "pouting", "man" });

	public static readonly Emoji ManPouting_MediumDark = new Emoji("1F64E-1F3FE-200D-2642-FE0F", null, "\ud83d\ude4e\ud83c\udffe\u200d♂\ufe0f", "man-pouting", new string[1] { "man-pouting" }, null, null, "Smileys & People", 33, 46, SkinVariationType.MediumDark, "man pouting", new string[4] { "gesture", "person pouting", "pouting", "man" });

	public static readonly Emoji ManPouting_Dark = new Emoji("1F64E-1F3FF-200D-2642-FE0F", null, "\ud83d\ude4e\ud83c\udfff\u200d♂\ufe0f", "man-pouting", new string[1] { "man-pouting" }, null, null, "Smileys & People", 33, 47, SkinVariationType.Dark, "man pouting", new string[4] { "gesture", "person pouting", "pouting", "man" });

	public static readonly Emoji WomanPouting = new Emoji("1F64E-200D-2640-FE0F", null, "\ud83d\ude4e\u200d♀\ufe0f", "woman-pouting", new string[1] { "woman-pouting" }, null, null, "Smileys & People", 33, 36, SkinVariationType.None, "woman pouting", new string[4] { "gesture", "person pouting", "pouting", "woman" });

	public static readonly Emoji WomanPouting_Light = new Emoji("1F64E-1F3FB-200D-2640-FE0F", null, "\ud83d\ude4e\ud83c\udffb\u200d♀\ufe0f", "woman-pouting", new string[1] { "woman-pouting" }, null, null, "Smileys & People", 33, 37, SkinVariationType.Light, "woman pouting", new string[4] { "gesture", "person pouting", "pouting", "woman" });

	public static readonly Emoji WomanPouting_MediumLight = new Emoji("1F64E-1F3FC-200D-2640-FE0F", null, "\ud83d\ude4e\ud83c\udffc\u200d♀\ufe0f", "woman-pouting", new string[1] { "woman-pouting" }, null, null, "Smileys & People", 33, 38, SkinVariationType.MediumLight, "woman pouting", new string[4] { "gesture", "person pouting", "pouting", "woman" });

	public static readonly Emoji WomanPouting_Medium = new Emoji("1F64E-1F3FD-200D-2640-FE0F", null, "\ud83d\ude4e\ud83c\udffd\u200d♀\ufe0f", "woman-pouting", new string[1] { "woman-pouting" }, null, null, "Smileys & People", 33, 39, SkinVariationType.Medium, "woman pouting", new string[4] { "gesture", "person pouting", "pouting", "woman" });

	public static readonly Emoji WomanPouting_MediumDark = new Emoji("1F64E-1F3FE-200D-2640-FE0F", null, "\ud83d\ude4e\ud83c\udffe\u200d♀\ufe0f", "woman-pouting", new string[1] { "woman-pouting" }, null, null, "Smileys & People", 33, 40, SkinVariationType.MediumDark, "woman pouting", new string[4] { "gesture", "person pouting", "pouting", "woman" });

	public static readonly Emoji WomanPouting_Dark = new Emoji("1F64E-1F3FF-200D-2640-FE0F", null, "\ud83d\ude4e\ud83c\udfff\u200d♀\ufe0f", "woman-pouting", new string[1] { "woman-pouting" }, null, null, "Smileys & People", 33, 41, SkinVariationType.Dark, "woman pouting", new string[4] { "gesture", "person pouting", "pouting", "woman" });

	public static readonly Emoji ManGesturingNo = new Emoji("1F645-200D-2642-FE0F", null, "\ud83d\ude45\u200d♂\ufe0f", "man-gesturing-no", new string[1] { "man-gesturing-no" }, null, null, "Smileys & People", 31, 47, SkinVariationType.None, "man gesturing NO", new string[9] { "forbidden", "gesture", "hand", "no", "not", "person gesturing NO", "prohibited", "man", "man gesturing NO" });

	public static readonly Emoji ManGesturingNo_Light = new Emoji("1F645-1F3FB-200D-2642-FE0F", null, "\ud83d\ude45\ud83c\udffb\u200d♂\ufe0f", "man-gesturing-no", new string[1] { "man-gesturing-no" }, null, null, "Smileys & People", 31, 48, SkinVariationType.Light, "man gesturing NO", new string[9] { "forbidden", "gesture", "hand", "no", "not", "person gesturing NO", "prohibited", "man", "man gesturing NO" });

	public static readonly Emoji ManGesturingNo_MediumLight = new Emoji("1F645-1F3FC-200D-2642-FE0F", null, "\ud83d\ude45\ud83c\udffc\u200d♂\ufe0f", "man-gesturing-no", new string[1] { "man-gesturing-no" }, null, null, "Smileys & People", 31, 49, SkinVariationType.MediumLight, "man gesturing NO", new string[9] { "forbidden", "gesture", "hand", "no", "not", "person gesturing NO", "prohibited", "man", "man gesturing NO" });

	public static readonly Emoji ManGesturingNo_Medium = new Emoji("1F645-1F3FD-200D-2642-FE0F", null, "\ud83d\ude45\ud83c\udffd\u200d♂\ufe0f", "man-gesturing-no", new string[1] { "man-gesturing-no" }, null, null, "Smileys & People", 31, 50, SkinVariationType.Medium, "man gesturing NO", new string[9] { "forbidden", "gesture", "hand", "no", "not", "person gesturing NO", "prohibited", "man", "man gesturing NO" });

	public static readonly Emoji ManGesturingNo_MediumDark = new Emoji("1F645-1F3FE-200D-2642-FE0F", null, "\ud83d\ude45\ud83c\udffe\u200d♂\ufe0f", "man-gesturing-no", new string[1] { "man-gesturing-no" }, null, null, "Smileys & People", 31, 51, SkinVariationType.MediumDark, "man gesturing NO", new string[9] { "forbidden", "gesture", "hand", "no", "not", "person gesturing NO", "prohibited", "man", "man gesturing NO" });

	public static readonly Emoji ManGesturingNo_Dark = new Emoji("1F645-1F3FF-200D-2642-FE0F", null, "\ud83d\ude45\ud83c\udfff\u200d♂\ufe0f", "man-gesturing-no", new string[1] { "man-gesturing-no" }, null, null, "Smileys & People", 32, 0, SkinVariationType.Dark, "man gesturing NO", new string[9] { "forbidden", "gesture", "hand", "no", "not", "person gesturing NO", "prohibited", "man", "man gesturing NO" });

	public static readonly Emoji WomanGesturingNo = new Emoji("1F645-200D-2640-FE0F", null, "\ud83d\ude45\u200d♀\ufe0f", "woman-gesturing-no", new string[1] { "woman-gesturing-no" }, null, null, "Smileys & People", 31, 41, SkinVariationType.None, "woman gesturing NO", new string[9] { "forbidden", "gesture", "hand", "no", "not", "person gesturing NO", "prohibited", "woman", "woman gesturing NO" });

	public static readonly Emoji WomanGesturingNo_Light = new Emoji("1F645-1F3FB-200D-2640-FE0F", null, "\ud83d\ude45\ud83c\udffb\u200d♀\ufe0f", "woman-gesturing-no", new string[1] { "woman-gesturing-no" }, null, null, "Smileys & People", 31, 42, SkinVariationType.Light, "woman gesturing NO", new string[9] { "forbidden", "gesture", "hand", "no", "not", "person gesturing NO", "prohibited", "woman", "woman gesturing NO" });

	public static readonly Emoji WomanGesturingNo_MediumLight = new Emoji("1F645-1F3FC-200D-2640-FE0F", null, "\ud83d\ude45\ud83c\udffc\u200d♀\ufe0f", "woman-gesturing-no", new string[1] { "woman-gesturing-no" }, null, null, "Smileys & People", 31, 43, SkinVariationType.MediumLight, "woman gesturing NO", new string[9] { "forbidden", "gesture", "hand", "no", "not", "person gesturing NO", "prohibited", "woman", "woman gesturing NO" });

	public static readonly Emoji WomanGesturingNo_Medium = new Emoji("1F645-1F3FD-200D-2640-FE0F", null, "\ud83d\ude45\ud83c\udffd\u200d♀\ufe0f", "woman-gesturing-no", new string[1] { "woman-gesturing-no" }, null, null, "Smileys & People", 31, 44, SkinVariationType.Medium, "woman gesturing NO", new string[9] { "forbidden", "gesture", "hand", "no", "not", "person gesturing NO", "prohibited", "woman", "woman gesturing NO" });

	public static readonly Emoji WomanGesturingNo_MediumDark = new Emoji("1F645-1F3FE-200D-2640-FE0F", null, "\ud83d\ude45\ud83c\udffe\u200d♀\ufe0f", "woman-gesturing-no", new string[1] { "woman-gesturing-no" }, null, null, "Smileys & People", 31, 45, SkinVariationType.MediumDark, "woman gesturing NO", new string[9] { "forbidden", "gesture", "hand", "no", "not", "person gesturing NO", "prohibited", "woman", "woman gesturing NO" });

	public static readonly Emoji WomanGesturingNo_Dark = new Emoji("1F645-1F3FF-200D-2640-FE0F", null, "\ud83d\ude45\ud83c\udfff\u200d♀\ufe0f", "woman-gesturing-no", new string[1] { "woman-gesturing-no" }, null, null, "Smileys & People", 31, 46, SkinVariationType.Dark, "woman gesturing NO", new string[9] { "forbidden", "gesture", "hand", "no", "not", "person gesturing NO", "prohibited", "woman", "woman gesturing NO" });

	public static readonly Emoji ManGesturingOk = new Emoji("1F646-200D-2642-FE0F", null, "\ud83d\ude46\u200d♂\ufe0f", "man-gesturing-ok", new string[1] { "man-gesturing-ok" }, null, null, "Smileys & People", 32, 13, SkinVariationType.None, "man gesturing OK", new string[6] { "gesture", "hand", "OK", "person gesturing OK", "man", "man gesturing OK" });

	public static readonly Emoji ManGesturingOk_Light = new Emoji("1F646-1F3FB-200D-2642-FE0F", null, "\ud83d\ude46\ud83c\udffb\u200d♂\ufe0f", "man-gesturing-ok", new string[1] { "man-gesturing-ok" }, null, null, "Smileys & People", 32, 14, SkinVariationType.Light, "man gesturing OK", new string[6] { "gesture", "hand", "OK", "person gesturing OK", "man", "man gesturing OK" });

	public static readonly Emoji ManGesturingOk_MediumLight = new Emoji("1F646-1F3FC-200D-2642-FE0F", null, "\ud83d\ude46\ud83c\udffc\u200d♂\ufe0f", "man-gesturing-ok", new string[1] { "man-gesturing-ok" }, null, null, "Smileys & People", 32, 15, SkinVariationType.MediumLight, "man gesturing OK", new string[6] { "gesture", "hand", "OK", "person gesturing OK", "man", "man gesturing OK" });

	public static readonly Emoji ManGesturingOk_Medium = new Emoji("1F646-1F3FD-200D-2642-FE0F", null, "\ud83d\ude46\ud83c\udffd\u200d♂\ufe0f", "man-gesturing-ok", new string[1] { "man-gesturing-ok" }, null, null, "Smileys & People", 32, 16, SkinVariationType.Medium, "man gesturing OK", new string[6] { "gesture", "hand", "OK", "person gesturing OK", "man", "man gesturing OK" });

	public static readonly Emoji ManGesturingOk_MediumDark = new Emoji("1F646-1F3FE-200D-2642-FE0F", null, "\ud83d\ude46\ud83c\udffe\u200d♂\ufe0f", "man-gesturing-ok", new string[1] { "man-gesturing-ok" }, null, null, "Smileys & People", 32, 17, SkinVariationType.MediumDark, "man gesturing OK", new string[6] { "gesture", "hand", "OK", "person gesturing OK", "man", "man gesturing OK" });

	public static readonly Emoji ManGesturingOk_Dark = new Emoji("1F646-1F3FF-200D-2642-FE0F", null, "\ud83d\ude46\ud83c\udfff\u200d♂\ufe0f", "man-gesturing-ok", new string[1] { "man-gesturing-ok" }, null, null, "Smileys & People", 32, 18, SkinVariationType.Dark, "man gesturing OK", new string[6] { "gesture", "hand", "OK", "person gesturing OK", "man", "man gesturing OK" });

	public static readonly Emoji WomanGesturingOk = new Emoji("1F646-200D-2640-FE0F", null, "\ud83d\ude46\u200d♀\ufe0f", "woman-gesturing-ok", new string[1] { "woman-gesturing-ok" }, null, null, "Smileys & People", 32, 7, SkinVariationType.None, "woman gesturing OK", new string[6] { "gesture", "hand", "OK", "person gesturing OK", "woman", "woman gesturing OK" });

	public static readonly Emoji WomanGesturingOk_Light = new Emoji("1F646-1F3FB-200D-2640-FE0F", null, "\ud83d\ude46\ud83c\udffb\u200d♀\ufe0f", "woman-gesturing-ok", new string[1] { "woman-gesturing-ok" }, null, null, "Smileys & People", 32, 8, SkinVariationType.Light, "woman gesturing OK", new string[6] { "gesture", "hand", "OK", "person gesturing OK", "woman", "woman gesturing OK" });

	public static readonly Emoji WomanGesturingOk_MediumLight = new Emoji("1F646-1F3FC-200D-2640-FE0F", null, "\ud83d\ude46\ud83c\udffc\u200d♀\ufe0f", "woman-gesturing-ok", new string[1] { "woman-gesturing-ok" }, null, null, "Smileys & People", 32, 9, SkinVariationType.MediumLight, "woman gesturing OK", new string[6] { "gesture", "hand", "OK", "person gesturing OK", "woman", "woman gesturing OK" });

	public static readonly Emoji WomanGesturingOk_Medium = new Emoji("1F646-1F3FD-200D-2640-FE0F", null, "\ud83d\ude46\ud83c\udffd\u200d♀\ufe0f", "woman-gesturing-ok", new string[1] { "woman-gesturing-ok" }, null, null, "Smileys & People", 32, 10, SkinVariationType.Medium, "woman gesturing OK", new string[6] { "gesture", "hand", "OK", "person gesturing OK", "woman", "woman gesturing OK" });

	public static readonly Emoji WomanGesturingOk_MediumDark = new Emoji("1F646-1F3FE-200D-2640-FE0F", null, "\ud83d\ude46\ud83c\udffe\u200d♀\ufe0f", "woman-gesturing-ok", new string[1] { "woman-gesturing-ok" }, null, null, "Smileys & People", 32, 11, SkinVariationType.MediumDark, "woman gesturing OK", new string[6] { "gesture", "hand", "OK", "person gesturing OK", "woman", "woman gesturing OK" });

	public static readonly Emoji WomanGesturingOk_Dark = new Emoji("1F646-1F3FF-200D-2640-FE0F", null, "\ud83d\ude46\ud83c\udfff\u200d♀\ufe0f", "woman-gesturing-ok", new string[1] { "woman-gesturing-ok" }, null, null, "Smileys & People", 32, 12, SkinVariationType.Dark, "woman gesturing OK", new string[6] { "gesture", "hand", "OK", "person gesturing OK", "woman", "woman gesturing OK" });

	public static readonly Emoji ManTippingHand = new Emoji("1F481-200D-2642-FE0F", null, "\ud83d\udc81\u200d♂\ufe0f", "man-tipping-hand", new string[1] { "man-tipping-hand" }, null, null, "Smileys & People", 23, 7, SkinVariationType.None, "man tipping hand", new string[9] { "hand", "help", "information", "person tipping hand", "sassy", "tipping", "man", "man tipping hand", "tipping hand" });

	public static readonly Emoji ManTippingHand_Light = new Emoji("1F481-1F3FB-200D-2642-FE0F", null, "\ud83d\udc81\ud83c\udffb\u200d♂\ufe0f", "man-tipping-hand", new string[1] { "man-tipping-hand" }, null, null, "Smileys & People", 23, 8, SkinVariationType.Light, "man tipping hand", new string[9] { "hand", "help", "information", "person tipping hand", "sassy", "tipping", "man", "man tipping hand", "tipping hand" });

	public static readonly Emoji ManTippingHand_MediumLight = new Emoji("1F481-1F3FC-200D-2642-FE0F", null, "\ud83d\udc81\ud83c\udffc\u200d♂\ufe0f", "man-tipping-hand", new string[1] { "man-tipping-hand" }, null, null, "Smileys & People", 23, 9, SkinVariationType.MediumLight, "man tipping hand", new string[9] { "hand", "help", "information", "person tipping hand", "sassy", "tipping", "man", "man tipping hand", "tipping hand" });

	public static readonly Emoji ManTippingHand_Medium = new Emoji("1F481-1F3FD-200D-2642-FE0F", null, "\ud83d\udc81\ud83c\udffd\u200d♂\ufe0f", "man-tipping-hand", new string[1] { "man-tipping-hand" }, null, null, "Smileys & People", 23, 10, SkinVariationType.Medium, "man tipping hand", new string[9] { "hand", "help", "information", "person tipping hand", "sassy", "tipping", "man", "man tipping hand", "tipping hand" });

	public static readonly Emoji ManTippingHand_MediumDark = new Emoji("1F481-1F3FE-200D-2642-FE0F", null, "\ud83d\udc81\ud83c\udffe\u200d♂\ufe0f", "man-tipping-hand", new string[1] { "man-tipping-hand" }, null, null, "Smileys & People", 23, 11, SkinVariationType.MediumDark, "man tipping hand", new string[9] { "hand", "help", "information", "person tipping hand", "sassy", "tipping", "man", "man tipping hand", "tipping hand" });

	public static readonly Emoji ManTippingHand_Dark = new Emoji("1F481-1F3FF-200D-2642-FE0F", null, "\ud83d\udc81\ud83c\udfff\u200d♂\ufe0f", "man-tipping-hand", new string[1] { "man-tipping-hand" }, null, null, "Smileys & People", 23, 12, SkinVariationType.Dark, "man tipping hand", new string[9] { "hand", "help", "information", "person tipping hand", "sassy", "tipping", "man", "man tipping hand", "tipping hand" });

	public static readonly Emoji WomanTippingHand = new Emoji("1F481-200D-2640-FE0F", null, "\ud83d\udc81\u200d♀\ufe0f", "woman-tipping-hand", new string[1] { "woman-tipping-hand" }, null, null, "Smileys & People", 23, 1, SkinVariationType.None, "woman tipping hand", new string[9] { "hand", "help", "information", "person tipping hand", "sassy", "tipping", "tipping hand", "woman", "woman tipping hand" });

	public static readonly Emoji WomanTippingHand_Light = new Emoji("1F481-1F3FB-200D-2640-FE0F", null, "\ud83d\udc81\ud83c\udffb\u200d♀\ufe0f", "woman-tipping-hand", new string[1] { "woman-tipping-hand" }, null, null, "Smileys & People", 23, 2, SkinVariationType.Light, "woman tipping hand", new string[9] { "hand", "help", "information", "person tipping hand", "sassy", "tipping", "tipping hand", "woman", "woman tipping hand" });

	public static readonly Emoji WomanTippingHand_MediumLight = new Emoji("1F481-1F3FC-200D-2640-FE0F", null, "\ud83d\udc81\ud83c\udffc\u200d♀\ufe0f", "woman-tipping-hand", new string[1] { "woman-tipping-hand" }, null, null, "Smileys & People", 23, 3, SkinVariationType.MediumLight, "woman tipping hand", new string[9] { "hand", "help", "information", "person tipping hand", "sassy", "tipping", "tipping hand", "woman", "woman tipping hand" });

	public static readonly Emoji WomanTippingHand_Medium = new Emoji("1F481-1F3FD-200D-2640-FE0F", null, "\ud83d\udc81\ud83c\udffd\u200d♀\ufe0f", "woman-tipping-hand", new string[1] { "woman-tipping-hand" }, null, null, "Smileys & People", 23, 4, SkinVariationType.Medium, "woman tipping hand", new string[9] { "hand", "help", "information", "person tipping hand", "sassy", "tipping", "tipping hand", "woman", "woman tipping hand" });

	public static readonly Emoji WomanTippingHand_MediumDark = new Emoji("1F481-1F3FE-200D-2640-FE0F", null, "\ud83d\udc81\ud83c\udffe\u200d♀\ufe0f", "woman-tipping-hand", new string[1] { "woman-tipping-hand" }, null, null, "Smileys & People", 23, 5, SkinVariationType.MediumDark, "woman tipping hand", new string[9] { "hand", "help", "information", "person tipping hand", "sassy", "tipping", "tipping hand", "woman", "woman tipping hand" });

	public static readonly Emoji WomanTippingHand_Dark = new Emoji("1F481-1F3FF-200D-2640-FE0F", null, "\ud83d\udc81\ud83c\udfff\u200d♀\ufe0f", "woman-tipping-hand", new string[1] { "woman-tipping-hand" }, null, null, "Smileys & People", 23, 6, SkinVariationType.Dark, "woman tipping hand", new string[9] { "hand", "help", "information", "person tipping hand", "sassy", "tipping", "tipping hand", "woman", "woman tipping hand" });

	public static readonly Emoji ManRaisingHand = new Emoji("1F64B-200D-2642-FE0F", null, "\ud83d\ude4b\u200d♂\ufe0f", "man-raising-hand", new string[1] { "man-raising-hand" }, null, null, "Smileys & People", 33, 0, SkinVariationType.None, "man raising hand", new string[8] { "gesture", "hand", "happy", "person raising hand", "raised", "man", "man raising hand", "raising hand" });

	public static readonly Emoji ManRaisingHand_Light = new Emoji("1F64B-1F3FB-200D-2642-FE0F", null, "\ud83d\ude4b\ud83c\udffb\u200d♂\ufe0f", "man-raising-hand", new string[1] { "man-raising-hand" }, null, null, "Smileys & People", 33, 1, SkinVariationType.Light, "man raising hand", new string[8] { "gesture", "hand", "happy", "person raising hand", "raised", "man", "man raising hand", "raising hand" });

	public static readonly Emoji ManRaisingHand_MediumLight = new Emoji("1F64B-1F3FC-200D-2642-FE0F", null, "\ud83d\ude4b\ud83c\udffc\u200d♂\ufe0f", "man-raising-hand", new string[1] { "man-raising-hand" }, null, null, "Smileys & People", 33, 2, SkinVariationType.MediumLight, "man raising hand", new string[8] { "gesture", "hand", "happy", "person raising hand", "raised", "man", "man raising hand", "raising hand" });

	public static readonly Emoji ManRaisingHand_Medium = new Emoji("1F64B-1F3FD-200D-2642-FE0F", null, "\ud83d\ude4b\ud83c\udffd\u200d♂\ufe0f", "man-raising-hand", new string[1] { "man-raising-hand" }, null, null, "Smileys & People", 33, 3, SkinVariationType.Medium, "man raising hand", new string[8] { "gesture", "hand", "happy", "person raising hand", "raised", "man", "man raising hand", "raising hand" });

	public static readonly Emoji ManRaisingHand_MediumDark = new Emoji("1F64B-1F3FE-200D-2642-FE0F", null, "\ud83d\ude4b\ud83c\udffe\u200d♂\ufe0f", "man-raising-hand", new string[1] { "man-raising-hand" }, null, null, "Smileys & People", 33, 4, SkinVariationType.MediumDark, "man raising hand", new string[8] { "gesture", "hand", "happy", "person raising hand", "raised", "man", "man raising hand", "raising hand" });

	public static readonly Emoji ManRaisingHand_Dark = new Emoji("1F64B-1F3FF-200D-2642-FE0F", null, "\ud83d\ude4b\ud83c\udfff\u200d♂\ufe0f", "man-raising-hand", new string[1] { "man-raising-hand" }, null, null, "Smileys & People", 33, 5, SkinVariationType.Dark, "man raising hand", new string[8] { "gesture", "hand", "happy", "person raising hand", "raised", "man", "man raising hand", "raising hand" });

	public static readonly Emoji WomanRaisingHand = new Emoji("1F64B-200D-2640-FE0F", null, "\ud83d\ude4b\u200d♀\ufe0f", "woman-raising-hand", new string[1] { "woman-raising-hand" }, null, null, "Smileys & People", 32, 46, SkinVariationType.None, "woman raising hand", new string[8] { "gesture", "hand", "happy", "person raising hand", "raised", "raising hand", "woman", "woman raising hand" });

	public static readonly Emoji WomanRaisingHand_Light = new Emoji("1F64B-1F3FB-200D-2640-FE0F", null, "\ud83d\ude4b\ud83c\udffb\u200d♀\ufe0f", "woman-raising-hand", new string[1] { "woman-raising-hand" }, null, null, "Smileys & People", 32, 47, SkinVariationType.Light, "woman raising hand", new string[8] { "gesture", "hand", "happy", "person raising hand", "raised", "raising hand", "woman", "woman raising hand" });

	public static readonly Emoji WomanRaisingHand_MediumLight = new Emoji("1F64B-1F3FC-200D-2640-FE0F", null, "\ud83d\ude4b\ud83c\udffc\u200d♀\ufe0f", "woman-raising-hand", new string[1] { "woman-raising-hand" }, null, null, "Smileys & People", 32, 48, SkinVariationType.MediumLight, "woman raising hand", new string[8] { "gesture", "hand", "happy", "person raising hand", "raised", "raising hand", "woman", "woman raising hand" });

	public static readonly Emoji WomanRaisingHand_Medium = new Emoji("1F64B-1F3FD-200D-2640-FE0F", null, "\ud83d\ude4b\ud83c\udffd\u200d♀\ufe0f", "woman-raising-hand", new string[1] { "woman-raising-hand" }, null, null, "Smileys & People", 32, 49, SkinVariationType.Medium, "woman raising hand", new string[8] { "gesture", "hand", "happy", "person raising hand", "raised", "raising hand", "woman", "woman raising hand" });

	public static readonly Emoji WomanRaisingHand_MediumDark = new Emoji("1F64B-1F3FE-200D-2640-FE0F", null, "\ud83d\ude4b\ud83c\udffe\u200d♀\ufe0f", "woman-raising-hand", new string[1] { "woman-raising-hand" }, null, null, "Smileys & People", 32, 50, SkinVariationType.MediumDark, "woman raising hand", new string[8] { "gesture", "hand", "happy", "person raising hand", "raised", "raising hand", "woman", "woman raising hand" });

	public static readonly Emoji WomanRaisingHand_Dark = new Emoji("1F64B-1F3FF-200D-2640-FE0F", null, "\ud83d\ude4b\ud83c\udfff\u200d♀\ufe0f", "woman-raising-hand", new string[1] { "woman-raising-hand" }, null, null, "Smileys & People", 32, 51, SkinVariationType.Dark, "woman raising hand", new string[8] { "gesture", "hand", "happy", "person raising hand", "raised", "raising hand", "woman", "woman raising hand" });

	public static readonly Emoji ManBowing = new Emoji("1F647-200D-2642-FE0F", null, "\ud83d\ude47\u200d♂\ufe0f", "man-bowing", new string[1] { "man-bowing" }, null, null, "Smileys & People", 32, 31, SkinVariationType.None, "man bowing", new string[8] { "apology", "bow", "gesture", "person bowing", "sorry", "bowing", "favor", "man" });

	public static readonly Emoji ManBowing_Light = new Emoji("1F647-1F3FB-200D-2642-FE0F", null, "\ud83d\ude47\ud83c\udffb\u200d♂\ufe0f", "man-bowing", new string[1] { "man-bowing" }, null, null, "Smileys & People", 32, 32, SkinVariationType.Light, "man bowing", new string[8] { "apology", "bow", "gesture", "person bowing", "sorry", "bowing", "favor", "man" });

	public static readonly Emoji ManBowing_MediumLight = new Emoji("1F647-1F3FC-200D-2642-FE0F", null, "\ud83d\ude47\ud83c\udffc\u200d♂\ufe0f", "man-bowing", new string[1] { "man-bowing" }, null, null, "Smileys & People", 32, 33, SkinVariationType.MediumLight, "man bowing", new string[8] { "apology", "bow", "gesture", "person bowing", "sorry", "bowing", "favor", "man" });

	public static readonly Emoji ManBowing_Medium = new Emoji("1F647-1F3FD-200D-2642-FE0F", null, "\ud83d\ude47\ud83c\udffd\u200d♂\ufe0f", "man-bowing", new string[1] { "man-bowing" }, null, null, "Smileys & People", 32, 34, SkinVariationType.Medium, "man bowing", new string[8] { "apology", "bow", "gesture", "person bowing", "sorry", "bowing", "favor", "man" });

	public static readonly Emoji ManBowing_MediumDark = new Emoji("1F647-1F3FE-200D-2642-FE0F", null, "\ud83d\ude47\ud83c\udffe\u200d♂\ufe0f", "man-bowing", new string[1] { "man-bowing" }, null, null, "Smileys & People", 32, 35, SkinVariationType.MediumDark, "man bowing", new string[8] { "apology", "bow", "gesture", "person bowing", "sorry", "bowing", "favor", "man" });

	public static readonly Emoji ManBowing_Dark = new Emoji("1F647-1F3FF-200D-2642-FE0F", null, "\ud83d\ude47\ud83c\udfff\u200d♂\ufe0f", "man-bowing", new string[1] { "man-bowing" }, null, null, "Smileys & People", 32, 36, SkinVariationType.Dark, "man bowing", new string[8] { "apology", "bow", "gesture", "person bowing", "sorry", "bowing", "favor", "man" });

	public static readonly Emoji WomanBowing = new Emoji("1F647-200D-2640-FE0F", null, "\ud83d\ude47\u200d♀\ufe0f", "woman-bowing", new string[1] { "woman-bowing" }, null, null, "Smileys & People", 32, 25, SkinVariationType.None, "woman bowing", new string[8] { "apology", "bow", "gesture", "person bowing", "sorry", "bowing", "favor", "woman" });

	public static readonly Emoji WomanBowing_Light = new Emoji("1F647-1F3FB-200D-2640-FE0F", null, "\ud83d\ude47\ud83c\udffb\u200d♀\ufe0f", "woman-bowing", new string[1] { "woman-bowing" }, null, null, "Smileys & People", 32, 26, SkinVariationType.Light, "woman bowing", new string[8] { "apology", "bow", "gesture", "person bowing", "sorry", "bowing", "favor", "woman" });

	public static readonly Emoji WomanBowing_MediumLight = new Emoji("1F647-1F3FC-200D-2640-FE0F", null, "\ud83d\ude47\ud83c\udffc\u200d♀\ufe0f", "woman-bowing", new string[1] { "woman-bowing" }, null, null, "Smileys & People", 32, 27, SkinVariationType.MediumLight, "woman bowing", new string[8] { "apology", "bow", "gesture", "person bowing", "sorry", "bowing", "favor", "woman" });

	public static readonly Emoji WomanBowing_Medium = new Emoji("1F647-1F3FD-200D-2640-FE0F", null, "\ud83d\ude47\ud83c\udffd\u200d♀\ufe0f", "woman-bowing", new string[1] { "woman-bowing" }, null, null, "Smileys & People", 32, 28, SkinVariationType.Medium, "woman bowing", new string[8] { "apology", "bow", "gesture", "person bowing", "sorry", "bowing", "favor", "woman" });

	public static readonly Emoji WomanBowing_MediumDark = new Emoji("1F647-1F3FE-200D-2640-FE0F", null, "\ud83d\ude47\ud83c\udffe\u200d♀\ufe0f", "woman-bowing", new string[1] { "woman-bowing" }, null, null, "Smileys & People", 32, 29, SkinVariationType.MediumDark, "woman bowing", new string[8] { "apology", "bow", "gesture", "person bowing", "sorry", "bowing", "favor", "woman" });

	public static readonly Emoji WomanBowing_Dark = new Emoji("1F647-1F3FF-200D-2640-FE0F", null, "\ud83d\ude47\ud83c\udfff\u200d♀\ufe0f", "woman-bowing", new string[1] { "woman-bowing" }, null, null, "Smileys & People", 32, 30, SkinVariationType.Dark, "woman bowing", new string[8] { "apology", "bow", "gesture", "person bowing", "sorry", "bowing", "favor", "woman" });

	public static readonly Emoji ManFacepalming = new Emoji("1F926-200D-2642-FE0F", null, "\ud83e\udd26\u200d♂\ufe0f", "man-facepalming", new string[1] { "man-facepalming" }, null, null, "Smileys & People", 38, 35, SkinVariationType.None, "man facepalming", new string[8] { "disbelief", "exasperation", "face", "palm", "person facepalming", "facepalm", "man", "man facepalming" });

	public static readonly Emoji ManFacepalming_Light = new Emoji("1F926-1F3FB-200D-2642-FE0F", null, "\ud83e\udd26\ud83c\udffb\u200d♂\ufe0f", "man-facepalming", new string[1] { "man-facepalming" }, null, null, "Smileys & People", 38, 36, SkinVariationType.Light, "man facepalming", new string[8] { "disbelief", "exasperation", "face", "palm", "person facepalming", "facepalm", "man", "man facepalming" });

	public static readonly Emoji ManFacepalming_MediumLight = new Emoji("1F926-1F3FC-200D-2642-FE0F", null, "\ud83e\udd26\ud83c\udffc\u200d♂\ufe0f", "man-facepalming", new string[1] { "man-facepalming" }, null, null, "Smileys & People", 38, 37, SkinVariationType.MediumLight, "man facepalming", new string[8] { "disbelief", "exasperation", "face", "palm", "person facepalming", "facepalm", "man", "man facepalming" });

	public static readonly Emoji ManFacepalming_Medium = new Emoji("1F926-1F3FD-200D-2642-FE0F", null, "\ud83e\udd26\ud83c\udffd\u200d♂\ufe0f", "man-facepalming", new string[1] { "man-facepalming" }, null, null, "Smileys & People", 38, 38, SkinVariationType.Medium, "man facepalming", new string[8] { "disbelief", "exasperation", "face", "palm", "person facepalming", "facepalm", "man", "man facepalming" });

	public static readonly Emoji ManFacepalming_MediumDark = new Emoji("1F926-1F3FE-200D-2642-FE0F", null, "\ud83e\udd26\ud83c\udffe\u200d♂\ufe0f", "man-facepalming", new string[1] { "man-facepalming" }, null, null, "Smileys & People", 38, 39, SkinVariationType.MediumDark, "man facepalming", new string[8] { "disbelief", "exasperation", "face", "palm", "person facepalming", "facepalm", "man", "man facepalming" });

	public static readonly Emoji ManFacepalming_Dark = new Emoji("1F926-1F3FF-200D-2642-FE0F", null, "\ud83e\udd26\ud83c\udfff\u200d♂\ufe0f", "man-facepalming", new string[1] { "man-facepalming" }, null, null, "Smileys & People", 38, 40, SkinVariationType.Dark, "man facepalming", new string[8] { "disbelief", "exasperation", "face", "palm", "person facepalming", "facepalm", "man", "man facepalming" });

	public static readonly Emoji WomanFacepalming = new Emoji("1F926-200D-2640-FE0F", null, "\ud83e\udd26\u200d♀\ufe0f", "woman-facepalming", new string[1] { "woman-facepalming" }, null, null, "Smileys & People", 38, 29, SkinVariationType.None, "woman facepalming", new string[8] { "disbelief", "exasperation", "face", "palm", "person facepalming", "facepalm", "woman", "woman facepalming" });

	public static readonly Emoji WomanFacepalming_Light = new Emoji("1F926-1F3FB-200D-2640-FE0F", null, "\ud83e\udd26\ud83c\udffb\u200d♀\ufe0f", "woman-facepalming", new string[1] { "woman-facepalming" }, null, null, "Smileys & People", 38, 30, SkinVariationType.Light, "woman facepalming", new string[8] { "disbelief", "exasperation", "face", "palm", "person facepalming", "facepalm", "woman", "woman facepalming" });

	public static readonly Emoji WomanFacepalming_MediumLight = new Emoji("1F926-1F3FC-200D-2640-FE0F", null, "\ud83e\udd26\ud83c\udffc\u200d♀\ufe0f", "woman-facepalming", new string[1] { "woman-facepalming" }, null, null, "Smileys & People", 38, 31, SkinVariationType.MediumLight, "woman facepalming", new string[8] { "disbelief", "exasperation", "face", "palm", "person facepalming", "facepalm", "woman", "woman facepalming" });

	public static readonly Emoji WomanFacepalming_Medium = new Emoji("1F926-1F3FD-200D-2640-FE0F", null, "\ud83e\udd26\ud83c\udffd\u200d♀\ufe0f", "woman-facepalming", new string[1] { "woman-facepalming" }, null, null, "Smileys & People", 38, 32, SkinVariationType.Medium, "woman facepalming", new string[8] { "disbelief", "exasperation", "face", "palm", "person facepalming", "facepalm", "woman", "woman facepalming" });

	public static readonly Emoji WomanFacepalming_MediumDark = new Emoji("1F926-1F3FE-200D-2640-FE0F", null, "\ud83e\udd26\ud83c\udffe\u200d♀\ufe0f", "woman-facepalming", new string[1] { "woman-facepalming" }, null, null, "Smileys & People", 38, 33, SkinVariationType.MediumDark, "woman facepalming", new string[8] { "disbelief", "exasperation", "face", "palm", "person facepalming", "facepalm", "woman", "woman facepalming" });

	public static readonly Emoji WomanFacepalming_Dark = new Emoji("1F926-1F3FF-200D-2640-FE0F", null, "\ud83e\udd26\ud83c\udfff\u200d♀\ufe0f", "woman-facepalming", new string[1] { "woman-facepalming" }, null, null, "Smileys & People", 38, 34, SkinVariationType.Dark, "woman facepalming", new string[8] { "disbelief", "exasperation", "face", "palm", "person facepalming", "facepalm", "woman", "woman facepalming" });

	public static readonly Emoji ManShrugging = new Emoji("1F937-200D-2642-FE0F", null, "\ud83e\udd37\u200d♂\ufe0f", "man-shrugging", new string[1] { "man-shrugging" }, null, null, "Smileys & People", 40, 0, SkinVariationType.None, "man shrugging", new string[7] { "doubt", "ignorance", "indifference", "person shrugging", "shrug", "man", "man shrugging" });

	public static readonly Emoji ManShrugging_Light = new Emoji("1F937-1F3FB-200D-2642-FE0F", null, "\ud83e\udd37\ud83c\udffb\u200d♂\ufe0f", "man-shrugging", new string[1] { "man-shrugging" }, null, null, "Smileys & People", 40, 1, SkinVariationType.Light, "man shrugging", new string[7] { "doubt", "ignorance", "indifference", "person shrugging", "shrug", "man", "man shrugging" });

	public static readonly Emoji ManShrugging_MediumLight = new Emoji("1F937-1F3FC-200D-2642-FE0F", null, "\ud83e\udd37\ud83c\udffc\u200d♂\ufe0f", "man-shrugging", new string[1] { "man-shrugging" }, null, null, "Smileys & People", 40, 2, SkinVariationType.MediumLight, "man shrugging", new string[7] { "doubt", "ignorance", "indifference", "person shrugging", "shrug", "man", "man shrugging" });

	public static readonly Emoji ManShrugging_Medium = new Emoji("1F937-1F3FD-200D-2642-FE0F", null, "\ud83e\udd37\ud83c\udffd\u200d♂\ufe0f", "man-shrugging", new string[1] { "man-shrugging" }, null, null, "Smileys & People", 40, 3, SkinVariationType.Medium, "man shrugging", new string[7] { "doubt", "ignorance", "indifference", "person shrugging", "shrug", "man", "man shrugging" });

	public static readonly Emoji ManShrugging_MediumDark = new Emoji("1F937-1F3FE-200D-2642-FE0F", null, "\ud83e\udd37\ud83c\udffe\u200d♂\ufe0f", "man-shrugging", new string[1] { "man-shrugging" }, null, null, "Smileys & People", 40, 4, SkinVariationType.MediumDark, "man shrugging", new string[7] { "doubt", "ignorance", "indifference", "person shrugging", "shrug", "man", "man shrugging" });

	public static readonly Emoji ManShrugging_Dark = new Emoji("1F937-1F3FF-200D-2642-FE0F", null, "\ud83e\udd37\ud83c\udfff\u200d♂\ufe0f", "man-shrugging", new string[1] { "man-shrugging" }, null, null, "Smileys & People", 40, 5, SkinVariationType.Dark, "man shrugging", new string[7] { "doubt", "ignorance", "indifference", "person shrugging", "shrug", "man", "man shrugging" });

	public static readonly Emoji WomanShrugging = new Emoji("1F937-200D-2640-FE0F", null, "\ud83e\udd37\u200d♀\ufe0f", "woman-shrugging", new string[1] { "woman-shrugging" }, null, null, "Smileys & People", 39, 46, SkinVariationType.None, "woman shrugging", new string[7] { "doubt", "ignorance", "indifference", "person shrugging", "shrug", "woman", "woman shrugging" });

	public static readonly Emoji WomanShrugging_Light = new Emoji("1F937-1F3FB-200D-2640-FE0F", null, "\ud83e\udd37\ud83c\udffb\u200d♀\ufe0f", "woman-shrugging", new string[1] { "woman-shrugging" }, null, null, "Smileys & People", 39, 47, SkinVariationType.Light, "woman shrugging", new string[7] { "doubt", "ignorance", "indifference", "person shrugging", "shrug", "woman", "woman shrugging" });

	public static readonly Emoji WomanShrugging_MediumLight = new Emoji("1F937-1F3FC-200D-2640-FE0F", null, "\ud83e\udd37\ud83c\udffc\u200d♀\ufe0f", "woman-shrugging", new string[1] { "woman-shrugging" }, null, null, "Smileys & People", 39, 48, SkinVariationType.MediumLight, "woman shrugging", new string[7] { "doubt", "ignorance", "indifference", "person shrugging", "shrug", "woman", "woman shrugging" });

	public static readonly Emoji WomanShrugging_Medium = new Emoji("1F937-1F3FD-200D-2640-FE0F", null, "\ud83e\udd37\ud83c\udffd\u200d♀\ufe0f", "woman-shrugging", new string[1] { "woman-shrugging" }, null, null, "Smileys & People", 39, 49, SkinVariationType.Medium, "woman shrugging", new string[7] { "doubt", "ignorance", "indifference", "person shrugging", "shrug", "woman", "woman shrugging" });

	public static readonly Emoji WomanShrugging_MediumDark = new Emoji("1F937-1F3FE-200D-2640-FE0F", null, "\ud83e\udd37\ud83c\udffe\u200d♀\ufe0f", "woman-shrugging", new string[1] { "woman-shrugging" }, null, null, "Smileys & People", 39, 50, SkinVariationType.MediumDark, "woman shrugging", new string[7] { "doubt", "ignorance", "indifference", "person shrugging", "shrug", "woman", "woman shrugging" });

	public static readonly Emoji WomanShrugging_Dark = new Emoji("1F937-1F3FF-200D-2640-FE0F", null, "\ud83e\udd37\ud83c\udfff\u200d♀\ufe0f", "woman-shrugging", new string[1] { "woman-shrugging" }, null, null, "Smileys & People", 39, 51, SkinVariationType.Dark, "woman shrugging", new string[7] { "doubt", "ignorance", "indifference", "person shrugging", "shrug", "woman", "woman shrugging" });

	public static readonly Emoji ManGettingMassage = new Emoji("1F486-200D-2642-FE0F", null, "\ud83d\udc86\u200d♂\ufe0f", "man-getting-massage", new string[1] { "man-getting-massage" }, null, null, "Smileys & People", 24, 4, SkinVariationType.None, "man getting massage", new string[6] { "face", "massage", "person getting massage", "salon", "man", "man getting massage" });

	public static readonly Emoji ManGettingMassage_Light = new Emoji("1F486-1F3FB-200D-2642-FE0F", null, "\ud83d\udc86\ud83c\udffb\u200d♂\ufe0f", "man-getting-massage", new string[1] { "man-getting-massage" }, null, null, "Smileys & People", 24, 5, SkinVariationType.Light, "man getting massage", new string[6] { "face", "massage", "person getting massage", "salon", "man", "man getting massage" });

	public static readonly Emoji ManGettingMassage_MediumLight = new Emoji("1F486-1F3FC-200D-2642-FE0F", null, "\ud83d\udc86\ud83c\udffc\u200d♂\ufe0f", "man-getting-massage", new string[1] { "man-getting-massage" }, null, null, "Smileys & People", 24, 6, SkinVariationType.MediumLight, "man getting massage", new string[6] { "face", "massage", "person getting massage", "salon", "man", "man getting massage" });

	public static readonly Emoji ManGettingMassage_Medium = new Emoji("1F486-1F3FD-200D-2642-FE0F", null, "\ud83d\udc86\ud83c\udffd\u200d♂\ufe0f", "man-getting-massage", new string[1] { "man-getting-massage" }, null, null, "Smileys & People", 24, 7, SkinVariationType.Medium, "man getting massage", new string[6] { "face", "massage", "person getting massage", "salon", "man", "man getting massage" });

	public static readonly Emoji ManGettingMassage_MediumDark = new Emoji("1F486-1F3FE-200D-2642-FE0F", null, "\ud83d\udc86\ud83c\udffe\u200d♂\ufe0f", "man-getting-massage", new string[1] { "man-getting-massage" }, null, null, "Smileys & People", 24, 8, SkinVariationType.MediumDark, "man getting massage", new string[6] { "face", "massage", "person getting massage", "salon", "man", "man getting massage" });

	public static readonly Emoji ManGettingMassage_Dark = new Emoji("1F486-1F3FF-200D-2642-FE0F", null, "\ud83d\udc86\ud83c\udfff\u200d♂\ufe0f", "man-getting-massage", new string[1] { "man-getting-massage" }, null, null, "Smileys & People", 24, 9, SkinVariationType.Dark, "man getting massage", new string[6] { "face", "massage", "person getting massage", "salon", "man", "man getting massage" });

	public static readonly Emoji WomanGettingMassage = new Emoji("1F486-200D-2640-FE0F", null, "\ud83d\udc86\u200d♀\ufe0f", "woman-getting-massage", new string[1] { "woman-getting-massage" }, null, null, "Smileys & People", 23, 50, SkinVariationType.None, "woman getting massage", new string[6] { "face", "massage", "person getting massage", "salon", "woman", "woman getting massage" });

	public static readonly Emoji WomanGettingMassage_Light = new Emoji("1F486-1F3FB-200D-2640-FE0F", null, "\ud83d\udc86\ud83c\udffb\u200d♀\ufe0f", "woman-getting-massage", new string[1] { "woman-getting-massage" }, null, null, "Smileys & People", 23, 51, SkinVariationType.Light, "woman getting massage", new string[6] { "face", "massage", "person getting massage", "salon", "woman", "woman getting massage" });

	public static readonly Emoji WomanGettingMassage_MediumLight = new Emoji("1F486-1F3FC-200D-2640-FE0F", null, "\ud83d\udc86\ud83c\udffc\u200d♀\ufe0f", "woman-getting-massage", new string[1] { "woman-getting-massage" }, null, null, "Smileys & People", 24, 0, SkinVariationType.MediumLight, "woman getting massage", new string[6] { "face", "massage", "person getting massage", "salon", "woman", "woman getting massage" });

	public static readonly Emoji WomanGettingMassage_Medium = new Emoji("1F486-1F3FD-200D-2640-FE0F", null, "\ud83d\udc86\ud83c\udffd\u200d♀\ufe0f", "woman-getting-massage", new string[1] { "woman-getting-massage" }, null, null, "Smileys & People", 24, 1, SkinVariationType.Medium, "woman getting massage", new string[6] { "face", "massage", "person getting massage", "salon", "woman", "woman getting massage" });

	public static readonly Emoji WomanGettingMassage_MediumDark = new Emoji("1F486-1F3FE-200D-2640-FE0F", null, "\ud83d\udc86\ud83c\udffe\u200d♀\ufe0f", "woman-getting-massage", new string[1] { "woman-getting-massage" }, null, null, "Smileys & People", 24, 2, SkinVariationType.MediumDark, "woman getting massage", new string[6] { "face", "massage", "person getting massage", "salon", "woman", "woman getting massage" });

	public static readonly Emoji WomanGettingMassage_Dark = new Emoji("1F486-1F3FF-200D-2640-FE0F", null, "\ud83d\udc86\ud83c\udfff\u200d♀\ufe0f", "woman-getting-massage", new string[1] { "woman-getting-massage" }, null, null, "Smileys & People", 24, 3, SkinVariationType.Dark, "woman getting massage", new string[6] { "face", "massage", "person getting massage", "salon", "woman", "woman getting massage" });

	public static readonly Emoji ManGettingHaircut = new Emoji("1F487-200D-2642-FE0F", null, "\ud83d\udc87\u200d♂\ufe0f", "man-getting-haircut", new string[1] { "man-getting-haircut" }, null, null, "Smileys & People", 24, 22, SkinVariationType.None, "man getting haircut", new string[7] { "barber", "beauty", "haircut", "parlor", "person getting haircut", "man", "man getting haircut" });

	public static readonly Emoji ManGettingHaircut_Light = new Emoji("1F487-1F3FB-200D-2642-FE0F", null, "\ud83d\udc87\ud83c\udffb\u200d♂\ufe0f", "man-getting-haircut", new string[1] { "man-getting-haircut" }, null, null, "Smileys & People", 24, 23, SkinVariationType.Light, "man getting haircut", new string[7] { "barber", "beauty", "haircut", "parlor", "person getting haircut", "man", "man getting haircut" });

	public static readonly Emoji ManGettingHaircut_MediumLight = new Emoji("1F487-1F3FC-200D-2642-FE0F", null, "\ud83d\udc87\ud83c\udffc\u200d♂\ufe0f", "man-getting-haircut", new string[1] { "man-getting-haircut" }, null, null, "Smileys & People", 24, 24, SkinVariationType.MediumLight, "man getting haircut", new string[7] { "barber", "beauty", "haircut", "parlor", "person getting haircut", "man", "man getting haircut" });

	public static readonly Emoji ManGettingHaircut_Medium = new Emoji("1F487-1F3FD-200D-2642-FE0F", null, "\ud83d\udc87\ud83c\udffd\u200d♂\ufe0f", "man-getting-haircut", new string[1] { "man-getting-haircut" }, null, null, "Smileys & People", 24, 25, SkinVariationType.Medium, "man getting haircut", new string[7] { "barber", "beauty", "haircut", "parlor", "person getting haircut", "man", "man getting haircut" });

	public static readonly Emoji ManGettingHaircut_MediumDark = new Emoji("1F487-1F3FE-200D-2642-FE0F", null, "\ud83d\udc87\ud83c\udffe\u200d♂\ufe0f", "man-getting-haircut", new string[1] { "man-getting-haircut" }, null, null, "Smileys & People", 24, 26, SkinVariationType.MediumDark, "man getting haircut", new string[7] { "barber", "beauty", "haircut", "parlor", "person getting haircut", "man", "man getting haircut" });

	public static readonly Emoji ManGettingHaircut_Dark = new Emoji("1F487-1F3FF-200D-2642-FE0F", null, "\ud83d\udc87\ud83c\udfff\u200d♂\ufe0f", "man-getting-haircut", new string[1] { "man-getting-haircut" }, null, null, "Smileys & People", 24, 27, SkinVariationType.Dark, "man getting haircut", new string[7] { "barber", "beauty", "haircut", "parlor", "person getting haircut", "man", "man getting haircut" });

	public static readonly Emoji WomanGettingHaircut = new Emoji("1F487-200D-2640-FE0F", null, "\ud83d\udc87\u200d♀\ufe0f", "woman-getting-haircut", new string[1] { "woman-getting-haircut" }, null, null, "Smileys & People", 24, 16, SkinVariationType.None, "woman getting haircut", new string[7] { "barber", "beauty", "haircut", "parlor", "person getting haircut", "woman", "woman getting haircut" });

	public static readonly Emoji WomanGettingHaircut_Light = new Emoji("1F487-1F3FB-200D-2640-FE0F", null, "\ud83d\udc87\ud83c\udffb\u200d♀\ufe0f", "woman-getting-haircut", new string[1] { "woman-getting-haircut" }, null, null, "Smileys & People", 24, 17, SkinVariationType.Light, "woman getting haircut", new string[7] { "barber", "beauty", "haircut", "parlor", "person getting haircut", "woman", "woman getting haircut" });

	public static readonly Emoji WomanGettingHaircut_MediumLight = new Emoji("1F487-1F3FC-200D-2640-FE0F", null, "\ud83d\udc87\ud83c\udffc\u200d♀\ufe0f", "woman-getting-haircut", new string[1] { "woman-getting-haircut" }, null, null, "Smileys & People", 24, 18, SkinVariationType.MediumLight, "woman getting haircut", new string[7] { "barber", "beauty", "haircut", "parlor", "person getting haircut", "woman", "woman getting haircut" });

	public static readonly Emoji WomanGettingHaircut_Medium = new Emoji("1F487-1F3FD-200D-2640-FE0F", null, "\ud83d\udc87\ud83c\udffd\u200d♀\ufe0f", "woman-getting-haircut", new string[1] { "woman-getting-haircut" }, null, null, "Smileys & People", 24, 19, SkinVariationType.Medium, "woman getting haircut", new string[7] { "barber", "beauty", "haircut", "parlor", "person getting haircut", "woman", "woman getting haircut" });

	public static readonly Emoji WomanGettingHaircut_MediumDark = new Emoji("1F487-1F3FE-200D-2640-FE0F", null, "\ud83d\udc87\ud83c\udffe\u200d♀\ufe0f", "woman-getting-haircut", new string[1] { "woman-getting-haircut" }, null, null, "Smileys & People", 24, 20, SkinVariationType.MediumDark, "woman getting haircut", new string[7] { "barber", "beauty", "haircut", "parlor", "person getting haircut", "woman", "woman getting haircut" });

	public static readonly Emoji WomanGettingHaircut_Dark = new Emoji("1F487-1F3FF-200D-2640-FE0F", null, "\ud83d\udc87\ud83c\udfff\u200d♀\ufe0f", "woman-getting-haircut", new string[1] { "woman-getting-haircut" }, null, null, "Smileys & People", 24, 21, SkinVariationType.Dark, "woman getting haircut", new string[7] { "barber", "beauty", "haircut", "parlor", "person getting haircut", "woman", "woman getting haircut" });

	public static readonly Emoji ManWalking = new Emoji("1F6B6-200D-2642-FE0F", null, "\ud83d\udeb6\u200d♂\ufe0f", "man-walking", new string[1] { "man-walking" }, null, null, "Smileys & People", 36, 15, SkinVariationType.None, "man walking", new string[6] { "hike", "person walking", "walk", "walking", "man", "man walking" });

	public static readonly Emoji ManWalking_Light = new Emoji("1F6B6-1F3FB-200D-2642-FE0F", null, "\ud83d\udeb6\ud83c\udffb\u200d♂\ufe0f", "man-walking", new string[1] { "man-walking" }, null, null, "Smileys & People", 36, 16, SkinVariationType.Light, "man walking", new string[6] { "hike", "person walking", "walk", "walking", "man", "man walking" });

	public static readonly Emoji ManWalking_MediumLight = new Emoji("1F6B6-1F3FC-200D-2642-FE0F", null, "\ud83d\udeb6\ud83c\udffc\u200d♂\ufe0f", "man-walking", new string[1] { "man-walking" }, null, null, "Smileys & People", 36, 17, SkinVariationType.MediumLight, "man walking", new string[6] { "hike", "person walking", "walk", "walking", "man", "man walking" });

	public static readonly Emoji ManWalking_Medium = new Emoji("1F6B6-1F3FD-200D-2642-FE0F", null, "\ud83d\udeb6\ud83c\udffd\u200d♂\ufe0f", "man-walking", new string[1] { "man-walking" }, null, null, "Smileys & People", 36, 18, SkinVariationType.Medium, "man walking", new string[6] { "hike", "person walking", "walk", "walking", "man", "man walking" });

	public static readonly Emoji ManWalking_MediumDark = new Emoji("1F6B6-1F3FE-200D-2642-FE0F", null, "\ud83d\udeb6\ud83c\udffe\u200d♂\ufe0f", "man-walking", new string[1] { "man-walking" }, null, null, "Smileys & People", 36, 19, SkinVariationType.MediumDark, "man walking", new string[6] { "hike", "person walking", "walk", "walking", "man", "man walking" });

	public static readonly Emoji ManWalking_Dark = new Emoji("1F6B6-1F3FF-200D-2642-FE0F", null, "\ud83d\udeb6\ud83c\udfff\u200d♂\ufe0f", "man-walking", new string[1] { "man-walking" }, null, null, "Smileys & People", 36, 20, SkinVariationType.Dark, "man walking", new string[6] { "hike", "person walking", "walk", "walking", "man", "man walking" });

	public static readonly Emoji WomanWalking = new Emoji("1F6B6-200D-2640-FE0F", null, "\ud83d\udeb6\u200d♀\ufe0f", "woman-walking", new string[1] { "woman-walking" }, null, null, "Smileys & People", 36, 9, SkinVariationType.None, "woman walking", new string[6] { "hike", "person walking", "walk", "walking", "woman", "woman walking" });

	public static readonly Emoji WomanWalking_Light = new Emoji("1F6B6-1F3FB-200D-2640-FE0F", null, "\ud83d\udeb6\ud83c\udffb\u200d♀\ufe0f", "woman-walking", new string[1] { "woman-walking" }, null, null, "Smileys & People", 36, 10, SkinVariationType.Light, "woman walking", new string[6] { "hike", "person walking", "walk", "walking", "woman", "woman walking" });

	public static readonly Emoji WomanWalking_MediumLight = new Emoji("1F6B6-1F3FC-200D-2640-FE0F", null, "\ud83d\udeb6\ud83c\udffc\u200d♀\ufe0f", "woman-walking", new string[1] { "woman-walking" }, null, null, "Smileys & People", 36, 11, SkinVariationType.MediumLight, "woman walking", new string[6] { "hike", "person walking", "walk", "walking", "woman", "woman walking" });

	public static readonly Emoji WomanWalking_Medium = new Emoji("1F6B6-1F3FD-200D-2640-FE0F", null, "\ud83d\udeb6\ud83c\udffd\u200d♀\ufe0f", "woman-walking", new string[1] { "woman-walking" }, null, null, "Smileys & People", 36, 12, SkinVariationType.Medium, "woman walking", new string[6] { "hike", "person walking", "walk", "walking", "woman", "woman walking" });

	public static readonly Emoji WomanWalking_MediumDark = new Emoji("1F6B6-1F3FE-200D-2640-FE0F", null, "\ud83d\udeb6\ud83c\udffe\u200d♀\ufe0f", "woman-walking", new string[1] { "woman-walking" }, null, null, "Smileys & People", 36, 13, SkinVariationType.MediumDark, "woman walking", new string[6] { "hike", "person walking", "walk", "walking", "woman", "woman walking" });

	public static readonly Emoji WomanWalking_Dark = new Emoji("1F6B6-1F3FF-200D-2640-FE0F", null, "\ud83d\udeb6\ud83c\udfff\u200d♀\ufe0f", "woman-walking", new string[1] { "woman-walking" }, null, null, "Smileys & People", 36, 14, SkinVariationType.Dark, "woman walking", new string[6] { "hike", "person walking", "walk", "walking", "woman", "woman walking" });

	public static readonly Emoji ManRunning = new Emoji("1F3C3-200D-2642-FE0F", null, "\ud83c\udfc3\u200d♂\ufe0f", "man-running", new string[1] { "man-running" }, null, null, "Smileys & People", 9, 40, SkinVariationType.None, "man running", new string[5] { "marathon", "person running", "running", "man", "racing" });

	public static readonly Emoji ManRunning_Light = new Emoji("1F3C3-1F3FB-200D-2642-FE0F", null, "\ud83c\udfc3\ud83c\udffb\u200d♂\ufe0f", "man-running", new string[1] { "man-running" }, null, null, "Smileys & People", 9, 41, SkinVariationType.Light, "man running", new string[5] { "marathon", "person running", "running", "man", "racing" });

	public static readonly Emoji ManRunning_MediumLight = new Emoji("1F3C3-1F3FC-200D-2642-FE0F", null, "\ud83c\udfc3\ud83c\udffc\u200d♂\ufe0f", "man-running", new string[1] { "man-running" }, null, null, "Smileys & People", 9, 42, SkinVariationType.MediumLight, "man running", new string[5] { "marathon", "person running", "running", "man", "racing" });

	public static readonly Emoji ManRunning_Medium = new Emoji("1F3C3-1F3FD-200D-2642-FE0F", null, "\ud83c\udfc3\ud83c\udffd\u200d♂\ufe0f", "man-running", new string[1] { "man-running" }, null, null, "Smileys & People", 9, 43, SkinVariationType.Medium, "man running", new string[5] { "marathon", "person running", "running", "man", "racing" });

	public static readonly Emoji ManRunning_MediumDark = new Emoji("1F3C3-1F3FE-200D-2642-FE0F", null, "\ud83c\udfc3\ud83c\udffe\u200d♂\ufe0f", "man-running", new string[1] { "man-running" }, null, null, "Smileys & People", 9, 44, SkinVariationType.MediumDark, "man running", new string[5] { "marathon", "person running", "running", "man", "racing" });

	public static readonly Emoji ManRunning_Dark = new Emoji("1F3C3-1F3FF-200D-2642-FE0F", null, "\ud83c\udfc3\ud83c\udfff\u200d♂\ufe0f", "man-running", new string[1] { "man-running" }, null, null, "Smileys & People", 9, 45, SkinVariationType.Dark, "man running", new string[5] { "marathon", "person running", "running", "man", "racing" });

	public static readonly Emoji WomanRunning = new Emoji("1F3C3-200D-2640-FE0F", null, "\ud83c\udfc3\u200d♀\ufe0f", "woman-running", new string[1] { "woman-running" }, null, null, "Smileys & People", 9, 34, SkinVariationType.None, "woman running", new string[5] { "marathon", "person running", "running", "racing", "woman" });

	public static readonly Emoji WomanRunning_Light = new Emoji("1F3C3-1F3FB-200D-2640-FE0F", null, "\ud83c\udfc3\ud83c\udffb\u200d♀\ufe0f", "woman-running", new string[1] { "woman-running" }, null, null, "Smileys & People", 9, 35, SkinVariationType.Light, "woman running", new string[5] { "marathon", "person running", "running", "racing", "woman" });

	public static readonly Emoji WomanRunning_MediumLight = new Emoji("1F3C3-1F3FC-200D-2640-FE0F", null, "\ud83c\udfc3\ud83c\udffc\u200d♀\ufe0f", "woman-running", new string[1] { "woman-running" }, null, null, "Smileys & People", 9, 36, SkinVariationType.MediumLight, "woman running", new string[5] { "marathon", "person running", "running", "racing", "woman" });

	public static readonly Emoji WomanRunning_Medium = new Emoji("1F3C3-1F3FD-200D-2640-FE0F", null, "\ud83c\udfc3\ud83c\udffd\u200d♀\ufe0f", "woman-running", new string[1] { "woman-running" }, null, null, "Smileys & People", 9, 37, SkinVariationType.Medium, "woman running", new string[5] { "marathon", "person running", "running", "racing", "woman" });

	public static readonly Emoji WomanRunning_MediumDark = new Emoji("1F3C3-1F3FE-200D-2640-FE0F", null, "\ud83c\udfc3\ud83c\udffe\u200d♀\ufe0f", "woman-running", new string[1] { "woman-running" }, null, null, "Smileys & People", 9, 38, SkinVariationType.MediumDark, "woman running", new string[5] { "marathon", "person running", "running", "racing", "woman" });

	public static readonly Emoji WomanRunning_Dark = new Emoji("1F3C3-1F3FF-200D-2640-FE0F", null, "\ud83c\udfc3\ud83c\udfff\u200d♀\ufe0f", "woman-running", new string[1] { "woman-running" }, null, null, "Smileys & People", 9, 39, SkinVariationType.Dark, "woman running", new string[5] { "marathon", "person running", "running", "racing", "woman" });

	public static readonly Emoji Dancer = new Emoji("1F483", "DANCER", "\ud83d\udc83", "dancer", new string[1] { "dancer" }, null, null, "Smileys & People", 23, 37, SkinVariationType.None, "woman dancing", new string[2] { "dancing", "woman" });

	public static readonly Emoji Dancer_Light = new Emoji("1F483-1F3FB", "DANCER", "\ud83d\udc83\ud83c\udffb", "dancer", new string[1] { "dancer" }, null, null, "Smileys & People", 23, 38, SkinVariationType.Light, "woman dancing", new string[2] { "dancing", "woman" });

	public static readonly Emoji Dancer_MediumLight = new Emoji("1F483-1F3FC", "DANCER", "\ud83d\udc83\ud83c\udffc", "dancer", new string[1] { "dancer" }, null, null, "Smileys & People", 23, 39, SkinVariationType.MediumLight, "woman dancing", new string[2] { "dancing", "woman" });

	public static readonly Emoji Dancer_Medium = new Emoji("1F483-1F3FD", "DANCER", "\ud83d\udc83\ud83c\udffd", "dancer", new string[1] { "dancer" }, null, null, "Smileys & People", 23, 40, SkinVariationType.Medium, "woman dancing", new string[2] { "dancing", "woman" });

	public static readonly Emoji Dancer_MediumDark = new Emoji("1F483-1F3FE", "DANCER", "\ud83d\udc83\ud83c\udffe", "dancer", new string[1] { "dancer" }, null, null, "Smileys & People", 23, 41, SkinVariationType.MediumDark, "woman dancing", new string[2] { "dancing", "woman" });

	public static readonly Emoji Dancer_Dark = new Emoji("1F483-1F3FF", "DANCER", "\ud83d\udc83\ud83c\udfff", "dancer", new string[1] { "dancer" }, null, null, "Smileys & People", 23, 42, SkinVariationType.Dark, "woman dancing", new string[2] { "dancing", "woman" });

	public static readonly Emoji ManDancing = new Emoji("1F57A", "MAN DANCING", "\ud83d\udd7a", "man_dancing", new string[1] { "man_dancing" }, null, null, "Smileys & People", 29, 21, SkinVariationType.None, "man dancing", new string[3] { "dance", "man", "man dancing" });

	public static readonly Emoji ManDancing_Light = new Emoji("1F57A-1F3FB", "MAN DANCING", "\ud83d\udd7a\ud83c\udffb", "man_dancing", new string[1] { "man_dancing" }, null, null, "Smileys & People", 29, 22, SkinVariationType.Light, "man dancing", new string[3] { "dance", "man", "man dancing" });

	public static readonly Emoji ManDancing_MediumLight = new Emoji("1F57A-1F3FC", "MAN DANCING", "\ud83d\udd7a\ud83c\udffc", "man_dancing", new string[1] { "man_dancing" }, null, null, "Smileys & People", 29, 23, SkinVariationType.MediumLight, "man dancing", new string[3] { "dance", "man", "man dancing" });

	public static readonly Emoji ManDancing_Medium = new Emoji("1F57A-1F3FD", "MAN DANCING", "\ud83d\udd7a\ud83c\udffd", "man_dancing", new string[1] { "man_dancing" }, null, null, "Smileys & People", 29, 24, SkinVariationType.Medium, "man dancing", new string[3] { "dance", "man", "man dancing" });

	public static readonly Emoji ManDancing_MediumDark = new Emoji("1F57A-1F3FE", "MAN DANCING", "\ud83d\udd7a\ud83c\udffe", "man_dancing", new string[1] { "man_dancing" }, null, null, "Smileys & People", 29, 25, SkinVariationType.MediumDark, "man dancing", new string[3] { "dance", "man", "man dancing" });

	public static readonly Emoji ManDancing_Dark = new Emoji("1F57A-1F3FF", "MAN DANCING", "\ud83d\udd7a\ud83c\udfff", "man_dancing", new string[1] { "man_dancing" }, null, null, "Smileys & People", 29, 26, SkinVariationType.Dark, "man dancing", new string[3] { "dance", "man", "man dancing" });

	public static readonly Emoji ManWithBunnyEarsPartying = new Emoji("1F46F-200D-2642-FE0F", null, "\ud83d\udc6f\u200d♂\ufe0f", "man-with-bunny-ears-partying", new string[1] { "man-with-bunny-ears-partying" }, null, null, "Smileys & People", 21, 0, SkinVariationType.None, "men with bunny ears", new string[6] { "bunny ear", "dancer", "partying", "people with bunny ears", "men", "men with bunny ears" });

	public static readonly Emoji WomanWithBunnyEarsPartying = new Emoji("1F46F-200D-2640-FE0F", null, "\ud83d\udc6f\u200d♀\ufe0f", "woman-with-bunny-ears-partying", new string[1] { "woman-with-bunny-ears-partying" }, null, null, "Smileys & People", 20, 51, SkinVariationType.None, "women with bunny ears", new string[6] { "bunny ear", "dancer", "partying", "people with bunny ears", "women", "women with bunny ears" });

	public static readonly Emoji WomanInSteamyRoom = new Emoji("1F9D6-200D-2640-FE0F", null, "\ud83e\uddd6\u200d♀\ufe0f", "woman_in_steamy_room", new string[1] { "woman_in_steamy_room" }, null, null, "Smileys & People", 43, 28, SkinVariationType.None, "woman in steamy room", new string[4] { "person in steamy room", "sauna", "steam room", "woman in steamy room" });

	public static readonly Emoji WomanInSteamyRoom_Light = new Emoji("1F9D6-1F3FB-200D-2640-FE0F", null, "\ud83e\uddd6\ud83c\udffb\u200d♀\ufe0f", "woman_in_steamy_room", new string[1] { "woman_in_steamy_room" }, null, null, "Smileys & People", 43, 29, SkinVariationType.Light, "woman in steamy room", new string[4] { "person in steamy room", "sauna", "steam room", "woman in steamy room" });

	public static readonly Emoji WomanInSteamyRoom_MediumLight = new Emoji("1F9D6-1F3FC-200D-2640-FE0F", null, "\ud83e\uddd6\ud83c\udffc\u200d♀\ufe0f", "woman_in_steamy_room", new string[1] { "woman_in_steamy_room" }, null, null, "Smileys & People", 43, 30, SkinVariationType.MediumLight, "woman in steamy room", new string[4] { "person in steamy room", "sauna", "steam room", "woman in steamy room" });

	public static readonly Emoji WomanInSteamyRoom_Medium = new Emoji("1F9D6-1F3FD-200D-2640-FE0F", null, "\ud83e\uddd6\ud83c\udffd\u200d♀\ufe0f", "woman_in_steamy_room", new string[1] { "woman_in_steamy_room" }, null, null, "Smileys & People", 43, 31, SkinVariationType.Medium, "woman in steamy room", new string[4] { "person in steamy room", "sauna", "steam room", "woman in steamy room" });

	public static readonly Emoji WomanInSteamyRoom_MediumDark = new Emoji("1F9D6-1F3FE-200D-2640-FE0F", null, "\ud83e\uddd6\ud83c\udffe\u200d♀\ufe0f", "woman_in_steamy_room", new string[1] { "woman_in_steamy_room" }, null, null, "Smileys & People", 43, 32, SkinVariationType.MediumDark, "woman in steamy room", new string[4] { "person in steamy room", "sauna", "steam room", "woman in steamy room" });

	public static readonly Emoji WomanInSteamyRoom_Dark = new Emoji("1F9D6-1F3FF-200D-2640-FE0F", null, "\ud83e\uddd6\ud83c\udfff\u200d♀\ufe0f", "woman_in_steamy_room", new string[1] { "woman_in_steamy_room" }, null, null, "Smileys & People", 43, 33, SkinVariationType.Dark, "woman in steamy room", new string[4] { "person in steamy room", "sauna", "steam room", "woman in steamy room" });

	public static readonly Emoji ManInSteamyRoom = new Emoji("1F9D6-200D-2642-FE0F", null, "\ud83e\uddd6\u200d♂\ufe0f", "man_in_steamy_room", new string[1] { "man_in_steamy_room" }, null, null, "Smileys & People", 43, 34, SkinVariationType.None, "man in steamy room", new string[4] { "person in steamy room", "sauna", "steam room", "man in steamy room" });

	public static readonly Emoji ManInSteamyRoom_Light = new Emoji("1F9D6-1F3FB-200D-2642-FE0F", null, "\ud83e\uddd6\ud83c\udffb\u200d♂\ufe0f", "man_in_steamy_room", new string[1] { "man_in_steamy_room" }, null, null, "Smileys & People", 43, 35, SkinVariationType.Light, "man in steamy room", new string[4] { "person in steamy room", "sauna", "steam room", "man in steamy room" });

	public static readonly Emoji ManInSteamyRoom_MediumLight = new Emoji("1F9D6-1F3FC-200D-2642-FE0F", null, "\ud83e\uddd6\ud83c\udffc\u200d♂\ufe0f", "man_in_steamy_room", new string[1] { "man_in_steamy_room" }, null, null, "Smileys & People", 43, 36, SkinVariationType.MediumLight, "man in steamy room", new string[4] { "person in steamy room", "sauna", "steam room", "man in steamy room" });

	public static readonly Emoji ManInSteamyRoom_Medium = new Emoji("1F9D6-1F3FD-200D-2642-FE0F", null, "\ud83e\uddd6\ud83c\udffd\u200d♂\ufe0f", "man_in_steamy_room", new string[1] { "man_in_steamy_room" }, null, null, "Smileys & People", 43, 37, SkinVariationType.Medium, "man in steamy room", new string[4] { "person in steamy room", "sauna", "steam room", "man in steamy room" });

	public static readonly Emoji ManInSteamyRoom_MediumDark = new Emoji("1F9D6-1F3FE-200D-2642-FE0F", null, "\ud83e\uddd6\ud83c\udffe\u200d♂\ufe0f", "man_in_steamy_room", new string[1] { "man_in_steamy_room" }, null, null, "Smileys & People", 43, 38, SkinVariationType.MediumDark, "man in steamy room", new string[4] { "person in steamy room", "sauna", "steam room", "man in steamy room" });

	public static readonly Emoji ManInSteamyRoom_Dark = new Emoji("1F9D6-1F3FF-200D-2642-FE0F", null, "\ud83e\uddd6\ud83c\udfff\u200d♂\ufe0f", "man_in_steamy_room", new string[1] { "man_in_steamy_room" }, null, null, "Smileys & People", 43, 39, SkinVariationType.Dark, "man in steamy room", new string[4] { "person in steamy room", "sauna", "steam room", "man in steamy room" });

	public static readonly Emoji WomanClimbing = new Emoji("1F9D7-200D-2640-FE0F", null, "\ud83e\uddd7\u200d♀\ufe0f", "woman_climbing", new string[1] { "woman_climbing" }, null, null, "Smileys & People", 43, 46, SkinVariationType.None, "woman climbing", new string[3] { "climber", "person climbing", "woman climbing" });

	public static readonly Emoji WomanClimbing_Light = new Emoji("1F9D7-1F3FB-200D-2640-FE0F", null, "\ud83e\uddd7\ud83c\udffb\u200d♀\ufe0f", "woman_climbing", new string[1] { "woman_climbing" }, null, null, "Smileys & People", 43, 47, SkinVariationType.Light, "woman climbing", new string[3] { "climber", "person climbing", "woman climbing" });

	public static readonly Emoji WomanClimbing_MediumLight = new Emoji("1F9D7-1F3FC-200D-2640-FE0F", null, "\ud83e\uddd7\ud83c\udffc\u200d♀\ufe0f", "woman_climbing", new string[1] { "woman_climbing" }, null, null, "Smileys & People", 43, 48, SkinVariationType.MediumLight, "woman climbing", new string[3] { "climber", "person climbing", "woman climbing" });

	public static readonly Emoji WomanClimbing_Medium = new Emoji("1F9D7-1F3FD-200D-2640-FE0F", null, "\ud83e\uddd7\ud83c\udffd\u200d♀\ufe0f", "woman_climbing", new string[1] { "woman_climbing" }, null, null, "Smileys & People", 43, 49, SkinVariationType.Medium, "woman climbing", new string[3] { "climber", "person climbing", "woman climbing" });

	public static readonly Emoji WomanClimbing_MediumDark = new Emoji("1F9D7-1F3FE-200D-2640-FE0F", null, "\ud83e\uddd7\ud83c\udffe\u200d♀\ufe0f", "woman_climbing", new string[1] { "woman_climbing" }, null, null, "Smileys & People", 43, 50, SkinVariationType.MediumDark, "woman climbing", new string[3] { "climber", "person climbing", "woman climbing" });

	public static readonly Emoji WomanClimbing_Dark = new Emoji("1F9D7-1F3FF-200D-2640-FE0F", null, "\ud83e\uddd7\ud83c\udfff\u200d♀\ufe0f", "woman_climbing", new string[1] { "woman_climbing" }, null, null, "Smileys & People", 43, 51, SkinVariationType.Dark, "woman climbing", new string[3] { "climber", "person climbing", "woman climbing" });

	public static readonly Emoji ManClimbing = new Emoji("1F9D7-200D-2642-FE0F", null, "\ud83e\uddd7\u200d♂\ufe0f", "man_climbing", new string[1] { "man_climbing" }, null, null, "Smileys & People", 44, 0, SkinVariationType.None, "man climbing", new string[3] { "climber", "person climbing", "man climbing" });

	public static readonly Emoji ManClimbing_Light = new Emoji("1F9D7-1F3FB-200D-2642-FE0F", null, "\ud83e\uddd7\ud83c\udffb\u200d♂\ufe0f", "man_climbing", new string[1] { "man_climbing" }, null, null, "Smileys & People", 44, 1, SkinVariationType.Light, "man climbing", new string[3] { "climber", "person climbing", "man climbing" });

	public static readonly Emoji ManClimbing_MediumLight = new Emoji("1F9D7-1F3FC-200D-2642-FE0F", null, "\ud83e\uddd7\ud83c\udffc\u200d♂\ufe0f", "man_climbing", new string[1] { "man_climbing" }, null, null, "Smileys & People", 44, 2, SkinVariationType.MediumLight, "man climbing", new string[3] { "climber", "person climbing", "man climbing" });

	public static readonly Emoji ManClimbing_Medium = new Emoji("1F9D7-1F3FD-200D-2642-FE0F", null, "\ud83e\uddd7\ud83c\udffd\u200d♂\ufe0f", "man_climbing", new string[1] { "man_climbing" }, null, null, "Smileys & People", 44, 3, SkinVariationType.Medium, "man climbing", new string[3] { "climber", "person climbing", "man climbing" });

	public static readonly Emoji ManClimbing_MediumDark = new Emoji("1F9D7-1F3FE-200D-2642-FE0F", null, "\ud83e\uddd7\ud83c\udffe\u200d♂\ufe0f", "man_climbing", new string[1] { "man_climbing" }, null, null, "Smileys & People", 44, 4, SkinVariationType.MediumDark, "man climbing", new string[3] { "climber", "person climbing", "man climbing" });

	public static readonly Emoji ManClimbing_Dark = new Emoji("1F9D7-1F3FF-200D-2642-FE0F", null, "\ud83e\uddd7\ud83c\udfff\u200d♂\ufe0f", "man_climbing", new string[1] { "man_climbing" }, null, null, "Smileys & People", 44, 5, SkinVariationType.Dark, "man climbing", new string[3] { "climber", "person climbing", "man climbing" });

	public static readonly Emoji WomanInLotusPosition = new Emoji("1F9D8-200D-2640-FE0F", null, "\ud83e\uddd8\u200d♀\ufe0f", "woman_in_lotus_position", new string[1] { "woman_in_lotus_position" }, null, null, "Smileys & People", 44, 12, SkinVariationType.None, "woman in lotus position", new string[4] { "meditation", "person in lotus position", "yoga", "woman in lotus position" });

	public static readonly Emoji WomanInLotusPosition_Light = new Emoji("1F9D8-1F3FB-200D-2640-FE0F", null, "\ud83e\uddd8\ud83c\udffb\u200d♀\ufe0f", "woman_in_lotus_position", new string[1] { "woman_in_lotus_position" }, null, null, "Smileys & People", 44, 13, SkinVariationType.Light, "woman in lotus position", new string[4] { "meditation", "person in lotus position", "yoga", "woman in lotus position" });

	public static readonly Emoji WomanInLotusPosition_MediumLight = new Emoji("1F9D8-1F3FC-200D-2640-FE0F", null, "\ud83e\uddd8\ud83c\udffc\u200d♀\ufe0f", "woman_in_lotus_position", new string[1] { "woman_in_lotus_position" }, null, null, "Smileys & People", 44, 14, SkinVariationType.MediumLight, "woman in lotus position", new string[4] { "meditation", "person in lotus position", "yoga", "woman in lotus position" });

	public static readonly Emoji WomanInLotusPosition_Medium = new Emoji("1F9D8-1F3FD-200D-2640-FE0F", null, "\ud83e\uddd8\ud83c\udffd\u200d♀\ufe0f", "woman_in_lotus_position", new string[1] { "woman_in_lotus_position" }, null, null, "Smileys & People", 44, 15, SkinVariationType.Medium, "woman in lotus position", new string[4] { "meditation", "person in lotus position", "yoga", "woman in lotus position" });

	public static readonly Emoji WomanInLotusPosition_MediumDark = new Emoji("1F9D8-1F3FE-200D-2640-FE0F", null, "\ud83e\uddd8\ud83c\udffe\u200d♀\ufe0f", "woman_in_lotus_position", new string[1] { "woman_in_lotus_position" }, null, null, "Smileys & People", 44, 16, SkinVariationType.MediumDark, "woman in lotus position", new string[4] { "meditation", "person in lotus position", "yoga", "woman in lotus position" });

	public static readonly Emoji WomanInLotusPosition_Dark = new Emoji("1F9D8-1F3FF-200D-2640-FE0F", null, "\ud83e\uddd8\ud83c\udfff\u200d♀\ufe0f", "woman_in_lotus_position", new string[1] { "woman_in_lotus_position" }, null, null, "Smileys & People", 44, 17, SkinVariationType.Dark, "woman in lotus position", new string[4] { "meditation", "person in lotus position", "yoga", "woman in lotus position" });

	public static readonly Emoji ManInLotusPosition = new Emoji("1F9D8-200D-2642-FE0F", null, "\ud83e\uddd8\u200d♂\ufe0f", "man_in_lotus_position", new string[1] { "man_in_lotus_position" }, null, null, "Smileys & People", 44, 18, SkinVariationType.None, "man in lotus position", new string[4] { "meditation", "person in lotus position", "yoga", "man in lotus position" });

	public static readonly Emoji ManInLotusPosition_Light = new Emoji("1F9D8-1F3FB-200D-2642-FE0F", null, "\ud83e\uddd8\ud83c\udffb\u200d♂\ufe0f", "man_in_lotus_position", new string[1] { "man_in_lotus_position" }, null, null, "Smileys & People", 44, 19, SkinVariationType.Light, "man in lotus position", new string[4] { "meditation", "person in lotus position", "yoga", "man in lotus position" });

	public static readonly Emoji ManInLotusPosition_MediumLight = new Emoji("1F9D8-1F3FC-200D-2642-FE0F", null, "\ud83e\uddd8\ud83c\udffc\u200d♂\ufe0f", "man_in_lotus_position", new string[1] { "man_in_lotus_position" }, null, null, "Smileys & People", 44, 20, SkinVariationType.MediumLight, "man in lotus position", new string[4] { "meditation", "person in lotus position", "yoga", "man in lotus position" });

	public static readonly Emoji ManInLotusPosition_Medium = new Emoji("1F9D8-1F3FD-200D-2642-FE0F", null, "\ud83e\uddd8\ud83c\udffd\u200d♂\ufe0f", "man_in_lotus_position", new string[1] { "man_in_lotus_position" }, null, null, "Smileys & People", 44, 21, SkinVariationType.Medium, "man in lotus position", new string[4] { "meditation", "person in lotus position", "yoga", "man in lotus position" });

	public static readonly Emoji ManInLotusPosition_MediumDark = new Emoji("1F9D8-1F3FE-200D-2642-FE0F", null, "\ud83e\uddd8\ud83c\udffe\u200d♂\ufe0f", "man_in_lotus_position", new string[1] { "man_in_lotus_position" }, null, null, "Smileys & People", 44, 22, SkinVariationType.MediumDark, "man in lotus position", new string[4] { "meditation", "person in lotus position", "yoga", "man in lotus position" });

	public static readonly Emoji ManInLotusPosition_Dark = new Emoji("1F9D8-1F3FF-200D-2642-FE0F", null, "\ud83e\uddd8\ud83c\udfff\u200d♂\ufe0f", "man_in_lotus_position", new string[1] { "man_in_lotus_position" }, null, null, "Smileys & People", 44, 23, SkinVariationType.Dark, "man in lotus position", new string[4] { "meditation", "person in lotus position", "yoga", "man in lotus position" });

	public static readonly Emoji Bath = new Emoji("1F6C0", "BATH", "\ud83d\udec0", "bath", new string[1] { "bath" }, null, null, "Smileys & People", 36, 36, SkinVariationType.None, "person taking bath", new string[3] { "bath", "bathtub", "person taking bath" });

	public static readonly Emoji Bath_Light = new Emoji("1F6C0-1F3FB", "BATH", "\ud83d\udec0\ud83c\udffb", "bath", new string[1] { "bath" }, null, null, "Smileys & People", 36, 37, SkinVariationType.Light, "person taking bath", new string[3] { "bath", "bathtub", "person taking bath" });

	public static readonly Emoji Bath_MediumLight = new Emoji("1F6C0-1F3FC", "BATH", "\ud83d\udec0\ud83c\udffc", "bath", new string[1] { "bath" }, null, null, "Smileys & People", 36, 38, SkinVariationType.MediumLight, "person taking bath", new string[3] { "bath", "bathtub", "person taking bath" });

	public static readonly Emoji Bath_Medium = new Emoji("1F6C0-1F3FD", "BATH", "\ud83d\udec0\ud83c\udffd", "bath", new string[1] { "bath" }, null, null, "Smileys & People", 36, 39, SkinVariationType.Medium, "person taking bath", new string[3] { "bath", "bathtub", "person taking bath" });

	public static readonly Emoji Bath_MediumDark = new Emoji("1F6C0-1F3FE", "BATH", "\ud83d\udec0\ud83c\udffe", "bath", new string[1] { "bath" }, null, null, "Smileys & People", 36, 40, SkinVariationType.MediumDark, "person taking bath", new string[3] { "bath", "bathtub", "person taking bath" });

	public static readonly Emoji Bath_Dark = new Emoji("1F6C0-1F3FF", "BATH", "\ud83d\udec0\ud83c\udfff", "bath", new string[1] { "bath" }, null, null, "Smileys & People", 36, 41, SkinVariationType.Dark, "person taking bath", new string[3] { "bath", "bathtub", "person taking bath" });

	public static readonly Emoji SleepingAccommodation = new Emoji("1F6CC", "SLEEPING ACCOMMODATION", "\ud83d\udecc", "sleeping_accommodation", new string[1] { "sleeping_accommodation" }, null, null, "Smileys & People", 36, 48, SkinVariationType.None, "person in bed", new string[3] { "hotel", "person in bed", "sleep" });

	public static readonly Emoji SleepingAccommodation_Light = new Emoji("1F6CC-1F3FB", "SLEEPING ACCOMMODATION", "\ud83d\udecc\ud83c\udffb", "sleeping_accommodation", new string[1] { "sleeping_accommodation" }, null, null, "Smileys & People", 36, 49, SkinVariationType.Light, "person in bed", new string[3] { "hotel", "person in bed", "sleep" });

	public static readonly Emoji SleepingAccommodation_MediumLight = new Emoji("1F6CC-1F3FC", "SLEEPING ACCOMMODATION", "\ud83d\udecc\ud83c\udffc", "sleeping_accommodation", new string[1] { "sleeping_accommodation" }, null, null, "Smileys & People", 36, 50, SkinVariationType.MediumLight, "person in bed", new string[3] { "hotel", "person in bed", "sleep" });

	public static readonly Emoji SleepingAccommodation_Medium = new Emoji("1F6CC-1F3FD", "SLEEPING ACCOMMODATION", "\ud83d\udecc\ud83c\udffd", "sleeping_accommodation", new string[1] { "sleeping_accommodation" }, null, null, "Smileys & People", 36, 51, SkinVariationType.Medium, "person in bed", new string[3] { "hotel", "person in bed", "sleep" });

	public static readonly Emoji SleepingAccommodation_MediumDark = new Emoji("1F6CC-1F3FE", "SLEEPING ACCOMMODATION", "\ud83d\udecc\ud83c\udffe", "sleeping_accommodation", new string[1] { "sleeping_accommodation" }, null, null, "Smileys & People", 37, 0, SkinVariationType.MediumDark, "person in bed", new string[3] { "hotel", "person in bed", "sleep" });

	public static readonly Emoji SleepingAccommodation_Dark = new Emoji("1F6CC-1F3FF", "SLEEPING ACCOMMODATION", "\ud83d\udecc\ud83c\udfff", "sleeping_accommodation", new string[1] { "sleeping_accommodation" }, null, null, "Smileys & People", 37, 1, SkinVariationType.Dark, "person in bed", new string[3] { "hotel", "person in bed", "sleep" });

	public static readonly Emoji ManInBusinessSuitLevitating = new Emoji("1F574-FE0F", null, "\ud83d\udd74\ufe0f", "man_in_business_suit_levitating", new string[1] { "man_in_business_suit_levitating" }, null, null, "Smileys & People", 28, 45, SkinVariationType.None, null, null);

	public static readonly Emoji ManInBusinessSuitLevitating_Light = new Emoji("1F574-1F3FB", null, "\ud83d\udd74\ud83c\udffb", "man_in_business_suit_levitating", new string[1] { "man_in_business_suit_levitating" }, null, null, "Smileys & People", 28, 46, SkinVariationType.Light, null, null);

	public static readonly Emoji ManInBusinessSuitLevitating_MediumLight = new Emoji("1F574-1F3FC", null, "\ud83d\udd74\ud83c\udffc", "man_in_business_suit_levitating", new string[1] { "man_in_business_suit_levitating" }, null, null, "Smileys & People", 28, 47, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji ManInBusinessSuitLevitating_Medium = new Emoji("1F574-1F3FD", null, "\ud83d\udd74\ud83c\udffd", "man_in_business_suit_levitating", new string[1] { "man_in_business_suit_levitating" }, null, null, "Smileys & People", 28, 48, SkinVariationType.Medium, null, null);

	public static readonly Emoji ManInBusinessSuitLevitating_MediumDark = new Emoji("1F574-1F3FE", null, "\ud83d\udd74\ud83c\udffe", "man_in_business_suit_levitating", new string[1] { "man_in_business_suit_levitating" }, null, null, "Smileys & People", 28, 49, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji ManInBusinessSuitLevitating_Dark = new Emoji("1F574-1F3FF", null, "\ud83d\udd74\ud83c\udfff", "man_in_business_suit_levitating", new string[1] { "man_in_business_suit_levitating" }, null, null, "Smileys & People", 28, 50, SkinVariationType.Dark, null, null);

	public static readonly Emoji SpeakingHeadInSilhouette = new Emoji("1F5E3-FE0F", null, "\ud83d\udde3\ufe0f", "speaking_head_in_silhouette", new string[1] { "speaking_head_in_silhouette" }, null, null, "Smileys & People", 30, 14, SkinVariationType.None, null, null);

	public static readonly Emoji BustInSilhouette = new Emoji("1F464", "BUST IN SILHOUETTE", "\ud83d\udc64", "bust_in_silhouette", new string[1] { "bust_in_silhouette" }, null, null, "Smileys & People", 15, 40, SkinVariationType.None, "bust in silhouette", new string[3] { "bust", "bust in silhouette", "silhouette" });

	public static readonly Emoji BustsInSilhouette = new Emoji("1F465", "BUSTS IN SILHOUETTE", "\ud83d\udc65", "busts_in_silhouette", new string[1] { "busts_in_silhouette" }, null, null, "Smileys & People", 15, 41, SkinVariationType.None, "busts in silhouette", new string[3] { "bust", "busts in silhouette", "silhouette" });

	public static readonly Emoji Fencer = new Emoji("1F93A", "FENCER", "\ud83e\udd3a", "fencer", new string[1] { "fencer" }, null, null, "Smileys & People", 40, 48, SkinVariationType.None, "person fencing", new string[4] { "fencer", "fencing", "person fencing", "sword" });

	public static readonly Emoji HorseRacing = new Emoji("1F3C7", "HORSE RACING", "\ud83c\udfc7", "horse_racing", new string[1] { "horse_racing" }, null, null, "Smileys & People", 10, 20, SkinVariationType.None, "horse racing", new string[4] { "horse", "jockey", "racehorse", "racing" });

	public static readonly Emoji HorseRacing_Light = new Emoji("1F3C7-1F3FB", "HORSE RACING", "\ud83c\udfc7\ud83c\udffb", "horse_racing", new string[1] { "horse_racing" }, null, null, "Smileys & People", 10, 21, SkinVariationType.Light, "horse racing", new string[4] { "horse", "jockey", "racehorse", "racing" });

	public static readonly Emoji HorseRacing_MediumLight = new Emoji("1F3C7-1F3FC", "HORSE RACING", "\ud83c\udfc7\ud83c\udffc", "horse_racing", new string[1] { "horse_racing" }, null, null, "Smileys & People", 10, 22, SkinVariationType.MediumLight, "horse racing", new string[4] { "horse", "jockey", "racehorse", "racing" });

	public static readonly Emoji HorseRacing_Medium = new Emoji("1F3C7-1F3FD", "HORSE RACING", "\ud83c\udfc7\ud83c\udffd", "horse_racing", new string[1] { "horse_racing" }, null, null, "Smileys & People", 10, 23, SkinVariationType.Medium, "horse racing", new string[4] { "horse", "jockey", "racehorse", "racing" });

	public static readonly Emoji HorseRacing_MediumDark = new Emoji("1F3C7-1F3FE", "HORSE RACING", "\ud83c\udfc7\ud83c\udffe", "horse_racing", new string[1] { "horse_racing" }, null, null, "Smileys & People", 10, 24, SkinVariationType.MediumDark, "horse racing", new string[4] { "horse", "jockey", "racehorse", "racing" });

	public static readonly Emoji HorseRacing_Dark = new Emoji("1F3C7-1F3FF", "HORSE RACING", "\ud83c\udfc7\ud83c\udfff", "horse_racing", new string[1] { "horse_racing" }, null, null, "Smileys & People", 10, 25, SkinVariationType.Dark, "horse racing", new string[4] { "horse", "jockey", "racehorse", "racing" });

	public static readonly Emoji Skier = new Emoji("26F7-FE0F", null, "⛷\ufe0f", "skier", new string[1] { "skier" }, null, null, "Smileys & People", 48, 44, SkinVariationType.None, null, null);

	public static readonly Emoji Snowboarder = new Emoji("1F3C2", "SNOWBOARDER", "\ud83c\udfc2", "snowboarder", new string[1] { "snowboarder" }, null, null, "Smileys & People", 9, 28, SkinVariationType.None, "snowboarder", new string[4] { "ski", "snow", "snowboard", "snowboarder" });

	public static readonly Emoji Snowboarder_Light = new Emoji("1F3C2-1F3FB", "SNOWBOARDER", "\ud83c\udfc2\ud83c\udffb", "snowboarder", new string[1] { "snowboarder" }, null, null, "Smileys & People", 9, 29, SkinVariationType.Light, "snowboarder", new string[4] { "ski", "snow", "snowboard", "snowboarder" });

	public static readonly Emoji Snowboarder_MediumLight = new Emoji("1F3C2-1F3FC", "SNOWBOARDER", "\ud83c\udfc2\ud83c\udffc", "snowboarder", new string[1] { "snowboarder" }, null, null, "Smileys & People", 9, 30, SkinVariationType.MediumLight, "snowboarder", new string[4] { "ski", "snow", "snowboard", "snowboarder" });

	public static readonly Emoji Snowboarder_Medium = new Emoji("1F3C2-1F3FD", "SNOWBOARDER", "\ud83c\udfc2\ud83c\udffd", "snowboarder", new string[1] { "snowboarder" }, null, null, "Smileys & People", 9, 31, SkinVariationType.Medium, "snowboarder", new string[4] { "ski", "snow", "snowboard", "snowboarder" });

	public static readonly Emoji Snowboarder_MediumDark = new Emoji("1F3C2-1F3FE", "SNOWBOARDER", "\ud83c\udfc2\ud83c\udffe", "snowboarder", new string[1] { "snowboarder" }, null, null, "Smileys & People", 9, 32, SkinVariationType.MediumDark, "snowboarder", new string[4] { "ski", "snow", "snowboard", "snowboarder" });

	public static readonly Emoji Snowboarder_Dark = new Emoji("1F3C2-1F3FF", "SNOWBOARDER", "\ud83c\udfc2\ud83c\udfff", "snowboarder", new string[1] { "snowboarder" }, null, null, "Smileys & People", 9, 33, SkinVariationType.Dark, "snowboarder", new string[4] { "ski", "snow", "snowboard", "snowboarder" });

	public static readonly Emoji Golfer = new Emoji("1F3CC-FE0F", null, "\ud83c\udfcc\ufe0f", "golfer", new string[1] { "golfer" }, null, null, "Smileys & People", 11, 24, SkinVariationType.None, null, null);

	public static readonly Emoji Golfer_Light = new Emoji("1F3CC-1F3FB", null, "\ud83c\udfcc\ud83c\udffb", "golfer", new string[1] { "golfer" }, null, null, "Smileys & People", 11, 25, SkinVariationType.Light, null, null);

	public static readonly Emoji Golfer_MediumLight = new Emoji("1F3CC-1F3FC", null, "\ud83c\udfcc\ud83c\udffc", "golfer", new string[1] { "golfer" }, null, null, "Smileys & People", 11, 26, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji Golfer_Medium = new Emoji("1F3CC-1F3FD", null, "\ud83c\udfcc\ud83c\udffd", "golfer", new string[1] { "golfer" }, null, null, "Smileys & People", 11, 27, SkinVariationType.Medium, null, null);

	public static readonly Emoji Golfer_MediumDark = new Emoji("1F3CC-1F3FE", null, "\ud83c\udfcc\ud83c\udffe", "golfer", new string[1] { "golfer" }, null, null, "Smileys & People", 11, 28, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji Golfer_Dark = new Emoji("1F3CC-1F3FF", null, "\ud83c\udfcc\ud83c\udfff", "golfer", new string[1] { "golfer" }, null, null, "Smileys & People", 11, 29, SkinVariationType.Dark, null, null);

	public static readonly Emoji ManGolfing = new Emoji("1F3CC-FE0F-200D-2642-FE0F", null, "\ud83c\udfcc\ufe0f\u200d♂\ufe0f", "man-golfing", new string[1] { "man-golfing" }, null, null, "Smileys & People", 11, 18, SkinVariationType.None, null, null);

	public static readonly Emoji ManGolfing_Light = new Emoji("1F3CC-1F3FB-200D-2642-FE0F", null, "\ud83c\udfcc\ud83c\udffb\u200d♂\ufe0f", "man-golfing", new string[1] { "man-golfing" }, null, null, "Smileys & People", 11, 19, SkinVariationType.Light, null, null);

	public static readonly Emoji ManGolfing_MediumLight = new Emoji("1F3CC-1F3FC-200D-2642-FE0F", null, "\ud83c\udfcc\ud83c\udffc\u200d♂\ufe0f", "man-golfing", new string[1] { "man-golfing" }, null, null, "Smileys & People", 11, 20, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji ManGolfing_Medium = new Emoji("1F3CC-1F3FD-200D-2642-FE0F", null, "\ud83c\udfcc\ud83c\udffd\u200d♂\ufe0f", "man-golfing", new string[1] { "man-golfing" }, null, null, "Smileys & People", 11, 21, SkinVariationType.Medium, null, null);

	public static readonly Emoji ManGolfing_MediumDark = new Emoji("1F3CC-1F3FE-200D-2642-FE0F", null, "\ud83c\udfcc\ud83c\udffe\u200d♂\ufe0f", "man-golfing", new string[1] { "man-golfing" }, null, null, "Smileys & People", 11, 22, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji ManGolfing_Dark = new Emoji("1F3CC-1F3FF-200D-2642-FE0F", null, "\ud83c\udfcc\ud83c\udfff\u200d♂\ufe0f", "man-golfing", new string[1] { "man-golfing" }, null, null, "Smileys & People", 11, 23, SkinVariationType.Dark, null, null);

	public static readonly Emoji WomanGolfing = new Emoji("1F3CC-FE0F-200D-2640-FE0F", null, "\ud83c\udfcc\ufe0f\u200d♀\ufe0f", "woman-golfing", new string[1] { "woman-golfing" }, null, null, "Smileys & People", 11, 12, SkinVariationType.None, null, null);

	public static readonly Emoji WomanGolfing_Light = new Emoji("1F3CC-1F3FB-200D-2640-FE0F", null, "\ud83c\udfcc\ud83c\udffb\u200d♀\ufe0f", "woman-golfing", new string[1] { "woman-golfing" }, null, null, "Smileys & People", 11, 13, SkinVariationType.Light, null, null);

	public static readonly Emoji WomanGolfing_MediumLight = new Emoji("1F3CC-1F3FC-200D-2640-FE0F", null, "\ud83c\udfcc\ud83c\udffc\u200d♀\ufe0f", "woman-golfing", new string[1] { "woman-golfing" }, null, null, "Smileys & People", 11, 14, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji WomanGolfing_Medium = new Emoji("1F3CC-1F3FD-200D-2640-FE0F", null, "\ud83c\udfcc\ud83c\udffd\u200d♀\ufe0f", "woman-golfing", new string[1] { "woman-golfing" }, null, null, "Smileys & People", 11, 15, SkinVariationType.Medium, null, null);

	public static readonly Emoji WomanGolfing_MediumDark = new Emoji("1F3CC-1F3FE-200D-2640-FE0F", null, "\ud83c\udfcc\ud83c\udffe\u200d♀\ufe0f", "woman-golfing", new string[1] { "woman-golfing" }, null, null, "Smileys & People", 11, 16, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji WomanGolfing_Dark = new Emoji("1F3CC-1F3FF-200D-2640-FE0F", null, "\ud83c\udfcc\ud83c\udfff\u200d♀\ufe0f", "woman-golfing", new string[1] { "woman-golfing" }, null, null, "Smileys & People", 11, 17, SkinVariationType.Dark, null, null);

	public static readonly Emoji ManSurfing = new Emoji("1F3C4-200D-2642-FE0F", null, "\ud83c\udfc4\u200d♂\ufe0f", "man-surfing", new string[1] { "man-surfing" }, null, null, "Smileys & People", 10, 6, SkinVariationType.None, "man surfing", new string[3] { "person surfing", "surfing", "man" });

	public static readonly Emoji ManSurfing_Light = new Emoji("1F3C4-1F3FB-200D-2642-FE0F", null, "\ud83c\udfc4\ud83c\udffb\u200d♂\ufe0f", "man-surfing", new string[1] { "man-surfing" }, null, null, "Smileys & People", 10, 7, SkinVariationType.Light, "man surfing", new string[3] { "person surfing", "surfing", "man" });

	public static readonly Emoji ManSurfing_MediumLight = new Emoji("1F3C4-1F3FC-200D-2642-FE0F", null, "\ud83c\udfc4\ud83c\udffc\u200d♂\ufe0f", "man-surfing", new string[1] { "man-surfing" }, null, null, "Smileys & People", 10, 8, SkinVariationType.MediumLight, "man surfing", new string[3] { "person surfing", "surfing", "man" });

	public static readonly Emoji ManSurfing_Medium = new Emoji("1F3C4-1F3FD-200D-2642-FE0F", null, "\ud83c\udfc4\ud83c\udffd\u200d♂\ufe0f", "man-surfing", new string[1] { "man-surfing" }, null, null, "Smileys & People", 10, 9, SkinVariationType.Medium, "man surfing", new string[3] { "person surfing", "surfing", "man" });

	public static readonly Emoji ManSurfing_MediumDark = new Emoji("1F3C4-1F3FE-200D-2642-FE0F", null, "\ud83c\udfc4\ud83c\udffe\u200d♂\ufe0f", "man-surfing", new string[1] { "man-surfing" }, null, null, "Smileys & People", 10, 10, SkinVariationType.MediumDark, "man surfing", new string[3] { "person surfing", "surfing", "man" });

	public static readonly Emoji ManSurfing_Dark = new Emoji("1F3C4-1F3FF-200D-2642-FE0F", null, "\ud83c\udfc4\ud83c\udfff\u200d♂\ufe0f", "man-surfing", new string[1] { "man-surfing" }, null, null, "Smileys & People", 10, 11, SkinVariationType.Dark, "man surfing", new string[3] { "person surfing", "surfing", "man" });

	public static readonly Emoji WomanSurfing = new Emoji("1F3C4-200D-2640-FE0F", null, "\ud83c\udfc4\u200d♀\ufe0f", "woman-surfing", new string[1] { "woman-surfing" }, null, null, "Smileys & People", 10, 0, SkinVariationType.None, "woman surfing", new string[3] { "person surfing", "surfing", "woman" });

	public static readonly Emoji WomanSurfing_Light = new Emoji("1F3C4-1F3FB-200D-2640-FE0F", null, "\ud83c\udfc4\ud83c\udffb\u200d♀\ufe0f", "woman-surfing", new string[1] { "woman-surfing" }, null, null, "Smileys & People", 10, 1, SkinVariationType.Light, "woman surfing", new string[3] { "person surfing", "surfing", "woman" });

	public static readonly Emoji WomanSurfing_MediumLight = new Emoji("1F3C4-1F3FC-200D-2640-FE0F", null, "\ud83c\udfc4\ud83c\udffc\u200d♀\ufe0f", "woman-surfing", new string[1] { "woman-surfing" }, null, null, "Smileys & People", 10, 2, SkinVariationType.MediumLight, "woman surfing", new string[3] { "person surfing", "surfing", "woman" });

	public static readonly Emoji WomanSurfing_Medium = new Emoji("1F3C4-1F3FD-200D-2640-FE0F", null, "\ud83c\udfc4\ud83c\udffd\u200d♀\ufe0f", "woman-surfing", new string[1] { "woman-surfing" }, null, null, "Smileys & People", 10, 3, SkinVariationType.Medium, "woman surfing", new string[3] { "person surfing", "surfing", "woman" });

	public static readonly Emoji WomanSurfing_MediumDark = new Emoji("1F3C4-1F3FE-200D-2640-FE0F", null, "\ud83c\udfc4\ud83c\udffe\u200d♀\ufe0f", "woman-surfing", new string[1] { "woman-surfing" }, null, null, "Smileys & People", 10, 4, SkinVariationType.MediumDark, "woman surfing", new string[3] { "person surfing", "surfing", "woman" });

	public static readonly Emoji WomanSurfing_Dark = new Emoji("1F3C4-1F3FF-200D-2640-FE0F", null, "\ud83c\udfc4\ud83c\udfff\u200d♀\ufe0f", "woman-surfing", new string[1] { "woman-surfing" }, null, null, "Smileys & People", 10, 5, SkinVariationType.Dark, "woman surfing", new string[3] { "person surfing", "surfing", "woman" });

	public static readonly Emoji ManRowingBoat = new Emoji("1F6A3-200D-2642-FE0F", null, "\ud83d\udea3\u200d♂\ufe0f", "man-rowing-boat", new string[1] { "man-rowing-boat" }, null, null, "Smileys & People", 34, 49, SkinVariationType.None, "man rowing boat", new string[5] { "boat", "person rowing boat", "rowboat", "man", "man rowing boat" });

	public static readonly Emoji ManRowingBoat_Light = new Emoji("1F6A3-1F3FB-200D-2642-FE0F", null, "\ud83d\udea3\ud83c\udffb\u200d♂\ufe0f", "man-rowing-boat", new string[1] { "man-rowing-boat" }, null, null, "Smileys & People", 34, 50, SkinVariationType.Light, "man rowing boat", new string[5] { "boat", "person rowing boat", "rowboat", "man", "man rowing boat" });

	public static readonly Emoji ManRowingBoat_MediumLight = new Emoji("1F6A3-1F3FC-200D-2642-FE0F", null, "\ud83d\udea3\ud83c\udffc\u200d♂\ufe0f", "man-rowing-boat", new string[1] { "man-rowing-boat" }, null, null, "Smileys & People", 34, 51, SkinVariationType.MediumLight, "man rowing boat", new string[5] { "boat", "person rowing boat", "rowboat", "man", "man rowing boat" });

	public static readonly Emoji ManRowingBoat_Medium = new Emoji("1F6A3-1F3FD-200D-2642-FE0F", null, "\ud83d\udea3\ud83c\udffd\u200d♂\ufe0f", "man-rowing-boat", new string[1] { "man-rowing-boat" }, null, null, "Smileys & People", 35, 0, SkinVariationType.Medium, "man rowing boat", new string[5] { "boat", "person rowing boat", "rowboat", "man", "man rowing boat" });

	public static readonly Emoji ManRowingBoat_MediumDark = new Emoji("1F6A3-1F3FE-200D-2642-FE0F", null, "\ud83d\udea3\ud83c\udffe\u200d♂\ufe0f", "man-rowing-boat", new string[1] { "man-rowing-boat" }, null, null, "Smileys & People", 35, 1, SkinVariationType.MediumDark, "man rowing boat", new string[5] { "boat", "person rowing boat", "rowboat", "man", "man rowing boat" });

	public static readonly Emoji ManRowingBoat_Dark = new Emoji("1F6A3-1F3FF-200D-2642-FE0F", null, "\ud83d\udea3\ud83c\udfff\u200d♂\ufe0f", "man-rowing-boat", new string[1] { "man-rowing-boat" }, null, null, "Smileys & People", 35, 2, SkinVariationType.Dark, "man rowing boat", new string[5] { "boat", "person rowing boat", "rowboat", "man", "man rowing boat" });

	public static readonly Emoji WomanRowingBoat = new Emoji("1F6A3-200D-2640-FE0F", null, "\ud83d\udea3\u200d♀\ufe0f", "woman-rowing-boat", new string[1] { "woman-rowing-boat" }, null, null, "Smileys & People", 34, 43, SkinVariationType.None, "woman rowing boat", new string[5] { "boat", "person rowing boat", "rowboat", "woman", "woman rowing boat" });

	public static readonly Emoji WomanRowingBoat_Light = new Emoji("1F6A3-1F3FB-200D-2640-FE0F", null, "\ud83d\udea3\ud83c\udffb\u200d♀\ufe0f", "woman-rowing-boat", new string[1] { "woman-rowing-boat" }, null, null, "Smileys & People", 34, 44, SkinVariationType.Light, "woman rowing boat", new string[5] { "boat", "person rowing boat", "rowboat", "woman", "woman rowing boat" });

	public static readonly Emoji WomanRowingBoat_MediumLight = new Emoji("1F6A3-1F3FC-200D-2640-FE0F", null, "\ud83d\udea3\ud83c\udffc\u200d♀\ufe0f", "woman-rowing-boat", new string[1] { "woman-rowing-boat" }, null, null, "Smileys & People", 34, 45, SkinVariationType.MediumLight, "woman rowing boat", new string[5] { "boat", "person rowing boat", "rowboat", "woman", "woman rowing boat" });

	public static readonly Emoji WomanRowingBoat_Medium = new Emoji("1F6A3-1F3FD-200D-2640-FE0F", null, "\ud83d\udea3\ud83c\udffd\u200d♀\ufe0f", "woman-rowing-boat", new string[1] { "woman-rowing-boat" }, null, null, "Smileys & People", 34, 46, SkinVariationType.Medium, "woman rowing boat", new string[5] { "boat", "person rowing boat", "rowboat", "woman", "woman rowing boat" });

	public static readonly Emoji WomanRowingBoat_MediumDark = new Emoji("1F6A3-1F3FE-200D-2640-FE0F", null, "\ud83d\udea3\ud83c\udffe\u200d♀\ufe0f", "woman-rowing-boat", new string[1] { "woman-rowing-boat" }, null, null, "Smileys & People", 34, 47, SkinVariationType.MediumDark, "woman rowing boat", new string[5] { "boat", "person rowing boat", "rowboat", "woman", "woman rowing boat" });

	public static readonly Emoji WomanRowingBoat_Dark = new Emoji("1F6A3-1F3FF-200D-2640-FE0F", null, "\ud83d\udea3\ud83c\udfff\u200d♀\ufe0f", "woman-rowing-boat", new string[1] { "woman-rowing-boat" }, null, null, "Smileys & People", 34, 48, SkinVariationType.Dark, "woman rowing boat", new string[5] { "boat", "person rowing boat", "rowboat", "woman", "woman rowing boat" });

	public static readonly Emoji ManSwimming = new Emoji("1F3CA-200D-2642-FE0F", null, "\ud83c\udfca\u200d♂\ufe0f", "man-swimming", new string[1] { "man-swimming" }, null, null, "Smileys & People", 10, 34, SkinVariationType.None, "man swimming", new string[4] { "person swimming", "swim", "man", "man swimming" });

	public static readonly Emoji ManSwimming_Light = new Emoji("1F3CA-1F3FB-200D-2642-FE0F", null, "\ud83c\udfca\ud83c\udffb\u200d♂\ufe0f", "man-swimming", new string[1] { "man-swimming" }, null, null, "Smileys & People", 10, 35, SkinVariationType.Light, "man swimming", new string[4] { "person swimming", "swim", "man", "man swimming" });

	public static readonly Emoji ManSwimming_MediumLight = new Emoji("1F3CA-1F3FC-200D-2642-FE0F", null, "\ud83c\udfca\ud83c\udffc\u200d♂\ufe0f", "man-swimming", new string[1] { "man-swimming" }, null, null, "Smileys & People", 10, 36, SkinVariationType.MediumLight, "man swimming", new string[4] { "person swimming", "swim", "man", "man swimming" });

	public static readonly Emoji ManSwimming_Medium = new Emoji("1F3CA-1F3FD-200D-2642-FE0F", null, "\ud83c\udfca\ud83c\udffd\u200d♂\ufe0f", "man-swimming", new string[1] { "man-swimming" }, null, null, "Smileys & People", 10, 37, SkinVariationType.Medium, "man swimming", new string[4] { "person swimming", "swim", "man", "man swimming" });

	public static readonly Emoji ManSwimming_MediumDark = new Emoji("1F3CA-1F3FE-200D-2642-FE0F", null, "\ud83c\udfca\ud83c\udffe\u200d♂\ufe0f", "man-swimming", new string[1] { "man-swimming" }, null, null, "Smileys & People", 10, 38, SkinVariationType.MediumDark, "man swimming", new string[4] { "person swimming", "swim", "man", "man swimming" });

	public static readonly Emoji ManSwimming_Dark = new Emoji("1F3CA-1F3FF-200D-2642-FE0F", null, "\ud83c\udfca\ud83c\udfff\u200d♂\ufe0f", "man-swimming", new string[1] { "man-swimming" }, null, null, "Smileys & People", 10, 39, SkinVariationType.Dark, "man swimming", new string[4] { "person swimming", "swim", "man", "man swimming" });

	public static readonly Emoji WomanSwimming = new Emoji("1F3CA-200D-2640-FE0F", null, "\ud83c\udfca\u200d♀\ufe0f", "woman-swimming", new string[1] { "woman-swimming" }, null, null, "Smileys & People", 10, 28, SkinVariationType.None, "woman swimming", new string[4] { "person swimming", "swim", "woman", "woman swimming" });

	public static readonly Emoji WomanSwimming_Light = new Emoji("1F3CA-1F3FB-200D-2640-FE0F", null, "\ud83c\udfca\ud83c\udffb\u200d♀\ufe0f", "woman-swimming", new string[1] { "woman-swimming" }, null, null, "Smileys & People", 10, 29, SkinVariationType.Light, "woman swimming", new string[4] { "person swimming", "swim", "woman", "woman swimming" });

	public static readonly Emoji WomanSwimming_MediumLight = new Emoji("1F3CA-1F3FC-200D-2640-FE0F", null, "\ud83c\udfca\ud83c\udffc\u200d♀\ufe0f", "woman-swimming", new string[1] { "woman-swimming" }, null, null, "Smileys & People", 10, 30, SkinVariationType.MediumLight, "woman swimming", new string[4] { "person swimming", "swim", "woman", "woman swimming" });

	public static readonly Emoji WomanSwimming_Medium = new Emoji("1F3CA-1F3FD-200D-2640-FE0F", null, "\ud83c\udfca\ud83c\udffd\u200d♀\ufe0f", "woman-swimming", new string[1] { "woman-swimming" }, null, null, "Smileys & People", 10, 31, SkinVariationType.Medium, "woman swimming", new string[4] { "person swimming", "swim", "woman", "woman swimming" });

	public static readonly Emoji WomanSwimming_MediumDark = new Emoji("1F3CA-1F3FE-200D-2640-FE0F", null, "\ud83c\udfca\ud83c\udffe\u200d♀\ufe0f", "woman-swimming", new string[1] { "woman-swimming" }, null, null, "Smileys & People", 10, 32, SkinVariationType.MediumDark, "woman swimming", new string[4] { "person swimming", "swim", "woman", "woman swimming" });

	public static readonly Emoji WomanSwimming_Dark = new Emoji("1F3CA-1F3FF-200D-2640-FE0F", null, "\ud83c\udfca\ud83c\udfff\u200d♀\ufe0f", "woman-swimming", new string[1] { "woman-swimming" }, null, null, "Smileys & People", 10, 33, SkinVariationType.Dark, "woman swimming", new string[4] { "person swimming", "swim", "woman", "woman swimming" });

	public static readonly Emoji PersonWithBall = new Emoji("26F9-FE0F", null, "⛹\ufe0f", "person_with_ball", new string[1] { "person_with_ball" }, null, null, "Smileys & People", 49, 6, SkinVariationType.None, null, null);

	public static readonly Emoji PersonWithBall_Light = new Emoji("26F9-1F3FB", null, "⛹\ud83c\udffb", "person_with_ball", new string[1] { "person_with_ball" }, null, null, "Smileys & People", 49, 7, SkinVariationType.Light, null, null);

	public static readonly Emoji PersonWithBall_MediumLight = new Emoji("26F9-1F3FC", null, "⛹\ud83c\udffc", "person_with_ball", new string[1] { "person_with_ball" }, null, null, "Smileys & People", 49, 8, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji PersonWithBall_Medium = new Emoji("26F9-1F3FD", null, "⛹\ud83c\udffd", "person_with_ball", new string[1] { "person_with_ball" }, null, null, "Smileys & People", 49, 9, SkinVariationType.Medium, null, null);

	public static readonly Emoji PersonWithBall_MediumDark = new Emoji("26F9-1F3FE", null, "⛹\ud83c\udffe", "person_with_ball", new string[1] { "person_with_ball" }, null, null, "Smileys & People", 49, 10, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji PersonWithBall_Dark = new Emoji("26F9-1F3FF", null, "⛹\ud83c\udfff", "person_with_ball", new string[1] { "person_with_ball" }, null, null, "Smileys & People", 49, 11, SkinVariationType.Dark, null, null);

	public static readonly Emoji ManBouncingBall = new Emoji("26F9-FE0F-200D-2642-FE0F", null, "⛹\ufe0f\u200d♂\ufe0f", "man-bouncing-ball", new string[1] { "man-bouncing-ball" }, null, null, "Smileys & People", 49, 0, SkinVariationType.None, null, null);

	public static readonly Emoji ManBouncingBall_Light = new Emoji("26F9-1F3FB-200D-2642-FE0F", null, "⛹\ud83c\udffb\u200d♂\ufe0f", "man-bouncing-ball", new string[1] { "man-bouncing-ball" }, null, null, "Smileys & People", 49, 1, SkinVariationType.Light, null, null);

	public static readonly Emoji ManBouncingBall_MediumLight = new Emoji("26F9-1F3FC-200D-2642-FE0F", null, "⛹\ud83c\udffc\u200d♂\ufe0f", "man-bouncing-ball", new string[1] { "man-bouncing-ball" }, null, null, "Smileys & People", 49, 2, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji ManBouncingBall_Medium = new Emoji("26F9-1F3FD-200D-2642-FE0F", null, "⛹\ud83c\udffd\u200d♂\ufe0f", "man-bouncing-ball", new string[1] { "man-bouncing-ball" }, null, null, "Smileys & People", 49, 3, SkinVariationType.Medium, null, null);

	public static readonly Emoji ManBouncingBall_MediumDark = new Emoji("26F9-1F3FE-200D-2642-FE0F", null, "⛹\ud83c\udffe\u200d♂\ufe0f", "man-bouncing-ball", new string[1] { "man-bouncing-ball" }, null, null, "Smileys & People", 49, 4, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji ManBouncingBall_Dark = new Emoji("26F9-1F3FF-200D-2642-FE0F", null, "⛹\ud83c\udfff\u200d♂\ufe0f", "man-bouncing-ball", new string[1] { "man-bouncing-ball" }, null, null, "Smileys & People", 49, 5, SkinVariationType.Dark, null, null);

	public static readonly Emoji WomanBouncingBall = new Emoji("26F9-FE0F-200D-2640-FE0F", null, "⛹\ufe0f\u200d♀\ufe0f", "woman-bouncing-ball", new string[1] { "woman-bouncing-ball" }, null, null, "Smileys & People", 48, 46, SkinVariationType.None, null, null);

	public static readonly Emoji WomanBouncingBall_Light = new Emoji("26F9-1F3FB-200D-2640-FE0F", null, "⛹\ud83c\udffb\u200d♀\ufe0f", "woman-bouncing-ball", new string[1] { "woman-bouncing-ball" }, null, null, "Smileys & People", 48, 47, SkinVariationType.Light, null, null);

	public static readonly Emoji WomanBouncingBall_MediumLight = new Emoji("26F9-1F3FC-200D-2640-FE0F", null, "⛹\ud83c\udffc\u200d♀\ufe0f", "woman-bouncing-ball", new string[1] { "woman-bouncing-ball" }, null, null, "Smileys & People", 48, 48, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji WomanBouncingBall_Medium = new Emoji("26F9-1F3FD-200D-2640-FE0F", null, "⛹\ud83c\udffd\u200d♀\ufe0f", "woman-bouncing-ball", new string[1] { "woman-bouncing-ball" }, null, null, "Smileys & People", 48, 49, SkinVariationType.Medium, null, null);

	public static readonly Emoji WomanBouncingBall_MediumDark = new Emoji("26F9-1F3FE-200D-2640-FE0F", null, "⛹\ud83c\udffe\u200d♀\ufe0f", "woman-bouncing-ball", new string[1] { "woman-bouncing-ball" }, null, null, "Smileys & People", 48, 50, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji WomanBouncingBall_Dark = new Emoji("26F9-1F3FF-200D-2640-FE0F", null, "⛹\ud83c\udfff\u200d♀\ufe0f", "woman-bouncing-ball", new string[1] { "woman-bouncing-ball" }, null, null, "Smileys & People", 48, 51, SkinVariationType.Dark, null, null);

	public static readonly Emoji WeightLifter = new Emoji("1F3CB-FE0F", null, "\ud83c\udfcb\ufe0f", "weight_lifter", new string[1] { "weight_lifter" }, null, null, "Smileys & People", 11, 6, SkinVariationType.None, null, null);

	public static readonly Emoji WeightLifter_Light = new Emoji("1F3CB-1F3FB", null, "\ud83c\udfcb\ud83c\udffb", "weight_lifter", new string[1] { "weight_lifter" }, null, null, "Smileys & People", 11, 7, SkinVariationType.Light, null, null);

	public static readonly Emoji WeightLifter_MediumLight = new Emoji("1F3CB-1F3FC", null, "\ud83c\udfcb\ud83c\udffc", "weight_lifter", new string[1] { "weight_lifter" }, null, null, "Smileys & People", 11, 8, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji WeightLifter_Medium = new Emoji("1F3CB-1F3FD", null, "\ud83c\udfcb\ud83c\udffd", "weight_lifter", new string[1] { "weight_lifter" }, null, null, "Smileys & People", 11, 9, SkinVariationType.Medium, null, null);

	public static readonly Emoji WeightLifter_MediumDark = new Emoji("1F3CB-1F3FE", null, "\ud83c\udfcb\ud83c\udffe", "weight_lifter", new string[1] { "weight_lifter" }, null, null, "Smileys & People", 11, 10, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji WeightLifter_Dark = new Emoji("1F3CB-1F3FF", null, "\ud83c\udfcb\ud83c\udfff", "weight_lifter", new string[1] { "weight_lifter" }, null, null, "Smileys & People", 11, 11, SkinVariationType.Dark, null, null);

	public static readonly Emoji ManLiftingWeights = new Emoji("1F3CB-FE0F-200D-2642-FE0F", null, "\ud83c\udfcb\ufe0f\u200d♂\ufe0f", "man-lifting-weights", new string[1] { "man-lifting-weights" }, null, null, "Smileys & People", 11, 0, SkinVariationType.None, null, null);

	public static readonly Emoji ManLiftingWeights_Light = new Emoji("1F3CB-1F3FB-200D-2642-FE0F", null, "\ud83c\udfcb\ud83c\udffb\u200d♂\ufe0f", "man-lifting-weights", new string[1] { "man-lifting-weights" }, null, null, "Smileys & People", 11, 1, SkinVariationType.Light, null, null);

	public static readonly Emoji ManLiftingWeights_MediumLight = new Emoji("1F3CB-1F3FC-200D-2642-FE0F", null, "\ud83c\udfcb\ud83c\udffc\u200d♂\ufe0f", "man-lifting-weights", new string[1] { "man-lifting-weights" }, null, null, "Smileys & People", 11, 2, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji ManLiftingWeights_Medium = new Emoji("1F3CB-1F3FD-200D-2642-FE0F", null, "\ud83c\udfcb\ud83c\udffd\u200d♂\ufe0f", "man-lifting-weights", new string[1] { "man-lifting-weights" }, null, null, "Smileys & People", 11, 3, SkinVariationType.Medium, null, null);

	public static readonly Emoji ManLiftingWeights_MediumDark = new Emoji("1F3CB-1F3FE-200D-2642-FE0F", null, "\ud83c\udfcb\ud83c\udffe\u200d♂\ufe0f", "man-lifting-weights", new string[1] { "man-lifting-weights" }, null, null, "Smileys & People", 11, 4, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji ManLiftingWeights_Dark = new Emoji("1F3CB-1F3FF-200D-2642-FE0F", null, "\ud83c\udfcb\ud83c\udfff\u200d♂\ufe0f", "man-lifting-weights", new string[1] { "man-lifting-weights" }, null, null, "Smileys & People", 11, 5, SkinVariationType.Dark, null, null);

	public static readonly Emoji WomanLiftingWeights = new Emoji("1F3CB-FE0F-200D-2640-FE0F", null, "\ud83c\udfcb\ufe0f\u200d♀\ufe0f", "woman-lifting-weights", new string[1] { "woman-lifting-weights" }, null, null, "Smileys & People", 10, 46, SkinVariationType.None, null, null);

	public static readonly Emoji WomanLiftingWeights_Light = new Emoji("1F3CB-1F3FB-200D-2640-FE0F", null, "\ud83c\udfcb\ud83c\udffb\u200d♀\ufe0f", "woman-lifting-weights", new string[1] { "woman-lifting-weights" }, null, null, "Smileys & People", 10, 47, SkinVariationType.Light, null, null);

	public static readonly Emoji WomanLiftingWeights_MediumLight = new Emoji("1F3CB-1F3FC-200D-2640-FE0F", null, "\ud83c\udfcb\ud83c\udffc\u200d♀\ufe0f", "woman-lifting-weights", new string[1] { "woman-lifting-weights" }, null, null, "Smileys & People", 10, 48, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji WomanLiftingWeights_Medium = new Emoji("1F3CB-1F3FD-200D-2640-FE0F", null, "\ud83c\udfcb\ud83c\udffd\u200d♀\ufe0f", "woman-lifting-weights", new string[1] { "woman-lifting-weights" }, null, null, "Smileys & People", 10, 49, SkinVariationType.Medium, null, null);

	public static readonly Emoji WomanLiftingWeights_MediumDark = new Emoji("1F3CB-1F3FE-200D-2640-FE0F", null, "\ud83c\udfcb\ud83c\udffe\u200d♀\ufe0f", "woman-lifting-weights", new string[1] { "woman-lifting-weights" }, null, null, "Smileys & People", 10, 50, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji WomanLiftingWeights_Dark = new Emoji("1F3CB-1F3FF-200D-2640-FE0F", null, "\ud83c\udfcb\ud83c\udfff\u200d♀\ufe0f", "woman-lifting-weights", new string[1] { "woman-lifting-weights" }, null, null, "Smileys & People", 10, 51, SkinVariationType.Dark, null, null);

	public static readonly Emoji ManBiking = new Emoji("1F6B4-200D-2642-FE0F", null, "\ud83d\udeb4\u200d♂\ufe0f", "man-biking", new string[1] { "man-biking" }, null, null, "Smileys & People", 35, 31, SkinVariationType.None, "man biking", new string[5] { "bicycle", "biking", "cyclist", "person biking", "man" });

	public static readonly Emoji ManBiking_Light = new Emoji("1F6B4-1F3FB-200D-2642-FE0F", null, "\ud83d\udeb4\ud83c\udffb\u200d♂\ufe0f", "man-biking", new string[1] { "man-biking" }, null, null, "Smileys & People", 35, 32, SkinVariationType.Light, "man biking", new string[5] { "bicycle", "biking", "cyclist", "person biking", "man" });

	public static readonly Emoji ManBiking_MediumLight = new Emoji("1F6B4-1F3FC-200D-2642-FE0F", null, "\ud83d\udeb4\ud83c\udffc\u200d♂\ufe0f", "man-biking", new string[1] { "man-biking" }, null, null, "Smileys & People", 35, 33, SkinVariationType.MediumLight, "man biking", new string[5] { "bicycle", "biking", "cyclist", "person biking", "man" });

	public static readonly Emoji ManBiking_Medium = new Emoji("1F6B4-1F3FD-200D-2642-FE0F", null, "\ud83d\udeb4\ud83c\udffd\u200d♂\ufe0f", "man-biking", new string[1] { "man-biking" }, null, null, "Smileys & People", 35, 34, SkinVariationType.Medium, "man biking", new string[5] { "bicycle", "biking", "cyclist", "person biking", "man" });

	public static readonly Emoji ManBiking_MediumDark = new Emoji("1F6B4-1F3FE-200D-2642-FE0F", null, "\ud83d\udeb4\ud83c\udffe\u200d♂\ufe0f", "man-biking", new string[1] { "man-biking" }, null, null, "Smileys & People", 35, 35, SkinVariationType.MediumDark, "man biking", new string[5] { "bicycle", "biking", "cyclist", "person biking", "man" });

	public static readonly Emoji ManBiking_Dark = new Emoji("1F6B4-1F3FF-200D-2642-FE0F", null, "\ud83d\udeb4\ud83c\udfff\u200d♂\ufe0f", "man-biking", new string[1] { "man-biking" }, null, null, "Smileys & People", 35, 36, SkinVariationType.Dark, "man biking", new string[5] { "bicycle", "biking", "cyclist", "person biking", "man" });

	public static readonly Emoji WomanBiking = new Emoji("1F6B4-200D-2640-FE0F", null, "\ud83d\udeb4\u200d♀\ufe0f", "woman-biking", new string[1] { "woman-biking" }, null, null, "Smileys & People", 35, 25, SkinVariationType.None, "woman biking", new string[5] { "bicycle", "biking", "cyclist", "person biking", "woman" });

	public static readonly Emoji WomanBiking_Light = new Emoji("1F6B4-1F3FB-200D-2640-FE0F", null, "\ud83d\udeb4\ud83c\udffb\u200d♀\ufe0f", "woman-biking", new string[1] { "woman-biking" }, null, null, "Smileys & People", 35, 26, SkinVariationType.Light, "woman biking", new string[5] { "bicycle", "biking", "cyclist", "person biking", "woman" });

	public static readonly Emoji WomanBiking_MediumLight = new Emoji("1F6B4-1F3FC-200D-2640-FE0F", null, "\ud83d\udeb4\ud83c\udffc\u200d♀\ufe0f", "woman-biking", new string[1] { "woman-biking" }, null, null, "Smileys & People", 35, 27, SkinVariationType.MediumLight, "woman biking", new string[5] { "bicycle", "biking", "cyclist", "person biking", "woman" });

	public static readonly Emoji WomanBiking_Medium = new Emoji("1F6B4-1F3FD-200D-2640-FE0F", null, "\ud83d\udeb4\ud83c\udffd\u200d♀\ufe0f", "woman-biking", new string[1] { "woman-biking" }, null, null, "Smileys & People", 35, 28, SkinVariationType.Medium, "woman biking", new string[5] { "bicycle", "biking", "cyclist", "person biking", "woman" });

	public static readonly Emoji WomanBiking_MediumDark = new Emoji("1F6B4-1F3FE-200D-2640-FE0F", null, "\ud83d\udeb4\ud83c\udffe\u200d♀\ufe0f", "woman-biking", new string[1] { "woman-biking" }, null, null, "Smileys & People", 35, 29, SkinVariationType.MediumDark, "woman biking", new string[5] { "bicycle", "biking", "cyclist", "person biking", "woman" });

	public static readonly Emoji WomanBiking_Dark = new Emoji("1F6B4-1F3FF-200D-2640-FE0F", null, "\ud83d\udeb4\ud83c\udfff\u200d♀\ufe0f", "woman-biking", new string[1] { "woman-biking" }, null, null, "Smileys & People", 35, 30, SkinVariationType.Dark, "woman biking", new string[5] { "bicycle", "biking", "cyclist", "person biking", "woman" });

	public static readonly Emoji ManMountainBiking = new Emoji("1F6B5-200D-2642-FE0F", null, "\ud83d\udeb5\u200d♂\ufe0f", "man-mountain-biking", new string[1] { "man-mountain-biking" }, null, null, "Smileys & People", 35, 49, SkinVariationType.None, "man mountain biking", new string[8] { "bicycle", "bicyclist", "bike", "cyclist", "mountain", "person mountain biking", "man", "man mountain biking" });

	public static readonly Emoji ManMountainBiking_Light = new Emoji("1F6B5-1F3FB-200D-2642-FE0F", null, "\ud83d\udeb5\ud83c\udffb\u200d♂\ufe0f", "man-mountain-biking", new string[1] { "man-mountain-biking" }, null, null, "Smileys & People", 35, 50, SkinVariationType.Light, "man mountain biking", new string[8] { "bicycle", "bicyclist", "bike", "cyclist", "mountain", "person mountain biking", "man", "man mountain biking" });

	public static readonly Emoji ManMountainBiking_MediumLight = new Emoji("1F6B5-1F3FC-200D-2642-FE0F", null, "\ud83d\udeb5\ud83c\udffc\u200d♂\ufe0f", "man-mountain-biking", new string[1] { "man-mountain-biking" }, null, null, "Smileys & People", 35, 51, SkinVariationType.MediumLight, "man mountain biking", new string[8] { "bicycle", "bicyclist", "bike", "cyclist", "mountain", "person mountain biking", "man", "man mountain biking" });

	public static readonly Emoji ManMountainBiking_Medium = new Emoji("1F6B5-1F3FD-200D-2642-FE0F", null, "\ud83d\udeb5\ud83c\udffd\u200d♂\ufe0f", "man-mountain-biking", new string[1] { "man-mountain-biking" }, null, null, "Smileys & People", 36, 0, SkinVariationType.Medium, "man mountain biking", new string[8] { "bicycle", "bicyclist", "bike", "cyclist", "mountain", "person mountain biking", "man", "man mountain biking" });

	public static readonly Emoji ManMountainBiking_MediumDark = new Emoji("1F6B5-1F3FE-200D-2642-FE0F", null, "\ud83d\udeb5\ud83c\udffe\u200d♂\ufe0f", "man-mountain-biking", new string[1] { "man-mountain-biking" }, null, null, "Smileys & People", 36, 1, SkinVariationType.MediumDark, "man mountain biking", new string[8] { "bicycle", "bicyclist", "bike", "cyclist", "mountain", "person mountain biking", "man", "man mountain biking" });

	public static readonly Emoji ManMountainBiking_Dark = new Emoji("1F6B5-1F3FF-200D-2642-FE0F", null, "\ud83d\udeb5\ud83c\udfff\u200d♂\ufe0f", "man-mountain-biking", new string[1] { "man-mountain-biking" }, null, null, "Smileys & People", 36, 2, SkinVariationType.Dark, "man mountain biking", new string[8] { "bicycle", "bicyclist", "bike", "cyclist", "mountain", "person mountain biking", "man", "man mountain biking" });

	public static readonly Emoji WomanMountainBiking = new Emoji("1F6B5-200D-2640-FE0F", null, "\ud83d\udeb5\u200d♀\ufe0f", "woman-mountain-biking", new string[1] { "woman-mountain-biking" }, null, null, "Smileys & People", 35, 43, SkinVariationType.None, "woman mountain biking", new string[8] { "bicycle", "bicyclist", "bike", "cyclist", "mountain", "person mountain biking", "biking", "woman" });

	public static readonly Emoji WomanMountainBiking_Light = new Emoji("1F6B5-1F3FB-200D-2640-FE0F", null, "\ud83d\udeb5\ud83c\udffb\u200d♀\ufe0f", "woman-mountain-biking", new string[1] { "woman-mountain-biking" }, null, null, "Smileys & People", 35, 44, SkinVariationType.Light, "woman mountain biking", new string[8] { "bicycle", "bicyclist", "bike", "cyclist", "mountain", "person mountain biking", "biking", "woman" });

	public static readonly Emoji WomanMountainBiking_MediumLight = new Emoji("1F6B5-1F3FC-200D-2640-FE0F", null, "\ud83d\udeb5\ud83c\udffc\u200d♀\ufe0f", "woman-mountain-biking", new string[1] { "woman-mountain-biking" }, null, null, "Smileys & People", 35, 45, SkinVariationType.MediumLight, "woman mountain biking", new string[8] { "bicycle", "bicyclist", "bike", "cyclist", "mountain", "person mountain biking", "biking", "woman" });

	public static readonly Emoji WomanMountainBiking_Medium = new Emoji("1F6B5-1F3FD-200D-2640-FE0F", null, "\ud83d\udeb5\ud83c\udffd\u200d♀\ufe0f", "woman-mountain-biking", new string[1] { "woman-mountain-biking" }, null, null, "Smileys & People", 35, 46, SkinVariationType.Medium, "woman mountain biking", new string[8] { "bicycle", "bicyclist", "bike", "cyclist", "mountain", "person mountain biking", "biking", "woman" });

	public static readonly Emoji WomanMountainBiking_MediumDark = new Emoji("1F6B5-1F3FE-200D-2640-FE0F", null, "\ud83d\udeb5\ud83c\udffe\u200d♀\ufe0f", "woman-mountain-biking", new string[1] { "woman-mountain-biking" }, null, null, "Smileys & People", 35, 47, SkinVariationType.MediumDark, "woman mountain biking", new string[8] { "bicycle", "bicyclist", "bike", "cyclist", "mountain", "person mountain biking", "biking", "woman" });

	public static readonly Emoji WomanMountainBiking_Dark = new Emoji("1F6B5-1F3FF-200D-2640-FE0F", null, "\ud83d\udeb5\ud83c\udfff\u200d♀\ufe0f", "woman-mountain-biking", new string[1] { "woman-mountain-biking" }, null, null, "Smileys & People", 35, 48, SkinVariationType.Dark, "woman mountain biking", new string[8] { "bicycle", "bicyclist", "bike", "cyclist", "mountain", "person mountain biking", "biking", "woman" });

	public static readonly Emoji RacingCar = new Emoji("1F3CE-FE0F", null, "\ud83c\udfce\ufe0f", "racing_car", new string[1] { "racing_car" }, null, null, "Smileys & People", 11, 31, SkinVariationType.None, null, null);

	public static readonly Emoji RacingMotorcycle = new Emoji("1F3CD-FE0F", null, "\ud83c\udfcd\ufe0f", "racing_motorcycle", new string[1] { "racing_motorcycle" }, null, null, "Smileys & People", 11, 30, SkinVariationType.None, null, null);

	public static readonly Emoji ManCartwheeling = new Emoji("1F938-200D-2642-FE0F", null, "\ud83e\udd38\u200d♂\ufe0f", "man-cartwheeling", new string[1] { "man-cartwheeling" }, null, null, "Smileys & People", 40, 18, SkinVariationType.None, "man cartwheeling", new string[5] { "cartwheel", "gymnastics", "person cartwheeling", "man", "man cartwheeling" });

	public static readonly Emoji ManCartwheeling_Light = new Emoji("1F938-1F3FB-200D-2642-FE0F", null, "\ud83e\udd38\ud83c\udffb\u200d♂\ufe0f", "man-cartwheeling", new string[1] { "man-cartwheeling" }, null, null, "Smileys & People", 40, 19, SkinVariationType.Light, "man cartwheeling", new string[5] { "cartwheel", "gymnastics", "person cartwheeling", "man", "man cartwheeling" });

	public static readonly Emoji ManCartwheeling_MediumLight = new Emoji("1F938-1F3FC-200D-2642-FE0F", null, "\ud83e\udd38\ud83c\udffc\u200d♂\ufe0f", "man-cartwheeling", new string[1] { "man-cartwheeling" }, null, null, "Smileys & People", 40, 20, SkinVariationType.MediumLight, "man cartwheeling", new string[5] { "cartwheel", "gymnastics", "person cartwheeling", "man", "man cartwheeling" });

	public static readonly Emoji ManCartwheeling_Medium = new Emoji("1F938-1F3FD-200D-2642-FE0F", null, "\ud83e\udd38\ud83c\udffd\u200d♂\ufe0f", "man-cartwheeling", new string[1] { "man-cartwheeling" }, null, null, "Smileys & People", 40, 21, SkinVariationType.Medium, "man cartwheeling", new string[5] { "cartwheel", "gymnastics", "person cartwheeling", "man", "man cartwheeling" });

	public static readonly Emoji ManCartwheeling_MediumDark = new Emoji("1F938-1F3FE-200D-2642-FE0F", null, "\ud83e\udd38\ud83c\udffe\u200d♂\ufe0f", "man-cartwheeling", new string[1] { "man-cartwheeling" }, null, null, "Smileys & People", 40, 22, SkinVariationType.MediumDark, "man cartwheeling", new string[5] { "cartwheel", "gymnastics", "person cartwheeling", "man", "man cartwheeling" });

	public static readonly Emoji ManCartwheeling_Dark = new Emoji("1F938-1F3FF-200D-2642-FE0F", null, "\ud83e\udd38\ud83c\udfff\u200d♂\ufe0f", "man-cartwheeling", new string[1] { "man-cartwheeling" }, null, null, "Smileys & People", 40, 23, SkinVariationType.Dark, "man cartwheeling", new string[5] { "cartwheel", "gymnastics", "person cartwheeling", "man", "man cartwheeling" });

	public static readonly Emoji WomanCartwheeling = new Emoji("1F938-200D-2640-FE0F", null, "\ud83e\udd38\u200d♀\ufe0f", "woman-cartwheeling", new string[1] { "woman-cartwheeling" }, null, null, "Smileys & People", 40, 12, SkinVariationType.None, "woman cartwheeling", new string[5] { "cartwheel", "gymnastics", "person cartwheeling", "woman", "woman cartwheeling" });

	public static readonly Emoji WomanCartwheeling_Light = new Emoji("1F938-1F3FB-200D-2640-FE0F", null, "\ud83e\udd38\ud83c\udffb\u200d♀\ufe0f", "woman-cartwheeling", new string[1] { "woman-cartwheeling" }, null, null, "Smileys & People", 40, 13, SkinVariationType.Light, "woman cartwheeling", new string[5] { "cartwheel", "gymnastics", "person cartwheeling", "woman", "woman cartwheeling" });

	public static readonly Emoji WomanCartwheeling_MediumLight = new Emoji("1F938-1F3FC-200D-2640-FE0F", null, "\ud83e\udd38\ud83c\udffc\u200d♀\ufe0f", "woman-cartwheeling", new string[1] { "woman-cartwheeling" }, null, null, "Smileys & People", 40, 14, SkinVariationType.MediumLight, "woman cartwheeling", new string[5] { "cartwheel", "gymnastics", "person cartwheeling", "woman", "woman cartwheeling" });

	public static readonly Emoji WomanCartwheeling_Medium = new Emoji("1F938-1F3FD-200D-2640-FE0F", null, "\ud83e\udd38\ud83c\udffd\u200d♀\ufe0f", "woman-cartwheeling", new string[1] { "woman-cartwheeling" }, null, null, "Smileys & People", 40, 15, SkinVariationType.Medium, "woman cartwheeling", new string[5] { "cartwheel", "gymnastics", "person cartwheeling", "woman", "woman cartwheeling" });

	public static readonly Emoji WomanCartwheeling_MediumDark = new Emoji("1F938-1F3FE-200D-2640-FE0F", null, "\ud83e\udd38\ud83c\udffe\u200d♀\ufe0f", "woman-cartwheeling", new string[1] { "woman-cartwheeling" }, null, null, "Smileys & People", 40, 16, SkinVariationType.MediumDark, "woman cartwheeling", new string[5] { "cartwheel", "gymnastics", "person cartwheeling", "woman", "woman cartwheeling" });

	public static readonly Emoji WomanCartwheeling_Dark = new Emoji("1F938-1F3FF-200D-2640-FE0F", null, "\ud83e\udd38\ud83c\udfff\u200d♀\ufe0f", "woman-cartwheeling", new string[1] { "woman-cartwheeling" }, null, null, "Smileys & People", 40, 17, SkinVariationType.Dark, "woman cartwheeling", new string[5] { "cartwheel", "gymnastics", "person cartwheeling", "woman", "woman cartwheeling" });

	public static readonly Emoji ManWrestling = new Emoji("1F93C-200D-2642-FE0F", null, "\ud83e\udd3c\u200d♂\ufe0f", "man-wrestling", new string[1] { "man-wrestling" }, null, null, "Smileys & People", 40, 50, SkinVariationType.None, "men wrestling", new string[5] { "people wrestling", "wrestle", "wrestler", "men", "men wrestling" });

	public static readonly Emoji WomanWrestling = new Emoji("1F93C-200D-2640-FE0F", null, "\ud83e\udd3c\u200d♀\ufe0f", "woman-wrestling", new string[1] { "woman-wrestling" }, null, null, "Smileys & People", 40, 49, SkinVariationType.None, "women wrestling", new string[5] { "people wrestling", "wrestle", "wrestler", "women", "women wrestling" });

	public static readonly Emoji ManPlayingWaterPolo = new Emoji("1F93D-200D-2642-FE0F", null, "\ud83e\udd3d\u200d♂\ufe0f", "man-playing-water-polo", new string[1] { "man-playing-water-polo" }, null, null, "Smileys & People", 41, 6, SkinVariationType.None, "man playing water polo", new string[6] { "person playing water polo", "polo", "water", "man", "man playing water polo", "water polo" });

	public static readonly Emoji ManPlayingWaterPolo_Light = new Emoji("1F93D-1F3FB-200D-2642-FE0F", null, "\ud83e\udd3d\ud83c\udffb\u200d♂\ufe0f", "man-playing-water-polo", new string[1] { "man-playing-water-polo" }, null, null, "Smileys & People", 41, 7, SkinVariationType.Light, "man playing water polo", new string[6] { "person playing water polo", "polo", "water", "man", "man playing water polo", "water polo" });

	public static readonly Emoji ManPlayingWaterPolo_MediumLight = new Emoji("1F93D-1F3FC-200D-2642-FE0F", null, "\ud83e\udd3d\ud83c\udffc\u200d♂\ufe0f", "man-playing-water-polo", new string[1] { "man-playing-water-polo" }, null, null, "Smileys & People", 41, 8, SkinVariationType.MediumLight, "man playing water polo", new string[6] { "person playing water polo", "polo", "water", "man", "man playing water polo", "water polo" });

	public static readonly Emoji ManPlayingWaterPolo_Medium = new Emoji("1F93D-1F3FD-200D-2642-FE0F", null, "\ud83e\udd3d\ud83c\udffd\u200d♂\ufe0f", "man-playing-water-polo", new string[1] { "man-playing-water-polo" }, null, null, "Smileys & People", 41, 9, SkinVariationType.Medium, "man playing water polo", new string[6] { "person playing water polo", "polo", "water", "man", "man playing water polo", "water polo" });

	public static readonly Emoji ManPlayingWaterPolo_MediumDark = new Emoji("1F93D-1F3FE-200D-2642-FE0F", null, "\ud83e\udd3d\ud83c\udffe\u200d♂\ufe0f", "man-playing-water-polo", new string[1] { "man-playing-water-polo" }, null, null, "Smileys & People", 41, 10, SkinVariationType.MediumDark, "man playing water polo", new string[6] { "person playing water polo", "polo", "water", "man", "man playing water polo", "water polo" });

	public static readonly Emoji ManPlayingWaterPolo_Dark = new Emoji("1F93D-1F3FF-200D-2642-FE0F", null, "\ud83e\udd3d\ud83c\udfff\u200d♂\ufe0f", "man-playing-water-polo", new string[1] { "man-playing-water-polo" }, null, null, "Smileys & People", 41, 11, SkinVariationType.Dark, "man playing water polo", new string[6] { "person playing water polo", "polo", "water", "man", "man playing water polo", "water polo" });

	public static readonly Emoji WomanPlayingWaterPolo = new Emoji("1F93D-200D-2640-FE0F", null, "\ud83e\udd3d\u200d♀\ufe0f", "woman-playing-water-polo", new string[1] { "woman-playing-water-polo" }, null, null, "Smileys & People", 41, 0, SkinVariationType.None, "woman playing water polo", new string[6] { "person playing water polo", "polo", "water", "water polo", "woman", "woman playing water polo" });

	public static readonly Emoji WomanPlayingWaterPolo_Light = new Emoji("1F93D-1F3FB-200D-2640-FE0F", null, "\ud83e\udd3d\ud83c\udffb\u200d♀\ufe0f", "woman-playing-water-polo", new string[1] { "woman-playing-water-polo" }, null, null, "Smileys & People", 41, 1, SkinVariationType.Light, "woman playing water polo", new string[6] { "person playing water polo", "polo", "water", "water polo", "woman", "woman playing water polo" });

	public static readonly Emoji WomanPlayingWaterPolo_MediumLight = new Emoji("1F93D-1F3FC-200D-2640-FE0F", null, "\ud83e\udd3d\ud83c\udffc\u200d♀\ufe0f", "woman-playing-water-polo", new string[1] { "woman-playing-water-polo" }, null, null, "Smileys & People", 41, 2, SkinVariationType.MediumLight, "woman playing water polo", new string[6] { "person playing water polo", "polo", "water", "water polo", "woman", "woman playing water polo" });

	public static readonly Emoji WomanPlayingWaterPolo_Medium = new Emoji("1F93D-1F3FD-200D-2640-FE0F", null, "\ud83e\udd3d\ud83c\udffd\u200d♀\ufe0f", "woman-playing-water-polo", new string[1] { "woman-playing-water-polo" }, null, null, "Smileys & People", 41, 3, SkinVariationType.Medium, "woman playing water polo", new string[6] { "person playing water polo", "polo", "water", "water polo", "woman", "woman playing water polo" });

	public static readonly Emoji WomanPlayingWaterPolo_MediumDark = new Emoji("1F93D-1F3FE-200D-2640-FE0F", null, "\ud83e\udd3d\ud83c\udffe\u200d♀\ufe0f", "woman-playing-water-polo", new string[1] { "woman-playing-water-polo" }, null, null, "Smileys & People", 41, 4, SkinVariationType.MediumDark, "woman playing water polo", new string[6] { "person playing water polo", "polo", "water", "water polo", "woman", "woman playing water polo" });

	public static readonly Emoji WomanPlayingWaterPolo_Dark = new Emoji("1F93D-1F3FF-200D-2640-FE0F", null, "\ud83e\udd3d\ud83c\udfff\u200d♀\ufe0f", "woman-playing-water-polo", new string[1] { "woman-playing-water-polo" }, null, null, "Smileys & People", 41, 5, SkinVariationType.Dark, "woman playing water polo", new string[6] { "person playing water polo", "polo", "water", "water polo", "woman", "woman playing water polo" });

	public static readonly Emoji ManPlayingHandball = new Emoji("1F93E-200D-2642-FE0F", null, "\ud83e\udd3e\u200d♂\ufe0f", "man-playing-handball", new string[1] { "man-playing-handball" }, null, null, "Smileys & People", 41, 24, SkinVariationType.None, "man playing handball", new string[5] { "ball", "handball", "person playing handball", "man", "man playing handball" });

	public static readonly Emoji ManPlayingHandball_Light = new Emoji("1F93E-1F3FB-200D-2642-FE0F", null, "\ud83e\udd3e\ud83c\udffb\u200d♂\ufe0f", "man-playing-handball", new string[1] { "man-playing-handball" }, null, null, "Smileys & People", 41, 25, SkinVariationType.Light, "man playing handball", new string[5] { "ball", "handball", "person playing handball", "man", "man playing handball" });

	public static readonly Emoji ManPlayingHandball_MediumLight = new Emoji("1F93E-1F3FC-200D-2642-FE0F", null, "\ud83e\udd3e\ud83c\udffc\u200d♂\ufe0f", "man-playing-handball", new string[1] { "man-playing-handball" }, null, null, "Smileys & People", 41, 26, SkinVariationType.MediumLight, "man playing handball", new string[5] { "ball", "handball", "person playing handball", "man", "man playing handball" });

	public static readonly Emoji ManPlayingHandball_Medium = new Emoji("1F93E-1F3FD-200D-2642-FE0F", null, "\ud83e\udd3e\ud83c\udffd\u200d♂\ufe0f", "man-playing-handball", new string[1] { "man-playing-handball" }, null, null, "Smileys & People", 41, 27, SkinVariationType.Medium, "man playing handball", new string[5] { "ball", "handball", "person playing handball", "man", "man playing handball" });

	public static readonly Emoji ManPlayingHandball_MediumDark = new Emoji("1F93E-1F3FE-200D-2642-FE0F", null, "\ud83e\udd3e\ud83c\udffe\u200d♂\ufe0f", "man-playing-handball", new string[1] { "man-playing-handball" }, null, null, "Smileys & People", 41, 28, SkinVariationType.MediumDark, "man playing handball", new string[5] { "ball", "handball", "person playing handball", "man", "man playing handball" });

	public static readonly Emoji ManPlayingHandball_Dark = new Emoji("1F93E-1F3FF-200D-2642-FE0F", null, "\ud83e\udd3e\ud83c\udfff\u200d♂\ufe0f", "man-playing-handball", new string[1] { "man-playing-handball" }, null, null, "Smileys & People", 41, 29, SkinVariationType.Dark, "man playing handball", new string[5] { "ball", "handball", "person playing handball", "man", "man playing handball" });

	public static readonly Emoji WomanPlayingHandball = new Emoji("1F93E-200D-2640-FE0F", null, "\ud83e\udd3e\u200d♀\ufe0f", "woman-playing-handball", new string[1] { "woman-playing-handball" }, null, null, "Smileys & People", 41, 18, SkinVariationType.None, "woman playing handball", new string[5] { "ball", "handball", "person playing handball", "woman", "woman playing handball" });

	public static readonly Emoji WomanPlayingHandball_Light = new Emoji("1F93E-1F3FB-200D-2640-FE0F", null, "\ud83e\udd3e\ud83c\udffb\u200d♀\ufe0f", "woman-playing-handball", new string[1] { "woman-playing-handball" }, null, null, "Smileys & People", 41, 19, SkinVariationType.Light, "woman playing handball", new string[5] { "ball", "handball", "person playing handball", "woman", "woman playing handball" });

	public static readonly Emoji WomanPlayingHandball_MediumLight = new Emoji("1F93E-1F3FC-200D-2640-FE0F", null, "\ud83e\udd3e\ud83c\udffc\u200d♀\ufe0f", "woman-playing-handball", new string[1] { "woman-playing-handball" }, null, null, "Smileys & People", 41, 20, SkinVariationType.MediumLight, "woman playing handball", new string[5] { "ball", "handball", "person playing handball", "woman", "woman playing handball" });

	public static readonly Emoji WomanPlayingHandball_Medium = new Emoji("1F93E-1F3FD-200D-2640-FE0F", null, "\ud83e\udd3e\ud83c\udffd\u200d♀\ufe0f", "woman-playing-handball", new string[1] { "woman-playing-handball" }, null, null, "Smileys & People", 41, 21, SkinVariationType.Medium, "woman playing handball", new string[5] { "ball", "handball", "person playing handball", "woman", "woman playing handball" });

	public static readonly Emoji WomanPlayingHandball_MediumDark = new Emoji("1F93E-1F3FE-200D-2640-FE0F", null, "\ud83e\udd3e\ud83c\udffe\u200d♀\ufe0f", "woman-playing-handball", new string[1] { "woman-playing-handball" }, null, null, "Smileys & People", 41, 22, SkinVariationType.MediumDark, "woman playing handball", new string[5] { "ball", "handball", "person playing handball", "woman", "woman playing handball" });

	public static readonly Emoji WomanPlayingHandball_Dark = new Emoji("1F93E-1F3FF-200D-2640-FE0F", null, "\ud83e\udd3e\ud83c\udfff\u200d♀\ufe0f", "woman-playing-handball", new string[1] { "woman-playing-handball" }, null, null, "Smileys & People", 41, 23, SkinVariationType.Dark, "woman playing handball", new string[5] { "ball", "handball", "person playing handball", "woman", "woman playing handball" });

	public static readonly Emoji ManJuggling = new Emoji("1F939-200D-2642-FE0F", null, "\ud83e\udd39\u200d♂\ufe0f", "man-juggling", new string[1] { "man-juggling" }, null, null, "Smileys & People", 40, 36, SkinVariationType.None, "man juggling", new string[7] { "balance", "juggle", "multitask", "person juggling", "skill", "juggling", "man" });

	public static readonly Emoji ManJuggling_Light = new Emoji("1F939-1F3FB-200D-2642-FE0F", null, "\ud83e\udd39\ud83c\udffb\u200d♂\ufe0f", "man-juggling", new string[1] { "man-juggling" }, null, null, "Smileys & People", 40, 37, SkinVariationType.Light, "man juggling", new string[7] { "balance", "juggle", "multitask", "person juggling", "skill", "juggling", "man" });

	public static readonly Emoji ManJuggling_MediumLight = new Emoji("1F939-1F3FC-200D-2642-FE0F", null, "\ud83e\udd39\ud83c\udffc\u200d♂\ufe0f", "man-juggling", new string[1] { "man-juggling" }, null, null, "Smileys & People", 40, 38, SkinVariationType.MediumLight, "man juggling", new string[7] { "balance", "juggle", "multitask", "person juggling", "skill", "juggling", "man" });

	public static readonly Emoji ManJuggling_Medium = new Emoji("1F939-1F3FD-200D-2642-FE0F", null, "\ud83e\udd39\ud83c\udffd\u200d♂\ufe0f", "man-juggling", new string[1] { "man-juggling" }, null, null, "Smileys & People", 40, 39, SkinVariationType.Medium, "man juggling", new string[7] { "balance", "juggle", "multitask", "person juggling", "skill", "juggling", "man" });

	public static readonly Emoji ManJuggling_MediumDark = new Emoji("1F939-1F3FE-200D-2642-FE0F", null, "\ud83e\udd39\ud83c\udffe\u200d♂\ufe0f", "man-juggling", new string[1] { "man-juggling" }, null, null, "Smileys & People", 40, 40, SkinVariationType.MediumDark, "man juggling", new string[7] { "balance", "juggle", "multitask", "person juggling", "skill", "juggling", "man" });

	public static readonly Emoji ManJuggling_Dark = new Emoji("1F939-1F3FF-200D-2642-FE0F", null, "\ud83e\udd39\ud83c\udfff\u200d♂\ufe0f", "man-juggling", new string[1] { "man-juggling" }, null, null, "Smileys & People", 40, 41, SkinVariationType.Dark, "man juggling", new string[7] { "balance", "juggle", "multitask", "person juggling", "skill", "juggling", "man" });

	public static readonly Emoji WomanJuggling = new Emoji("1F939-200D-2640-FE0F", null, "\ud83e\udd39\u200d♀\ufe0f", "woman-juggling", new string[1] { "woman-juggling" }, null, null, "Smileys & People", 40, 30, SkinVariationType.None, "woman juggling", new string[7] { "balance", "juggle", "multitask", "person juggling", "skill", "juggling", "woman" });

	public static readonly Emoji WomanJuggling_Light = new Emoji("1F939-1F3FB-200D-2640-FE0F", null, "\ud83e\udd39\ud83c\udffb\u200d♀\ufe0f", "woman-juggling", new string[1] { "woman-juggling" }, null, null, "Smileys & People", 40, 31, SkinVariationType.Light, "woman juggling", new string[7] { "balance", "juggle", "multitask", "person juggling", "skill", "juggling", "woman" });

	public static readonly Emoji WomanJuggling_MediumLight = new Emoji("1F939-1F3FC-200D-2640-FE0F", null, "\ud83e\udd39\ud83c\udffc\u200d♀\ufe0f", "woman-juggling", new string[1] { "woman-juggling" }, null, null, "Smileys & People", 40, 32, SkinVariationType.MediumLight, "woman juggling", new string[7] { "balance", "juggle", "multitask", "person juggling", "skill", "juggling", "woman" });

	public static readonly Emoji WomanJuggling_Medium = new Emoji("1F939-1F3FD-200D-2640-FE0F", null, "\ud83e\udd39\ud83c\udffd\u200d♀\ufe0f", "woman-juggling", new string[1] { "woman-juggling" }, null, null, "Smileys & People", 40, 33, SkinVariationType.Medium, "woman juggling", new string[7] { "balance", "juggle", "multitask", "person juggling", "skill", "juggling", "woman" });

	public static readonly Emoji WomanJuggling_MediumDark = new Emoji("1F939-1F3FE-200D-2640-FE0F", null, "\ud83e\udd39\ud83c\udffe\u200d♀\ufe0f", "woman-juggling", new string[1] { "woman-juggling" }, null, null, "Smileys & People", 40, 34, SkinVariationType.MediumDark, "woman juggling", new string[7] { "balance", "juggle", "multitask", "person juggling", "skill", "juggling", "woman" });

	public static readonly Emoji WomanJuggling_Dark = new Emoji("1F939-1F3FF-200D-2640-FE0F", null, "\ud83e\udd39\ud83c\udfff\u200d♀\ufe0f", "woman-juggling", new string[1] { "woman-juggling" }, null, null, "Smileys & People", 40, 35, SkinVariationType.Dark, "woman juggling", new string[7] { "balance", "juggle", "multitask", "person juggling", "skill", "juggling", "woman" });

	public static readonly Emoji Couple = new Emoji("1F46B", "MAN AND WOMAN HOLDING HANDS", "\ud83d\udc6b", "couple", new string[2] { "couple", "man_and_woman_holding_hands" }, null, null, "Smileys & People", 20, 30, SkinVariationType.None, "man and woman holding hands", new string[6] { "couple", "hand", "hold", "man", "man and woman holding hands", "woman" });

	public static readonly Emoji TwoMenHoldingHands = new Emoji("1F46C", "TWO MEN HOLDING HANDS", "\ud83d\udc6c", "two_men_holding_hands", new string[1] { "two_men_holding_hands" }, null, null, "Smileys & People", 20, 31, SkinVariationType.None, "two men holding hands", new string[8] { "couple", "Gemini", "hand", "hold", "man", "twins", "two men holding hands", "zodiac" });

	public static readonly Emoji TwoWomenHoldingHands = new Emoji("1F46D", "TWO WOMEN HOLDING HANDS", "\ud83d\udc6d", "two_women_holding_hands", new string[1] { "two_women_holding_hands" }, null, null, "Smileys & People", 20, 32, SkinVariationType.None, "two women holding hands", new string[5] { "couple", "hand", "hold", "two women holding hands", "woman" });

	public static readonly Emoji Couplekiss = new Emoji("1F48F", "KISS", "\ud83d\udc8f", "couplekiss", new string[1] { "couplekiss" }, null, null, "Smileys & People", 24, 41, SkinVariationType.None, "kiss", new string[2] { "couple", "kiss" });

	public static readonly Emoji WomanKissMan = new Emoji("1F469-200D-2764-FE0F-200D-1F48B-200D-1F468", null, "\ud83d\udc69\u200d❤\ufe0f\u200d\ud83d\udc8b\u200d\ud83d\udc68", "woman-kiss-man", new string[1] { "woman-kiss-man" }, null, null, "Smileys & People", 20, 21, SkinVariationType.None, null, null);

	public static readonly Emoji ManKissMan = new Emoji("1F468-200D-2764-FE0F-200D-1F48B-200D-1F468", null, "\ud83d\udc68\u200d❤\ufe0f\u200d\ud83d\udc8b\u200d\ud83d\udc68", "man-kiss-man", new string[1] { "man-kiss-man" }, null, null, "Smileys & People", 18, 10, SkinVariationType.None, null, null);

	public static readonly Emoji WomanKissWoman = new Emoji("1F469-200D-2764-FE0F-200D-1F48B-200D-1F469", null, "\ud83d\udc69\u200d❤\ufe0f\u200d\ud83d\udc8b\u200d\ud83d\udc69", "woman-kiss-woman", new string[1] { "woman-kiss-woman" }, null, null, "Smileys & People", 20, 22, SkinVariationType.None, null, null);

	public static readonly Emoji CoupleWithHeart = new Emoji("1F491", "COUPLE WITH HEART", "\ud83d\udc91", "couple_with_heart", new string[1] { "couple_with_heart" }, null, null, "Smileys & People", 24, 43, SkinVariationType.None, "couple with heart", new string[3] { "couple", "couple with heart", "love" });

	public static readonly Emoji WomanHeartMan = new Emoji("1F469-200D-2764-FE0F-200D-1F468", null, "\ud83d\udc69\u200d❤\ufe0f\u200d\ud83d\udc68", "woman-heart-man", new string[1] { "woman-heart-man" }, null, null, "Smileys & People", 20, 19, SkinVariationType.None, null, null);

	public static readonly Emoji ManHeartMan = new Emoji("1F468-200D-2764-FE0F-200D-1F468", null, "\ud83d\udc68\u200d❤\ufe0f\u200d\ud83d\udc68", "man-heart-man", new string[1] { "man-heart-man" }, null, null, "Smileys & People", 18, 9, SkinVariationType.None, null, null);

	public static readonly Emoji WomanHeartWoman = new Emoji("1F469-200D-2764-FE0F-200D-1F469", null, "\ud83d\udc69\u200d❤\ufe0f\u200d\ud83d\udc69", "woman-heart-woman", new string[1] { "woman-heart-woman" }, null, null, "Smileys & People", 20, 20, SkinVariationType.None, null, null);

	public static readonly Emoji Family = new Emoji("1F46A", "FAMILY", "\ud83d\udc6a", "family", new string[2] { "family", "man-woman-boy" }, null, null, "Smileys & People", 20, 29, SkinVariationType.None, "family", new string[1] { "family" });

	public static readonly Emoji ManWomanBoy = new Emoji("1F468-200D-1F469-200D-1F466", null, "\ud83d\udc68\u200d\ud83d\udc69\u200d\ud83d\udc66", "man-woman-boy", new string[2] { "man-woman-boy", "family" }, null, null, "Smileys & People", 17, 2, SkinVariationType.None, null, null);

	public static readonly Emoji ManWomanGirl = new Emoji("1F468-200D-1F469-200D-1F467", null, "\ud83d\udc68\u200d\ud83d\udc69\u200d\ud83d\udc67", "man-woman-girl", new string[1] { "man-woman-girl" }, null, null, "Smileys & People", 17, 4, SkinVariationType.None, null, null);

	public static readonly Emoji ManWomanGirlBoy = new Emoji("1F468-200D-1F469-200D-1F467-200D-1F466", null, "\ud83d\udc68\u200d\ud83d\udc69\u200d\ud83d\udc67\u200d\ud83d\udc66", "man-woman-girl-boy", new string[1] { "man-woman-girl-boy" }, null, null, "Smileys & People", 17, 5, SkinVariationType.None, null, null);

	public static readonly Emoji ManWomanBoyBoy = new Emoji("1F468-200D-1F469-200D-1F466-200D-1F466", null, "\ud83d\udc68\u200d\ud83d\udc69\u200d\ud83d\udc66\u200d\ud83d\udc66", "man-woman-boy-boy", new string[1] { "man-woman-boy-boy" }, null, null, "Smileys & People", 17, 3, SkinVariationType.None, null, null);

	public static readonly Emoji ManWomanGirlGirl = new Emoji("1F468-200D-1F469-200D-1F467-200D-1F467", null, "\ud83d\udc68\u200d\ud83d\udc69\u200d\ud83d\udc67\u200d\ud83d\udc67", "man-woman-girl-girl", new string[1] { "man-woman-girl-girl" }, null, null, "Smileys & People", 17, 6, SkinVariationType.None, null, null);

	public static readonly Emoji ManManBoy = new Emoji("1F468-200D-1F468-200D-1F466", null, "\ud83d\udc68\u200d\ud83d\udc68\u200d\ud83d\udc66", "man-man-boy", new string[1] { "man-man-boy" }, null, null, "Smileys & People", 16, 49, SkinVariationType.None, null, null);

	public static readonly Emoji ManManGirl = new Emoji("1F468-200D-1F468-200D-1F467", null, "\ud83d\udc68\u200d\ud83d\udc68\u200d\ud83d\udc67", "man-man-girl", new string[1] { "man-man-girl" }, null, null, "Smileys & People", 16, 51, SkinVariationType.None, null, null);

	public static readonly Emoji ManManGirlBoy = new Emoji("1F468-200D-1F468-200D-1F467-200D-1F466", null, "\ud83d\udc68\u200d\ud83d\udc68\u200d\ud83d\udc67\u200d\ud83d\udc66", "man-man-girl-boy", new string[1] { "man-man-girl-boy" }, null, null, "Smileys & People", 17, 0, SkinVariationType.None, null, null);

	public static readonly Emoji ManManBoyBoy = new Emoji("1F468-200D-1F468-200D-1F466-200D-1F466", null, "\ud83d\udc68\u200d\ud83d\udc68\u200d\ud83d\udc66\u200d\ud83d\udc66", "man-man-boy-boy", new string[1] { "man-man-boy-boy" }, null, null, "Smileys & People", 16, 50, SkinVariationType.None, null, null);

	public static readonly Emoji ManManGirlGirl = new Emoji("1F468-200D-1F468-200D-1F467-200D-1F467", null, "\ud83d\udc68\u200d\ud83d\udc68\u200d\ud83d\udc67\u200d\ud83d\udc67", "man-man-girl-girl", new string[1] { "man-man-girl-girl" }, null, null, "Smileys & People", 17, 1, SkinVariationType.None, null, null);

	public static readonly Emoji WomanWomanBoy = new Emoji("1F469-200D-1F469-200D-1F466", null, "\ud83d\udc69\u200d\ud83d\udc69\u200d\ud83d\udc66", "woman-woman-boy", new string[1] { "woman-woman-boy" }, null, null, "Smileys & People", 19, 12, SkinVariationType.None, null, null);

	public static readonly Emoji WomanWomanGirl = new Emoji("1F469-200D-1F469-200D-1F467", null, "\ud83d\udc69\u200d\ud83d\udc69\u200d\ud83d\udc67", "woman-woman-girl", new string[1] { "woman-woman-girl" }, null, null, "Smileys & People", 19, 14, SkinVariationType.None, null, null);

	public static readonly Emoji WomanWomanGirlBoy = new Emoji("1F469-200D-1F469-200D-1F467-200D-1F466", null, "\ud83d\udc69\u200d\ud83d\udc69\u200d\ud83d\udc67\u200d\ud83d\udc66", "woman-woman-girl-boy", new string[1] { "woman-woman-girl-boy" }, null, null, "Smileys & People", 19, 15, SkinVariationType.None, null, null);

	public static readonly Emoji WomanWomanBoyBoy = new Emoji("1F469-200D-1F469-200D-1F466-200D-1F466", null, "\ud83d\udc69\u200d\ud83d\udc69\u200d\ud83d\udc66\u200d\ud83d\udc66", "woman-woman-boy-boy", new string[1] { "woman-woman-boy-boy" }, null, null, "Smileys & People", 19, 13, SkinVariationType.None, null, null);

	public static readonly Emoji WomanWomanGirlGirl = new Emoji("1F469-200D-1F469-200D-1F467-200D-1F467", null, "\ud83d\udc69\u200d\ud83d\udc69\u200d\ud83d\udc67\u200d\ud83d\udc67", "woman-woman-girl-girl", new string[1] { "woman-woman-girl-girl" }, null, null, "Smileys & People", 19, 16, SkinVariationType.None, null, null);

	public static readonly Emoji ManBoy = new Emoji("1F468-200D-1F466", null, "\ud83d\udc68\u200d\ud83d\udc66", "man-boy", new string[1] { "man-boy" }, null, null, "Smileys & People", 16, 45, SkinVariationType.None, null, null);

	public static readonly Emoji ManBoyBoy = new Emoji("1F468-200D-1F466-200D-1F466", null, "\ud83d\udc68\u200d\ud83d\udc66\u200d\ud83d\udc66", "man-boy-boy", new string[1] { "man-boy-boy" }, null, null, "Smileys & People", 16, 44, SkinVariationType.None, null, null);

	public static readonly Emoji ManGirl = new Emoji("1F468-200D-1F467", null, "\ud83d\udc68\u200d\ud83d\udc67", "man-girl", new string[1] { "man-girl" }, null, null, "Smileys & People", 16, 48, SkinVariationType.None, null, null);

	public static readonly Emoji ManGirlBoy = new Emoji("1F468-200D-1F467-200D-1F466", null, "\ud83d\udc68\u200d\ud83d\udc67\u200d\ud83d\udc66", "man-girl-boy", new string[1] { "man-girl-boy" }, null, null, "Smileys & People", 16, 46, SkinVariationType.None, null, null);

	public static readonly Emoji ManGirlGirl = new Emoji("1F468-200D-1F467-200D-1F467", null, "\ud83d\udc68\u200d\ud83d\udc67\u200d\ud83d\udc67", "man-girl-girl", new string[1] { "man-girl-girl" }, null, null, "Smileys & People", 16, 47, SkinVariationType.None, null, null);

	public static readonly Emoji WomanBoy = new Emoji("1F469-200D-1F466", null, "\ud83d\udc69\u200d\ud83d\udc66", "woman-boy", new string[1] { "woman-boy" }, null, null, "Smileys & People", 19, 8, SkinVariationType.None, null, null);

	public static readonly Emoji WomanBoyBoy = new Emoji("1F469-200D-1F466-200D-1F466", null, "\ud83d\udc69\u200d\ud83d\udc66\u200d\ud83d\udc66", "woman-boy-boy", new string[1] { "woman-boy-boy" }, null, null, "Smileys & People", 19, 7, SkinVariationType.None, null, null);

	public static readonly Emoji WomanGirl = new Emoji("1F469-200D-1F467", null, "\ud83d\udc69\u200d\ud83d\udc67", "woman-girl", new string[1] { "woman-girl" }, null, null, "Smileys & People", 19, 11, SkinVariationType.None, null, null);

	public static readonly Emoji WomanGirlBoy = new Emoji("1F469-200D-1F467-200D-1F466", null, "\ud83d\udc69\u200d\ud83d\udc67\u200d\ud83d\udc66", "woman-girl-boy", new string[1] { "woman-girl-boy" }, null, null, "Smileys & People", 19, 9, SkinVariationType.None, null, null);

	public static readonly Emoji WomanGirlGirl = new Emoji("1F469-200D-1F467-200D-1F467", null, "\ud83d\udc69\u200d\ud83d\udc67\u200d\ud83d\udc67", "woman-girl-girl", new string[1] { "woman-girl-girl" }, null, null, "Smileys & People", 19, 10, SkinVariationType.None, null, null);

	public static readonly Emoji Selfie = new Emoji("1F933", "SELFIE", "\ud83e\udd33", "selfie", new string[1] { "selfie" }, null, null, "Smileys & People", 39, 22, SkinVariationType.None, "selfie", new string[3] { "camera", "phone", "selfie" });

	public static readonly Emoji Selfie_Light = new Emoji("1F933-1F3FB", "SELFIE", "\ud83e\udd33\ud83c\udffb", "selfie", new string[1] { "selfie" }, null, null, "Smileys & People", 39, 23, SkinVariationType.Light, "selfie", new string[3] { "camera", "phone", "selfie" });

	public static readonly Emoji Selfie_MediumLight = new Emoji("1F933-1F3FC", "SELFIE", "\ud83e\udd33\ud83c\udffc", "selfie", new string[1] { "selfie" }, null, null, "Smileys & People", 39, 24, SkinVariationType.MediumLight, "selfie", new string[3] { "camera", "phone", "selfie" });

	public static readonly Emoji Selfie_Medium = new Emoji("1F933-1F3FD", "SELFIE", "\ud83e\udd33\ud83c\udffd", "selfie", new string[1] { "selfie" }, null, null, "Smileys & People", 39, 25, SkinVariationType.Medium, "selfie", new string[3] { "camera", "phone", "selfie" });

	public static readonly Emoji Selfie_MediumDark = new Emoji("1F933-1F3FE", "SELFIE", "\ud83e\udd33\ud83c\udffe", "selfie", new string[1] { "selfie" }, null, null, "Smileys & People", 39, 26, SkinVariationType.MediumDark, "selfie", new string[3] { "camera", "phone", "selfie" });

	public static readonly Emoji Selfie_Dark = new Emoji("1F933-1F3FF", "SELFIE", "\ud83e\udd33\ud83c\udfff", "selfie", new string[1] { "selfie" }, null, null, "Smileys & People", 39, 27, SkinVariationType.Dark, "selfie", new string[3] { "camera", "phone", "selfie" });

	public static readonly Emoji Muscle = new Emoji("1F4AA", "FLEXED BICEPS", "\ud83d\udcaa", "muscle", new string[1] { "muscle" }, null, null, "Smileys & People", 25, 16, SkinVariationType.None, "flexed biceps", new string[5] { "biceps", "comic", "flex", "flexed biceps", "muscle" });

	public static readonly Emoji Muscle_Light = new Emoji("1F4AA-1F3FB", "FLEXED BICEPS", "\ud83d\udcaa\ud83c\udffb", "muscle", new string[1] { "muscle" }, null, null, "Smileys & People", 25, 17, SkinVariationType.Light, "flexed biceps", new string[5] { "biceps", "comic", "flex", "flexed biceps", "muscle" });

	public static readonly Emoji Muscle_MediumLight = new Emoji("1F4AA-1F3FC", "FLEXED BICEPS", "\ud83d\udcaa\ud83c\udffc", "muscle", new string[1] { "muscle" }, null, null, "Smileys & People", 25, 18, SkinVariationType.MediumLight, "flexed biceps", new string[5] { "biceps", "comic", "flex", "flexed biceps", "muscle" });

	public static readonly Emoji Muscle_Medium = new Emoji("1F4AA-1F3FD", "FLEXED BICEPS", "\ud83d\udcaa\ud83c\udffd", "muscle", new string[1] { "muscle" }, null, null, "Smileys & People", 25, 19, SkinVariationType.Medium, "flexed biceps", new string[5] { "biceps", "comic", "flex", "flexed biceps", "muscle" });

	public static readonly Emoji Muscle_MediumDark = new Emoji("1F4AA-1F3FE", "FLEXED BICEPS", "\ud83d\udcaa\ud83c\udffe", "muscle", new string[1] { "muscle" }, null, null, "Smileys & People", 25, 20, SkinVariationType.MediumDark, "flexed biceps", new string[5] { "biceps", "comic", "flex", "flexed biceps", "muscle" });

	public static readonly Emoji Muscle_Dark = new Emoji("1F4AA-1F3FF", "FLEXED BICEPS", "\ud83d\udcaa\ud83c\udfff", "muscle", new string[1] { "muscle" }, null, null, "Smileys & People", 25, 21, SkinVariationType.Dark, "flexed biceps", new string[5] { "biceps", "comic", "flex", "flexed biceps", "muscle" });

	public static readonly Emoji PointLeft = new Emoji("1F448", "WHITE LEFT POINTING BACKHAND INDEX", "\ud83d\udc48", "point_left", new string[1] { "point_left" }, null, null, "Smileys & People", 14, 19, SkinVariationType.None, "backhand index pointing left", new string[6] { "backhand", "backhand index pointing left", "finger", "hand", "index", "point" });

	public static readonly Emoji PointLeft_Light = new Emoji("1F448-1F3FB", "WHITE LEFT POINTING BACKHAND INDEX", "\ud83d\udc48\ud83c\udffb", "point_left", new string[1] { "point_left" }, null, null, "Smileys & People", 14, 20, SkinVariationType.Light, "backhand index pointing left", new string[6] { "backhand", "backhand index pointing left", "finger", "hand", "index", "point" });

	public static readonly Emoji PointLeft_MediumLight = new Emoji("1F448-1F3FC", "WHITE LEFT POINTING BACKHAND INDEX", "\ud83d\udc48\ud83c\udffc", "point_left", new string[1] { "point_left" }, null, null, "Smileys & People", 14, 21, SkinVariationType.MediumLight, "backhand index pointing left", new string[6] { "backhand", "backhand index pointing left", "finger", "hand", "index", "point" });

	public static readonly Emoji PointLeft_Medium = new Emoji("1F448-1F3FD", "WHITE LEFT POINTING BACKHAND INDEX", "\ud83d\udc48\ud83c\udffd", "point_left", new string[1] { "point_left" }, null, null, "Smileys & People", 14, 22, SkinVariationType.Medium, "backhand index pointing left", new string[6] { "backhand", "backhand index pointing left", "finger", "hand", "index", "point" });

	public static readonly Emoji PointLeft_MediumDark = new Emoji("1F448-1F3FE", "WHITE LEFT POINTING BACKHAND INDEX", "\ud83d\udc48\ud83c\udffe", "point_left", new string[1] { "point_left" }, null, null, "Smileys & People", 14, 23, SkinVariationType.MediumDark, "backhand index pointing left", new string[6] { "backhand", "backhand index pointing left", "finger", "hand", "index", "point" });

	public static readonly Emoji PointLeft_Dark = new Emoji("1F448-1F3FF", "WHITE LEFT POINTING BACKHAND INDEX", "\ud83d\udc48\ud83c\udfff", "point_left", new string[1] { "point_left" }, null, null, "Smileys & People", 14, 24, SkinVariationType.Dark, "backhand index pointing left", new string[6] { "backhand", "backhand index pointing left", "finger", "hand", "index", "point" });

	public static readonly Emoji PointRight = new Emoji("1F449", "WHITE RIGHT POINTING BACKHAND INDEX", "\ud83d\udc49", "point_right", new string[1] { "point_right" }, null, null, "Smileys & People", 14, 25, SkinVariationType.None, "backhand index pointing right", new string[6] { "backhand", "backhand index pointing right", "finger", "hand", "index", "point" });

	public static readonly Emoji PointRight_Light = new Emoji("1F449-1F3FB", "WHITE RIGHT POINTING BACKHAND INDEX", "\ud83d\udc49\ud83c\udffb", "point_right", new string[1] { "point_right" }, null, null, "Smileys & People", 14, 26, SkinVariationType.Light, "backhand index pointing right", new string[6] { "backhand", "backhand index pointing right", "finger", "hand", "index", "point" });

	public static readonly Emoji PointRight_MediumLight = new Emoji("1F449-1F3FC", "WHITE RIGHT POINTING BACKHAND INDEX", "\ud83d\udc49\ud83c\udffc", "point_right", new string[1] { "point_right" }, null, null, "Smileys & People", 14, 27, SkinVariationType.MediumLight, "backhand index pointing right", new string[6] { "backhand", "backhand index pointing right", "finger", "hand", "index", "point" });

	public static readonly Emoji PointRight_Medium = new Emoji("1F449-1F3FD", "WHITE RIGHT POINTING BACKHAND INDEX", "\ud83d\udc49\ud83c\udffd", "point_right", new string[1] { "point_right" }, null, null, "Smileys & People", 14, 28, SkinVariationType.Medium, "backhand index pointing right", new string[6] { "backhand", "backhand index pointing right", "finger", "hand", "index", "point" });

	public static readonly Emoji PointRight_MediumDark = new Emoji("1F449-1F3FE", "WHITE RIGHT POINTING BACKHAND INDEX", "\ud83d\udc49\ud83c\udffe", "point_right", new string[1] { "point_right" }, null, null, "Smileys & People", 14, 29, SkinVariationType.MediumDark, "backhand index pointing right", new string[6] { "backhand", "backhand index pointing right", "finger", "hand", "index", "point" });

	public static readonly Emoji PointRight_Dark = new Emoji("1F449-1F3FF", "WHITE RIGHT POINTING BACKHAND INDEX", "\ud83d\udc49\ud83c\udfff", "point_right", new string[1] { "point_right" }, null, null, "Smileys & People", 14, 30, SkinVariationType.Dark, "backhand index pointing right", new string[6] { "backhand", "backhand index pointing right", "finger", "hand", "index", "point" });

	public static readonly Emoji PointUp = new Emoji("261D-FE0F", "WHITE UP POINTING INDEX", "☝\ufe0f", "point_up", new string[1] { "point_up" }, null, null, "Smileys & People", 47, 26, SkinVariationType.None, null, null);

	public static readonly Emoji PointUp_Light = new Emoji("261D-1F3FB", "WHITE UP POINTING INDEX", "☝\ud83c\udffb", "point_up", new string[1] { "point_up" }, null, null, "Smileys & People", 47, 27, SkinVariationType.Light, null, null);

	public static readonly Emoji PointUp_MediumLight = new Emoji("261D-1F3FC", "WHITE UP POINTING INDEX", "☝\ud83c\udffc", "point_up", new string[1] { "point_up" }, null, null, "Smileys & People", 47, 28, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji PointUp_Medium = new Emoji("261D-1F3FD", "WHITE UP POINTING INDEX", "☝\ud83c\udffd", "point_up", new string[1] { "point_up" }, null, null, "Smileys & People", 47, 29, SkinVariationType.Medium, null, null);

	public static readonly Emoji PointUp_MediumDark = new Emoji("261D-1F3FE", "WHITE UP POINTING INDEX", "☝\ud83c\udffe", "point_up", new string[1] { "point_up" }, null, null, "Smileys & People", 47, 30, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji PointUp_Dark = new Emoji("261D-1F3FF", "WHITE UP POINTING INDEX", "☝\ud83c\udfff", "point_up", new string[1] { "point_up" }, null, null, "Smileys & People", 47, 31, SkinVariationType.Dark, null, null);

	public static readonly Emoji PointUp2 = new Emoji("1F446", "WHITE UP POINTING BACKHAND INDEX", "\ud83d\udc46", "point_up_2", new string[1] { "point_up_2" }, null, null, "Smileys & People", 14, 7, SkinVariationType.None, "backhand index pointing up", new string[7] { "backhand", "backhand index pointing up", "finger", "hand", "index", "point", "up" });

	public static readonly Emoji PointUp2_Light = new Emoji("1F446-1F3FB", "WHITE UP POINTING BACKHAND INDEX", "\ud83d\udc46\ud83c\udffb", "point_up_2", new string[1] { "point_up_2" }, null, null, "Smileys & People", 14, 8, SkinVariationType.Light, "backhand index pointing up", new string[7] { "backhand", "backhand index pointing up", "finger", "hand", "index", "point", "up" });

	public static readonly Emoji PointUp2_MediumLight = new Emoji("1F446-1F3FC", "WHITE UP POINTING BACKHAND INDEX", "\ud83d\udc46\ud83c\udffc", "point_up_2", new string[1] { "point_up_2" }, null, null, "Smileys & People", 14, 9, SkinVariationType.MediumLight, "backhand index pointing up", new string[7] { "backhand", "backhand index pointing up", "finger", "hand", "index", "point", "up" });

	public static readonly Emoji PointUp2_Medium = new Emoji("1F446-1F3FD", "WHITE UP POINTING BACKHAND INDEX", "\ud83d\udc46\ud83c\udffd", "point_up_2", new string[1] { "point_up_2" }, null, null, "Smileys & People", 14, 10, SkinVariationType.Medium, "backhand index pointing up", new string[7] { "backhand", "backhand index pointing up", "finger", "hand", "index", "point", "up" });

	public static readonly Emoji PointUp2_MediumDark = new Emoji("1F446-1F3FE", "WHITE UP POINTING BACKHAND INDEX", "\ud83d\udc46\ud83c\udffe", "point_up_2", new string[1] { "point_up_2" }, null, null, "Smileys & People", 14, 11, SkinVariationType.MediumDark, "backhand index pointing up", new string[7] { "backhand", "backhand index pointing up", "finger", "hand", "index", "point", "up" });

	public static readonly Emoji PointUp2_Dark = new Emoji("1F446-1F3FF", "WHITE UP POINTING BACKHAND INDEX", "\ud83d\udc46\ud83c\udfff", "point_up_2", new string[1] { "point_up_2" }, null, null, "Smileys & People", 14, 12, SkinVariationType.Dark, "backhand index pointing up", new string[7] { "backhand", "backhand index pointing up", "finger", "hand", "index", "point", "up" });

	public static readonly Emoji MiddleFinger = new Emoji("1F595", "REVERSED HAND WITH MIDDLE FINGER EXTENDED", "\ud83d\udd95", "middle_finger", new string[2] { "middle_finger", "reversed_hand_with_middle_finger_extended" }, null, null, "Smileys & People", 29, 38, SkinVariationType.None, "middle finger", new string[3] { "finger", "hand", "middle finger" });

	public static readonly Emoji MiddleFinger_Light = new Emoji("1F595-1F3FB", "REVERSED HAND WITH MIDDLE FINGER EXTENDED", "\ud83d\udd95\ud83c\udffb", "middle_finger", new string[2] { "middle_finger", "reversed_hand_with_middle_finger_extended" }, null, null, "Smileys & People", 29, 39, SkinVariationType.Light, "middle finger", new string[3] { "finger", "hand", "middle finger" });

	public static readonly Emoji MiddleFinger_MediumLight = new Emoji("1F595-1F3FC", "REVERSED HAND WITH MIDDLE FINGER EXTENDED", "\ud83d\udd95\ud83c\udffc", "middle_finger", new string[2] { "middle_finger", "reversed_hand_with_middle_finger_extended" }, null, null, "Smileys & People", 29, 40, SkinVariationType.MediumLight, "middle finger", new string[3] { "finger", "hand", "middle finger" });

	public static readonly Emoji MiddleFinger_Medium = new Emoji("1F595-1F3FD", "REVERSED HAND WITH MIDDLE FINGER EXTENDED", "\ud83d\udd95\ud83c\udffd", "middle_finger", new string[2] { "middle_finger", "reversed_hand_with_middle_finger_extended" }, null, null, "Smileys & People", 29, 41, SkinVariationType.Medium, "middle finger", new string[3] { "finger", "hand", "middle finger" });

	public static readonly Emoji MiddleFinger_MediumDark = new Emoji("1F595-1F3FE", "REVERSED HAND WITH MIDDLE FINGER EXTENDED", "\ud83d\udd95\ud83c\udffe", "middle_finger", new string[2] { "middle_finger", "reversed_hand_with_middle_finger_extended" }, null, null, "Smileys & People", 29, 42, SkinVariationType.MediumDark, "middle finger", new string[3] { "finger", "hand", "middle finger" });

	public static readonly Emoji MiddleFinger_Dark = new Emoji("1F595-1F3FF", "REVERSED HAND WITH MIDDLE FINGER EXTENDED", "\ud83d\udd95\ud83c\udfff", "middle_finger", new string[2] { "middle_finger", "reversed_hand_with_middle_finger_extended" }, null, null, "Smileys & People", 29, 43, SkinVariationType.Dark, "middle finger", new string[3] { "finger", "hand", "middle finger" });

	public static readonly Emoji PointDown = new Emoji("1F447", "WHITE DOWN POINTING BACKHAND INDEX", "\ud83d\udc47", "point_down", new string[1] { "point_down" }, null, null, "Smileys & People", 14, 13, SkinVariationType.None, "backhand index pointing down", new string[7] { "backhand", "backhand index pointing down", "down", "finger", "hand", "index", "point" });

	public static readonly Emoji PointDown_Light = new Emoji("1F447-1F3FB", "WHITE DOWN POINTING BACKHAND INDEX", "\ud83d\udc47\ud83c\udffb", "point_down", new string[1] { "point_down" }, null, null, "Smileys & People", 14, 14, SkinVariationType.Light, "backhand index pointing down", new string[7] { "backhand", "backhand index pointing down", "down", "finger", "hand", "index", "point" });

	public static readonly Emoji PointDown_MediumLight = new Emoji("1F447-1F3FC", "WHITE DOWN POINTING BACKHAND INDEX", "\ud83d\udc47\ud83c\udffc", "point_down", new string[1] { "point_down" }, null, null, "Smileys & People", 14, 15, SkinVariationType.MediumLight, "backhand index pointing down", new string[7] { "backhand", "backhand index pointing down", "down", "finger", "hand", "index", "point" });

	public static readonly Emoji PointDown_Medium = new Emoji("1F447-1F3FD", "WHITE DOWN POINTING BACKHAND INDEX", "\ud83d\udc47\ud83c\udffd", "point_down", new string[1] { "point_down" }, null, null, "Smileys & People", 14, 16, SkinVariationType.Medium, "backhand index pointing down", new string[7] { "backhand", "backhand index pointing down", "down", "finger", "hand", "index", "point" });

	public static readonly Emoji PointDown_MediumDark = new Emoji("1F447-1F3FE", "WHITE DOWN POINTING BACKHAND INDEX", "\ud83d\udc47\ud83c\udffe", "point_down", new string[1] { "point_down" }, null, null, "Smileys & People", 14, 17, SkinVariationType.MediumDark, "backhand index pointing down", new string[7] { "backhand", "backhand index pointing down", "down", "finger", "hand", "index", "point" });

	public static readonly Emoji PointDown_Dark = new Emoji("1F447-1F3FF", "WHITE DOWN POINTING BACKHAND INDEX", "\ud83d\udc47\ud83c\udfff", "point_down", new string[1] { "point_down" }, null, null, "Smileys & People", 14, 18, SkinVariationType.Dark, "backhand index pointing down", new string[7] { "backhand", "backhand index pointing down", "down", "finger", "hand", "index", "point" });

	public static readonly Emoji V = new Emoji("270C-FE0F", "VICTORY HAND", "✌\ufe0f", "v", new string[1] { "v" }, null, null, "Smileys & People", 49, 30, SkinVariationType.None, null, null);

	public static readonly Emoji V_Light = new Emoji("270C-1F3FB", "VICTORY HAND", "✌\ud83c\udffb", "v", new string[1] { "v" }, null, null, "Smileys & People", 49, 31, SkinVariationType.Light, null, null);

	public static readonly Emoji V_MediumLight = new Emoji("270C-1F3FC", "VICTORY HAND", "✌\ud83c\udffc", "v", new string[1] { "v" }, null, null, "Smileys & People", 49, 32, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji V_Medium = new Emoji("270C-1F3FD", "VICTORY HAND", "✌\ud83c\udffd", "v", new string[1] { "v" }, null, null, "Smileys & People", 49, 33, SkinVariationType.Medium, null, null);

	public static readonly Emoji V_MediumDark = new Emoji("270C-1F3FE", "VICTORY HAND", "✌\ud83c\udffe", "v", new string[1] { "v" }, null, null, "Smileys & People", 49, 34, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji V_Dark = new Emoji("270C-1F3FF", "VICTORY HAND", "✌\ud83c\udfff", "v", new string[1] { "v" }, null, null, "Smileys & People", 49, 35, SkinVariationType.Dark, null, null);

	public static readonly Emoji CrossedFingers = new Emoji("1F91E", "HAND WITH INDEX AND MIDDLE FINGERS CROSSED", "\ud83e\udd1e", "crossed_fingers", new string[2] { "crossed_fingers", "hand_with_index_and_middle_fingers_crossed" }, null, null, "Smileys & People", 38, 11, SkinVariationType.None, "crossed fingers", new string[5] { "cross", "crossed fingers", "finger", "hand", "luck" });

	public static readonly Emoji CrossedFingers_Light = new Emoji("1F91E-1F3FB", "HAND WITH INDEX AND MIDDLE FINGERS CROSSED", "\ud83e\udd1e\ud83c\udffb", "crossed_fingers", new string[2] { "crossed_fingers", "hand_with_index_and_middle_fingers_crossed" }, null, null, "Smileys & People", 38, 12, SkinVariationType.Light, "crossed fingers", new string[5] { "cross", "crossed fingers", "finger", "hand", "luck" });

	public static readonly Emoji CrossedFingers_MediumLight = new Emoji("1F91E-1F3FC", "HAND WITH INDEX AND MIDDLE FINGERS CROSSED", "\ud83e\udd1e\ud83c\udffc", "crossed_fingers", new string[2] { "crossed_fingers", "hand_with_index_and_middle_fingers_crossed" }, null, null, "Smileys & People", 38, 13, SkinVariationType.MediumLight, "crossed fingers", new string[5] { "cross", "crossed fingers", "finger", "hand", "luck" });

	public static readonly Emoji CrossedFingers_Medium = new Emoji("1F91E-1F3FD", "HAND WITH INDEX AND MIDDLE FINGERS CROSSED", "\ud83e\udd1e\ud83c\udffd", "crossed_fingers", new string[2] { "crossed_fingers", "hand_with_index_and_middle_fingers_crossed" }, null, null, "Smileys & People", 38, 14, SkinVariationType.Medium, "crossed fingers", new string[5] { "cross", "crossed fingers", "finger", "hand", "luck" });

	public static readonly Emoji CrossedFingers_MediumDark = new Emoji("1F91E-1F3FE", "HAND WITH INDEX AND MIDDLE FINGERS CROSSED", "\ud83e\udd1e\ud83c\udffe", "crossed_fingers", new string[2] { "crossed_fingers", "hand_with_index_and_middle_fingers_crossed" }, null, null, "Smileys & People", 38, 15, SkinVariationType.MediumDark, "crossed fingers", new string[5] { "cross", "crossed fingers", "finger", "hand", "luck" });

	public static readonly Emoji CrossedFingers_Dark = new Emoji("1F91E-1F3FF", "HAND WITH INDEX AND MIDDLE FINGERS CROSSED", "\ud83e\udd1e\ud83c\udfff", "crossed_fingers", new string[2] { "crossed_fingers", "hand_with_index_and_middle_fingers_crossed" }, null, null, "Smileys & People", 38, 16, SkinVariationType.Dark, "crossed fingers", new string[5] { "cross", "crossed fingers", "finger", "hand", "luck" });

	public static readonly Emoji SpockHand = new Emoji("1F596", "RAISED HAND WITH PART BETWEEN MIDDLE AND RING FINGERS", "\ud83d\udd96", "spock-hand", new string[1] { "spock-hand" }, null, null, "Smileys & People", 29, 44, SkinVariationType.None, "vulcan salute", new string[5] { "finger", "hand", "spock", "vulcan", "vulcan salute" });

	public static readonly Emoji SpockHand_Light = new Emoji("1F596-1F3FB", "RAISED HAND WITH PART BETWEEN MIDDLE AND RING FINGERS", "\ud83d\udd96\ud83c\udffb", "spock-hand", new string[1] { "spock-hand" }, null, null, "Smileys & People", 29, 45, SkinVariationType.Light, "vulcan salute", new string[5] { "finger", "hand", "spock", "vulcan", "vulcan salute" });

	public static readonly Emoji SpockHand_MediumLight = new Emoji("1F596-1F3FC", "RAISED HAND WITH PART BETWEEN MIDDLE AND RING FINGERS", "\ud83d\udd96\ud83c\udffc", "spock-hand", new string[1] { "spock-hand" }, null, null, "Smileys & People", 29, 46, SkinVariationType.MediumLight, "vulcan salute", new string[5] { "finger", "hand", "spock", "vulcan", "vulcan salute" });

	public static readonly Emoji SpockHand_Medium = new Emoji("1F596-1F3FD", "RAISED HAND WITH PART BETWEEN MIDDLE AND RING FINGERS", "\ud83d\udd96\ud83c\udffd", "spock-hand", new string[1] { "spock-hand" }, null, null, "Smileys & People", 29, 47, SkinVariationType.Medium, "vulcan salute", new string[5] { "finger", "hand", "spock", "vulcan", "vulcan salute" });

	public static readonly Emoji SpockHand_MediumDark = new Emoji("1F596-1F3FE", "RAISED HAND WITH PART BETWEEN MIDDLE AND RING FINGERS", "\ud83d\udd96\ud83c\udffe", "spock-hand", new string[1] { "spock-hand" }, null, null, "Smileys & People", 29, 48, SkinVariationType.MediumDark, "vulcan salute", new string[5] { "finger", "hand", "spock", "vulcan", "vulcan salute" });

	public static readonly Emoji SpockHand_Dark = new Emoji("1F596-1F3FF", "RAISED HAND WITH PART BETWEEN MIDDLE AND RING FINGERS", "\ud83d\udd96\ud83c\udfff", "spock-hand", new string[1] { "spock-hand" }, null, null, "Smileys & People", 29, 49, SkinVariationType.Dark, "vulcan salute", new string[5] { "finger", "hand", "spock", "vulcan", "vulcan salute" });

	public static readonly Emoji TheHorns = new Emoji("1F918", "SIGN OF THE HORNS", "\ud83e\udd18", "the_horns", new string[2] { "the_horns", "sign_of_the_horns" }, null, null, "Smileys & People", 37, 32, SkinVariationType.None, "sign of the horns", new string[5] { "finger", "hand", "horns", "rock-on", "sign of the horns" });

	public static readonly Emoji TheHorns_Light = new Emoji("1F918-1F3FB", "SIGN OF THE HORNS", "\ud83e\udd18\ud83c\udffb", "the_horns", new string[2] { "the_horns", "sign_of_the_horns" }, null, null, "Smileys & People", 37, 33, SkinVariationType.Light, "sign of the horns", new string[5] { "finger", "hand", "horns", "rock-on", "sign of the horns" });

	public static readonly Emoji TheHorns_MediumLight = new Emoji("1F918-1F3FC", "SIGN OF THE HORNS", "\ud83e\udd18\ud83c\udffc", "the_horns", new string[2] { "the_horns", "sign_of_the_horns" }, null, null, "Smileys & People", 37, 34, SkinVariationType.MediumLight, "sign of the horns", new string[5] { "finger", "hand", "horns", "rock-on", "sign of the horns" });

	public static readonly Emoji TheHorns_Medium = new Emoji("1F918-1F3FD", "SIGN OF THE HORNS", "\ud83e\udd18\ud83c\udffd", "the_horns", new string[2] { "the_horns", "sign_of_the_horns" }, null, null, "Smileys & People", 37, 35, SkinVariationType.Medium, "sign of the horns", new string[5] { "finger", "hand", "horns", "rock-on", "sign of the horns" });

	public static readonly Emoji TheHorns_MediumDark = new Emoji("1F918-1F3FE", "SIGN OF THE HORNS", "\ud83e\udd18\ud83c\udffe", "the_horns", new string[2] { "the_horns", "sign_of_the_horns" }, null, null, "Smileys & People", 37, 36, SkinVariationType.MediumDark, "sign of the horns", new string[5] { "finger", "hand", "horns", "rock-on", "sign of the horns" });

	public static readonly Emoji TheHorns_Dark = new Emoji("1F918-1F3FF", "SIGN OF THE HORNS", "\ud83e\udd18\ud83c\udfff", "the_horns", new string[2] { "the_horns", "sign_of_the_horns" }, null, null, "Smileys & People", 37, 37, SkinVariationType.Dark, "sign of the horns", new string[5] { "finger", "hand", "horns", "rock-on", "sign of the horns" });

	public static readonly Emoji CallMeHand = new Emoji("1F919", "CALL ME HAND", "\ud83e\udd19", "call_me_hand", new string[1] { "call_me_hand" }, null, null, "Smileys & People", 37, 38, SkinVariationType.None, "call me hand", new string[3] { "call", "call me hand", "hand" });

	public static readonly Emoji CallMeHand_Light = new Emoji("1F919-1F3FB", "CALL ME HAND", "\ud83e\udd19\ud83c\udffb", "call_me_hand", new string[1] { "call_me_hand" }, null, null, "Smileys & People", 37, 39, SkinVariationType.Light, "call me hand", new string[3] { "call", "call me hand", "hand" });

	public static readonly Emoji CallMeHand_MediumLight = new Emoji("1F919-1F3FC", "CALL ME HAND", "\ud83e\udd19\ud83c\udffc", "call_me_hand", new string[1] { "call_me_hand" }, null, null, "Smileys & People", 37, 40, SkinVariationType.MediumLight, "call me hand", new string[3] { "call", "call me hand", "hand" });

	public static readonly Emoji CallMeHand_Medium = new Emoji("1F919-1F3FD", "CALL ME HAND", "\ud83e\udd19\ud83c\udffd", "call_me_hand", new string[1] { "call_me_hand" }, null, null, "Smileys & People", 37, 41, SkinVariationType.Medium, "call me hand", new string[3] { "call", "call me hand", "hand" });

	public static readonly Emoji CallMeHand_MediumDark = new Emoji("1F919-1F3FE", "CALL ME HAND", "\ud83e\udd19\ud83c\udffe", "call_me_hand", new string[1] { "call_me_hand" }, null, null, "Smileys & People", 37, 42, SkinVariationType.MediumDark, "call me hand", new string[3] { "call", "call me hand", "hand" });

	public static readonly Emoji CallMeHand_Dark = new Emoji("1F919-1F3FF", "CALL ME HAND", "\ud83e\udd19\ud83c\udfff", "call_me_hand", new string[1] { "call_me_hand" }, null, null, "Smileys & People", 37, 43, SkinVariationType.Dark, "call me hand", new string[3] { "call", "call me hand", "hand" });

	public static readonly Emoji RaisedHandWithFingersSplayed = new Emoji("1F590-FE0F", null, "\ud83d\udd90\ufe0f", "raised_hand_with_fingers_splayed", new string[1] { "raised_hand_with_fingers_splayed" }, null, null, "Smileys & People", 29, 32, SkinVariationType.None, null, null);

	public static readonly Emoji RaisedHandWithFingersSplayed_Light = new Emoji("1F590-1F3FB", null, "\ud83d\udd90\ud83c\udffb", "raised_hand_with_fingers_splayed", new string[1] { "raised_hand_with_fingers_splayed" }, null, null, "Smileys & People", 29, 33, SkinVariationType.Light, null, null);

	public static readonly Emoji RaisedHandWithFingersSplayed_MediumLight = new Emoji("1F590-1F3FC", null, "\ud83d\udd90\ud83c\udffc", "raised_hand_with_fingers_splayed", new string[1] { "raised_hand_with_fingers_splayed" }, null, null, "Smileys & People", 29, 34, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji RaisedHandWithFingersSplayed_Medium = new Emoji("1F590-1F3FD", null, "\ud83d\udd90\ud83c\udffd", "raised_hand_with_fingers_splayed", new string[1] { "raised_hand_with_fingers_splayed" }, null, null, "Smileys & People", 29, 35, SkinVariationType.Medium, null, null);

	public static readonly Emoji RaisedHandWithFingersSplayed_MediumDark = new Emoji("1F590-1F3FE", null, "\ud83d\udd90\ud83c\udffe", "raised_hand_with_fingers_splayed", new string[1] { "raised_hand_with_fingers_splayed" }, null, null, "Smileys & People", 29, 36, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji RaisedHandWithFingersSplayed_Dark = new Emoji("1F590-1F3FF", null, "\ud83d\udd90\ud83c\udfff", "raised_hand_with_fingers_splayed", new string[1] { "raised_hand_with_fingers_splayed" }, null, null, "Smileys & People", 29, 37, SkinVariationType.Dark, null, null);

	public static readonly Emoji Hand = new Emoji("270B", "RAISED HAND", "✋", "hand", new string[2] { "hand", "raised_hand" }, null, null, "Smileys & People", 49, 24, SkinVariationType.None, "raised hand", new string[2] { "hand", "raised hand" });

	public static readonly Emoji Hand_Light = new Emoji("270B-1F3FB", "RAISED HAND", "✋\ud83c\udffb", "hand", new string[2] { "hand", "raised_hand" }, null, null, "Smileys & People", 49, 25, SkinVariationType.Light, "raised hand", new string[2] { "hand", "raised hand" });

	public static readonly Emoji Hand_MediumLight = new Emoji("270B-1F3FC", "RAISED HAND", "✋\ud83c\udffc", "hand", new string[2] { "hand", "raised_hand" }, null, null, "Smileys & People", 49, 26, SkinVariationType.MediumLight, "raised hand", new string[2] { "hand", "raised hand" });

	public static readonly Emoji Hand_Medium = new Emoji("270B-1F3FD", "RAISED HAND", "✋\ud83c\udffd", "hand", new string[2] { "hand", "raised_hand" }, null, null, "Smileys & People", 49, 27, SkinVariationType.Medium, "raised hand", new string[2] { "hand", "raised hand" });

	public static readonly Emoji Hand_MediumDark = new Emoji("270B-1F3FE", "RAISED HAND", "✋\ud83c\udffe", "hand", new string[2] { "hand", "raised_hand" }, null, null, "Smileys & People", 49, 28, SkinVariationType.MediumDark, "raised hand", new string[2] { "hand", "raised hand" });

	public static readonly Emoji Hand_Dark = new Emoji("270B-1F3FF", "RAISED HAND", "✋\ud83c\udfff", "hand", new string[2] { "hand", "raised_hand" }, null, null, "Smileys & People", 49, 29, SkinVariationType.Dark, "raised hand", new string[2] { "hand", "raised hand" });

	public static readonly Emoji OkHand = new Emoji("1F44C", "OK HAND SIGN", "\ud83d\udc4c", "ok_hand", new string[1] { "ok_hand" }, null, null, "Smileys & People", 14, 43, SkinVariationType.None, "OK hand", new string[2] { "hand", "OK" });

	public static readonly Emoji OkHand_Light = new Emoji("1F44C-1F3FB", "OK HAND SIGN", "\ud83d\udc4c\ud83c\udffb", "ok_hand", new string[1] { "ok_hand" }, null, null, "Smileys & People", 14, 44, SkinVariationType.Light, "OK hand", new string[2] { "hand", "OK" });

	public static readonly Emoji OkHand_MediumLight = new Emoji("1F44C-1F3FC", "OK HAND SIGN", "\ud83d\udc4c\ud83c\udffc", "ok_hand", new string[1] { "ok_hand" }, null, null, "Smileys & People", 14, 45, SkinVariationType.MediumLight, "OK hand", new string[2] { "hand", "OK" });

	public static readonly Emoji OkHand_Medium = new Emoji("1F44C-1F3FD", "OK HAND SIGN", "\ud83d\udc4c\ud83c\udffd", "ok_hand", new string[1] { "ok_hand" }, null, null, "Smileys & People", 14, 46, SkinVariationType.Medium, "OK hand", new string[2] { "hand", "OK" });

	public static readonly Emoji OkHand_MediumDark = new Emoji("1F44C-1F3FE", "OK HAND SIGN", "\ud83d\udc4c\ud83c\udffe", "ok_hand", new string[1] { "ok_hand" }, null, null, "Smileys & People", 14, 47, SkinVariationType.MediumDark, "OK hand", new string[2] { "hand", "OK" });

	public static readonly Emoji OkHand_Dark = new Emoji("1F44C-1F3FF", "OK HAND SIGN", "\ud83d\udc4c\ud83c\udfff", "ok_hand", new string[1] { "ok_hand" }, null, null, "Smileys & People", 14, 48, SkinVariationType.Dark, "OK hand", new string[2] { "hand", "OK" });

	public static readonly Emoji ThumbsUp = new Emoji("1F44D", "THUMBS UP SIGN", "\ud83d\udc4d", "+1", new string[2] { "+1", "thumbsup" }, null, null, "Smileys & People", 14, 49, SkinVariationType.None, "thumbs up", new string[5] { "+1", "hand", "thumb", "thumbs up", "up" });

	public static readonly Emoji ThumbsUp_Light = new Emoji("1F44D-1F3FB", "THUMBS UP SIGN", "\ud83d\udc4d\ud83c\udffb", "+1", new string[2] { "+1", "thumbsup" }, null, null, "Smileys & People", 14, 50, SkinVariationType.Light, "thumbs up", new string[5] { "+1", "hand", "thumb", "thumbs up", "up" });

	public static readonly Emoji ThumbsUp_MediumLight = new Emoji("1F44D-1F3FC", "THUMBS UP SIGN", "\ud83d\udc4d\ud83c\udffc", "+1", new string[2] { "+1", "thumbsup" }, null, null, "Smileys & People", 14, 51, SkinVariationType.MediumLight, "thumbs up", new string[5] { "+1", "hand", "thumb", "thumbs up", "up" });

	public static readonly Emoji ThumbsUp_Medium = new Emoji("1F44D-1F3FD", "THUMBS UP SIGN", "\ud83d\udc4d\ud83c\udffd", "+1", new string[2] { "+1", "thumbsup" }, null, null, "Smileys & People", 15, 0, SkinVariationType.Medium, "thumbs up", new string[5] { "+1", "hand", "thumb", "thumbs up", "up" });

	public static readonly Emoji ThumbsUp_MediumDark = new Emoji("1F44D-1F3FE", "THUMBS UP SIGN", "\ud83d\udc4d\ud83c\udffe", "+1", new string[2] { "+1", "thumbsup" }, null, null, "Smileys & People", 15, 1, SkinVariationType.MediumDark, "thumbs up", new string[5] { "+1", "hand", "thumb", "thumbs up", "up" });

	public static readonly Emoji ThumbsUp_Dark = new Emoji("1F44D-1F3FF", "THUMBS UP SIGN", "\ud83d\udc4d\ud83c\udfff", "+1", new string[2] { "+1", "thumbsup" }, null, null, "Smileys & People", 15, 2, SkinVariationType.Dark, "thumbs up", new string[5] { "+1", "hand", "thumb", "thumbs up", "up" });

	public static readonly Emoji ThumbsDown = new Emoji("1F44E", "THUMBS DOWN SIGN", "\ud83d\udc4e", "-1", new string[2] { "-1", "thumbsdown" }, null, null, "Smileys & People", 15, 3, SkinVariationType.None, "thumbs down", new string[5] { "-1", "down", "hand", "thumb", "thumbs down" });

	public static readonly Emoji ThumbsDown_Light = new Emoji("1F44E-1F3FB", "THUMBS DOWN SIGN", "\ud83d\udc4e\ud83c\udffb", "-1", new string[2] { "-1", "thumbsdown" }, null, null, "Smileys & People", 15, 4, SkinVariationType.Light, "thumbs down", new string[5] { "-1", "down", "hand", "thumb", "thumbs down" });

	public static readonly Emoji ThumbsDown_MediumLight = new Emoji("1F44E-1F3FC", "THUMBS DOWN SIGN", "\ud83d\udc4e\ud83c\udffc", "-1", new string[2] { "-1", "thumbsdown" }, null, null, "Smileys & People", 15, 5, SkinVariationType.MediumLight, "thumbs down", new string[5] { "-1", "down", "hand", "thumb", "thumbs down" });

	public static readonly Emoji ThumbsDown_Medium = new Emoji("1F44E-1F3FD", "THUMBS DOWN SIGN", "\ud83d\udc4e\ud83c\udffd", "-1", new string[2] { "-1", "thumbsdown" }, null, null, "Smileys & People", 15, 6, SkinVariationType.Medium, "thumbs down", new string[5] { "-1", "down", "hand", "thumb", "thumbs down" });

	public static readonly Emoji ThumbsDown_MediumDark = new Emoji("1F44E-1F3FE", "THUMBS DOWN SIGN", "\ud83d\udc4e\ud83c\udffe", "-1", new string[2] { "-1", "thumbsdown" }, null, null, "Smileys & People", 15, 7, SkinVariationType.MediumDark, "thumbs down", new string[5] { "-1", "down", "hand", "thumb", "thumbs down" });

	public static readonly Emoji ThumbsDown_Dark = new Emoji("1F44E-1F3FF", "THUMBS DOWN SIGN", "\ud83d\udc4e\ud83c\udfff", "-1", new string[2] { "-1", "thumbsdown" }, null, null, "Smileys & People", 15, 8, SkinVariationType.Dark, "thumbs down", new string[5] { "-1", "down", "hand", "thumb", "thumbs down" });

	public static readonly Emoji Fist = new Emoji("270A", "RAISED FIST", "✊", "fist", new string[1] { "fist" }, null, null, "Smileys & People", 49, 18, SkinVariationType.None, "raised fist", new string[5] { "clenched", "fist", "hand", "punch", "raised fist" });

	public static readonly Emoji Fist_Light = new Emoji("270A-1F3FB", "RAISED FIST", "✊\ud83c\udffb", "fist", new string[1] { "fist" }, null, null, "Smileys & People", 49, 19, SkinVariationType.Light, "raised fist", new string[5] { "clenched", "fist", "hand", "punch", "raised fist" });

	public static readonly Emoji Fist_MediumLight = new Emoji("270A-1F3FC", "RAISED FIST", "✊\ud83c\udffc", "fist", new string[1] { "fist" }, null, null, "Smileys & People", 49, 20, SkinVariationType.MediumLight, "raised fist", new string[5] { "clenched", "fist", "hand", "punch", "raised fist" });

	public static readonly Emoji Fist_Medium = new Emoji("270A-1F3FD", "RAISED FIST", "✊\ud83c\udffd", "fist", new string[1] { "fist" }, null, null, "Smileys & People", 49, 21, SkinVariationType.Medium, "raised fist", new string[5] { "clenched", "fist", "hand", "punch", "raised fist" });

	public static readonly Emoji Fist_MediumDark = new Emoji("270A-1F3FE", "RAISED FIST", "✊\ud83c\udffe", "fist", new string[1] { "fist" }, null, null, "Smileys & People", 49, 22, SkinVariationType.MediumDark, "raised fist", new string[5] { "clenched", "fist", "hand", "punch", "raised fist" });

	public static readonly Emoji Fist_Dark = new Emoji("270A-1F3FF", "RAISED FIST", "✊\ud83c\udfff", "fist", new string[1] { "fist" }, null, null, "Smileys & People", 49, 23, SkinVariationType.Dark, "raised fist", new string[5] { "clenched", "fist", "hand", "punch", "raised fist" });

	public static readonly Emoji Facepunch = new Emoji("1F44A", "FISTED HAND SIGN", "\ud83d\udc4a", "facepunch", new string[2] { "facepunch", "punch" }, null, null, "Smileys & People", 14, 31, SkinVariationType.None, "oncoming fist", new string[5] { "clenched", "fist", "hand", "oncoming fist", "punch" });

	public static readonly Emoji Facepunch_Light = new Emoji("1F44A-1F3FB", "FISTED HAND SIGN", "\ud83d\udc4a\ud83c\udffb", "facepunch", new string[2] { "facepunch", "punch" }, null, null, "Smileys & People", 14, 32, SkinVariationType.Light, "oncoming fist", new string[5] { "clenched", "fist", "hand", "oncoming fist", "punch" });

	public static readonly Emoji Facepunch_MediumLight = new Emoji("1F44A-1F3FC", "FISTED HAND SIGN", "\ud83d\udc4a\ud83c\udffc", "facepunch", new string[2] { "facepunch", "punch" }, null, null, "Smileys & People", 14, 33, SkinVariationType.MediumLight, "oncoming fist", new string[5] { "clenched", "fist", "hand", "oncoming fist", "punch" });

	public static readonly Emoji Facepunch_Medium = new Emoji("1F44A-1F3FD", "FISTED HAND SIGN", "\ud83d\udc4a\ud83c\udffd", "facepunch", new string[2] { "facepunch", "punch" }, null, null, "Smileys & People", 14, 34, SkinVariationType.Medium, "oncoming fist", new string[5] { "clenched", "fist", "hand", "oncoming fist", "punch" });

	public static readonly Emoji Facepunch_MediumDark = new Emoji("1F44A-1F3FE", "FISTED HAND SIGN", "\ud83d\udc4a\ud83c\udffe", "facepunch", new string[2] { "facepunch", "punch" }, null, null, "Smileys & People", 14, 35, SkinVariationType.MediumDark, "oncoming fist", new string[5] { "clenched", "fist", "hand", "oncoming fist", "punch" });

	public static readonly Emoji Facepunch_Dark = new Emoji("1F44A-1F3FF", "FISTED HAND SIGN", "\ud83d\udc4a\ud83c\udfff", "facepunch", new string[2] { "facepunch", "punch" }, null, null, "Smileys & People", 14, 36, SkinVariationType.Dark, "oncoming fist", new string[5] { "clenched", "fist", "hand", "oncoming fist", "punch" });

	public static readonly Emoji LeftFacingFist = new Emoji("1F91B", "LEFT-FACING FIST", "\ud83e\udd1b", "left-facing_fist", new string[1] { "left-facing_fist" }, null, null, "Smileys & People", 37, 50, SkinVariationType.None, "left-facing fist", new string[3] { "fist", "left-facing fist", "leftwards" });

	public static readonly Emoji LeftFacingFist_Light = new Emoji("1F91B-1F3FB", "LEFT-FACING FIST", "\ud83e\udd1b\ud83c\udffb", "left-facing_fist", new string[1] { "left-facing_fist" }, null, null, "Smileys & People", 37, 51, SkinVariationType.Light, "left-facing fist", new string[3] { "fist", "left-facing fist", "leftwards" });

	public static readonly Emoji LeftFacingFist_MediumLight = new Emoji("1F91B-1F3FC", "LEFT-FACING FIST", "\ud83e\udd1b\ud83c\udffc", "left-facing_fist", new string[1] { "left-facing_fist" }, null, null, "Smileys & People", 38, 0, SkinVariationType.MediumLight, "left-facing fist", new string[3] { "fist", "left-facing fist", "leftwards" });

	public static readonly Emoji LeftFacingFist_Medium = new Emoji("1F91B-1F3FD", "LEFT-FACING FIST", "\ud83e\udd1b\ud83c\udffd", "left-facing_fist", new string[1] { "left-facing_fist" }, null, null, "Smileys & People", 38, 1, SkinVariationType.Medium, "left-facing fist", new string[3] { "fist", "left-facing fist", "leftwards" });

	public static readonly Emoji LeftFacingFist_MediumDark = new Emoji("1F91B-1F3FE", "LEFT-FACING FIST", "\ud83e\udd1b\ud83c\udffe", "left-facing_fist", new string[1] { "left-facing_fist" }, null, null, "Smileys & People", 38, 2, SkinVariationType.MediumDark, "left-facing fist", new string[3] { "fist", "left-facing fist", "leftwards" });

	public static readonly Emoji LeftFacingFist_Dark = new Emoji("1F91B-1F3FF", "LEFT-FACING FIST", "\ud83e\udd1b\ud83c\udfff", "left-facing_fist", new string[1] { "left-facing_fist" }, null, null, "Smileys & People", 38, 3, SkinVariationType.Dark, "left-facing fist", new string[3] { "fist", "left-facing fist", "leftwards" });

	public static readonly Emoji RightFacingFist = new Emoji("1F91C", "RIGHT-FACING FIST", "\ud83e\udd1c", "right-facing_fist", new string[1] { "right-facing_fist" }, null, null, "Smileys & People", 38, 4, SkinVariationType.None, "right-facing fist", new string[3] { "fist", "right-facing fist", "rightwards" });

	public static readonly Emoji RightFacingFist_Light = new Emoji("1F91C-1F3FB", "RIGHT-FACING FIST", "\ud83e\udd1c\ud83c\udffb", "right-facing_fist", new string[1] { "right-facing_fist" }, null, null, "Smileys & People", 38, 5, SkinVariationType.Light, "right-facing fist", new string[3] { "fist", "right-facing fist", "rightwards" });

	public static readonly Emoji RightFacingFist_MediumLight = new Emoji("1F91C-1F3FC", "RIGHT-FACING FIST", "\ud83e\udd1c\ud83c\udffc", "right-facing_fist", new string[1] { "right-facing_fist" }, null, null, "Smileys & People", 38, 6, SkinVariationType.MediumLight, "right-facing fist", new string[3] { "fist", "right-facing fist", "rightwards" });

	public static readonly Emoji RightFacingFist_Medium = new Emoji("1F91C-1F3FD", "RIGHT-FACING FIST", "\ud83e\udd1c\ud83c\udffd", "right-facing_fist", new string[1] { "right-facing_fist" }, null, null, "Smileys & People", 38, 7, SkinVariationType.Medium, "right-facing fist", new string[3] { "fist", "right-facing fist", "rightwards" });

	public static readonly Emoji RightFacingFist_MediumDark = new Emoji("1F91C-1F3FE", "RIGHT-FACING FIST", "\ud83e\udd1c\ud83c\udffe", "right-facing_fist", new string[1] { "right-facing_fist" }, null, null, "Smileys & People", 38, 8, SkinVariationType.MediumDark, "right-facing fist", new string[3] { "fist", "right-facing fist", "rightwards" });

	public static readonly Emoji RightFacingFist_Dark = new Emoji("1F91C-1F3FF", "RIGHT-FACING FIST", "\ud83e\udd1c\ud83c\udfff", "right-facing_fist", new string[1] { "right-facing_fist" }, null, null, "Smileys & People", 38, 9, SkinVariationType.Dark, "right-facing fist", new string[3] { "fist", "right-facing fist", "rightwards" });

	public static readonly Emoji RaisedBackOfHand = new Emoji("1F91A", "RAISED BACK OF HAND", "\ud83e\udd1a", "raised_back_of_hand", new string[1] { "raised_back_of_hand" }, null, null, "Smileys & People", 37, 44, SkinVariationType.None, "raised back of hand", new string[3] { "backhand", "raised", "raised back of hand" });

	public static readonly Emoji RaisedBackOfHand_Light = new Emoji("1F91A-1F3FB", "RAISED BACK OF HAND", "\ud83e\udd1a\ud83c\udffb", "raised_back_of_hand", new string[1] { "raised_back_of_hand" }, null, null, "Smileys & People", 37, 45, SkinVariationType.Light, "raised back of hand", new string[3] { "backhand", "raised", "raised back of hand" });

	public static readonly Emoji RaisedBackOfHand_MediumLight = new Emoji("1F91A-1F3FC", "RAISED BACK OF HAND", "\ud83e\udd1a\ud83c\udffc", "raised_back_of_hand", new string[1] { "raised_back_of_hand" }, null, null, "Smileys & People", 37, 46, SkinVariationType.MediumLight, "raised back of hand", new string[3] { "backhand", "raised", "raised back of hand" });

	public static readonly Emoji RaisedBackOfHand_Medium = new Emoji("1F91A-1F3FD", "RAISED BACK OF HAND", "\ud83e\udd1a\ud83c\udffd", "raised_back_of_hand", new string[1] { "raised_back_of_hand" }, null, null, "Smileys & People", 37, 47, SkinVariationType.Medium, "raised back of hand", new string[3] { "backhand", "raised", "raised back of hand" });

	public static readonly Emoji RaisedBackOfHand_MediumDark = new Emoji("1F91A-1F3FE", "RAISED BACK OF HAND", "\ud83e\udd1a\ud83c\udffe", "raised_back_of_hand", new string[1] { "raised_back_of_hand" }, null, null, "Smileys & People", 37, 48, SkinVariationType.MediumDark, "raised back of hand", new string[3] { "backhand", "raised", "raised back of hand" });

	public static readonly Emoji RaisedBackOfHand_Dark = new Emoji("1F91A-1F3FF", "RAISED BACK OF HAND", "\ud83e\udd1a\ud83c\udfff", "raised_back_of_hand", new string[1] { "raised_back_of_hand" }, null, null, "Smileys & People", 37, 49, SkinVariationType.Dark, "raised back of hand", new string[3] { "backhand", "raised", "raised back of hand" });

	public static readonly Emoji Wave = new Emoji("1F44B", "WAVING HAND SIGN", "\ud83d\udc4b", "wave", new string[1] { "wave" }, null, null, "Smileys & People", 14, 37, SkinVariationType.None, "waving hand", new string[3] { "hand", "wave", "waving" });

	public static readonly Emoji Wave_Light = new Emoji("1F44B-1F3FB", "WAVING HAND SIGN", "\ud83d\udc4b\ud83c\udffb", "wave", new string[1] { "wave" }, null, null, "Smileys & People", 14, 38, SkinVariationType.Light, "waving hand", new string[3] { "hand", "wave", "waving" });

	public static readonly Emoji Wave_MediumLight = new Emoji("1F44B-1F3FC", "WAVING HAND SIGN", "\ud83d\udc4b\ud83c\udffc", "wave", new string[1] { "wave" }, null, null, "Smileys & People", 14, 39, SkinVariationType.MediumLight, "waving hand", new string[3] { "hand", "wave", "waving" });

	public static readonly Emoji Wave_Medium = new Emoji("1F44B-1F3FD", "WAVING HAND SIGN", "\ud83d\udc4b\ud83c\udffd", "wave", new string[1] { "wave" }, null, null, "Smileys & People", 14, 40, SkinVariationType.Medium, "waving hand", new string[3] { "hand", "wave", "waving" });

	public static readonly Emoji Wave_MediumDark = new Emoji("1F44B-1F3FE", "WAVING HAND SIGN", "\ud83d\udc4b\ud83c\udffe", "wave", new string[1] { "wave" }, null, null, "Smileys & People", 14, 41, SkinVariationType.MediumDark, "waving hand", new string[3] { "hand", "wave", "waving" });

	public static readonly Emoji Wave_Dark = new Emoji("1F44B-1F3FF", "WAVING HAND SIGN", "\ud83d\udc4b\ud83c\udfff", "wave", new string[1] { "wave" }, null, null, "Smileys & People", 14, 42, SkinVariationType.Dark, "waving hand", new string[3] { "hand", "wave", "waving" });

	public static readonly Emoji ILoveYouHandSign = new Emoji("1F91F", "I LOVE YOU HAND SIGN", "\ud83e\udd1f", "i_love_you_hand_sign", new string[1] { "i_love_you_hand_sign" }, null, null, "Smileys & People", 38, 17, SkinVariationType.None, "love-you gesture", new string[3] { "hand", "ILY", "love-you gesture" });

	public static readonly Emoji ILoveYouHandSign_Light = new Emoji("1F91F-1F3FB", "I LOVE YOU HAND SIGN", "\ud83e\udd1f\ud83c\udffb", "i_love_you_hand_sign", new string[1] { "i_love_you_hand_sign" }, null, null, "Smileys & People", 38, 18, SkinVariationType.Light, "love-you gesture", new string[3] { "hand", "ILY", "love-you gesture" });

	public static readonly Emoji ILoveYouHandSign_MediumLight = new Emoji("1F91F-1F3FC", "I LOVE YOU HAND SIGN", "\ud83e\udd1f\ud83c\udffc", "i_love_you_hand_sign", new string[1] { "i_love_you_hand_sign" }, null, null, "Smileys & People", 38, 19, SkinVariationType.MediumLight, "love-you gesture", new string[3] { "hand", "ILY", "love-you gesture" });

	public static readonly Emoji ILoveYouHandSign_Medium = new Emoji("1F91F-1F3FD", "I LOVE YOU HAND SIGN", "\ud83e\udd1f\ud83c\udffd", "i_love_you_hand_sign", new string[1] { "i_love_you_hand_sign" }, null, null, "Smileys & People", 38, 20, SkinVariationType.Medium, "love-you gesture", new string[3] { "hand", "ILY", "love-you gesture" });

	public static readonly Emoji ILoveYouHandSign_MediumDark = new Emoji("1F91F-1F3FE", "I LOVE YOU HAND SIGN", "\ud83e\udd1f\ud83c\udffe", "i_love_you_hand_sign", new string[1] { "i_love_you_hand_sign" }, null, null, "Smileys & People", 38, 21, SkinVariationType.MediumDark, "love-you gesture", new string[3] { "hand", "ILY", "love-you gesture" });

	public static readonly Emoji ILoveYouHandSign_Dark = new Emoji("1F91F-1F3FF", "I LOVE YOU HAND SIGN", "\ud83e\udd1f\ud83c\udfff", "i_love_you_hand_sign", new string[1] { "i_love_you_hand_sign" }, null, null, "Smileys & People", 38, 22, SkinVariationType.Dark, "love-you gesture", new string[3] { "hand", "ILY", "love-you gesture" });

	public static readonly Emoji WritingHand = new Emoji("270D-FE0F", null, "✍\ufe0f", "writing_hand", new string[1] { "writing_hand" }, null, null, "Smileys & People", 49, 36, SkinVariationType.None, null, null);

	public static readonly Emoji WritingHand_Light = new Emoji("270D-1F3FB", null, "✍\ud83c\udffb", "writing_hand", new string[1] { "writing_hand" }, null, null, "Smileys & People", 49, 37, SkinVariationType.Light, null, null);

	public static readonly Emoji WritingHand_MediumLight = new Emoji("270D-1F3FC", null, "✍\ud83c\udffc", "writing_hand", new string[1] { "writing_hand" }, null, null, "Smileys & People", 49, 38, SkinVariationType.MediumLight, null, null);

	public static readonly Emoji WritingHand_Medium = new Emoji("270D-1F3FD", null, "✍\ud83c\udffd", "writing_hand", new string[1] { "writing_hand" }, null, null, "Smileys & People", 49, 39, SkinVariationType.Medium, null, null);

	public static readonly Emoji WritingHand_MediumDark = new Emoji("270D-1F3FE", null, "✍\ud83c\udffe", "writing_hand", new string[1] { "writing_hand" }, null, null, "Smileys & People", 49, 40, SkinVariationType.MediumDark, null, null);

	public static readonly Emoji WritingHand_Dark = new Emoji("270D-1F3FF", null, "✍\ud83c\udfff", "writing_hand", new string[1] { "writing_hand" }, null, null, "Smileys & People", 49, 41, SkinVariationType.Dark, null, null);

	public static readonly Emoji Clap = new Emoji("1F44F", "CLAPPING HANDS SIGN", "\ud83d\udc4f", "clap", new string[1] { "clap" }, null, null, "Smileys & People", 15, 9, SkinVariationType.None, "clapping hands", new string[3] { "clap", "clapping hands", "hand" });

	public static readonly Emoji Clap_Light = new Emoji("1F44F-1F3FB", "CLAPPING HANDS SIGN", "\ud83d\udc4f\ud83c\udffb", "clap", new string[1] { "clap" }, null, null, "Smileys & People", 15, 10, SkinVariationType.Light, "clapping hands", new string[3] { "clap", "clapping hands", "hand" });

	public static readonly Emoji Clap_MediumLight = new Emoji("1F44F-1F3FC", "CLAPPING HANDS SIGN", "\ud83d\udc4f\ud83c\udffc", "clap", new string[1] { "clap" }, null, null, "Smileys & People", 15, 11, SkinVariationType.MediumLight, "clapping hands", new string[3] { "clap", "clapping hands", "hand" });

	public static readonly Emoji Clap_Medium = new Emoji("1F44F-1F3FD", "CLAPPING HANDS SIGN", "\ud83d\udc4f\ud83c\udffd", "clap", new string[1] { "clap" }, null, null, "Smileys & People", 15, 12, SkinVariationType.Medium, "clapping hands", new string[3] { "clap", "clapping hands", "hand" });

	public static readonly Emoji Clap_MediumDark = new Emoji("1F44F-1F3FE", "CLAPPING HANDS SIGN", "\ud83d\udc4f\ud83c\udffe", "clap", new string[1] { "clap" }, null, null, "Smileys & People", 15, 13, SkinVariationType.MediumDark, "clapping hands", new string[3] { "clap", "clapping hands", "hand" });

	public static readonly Emoji Clap_Dark = new Emoji("1F44F-1F3FF", "CLAPPING HANDS SIGN", "\ud83d\udc4f\ud83c\udfff", "clap", new string[1] { "clap" }, null, null, "Smileys & People", 15, 14, SkinVariationType.Dark, "clapping hands", new string[3] { "clap", "clapping hands", "hand" });

	public static readonly Emoji OpenHands = new Emoji("1F450", "OPEN HANDS SIGN", "\ud83d\udc50", "open_hands", new string[1] { "open_hands" }, null, null, "Smileys & People", 15, 15, SkinVariationType.None, "open hands", new string[3] { "hand", "open", "open hands" });

	public static readonly Emoji OpenHands_Light = new Emoji("1F450-1F3FB", "OPEN HANDS SIGN", "\ud83d\udc50\ud83c\udffb", "open_hands", new string[1] { "open_hands" }, null, null, "Smileys & People", 15, 16, SkinVariationType.Light, "open hands", new string[3] { "hand", "open", "open hands" });

	public static readonly Emoji OpenHands_MediumLight = new Emoji("1F450-1F3FC", "OPEN HANDS SIGN", "\ud83d\udc50\ud83c\udffc", "open_hands", new string[1] { "open_hands" }, null, null, "Smileys & People", 15, 17, SkinVariationType.MediumLight, "open hands", new string[3] { "hand", "open", "open hands" });

	public static readonly Emoji OpenHands_Medium = new Emoji("1F450-1F3FD", "OPEN HANDS SIGN", "\ud83d\udc50\ud83c\udffd", "open_hands", new string[1] { "open_hands" }, null, null, "Smileys & People", 15, 18, SkinVariationType.Medium, "open hands", new string[3] { "hand", "open", "open hands" });

	public static readonly Emoji OpenHands_MediumDark = new Emoji("1F450-1F3FE", "OPEN HANDS SIGN", "\ud83d\udc50\ud83c\udffe", "open_hands", new string[1] { "open_hands" }, null, null, "Smileys & People", 15, 19, SkinVariationType.MediumDark, "open hands", new string[3] { "hand", "open", "open hands" });

	public static readonly Emoji OpenHands_Dark = new Emoji("1F450-1F3FF", "OPEN HANDS SIGN", "\ud83d\udc50\ud83c\udfff", "open_hands", new string[1] { "open_hands" }, null, null, "Smileys & People", 15, 20, SkinVariationType.Dark, "open hands", new string[3] { "hand", "open", "open hands" });

	public static readonly Emoji RaisedHands = new Emoji("1F64C", "PERSON RAISING BOTH HANDS IN CELEBRATION", "\ud83d\ude4c", "raised_hands", new string[1] { "raised_hands" }, null, null, "Smileys & People", 33, 12, SkinVariationType.None, "raising hands", new string[6] { "celebration", "gesture", "hand", "hooray", "raised", "raising hands" });

	public static readonly Emoji RaisedHands_Light = new Emoji("1F64C-1F3FB", "PERSON RAISING BOTH HANDS IN CELEBRATION", "\ud83d\ude4c\ud83c\udffb", "raised_hands", new string[1] { "raised_hands" }, null, null, "Smileys & People", 33, 13, SkinVariationType.Light, "raising hands", new string[6] { "celebration", "gesture", "hand", "hooray", "raised", "raising hands" });

	public static readonly Emoji RaisedHands_MediumLight = new Emoji("1F64C-1F3FC", "PERSON RAISING BOTH HANDS IN CELEBRATION", "\ud83d\ude4c\ud83c\udffc", "raised_hands", new string[1] { "raised_hands" }, null, null, "Smileys & People", 33, 14, SkinVariationType.MediumLight, "raising hands", new string[6] { "celebration", "gesture", "hand", "hooray", "raised", "raising hands" });

	public static readonly Emoji RaisedHands_Medium = new Emoji("1F64C-1F3FD", "PERSON RAISING BOTH HANDS IN CELEBRATION", "\ud83d\ude4c\ud83c\udffd", "raised_hands", new string[1] { "raised_hands" }, null, null, "Smileys & People", 33, 15, SkinVariationType.Medium, "raising hands", new string[6] { "celebration", "gesture", "hand", "hooray", "raised", "raising hands" });

	public static readonly Emoji RaisedHands_MediumDark = new Emoji("1F64C-1F3FE", "PERSON RAISING BOTH HANDS IN CELEBRATION", "\ud83d\ude4c\ud83c\udffe", "raised_hands", new string[1] { "raised_hands" }, null, null, "Smileys & People", 33, 16, SkinVariationType.MediumDark, "raising hands", new string[6] { "celebration", "gesture", "hand", "hooray", "raised", "raising hands" });

	public static readonly Emoji RaisedHands_Dark = new Emoji("1F64C-1F3FF", "PERSON RAISING BOTH HANDS IN CELEBRATION", "\ud83d\ude4c\ud83c\udfff", "raised_hands", new string[1] { "raised_hands" }, null, null, "Smileys & People", 33, 17, SkinVariationType.Dark, "raising hands", new string[6] { "celebration", "gesture", "hand", "hooray", "raised", "raising hands" });

	public static readonly Emoji PalmsUpTogether = new Emoji("1F932", "PALMS UP TOGETHER", "\ud83e\udd32", "palms_up_together", new string[1] { "palms_up_together" }, null, null, "Smileys & People", 39, 16, SkinVariationType.None, "palms up together", new string[2] { "palms up together", "prayer" });

	public static readonly Emoji PalmsUpTogether_Light = new Emoji("1F932-1F3FB", "PALMS UP TOGETHER", "\ud83e\udd32\ud83c\udffb", "palms_up_together", new string[1] { "palms_up_together" }, null, null, "Smileys & People", 39, 17, SkinVariationType.Light, "palms up together", new string[2] { "palms up together", "prayer" });

	public static readonly Emoji PalmsUpTogether_MediumLight = new Emoji("1F932-1F3FC", "PALMS UP TOGETHER", "\ud83e\udd32\ud83c\udffc", "palms_up_together", new string[1] { "palms_up_together" }, null, null, "Smileys & People", 39, 18, SkinVariationType.MediumLight, "palms up together", new string[2] { "palms up together", "prayer" });

	public static readonly Emoji PalmsUpTogether_Medium = new Emoji("1F932-1F3FD", "PALMS UP TOGETHER", "\ud83e\udd32\ud83c\udffd", "palms_up_together", new string[1] { "palms_up_together" }, null, null, "Smileys & People", 39, 19, SkinVariationType.Medium, "palms up together", new string[2] { "palms up together", "prayer" });

	public static readonly Emoji PalmsUpTogether_MediumDark = new Emoji("1F932-1F3FE", "PALMS UP TOGETHER", "\ud83e\udd32\ud83c\udffe", "palms_up_together", new string[1] { "palms_up_together" }, null, null, "Smileys & People", 39, 20, SkinVariationType.MediumDark, "palms up together", new string[2] { "palms up together", "prayer" });

	public static readonly Emoji PalmsUpTogether_Dark = new Emoji("1F932-1F3FF", "PALMS UP TOGETHER", "\ud83e\udd32\ud83c\udfff", "palms_up_together", new string[1] { "palms_up_together" }, null, null, "Smileys & People", 39, 21, SkinVariationType.Dark, "palms up together", new string[2] { "palms up together", "prayer" });

	public static readonly Emoji Pray = new Emoji("1F64F", "PERSON WITH FOLDED HANDS", "\ud83d\ude4f", "pray", new string[1] { "pray" }, null, null, "Smileys & People", 34, 2, SkinVariationType.None, "folded hands", new string[9] { "ask", "bow", "folded", "folded hands", "gesture", "hand", "please", "pray", "thanks" });

	public static readonly Emoji Pray_Light = new Emoji("1F64F-1F3FB", "PERSON WITH FOLDED HANDS", "\ud83d\ude4f\ud83c\udffb", "pray", new string[1] { "pray" }, null, null, "Smileys & People", 34, 3, SkinVariationType.Light, "folded hands", new string[9] { "ask", "bow", "folded", "folded hands", "gesture", "hand", "please", "pray", "thanks" });

	public static readonly Emoji Pray_MediumLight = new Emoji("1F64F-1F3FC", "PERSON WITH FOLDED HANDS", "\ud83d\ude4f\ud83c\udffc", "pray", new string[1] { "pray" }, null, null, "Smileys & People", 34, 4, SkinVariationType.MediumLight, "folded hands", new string[9] { "ask", "bow", "folded", "folded hands", "gesture", "hand", "please", "pray", "thanks" });

	public static readonly Emoji Pray_Medium = new Emoji("1F64F-1F3FD", "PERSON WITH FOLDED HANDS", "\ud83d\ude4f\ud83c\udffd", "pray", new string[1] { "pray" }, null, null, "Smileys & People", 34, 5, SkinVariationType.Medium, "folded hands", new string[9] { "ask", "bow", "folded", "folded hands", "gesture", "hand", "please", "pray", "thanks" });

	public static readonly Emoji Pray_MediumDark = new Emoji("1F64F-1F3FE", "PERSON WITH FOLDED HANDS", "\ud83d\ude4f\ud83c\udffe", "pray", new string[1] { "pray" }, null, null, "Smileys & People", 34, 6, SkinVariationType.MediumDark, "folded hands", new string[9] { "ask", "bow", "folded", "folded hands", "gesture", "hand", "please", "pray", "thanks" });

	public static readonly Emoji Pray_Dark = new Emoji("1F64F-1F3FF", "PERSON WITH FOLDED HANDS", "\ud83d\ude4f\ud83c\udfff", "pray", new string[1] { "pray" }, null, null, "Smileys & People", 34, 7, SkinVariationType.Dark, "folded hands", new string[9] { "ask", "bow", "folded", "folded hands", "gesture", "hand", "please", "pray", "thanks" });

	public static readonly Emoji Handshake = new Emoji("1F91D", "HANDSHAKE", "\ud83e\udd1d", "handshake", new string[1] { "handshake" }, null, null, "Smileys & People", 38, 10, SkinVariationType.None, "handshake", new string[5] { "agreement", "hand", "handshake", "meeting", "shake" });

	public static readonly Emoji NailCare = new Emoji("1F485", "NAIL POLISH", "\ud83d\udc85", "nail_care", new string[1] { "nail_care" }, null, null, "Smileys & People", 23, 44, SkinVariationType.None, "nail polish", new string[5] { "care", "cosmetics", "manicure", "nail", "polish" });

	public static readonly Emoji NailCare_Light = new Emoji("1F485-1F3FB", "NAIL POLISH", "\ud83d\udc85\ud83c\udffb", "nail_care", new string[1] { "nail_care" }, null, null, "Smileys & People", 23, 45, SkinVariationType.Light, "nail polish", new string[5] { "care", "cosmetics", "manicure", "nail", "polish" });

	public static readonly Emoji NailCare_MediumLight = new Emoji("1F485-1F3FC", "NAIL POLISH", "\ud83d\udc85\ud83c\udffc", "nail_care", new string[1] { "nail_care" }, null, null, "Smileys & People", 23, 46, SkinVariationType.MediumLight, "nail polish", new string[5] { "care", "cosmetics", "manicure", "nail", "polish" });

	public static readonly Emoji NailCare_Medium = new Emoji("1F485-1F3FD", "NAIL POLISH", "\ud83d\udc85\ud83c\udffd", "nail_care", new string[1] { "nail_care" }, null, null, "Smileys & People", 23, 47, SkinVariationType.Medium, "nail polish", new string[5] { "care", "cosmetics", "manicure", "nail", "polish" });

	public static readonly Emoji NailCare_MediumDark = new Emoji("1F485-1F3FE", "NAIL POLISH", "\ud83d\udc85\ud83c\udffe", "nail_care", new string[1] { "nail_care" }, null, null, "Smileys & People", 23, 48, SkinVariationType.MediumDark, "nail polish", new string[5] { "care", "cosmetics", "manicure", "nail", "polish" });

	public static readonly Emoji NailCare_Dark = new Emoji("1F485-1F3FF", "NAIL POLISH", "\ud83d\udc85\ud83c\udfff", "nail_care", new string[1] { "nail_care" }, null, null, "Smileys & People", 23, 49, SkinVariationType.Dark, "nail polish", new string[5] { "care", "cosmetics", "manicure", "nail", "polish" });

	public static readonly Emoji Ear = new Emoji("1F442", "EAR", "\ud83d\udc42", "ear", new string[1] { "ear" }, null, null, "Smileys & People", 13, 45, SkinVariationType.None, "ear", new string[2] { "body", "ear" });

	public static readonly Emoji Ear_Light = new Emoji("1F442-1F3FB", "EAR", "\ud83d\udc42\ud83c\udffb", "ear", new string[1] { "ear" }, null, null, "Smileys & People", 13, 46, SkinVariationType.Light, "ear", new string[2] { "body", "ear" });

	public static readonly Emoji Ear_MediumLight = new Emoji("1F442-1F3FC", "EAR", "\ud83d\udc42\ud83c\udffc", "ear", new string[1] { "ear" }, null, null, "Smileys & People", 13, 47, SkinVariationType.MediumLight, "ear", new string[2] { "body", "ear" });

	public static readonly Emoji Ear_Medium = new Emoji("1F442-1F3FD", "EAR", "\ud83d\udc42\ud83c\udffd", "ear", new string[1] { "ear" }, null, null, "Smileys & People", 13, 48, SkinVariationType.Medium, "ear", new string[2] { "body", "ear" });

	public static readonly Emoji Ear_MediumDark = new Emoji("1F442-1F3FE", "EAR", "\ud83d\udc42\ud83c\udffe", "ear", new string[1] { "ear" }, null, null, "Smileys & People", 13, 49, SkinVariationType.MediumDark, "ear", new string[2] { "body", "ear" });

	public static readonly Emoji Ear_Dark = new Emoji("1F442-1F3FF", "EAR", "\ud83d\udc42\ud83c\udfff", "ear", new string[1] { "ear" }, null, null, "Smileys & People", 13, 50, SkinVariationType.Dark, "ear", new string[2] { "body", "ear" });

	public static readonly Emoji Nose = new Emoji("1F443", "NOSE", "\ud83d\udc43", "nose", new string[1] { "nose" }, null, null, "Smileys & People", 13, 51, SkinVariationType.None, "nose", new string[2] { "body", "nose" });

	public static readonly Emoji Nose_Light = new Emoji("1F443-1F3FB", "NOSE", "\ud83d\udc43\ud83c\udffb", "nose", new string[1] { "nose" }, null, null, "Smileys & People", 14, 0, SkinVariationType.Light, "nose", new string[2] { "body", "nose" });

	public static readonly Emoji Nose_MediumLight = new Emoji("1F443-1F3FC", "NOSE", "\ud83d\udc43\ud83c\udffc", "nose", new string[1] { "nose" }, null, null, "Smileys & People", 14, 1, SkinVariationType.MediumLight, "nose", new string[2] { "body", "nose" });

	public static readonly Emoji Nose_Medium = new Emoji("1F443-1F3FD", "NOSE", "\ud83d\udc43\ud83c\udffd", "nose", new string[1] { "nose" }, null, null, "Smileys & People", 14, 2, SkinVariationType.Medium, "nose", new string[2] { "body", "nose" });

	public static readonly Emoji Nose_MediumDark = new Emoji("1F443-1F3FE", "NOSE", "\ud83d\udc43\ud83c\udffe", "nose", new string[1] { "nose" }, null, null, "Smileys & People", 14, 3, SkinVariationType.MediumDark, "nose", new string[2] { "body", "nose" });

	public static readonly Emoji Nose_Dark = new Emoji("1F443-1F3FF", "NOSE", "\ud83d\udc43\ud83c\udfff", "nose", new string[1] { "nose" }, null, null, "Smileys & People", 14, 4, SkinVariationType.Dark, "nose", new string[2] { "body", "nose" });

	public static readonly Emoji Footprints = new Emoji("1F463", "FOOTPRINTS", "\ud83d\udc63", "footprints", new string[1] { "footprints" }, null, null, "Smileys & People", 15, 39, SkinVariationType.None, "footprints", new string[4] { "clothing", "footprint", "footprints", "print" });

	public static readonly Emoji Eyes = new Emoji("1F440", "EYES", "\ud83d\udc40", "eyes", new string[1] { "eyes" }, null, null, "Smileys & People", 13, 42, SkinVariationType.None, "eyes", new string[3] { "eye", "eyes", "face" });

	public static readonly Emoji Eye = new Emoji("1F441-FE0F", null, "\ud83d\udc41\ufe0f", "eye", new string[1] { "eye" }, null, null, "Smileys & People", 13, 44, SkinVariationType.None, null, null);

	public static readonly Emoji EyeInSpeechBubble = new Emoji("1F441-FE0F-200D-1F5E8-FE0F", null, "\ud83d\udc41\ufe0f\u200d\ud83d\udde8\ufe0f", "eye-in-speech-bubble", new string[1] { "eye-in-speech-bubble" }, null, null, "Smileys & People", 13, 43, SkinVariationType.None, null, null);

	public static readonly Emoji Brain = new Emoji("1F9E0", "BRAIN", "\ud83e\udde0", "brain", new string[1] { "brain" }, null, null, "Smileys & People", 46, 22, SkinVariationType.None, "brain", new string[2] { "brain", "intelligent" });

	public static readonly Emoji Tongue = new Emoji("1F445", "TONGUE", "\ud83d\udc45", "tongue", new string[1] { "tongue" }, null, null, "Smileys & People", 14, 6, SkinVariationType.None, "tongue", new string[2] { "body", "tongue" });

	public static readonly Emoji Lips = new Emoji("1F444", "MOUTH", "\ud83d\udc44", "lips", new string[1] { "lips" }, null, null, "Smileys & People", 14, 5, SkinVariationType.None, "mouth", new string[2] { "lips", "mouth" });

	public static readonly Emoji SkinTone2 = new Emoji("1F3FB", "EMOJI MODIFIER FITZPATRICK TYPE-1-2", "\ud83c\udffb", "skin-tone-2", new string[1] { "skin-tone-2" }, null, null, "Skin Tones", 12, 25, SkinVariationType.None, "light skin tone", new string[3] { "light skin tone", "skin tone", "type 1–2" });

	public static readonly Emoji SkinTone3 = new Emoji("1F3FC", "EMOJI MODIFIER FITZPATRICK TYPE-3", "\ud83c\udffc", "skin-tone-3", new string[1] { "skin-tone-3" }, null, null, "Skin Tones", 12, 26, SkinVariationType.None, "medium-light skin tone", new string[3] { "medium-light skin tone", "skin tone", "type 3" });

	public static readonly Emoji SkinTone4 = new Emoji("1F3FD", "EMOJI MODIFIER FITZPATRICK TYPE-4", "\ud83c\udffd", "skin-tone-4", new string[1] { "skin-tone-4" }, null, null, "Skin Tones", 12, 27, SkinVariationType.None, "medium skin tone", new string[3] { "medium skin tone", "skin tone", "type 4" });

	public static readonly Emoji SkinTone5 = new Emoji("1F3FE", "EMOJI MODIFIER FITZPATRICK TYPE-5", "\ud83c\udffe", "skin-tone-5", new string[1] { "skin-tone-5" }, null, null, "Skin Tones", 12, 28, SkinVariationType.None, "medium-dark skin tone", new string[3] { "medium-dark skin tone", "skin tone", "type 5" });

	public static readonly Emoji SkinTone6 = new Emoji("1F3FF", "EMOJI MODIFIER FITZPATRICK TYPE-6", "\ud83c\udfff", "skin-tone-6", new string[1] { "skin-tone-6" }, null, null, "Skin Tones", 12, 29, SkinVariationType.None, "dark skin tone", new string[3] { "dark skin tone", "skin tone", "type 6" });

	public static readonly Emoji Kiss = new Emoji("1F48B", "KISS MARK", "\ud83d\udc8b", "kiss", new string[1] { "kiss" }, null, null, "Smileys & People", 24, 37, SkinVariationType.None, "kiss mark", new string[3] { "kiss", "kiss mark", "lips" });

	public static readonly Emoji Cupid = new Emoji("1F498", "HEART WITH ARROW", "\ud83d\udc98", "cupid", new string[1] { "cupid" }, null, null, "Smileys & People", 24, 50, SkinVariationType.None, "heart with arrow", new string[3] { "arrow", "cupid", "heart with arrow" });

	public static readonly Emoji GiftHeart = new Emoji("1F49D", "HEART WITH RIBBON", "\ud83d\udc9d", "gift_heart", new string[1] { "gift_heart" }, null, null, "Smileys & People", 25, 3, SkinVariationType.None, "heart with ribbon", new string[3] { "heart with ribbon", "ribbon", "valentine" });

	public static readonly Emoji SparklingHeart = new Emoji("1F496", "SPARKLING HEART", "\ud83d\udc96", "sparkling_heart", new string[1] { "sparkling_heart" }, null, null, "Smileys & People", 24, 48, SkinVariationType.None, "sparkling heart", new string[3] { "excited", "sparkle", "sparkling heart" });

	public static readonly Emoji Heartpulse = new Emoji("1F497", "GROWING HEART", "\ud83d\udc97", "heartpulse", new string[1] { "heartpulse" }, null, null, "Smileys & People", 24, 49, SkinVariationType.None, "growing heart", new string[5] { "excited", "growing", "growing heart", "nervous", "pulse" });

	public static readonly Emoji Heartbeat = new Emoji("1F493", "BEATING HEART", "\ud83d\udc93", "heartbeat", new string[1] { "heartbeat" }, null, null, "Smileys & People", 24, 45, SkinVariationType.None, "beating heart", new string[4] { "beating", "beating heart", "heartbeat", "pulsating" });

	public static readonly Emoji RevolvingHearts = new Emoji("1F49E", "REVOLVING HEARTS", "\ud83d\udc9e", "revolving_hearts", new string[1] { "revolving_hearts" }, null, null, "Smileys & People", 25, 4, SkinVariationType.None, "revolving hearts", new string[2] { "revolving", "revolving hearts" });

	public static readonly Emoji TwoHearts = new Emoji("1F495", "TWO HEARTS", "\ud83d\udc95", "two_hearts", new string[1] { "two_hearts" }, null, null, "Smileys & People", 24, 47, SkinVariationType.None, "two hearts", new string[2] { "love", "two hearts" });

	public static readonly Emoji LoveLetter = new Emoji("1F48C", "LOVE LETTER", "\ud83d\udc8c", "love_letter", new string[1] { "love_letter" }, null, null, "Smileys & People", 24, 38, SkinVariationType.None, "love letter", new string[4] { "heart", "letter", "love", "mail" });

	public static readonly Emoji HeavyHeartExclamationMarkOrnament = new Emoji("2763-FE0F", null, "❣\ufe0f", "heavy_heart_exclamation_mark_ornament", new string[1] { "heavy_heart_exclamation_mark_ornament" }, null, null, "Smileys & People", 50, 7, SkinVariationType.None, null, null);

	public static readonly Emoji BrokenHeart = new Emoji("1F494", "BROKEN HEART", "\ud83d\udc94", "broken_heart", new string[1] { "broken_heart" }, "</3", new string[1] { "</3" }, "Smileys & People", 24, 46, SkinVariationType.None, "broken heart", new string[3] { "break", "broken", "broken heart" });

	public static readonly Emoji Heart = new Emoji("2764-FE0F", "HEAVY BLACK HEART", "❤\ufe0f", "heart", new string[1] { "heart" }, "<3", new string[1] { "<3" }, "Smileys & People", 50, 8, SkinVariationType.None, null, null);

	public static readonly Emoji OrangeHeart = new Emoji("1F9E1", "ORANGE HEART", "\ud83e\udde1", "orange_heart", new string[1] { "orange_heart" }, null, null, "Smileys & People", 46, 23, SkinVariationType.None, "orange heart", new string[2] { "orange", "orange heart" });

	public static readonly Emoji YellowHeart = new Emoji("1F49B", "YELLOW HEART", "\ud83d\udc9b", "yellow_heart", new string[1] { "yellow_heart" }, "<3", null, "Smileys & People", 25, 1, SkinVariationType.None, "yellow heart", new string[2] { "yellow", "yellow heart" });

	public static readonly Emoji GreenHeart = new Emoji("1F49A", "GREEN HEART", "\ud83d\udc9a", "green_heart", new string[1] { "green_heart" }, "<3", null, "Smileys & People", 25, 0, SkinVariationType.None, "green heart", new string[2] { "green", "green heart" });

	public static readonly Emoji BlueHeart = new Emoji("1F499", "BLUE HEART", "\ud83d\udc99", "blue_heart", new string[1] { "blue_heart" }, "<3", null, "Smileys & People", 24, 51, SkinVariationType.None, "blue heart", new string[2] { "blue", "blue heart" });

	public static readonly Emoji PurpleHeart = new Emoji("1F49C", "PURPLE HEART", "\ud83d\udc9c", "purple_heart", new string[1] { "purple_heart" }, "<3", null, "Smileys & People", 25, 2, SkinVariationType.None, "purple heart", new string[2] { "purple", "purple heart" });

	public static readonly Emoji BlackHeart = new Emoji("1F5A4", "BLACK HEART", "\ud83d\udda4", "black_heart", new string[1] { "black_heart" }, null, null, "Smileys & People", 29, 50, SkinVariationType.None, "black heart", new string[4] { "black", "black heart", "evil", "wicked" });

	public static readonly Emoji HeartDecoration = new Emoji("1F49F", "HEART DECORATION", "\ud83d\udc9f", "heart_decoration", new string[1] { "heart_decoration" }, null, null, "Smileys & People", 25, 5, SkinVariationType.None, "heart decoration", new string[2] { "heart", "heart decoration" });

	public static readonly Emoji Zzz = new Emoji("1F4A4", "SLEEPING SYMBOL", "\ud83d\udca4", "zzz", new string[1] { "zzz" }, null, null, "Smileys & People", 25, 10, SkinVariationType.None, "zzz", new string[3] { "comic", "sleep", "zzz" });

	public static readonly Emoji Anger = new Emoji("1F4A2", "ANGER SYMBOL", "\ud83d\udca2", "anger", new string[1] { "anger" }, null, null, "Smileys & People", 25, 8, SkinVariationType.None, "anger symbol", new string[4] { "anger symbol", "angry", "comic", "mad" });

	public static readonly Emoji Bomb = new Emoji("1F4A3", "BOMB", "\ud83d\udca3", "bomb", new string[1] { "bomb" }, null, null, "Smileys & People", 25, 9, SkinVariationType.None, "bomb", new string[2] { "bomb", "comic" });

	public static readonly Emoji Boom = new Emoji("1F4A5", "COLLISION SYMBOL", "\ud83d\udca5", "boom", new string[2] { "boom", "collision" }, null, null, "Smileys & People", 25, 11, SkinVariationType.None, "collision", new string[3] { "boom", "collision", "comic" });

	public static readonly Emoji SweatDrops = new Emoji("1F4A6", "SPLASHING SWEAT SYMBOL", "\ud83d\udca6", "sweat_drops", new string[1] { "sweat_drops" }, null, null, "Smileys & People", 25, 12, SkinVariationType.None, "sweat droplets", new string[4] { "comic", "splashing", "sweat", "sweat droplets" });

	public static readonly Emoji Dash = new Emoji("1F4A8", "DASH SYMBOL", "\ud83d\udca8", "dash", new string[1] { "dash" }, null, null, "Smileys & People", 25, 14, SkinVariationType.None, "dashing away", new string[4] { "comic", "dash", "dashing away", "running" });

	public static readonly Emoji Dizzy = new Emoji("1F4AB", "DIZZY SYMBOL", "\ud83d\udcab", "dizzy", new string[1] { "dizzy" }, null, null, "Smileys & People", 25, 22, SkinVariationType.None, "dizzy", new string[3] { "comic", "dizzy", "star" });

	public static readonly Emoji SpeechBalloon = new Emoji("1F4AC", "SPEECH BALLOON", "\ud83d\udcac", "speech_balloon", new string[1] { "speech_balloon" }, null, null, "Smileys & People", 25, 23, SkinVariationType.None, "speech balloon", new string[5] { "balloon", "bubble", "comic", "dialog", "speech" });

	public static readonly Emoji LeftSpeechBubble = new Emoji("1F5E8-FE0F", null, "\ud83d\udde8\ufe0f", "left_speech_bubble", new string[1] { "left_speech_bubble" }, null, null, "Smileys & People", 30, 15, SkinVariationType.None, null, null);

	public static readonly Emoji RightAngerBubble = new Emoji("1F5EF-FE0F", null, "\ud83d\uddef\ufe0f", "right_anger_bubble", new string[1] { "right_anger_bubble" }, null, null, "Smileys & People", 30, 16, SkinVariationType.None, null, null);

	public static readonly Emoji ThoughtBalloon = new Emoji("1F4AD", "THOUGHT BALLOON", "\ud83d\udcad", "thought_balloon", new string[1] { "thought_balloon" }, null, null, "Smileys & People", 25, 24, SkinVariationType.None, "thought balloon", new string[4] { "balloon", "bubble", "comic", "thought" });

	public static readonly Emoji Hole = new Emoji("1F573-FE0F", null, "\ud83d\udd73\ufe0f", "hole", new string[1] { "hole" }, null, null, "Smileys & People", 28, 44, SkinVariationType.None, null, null);

	public static readonly Emoji Eyeglasses = new Emoji("1F453", "EYEGLASSES", "\ud83d\udc53", "eyeglasses", new string[1] { "eyeglasses" }, null, null, "Smileys & People", 15, 23, SkinVariationType.None, "glasses", new string[5] { "clothing", "eye", "eyeglasses", "eyewear", "glasses" });

	public static readonly Emoji DarkSunglasses = new Emoji("1F576-FE0F", null, "\ud83d\udd76\ufe0f", "dark_sunglasses", new string[1] { "dark_sunglasses" }, null, null, "Smileys & People", 29, 17, SkinVariationType.None, null, null);

	public static readonly Emoji Necktie = new Emoji("1F454", "NECKTIE", "\ud83d\udc54", "necktie", new string[1] { "necktie" }, null, null, "Smileys & People", 15, 24, SkinVariationType.None, "necktie", new string[3] { "clothing", "necktie", "tie" });

	public static readonly Emoji Shirt = new Emoji("1F455", "T-SHIRT", "\ud83d\udc55", "shirt", new string[2] { "shirt", "tshirt" }, null, null, "Smileys & People", 15, 25, SkinVariationType.None, "t-shirt", new string[4] { "clothing", "shirt", "t-shirt", "tshirt" });

	public static readonly Emoji Jeans = new Emoji("1F456", "JEANS", "\ud83d\udc56", "jeans", new string[1] { "jeans" }, null, null, "Smileys & People", 15, 26, SkinVariationType.None, "jeans", new string[4] { "clothing", "jeans", "pants", "trousers" });

	public static readonly Emoji Scarf = new Emoji("1F9E3", "SCARF", "\ud83e\udde3", "scarf", new string[1] { "scarf" }, null, null, "Smileys & People", 46, 25, SkinVariationType.None, "scarf", new string[2] { "neck", "scarf" });

	public static readonly Emoji Gloves = new Emoji("1F9E4", "GLOVES", "\ud83e\udde4", "gloves", new string[1] { "gloves" }, null, null, "Smileys & People", 46, 26, SkinVariationType.None, "gloves", new string[2] { "gloves", "hand" });

	public static readonly Emoji Coat = new Emoji("1F9E5", "COAT", "\ud83e\udde5", "coat", new string[1] { "coat" }, null, null, "Smileys & People", 46, 27, SkinVariationType.None, "coat", new string[2] { "coat", "jacket" });

	public static readonly Emoji Socks = new Emoji("1F9E6", "SOCKS", "\ud83e\udde6", "socks", new string[1] { "socks" }, null, null, "Smileys & People", 46, 28, SkinVariationType.None, "socks", new string[2] { "socks", "stocking" });

	public static readonly Emoji Dress = new Emoji("1F457", "DRESS", "\ud83d\udc57", "dress", new string[1] { "dress" }, null, null, "Smileys & People", 15, 27, SkinVariationType.None, "dress", new string[2] { "clothing", "dress" });

	public static readonly Emoji Kimono = new Emoji("1F458", "KIMONO", "\ud83d\udc58", "kimono", new string[1] { "kimono" }, null, null, "Smileys & People", 15, 28, SkinVariationType.None, "kimono", new string[2] { "clothing", "kimono" });

	public static readonly Emoji Bikini = new Emoji("1F459", "BIKINI", "\ud83d\udc59", "bikini", new string[1] { "bikini" }, null, null, "Smileys & People", 15, 29, SkinVariationType.None, "bikini", new string[3] { "bikini", "clothing", "swim" });

	public static readonly Emoji WomansClothes = new Emoji("1F45A", "WOMANS CLOTHES", "\ud83d\udc5a", "womans_clothes", new string[1] { "womans_clothes" }, null, null, "Smileys & People", 15, 30, SkinVariationType.None, "woman’s clothes", new string[3] { "clothing", "woman", "woman’s clothes" });

	public static readonly Emoji Purse = new Emoji("1F45B", "PURSE", "\ud83d\udc5b", "purse", new string[1] { "purse" }, null, null, "Smileys & People", 15, 31, SkinVariationType.None, "purse", new string[3] { "clothing", "coin", "purse" });

	public static readonly Emoji Handbag = new Emoji("1F45C", "HANDBAG", "\ud83d\udc5c", "handbag", new string[1] { "handbag" }, null, null, "Smileys & People", 15, 32, SkinVariationType.None, "handbag", new string[4] { "bag", "clothing", "handbag", "purse" });

	public static readonly Emoji Pouch = new Emoji("1F45D", "POUCH", "\ud83d\udc5d", "pouch", new string[1] { "pouch" }, null, null, "Smileys & People", 15, 33, SkinVariationType.None, "clutch bag", new string[4] { "bag", "clothing", "clutch bag", "pouch" });

	public static readonly Emoji ShoppingBags = new Emoji("1F6CD-FE0F", null, "\ud83d\udecd\ufe0f", "shopping_bags", new string[1] { "shopping_bags" }, null, null, "Smileys & People", 37, 2, SkinVariationType.None, null, null);

	public static readonly Emoji SchoolSatchel = new Emoji("1F392", "SCHOOL SATCHEL", "\ud83c\udf92", "school_satchel", new string[1] { "school_satchel" }, null, null, "Smileys & People", 8, 37, SkinVariationType.None, "backpack", new string[5] { "backpack", "bag", "rucksack", "satchel", "school" });

	public static readonly Emoji MansShoe = new Emoji("1F45E", "MANS SHOE", "\ud83d\udc5e", "mans_shoe", new string[2] { "mans_shoe", "shoe" }, null, null, "Smileys & People", 15, 34, SkinVariationType.None, "man’s shoe", new string[4] { "clothing", "man", "man’s shoe", "shoe" });

	public static readonly Emoji AthleticShoe = new Emoji("1F45F", "ATHLETIC SHOE", "\ud83d\udc5f", "athletic_shoe", new string[1] { "athletic_shoe" }, null, null, "Smileys & People", 15, 35, SkinVariationType.None, "running shoe", new string[5] { "athletic", "clothing", "running shoe", "shoe", "sneaker" });

	public static readonly Emoji HighHeel = new Emoji("1F460", "HIGH-HEELED SHOE", "\ud83d\udc60", "high_heel", new string[1] { "high_heel" }, null, null, "Smileys & People", 15, 36, SkinVariationType.None, "high-heeled shoe", new string[5] { "clothing", "heel", "high-heeled shoe", "shoe", "woman" });

	public static readonly Emoji Sandal = new Emoji("1F461", "WOMANS SANDAL", "\ud83d\udc61", "sandal", new string[1] { "sandal" }, null, null, "Smileys & People", 15, 37, SkinVariationType.None, "woman’s sandal", new string[5] { "clothing", "sandal", "shoe", "woman", "woman’s sandal" });

	public static readonly Emoji Boot = new Emoji("1F462", "WOMANS BOOTS", "\ud83d\udc62", "boot", new string[1] { "boot" }, null, null, "Smileys & People", 15, 38, SkinVariationType.None, "woman’s boot", new string[5] { "boot", "clothing", "shoe", "woman", "woman’s boot" });

	public static readonly Emoji Crown = new Emoji("1F451", "CROWN", "\ud83d\udc51", "crown", new string[1] { "crown" }, null, null, "Smileys & People", 15, 21, SkinVariationType.None, "crown", new string[4] { "clothing", "crown", "king", "queen" });

	public static readonly Emoji WomansHat = new Emoji("1F452", "WOMANS HAT", "\ud83d\udc52", "womans_hat", new string[1] { "womans_hat" }, null, null, "Smileys & People", 15, 22, SkinVariationType.None, "woman’s hat", new string[4] { "clothing", "hat", "woman", "woman’s hat" });

	public static readonly Emoji Tophat = new Emoji("1F3A9", "TOP HAT", "\ud83c\udfa9", "tophat", new string[1] { "tophat" }, null, null, "Smileys & People", 9, 3, SkinVariationType.None, "top hat", new string[4] { "clothing", "hat", "top", "tophat" });

	public static readonly Emoji MortarBoard = new Emoji("1F393", "GRADUATION CAP", "\ud83c\udf93", "mortar_board", new string[1] { "mortar_board" }, null, null, "Smileys & People", 8, 38, SkinVariationType.None, "graduation cap", new string[5] { "cap", "celebration", "clothing", "graduation", "hat" });

	public static readonly Emoji BilledCap = new Emoji("1F9E2", "BILLED CAP", "\ud83e\udde2", "billed_cap", new string[1] { "billed_cap" }, null, null, "Smileys & People", 46, 24, SkinVariationType.None, "billed cap", new string[2] { "baseball cap", "billed cap" });

	public static readonly Emoji HelmetWithWhiteCross = new Emoji("26D1-FE0F", null, "⛑\ufe0f", "helmet_with_white_cross", new string[1] { "helmet_with_white_cross" }, null, null, "Smileys & People", 48, 33, SkinVariationType.None, null, null);

	public static readonly Emoji PrayerBeads = new Emoji("1F4FF", "PRAYER BEADS", "\ud83d\udcff", "prayer_beads", new string[1] { "prayer_beads" }, null, null, "Smileys & People", 27, 1, SkinVariationType.None, "prayer beads", new string[5] { "beads", "clothing", "necklace", "prayer", "religion" });

	public static readonly Emoji Lipstick = new Emoji("1F484", "LIPSTICK", "\ud83d\udc84", "lipstick", new string[1] { "lipstick" }, null, null, "Smileys & People", 23, 43, SkinVariationType.None, "lipstick", new string[3] { "cosmetics", "lipstick", "makeup" });

	public static readonly Emoji Ring = new Emoji("1F48D", "RING", "\ud83d\udc8d", "ring", new string[1] { "ring" }, null, null, "Smileys & People", 24, 39, SkinVariationType.None, "ring", new string[2] { "diamond", "ring" });

	public static readonly Emoji Gem = new Emoji("1F48E", "GEM STONE", "\ud83d\udc8e", "gem", new string[1] { "gem" }, null, null, "Smileys & People", 24, 40, SkinVariationType.None, "gem stone", new string[4] { "diamond", "gem", "gem stone", "jewel" });

	public static readonly Emoji MonkeyFace = new Emoji("1F435", "MONKEY FACE", "\ud83d\udc35", "monkey_face", new string[1] { "monkey_face" }, null, new string[1] { ":o)" }, "Animals & Nature", 13, 31, SkinVariationType.None, "monkey face", new string[2] { "face", "monkey" });

	public static readonly Emoji Monkey = new Emoji("1F412", "MONKEY", "\ud83d\udc12", "monkey", new string[1] { "monkey" }, null, null, "Animals & Nature", 12, 48, SkinVariationType.None, "monkey", new string[1] { "monkey" });

	public static readonly Emoji Gorilla = new Emoji("1F98D", "GORILLA", "\ud83e\udd8d", "gorilla", new string[1] { "gorilla" }, null, null, "Animals & Nature", 42, 37, SkinVariationType.None, "gorilla", new string[1] { "gorilla" });

	public static readonly Emoji Dog = new Emoji("1F436", "DOG FACE", "\ud83d\udc36", "dog", new string[1] { "dog" }, null, null, "Animals & Nature", 13, 32, SkinVariationType.None, "dog face", new string[3] { "dog", "face", "pet" });

	public static readonly Emoji Dog2 = new Emoji("1F415", "DOG", "\ud83d\udc15", "dog2", new string[1] { "dog2" }, null, null, "Animals & Nature", 12, 51, SkinVariationType.None, "dog", new string[2] { "dog", "pet" });

	public static readonly Emoji Poodle = new Emoji("1F429", "POODLE", "\ud83d\udc29", "poodle", new string[1] { "poodle" }, null, null, "Animals & Nature", 13, 19, SkinVariationType.None, "poodle", new string[2] { "dog", "poodle" });

	public static readonly Emoji Wolf = new Emoji("1F43A", "WOLF FACE", "\ud83d\udc3a", "wolf", new string[1] { "wolf" }, null, null, "Animals & Nature", 13, 36, SkinVariationType.None, "wolf face", new string[2] { "face", "wolf" });

	public static readonly Emoji FoxFace = new Emoji("1F98A", "FOX FACE", "\ud83e\udd8a", "fox_face", new string[1] { "fox_face" }, null, null, "Animals & Nature", 42, 34, SkinVariationType.None, "fox face", new string[2] { "face", "fox" });

	public static readonly Emoji Cat = new Emoji("1F431", "CAT FACE", "\ud83d\udc31", "cat", new string[1] { "cat" }, null, null, "Animals & Nature", 13, 27, SkinVariationType.None, "cat face", new string[3] { "cat", "face", "pet" });

	public static readonly Emoji Cat2 = new Emoji("1F408", "CAT", "\ud83d\udc08", "cat2", new string[1] { "cat2" }, null, null, "Animals & Nature", 12, 38, SkinVariationType.None, "cat", new string[2] { "cat", "pet" });

	public static readonly Emoji LionFace = new Emoji("1F981", "LION FACE", "\ud83e\udd81", "lion_face", new string[1] { "lion_face" }, null, null, "Animals & Nature", 42, 25, SkinVariationType.None, "lion face", new string[4] { "face", "Leo", "lion", "zodiac" });

	public static readonly Emoji Tiger = new Emoji("1F42F", "TIGER FACE", "\ud83d\udc2f", "tiger", new string[1] { "tiger" }, null, null, "Animals & Nature", 13, 25, SkinVariationType.None, "tiger face", new string[2] { "face", "tiger" });

	public static readonly Emoji Tiger2 = new Emoji("1F405", "TIGER", "\ud83d\udc05", "tiger2", new string[1] { "tiger2" }, null, null, "Animals & Nature", 12, 35, SkinVariationType.None, "tiger", new string[1] { "tiger" });

	public static readonly Emoji Leopard = new Emoji("1F406", "LEOPARD", "\ud83d\udc06", "leopard", new string[1] { "leopard" }, null, null, "Animals & Nature", 12, 36, SkinVariationType.None, "leopard", new string[1] { "leopard" });

	public static readonly Emoji Horse = new Emoji("1F434", "HORSE FACE", "\ud83d\udc34", "horse", new string[1] { "horse" }, null, null, "Animals & Nature", 13, 30, SkinVariationType.None, "horse face", new string[2] { "face", "horse" });

	public static readonly Emoji Racehorse = new Emoji("1F40E", "HORSE", "\ud83d\udc0e", "racehorse", new string[1] { "racehorse" }, null, null, "Animals & Nature", 12, 44, SkinVariationType.None, "horse", new string[4] { "equestrian", "horse", "racehorse", "racing" });

	public static readonly Emoji UnicornFace = new Emoji("1F984", "UNICORN FACE", "\ud83e\udd84", "unicorn_face", new string[1] { "unicorn_face" }, null, null, "Animals & Nature", 42, 28, SkinVariationType.None, "unicorn face", new string[2] { "face", "unicorn" });

	public static readonly Emoji ZebraFace = new Emoji("1F993", "ZEBRA FACE", "\ud83e\udd93", "zebra_face", new string[1] { "zebra_face" }, null, null, "Animals & Nature", 42, 43, SkinVariationType.None, "zebra", new string[2] { "stripe", "zebra" });

	public static readonly Emoji Deer = new Emoji("1F98C", "DEER", "\ud83e\udd8c", "deer", new string[1] { "deer" }, null, null, "Animals & Nature", 42, 36, SkinVariationType.None, "deer", new string[1] { "deer" });

	public static readonly Emoji Cow = new Emoji("1F42E", "COW FACE", "\ud83d\udc2e", "cow", new string[1] { "cow" }, null, null, "Animals & Nature", 13, 24, SkinVariationType.None, "cow face", new string[2] { "cow", "face" });

	public static readonly Emoji Ox = new Emoji("1F402", "OX", "\ud83d\udc02", "ox", new string[1] { "ox" }, null, null, "Animals & Nature", 12, 32, SkinVariationType.None, "ox", new string[4] { "bull", "ox", "Taurus", "zodiac" });

	public static readonly Emoji WaterBuffalo = new Emoji("1F403", "WATER BUFFALO", "\ud83d\udc03", "water_buffalo", new string[1] { "water_buffalo" }, null, null, "Animals & Nature", 12, 33, SkinVariationType.None, "water buffalo", new string[2] { "buffalo", "water" });

	public static readonly Emoji Cow2 = new Emoji("1F404", "COW", "\ud83d\udc04", "cow2", new string[1] { "cow2" }, null, null, "Animals & Nature", 12, 34, SkinVariationType.None, "cow", new string[1] { "cow" });

	public static readonly Emoji Pig = new Emoji("1F437", "PIG FACE", "\ud83d\udc37", "pig", new string[1] { "pig" }, null, null, "Animals & Nature", 13, 33, SkinVariationType.None, "pig face", new string[2] { "face", "pig" });

	public static readonly Emoji Pig2 = new Emoji("1F416", "PIG", "\ud83d\udc16", "pig2", new string[1] { "pig2" }, null, null, "Animals & Nature", 13, 0, SkinVariationType.None, "pig", new string[2] { "pig", "sow" });

	public static readonly Emoji Boar = new Emoji("1F417", "BOAR", "\ud83d\udc17", "boar", new string[1] { "boar" }, null, null, "Animals & Nature", 13, 1, SkinVariationType.None, "boar", new string[2] { "boar", "pig" });

	public static readonly Emoji PigNose = new Emoji("1F43D", "PIG NOSE", "\ud83d\udc3d", "pig_nose", new string[1] { "pig_nose" }, null, null, "Animals & Nature", 13, 39, SkinVariationType.None, "pig nose", new string[3] { "face", "nose", "pig" });

	public static readonly Emoji Ram = new Emoji("1F40F", "RAM", "\ud83d\udc0f", "ram", new string[1] { "ram" }, null, null, "Animals & Nature", 12, 45, SkinVariationType.None, "ram", new string[5] { "Aries", "male", "ram", "sheep", "zodiac" });

	public static readonly Emoji Sheep = new Emoji("1F411", "SHEEP", "\ud83d\udc11", "sheep", new string[1] { "sheep" }, null, null, "Animals & Nature", 12, 47, SkinVariationType.None, "ewe", new string[3] { "ewe", "female", "sheep" });

	public static readonly Emoji Goat = new Emoji("1F410", "GOAT", "\ud83d\udc10", "goat", new string[1] { "goat" }, null, null, "Animals & Nature", 12, 46, SkinVariationType.None, "goat", new string[3] { "Capricorn", "goat", "zodiac" });

	public static readonly Emoji DromedaryCamel = new Emoji("1F42A", "DROMEDARY CAMEL", "\ud83d\udc2a", "dromedary_camel", new string[1] { "dromedary_camel" }, null, null, "Animals & Nature", 13, 20, SkinVariationType.None, "camel", new string[3] { "camel", "dromedary", "hump" });

	public static readonly Emoji Camel = new Emoji("1F42B", "BACTRIAN CAMEL", "\ud83d\udc2b", "camel", new string[1] { "camel" }, null, null, "Animals & Nature", 13, 21, SkinVariationType.None, "two-hump camel", new string[4] { "bactrian", "camel", "hump", "two-hump camel" });

	public static readonly Emoji GiraffeFace = new Emoji("1F992", "GIRAFFE FACE", "\ud83e\udd92", "giraffe_face", new string[1] { "giraffe_face" }, null, null, "Animals & Nature", 42, 42, SkinVariationType.None, "giraffe", new string[2] { "giraffe", "spots" });

	public static readonly Emoji Elephant = new Emoji("1F418", "ELEPHANT", "\ud83d\udc18", "elephant", new string[1] { "elephant" }, null, null, "Animals & Nature", 13, 2, SkinVariationType.None, "elephant", new string[1] { "elephant" });

	public static readonly Emoji Rhinoceros = new Emoji("1F98F", "RHINOCEROS", "\ud83e\udd8f", "rhinoceros", new string[1] { "rhinoceros" }, null, null, "Animals & Nature", 42, 39, SkinVariationType.None, "rhinoceros", new string[1] { "rhinoceros" });

	public static readonly Emoji Mouse = new Emoji("1F42D", "MOUSE FACE", "\ud83d\udc2d", "mouse", new string[1] { "mouse" }, null, null, "Animals & Nature", 13, 23, SkinVariationType.None, "mouse face", new string[2] { "face", "mouse" });

	public static readonly Emoji Mouse2 = new Emoji("1F401", "MOUSE", "\ud83d\udc01", "mouse2", new string[1] { "mouse2" }, null, null, "Animals & Nature", 12, 31, SkinVariationType.None, "mouse", new string[1] { "mouse" });

	public static readonly Emoji Rat = new Emoji("1F400", "RAT", "\ud83d\udc00", "rat", new string[1] { "rat" }, null, null, "Animals & Nature", 12, 30, SkinVariationType.None, "rat", new string[1] { "rat" });

	public static readonly Emoji Hamster = new Emoji("1F439", "HAMSTER FACE", "\ud83d\udc39", "hamster", new string[1] { "hamster" }, null, null, "Animals & Nature", 13, 35, SkinVariationType.None, "hamster face", new string[3] { "face", "hamster", "pet" });

	public static readonly Emoji Rabbit = new Emoji("1F430", "RABBIT FACE", "\ud83d\udc30", "rabbit", new string[1] { "rabbit" }, null, null, "Animals & Nature", 13, 26, SkinVariationType.None, "rabbit face", new string[4] { "bunny", "face", "pet", "rabbit" });

	public static readonly Emoji Rabbit2 = new Emoji("1F407", "RABBIT", "\ud83d\udc07", "rabbit2", new string[1] { "rabbit2" }, null, null, "Animals & Nature", 12, 37, SkinVariationType.None, "rabbit", new string[3] { "bunny", "pet", "rabbit" });

	public static readonly Emoji Chipmunk = new Emoji("1F43F-FE0F", null, "\ud83d\udc3f\ufe0f", "chipmunk", new string[1] { "chipmunk" }, null, null, "Animals & Nature", 13, 41, SkinVariationType.None, null, null);

	public static readonly Emoji Hedgehog = new Emoji("1F994", "HEDGEHOG", "\ud83e\udd94", "hedgehog", new string[1] { "hedgehog" }, null, null, "Animals & Nature", 42, 44, SkinVariationType.None, "hedgehog", new string[2] { "hedgehog", "spiny" });

	public static readonly Emoji Bat = new Emoji("1F987", "BAT", "\ud83e\udd87", "bat", new string[1] { "bat" }, null, null, "Animals & Nature", 42, 31, SkinVariationType.None, "bat", new string[2] { "bat", "vampire" });

	public static readonly Emoji Bear = new Emoji("1F43B", "BEAR FACE", "\ud83d\udc3b", "bear", new string[1] { "bear" }, null, null, "Animals & Nature", 13, 37, SkinVariationType.None, "bear face", new string[2] { "bear", "face" });

	public static readonly Emoji Koala = new Emoji("1F428", "KOALA", "\ud83d\udc28", "koala", new string[1] { "koala" }, null, null, "Animals & Nature", 13, 18, SkinVariationType.None, "koala", new string[2] { "bear", "koala" });

	public static readonly Emoji PandaFace = new Emoji("1F43C", "PANDA FACE", "\ud83d\udc3c", "panda_face", new string[1] { "panda_face" }, null, null, "Animals & Nature", 13, 38, SkinVariationType.None, "panda face", new string[2] { "face", "panda" });

	public static readonly Emoji Feet = new Emoji("1F43E", "PAW PRINTS", "\ud83d\udc3e", "feet", new string[2] { "feet", "paw_prints" }, null, null, "Animals & Nature", 13, 40, SkinVariationType.None, "paw prints", new string[4] { "feet", "paw", "paw prints", "print" });

	public static readonly Emoji Turkey = new Emoji("1F983", "TURKEY", "\ud83e\udd83", "turkey", new string[1] { "turkey" }, null, null, "Animals & Nature", 42, 27, SkinVariationType.None, "turkey", new string[2] { "bird", "turkey" });

	public static readonly Emoji Chicken = new Emoji("1F414", "CHICKEN", "\ud83d\udc14", "chicken", new string[1] { "chicken" }, null, null, "Animals & Nature", 12, 50, SkinVariationType.None, "chicken", new string[2] { "bird", "chicken" });

	public static readonly Emoji Rooster = new Emoji("1F413", "ROOSTER", "\ud83d\udc13", "rooster", new string[1] { "rooster" }, null, null, "Animals & Nature", 12, 49, SkinVariationType.None, "rooster", new string[2] { "bird", "rooster" });

	public static readonly Emoji HatchingChick = new Emoji("1F423", "HATCHING CHICK", "\ud83d\udc23", "hatching_chick", new string[1] { "hatching_chick" }, null, null, "Animals & Nature", 13, 13, SkinVariationType.None, "hatching chick", new string[4] { "baby", "bird", "chick", "hatching" });

	public static readonly Emoji BabyChick = new Emoji("1F424", "BABY CHICK", "\ud83d\udc24", "baby_chick", new string[1] { "baby_chick" }, null, null, "Animals & Nature", 13, 14, SkinVariationType.None, "baby chick", new string[3] { "baby", "bird", "chick" });

	public static readonly Emoji HatchedChick = new Emoji("1F425", "FRONT-FACING BABY CHICK", "\ud83d\udc25", "hatched_chick", new string[1] { "hatched_chick" }, null, null, "Animals & Nature", 13, 15, SkinVariationType.None, "front-facing baby chick", new string[4] { "baby", "bird", "chick", "front-facing baby chick" });

	public static readonly Emoji Bird = new Emoji("1F426", "BIRD", "\ud83d\udc26", "bird", new string[1] { "bird" }, null, null, "Animals & Nature", 13, 16, SkinVariationType.None, "bird", new string[1] { "bird" });

	public static readonly Emoji Penguin = new Emoji("1F427", "PENGUIN", "\ud83d\udc27", "penguin", new string[1] { "penguin" }, null, null, "Animals & Nature", 13, 17, SkinVariationType.None, "penguin", new string[2] { "bird", "penguin" });

	public static readonly Emoji DoveOfPeace = new Emoji("1F54A-FE0F", null, "\ud83d\udd4a\ufe0f", "dove_of_peace", new string[1] { "dove_of_peace" }, null, null, "Animals & Nature", 28, 13, SkinVariationType.None, null, null);

	public static readonly Emoji Eagle = new Emoji("1F985", "EAGLE", "\ud83e\udd85", "eagle", new string[1] { "eagle" }, null, null, "Animals & Nature", 42, 29, SkinVariationType.None, "eagle", new string[2] { "bird", "eagle" });

	public static readonly Emoji Duck = new Emoji("1F986", "DUCK", "\ud83e\udd86", "duck", new string[1] { "duck" }, null, null, "Animals & Nature", 42, 30, SkinVariationType.None, "duck", new string[2] { "bird", "duck" });

	public static readonly Emoji Owl = new Emoji("1F989", "OWL", "\ud83e\udd89", "owl", new string[1] { "owl" }, null, null, "Animals & Nature", 42, 33, SkinVariationType.None, "owl", new string[3] { "bird", "owl", "wise" });

	public static readonly Emoji Frog = new Emoji("1F438", "FROG FACE", "\ud83d\udc38", "frog", new string[1] { "frog" }, null, null, "Animals & Nature", 13, 34, SkinVariationType.None, "frog face", new string[2] { "face", "frog" });

	public static readonly Emoji Crocodile = new Emoji("1F40A", "CROCODILE", "\ud83d\udc0a", "crocodile", new string[1] { "crocodile" }, null, null, "Animals & Nature", 12, 40, SkinVariationType.None, "crocodile", new string[1] { "crocodile" });

	public static readonly Emoji Turtle = new Emoji("1F422", "TURTLE", "\ud83d\udc22", "turtle", new string[1] { "turtle" }, null, null, "Animals & Nature", 13, 12, SkinVariationType.None, "turtle", new string[3] { "terrapin", "tortoise", "turtle" });

	public static readonly Emoji Lizard = new Emoji("1F98E", "LIZARD", "\ud83e\udd8e", "lizard", new string[1] { "lizard" }, null, null, "Animals & Nature", 42, 38, SkinVariationType.None, "lizard", new string[2] { "lizard", "reptile" });

	public static readonly Emoji Snake = new Emoji("1F40D", "SNAKE", "\ud83d\udc0d", "snake", new string[1] { "snake" }, null, null, "Animals & Nature", 12, 43, SkinVariationType.None, "snake", new string[5] { "bearer", "Ophiuchus", "serpent", "snake", "zodiac" });

	public static readonly Emoji DragonFace = new Emoji("1F432", "DRAGON FACE", "\ud83d\udc32", "dragon_face", new string[1] { "dragon_face" }, null, null, "Animals & Nature", 13, 28, SkinVariationType.None, "dragon face", new string[3] { "dragon", "face", "fairy tale" });

	public static readonly Emoji Dragon = new Emoji("1F409", "DRAGON", "\ud83d\udc09", "dragon", new string[1] { "dragon" }, null, null, "Animals & Nature", 12, 39, SkinVariationType.None, "dragon", new string[2] { "dragon", "fairy tale" });

	public static readonly Emoji Sauropod = new Emoji("1F995", "SAUROPOD", "\ud83e\udd95", "sauropod", new string[1] { "sauropod" }, null, null, "Animals & Nature", 42, 45, SkinVariationType.None, "sauropod", new string[4] { "brachiosaurus", "brontosaurus", "diplodocus", "sauropod" });

	public static readonly Emoji TRex = new Emoji("1F996", "T-REX", "\ud83e\udd96", "t-rex", new string[1] { "t-rex" }, null, null, "Animals & Nature", 42, 46, SkinVariationType.None, "T-Rex", new string[2] { "T-Rex", "Tyrannosaurus Rex" });

	public static readonly Emoji Whale = new Emoji("1F433", "SPOUTING WHALE", "\ud83d\udc33", "whale", new string[1] { "whale" }, null, null, "Animals & Nature", 13, 29, SkinVariationType.None, "spouting whale", new string[3] { "face", "spouting", "whale" });

	public static readonly Emoji Whale2 = new Emoji("1F40B", "WHALE", "\ud83d\udc0b", "whale2", new string[1] { "whale2" }, null, null, "Animals & Nature", 12, 41, SkinVariationType.None, "whale", new string[1] { "whale" });

	public static readonly Emoji Dolphin = new Emoji("1F42C", "DOLPHIN", "\ud83d\udc2c", "dolphin", new string[2] { "dolphin", "flipper" }, null, null, "Animals & Nature", 13, 22, SkinVariationType.None, "dolphin", new string[2] { "dolphin", "flipper" });

	public static readonly Emoji Fish = new Emoji("1F41F", "FISH", "\ud83d\udc1f", "fish", new string[1] { "fish" }, null, null, "Animals & Nature", 13, 9, SkinVariationType.None, "fish", new string[3] { "fish", "Pisces", "zodiac" });

	public static readonly Emoji TropicalFish = new Emoji("1F420", "TROPICAL FISH", "\ud83d\udc20", "tropical_fish", new string[1] { "tropical_fish" }, null, null, "Animals & Nature", 13, 10, SkinVariationType.None, "tropical fish", new string[2] { "fish", "tropical" });

	public static readonly Emoji Blowfish = new Emoji("1F421", "BLOWFISH", "\ud83d\udc21", "blowfish", new string[1] { "blowfish" }, null, null, "Animals & Nature", 13, 11, SkinVariationType.None, "blowfish", new string[2] { "blowfish", "fish" });

	public static readonly Emoji Shark = new Emoji("1F988", "SHARK", "\ud83e\udd88", "shark", new string[1] { "shark" }, null, null, "Animals & Nature", 42, 32, SkinVariationType.None, "shark", new string[2] { "fish", "shark" });

	public static readonly Emoji Octopus = new Emoji("1F419", "OCTOPUS", "\ud83d\udc19", "octopus", new string[1] { "octopus" }, null, null, "Animals & Nature", 13, 3, SkinVariationType.None, "octopus", new string[1] { "octopus" });

	public static readonly Emoji Shell = new Emoji("1F41A", "SPIRAL SHELL", "\ud83d\udc1a", "shell", new string[1] { "shell" }, null, null, "Animals & Nature", 13, 4, SkinVariationType.None, "spiral shell", new string[2] { "shell", "spiral" });

	public static readonly Emoji Crab = new Emoji("1F980", "CRAB", "\ud83e\udd80", "crab", new string[1] { "crab" }, null, null, "Animals & Nature", 42, 24, SkinVariationType.None, "crab", new string[3] { "Cancer", "crab", "zodiac" });

	public static readonly Emoji Shrimp = new Emoji("1F990", "SHRIMP", "\ud83e\udd90", "shrimp", new string[1] { "shrimp" }, null, null, "Animals & Nature", 42, 40, SkinVariationType.None, "shrimp", new string[4] { "food", "shellfish", "shrimp", "small" });

	public static readonly Emoji Squid = new Emoji("1F991", "SQUID", "\ud83e\udd91", "squid", new string[1] { "squid" }, null, null, "Animals & Nature", 42, 41, SkinVariationType.None, "squid", new string[3] { "food", "molusc", "squid" });

	public static readonly Emoji Snail = new Emoji("1F40C", "SNAIL", "\ud83d\udc0c", "snail", new string[1] { "snail" }, null, null, "Animals & Nature", 12, 42, SkinVariationType.None, "snail", new string[1] { "snail" });

	public static readonly Emoji Butterfly = new Emoji("1F98B", "BUTTERFLY", "\ud83e\udd8b", "butterfly", new string[1] { "butterfly" }, null, null, "Animals & Nature", 42, 35, SkinVariationType.None, "butterfly", new string[3] { "butterfly", "insect", "pretty" });

	public static readonly Emoji Bug = new Emoji("1F41B", "BUG", "\ud83d\udc1b", "bug", new string[1] { "bug" }, null, null, "Animals & Nature", 13, 5, SkinVariationType.None, "bug", new string[2] { "bug", "insect" });

	public static readonly Emoji Ant = new Emoji("1F41C", "ANT", "\ud83d\udc1c", "ant", new string[1] { "ant" }, null, null, "Animals & Nature", 13, 6, SkinVariationType.None, "ant", new string[2] { "ant", "insect" });

	public static readonly Emoji Bee = new Emoji("1F41D", "HONEYBEE", "\ud83d\udc1d", "bee", new string[2] { "bee", "honeybee" }, null, null, "Animals & Nature", 13, 7, SkinVariationType.None, "honeybee", new string[3] { "bee", "honeybee", "insect" });

	public static readonly Emoji Beetle = new Emoji("1F41E", "LADY BEETLE", "\ud83d\udc1e", "beetle", new string[1] { "beetle" }, null, null, "Animals & Nature", 13, 8, SkinVariationType.None, "lady beetle", new string[5] { "beetle", "insect", "lady beetle", "ladybird", "ladybug" });

	public static readonly Emoji Cricket = new Emoji("1F997", "CRICKET", "\ud83e\udd97", "cricket", new string[1] { "cricket" }, null, null, "Animals & Nature", 42, 47, SkinVariationType.None, "cricket", new string[2] { "cricket", "grasshopper" });

	public static readonly Emoji Spider = new Emoji("1F577-FE0F", null, "\ud83d\udd77\ufe0f", "spider", new string[1] { "spider" }, null, null, "Animals & Nature", 29, 18, SkinVariationType.None, null, null);

	public static readonly Emoji SpiderWeb = new Emoji("1F578-FE0F", null, "\ud83d\udd78\ufe0f", "spider_web", new string[1] { "spider_web" }, null, null, "Animals & Nature", 29, 19, SkinVariationType.None, null, null);

	public static readonly Emoji Scorpion = new Emoji("1F982", "SCORPION", "\ud83e\udd82", "scorpion", new string[1] { "scorpion" }, null, null, "Animals & Nature", 42, 26, SkinVariationType.None, "scorpion", new string[4] { "scorpio", "Scorpio", "scorpion", "zodiac" });

	public static readonly Emoji Bouquet = new Emoji("1F490", "BOUQUET", "\ud83d\udc90", "bouquet", new string[1] { "bouquet" }, null, null, "Animals & Nature", 24, 42, SkinVariationType.None, "bouquet", new string[2] { "bouquet", "flower" });

	public static readonly Emoji CherryBlossom = new Emoji("1F338", "CHERRY BLOSSOM", "\ud83c\udf38", "cherry_blossom", new string[1] { "cherry_blossom" }, null, null, "Animals & Nature", 6, 46, SkinVariationType.None, "cherry blossom", new string[3] { "blossom", "cherry", "flower" });

	public static readonly Emoji WhiteFlower = new Emoji("1F4AE", "WHITE FLOWER", "\ud83d\udcae", "white_flower", new string[1] { "white_flower" }, null, null, "Animals & Nature", 25, 25, SkinVariationType.None, "white flower", new string[2] { "flower", "white flower" });

	public static readonly Emoji Rosette = new Emoji("1F3F5-FE0F", null, "\ud83c\udff5\ufe0f", "rosette", new string[1] { "rosette" }, null, null, "Animals & Nature", 12, 20, SkinVariationType.None, null, null);

	public static readonly Emoji Rose = new Emoji("1F339", "ROSE", "\ud83c\udf39", "rose", new string[1] { "rose" }, null, null, "Animals & Nature", 6, 47, SkinVariationType.None, "rose", new string[2] { "flower", "rose" });

	public static readonly Emoji WiltedFlower = new Emoji("1F940", "WILTED FLOWER", "\ud83e\udd40", "wilted_flower", new string[1] { "wilted_flower" }, null, null, "Animals & Nature", 41, 36, SkinVariationType.None, "wilted flower", new string[2] { "flower", "wilted" });

	public static readonly Emoji Hibiscus = new Emoji("1F33A", "HIBISCUS", "\ud83c\udf3a", "hibiscus", new string[1] { "hibiscus" }, null, null, "Animals & Nature", 6, 48, SkinVariationType.None, "hibiscus", new string[2] { "flower", "hibiscus" });

	public static readonly Emoji Sunflower = new Emoji("1F33B", "SUNFLOWER", "\ud83c\udf3b", "sunflower", new string[1] { "sunflower" }, null, null, "Animals & Nature", 6, 49, SkinVariationType.None, "sunflower", new string[3] { "flower", "sun", "sunflower" });

	public static readonly Emoji Blossom = new Emoji("1F33C", "BLOSSOM", "\ud83c\udf3c", "blossom", new string[1] { "blossom" }, null, null, "Animals & Nature", 6, 50, SkinVariationType.None, "blossom", new string[2] { "blossom", "flower" });

	public static readonly Emoji Tulip = new Emoji("1F337", "TULIP", "\ud83c\udf37", "tulip", new string[1] { "tulip" }, null, null, "Animals & Nature", 6, 45, SkinVariationType.None, "tulip", new string[2] { "flower", "tulip" });

	public static readonly Emoji Seedling = new Emoji("1F331", "SEEDLING", "\ud83c\udf31", "seedling", new string[1] { "seedling" }, null, null, "Animals & Nature", 6, 39, SkinVariationType.None, "seedling", new string[2] { "seedling", "young" });

	public static readonly Emoji EvergreenTree = new Emoji("1F332", "EVERGREEN TREE", "\ud83c\udf32", "evergreen_tree", new string[1] { "evergreen_tree" }, null, null, "Animals & Nature", 6, 40, SkinVariationType.None, "evergreen tree", new string[2] { "evergreen tree", "tree" });

	public static readonly Emoji DeciduousTree = new Emoji("1F333", "DECIDUOUS TREE", "\ud83c\udf33", "deciduous_tree", new string[1] { "deciduous_tree" }, null, null, "Animals & Nature", 6, 41, SkinVariationType.None, "deciduous tree", new string[3] { "deciduous", "shedding", "tree" });

	public static readonly Emoji PalmTree = new Emoji("1F334", "PALM TREE", "\ud83c\udf34", "palm_tree", new string[1] { "palm_tree" }, null, null, "Animals & Nature", 6, 42, SkinVariationType.None, "palm tree", new string[2] { "palm", "tree" });

	public static readonly Emoji Cactus = new Emoji("1F335", "CACTUS", "\ud83c\udf35", "cactus", new string[1] { "cactus" }, null, null, "Animals & Nature", 6, 43, SkinVariationType.None, "cactus", new string[2] { "cactus", "plant" });

	public static readonly Emoji EarOfRice = new Emoji("1F33E", "EAR OF RICE", "\ud83c\udf3e", "ear_of_rice", new string[1] { "ear_of_rice" }, null, null, "Animals & Nature", 7, 0, SkinVariationType.None, "sheaf of rice", new string[4] { "ear", "grain", "rice", "sheaf of rice" });

	public static readonly Emoji Herb = new Emoji("1F33F", "HERB", "\ud83c\udf3f", "herb", new string[1] { "herb" }, null, null, "Animals & Nature", 7, 1, SkinVariationType.None, "herb", new string[2] { "herb", "leaf" });

	public static readonly Emoji Shamrock = new Emoji("2618-FE0F", null, "☘\ufe0f", "shamrock", new string[1] { "shamrock" }, null, null, "Animals & Nature", 47, 25, SkinVariationType.None, null, null);

	public static readonly Emoji FourLeafClover = new Emoji("1F340", "FOUR LEAF CLOVER", "\ud83c\udf40", "four_leaf_clover", new string[1] { "four_leaf_clover" }, null, null, "Animals & Nature", 7, 2, SkinVariationType.None, "four leaf clover", new string[5] { "4", "clover", "four", "four-leaf clover", "leaf" });

	public static readonly Emoji MapleLeaf = new Emoji("1F341", "MAPLE LEAF", "\ud83c\udf41", "maple_leaf", new string[1] { "maple_leaf" }, null, null, "Animals & Nature", 7, 3, SkinVariationType.None, "maple leaf", new string[3] { "falling", "leaf", "maple" });

	public static readonly Emoji FallenLeaf = new Emoji("1F342", "FALLEN LEAF", "\ud83c\udf42", "fallen_leaf", new string[1] { "fallen_leaf" }, null, null, "Animals & Nature", 7, 4, SkinVariationType.None, "fallen leaf", new string[3] { "fallen leaf", "falling", "leaf" });

	public static readonly Emoji Leaves = new Emoji("1F343", "LEAF FLUTTERING IN WIND", "\ud83c\udf43", "leaves", new string[1] { "leaves" }, null, null, "Animals & Nature", 7, 5, SkinVariationType.None, "leaf fluttering in wind", new string[5] { "blow", "flutter", "leaf", "leaf fluttering in wind", "wind" });

	public static readonly Emoji Grapes = new Emoji("1F347", "GRAPES", "\ud83c\udf47", "grapes", new string[1] { "grapes" }, null, null, "Food & Drink", 7, 9, SkinVariationType.None, "grapes", new string[3] { "fruit", "grape", "grapes" });

	public static readonly Emoji Melon = new Emoji("1F348", "MELON", "\ud83c\udf48", "melon", new string[1] { "melon" }, null, null, "Food & Drink", 7, 10, SkinVariationType.None, "melon", new string[2] { "fruit", "melon" });

	public static readonly Emoji Watermelon = new Emoji("1F349", "WATERMELON", "\ud83c\udf49", "watermelon", new string[1] { "watermelon" }, null, null, "Food & Drink", 7, 11, SkinVariationType.None, "watermelon", new string[2] { "fruit", "watermelon" });

	public static readonly Emoji Tangerine = new Emoji("1F34A", "TANGERINE", "\ud83c\udf4a", "tangerine", new string[1] { "tangerine" }, null, null, "Food & Drink", 7, 12, SkinVariationType.None, "tangerine", new string[3] { "fruit", "orange", "tangerine" });

	public static readonly Emoji Lemon = new Emoji("1F34B", "LEMON", "\ud83c\udf4b", "lemon", new string[1] { "lemon" }, null, null, "Food & Drink", 7, 13, SkinVariationType.None, "lemon", new string[3] { "citrus", "fruit", "lemon" });

	public static readonly Emoji Banana = new Emoji("1F34C", "BANANA", "\ud83c\udf4c", "banana", new string[1] { "banana" }, null, null, "Food & Drink", 7, 14, SkinVariationType.None, "banana", new string[2] { "banana", "fruit" });

	public static readonly Emoji Pineapple = new Emoji("1F34D", "PINEAPPLE", "\ud83c\udf4d", "pineapple", new string[1] { "pineapple" }, null, null, "Food & Drink", 7, 15, SkinVariationType.None, "pineapple", new string[2] { "fruit", "pineapple" });

	public static readonly Emoji Apple = new Emoji("1F34E", "RED APPLE", "\ud83c\udf4e", "apple", new string[1] { "apple" }, null, null, "Food & Drink", 7, 16, SkinVariationType.None, "red apple", new string[3] { "apple", "fruit", "red" });

	public static readonly Emoji GreenApple = new Emoji("1F34F", "GREEN APPLE", "\ud83c\udf4f", "green_apple", new string[1] { "green_apple" }, null, null, "Food & Drink", 7, 17, SkinVariationType.None, "green apple", new string[3] { "apple", "fruit", "green" });

	public static readonly Emoji Pear = new Emoji("1F350", "PEAR", "\ud83c\udf50", "pear", new string[1] { "pear" }, null, null, "Food & Drink", 7, 18, SkinVariationType.None, "pear", new string[2] { "fruit", "pear" });

	public static readonly Emoji Peach = new Emoji("1F351", "PEACH", "\ud83c\udf51", "peach", new string[1] { "peach" }, null, null, "Food & Drink", 7, 19, SkinVariationType.None, "peach", new string[2] { "fruit", "peach" });

	public static readonly Emoji Cherries = new Emoji("1F352", "CHERRIES", "\ud83c\udf52", "cherries", new string[1] { "cherries" }, null, null, "Food & Drink", 7, 20, SkinVariationType.None, "cherries", new string[5] { "berries", "cherries", "cherry", "fruit", "red" });

	public static readonly Emoji Strawberry = new Emoji("1F353", "STRAWBERRY", "\ud83c\udf53", "strawberry", new string[1] { "strawberry" }, null, null, "Food & Drink", 7, 21, SkinVariationType.None, "strawberry", new string[3] { "berry", "fruit", "strawberry" });

	public static readonly Emoji Kiwifruit = new Emoji("1F95D", "KIWIFRUIT", "\ud83e\udd5d", "kiwifruit", new string[1] { "kiwifruit" }, null, null, "Food & Drink", 42, 9, SkinVariationType.None, "kiwi fruit", new string[3] { "food", "fruit", "kiwi" });

	public static readonly Emoji Tomato = new Emoji("1F345", "TOMATO", "\ud83c\udf45", "tomato", new string[1] { "tomato" }, null, null, "Food & Drink", 7, 7, SkinVariationType.None, "tomato", new string[3] { "fruit", "tomato", "vegetable" });

	public static readonly Emoji Coconut = new Emoji("1F965", "COCONUT", "\ud83e\udd65", "coconut", new string[1] { "coconut" }, null, null, "Food & Drink", 42, 17, SkinVariationType.None, "coconut", new string[3] { "coconut", "palm", "piña colada" });

	public static readonly Emoji Avocado = new Emoji("1F951", "AVOCADO", "\ud83e\udd51", "avocado", new string[1] { "avocado" }, null, null, "Food & Drink", 41, 49, SkinVariationType.None, "avocado", new string[3] { "avocado", "food", "fruit" });

	public static readonly Emoji Eggplant = new Emoji("1F346", "AUBERGINE", "\ud83c\udf46", "eggplant", new string[1] { "eggplant" }, null, null, "Food & Drink", 7, 8, SkinVariationType.None, "eggplant", new string[3] { "aubergine", "eggplant", "vegetable" });

	public static readonly Emoji Potato = new Emoji("1F954", "POTATO", "\ud83e\udd54", "potato", new string[1] { "potato" }, null, null, "Food & Drink", 42, 0, SkinVariationType.None, "potato", new string[3] { "food", "potato", "vegetable" });

	public static readonly Emoji Carrot = new Emoji("1F955", "CARROT", "\ud83e\udd55", "carrot", new string[1] { "carrot" }, null, null, "Food & Drink", 42, 1, SkinVariationType.None, "carrot", new string[3] { "carrot", "food", "vegetable" });

	public static readonly Emoji Corn = new Emoji("1F33D", "EAR OF MAIZE", "\ud83c\udf3d", "corn", new string[1] { "corn" }, null, null, "Food & Drink", 6, 51, SkinVariationType.None, "ear of corn", new string[5] { "corn", "ear", "ear of corn", "maize", "maze" });

	public static readonly Emoji HotPepper = new Emoji("1F336-FE0F", null, "\ud83c\udf36\ufe0f", "hot_pepper", new string[1] { "hot_pepper" }, null, null, "Food & Drink", 6, 44, SkinVariationType.None, null, null);

	public static readonly Emoji Cucumber = new Emoji("1F952", "CUCUMBER", "\ud83e\udd52", "cucumber", new string[1] { "cucumber" }, null, null, "Food & Drink", 41, 50, SkinVariationType.None, "cucumber", new string[4] { "cucumber", "food", "pickle", "vegetable" });

	public static readonly Emoji Broccoli = new Emoji("1F966", "BROCCOLI", "\ud83e\udd66", "broccoli", new string[1] { "broccoli" }, null, null, "Food & Drink", 42, 18, SkinVariationType.None, "broccoli", new string[2] { "broccoli", "wild cabbage" });

	public static readonly Emoji Mushroom = new Emoji("1F344", "MUSHROOM", "\ud83c\udf44", "mushroom", new string[1] { "mushroom" }, null, null, "Food & Drink", 7, 6, SkinVariationType.None, "mushroom", new string[2] { "mushroom", "toadstool" });

	public static readonly Emoji Peanuts = new Emoji("1F95C", "PEANUTS", "\ud83e\udd5c", "peanuts", new string[1] { "peanuts" }, null, null, "Food & Drink", 42, 8, SkinVariationType.None, "peanuts", new string[5] { "food", "nut", "peanut", "peanuts", "vegetable" });

	public static readonly Emoji Chestnut = new Emoji("1F330", "CHESTNUT", "\ud83c\udf30", "chestnut", new string[1] { "chestnut" }, null, null, "Food & Drink", 6, 38, SkinVariationType.None, "chestnut", new string[2] { "chestnut", "plant" });

	public static readonly Emoji Bread = new Emoji("1F35E", "BREAD", "\ud83c\udf5e", "bread", new string[1] { "bread" }, null, null, "Food & Drink", 7, 32, SkinVariationType.None, "bread", new string[2] { "bread", "loaf" });

	public static readonly Emoji Croissant = new Emoji("1F950", "CROISSANT", "\ud83e\udd50", "croissant", new string[1] { "croissant" }, null, null, "Food & Drink", 41, 48, SkinVariationType.None, "croissant", new string[5] { "bread", "crescent roll", "croissant", "food", "french" });

	public static readonly Emoji BaguetteBread = new Emoji("1F956", "BAGUETTE BREAD", "\ud83e\udd56", "baguette_bread", new string[1] { "baguette_bread" }, null, null, "Food & Drink", 42, 2, SkinVariationType.None, "baguette bread", new string[4] { "baguette", "bread", "food", "french" });

	public static readonly Emoji Pretzel = new Emoji("1F968", "PRETZEL", "\ud83e\udd68", "pretzel", new string[1] { "pretzel" }, null, null, "Food & Drink", 42, 20, SkinVariationType.None, "pretzel", new string[2] { "pretzel", "twisted" });

	public static readonly Emoji Pancakes = new Emoji("1F95E", "PANCAKES", "\ud83e\udd5e", "pancakes", new string[1] { "pancakes" }, null, null, "Food & Drink", 42, 10, SkinVariationType.None, "pancakes", new string[5] { "crêpe", "food", "hotcake", "pancake", "pancakes" });

	public static readonly Emoji CheeseWedge = new Emoji("1F9C0", "CHEESE WEDGE", "\ud83e\uddc0", "cheese_wedge", new string[1] { "cheese_wedge" }, null, null, "Food & Drink", 42, 48, SkinVariationType.None, "cheese wedge", new string[2] { "cheese", "cheese wedge" });

	public static readonly Emoji MeatOnBone = new Emoji("1F356", "MEAT ON BONE", "\ud83c\udf56", "meat_on_bone", new string[1] { "meat_on_bone" }, null, null, "Food & Drink", 7, 24, SkinVariationType.None, "meat on bone", new string[3] { "bone", "meat", "meat on bone" });

	public static readonly Emoji PoultryLeg = new Emoji("1F357", "POULTRY LEG", "\ud83c\udf57", "poultry_leg", new string[1] { "poultry_leg" }, null, null, "Food & Drink", 7, 25, SkinVariationType.None, "poultry leg", new string[5] { "bone", "chicken", "drumstick", "leg", "poultry" });

	public static readonly Emoji CutOfMeat = new Emoji("1F969", "CUT OF MEAT", "\ud83e\udd69", "cut_of_meat", new string[1] { "cut_of_meat" }, null, null, "Food & Drink", 42, 21, SkinVariationType.None, "cut of meat", new string[5] { "chop", "cut of meat", "lambchop", "porkchop", "steak" });

	public static readonly Emoji Bacon = new Emoji("1F953", "BACON", "\ud83e\udd53", "bacon", new string[1] { "bacon" }, null, null, "Food & Drink", 41, 51, SkinVariationType.None, "bacon", new string[3] { "bacon", "food", "meat" });

	public static readonly Emoji Hamburger = new Emoji("1F354", "HAMBURGER", "\ud83c\udf54", "hamburger", new string[1] { "hamburger" }, null, null, "Food & Drink", 7, 22, SkinVariationType.None, "hamburger", new string[2] { "burger", "hamburger" });

	public static readonly Emoji Fries = new Emoji("1F35F", "FRENCH FRIES", "\ud83c\udf5f", "fries", new string[1] { "fries" }, null, null, "Food & Drink", 7, 33, SkinVariationType.None, "french fries", new string[2] { "french", "fries" });

	public static readonly Emoji Pizza = new Emoji("1F355", "SLICE OF PIZZA", "\ud83c\udf55", "pizza", new string[1] { "pizza" }, null, null, "Food & Drink", 7, 23, SkinVariationType.None, "pizza", new string[3] { "cheese", "pizza", "slice" });

	public static readonly Emoji Hotdog = new Emoji("1F32D", "HOT DOG", "\ud83c\udf2d", "hotdog", new string[1] { "hotdog" }, null, null, "Food & Drink", 6, 35, SkinVariationType.None, "hot dog", new string[4] { "frankfurter", "hot dog", "hotdog", "sausage" });

	public static readonly Emoji Sandwich = new Emoji("1F96A", "SANDWICH", "\ud83e\udd6a", "sandwich", new string[1] { "sandwich" }, null, null, "Food & Drink", 42, 22, SkinVariationType.None, "sandwich", new string[2] { "bread", "sandwich" });

	public static readonly Emoji Taco = new Emoji("1F32E", "TACO", "\ud83c\udf2e", "taco", new string[1] { "taco" }, null, null, "Food & Drink", 6, 36, SkinVariationType.None, "taco", new string[2] { "mexican", "taco" });

	public static readonly Emoji Burrito = new Emoji("1F32F", "BURRITO", "\ud83c\udf2f", "burrito", new string[1] { "burrito" }, null, null, "Food & Drink", 6, 37, SkinVariationType.None, "burrito", new string[3] { "burrito", "mexican", "wrap" });

	public static readonly Emoji StuffedFlatbread = new Emoji("1F959", "STUFFED FLATBREAD", "\ud83e\udd59", "stuffed_flatbread", new string[1] { "stuffed_flatbread" }, null, null, "Food & Drink", 42, 5, SkinVariationType.None, "stuffed flatbread", new string[6] { "falafel", "flatbread", "food", "gyro", "kebab", "stuffed" });

	public static readonly Emoji Egg = new Emoji("1F95A", "EGG", "\ud83e\udd5a", "egg", new string[1] { "egg" }, null, null, "Food & Drink", 42, 6, SkinVariationType.None, "egg", new string[2] { "egg", "food" });

	public static readonly Emoji FriedEgg = new Emoji("1F373", "COOKING", "\ud83c\udf73", "fried_egg", new string[2] { "fried_egg", "cooking" }, null, null, "Food & Drink", 8, 1, SkinVariationType.None, "cooking", new string[4] { "cooking", "egg", "frying", "pan" });

	public static readonly Emoji ShallowPanOfFood = new Emoji("1F958", "SHALLOW PAN OF FOOD", "\ud83e\udd58", "shallow_pan_of_food", new string[1] { "shallow_pan_of_food" }, null, null, "Food & Drink", 42, 4, SkinVariationType.None, "shallow pan of food", new string[6] { "casserole", "food", "paella", "pan", "shallow", "shallow pan of food" });

	public static readonly Emoji Stew = new Emoji("1F372", "POT OF FOOD", "\ud83c\udf72", "stew", new string[1] { "stew" }, null, null, "Food & Drink", 8, 0, SkinVariationType.None, "pot of food", new string[3] { "pot", "pot of food", "stew" });

	public static readonly Emoji BowlWithSpoon = new Emoji("1F963", "BOWL WITH SPOON", "\ud83e\udd63", "bowl_with_spoon", new string[1] { "bowl_with_spoon" }, null, null, "Food & Drink", 42, 15, SkinVariationType.None, "bowl with spoon", new string[4] { "bowl with spoon", "breakfast", "cereal", "congee" });

	public static readonly Emoji GreenSalad = new Emoji("1F957", "GREEN SALAD", "\ud83e\udd57", "green_salad", new string[1] { "green_salad" }, null, null, "Food & Drink", 42, 3, SkinVariationType.None, "green salad", new string[3] { "food", "green", "salad" });

	public static readonly Emoji Popcorn = new Emoji("1F37F", "POPCORN", "\ud83c\udf7f", "popcorn", new string[1] { "popcorn" }, null, null, "Food & Drink", 8, 13, SkinVariationType.None, "popcorn", new string[1] { "popcorn" });

	public static readonly Emoji CannedFood = new Emoji("1F96B", "CANNED FOOD", "\ud83e\udd6b", "canned_food", new string[1] { "canned_food" }, null, null, "Food & Drink", 42, 23, SkinVariationType.None, "canned food", new string[2] { "can", "canned food" });

	public static readonly Emoji Bento = new Emoji("1F371", "BENTO BOX", "\ud83c\udf71", "bento", new string[1] { "bento" }, null, null, "Food & Drink", 7, 51, SkinVariationType.None, "bento box", new string[2] { "bento", "box" });

	public static readonly Emoji RiceCracker = new Emoji("1F358", "RICE CRACKER", "\ud83c\udf58", "rice_cracker", new string[1] { "rice_cracker" }, null, null, "Food & Drink", 7, 26, SkinVariationType.None, "rice cracker", new string[2] { "cracker", "rice" });

	public static readonly Emoji RiceBall = new Emoji("1F359", "RICE BALL", "\ud83c\udf59", "rice_ball", new string[1] { "rice_ball" }, null, null, "Food & Drink", 7, 27, SkinVariationType.None, "rice ball", new string[3] { "ball", "Japanese", "rice" });

	public static readonly Emoji Rice = new Emoji("1F35A", "COOKED RICE", "\ud83c\udf5a", "rice", new string[1] { "rice" }, null, null, "Food & Drink", 7, 28, SkinVariationType.None, "cooked rice", new string[2] { "cooked", "rice" });

	public static readonly Emoji Curry = new Emoji("1F35B", "CURRY AND RICE", "\ud83c\udf5b", "curry", new string[1] { "curry" }, null, null, "Food & Drink", 7, 29, SkinVariationType.None, "curry rice", new string[2] { "curry", "rice" });

	public static readonly Emoji Ramen = new Emoji("1F35C", "STEAMING BOWL", "\ud83c\udf5c", "ramen", new string[1] { "ramen" }, null, null, "Food & Drink", 7, 30, SkinVariationType.None, "steaming bowl", new string[4] { "bowl", "noodle", "ramen", "steaming" });

	public static readonly Emoji Spaghetti = new Emoji("1F35D", "SPAGHETTI", "\ud83c\udf5d", "spaghetti", new string[1] { "spaghetti" }, null, null, "Food & Drink", 7, 31, SkinVariationType.None, "spaghetti", new string[2] { "pasta", "spaghetti" });

	public static readonly Emoji SweetPotato = new Emoji("1F360", "ROASTED SWEET POTATO", "\ud83c\udf60", "sweet_potato", new string[1] { "sweet_potato" }, null, null, "Food & Drink", 7, 34, SkinVariationType.None, "roasted sweet potato", new string[3] { "potato", "roasted", "sweet" });

	public static readonly Emoji Oden = new Emoji("1F362", "ODEN", "\ud83c\udf62", "oden", new string[1] { "oden" }, null, null, "Food & Drink", 7, 36, SkinVariationType.None, "oden", new string[5] { "kebab", "oden", "seafood", "skewer", "stick" });

	public static readonly Emoji Sushi = new Emoji("1F363", "SUSHI", "\ud83c\udf63", "sushi", new string[1] { "sushi" }, null, null, "Food & Drink", 7, 37, SkinVariationType.None, "sushi", new string[1] { "sushi" });

	public static readonly Emoji FriedShrimp = new Emoji("1F364", "FRIED SHRIMP", "\ud83c\udf64", "fried_shrimp", new string[1] { "fried_shrimp" }, null, null, "Food & Drink", 7, 38, SkinVariationType.None, "fried shrimp", new string[4] { "fried", "prawn", "shrimp", "tempura" });

	public static readonly Emoji FishCake = new Emoji("1F365", "FISH CAKE WITH SWIRL DESIGN", "\ud83c\udf65", "fish_cake", new string[1] { "fish_cake" }, null, null, "Food & Drink", 7, 39, SkinVariationType.None, "fish cake with swirl", new string[5] { "cake", "fish", "fish cake with swirl", "pastry", "swirl" });

	public static readonly Emoji Dango = new Emoji("1F361", "DANGO", "\ud83c\udf61", "dango", new string[1] { "dango" }, null, null, "Food & Drink", 7, 35, SkinVariationType.None, "dango", new string[6] { "dango", "dessert", "Japanese", "skewer", "stick", "sweet" });

	public static readonly Emoji Dumpling = new Emoji("1F95F", "DUMPLING", "\ud83e\udd5f", "dumpling", new string[1] { "dumpling" }, null, null, "Food & Drink", 42, 11, SkinVariationType.None, "dumpling", new string[6] { "dumpling", "empanada", "gyōza", "jiaozi", "pierogi", "potsticker" });

	public static readonly Emoji FortuneCookie = new Emoji("1F960", "FORTUNE COOKIE", "\ud83e\udd60", "fortune_cookie", new string[1] { "fortune_cookie" }, null, null, "Food & Drink", 42, 12, SkinVariationType.None, "fortune cookie", new string[2] { "fortune cookie", "prophecy" });

	public static readonly Emoji TakeoutBox = new Emoji("1F961", "TAKEOUT BOX", "\ud83e\udd61", "takeout_box", new string[1] { "takeout_box" }, null, null, "Food & Drink", 42, 13, SkinVariationType.None, "takeout box", new string[2] { "oyster pail", "takeout box" });

	public static readonly Emoji Icecream = new Emoji("1F366", "SOFT ICE CREAM", "\ud83c\udf66", "icecream", new string[1] { "icecream" }, null, null, "Food & Drink", 7, 40, SkinVariationType.None, "soft ice cream", new string[6] { "cream", "dessert", "ice", "icecream", "soft", "sweet" });

	public static readonly Emoji ShavedIce = new Emoji("1F367", "SHAVED ICE", "\ud83c\udf67", "shaved_ice", new string[1] { "shaved_ice" }, null, null, "Food & Drink", 7, 41, SkinVariationType.None, "shaved ice", new string[4] { "dessert", "ice", "shaved", "sweet" });

	public static readonly Emoji IceCream = new Emoji("1F368", "ICE CREAM", "\ud83c\udf68", "ice_cream", new string[1] { "ice_cream" }, null, null, "Food & Drink", 7, 42, SkinVariationType.None, "ice cream", new string[4] { "cream", "dessert", "ice", "sweet" });

	public static readonly Emoji Doughnut = new Emoji("1F369", "DOUGHNUT", "\ud83c\udf69", "doughnut", new string[1] { "doughnut" }, null, null, "Food & Drink", 7, 43, SkinVariationType.None, "doughnut", new string[4] { "dessert", "donut", "doughnut", "sweet" });

	public static readonly Emoji Cookie = new Emoji("1F36A", "COOKIE", "\ud83c\udf6a", "cookie", new string[1] { "cookie" }, null, null, "Food & Drink", 7, 44, SkinVariationType.None, "cookie", new string[3] { "cookie", "dessert", "sweet" });

	public static readonly Emoji Birthday = new Emoji("1F382", "BIRTHDAY CAKE", "\ud83c\udf82", "birthday", new string[1] { "birthday" }, null, null, "Food & Drink", 8, 16, SkinVariationType.None, "birthday cake", new string[6] { "birthday", "cake", "celebration", "dessert", "pastry", "sweet" });

	public static readonly Emoji Cake = new Emoji("1F370", "SHORTCAKE", "\ud83c\udf70", "cake", new string[1] { "cake" }, null, null, "Food & Drink", 7, 50, SkinVariationType.None, "shortcake", new string[6] { "cake", "dessert", "pastry", "shortcake", "slice", "sweet" });

	public static readonly Emoji Pie = new Emoji("1F967", "PIE", "\ud83e\udd67", "pie", new string[1] { "pie" }, null, null, "Food & Drink", 42, 19, SkinVariationType.None, "pie", new string[3] { "filling", "pastry", "pie" });

	public static readonly Emoji ChocolateBar = new Emoji("1F36B", "CHOCOLATE BAR", "\ud83c\udf6b", "chocolate_bar", new string[1] { "chocolate_bar" }, null, null, "Food & Drink", 7, 45, SkinVariationType.None, "chocolate bar", new string[4] { "bar", "chocolate", "dessert", "sweet" });

	public static readonly Emoji Candy = new Emoji("1F36C", "CANDY", "\ud83c\udf6c", "candy", new string[1] { "candy" }, null, null, "Food & Drink", 7, 46, SkinVariationType.None, "candy", new string[3] { "candy", "dessert", "sweet" });

	public static readonly Emoji Lollipop = new Emoji("1F36D", "LOLLIPOP", "\ud83c\udf6d", "lollipop", new string[1] { "lollipop" }, null, null, "Food & Drink", 7, 47, SkinVariationType.None, "lollipop", new string[4] { "candy", "dessert", "lollipop", "sweet" });

	public static readonly Emoji Custard = new Emoji("1F36E", "CUSTARD", "\ud83c\udf6e", "custard", new string[1] { "custard" }, null, null, "Food & Drink", 7, 48, SkinVariationType.None, "custard", new string[4] { "custard", "dessert", "pudding", "sweet" });

	public static readonly Emoji HoneyPot = new Emoji("1F36F", "HONEY POT", "\ud83c\udf6f", "honey_pot", new string[1] { "honey_pot" }, null, null, "Food & Drink", 7, 49, SkinVariationType.None, "honey pot", new string[4] { "honey", "honeypot", "pot", "sweet" });

	public static readonly Emoji BabyBottle = new Emoji("1F37C", "BABY BOTTLE", "\ud83c\udf7c", "baby_bottle", new string[1] { "baby_bottle" }, null, null, "Food & Drink", 8, 10, SkinVariationType.None, "baby bottle", new string[4] { "baby", "bottle", "drink", "milk" });

	public static readonly Emoji GlassOfMilk = new Emoji("1F95B", "GLASS OF MILK", "\ud83e\udd5b", "glass_of_milk", new string[1] { "glass_of_milk" }, null, null, "Food & Drink", 42, 7, SkinVariationType.None, "glass of milk", new string[4] { "drink", "glass", "glass of milk", "milk" });

	public static readonly Emoji Coffee = new Emoji("2615", "HOT BEVERAGE", "☕", "coffee", new string[1] { "coffee" }, null, null, "Food & Drink", 47, 24, SkinVariationType.None, "hot beverage", new string[6] { "beverage", "coffee", "drink", "hot", "steaming", "tea" });

	public static readonly Emoji Tea = new Emoji("1F375", "TEACUP WITHOUT HANDLE", "\ud83c\udf75", "tea", new string[1] { "tea" }, null, null, "Food & Drink", 8, 3, SkinVariationType.None, "teacup without handle", new string[6] { "beverage", "cup", "drink", "tea", "teacup", "teacup without handle" });

	public static readonly Emoji Sake = new Emoji("1F376", "SAKE BOTTLE AND CUP", "\ud83c\udf76", "sake", new string[1] { "sake" }, null, null, "Food & Drink", 8, 4, SkinVariationType.None, "sake", new string[6] { "bar", "beverage", "bottle", "cup", "drink", "sake" });

	public static readonly Emoji Champagne = new Emoji("1F37E", "BOTTLE WITH POPPING CORK", "\ud83c\udf7e", "champagne", new string[1] { "champagne" }, null, null, "Food & Drink", 8, 12, SkinVariationType.None, "bottle with popping cork", new string[6] { "bar", "bottle", "bottle with popping cork", "cork", "drink", "popping" });

	public static readonly Emoji WineGlass = new Emoji("1F377", "WINE GLASS", "\ud83c\udf77", "wine_glass", new string[1] { "wine_glass" }, null, null, "Food & Drink", 8, 5, SkinVariationType.None, "wine glass", new string[5] { "bar", "beverage", "drink", "glass", "wine" });

	public static readonly Emoji Cocktail = new Emoji("1F378", "COCKTAIL GLASS", "\ud83c\udf78", "cocktail", new string[1] { "cocktail" }, null, null, "Food & Drink", 8, 6, SkinVariationType.None, "cocktail glass", new string[4] { "bar", "cocktail", "drink", "glass" });

	public static readonly Emoji TropicalDrink = new Emoji("1F379", "TROPICAL DRINK", "\ud83c\udf79", "tropical_drink", new string[1] { "tropical_drink" }, null, null, "Food & Drink", 8, 7, SkinVariationType.None, "tropical drink", new string[3] { "bar", "drink", "tropical" });

	public static readonly Emoji Beer = new Emoji("1F37A", "BEER MUG", "\ud83c\udf7a", "beer", new string[1] { "beer" }, null, null, "Food & Drink", 8, 8, SkinVariationType.None, "beer mug", new string[4] { "bar", "beer", "drink", "mug" });

	public static readonly Emoji Beers = new Emoji("1F37B", "CLINKING BEER MUGS", "\ud83c\udf7b", "beers", new string[1] { "beers" }, null, null, "Food & Drink", 8, 9, SkinVariationType.None, "clinking beer mugs", new string[6] { "bar", "beer", "clink", "clinking beer mugs", "drink", "mug" });

	public static readonly Emoji ClinkingGlasses = new Emoji("1F942", "CLINKING GLASSES", "\ud83e\udd42", "clinking_glasses", new string[1] { "clinking_glasses" }, null, null, "Food & Drink", 41, 38, SkinVariationType.None, "clinking glasses", new string[5] { "celebrate", "clink", "clinking glasses", "drink", "glass" });

	public static readonly Emoji TumblerGlass = new Emoji("1F943", "TUMBLER GLASS", "\ud83e\udd43", "tumbler_glass", new string[1] { "tumbler_glass" }, null, null, "Food & Drink", 41, 39, SkinVariationType.None, "tumbler glass", new string[5] { "glass", "liquor", "shot", "tumbler", "whisky" });

	public static readonly Emoji CupWithStraw = new Emoji("1F964", "CUP WITH STRAW", "\ud83e\udd64", "cup_with_straw", new string[1] { "cup_with_straw" }, null, null, "Food & Drink", 42, 16, SkinVariationType.None, "cup with straw", new string[3] { "cup with straw", "juice", "soda" });

	public static readonly Emoji Chopsticks = new Emoji("1F962", "CHOPSTICKS", "\ud83e\udd62", "chopsticks", new string[1] { "chopsticks" }, null, null, "Food & Drink", 42, 14, SkinVariationType.None, "chopsticks", new string[2] { "chopsticks", "hashi" });

	public static readonly Emoji KnifeForkPlate = new Emoji("1F37D-FE0F", null, "\ud83c\udf7d\ufe0f", "knife_fork_plate", new string[1] { "knife_fork_plate" }, null, null, "Food & Drink", 8, 11, SkinVariationType.None, null, null);

	public static readonly Emoji ForkAndKnife = new Emoji("1F374", "FORK AND KNIFE", "\ud83c\udf74", "fork_and_knife", new string[1] { "fork_and_knife" }, null, null, "Food & Drink", 8, 2, SkinVariationType.None, "fork and knife", new string[5] { "cooking", "cutlery", "fork", "fork and knife", "knife" });

	public static readonly Emoji Spoon = new Emoji("1F944", "SPOON", "\ud83e\udd44", "spoon", new string[1] { "spoon" }, null, null, "Food & Drink", 41, 40, SkinVariationType.None, "spoon", new string[2] { "spoon", "tableware" });

	public static readonly Emoji Hocho = new Emoji("1F52A", "HOCHO", "\ud83d\udd2a", "hocho", new string[2] { "hocho", "knife" }, null, null, "Food & Drink", 27, 44, SkinVariationType.None, "kitchen knife", new string[6] { "cooking", "hocho", "kitchen knife", "knife", "tool", "weapon" });

	public static readonly Emoji Amphora = new Emoji("1F3FA", "AMPHORA", "\ud83c\udffa", "amphora", new string[1] { "amphora" }, null, null, "Food & Drink", 12, 24, SkinVariationType.None, "amphora", new string[8] { "amphora", "Aquarius", "cooking", "drink", "jug", "tool", "weapon", "zodiac" });

	public static readonly Emoji EarthAfrica = new Emoji("1F30D", "EARTH GLOBE EUROPE-AFRICA", "\ud83c\udf0d", "earth_africa", new string[1] { "earth_africa" }, null, null, "Travel & Places", 6, 5, SkinVariationType.None, "globe showing Europe-Africa", new string[6] { "Africa", "earth", "Europe", "globe", "globe showing Europe-Africa", "world" });

	public static readonly Emoji EarthAmericas = new Emoji("1F30E", "EARTH GLOBE AMERICAS", "\ud83c\udf0e", "earth_americas", new string[1] { "earth_americas" }, null, null, "Travel & Places", 6, 6, SkinVariationType.None, "globe showing Americas", new string[5] { "Americas", "earth", "globe", "globe showing Americas", "world" });

	public static readonly Emoji EarthAsia = new Emoji("1F30F", "EARTH GLOBE ASIA-AUSTRALIA", "\ud83c\udf0f", "earth_asia", new string[1] { "earth_asia" }, null, null, "Travel & Places", 6, 7, SkinVariationType.None, "globe showing Asia-Australia", new string[6] { "Asia", "Australia", "earth", "globe", "globe showing Asia-Australia", "world" });

	public static readonly Emoji GlobeWithMeridians = new Emoji("1F310", "GLOBE WITH MERIDIANS", "\ud83c\udf10", "globe_with_meridians", new string[1] { "globe_with_meridians" }, null, null, "Travel & Places", 6, 8, SkinVariationType.None, "globe with meridians", new string[5] { "earth", "globe", "globe with meridians", "meridians", "world" });

	public static readonly Emoji WorldMap = new Emoji("1F5FA-FE0F", null, "\ud83d\uddfa\ufe0f", "world_map", new string[1] { "world_map" }, null, null, "Travel & Places", 30, 18, SkinVariationType.None, null, null);

	public static readonly Emoji Japan = new Emoji("1F5FE", "SILHOUETTE OF JAPAN", "\ud83d\uddfe", "japan", new string[1] { "japan" }, null, null, "Travel & Places", 30, 22, SkinVariationType.None, "map of Japan", new string[3] { "Japan", "map", "map of Japan" });

	public static readonly Emoji SnowCappedMountain = new Emoji("1F3D4-FE0F", null, "\ud83c\udfd4\ufe0f", "snow_capped_mountain", new string[1] { "snow_capped_mountain" }, null, null, "Travel & Places", 11, 37, SkinVariationType.None, null, null);

	public static readonly Emoji Mountain = new Emoji("26F0-FE0F", null, "⛰\ufe0f", "mountain", new string[1] { "mountain" }, null, null, "Travel & Places", 48, 38, SkinVariationType.None, null, null);

	public static readonly Emoji Volcano = new Emoji("1F30B", "VOLCANO", "\ud83c\udf0b", "volcano", new string[1] { "volcano" }, null, null, "Travel & Places", 6, 3, SkinVariationType.None, "volcano", new string[3] { "eruption", "mountain", "volcano" });

	public static readonly Emoji MountFuji = new Emoji("1F5FB", "MOUNT FUJI", "\ud83d\uddfb", "mount_fuji", new string[1] { "mount_fuji" }, null, null, "Travel & Places", 30, 19, SkinVariationType.None, "mount fuji", new string[3] { "fuji", "mount fuji", "mountain" });

	public static readonly Emoji Camping = new Emoji("1F3D5-FE0F", null, "\ud83c\udfd5\ufe0f", "camping", new string[1] { "camping" }, null, null, "Travel & Places", 11, 38, SkinVariationType.None, null, null);

	public static readonly Emoji BeachWithUmbrella = new Emoji("1F3D6-FE0F", null, "\ud83c\udfd6\ufe0f", "beach_with_umbrella", new string[1] { "beach_with_umbrella" }, null, null, "Travel & Places", 11, 39, SkinVariationType.None, null, null);

	public static readonly Emoji Desert = new Emoji("1F3DC-FE0F", null, "\ud83c\udfdc\ufe0f", "desert", new string[1] { "desert" }, null, null, "Travel & Places", 11, 45, SkinVariationType.None, null, null);

	public static readonly Emoji DesertIsland = new Emoji("1F3DD-FE0F", null, "\ud83c\udfdd\ufe0f", "desert_island", new string[1] { "desert_island" }, null, null, "Travel & Places", 11, 46, SkinVariationType.None, null, null);

	public static readonly Emoji NationalPark = new Emoji("1F3DE-FE0F", null, "\ud83c\udfde\ufe0f", "national_park", new string[1] { "national_park" }, null, null, "Travel & Places", 11, 47, SkinVariationType.None, null, null);

	public static readonly Emoji Stadium = new Emoji("1F3DF-FE0F", null, "\ud83c\udfdf\ufe0f", "stadium", new string[1] { "stadium" }, null, null, "Travel & Places", 11, 48, SkinVariationType.None, null, null);

	public static readonly Emoji ClassicalBuilding = new Emoji("1F3DB-FE0F", null, "\ud83c\udfdb\ufe0f", "classical_building", new string[1] { "classical_building" }, null, null, "Travel & Places", 11, 44, SkinVariationType.None, null, null);

	public static readonly Emoji BuildingConstruction = new Emoji("1F3D7-FE0F", null, "\ud83c\udfd7\ufe0f", "building_construction", new string[1] { "building_construction" }, null, null, "Travel & Places", 11, 40, SkinVariationType.None, null, null);

	public static readonly Emoji HouseBuildings = new Emoji("1F3D8-FE0F", null, "\ud83c\udfd8\ufe0f", "house_buildings", new string[1] { "house_buildings" }, null, null, "Travel & Places", 11, 41, SkinVariationType.None, null, null);

	public static readonly Emoji DerelictHouseBuilding = new Emoji("1F3DA-FE0F", null, "\ud83c\udfda\ufe0f", "derelict_house_building", new string[1] { "derelict_house_building" }, null, null, "Travel & Places", 11, 43, SkinVariationType.None, null, null);

	public static readonly Emoji House = new Emoji("1F3E0", "HOUSE BUILDING", "\ud83c\udfe0", "house", new string[1] { "house" }, null, null, "Travel & Places", 11, 49, SkinVariationType.None, "house", new string[2] { "home", "house" });

	public static readonly Emoji HouseWithGarden = new Emoji("1F3E1", "HOUSE WITH GARDEN", "\ud83c\udfe1", "house_with_garden", new string[1] { "house_with_garden" }, null, null, "Travel & Places", 11, 50, SkinVariationType.None, "house with garden", new string[4] { "garden", "home", "house", "house with garden" });

	public static readonly Emoji Office = new Emoji("1F3E2", "OFFICE BUILDING", "\ud83c\udfe2", "office", new string[1] { "office" }, null, null, "Travel & Places", 11, 51, SkinVariationType.None, "office building", new string[2] { "building", "office building" });

	public static readonly Emoji PostOffice = new Emoji("1F3E3", "JAPANESE POST OFFICE", "\ud83c\udfe3", "post_office", new string[1] { "post_office" }, null, null, "Travel & Places", 12, 0, SkinVariationType.None, "Japanese post office", new string[3] { "Japanese", "Japanese post office", "post" });

	public static readonly Emoji EuropeanPostOffice = new Emoji("1F3E4", "EUROPEAN POST OFFICE", "\ud83c\udfe4", "european_post_office", new string[1] { "european_post_office" }, null, null, "Travel & Places", 12, 1, SkinVariationType.None, "post office", new string[3] { "European", "post", "post office" });

	public static readonly Emoji Hospital = new Emoji("1F3E5", "HOSPITAL", "\ud83c\udfe5", "hospital", new string[1] { "hospital" }, null, null, "Travel & Places", 12, 2, SkinVariationType.None, "hospital", new string[3] { "doctor", "hospital", "medicine" });

	public static readonly Emoji Bank = new Emoji("1F3E6", "BANK", "\ud83c\udfe6", "bank", new string[1] { "bank" }, null, null, "Travel & Places", 12, 3, SkinVariationType.None, "bank", new string[2] { "bank", "building" });

	public static readonly Emoji Hotel = new Emoji("1F3E8", "HOTEL", "\ud83c\udfe8", "hotel", new string[1] { "hotel" }, null, null, "Travel & Places", 12, 5, SkinVariationType.None, "hotel", new string[2] { "building", "hotel" });

	public static readonly Emoji LoveHotel = new Emoji("1F3E9", "LOVE HOTEL", "\ud83c\udfe9", "love_hotel", new string[1] { "love_hotel" }, null, null, "Travel & Places", 12, 6, SkinVariationType.None, "love hotel", new string[2] { "hotel", "love" });

	public static readonly Emoji ConvenienceStore = new Emoji("1F3EA", "CONVENIENCE STORE", "\ud83c\udfea", "convenience_store", new string[1] { "convenience_store" }, null, null, "Travel & Places", 12, 7, SkinVariationType.None, "convenience store", new string[2] { "convenience", "store" });

	public static readonly Emoji School = new Emoji("1F3EB", "SCHOOL", "\ud83c\udfeb", "school", new string[1] { "school" }, null, null, "Travel & Places", 12, 8, SkinVariationType.None, "school", new string[2] { "building", "school" });

	public static readonly Emoji DepartmentStore = new Emoji("1F3EC", "DEPARTMENT STORE", "\ud83c\udfec", "department_store", new string[1] { "department_store" }, null, null, "Travel & Places", 12, 9, SkinVariationType.None, "department store", new string[2] { "department", "store" });

	public static readonly Emoji Factory = new Emoji("1F3ED", "FACTORY", "\ud83c\udfed", "factory", new string[1] { "factory" }, null, null, "Travel & Places", 12, 10, SkinVariationType.None, "factory", new string[2] { "building", "factory" });

	public static readonly Emoji JapaneseCastle = new Emoji("1F3EF", "JAPANESE CASTLE", "\ud83c\udfef", "japanese_castle", new string[1] { "japanese_castle" }, null, null, "Travel & Places", 12, 12, SkinVariationType.None, "Japanese castle", new string[2] { "castle", "Japanese" });

	public static readonly Emoji EuropeanCastle = new Emoji("1F3F0", "EUROPEAN CASTLE", "\ud83c\udff0", "european_castle", new string[1] { "european_castle" }, null, null, "Travel & Places", 12, 13, SkinVariationType.None, "castle", new string[2] { "castle", "European" });

	public static readonly Emoji Wedding = new Emoji("1F492", "WEDDING", "\ud83d\udc92", "wedding", new string[1] { "wedding" }, null, null, "Travel & Places", 24, 44, SkinVariationType.None, "wedding", new string[3] { "chapel", "romance", "wedding" });

	public static readonly Emoji TokyoTower = new Emoji("1F5FC", "TOKYO TOWER", "\ud83d\uddfc", "tokyo_tower", new string[1] { "tokyo_tower" }, null, null, "Travel & Places", 30, 20, SkinVariationType.None, "Tokyo tower", new string[2] { "Tokyo", "tower" });

	public static readonly Emoji StatueOfLiberty = new Emoji("1F5FD", "STATUE OF LIBERTY", "\ud83d\uddfd", "statue_of_liberty", new string[1] { "statue_of_liberty" }, null, null, "Travel & Places", 30, 21, SkinVariationType.None, "Statue of Liberty", new string[3] { "liberty", "statue", "Statue of Liberty" });

	public static readonly Emoji Church = new Emoji("26EA", "CHURCH", "⛪", "church", new string[1] { "church" }, null, null, "Travel & Places", 48, 37, SkinVariationType.None, "church", new string[4] { "Christian", "church", "cross", "religion" });

	public static readonly Emoji Mosque = new Emoji("1F54C", "MOSQUE", "\ud83d\udd4c", "mosque", new string[1] { "mosque" }, null, null, "Travel & Places", 28, 15, SkinVariationType.None, "mosque", new string[4] { "islam", "mosque", "Muslim", "religion" });

	public static readonly Emoji Synagogue = new Emoji("1F54D", "SYNAGOGUE", "\ud83d\udd4d", "synagogue", new string[1] { "synagogue" }, null, null, "Travel & Places", 28, 16, SkinVariationType.None, "synagogue", new string[5] { "Jew", "Jewish", "religion", "synagogue", "temple" });

	public static readonly Emoji ShintoShrine = new Emoji("26E9-FE0F", null, "⛩\ufe0f", "shinto_shrine", new string[1] { "shinto_shrine" }, null, null, "Travel & Places", 48, 36, SkinVariationType.None, null, null);

	public static readonly Emoji Kaaba = new Emoji("1F54B", "KAABA", "\ud83d\udd4b", "kaaba", new string[1] { "kaaba" }, null, null, "Travel & Places", 28, 14, SkinVariationType.None, "kaaba", new string[4] { "islam", "kaaba", "Muslim", "religion" });

	public static readonly Emoji Fountain = new Emoji("26F2", "FOUNTAIN", "⛲", "fountain", new string[1] { "fountain" }, null, null, "Travel & Places", 48, 40, SkinVariationType.None, "fountain", new string[1] { "fountain" });

	public static readonly Emoji Tent = new Emoji("26FA", "TENT", "⛺", "tent", new string[1] { "tent" }, null, null, "Travel & Places", 49, 12, SkinVariationType.None, "tent", new string[2] { "camping", "tent" });

	public static readonly Emoji Foggy = new Emoji("1F301", "FOGGY", "\ud83c\udf01", "foggy", new string[1] { "foggy" }, null, null, "Travel & Places", 5, 45, SkinVariationType.None, "foggy", new string[2] { "fog", "foggy" });

	public static readonly Emoji NightWithStars = new Emoji("1F303", "NIGHT WITH STARS", "\ud83c\udf03", "night_with_stars", new string[1] { "night_with_stars" }, null, null, "Travel & Places", 5, 47, SkinVariationType.None, "night with stars", new string[3] { "night", "night with stars", "star" });

	public static readonly Emoji Cityscape = new Emoji("1F3D9-FE0F", null, "\ud83c\udfd9\ufe0f", "cityscape", new string[1] { "cityscape" }, null, null, "Travel & Places", 11, 42, SkinVariationType.None, null, null);

	public static readonly Emoji SunriseOverMountains = new Emoji("1F304", "SUNRISE OVER MOUNTAINS", "\ud83c\udf04", "sunrise_over_mountains", new string[1] { "sunrise_over_mountains" }, null, null, "Travel & Places", 5, 48, SkinVariationType.None, "sunrise over mountains", new string[5] { "morning", "mountain", "sun", "sunrise", "sunrise over mountains" });

	public static readonly Emoji Sunrise = new Emoji("1F305", "SUNRISE", "\ud83c\udf05", "sunrise", new string[1] { "sunrise" }, null, null, "Travel & Places", 5, 49, SkinVariationType.None, "sunrise", new string[3] { "morning", "sun", "sunrise" });

	public static readonly Emoji CitySunset = new Emoji("1F306", "CITYSCAPE AT DUSK", "\ud83c\udf06", "city_sunset", new string[1] { "city_sunset" }, null, null, "Travel & Places", 5, 50, SkinVariationType.None, "cityscape at dusk", new string[7] { "city", "cityscape at dusk", "dusk", "evening", "landscape", "sun", "sunset" });

	public static readonly Emoji CitySunrise = new Emoji("1F307", "SUNSET OVER BUILDINGS", "\ud83c\udf07", "city_sunrise", new string[1] { "city_sunrise" }, null, null, "Travel & Places", 5, 51, SkinVariationType.None, "sunset", new string[3] { "dusk", "sun", "sunset" });

	public static readonly Emoji BridgeAtNight = new Emoji("1F309", "BRIDGE AT NIGHT", "\ud83c\udf09", "bridge_at_night", new string[1] { "bridge_at_night" }, null, null, "Travel & Places", 6, 1, SkinVariationType.None, "bridge at night", new string[3] { "bridge", "bridge at night", "night" });

	public static readonly Emoji Hotsprings = new Emoji("2668-FE0F", "HOT SPRINGS", "♨\ufe0f", "hotsprings", new string[1] { "hotsprings" }, null, null, "Travel & Places", 48, 8, SkinVariationType.None, null, null);

	public static readonly Emoji MilkyWay = new Emoji("1F30C", "MILKY WAY", "\ud83c\udf0c", "milky_way", new string[1] { "milky_way" }, null, null, "Travel & Places", 6, 4, SkinVariationType.None, "milky way", new string[2] { "milky way", "space" });

	public static readonly Emoji CarouselHorse = new Emoji("1F3A0", "CAROUSEL HORSE", "\ud83c\udfa0", "carousel_horse", new string[1] { "carousel_horse" }, null, null, "Travel & Places", 8, 46, SkinVariationType.None, "carousel horse", new string[2] { "carousel", "horse" });

	public static readonly Emoji FerrisWheel = new Emoji("1F3A1", "FERRIS WHEEL", "\ud83c\udfa1", "ferris_wheel", new string[1] { "ferris_wheel" }, null, null, "Travel & Places", 8, 47, SkinVariationType.None, "ferris wheel", new string[3] { "amusement park", "ferris", "wheel" });

	public static readonly Emoji RollerCoaster = new Emoji("1F3A2", "ROLLER COASTER", "\ud83c\udfa2", "roller_coaster", new string[1] { "roller_coaster" }, null, null, "Travel & Places", 8, 48, SkinVariationType.None, "roller coaster", new string[3] { "amusement park", "coaster", "roller" });

	public static readonly Emoji Barber = new Emoji("1F488", "BARBER POLE", "\ud83d\udc88", "barber", new string[1] { "barber" }, null, null, "Travel & Places", 24, 34, SkinVariationType.None, "barber pole", new string[3] { "barber", "haircut", "pole" });

	public static readonly Emoji CircusTent = new Emoji("1F3AA", "CIRCUS TENT", "\ud83c\udfaa", "circus_tent", new string[1] { "circus_tent" }, null, null, "Travel & Places", 9, 4, SkinVariationType.None, "circus tent", new string[2] { "circus", "tent" });

	public static readonly Emoji SteamLocomotive = new Emoji("1F682", "STEAM LOCOMOTIVE", "\ud83d\ude82", "steam_locomotive", new string[1] { "steam_locomotive" }, null, null, "Travel & Places", 34, 10, SkinVariationType.None, "locomotive", new string[5] { "engine", "locomotive", "railway", "steam", "train" });

	public static readonly Emoji RailwayCar = new Emoji("1F683", "RAILWAY CAR", "\ud83d\ude83", "railway_car", new string[1] { "railway_car" }, null, null, "Travel & Places", 34, 11, SkinVariationType.None, "railway car", new string[6] { "car", "electric", "railway", "train", "tram", "trolleybus" });

	public static readonly Emoji BullettrainSide = new Emoji("1F684", "HIGH-SPEED TRAIN", "\ud83d\ude84", "bullettrain_side", new string[1] { "bullettrain_side" }, null, null, "Travel & Places", 34, 12, SkinVariationType.None, "high-speed train", new string[5] { "high-speed train", "railway", "shinkansen", "speed", "train" });

	public static readonly Emoji BullettrainFront = new Emoji("1F685", "HIGH-SPEED TRAIN WITH BULLET NOSE", "\ud83d\ude85", "bullettrain_front", new string[1] { "bullettrain_front" }, null, null, "Travel & Places", 34, 13, SkinVariationType.None, "bullet train", new string[5] { "bullet", "railway", "shinkansen", "speed", "train" });

	public static readonly Emoji Train2 = new Emoji("1F686", "TRAIN", "\ud83d\ude86", "train2", new string[1] { "train2" }, null, null, "Travel & Places", 34, 14, SkinVariationType.None, "train", new string[2] { "railway", "train" });

	public static readonly Emoji Metro = new Emoji("1F687", "METRO", "\ud83d\ude87", "metro", new string[1] { "metro" }, null, null, "Travel & Places", 34, 15, SkinVariationType.None, "metro", new string[2] { "metro", "subway" });

	public static readonly Emoji LightRail = new Emoji("1F688", "LIGHT RAIL", "\ud83d\ude88", "light_rail", new string[1] { "light_rail" }, null, null, "Travel & Places", 34, 16, SkinVariationType.None, "light rail", new string[2] { "light rail", "railway" });

	public static readonly Emoji Station = new Emoji("1F689", "STATION", "\ud83d\ude89", "station", new string[1] { "station" }, null, null, "Travel & Places", 34, 17, SkinVariationType.None, "station", new string[3] { "railway", "station", "train" });

	public static readonly Emoji Tram = new Emoji("1F68A", "TRAM", "\ud83d\ude8a", "tram", new string[1] { "tram" }, null, null, "Travel & Places", 34, 18, SkinVariationType.None, "tram", new string[2] { "tram", "trolleybus" });

	public static readonly Emoji Monorail = new Emoji("1F69D", "MONORAIL", "\ud83d\ude9d", "monorail", new string[1] { "monorail" }, null, null, "Travel & Places", 34, 37, SkinVariationType.None, "monorail", new string[2] { "monorail", "vehicle" });

	public static readonly Emoji MountainRailway = new Emoji("1F69E", "MOUNTAIN RAILWAY", "\ud83d\ude9e", "mountain_railway", new string[1] { "mountain_railway" }, null, null, "Travel & Places", 34, 38, SkinVariationType.None, "mountain railway", new string[3] { "car", "mountain", "railway" });

	public static readonly Emoji Train = new Emoji("1F68B", "TRAM CAR", "\ud83d\ude8b", "train", new string[1] { "train" }, null, null, "Travel & Places", 34, 19, SkinVariationType.None, "tram car", new string[3] { "car", "tram", "trolleybus" });

	public static readonly Emoji Bus = new Emoji("1F68C", "BUS", "\ud83d\ude8c", "bus", new string[1] { "bus" }, null, null, "Travel & Places", 34, 20, SkinVariationType.None, "bus", new string[2] { "bus", "vehicle" });

	public static readonly Emoji OncomingBus = new Emoji("1F68D", "ONCOMING BUS", "\ud83d\ude8d", "oncoming_bus", new string[1] { "oncoming_bus" }, null, null, "Travel & Places", 34, 21, SkinVariationType.None, "oncoming bus", new string[2] { "bus", "oncoming" });

	public static readonly Emoji Trolleybus = new Emoji("1F68E", "TROLLEYBUS", "\ud83d\ude8e", "trolleybus", new string[1] { "trolleybus" }, null, null, "Travel & Places", 34, 22, SkinVariationType.None, "trolleybus", new string[4] { "bus", "tram", "trolley", "trolleybus" });

	public static readonly Emoji Minibus = new Emoji("1F690", "MINIBUS", "\ud83d\ude90", "minibus", new string[1] { "minibus" }, null, null, "Travel & Places", 34, 24, SkinVariationType.None, "minibus", new string[2] { "bus", "minibus" });

	public static readonly Emoji Ambulance = new Emoji("1F691", "AMBULANCE", "\ud83d\ude91", "ambulance", new string[1] { "ambulance" }, null, null, "Travel & Places", 34, 25, SkinVariationType.None, "ambulance", new string[2] { "ambulance", "vehicle" });

	public static readonly Emoji FireEngine = new Emoji("1F692", "FIRE ENGINE", "\ud83d\ude92", "fire_engine", new string[1] { "fire_engine" }, null, null, "Travel & Places", 34, 26, SkinVariationType.None, "fire engine", new string[3] { "engine", "fire", "truck" });

	public static readonly Emoji PoliceCar = new Emoji("1F693", "POLICE CAR", "\ud83d\ude93", "police_car", new string[1] { "police_car" }, null, null, "Travel & Places", 34, 27, SkinVariationType.None, "police car", new string[3] { "car", "patrol", "police" });

	public static readonly Emoji OncomingPoliceCar = new Emoji("1F694", "ONCOMING POLICE CAR", "\ud83d\ude94", "oncoming_police_car", new string[1] { "oncoming_police_car" }, null, null, "Travel & Places", 34, 28, SkinVariationType.None, "oncoming police car", new string[3] { "car", "oncoming", "police" });

	public static readonly Emoji Taxi = new Emoji("1F695", "TAXI", "\ud83d\ude95", "taxi", new string[1] { "taxi" }, null, null, "Travel & Places", 34, 29, SkinVariationType.None, "taxi", new string[2] { "taxi", "vehicle" });

	public static readonly Emoji OncomingTaxi = new Emoji("1F696", "ONCOMING TAXI", "\ud83d\ude96", "oncoming_taxi", new string[1] { "oncoming_taxi" }, null, null, "Travel & Places", 34, 30, SkinVariationType.None, "oncoming taxi", new string[2] { "oncoming", "taxi" });

	public static readonly Emoji Car = new Emoji("1F697", "AUTOMOBILE", "\ud83d\ude97", "car", new string[2] { "car", "red_car" }, null, null, "Travel & Places", 34, 31, SkinVariationType.None, "automobile", new string[2] { "automobile", "car" });

	public static readonly Emoji OncomingAutomobile = new Emoji("1F698", "ONCOMING AUTOMOBILE", "\ud83d\ude98", "oncoming_automobile", new string[1] { "oncoming_automobile" }, null, null, "Travel & Places", 34, 32, SkinVariationType.None, "oncoming automobile", new string[3] { "automobile", "car", "oncoming" });

	public static readonly Emoji BlueCar = new Emoji("1F699", "RECREATIONAL VEHICLE", "\ud83d\ude99", "blue_car", new string[1] { "blue_car" }, null, null, "Travel & Places", 34, 33, SkinVariationType.None, "sport utility vehicle", new string[3] { "recreational", "sport utility", "sport utility vehicle" });

	public static readonly Emoji Truck = new Emoji("1F69A", "DELIVERY TRUCK", "\ud83d\ude9a", "truck", new string[1] { "truck" }, null, null, "Travel & Places", 34, 34, SkinVariationType.None, "delivery truck", new string[2] { "delivery", "truck" });

	public static readonly Emoji ArticulatedLorry = new Emoji("1F69B", "ARTICULATED LORRY", "\ud83d\ude9b", "articulated_lorry", new string[1] { "articulated_lorry" }, null, null, "Travel & Places", 34, 35, SkinVariationType.None, "articulated lorry", new string[4] { "articulated lorry", "lorry", "semi", "truck" });

	public static readonly Emoji Tractor = new Emoji("1F69C", "TRACTOR", "\ud83d\ude9c", "tractor", new string[1] { "tractor" }, null, null, "Travel & Places", 34, 36, SkinVariationType.None, "tractor", new string[2] { "tractor", "vehicle" });

	public static readonly Emoji Bike = new Emoji("1F6B2", "BICYCLE", "\ud83d\udeb2", "bike", new string[1] { "bike" }, null, null, "Travel & Places", 35, 23, SkinVariationType.None, "bicycle", new string[2] { "bicycle", "bike" });

	public static readonly Emoji Scooter = new Emoji("1F6F4", "SCOOTER", "\ud83d\udef4", "scooter", new string[1] { "scooter" }, null, null, "Travel & Places", 37, 19, SkinVariationType.None, "kick scooter", new string[2] { "kick", "scooter" });

	public static readonly Emoji MotorScooter = new Emoji("1F6F5", "MOTOR SCOOTER", "\ud83d\udef5", "motor_scooter", new string[1] { "motor_scooter" }, null, null, "Travel & Places", 37, 20, SkinVariationType.None, "motor scooter", new string[2] { "motor", "scooter" });

	public static readonly Emoji Busstop = new Emoji("1F68F", "BUS STOP", "\ud83d\ude8f", "busstop", new string[1] { "busstop" }, null, null, "Travel & Places", 34, 23, SkinVariationType.None, "bus stop", new string[3] { "bus", "busstop", "stop" });

	public static readonly Emoji Motorway = new Emoji("1F6E3-FE0F", null, "\ud83d\udee3\ufe0f", "motorway", new string[1] { "motorway" }, null, null, "Travel & Places", 37, 11, SkinVariationType.None, null, null);

	public static readonly Emoji RailwayTrack = new Emoji("1F6E4-FE0F", null, "\ud83d\udee4\ufe0f", "railway_track", new string[1] { "railway_track" }, null, null, "Travel & Places", 37, 12, SkinVariationType.None, null, null);

	public static readonly Emoji OilDrum = new Emoji("1F6E2-FE0F", null, "\ud83d\udee2\ufe0f", "oil_drum", new string[1] { "oil_drum" }, null, null, "Objects", 37, 10, SkinVariationType.None, null, null);

	public static readonly Emoji Fuelpump = new Emoji("26FD", "FUEL PUMP", "⛽", "fuelpump", new string[1] { "fuelpump" }, null, null, "Travel & Places", 49, 13, SkinVariationType.None, "fuel pump", new string[6] { "diesel", "fuel", "fuelpump", "gas", "pump", "station" });

	public static readonly Emoji RotatingLight = new Emoji("1F6A8", "POLICE CARS REVOLVING LIGHT", "\ud83d\udea8", "rotating_light", new string[1] { "rotating_light" }, null, null, "Travel & Places", 35, 13, SkinVariationType.None, "police car light", new string[5] { "beacon", "car", "light", "police", "revolving" });

	public static readonly Emoji TrafficLight = new Emoji("1F6A5", "HORIZONTAL TRAFFIC LIGHT", "\ud83d\udea5", "traffic_light", new string[1] { "traffic_light" }, null, null, "Travel & Places", 35, 10, SkinVariationType.None, "horizontal traffic light", new string[4] { "horizontal traffic light", "light", "signal", "traffic" });

	public static readonly Emoji VerticalTrafficLight = new Emoji("1F6A6", "VERTICAL TRAFFIC LIGHT", "\ud83d\udea6", "vertical_traffic_light", new string[1] { "vertical_traffic_light" }, null, null, "Travel & Places", 35, 11, SkinVariationType.None, "vertical traffic light", new string[4] { "light", "signal", "traffic", "vertical traffic light" });

	public static readonly Emoji OctagonalSign = new Emoji("1F6D1", "OCTAGONAL SIGN", "\ud83d\uded1", "octagonal_sign", new string[1] { "octagonal_sign" }, null, null, "Travel & Places", 37, 6, SkinVariationType.None, "stop sign", new string[3] { "octagonal", "sign", "stop" });

	public static readonly Emoji Construction = new Emoji("1F6A7", "CONSTRUCTION SIGN", "\ud83d\udea7", "construction", new string[1] { "construction" }, null, null, "Travel & Places", 35, 12, SkinVariationType.None, "construction", new string[2] { "barrier", "construction" });

	public static readonly Emoji Anchor = new Emoji("2693", "ANCHOR", "⚓", "anchor", new string[1] { "anchor" }, null, null, "Travel & Places", 48, 12, SkinVariationType.None, "anchor", new string[3] { "anchor", "ship", "tool" });

	public static readonly Emoji Boat = new Emoji("26F5", "SAILBOAT", "⛵", "boat", new string[2] { "boat", "sailboat" }, null, null, "Travel & Places", 48, 43, SkinVariationType.None, "sailboat", new string[5] { "boat", "resort", "sailboat", "sea", "yacht" });

	public static readonly Emoji Canoe = new Emoji("1F6F6", "CANOE", "\ud83d\udef6", "canoe", new string[1] { "canoe" }, null, null, "Travel & Places", 37, 21, SkinVariationType.None, "canoe", new string[2] { "boat", "canoe" });

	public static readonly Emoji Speedboat = new Emoji("1F6A4", "SPEEDBOAT", "\ud83d\udea4", "speedboat", new string[1] { "speedboat" }, null, null, "Travel & Places", 35, 9, SkinVariationType.None, "speedboat", new string[2] { "boat", "speedboat" });

	public static readonly Emoji PassengerShip = new Emoji("1F6F3-FE0F", null, "\ud83d\udef3\ufe0f", "passenger_ship", new string[1] { "passenger_ship" }, null, null, "Travel & Places", 37, 18, SkinVariationType.None, null, null);

	public static readonly Emoji Ferry = new Emoji("26F4-FE0F", null, "⛴\ufe0f", "ferry", new string[1] { "ferry" }, null, null, "Travel & Places", 48, 42, SkinVariationType.None, null, null);

	public static readonly Emoji MotorBoat = new Emoji("1F6E5-FE0F", null, "\ud83d\udee5\ufe0f", "motor_boat", new string[1] { "motor_boat" }, null, null, "Travel & Places", 37, 13, SkinVariationType.None, null, null);

	public static readonly Emoji Ship = new Emoji("1F6A2", "SHIP", "\ud83d\udea2", "ship", new string[1] { "ship" }, null, null, "Travel & Places", 34, 42, SkinVariationType.None, "ship", new string[3] { "boat", "passenger", "ship" });

	public static readonly Emoji Airplane = new Emoji("2708-FE0F", "AIRPLANE", "✈\ufe0f", "airplane", new string[1] { "airplane" }, null, null, "Travel & Places", 49, 16, SkinVariationType.None, null, null);

	public static readonly Emoji SmallAirplane = new Emoji("1F6E9-FE0F", null, "\ud83d\udee9\ufe0f", "small_airplane", new string[1] { "small_airplane" }, null, null, "Travel & Places", 37, 14, SkinVariationType.None, null, null);

	public static readonly Emoji AirplaneDeparture = new Emoji("1F6EB", "AIRPLANE DEPARTURE", "\ud83d\udeeb", "airplane_departure", new string[1] { "airplane_departure" }, null, null, "Travel & Places", 37, 15, SkinVariationType.None, "airplane departure", new string[5] { "aeroplane", "airplane", "check-in", "departure", "departures" });

	public static readonly Emoji AirplaneArriving = new Emoji("1F6EC", "AIRPLANE ARRIVING", "\ud83d\udeec", "airplane_arriving", new string[1] { "airplane_arriving" }, null, null, "Travel & Places", 37, 16, SkinVariationType.None, "airplane arrival", new string[6] { "aeroplane", "airplane", "airplane arrival", "arrivals", "arriving", "landing" });

	public static readonly Emoji Seat = new Emoji("1F4BA", "SEAT", "\ud83d\udcba", "seat", new string[1] { "seat" }, null, null, "Travel & Places", 25, 37, SkinVariationType.None, "seat", new string[2] { "chair", "seat" });

	public static readonly Emoji Helicopter = new Emoji("1F681", "HELICOPTER", "\ud83d\ude81", "helicopter", new string[1] { "helicopter" }, null, null, "Travel & Places", 34, 9, SkinVariationType.None, "helicopter", new string[2] { "helicopter", "vehicle" });

	public static readonly Emoji SuspensionRailway = new Emoji("1F69F", "SUSPENSION RAILWAY", "\ud83d\ude9f", "suspension_railway", new string[1] { "suspension_railway" }, null, null, "Travel & Places", 34, 39, SkinVariationType.None, "suspension railway", new string[2] { "railway", "suspension" });

	public static readonly Emoji MountainCableway = new Emoji("1F6A0", "MOUNTAIN CABLEWAY", "\ud83d\udea0", "mountain_cableway", new string[1] { "mountain_cableway" }, null, null, "Travel & Places", 34, 40, SkinVariationType.None, "mountain cableway", new string[4] { "cable", "gondola", "mountain", "mountain cableway" });

	public static readonly Emoji AerialTramway = new Emoji("1F6A1", "AERIAL TRAMWAY", "\ud83d\udea1", "aerial_tramway", new string[1] { "aerial_tramway" }, null, null, "Travel & Places", 34, 41, SkinVariationType.None, "aerial tramway", new string[5] { "aerial", "cable", "car", "gondola", "tramway" });

	public static readonly Emoji Satellite = new Emoji("1F6F0-FE0F", null, "\ud83d\udef0\ufe0f", "satellite", new string[1] { "satellite" }, null, null, "Travel & Places", 37, 17, SkinVariationType.None, null, null);

	public static readonly Emoji Rocket = new Emoji("1F680", "ROCKET", "\ud83d\ude80", "rocket", new string[1] { "rocket" }, null, null, "Travel & Places", 34, 8, SkinVariationType.None, "rocket", new string[2] { "rocket", "space" });

	public static readonly Emoji FlyingSaucer = new Emoji("1F6F8", "FLYING SAUCER", "\ud83d\udef8", "flying_saucer", new string[1] { "flying_saucer" }, null, null, "Travel & Places", 37, 23, SkinVariationType.None, "flying saucer", new string[2] { "flying saucer", "UFO" });

	public static readonly Emoji BellhopBell = new Emoji("1F6CE-FE0F", null, "\ud83d\udece\ufe0f", "bellhop_bell", new string[1] { "bellhop_bell" }, null, null, "Travel & Places", 37, 3, SkinVariationType.None, null, null);

	public static readonly Emoji Hourglass = new Emoji("231B", "HOURGLASS", "⌛", "hourglass", new string[1] { "hourglass" }, null, null, "Travel & Places", 46, 42, SkinVariationType.None, "hourglass done", new string[3] { "hourglass done", "sand", "timer" });

	public static readonly Emoji HourglassFlowingSand = new Emoji("23F3", "HOURGLASS WITH FLOWING SAND", "⏳", "hourglass_flowing_sand", new string[1] { "hourglass_flowing_sand" }, null, null, "Travel & Places", 47, 3, SkinVariationType.None, "hourglass not done", new string[4] { "hourglass", "hourglass not done", "sand", "timer" });

	public static readonly Emoji Watch = new Emoji("231A", "WATCH", "⌚", "watch", new string[1] { "watch" }, null, null, "Travel & Places", 46, 41, SkinVariationType.None, "watch", new string[2] { "clock", "watch" });

	public static readonly Emoji AlarmClock = new Emoji("23F0", "ALARM CLOCK", "⏰", "alarm_clock", new string[1] { "alarm_clock" }, null, null, "Travel & Places", 47, 0, SkinVariationType.None, "alarm clock", new string[2] { "alarm", "clock" });

	public static readonly Emoji Stopwatch = new Emoji("23F1-FE0F", null, "⏱\ufe0f", "stopwatch", new string[1] { "stopwatch" }, null, null, "Travel & Places", 47, 1, SkinVariationType.None, null, null);

	public static readonly Emoji TimerClock = new Emoji("23F2-FE0F", null, "⏲\ufe0f", "timer_clock", new string[1] { "timer_clock" }, null, null, "Travel & Places", 47, 2, SkinVariationType.None, null, null);

	public static readonly Emoji MantelpieceClock = new Emoji("1F570-FE0F", null, "\ud83d\udd70\ufe0f", "mantelpiece_clock", new string[1] { "mantelpiece_clock" }, null, null, "Travel & Places", 28, 43, SkinVariationType.None, null, null);

	public static readonly Emoji Clock12 = new Emoji("1F55B", "CLOCK FACE TWELVE OCLOCK", "\ud83d\udd5b", "clock12", new string[1] { "clock12" }, null, null, "Travel & Places", 28, 29, SkinVariationType.None, "twelve o’clock", new string[6] { "00", "12", "12:00", "clock", "o’clock", "twelve" });

	public static readonly Emoji Clock1230 = new Emoji("1F567", "CLOCK FACE TWELVE-THIRTY", "\ud83d\udd67", "clock1230", new string[1] { "clock1230" }, null, null, "Travel & Places", 28, 41, SkinVariationType.None, "twelve-thirty", new string[7] { "12", "12:30", "30", "clock", "thirty", "twelve", "twelve-thirty" });

	public static readonly Emoji Clock1 = new Emoji("1F550", "CLOCK FACE ONE OCLOCK", "\ud83d\udd50", "clock1", new string[1] { "clock1" }, null, null, "Travel & Places", 28, 18, SkinVariationType.None, "one o’clock", new string[6] { "00", "1", "1:00", "clock", "o’clock", "one" });

	public static readonly Emoji Clock130 = new Emoji("1F55C", "CLOCK FACE ONE-THIRTY", "\ud83d\udd5c", "clock130", new string[1] { "clock130" }, null, null, "Travel & Places", 28, 30, SkinVariationType.None, "one-thirty", new string[7] { "1", "1:30", "30", "clock", "one", "one-thirty", "thirty" });

	public static readonly Emoji Clock2 = new Emoji("1F551", "CLOCK FACE TWO OCLOCK", "\ud83d\udd51", "clock2", new string[1] { "clock2" }, null, null, "Travel & Places", 28, 19, SkinVariationType.None, "two o’clock", new string[6] { "00", "2", "2:00", "clock", "o’clock", "two" });

	public static readonly Emoji Clock230 = new Emoji("1F55D", "CLOCK FACE TWO-THIRTY", "\ud83d\udd5d", "clock230", new string[1] { "clock230" }, null, null, "Travel & Places", 28, 31, SkinVariationType.None, "two-thirty", new string[7] { "2", "2:30", "30", "clock", "thirty", "two", "two-thirty" });

	public static readonly Emoji Clock3 = new Emoji("1F552", "CLOCK FACE THREE OCLOCK", "\ud83d\udd52", "clock3", new string[1] { "clock3" }, null, null, "Travel & Places", 28, 20, SkinVariationType.None, "three o’clock", new string[6] { "00", "3", "3:00", "clock", "o’clock", "three" });

	public static readonly Emoji Clock330 = new Emoji("1F55E", "CLOCK FACE THREE-THIRTY", "\ud83d\udd5e", "clock330", new string[1] { "clock330" }, null, null, "Travel & Places", 28, 32, SkinVariationType.None, "three-thirty", new string[7] { "3", "3:30", "30", "clock", "thirty", "three", "three-thirty" });

	public static readonly Emoji Clock4 = new Emoji("1F553", "CLOCK FACE FOUR OCLOCK", "\ud83d\udd53", "clock4", new string[1] { "clock4" }, null, null, "Travel & Places", 28, 21, SkinVariationType.None, "four o’clock", new string[6] { "00", "4", "4:00", "clock", "four", "o’clock" });

	public static readonly Emoji Clock430 = new Emoji("1F55F", "CLOCK FACE FOUR-THIRTY", "\ud83d\udd5f", "clock430", new string[1] { "clock430" }, null, null, "Travel & Places", 28, 33, SkinVariationType.None, "four-thirty", new string[7] { "30", "4", "4:30", "clock", "four", "four-thirty", "thirty" });

	public static readonly Emoji Clock5 = new Emoji("1F554", "CLOCK FACE FIVE OCLOCK", "\ud83d\udd54", "clock5", new string[1] { "clock5" }, null, null, "Travel & Places", 28, 22, SkinVariationType.None, "five o’clock", new string[6] { "00", "5", "5:00", "clock", "five", "o’clock" });

	public static readonly Emoji Clock530 = new Emoji("1F560", "CLOCK FACE FIVE-THIRTY", "\ud83d\udd60", "clock530", new string[1] { "clock530" }, null, null, "Travel & Places", 28, 34, SkinVariationType.None, "five-thirty", new string[7] { "30", "5", "5:30", "clock", "five", "five-thirty", "thirty" });

	public static readonly Emoji Clock6 = new Emoji("1F555", "CLOCK FACE SIX OCLOCK", "\ud83d\udd55", "clock6", new string[1] { "clock6" }, null, null, "Travel & Places", 28, 23, SkinVariationType.None, "six o’clock", new string[6] { "00", "6", "6:00", "clock", "o’clock", "six" });

	public static readonly Emoji Clock630 = new Emoji("1F561", "CLOCK FACE SIX-THIRTY", "\ud83d\udd61", "clock630", new string[1] { "clock630" }, null, null, "Travel & Places", 28, 35, SkinVariationType.None, "six-thirty", new string[7] { "30", "6", "6:30", "clock", "six", "six-thirty", "thirty" });

	public static readonly Emoji Clock7 = new Emoji("1F556", "CLOCK FACE SEVEN OCLOCK", "\ud83d\udd56", "clock7", new string[1] { "clock7" }, null, null, "Travel & Places", 28, 24, SkinVariationType.None, "seven o’clock", new string[6] { "00", "7", "7:00", "clock", "o’clock", "seven" });

	public static readonly Emoji Clock730 = new Emoji("1F562", "CLOCK FACE SEVEN-THIRTY", "\ud83d\udd62", "clock730", new string[1] { "clock730" }, null, null, "Travel & Places", 28, 36, SkinVariationType.None, "seven-thirty", new string[7] { "30", "7", "7:30", "clock", "seven", "seven-thirty", "thirty" });

	public static readonly Emoji Clock8 = new Emoji("1F557", "CLOCK FACE EIGHT OCLOCK", "\ud83d\udd57", "clock8", new string[1] { "clock8" }, null, null, "Travel & Places", 28, 25, SkinVariationType.None, "eight o’clock", new string[6] { "00", "8", "8:00", "clock", "eight", "o’clock" });

	public static readonly Emoji Clock830 = new Emoji("1F563", "CLOCK FACE EIGHT-THIRTY", "\ud83d\udd63", "clock830", new string[1] { "clock830" }, null, null, "Travel & Places", 28, 37, SkinVariationType.None, "eight-thirty", new string[7] { "30", "8", "8:30", "clock", "eight", "eight-thirty", "thirty" });

	public static readonly Emoji Clock9 = new Emoji("1F558", "CLOCK FACE NINE OCLOCK", "\ud83d\udd58", "clock9", new string[1] { "clock9" }, null, null, "Travel & Places", 28, 26, SkinVariationType.None, "nine o’clock", new string[6] { "00", "9", "9:00", "clock", "nine", "o’clock" });

	public static readonly Emoji Clock930 = new Emoji("1F564", "CLOCK FACE NINE-THIRTY", "\ud83d\udd64", "clock930", new string[1] { "clock930" }, null, null, "Travel & Places", 28, 38, SkinVariationType.None, "nine-thirty", new string[7] { "30", "9", "9:30", "clock", "nine", "nine-thirty", "thirty" });

	public static readonly Emoji Clock10 = new Emoji("1F559", "CLOCK FACE TEN OCLOCK", "\ud83d\udd59", "clock10", new string[1] { "clock10" }, null, null, "Travel & Places", 28, 27, SkinVariationType.None, "ten o’clock", new string[6] { "00", "10", "10:00", "clock", "o’clock", "ten" });

	public static readonly Emoji Clock1030 = new Emoji("1F565", "CLOCK FACE TEN-THIRTY", "\ud83d\udd65", "clock1030", new string[1] { "clock1030" }, null, null, "Travel & Places", 28, 39, SkinVariationType.None, "ten-thirty", new string[7] { "10", "10:30", "30", "clock", "ten", "ten-thirty", "thirty" });

	public static readonly Emoji Clock11 = new Emoji("1F55A", "CLOCK FACE ELEVEN OCLOCK", "\ud83d\udd5a", "clock11", new string[1] { "clock11" }, null, null, "Travel & Places", 28, 28, SkinVariationType.None, "eleven o’clock", new string[6] { "00", "11", "11:00", "clock", "eleven", "o’clock" });

	public static readonly Emoji Clock1130 = new Emoji("1F566", "CLOCK FACE ELEVEN-THIRTY", "\ud83d\udd66", "clock1130", new string[1] { "clock1130" }, null, null, "Travel & Places", 28, 40, SkinVariationType.None, "eleven-thirty", new string[7] { "11", "11:30", "30", "clock", "eleven", "eleven-thirty", "thirty" });

	public static readonly Emoji NewMoon = new Emoji("1F311", "NEW MOON SYMBOL", "\ud83c\udf11", "new_moon", new string[1] { "new_moon" }, null, null, "Travel & Places", 6, 9, SkinVariationType.None, "new moon", new string[3] { "dark", "moon", "new moon" });

	public static readonly Emoji WaxingCrescentMoon = new Emoji("1F312", "WAXING CRESCENT MOON SYMBOL", "\ud83c\udf12", "waxing_crescent_moon", new string[1] { "waxing_crescent_moon" }, null, null, "Travel & Places", 6, 10, SkinVariationType.None, "waxing crescent moon", new string[3] { "crescent", "moon", "waxing" });

	public static readonly Emoji FirstQuarterMoon = new Emoji("1F313", "FIRST QUARTER MOON SYMBOL", "\ud83c\udf13", "first_quarter_moon", new string[1] { "first_quarter_moon" }, null, null, "Travel & Places", 6, 11, SkinVariationType.None, "first quarter moon", new string[3] { "first quarter moon", "moon", "quarter" });

	public static readonly Emoji Moon = new Emoji("1F314", "WAXING GIBBOUS MOON SYMBOL", "\ud83c\udf14", "moon", new string[2] { "moon", "waxing_gibbous_moon" }, null, null, "Travel & Places", 6, 12, SkinVariationType.None, "waxing gibbous moon", new string[3] { "gibbous", "moon", "waxing" });

	public static readonly Emoji FullMoon = new Emoji("1F315", "FULL MOON SYMBOL", "\ud83c\udf15", "full_moon", new string[1] { "full_moon" }, null, null, "Travel & Places", 6, 13, SkinVariationType.None, "full moon", new string[2] { "full", "moon" });

	public static readonly Emoji WaningGibbousMoon = new Emoji("1F316", "WANING GIBBOUS MOON SYMBOL", "\ud83c\udf16", "waning_gibbous_moon", new string[1] { "waning_gibbous_moon" }, null, null, "Travel & Places", 6, 14, SkinVariationType.None, "waning gibbous moon", new string[3] { "gibbous", "moon", "waning" });

	public static readonly Emoji LastQuarterMoon = new Emoji("1F317", "LAST QUARTER MOON SYMBOL", "\ud83c\udf17", "last_quarter_moon", new string[1] { "last_quarter_moon" }, null, null, "Travel & Places", 6, 15, SkinVariationType.None, "last quarter moon", new string[3] { "last quarter moon", "moon", "quarter" });

	public static readonly Emoji WaningCrescentMoon = new Emoji("1F318", "WANING CRESCENT MOON SYMBOL", "\ud83c\udf18", "waning_crescent_moon", new string[1] { "waning_crescent_moon" }, null, null, "Travel & Places", 6, 16, SkinVariationType.None, "waning crescent moon", new string[3] { "crescent", "moon", "waning" });

	public static readonly Emoji CrescentMoon = new Emoji("1F319", "CRESCENT MOON", "\ud83c\udf19", "crescent_moon", new string[1] { "crescent_moon" }, null, null, "Travel & Places", 6, 17, SkinVariationType.None, "crescent moon", new string[2] { "crescent", "moon" });

	public static readonly Emoji NewMoonWithFace = new Emoji("1F31A", "NEW MOON WITH FACE", "\ud83c\udf1a", "new_moon_with_face", new string[1] { "new_moon_with_face" }, null, null, "Travel & Places", 6, 18, SkinVariationType.None, "new moon face", new string[3] { "face", "moon", "new moon face" });

	public static readonly Emoji FirstQuarterMoonWithFace = new Emoji("1F31B", "FIRST QUARTER MOON WITH FACE", "\ud83c\udf1b", "first_quarter_moon_with_face", new string[1] { "first_quarter_moon_with_face" }, null, null, "Travel & Places", 6, 19, SkinVariationType.None, "first quarter moon face", new string[4] { "face", "first quarter moon face", "moon", "quarter" });

	public static readonly Emoji LastQuarterMoonWithFace = new Emoji("1F31C", "LAST QUARTER MOON WITH FACE", "\ud83c\udf1c", "last_quarter_moon_with_face", new string[1] { "last_quarter_moon_with_face" }, null, null, "Travel & Places", 6, 20, SkinVariationType.None, "last quarter moon face", new string[4] { "face", "last quarter moon face", "moon", "quarter" });

	public static readonly Emoji Thermometer = new Emoji("1F321-FE0F", null, "\ud83c\udf21\ufe0f", "thermometer", new string[1] { "thermometer" }, null, null, "Travel & Places", 6, 25, SkinVariationType.None, null, null);

	public static readonly Emoji Sunny = new Emoji("2600-FE0F", "BLACK SUN WITH RAYS", "☀\ufe0f", "sunny", new string[1] { "sunny" }, null, null, "Travel & Places", 47, 16, SkinVariationType.None, null, null);

	public static readonly Emoji FullMoonWithFace = new Emoji("1F31D", "FULL MOON WITH FACE", "\ud83c\udf1d", "full_moon_with_face", new string[1] { "full_moon_with_face" }, null, null, "Travel & Places", 6, 21, SkinVariationType.None, "full moon face", new string[4] { "bright", "face", "full", "moon" });

	public static readonly Emoji SunWithFace = new Emoji("1F31E", "SUN WITH FACE", "\ud83c\udf1e", "sun_with_face", new string[1] { "sun_with_face" }, null, null, "Travel & Places", 6, 22, SkinVariationType.None, "sun with face", new string[4] { "bright", "face", "sun", "sun with face" });

	public static readonly Emoji Star = new Emoji("2B50", "WHITE MEDIUM STAR", "⭐", "star", new string[1] { "star" }, null, null, "Travel & Places", 50, 22, SkinVariationType.None, "star", new string[1] { "star" });

	public static readonly Emoji Star2 = new Emoji("1F31F", "GLOWING STAR", "\ud83c\udf1f", "star2", new string[1] { "star2" }, null, null, "Travel & Places", 6, 23, SkinVariationType.None, "glowing star", new string[6] { "glittery", "glow", "glowing star", "shining", "sparkle", "star" });

	public static readonly Emoji Stars = new Emoji("1F320", "SHOOTING STAR", "\ud83c\udf20", "stars", new string[1] { "stars" }, null, null, "Travel & Places", 6, 24, SkinVariationType.None, "shooting star", new string[3] { "falling", "shooting", "star" });

	public static readonly Emoji Cloud = new Emoji("2601-FE0F", "CLOUD", "☁\ufe0f", "cloud", new string[1] { "cloud" }, null, null, "Travel & Places", 47, 17, SkinVariationType.None, null, null);

	public static readonly Emoji PartlySunny = new Emoji("26C5", "SUN BEHIND CLOUD", "⛅", "partly_sunny", new string[1] { "partly_sunny" }, null, null, "Travel & Places", 48, 29, SkinVariationType.None, "sun behind cloud", new string[3] { "cloud", "sun", "sun behind cloud" });

	public static readonly Emoji ThunderCloudAndRain = new Emoji("26C8-FE0F", null, "⛈\ufe0f", "thunder_cloud_and_rain", new string[1] { "thunder_cloud_and_rain" }, null, null, "Travel & Places", 48, 30, SkinVariationType.None, null, null);

	public static readonly Emoji MostlySunny = new Emoji("1F324-FE0F", null, "\ud83c\udf24\ufe0f", "mostly_sunny", new string[2] { "mostly_sunny", "sun_small_cloud" }, null, null, "Travel & Places", 6, 26, SkinVariationType.None, null, null);

	public static readonly Emoji BarelySunny = new Emoji("1F325-FE0F", null, "\ud83c\udf25\ufe0f", "barely_sunny", new string[2] { "barely_sunny", "sun_behind_cloud" }, null, null, "Travel & Places", 6, 27, SkinVariationType.None, null, null);

	public static readonly Emoji PartlySunnyRain = new Emoji("1F326-FE0F", null, "\ud83c\udf26\ufe0f", "partly_sunny_rain", new string[2] { "partly_sunny_rain", "sun_behind_rain_cloud" }, null, null, "Travel & Places", 6, 28, SkinVariationType.None, null, null);

	public static readonly Emoji RainCloud = new Emoji("1F327-FE0F", null, "\ud83c\udf27\ufe0f", "rain_cloud", new string[1] { "rain_cloud" }, null, null, "Travel & Places", 6, 29, SkinVariationType.None, null, null);

	public static readonly Emoji SnowCloud = new Emoji("1F328-FE0F", null, "\ud83c\udf28\ufe0f", "snow_cloud", new string[1] { "snow_cloud" }, null, null, "Travel & Places", 6, 30, SkinVariationType.None, null, null);

	public static readonly Emoji Lightning = new Emoji("1F329-FE0F", null, "\ud83c\udf29\ufe0f", "lightning", new string[2] { "lightning", "lightning_cloud" }, null, null, "Travel & Places", 6, 31, SkinVariationType.None, null, null);

	public static readonly Emoji Tornado = new Emoji("1F32A-FE0F", null, "\ud83c\udf2a\ufe0f", "tornado", new string[2] { "tornado", "tornado_cloud" }, null, null, "Travel & Places", 6, 32, SkinVariationType.None, null, null);

	public static readonly Emoji Fog = new Emoji("1F32B-FE0F", null, "\ud83c\udf2b\ufe0f", "fog", new string[1] { "fog" }, null, null, "Travel & Places", 6, 33, SkinVariationType.None, null, null);

	public static readonly Emoji WindBlowingFace = new Emoji("1F32C-FE0F", null, "\ud83c\udf2c\ufe0f", "wind_blowing_face", new string[1] { "wind_blowing_face" }, null, null, "Travel & Places", 6, 34, SkinVariationType.None, null, null);

	public static readonly Emoji Cyclone = new Emoji("1F300", "CYCLONE", "\ud83c\udf00", "cyclone", new string[1] { "cyclone" }, null, null, "Travel & Places", 5, 44, SkinVariationType.None, "cyclone", new string[5] { "cyclone", "dizzy", "hurricane", "twister", "typhoon" });

	public static readonly Emoji Rainbow = new Emoji("1F308", "RAINBOW", "\ud83c\udf08", "rainbow", new string[1] { "rainbow" }, null, null, "Travel & Places", 6, 0, SkinVariationType.None, "rainbow", new string[2] { "rain", "rainbow" });

	public static readonly Emoji ClosedUmbrella = new Emoji("1F302", "CLOSED UMBRELLA", "\ud83c\udf02", "closed_umbrella", new string[1] { "closed_umbrella" }, null, null, "Travel & Places", 5, 46, SkinVariationType.None, "closed umbrella", new string[4] { "closed umbrella", "clothing", "rain", "umbrella" });

	public static readonly Emoji Umbrella = new Emoji("2602-FE0F", null, "☂\ufe0f", "umbrella", new string[1] { "umbrella" }, null, null, "Travel & Places", 47, 18, SkinVariationType.None, null, null);

	public static readonly Emoji UmbrellaWithRainDrops = new Emoji("2614", "UMBRELLA WITH RAIN DROPS", "☔", "umbrella_with_rain_drops", new string[1] { "umbrella_with_rain_drops" }, null, null, "Travel & Places", 47, 23, SkinVariationType.None, "umbrella with rain drops", new string[5] { "clothing", "drop", "rain", "umbrella", "umbrella with rain drops" });

	public static readonly Emoji UmbrellaOnGround = new Emoji("26F1-FE0F", null, "⛱\ufe0f", "umbrella_on_ground", new string[1] { "umbrella_on_ground" }, null, null, "Travel & Places", 48, 39, SkinVariationType.None, null, null);

	public static readonly Emoji Zap = new Emoji("26A1", "HIGH VOLTAGE SIGN", "⚡", "zap", new string[1] { "zap" }, null, null, "Travel & Places", 48, 21, SkinVariationType.None, "high voltage", new string[7] { "danger", "electric", "electricity", "high voltage", "lightning", "voltage", "zap" });

	public static readonly Emoji Snowflake = new Emoji("2744-FE0F", "SNOWFLAKE", "❄\ufe0f", "snowflake", new string[1] { "snowflake" }, null, null, "Travel & Places", 49, 51, SkinVariationType.None, null, null);

	public static readonly Emoji Snowman = new Emoji("2603-FE0F", null, "☃\ufe0f", "snowman", new string[1] { "snowman" }, null, null, "Travel & Places", 47, 19, SkinVariationType.None, null, null);

	public static readonly Emoji SnowmanWithoutSnow = new Emoji("26C4", "SNOWMAN WITHOUT SNOW", "⛄", "snowman_without_snow", new string[1] { "snowman_without_snow" }, null, null, "Travel & Places", 48, 28, SkinVariationType.None, "snowman without snow", new string[4] { "cold", "snow", "snowman", "snowman without snow" });

	public static readonly Emoji Comet = new Emoji("2604-FE0F", null, "☄\ufe0f", "comet", new string[1] { "comet" }, null, null, "Travel & Places", 47, 20, SkinVariationType.None, null, null);

	public static readonly Emoji Fire = new Emoji("1F525", "FIRE", "\ud83d\udd25", "fire", new string[1] { "fire" }, null, null, "Travel & Places", 27, 39, SkinVariationType.None, "fire", new string[3] { "fire", "flame", "tool" });

	public static readonly Emoji Droplet = new Emoji("1F4A7", "DROPLET", "\ud83d\udca7", "droplet", new string[1] { "droplet" }, null, null, "Travel & Places", 25, 13, SkinVariationType.None, "droplet", new string[5] { "cold", "comic", "drop", "droplet", "sweat" });

	public static readonly Emoji Ocean = new Emoji("1F30A", "WATER WAVE", "\ud83c\udf0a", "ocean", new string[1] { "ocean" }, null, null, "Travel & Places", 6, 2, SkinVariationType.None, "water wave", new string[3] { "ocean", "water", "wave" });

	public static readonly Emoji JackOLantern = new Emoji("1F383", "JACK-O-LANTERN", "\ud83c\udf83", "jack_o_lantern", new string[1] { "jack_o_lantern" }, null, null, "Activities", 8, 17, SkinVariationType.None, "jack-o-lantern", new string[5] { "celebration", "halloween", "jack", "jack-o-lantern", "lantern" });

	public static readonly Emoji ChristmasTree = new Emoji("1F384", "CHRISTMAS TREE", "\ud83c\udf84", "christmas_tree", new string[1] { "christmas_tree" }, null, null, "Activities", 8, 18, SkinVariationType.None, "Christmas tree", new string[3] { "celebration", "Christmas", "tree" });

	public static readonly Emoji Fireworks = new Emoji("1F386", "FIREWORKS", "\ud83c\udf86", "fireworks", new string[1] { "fireworks" }, null, null, "Activities", 8, 25, SkinVariationType.None, "fireworks", new string[2] { "celebration", "fireworks" });

	public static readonly Emoji Sparkler = new Emoji("1F387", "FIREWORK SPARKLER", "\ud83c\udf87", "sparkler", new string[1] { "sparkler" }, null, null, "Activities", 8, 26, SkinVariationType.None, "sparkler", new string[4] { "celebration", "fireworks", "sparkle", "sparkler" });

	public static readonly Emoji Sparkles = new Emoji("2728", "SPARKLES", "✨", "sparkles", new string[1] { "sparkles" }, null, null, "Activities", 49, 48, SkinVariationType.None, "sparkles", new string[3] { "sparkle", "sparkles", "star" });

	public static readonly Emoji Balloon = new Emoji("1F388", "BALLOON", "\ud83c\udf88", "balloon", new string[1] { "balloon" }, null, null, "Activities", 8, 27, SkinVariationType.None, "balloon", new string[2] { "balloon", "celebration" });

	public static readonly Emoji Tada = new Emoji("1F389", "PARTY POPPER", "\ud83c\udf89", "tada", new string[1] { "tada" }, null, null, "Activities", 8, 28, SkinVariationType.None, "party popper", new string[4] { "celebration", "party", "popper", "tada" });

	public static readonly Emoji ConfettiBall = new Emoji("1F38A", "CONFETTI BALL", "\ud83c\udf8a", "confetti_ball", new string[1] { "confetti_ball" }, null, null, "Activities", 8, 29, SkinVariationType.None, "confetti ball", new string[3] { "ball", "celebration", "confetti" });

	public static readonly Emoji TanabataTree = new Emoji("1F38B", "TANABATA TREE", "\ud83c\udf8b", "tanabata_tree", new string[1] { "tanabata_tree" }, null, null, "Activities", 8, 30, SkinVariationType.None, "tanabata tree", new string[5] { "banner", "celebration", "Japanese", "tanabata tree", "tree" });

	public static readonly Emoji Bamboo = new Emoji("1F38D", "PINE DECORATION", "\ud83c\udf8d", "bamboo", new string[1] { "bamboo" }, null, null, "Activities", 8, 32, SkinVariationType.None, "pine decoration", new string[5] { "bamboo", "celebration", "Japanese", "pine", "pine decoration" });

	public static readonly Emoji Dolls = new Emoji("1F38E", "JAPANESE DOLLS", "\ud83c\udf8e", "dolls", new string[1] { "dolls" }, null, null, "Activities", 8, 33, SkinVariationType.None, "Japanese dolls", new string[5] { "celebration", "doll", "festival", "Japanese", "Japanese dolls" });

	public static readonly Emoji Flags = new Emoji("1F38F", "CARP STREAMER", "\ud83c\udf8f", "flags", new string[1] { "flags" }, null, null, "Activities", 8, 34, SkinVariationType.None, "carp streamer", new string[3] { "carp", "celebration", "streamer" });

	public static readonly Emoji WindChime = new Emoji("1F390", "WIND CHIME", "\ud83c\udf90", "wind_chime", new string[1] { "wind_chime" }, null, null, "Activities", 8, 35, SkinVariationType.None, "wind chime", new string[4] { "bell", "celebration", "chime", "wind" });

	public static readonly Emoji RiceScene = new Emoji("1F391", "MOON VIEWING CEREMONY", "\ud83c\udf91", "rice_scene", new string[1] { "rice_scene" }, null, null, "Activities", 8, 36, SkinVariationType.None, "moon viewing ceremony", new string[4] { "celebration", "ceremony", "moon", "moon viewing ceremony" });

	public static readonly Emoji Ribbon = new Emoji("1F380", "RIBBON", "\ud83c\udf80", "ribbon", new string[1] { "ribbon" }, null, null, "Activities", 8, 14, SkinVariationType.None, "ribbon", new string[2] { "celebration", "ribbon" });

	public static readonly Emoji Gift = new Emoji("1F381", "WRAPPED PRESENT", "\ud83c\udf81", "gift", new string[1] { "gift" }, null, null, "Activities", 8, 15, SkinVariationType.None, "wrapped gift", new string[5] { "box", "celebration", "gift", "present", "wrapped" });

	public static readonly Emoji ReminderRibbon = new Emoji("1F397-FE0F", null, "\ud83c\udf97\ufe0f", "reminder_ribbon", new string[1] { "reminder_ribbon" }, null, null, "Activities", 8, 40, SkinVariationType.None, null, null);

	public static readonly Emoji AdmissionTickets = new Emoji("1F39F-FE0F", null, "\ud83c\udf9f\ufe0f", "admission_tickets", new string[1] { "admission_tickets" }, null, null, "Activities", 8, 45, SkinVariationType.None, null, null);

	public static readonly Emoji Ticket = new Emoji("1F3AB", "TICKET", "\ud83c\udfab", "ticket", new string[1] { "ticket" }, null, null, "Activities", 9, 5, SkinVariationType.None, "ticket", new string[2] { "admission", "ticket" });

	public static readonly Emoji Medal = new Emoji("1F396-FE0F", null, "\ud83c\udf96\ufe0f", "medal", new string[1] { "medal" }, null, null, "Activities", 8, 39, SkinVariationType.None, null, null);

	public static readonly Emoji Trophy = new Emoji("1F3C6", "TROPHY", "\ud83c\udfc6", "trophy", new string[1] { "trophy" }, null, null, "Activities", 10, 19, SkinVariationType.None, "trophy", new string[2] { "prize", "trophy" });

	public static readonly Emoji SportsMedal = new Emoji("1F3C5", "SPORTS MEDAL", "\ud83c\udfc5", "sports_medal", new string[1] { "sports_medal" }, null, null, "Activities", 10, 18, SkinVariationType.None, "sports medal", new string[2] { "medal", "sports medal" });

	public static readonly Emoji FirstPlaceMedal = new Emoji("1F947", "FIRST PLACE MEDAL", "\ud83e\udd47", "first_place_medal", new string[1] { "first_place_medal" }, null, null, "Activities", 41, 42, SkinVariationType.None, "1st place medal", new string[4] { "1st place medal", "first", "gold", "medal" });

	public static readonly Emoji SecondPlaceMedal = new Emoji("1F948", "SECOND PLACE MEDAL", "\ud83e\udd48", "second_place_medal", new string[1] { "second_place_medal" }, null, null, "Activities", 41, 43, SkinVariationType.None, "2nd place medal", new string[4] { "2nd place medal", "medal", "second", "silver" });

	public static readonly Emoji ThirdPlaceMedal = new Emoji("1F949", "THIRD PLACE MEDAL", "\ud83e\udd49", "third_place_medal", new string[1] { "third_place_medal" }, null, null, "Activities", 41, 44, SkinVariationType.None, "3rd place medal", new string[4] { "3rd place medal", "bronze", "medal", "third" });

	public static readonly Emoji Soccer = new Emoji("26BD", "SOCCER BALL", "⚽", "soccer", new string[1] { "soccer" }, null, null, "Activities", 48, 26, SkinVariationType.None, "soccer ball", new string[3] { "ball", "football", "soccer" });

	public static readonly Emoji Baseball = new Emoji("26BE", "BASEBALL", "⚾", "baseball", new string[1] { "baseball" }, null, null, "Activities", 48, 27, SkinVariationType.None, "baseball", new string[2] { "ball", "baseball" });

	public static readonly Emoji Basketball = new Emoji("1F3C0", "BASKETBALL AND HOOP", "\ud83c\udfc0", "basketball", new string[1] { "basketball" }, null, null, "Activities", 9, 26, SkinVariationType.None, "basketball", new string[3] { "ball", "basketball", "hoop" });

	public static readonly Emoji Volleyball = new Emoji("1F3D0", "VOLLEYBALL", "\ud83c\udfd0", "volleyball", new string[1] { "volleyball" }, null, null, "Activities", 11, 33, SkinVariationType.None, "volleyball", new string[3] { "ball", "game", "volleyball" });

	public static readonly Emoji Football = new Emoji("1F3C8", "AMERICAN FOOTBALL", "\ud83c\udfc8", "football", new string[1] { "football" }, null, null, "Activities", 10, 26, SkinVariationType.None, "american football", new string[3] { "american", "ball", "football" });

	public static readonly Emoji RugbyFootball = new Emoji("1F3C9", "RUGBY FOOTBALL", "\ud83c\udfc9", "rugby_football", new string[1] { "rugby_football" }, null, null, "Activities", 10, 27, SkinVariationType.None, "rugby football", new string[3] { "ball", "football", "rugby" });

	public static readonly Emoji Tennis = new Emoji("1F3BE", "TENNIS RACQUET AND BALL", "\ud83c\udfbe", "tennis", new string[1] { "tennis" }, null, null, "Activities", 9, 24, SkinVariationType.None, "tennis", new string[3] { "ball", "racquet", "tennis" });

	public static readonly Emoji Bowling = new Emoji("1F3B3", "BOWLING", "\ud83c\udfb3", "bowling", new string[1] { "bowling" }, null, null, "Activities", 9, 13, SkinVariationType.None, "bowling", new string[3] { "ball", "bowling", "game" });

	public static readonly Emoji CricketBatAndBall = new Emoji("1F3CF", "CRICKET BAT AND BALL", "\ud83c\udfcf", "cricket_bat_and_ball", new string[1] { "cricket_bat_and_ball" }, null, null, "Activities", 11, 32, SkinVariationType.None, "cricket game", new string[4] { "ball", "bat", "cricket game", "game" });

	public static readonly Emoji FieldHockeyStickAndBall = new Emoji("1F3D1", "FIELD HOCKEY STICK AND BALL", "\ud83c\udfd1", "field_hockey_stick_and_ball", new string[1] { "field_hockey_stick_and_ball" }, null, null, "Activities", 11, 34, SkinVariationType.None, "field hockey", new string[5] { "ball", "field", "game", "hockey", "stick" });

	public static readonly Emoji IceHockeyStickAndPuck = new Emoji("1F3D2", "ICE HOCKEY STICK AND PUCK", "\ud83c\udfd2", "ice_hockey_stick_and_puck", new string[1] { "ice_hockey_stick_and_puck" }, null, null, "Activities", 11, 35, SkinVariationType.None, "ice hockey", new string[5] { "game", "hockey", "ice", "puck", "stick" });

	public static readonly Emoji TableTennisPaddleAndBall = new Emoji("1F3D3", "TABLE TENNIS PADDLE AND BALL", "\ud83c\udfd3", "table_tennis_paddle_and_ball", new string[1] { "table_tennis_paddle_and_ball" }, null, null, "Activities", 11, 36, SkinVariationType.None, "ping pong", new string[6] { "ball", "bat", "game", "paddle", "ping pong", "table tennis" });

	public static readonly Emoji BadmintonRacquetAndShuttlecock = new Emoji("1F3F8", "BADMINTON RACQUET AND SHUTTLECOCK", "\ud83c\udff8", "badminton_racquet_and_shuttlecock", new string[1] { "badminton_racquet_and_shuttlecock" }, null, null, "Activities", 12, 22, SkinVariationType.None, "badminton", new string[5] { "badminton", "birdie", "game", "racquet", "shuttlecock" });

	public static readonly Emoji BoxingGlove = new Emoji("1F94A", "BOXING GLOVE", "\ud83e\udd4a", "boxing_glove", new string[1] { "boxing_glove" }, null, null, "Activities", 41, 45, SkinVariationType.None, "boxing glove", new string[2] { "boxing", "glove" });

	public static readonly Emoji MartialArtsUniform = new Emoji("1F94B", "MARTIAL ARTS UNIFORM", "\ud83e\udd4b", "martial_arts_uniform", new string[1] { "martial_arts_uniform" }, null, null, "Activities", 41, 46, SkinVariationType.None, "martial arts uniform", new string[6] { "judo", "karate", "martial arts", "martial arts uniform", "taekwondo", "uniform" });

	public static readonly Emoji GoalNet = new Emoji("1F945", "GOAL NET", "\ud83e\udd45", "goal_net", new string[1] { "goal_net" }, null, null, "Activities", 41, 41, SkinVariationType.None, "goal net", new string[2] { "goal", "net" });

	public static readonly Emoji Golf = new Emoji("26F3", "FLAG IN HOLE", "⛳", "golf", new string[1] { "golf" }, null, null, "Activities", 48, 41, SkinVariationType.None, "flag in hole", new string[3] { "flag in hole", "golf", "hole" });

	public static readonly Emoji IceSkate = new Emoji("26F8-FE0F", null, "⛸\ufe0f", "ice_skate", new string[1] { "ice_skate" }, null, null, "Activities", 48, 45, SkinVariationType.None, null, null);

	public static readonly Emoji FishingPoleAndFish = new Emoji("1F3A3", "FISHING POLE AND FISH", "\ud83c\udfa3", "fishing_pole_and_fish", new string[1] { "fishing_pole_and_fish" }, null, null, "Activities", 8, 49, SkinVariationType.None, "fishing pole", new string[3] { "fish", "fishing pole", "pole" });

	public static readonly Emoji RunningShirtWithSash = new Emoji("1F3BD", "RUNNING SHIRT WITH SASH", "\ud83c\udfbd", "running_shirt_with_sash", new string[1] { "running_shirt_with_sash" }, null, null, "Activities", 9, 23, SkinVariationType.None, "running shirt", new string[4] { "athletics", "running", "sash", "shirt" });

	public static readonly Emoji Ski = new Emoji("1F3BF", "SKI AND SKI BOOT", "\ud83c\udfbf", "ski", new string[1] { "ski" }, null, null, "Activities", 9, 25, SkinVariationType.None, "skis", new string[3] { "ski", "skis", "snow" });

	public static readonly Emoji Sled = new Emoji("1F6F7", "SLED", "\ud83d\udef7", "sled", new string[1] { "sled" }, null, null, "Activities", 37, 22, SkinVariationType.None, "sled", new string[3] { "sled", "sledge", "sleigh" });

	public static readonly Emoji CurlingStone = new Emoji("1F94C", "CURLING STONE", "\ud83e\udd4c", "curling_stone", new string[1] { "curling_stone" }, null, null, "Activities", 41, 47, SkinVariationType.None, "curling stone", new string[3] { "curling stone", "game", "rock" });

	public static readonly Emoji Dart = new Emoji("1F3AF", "DIRECT HIT", "\ud83c\udfaf", "dart", new string[1] { "dart" }, null, null, "Activities", 9, 9, SkinVariationType.None, "direct hit", new string[8] { "bull", "bullseye", "dart", "direct hit", "eye", "game", "hit", "target" });

	public static readonly Emoji EightBall = new Emoji("1F3B1", "BILLIARDS", "\ud83c\udfb1", "8ball", new string[1] { "8ball" }, null, null, "Activities", 9, 11, SkinVariationType.None, "pool 8 ball", new string[6] { "8", "ball", "billiard", "eight", "game", "pool 8 ball" });

	public static readonly Emoji CrystalBall = new Emoji("1F52E", "CRYSTAL BALL", "\ud83d\udd2e", "crystal_ball", new string[1] { "crystal_ball" }, null, null, "Objects", 27, 48, SkinVariationType.None, "crystal ball", new string[6] { "ball", "crystal", "fairy tale", "fantasy", "fortune", "tool" });

	public static readonly Emoji VideoGame = new Emoji("1F3AE", "VIDEO GAME", "\ud83c\udfae", "video_game", new string[1] { "video_game" }, null, null, "Activities", 9, 8, SkinVariationType.None, "video game", new string[3] { "controller", "game", "video game" });

	public static readonly Emoji Joystick = new Emoji("1F579-FE0F", null, "\ud83d\udd79\ufe0f", "joystick", new string[1] { "joystick" }, null, null, "Activities", 29, 20, SkinVariationType.None, null, null);

	public static readonly Emoji SlotMachine = new Emoji("1F3B0", "SLOT MACHINE", "\ud83c\udfb0", "slot_machine", new string[1] { "slot_machine" }, null, null, "Travel & Places", 9, 10, SkinVariationType.None, "slot machine", new string[3] { "game", "slot", "slot machine" });

	public static readonly Emoji GameDie = new Emoji("1F3B2", "GAME DIE", "\ud83c\udfb2", "game_die", new string[1] { "game_die" }, null, null, "Activities", 9, 12, SkinVariationType.None, "game die", new string[3] { "dice", "die", "game" });

	public static readonly Emoji Spades = new Emoji("2660-FE0F", "BLACK SPADE SUIT", "♠\ufe0f", "spades", new string[1] { "spades" }, null, null, "Activities", 48, 4, SkinVariationType.None, null, null);

	public static readonly Emoji Hearts = new Emoji("2665-FE0F", "BLACK HEART SUIT", "♥\ufe0f", "hearts", new string[1] { "hearts" }, null, null, "Activities", 48, 6, SkinVariationType.None, null, null);

	public static readonly Emoji Diamonds = new Emoji("2666-FE0F", "BLACK DIAMOND SUIT", "♦\ufe0f", "diamonds", new string[1] { "diamonds" }, null, null, "Activities", 48, 7, SkinVariationType.None, null, null);

	public static readonly Emoji Clubs = new Emoji("2663-FE0F", "BLACK CLUB SUIT", "♣\ufe0f", "clubs", new string[1] { "clubs" }, null, null, "Activities", 48, 5, SkinVariationType.None, null, null);

	public static readonly Emoji BlackJoker = new Emoji("1F0CF", "PLAYING CARD BLACK JOKER", "\ud83c\udccf", "black_joker", new string[1] { "black_joker" }, null, null, "Activities", 0, 15, SkinVariationType.None, "joker", new string[4] { "card", "game", "joker", "wildcard" });

	public static readonly Emoji Mahjong = new Emoji("1F004", "MAHJONG TILE RED DRAGON", "\ud83c\udc04", "mahjong", new string[1] { "mahjong" }, null, null, "Activities", 0, 14, SkinVariationType.None, "mahjong red dragon", new string[4] { "game", "mahjong", "mahjong red dragon", "red" });

	public static readonly Emoji FlowerPlayingCards = new Emoji("1F3B4", "FLOWER PLAYING CARDS", "\ud83c\udfb4", "flower_playing_cards", new string[1] { "flower_playing_cards" }, null, null, "Activities", 9, 14, SkinVariationType.None, "flower playing cards", new string[6] { "card", "flower", "flower playing cards", "game", "Japanese", "playing" });

	public static readonly Emoji PerformingArts = new Emoji("1F3AD", "PERFORMING ARTS", "\ud83c\udfad", "performing_arts", new string[1] { "performing_arts" }, null, null, "Travel & Places", 9, 7, SkinVariationType.None, "performing arts", new string[6] { "art", "mask", "performing", "performing arts", "theater", "theatre" });

	public static readonly Emoji FrameWithPicture = new Emoji("1F5BC-FE0F", null, "\ud83d\uddbc\ufe0f", "frame_with_picture", new string[1] { "frame_with_picture" }, null, null, "Travel & Places", 30, 3, SkinVariationType.None, null, null);

	public static readonly Emoji Art = new Emoji("1F3A8", "ARTIST PALETTE", "\ud83c\udfa8", "art", new string[1] { "art" }, null, null, "Travel & Places", 9, 2, SkinVariationType.None, "artist palette", new string[5] { "art", "artist palette", "museum", "painting", "palette" });

	public static readonly Emoji Mute = new Emoji("1F507", "SPEAKER WITH CANCELLATION STROKE", "\ud83d\udd07", "mute", new string[1] { "mute" }, null, null, "Objects", 27, 9, SkinVariationType.None, "muted speaker", new string[5] { "mute", "muted speaker", "quiet", "silent", "speaker" });

	public static readonly Emoji Speaker = new Emoji("1F508", "SPEAKER", "\ud83d\udd08", "speaker", new string[1] { "speaker" }, null, null, "Objects", 27, 10, SkinVariationType.None, "speaker low volume", new string[2] { "soft", "speaker low volume" });

	public static readonly Emoji Sound = new Emoji("1F509", "SPEAKER WITH ONE SOUND WAVE", "\ud83d\udd09", "sound", new string[1] { "sound" }, null, null, "Objects", 27, 11, SkinVariationType.None, "speaker medium volume", new string[2] { "medium", "speaker medium volume" });

	public static readonly Emoji LoudSound = new Emoji("1F50A", "SPEAKER WITH THREE SOUND WAVES", "\ud83d\udd0a", "loud_sound", new string[1] { "loud_sound" }, null, null, "Objects", 27, 12, SkinVariationType.None, "speaker high volume", new string[2] { "loud", "speaker high volume" });

	public static readonly Emoji Loudspeaker = new Emoji("1F4E2", "PUBLIC ADDRESS LOUDSPEAKER", "\ud83d\udce2", "loudspeaker", new string[1] { "loudspeaker" }, null, null, "Objects", 26, 25, SkinVariationType.None, "loudspeaker", new string[3] { "loud", "loudspeaker", "public address" });

	public static readonly Emoji Mega = new Emoji("1F4E3", "CHEERING MEGAPHONE", "\ud83d\udce3", "mega", new string[1] { "mega" }, null, null, "Objects", 26, 26, SkinVariationType.None, "megaphone", new string[2] { "cheering", "megaphone" });

	public static readonly Emoji PostalHorn = new Emoji("1F4EF", "POSTAL HORN", "\ud83d\udcef", "postal_horn", new string[1] { "postal_horn" }, null, null, "Objects", 26, 38, SkinVariationType.None, "postal horn", new string[3] { "horn", "post", "postal" });

	public static readonly Emoji Bell = new Emoji("1F514", "BELL", "\ud83d\udd14", "bell", new string[1] { "bell" }, null, null, "Objects", 27, 22, SkinVariationType.None, "bell", new string[1] { "bell" });

	public static readonly Emoji NoBell = new Emoji("1F515", "BELL WITH CANCELLATION STROKE", "\ud83d\udd15", "no_bell", new string[1] { "no_bell" }, null, null, "Objects", 27, 23, SkinVariationType.None, "bell with slash", new string[9] { "bell", "bell with slash", "forbidden", "mute", "no", "not", "prohibited", "quiet", "silent" });

	public static readonly Emoji MusicalScore = new Emoji("1F3BC", "MUSICAL SCORE", "\ud83c\udfbc", "musical_score", new string[1] { "musical_score" }, null, null, "Objects", 9, 22, SkinVariationType.None, "musical score", new string[3] { "music", "musical score", "score" });

	public static readonly Emoji MusicalNote = new Emoji("1F3B5", "MUSICAL NOTE", "\ud83c\udfb5", "musical_note", new string[1] { "musical_note" }, null, null, "Objects", 9, 15, SkinVariationType.None, "musical note", new string[3] { "music", "musical note", "note" });

	public static readonly Emoji Notes = new Emoji("1F3B6", "MULTIPLE MUSICAL NOTES", "\ud83c\udfb6", "notes", new string[1] { "notes" }, null, null, "Objects", 9, 16, SkinVariationType.None, "musical notes", new string[4] { "music", "musical notes", "note", "notes" });

	public static readonly Emoji StudioMicrophone = new Emoji("1F399-FE0F", null, "\ud83c\udf99\ufe0f", "studio_microphone", new string[1] { "studio_microphone" }, null, null, "Objects", 8, 41, SkinVariationType.None, null, null);

	public static readonly Emoji LevelSlider = new Emoji("1F39A-FE0F", null, "\ud83c\udf9a\ufe0f", "level_slider", new string[1] { "level_slider" }, null, null, "Objects", 8, 42, SkinVariationType.None, null, null);

	public static readonly Emoji ControlKnobs = new Emoji("1F39B-FE0F", null, "\ud83c\udf9b\ufe0f", "control_knobs", new string[1] { "control_knobs" }, null, null, "Objects", 8, 43, SkinVariationType.None, null, null);

	public static readonly Emoji Microphone = new Emoji("1F3A4", "MICROPHONE", "\ud83c\udfa4", "microphone", new string[1] { "microphone" }, null, null, "Objects", 8, 50, SkinVariationType.None, "microphone", new string[3] { "karaoke", "mic", "microphone" });

	public static readonly Emoji Headphones = new Emoji("1F3A7", "HEADPHONE", "\ud83c\udfa7", "headphones", new string[1] { "headphones" }, null, null, "Objects", 9, 1, SkinVariationType.None, "headphone", new string[2] { "earbud", "headphone" });

	public static readonly Emoji Radio = new Emoji("1F4FB", "RADIO", "\ud83d\udcfb", "radio", new string[1] { "radio" }, null, null, "Objects", 26, 50, SkinVariationType.None, "radio", new string[2] { "radio", "video" });

	public static readonly Emoji Saxophone = new Emoji("1F3B7", "SAXOPHONE", "\ud83c\udfb7", "saxophone", new string[1] { "saxophone" }, null, null, "Objects", 9, 17, SkinVariationType.None, "saxophone", new string[4] { "instrument", "music", "sax", "saxophone" });

	public static readonly Emoji Guitar = new Emoji("1F3B8", "GUITAR", "\ud83c\udfb8", "guitar", new string[1] { "guitar" }, null, null, "Objects", 9, 18, SkinVariationType.None, "guitar", new string[3] { "guitar", "instrument", "music" });

	public static readonly Emoji MusicalKeyboard = new Emoji("1F3B9", "MUSICAL KEYBOARD", "\ud83c\udfb9", "musical_keyboard", new string[1] { "musical_keyboard" }, null, null, "Objects", 9, 19, SkinVariationType.None, "musical keyboard", new string[5] { "instrument", "keyboard", "music", "musical keyboard", "piano" });

	public static readonly Emoji Trumpet = new Emoji("1F3BA", "TRUMPET", "\ud83c\udfba", "trumpet", new string[1] { "trumpet" }, null, null, "Objects", 9, 20, SkinVariationType.None, "trumpet", new string[3] { "instrument", "music", "trumpet" });

	public static readonly Emoji Violin = new Emoji("1F3BB", "VIOLIN", "\ud83c\udfbb", "violin", new string[1] { "violin" }, null, null, "Objects", 9, 21, SkinVariationType.None, "violin", new string[3] { "instrument", "music", "violin" });

	public static readonly Emoji DrumWithDrumsticks = new Emoji("1F941", "DRUM WITH DRUMSTICKS", "\ud83e\udd41", "drum_with_drumsticks", new string[1] { "drum_with_drumsticks" }, null, null, "Objects", 41, 37, SkinVariationType.None, "drum", new string[3] { "drum", "drumsticks", "music" });

	public static readonly Emoji Iphone = new Emoji("1F4F1", "MOBILE PHONE", "\ud83d\udcf1", "iphone", new string[1] { "iphone" }, null, null, "Objects", 26, 40, SkinVariationType.None, "mobile phone", new string[4] { "cell", "mobile", "phone", "telephone" });

	public static readonly Emoji Calling = new Emoji("1F4F2", "MOBILE PHONE WITH RIGHTWARDS ARROW AT LEFT", "\ud83d\udcf2", "calling", new string[1] { "calling" }, null, null, "Objects", 26, 41, SkinVariationType.None, "mobile phone with arrow", new string[8] { "arrow", "call", "cell", "mobile", "mobile phone with arrow", "phone", "receive", "telephone" });

	public static readonly Emoji Phone = new Emoji("260E-FE0F", "BLACK TELEPHONE", "☎\ufe0f", "phone", new string[2] { "phone", "telephone" }, null, null, "Objects", 47, 21, SkinVariationType.None, null, null);

	public static readonly Emoji TelephoneReceiver = new Emoji("1F4DE", "TELEPHONE RECEIVER", "\ud83d\udcde", "telephone_receiver", new string[1] { "telephone_receiver" }, null, null, "Objects", 26, 21, SkinVariationType.None, "telephone receiver", new string[3] { "phone", "receiver", "telephone" });

	public static readonly Emoji Pager = new Emoji("1F4DF", "PAGER", "\ud83d\udcdf", "pager", new string[1] { "pager" }, null, null, "Objects", 26, 22, SkinVariationType.None, "pager", new string[1] { "pager" });

	public static readonly Emoji Fax = new Emoji("1F4E0", "FAX MACHINE", "\ud83d\udce0", "fax", new string[1] { "fax" }, null, null, "Objects", 26, 23, SkinVariationType.None, "fax machine", new string[2] { "fax", "fax machine" });

	public static readonly Emoji Battery = new Emoji("1F50B", "BATTERY", "\ud83d\udd0b", "battery", new string[1] { "battery" }, null, null, "Objects", 27, 13, SkinVariationType.None, "battery", new string[1] { "battery" });

	public static readonly Emoji ElectricPlug = new Emoji("1F50C", "ELECTRIC PLUG", "\ud83d\udd0c", "electric_plug", new string[1] { "electric_plug" }, null, null, "Objects", 27, 14, SkinVariationType.None, "electric plug", new string[3] { "electric", "electricity", "plug" });

	public static readonly Emoji Computer = new Emoji("1F4BB", "PERSONAL COMPUTER", "\ud83d\udcbb", "computer", new string[1] { "computer" }, null, null, "Objects", 25, 38, SkinVariationType.None, "laptop computer", new string[4] { "computer", "laptop computer", "pc", "personal" });

	public static readonly Emoji DesktopComputer = new Emoji("1F5A5-FE0F", null, "\ud83d\udda5\ufe0f", "desktop_computer", new string[1] { "desktop_computer" }, null, null, "Objects", 29, 51, SkinVariationType.None, null, null);

	public static readonly Emoji Printer = new Emoji("1F5A8-FE0F", null, "\ud83d\udda8\ufe0f", "printer", new string[1] { "printer" }, null, null, "Objects", 30, 0, SkinVariationType.None, null, null);

	public static readonly Emoji Keyboard = new Emoji("2328-FE0F", null, "⌨\ufe0f", "keyboard", new string[1] { "keyboard" }, null, null, "Objects", 46, 43, SkinVariationType.None, null, null);

	public static readonly Emoji ThreeButtonMouse = new Emoji("1F5B1-FE0F", null, "\ud83d\uddb1\ufe0f", "three_button_mouse", new string[1] { "three_button_mouse" }, null, null, "Objects", 30, 1, SkinVariationType.None, null, null);

	public static readonly Emoji Trackball = new Emoji("1F5B2-FE0F", null, "\ud83d\uddb2\ufe0f", "trackball", new string[1] { "trackball" }, null, null, "Objects", 30, 2, SkinVariationType.None, null, null);

	public static readonly Emoji Minidisc = new Emoji("1F4BD", "MINIDISC", "\ud83d\udcbd", "minidisc", new string[1] { "minidisc" }, null, null, "Objects", 25, 40, SkinVariationType.None, "computer disk", new string[4] { "computer", "disk", "minidisk", "optical" });

	public static readonly Emoji FloppyDisk = new Emoji("1F4BE", "FLOPPY DISK", "\ud83d\udcbe", "floppy_disk", new string[1] { "floppy_disk" }, null, null, "Objects", 25, 41, SkinVariationType.None, "floppy disk", new string[3] { "computer", "disk", "floppy" });

	public static readonly Emoji Cd = new Emoji("1F4BF", "OPTICAL DISC", "\ud83d\udcbf", "cd", new string[1] { "cd" }, null, null, "Objects", 25, 42, SkinVariationType.None, "optical disk", new string[4] { "cd", "computer", "disk", "optical" });

	public static readonly Emoji Dvd = new Emoji("1F4C0", "DVD", "\ud83d\udcc0", "dvd", new string[1] { "dvd" }, null, null, "Objects", 25, 43, SkinVariationType.None, "dvd", new string[5] { "blu-ray", "computer", "disk", "dvd", "optical" });

	public static readonly Emoji MovieCamera = new Emoji("1F3A5", "MOVIE CAMERA", "\ud83c\udfa5", "movie_camera", new string[1] { "movie_camera" }, null, null, "Objects", 8, 51, SkinVariationType.None, "movie camera", new string[3] { "camera", "cinema", "movie" });

	public static readonly Emoji FilmFrames = new Emoji("1F39E-FE0F", null, "\ud83c\udf9e\ufe0f", "film_frames", new string[1] { "film_frames" }, null, null, "Objects", 8, 44, SkinVariationType.None, null, null);

	public static readonly Emoji FilmProjector = new Emoji("1F4FD-FE0F", null, "\ud83d\udcfd\ufe0f", "film_projector", new string[1] { "film_projector" }, null, null, "Objects", 27, 0, SkinVariationType.None, null, null);

	public static readonly Emoji Clapper = new Emoji("1F3AC", "CLAPPER BOARD", "\ud83c\udfac", "clapper", new string[1] { "clapper" }, null, null, "Objects", 9, 6, SkinVariationType.None, "clapper board", new string[3] { "clapper", "clapper board", "movie" });

	public static readonly Emoji Tv = new Emoji("1F4FA", "TELEVISION", "\ud83d\udcfa", "tv", new string[1] { "tv" }, null, null, "Objects", 26, 49, SkinVariationType.None, "television", new string[3] { "television", "tv", "video" });

	public static readonly Emoji Camera = new Emoji("1F4F7", "CAMERA", "\ud83d\udcf7", "camera", new string[1] { "camera" }, null, null, "Objects", 26, 46, SkinVariationType.None, "camera", new string[2] { "camera", "video" });

	public static readonly Emoji CameraWithFlash = new Emoji("1F4F8", "CAMERA WITH FLASH", "\ud83d\udcf8", "camera_with_flash", new string[1] { "camera_with_flash" }, null, null, "Objects", 26, 47, SkinVariationType.None, "camera with flash", new string[4] { "camera", "camera with flash", "flash", "video" });

	public static readonly Emoji VideoCamera = new Emoji("1F4F9", "VIDEO CAMERA", "\ud83d\udcf9", "video_camera", new string[1] { "video_camera" }, null, null, "Objects", 26, 48, SkinVariationType.None, "video camera", new string[2] { "camera", "video" });

	public static readonly Emoji Vhs = new Emoji("1F4FC", "VIDEOCASSETTE", "\ud83d\udcfc", "vhs", new string[1] { "vhs" }, null, null, "Objects", 26, 51, SkinVariationType.None, "videocassette", new string[4] { "tape", "vhs", "video", "videocassette" });

	public static readonly Emoji Mag = new Emoji("1F50D", "LEFT-POINTING MAGNIFYING GLASS", "\ud83d\udd0d", "mag", new string[1] { "mag" }, null, null, "Objects", 27, 15, SkinVariationType.None, "magnifying glass tilted left", new string[5] { "glass", "magnifying", "magnifying glass tilted left", "search", "tool" });

	public static readonly Emoji MagRight = new Emoji("1F50E", "RIGHT-POINTING MAGNIFYING GLASS", "\ud83d\udd0e", "mag_right", new string[1] { "mag_right" }, null, null, "Objects", 27, 16, SkinVariationType.None, "magnifying glass tilted right", new string[5] { "glass", "magnifying", "magnifying glass tilted right", "search", "tool" });

	public static readonly Emoji Candle = new Emoji("1F56F-FE0F", null, "\ud83d\udd6f\ufe0f", "candle", new string[1] { "candle" }, null, null, "Objects", 28, 42, SkinVariationType.None, null, null);

	public static readonly Emoji Bulb = new Emoji("1F4A1", "ELECTRIC LIGHT BULB", "\ud83d\udca1", "bulb", new string[1] { "bulb" }, null, null, "Objects", 25, 7, SkinVariationType.None, "light bulb", new string[5] { "bulb", "comic", "electric", "idea", "light" });

	public static readonly Emoji Flashlight = new Emoji("1F526", "ELECTRIC TORCH", "\ud83d\udd26", "flashlight", new string[1] { "flashlight" }, null, null, "Objects", 27, 40, SkinVariationType.None, "flashlight", new string[5] { "electric", "flashlight", "light", "tool", "torch" });

	public static readonly Emoji IzakayaLantern = new Emoji("1F3EE", "IZAKAYA LANTERN", "\ud83c\udfee", "izakaya_lantern", new string[2] { "izakaya_lantern", "lantern" }, null, null, "Objects", 12, 11, SkinVariationType.None, "red paper lantern", new string[5] { "bar", "lantern", "light", "red", "red paper lantern" });

	public static readonly Emoji NotebookWithDecorativeCover = new Emoji("1F4D4", "NOTEBOOK WITH DECORATIVE COVER", "\ud83d\udcd4", "notebook_with_decorative_cover", new string[1] { "notebook_with_decorative_cover" }, null, null, "Objects", 26, 11, SkinVariationType.None, "notebook with decorative cover", new string[5] { "book", "cover", "decorated", "notebook", "notebook with decorative cover" });

	public static readonly Emoji ClosedBook = new Emoji("1F4D5", "CLOSED BOOK", "\ud83d\udcd5", "closed_book", new string[1] { "closed_book" }, null, null, "Objects", 26, 12, SkinVariationType.None, "closed book", new string[2] { "book", "closed" });

	public static readonly Emoji Book = new Emoji("1F4D6", "OPEN BOOK", "\ud83d\udcd6", "book", new string[2] { "book", "open_book" }, null, null, "Objects", 26, 13, SkinVariationType.None, "open book", new string[2] { "book", "open" });

	public static readonly Emoji GreenBook = new Emoji("1F4D7", "GREEN BOOK", "\ud83d\udcd7", "green_book", new string[1] { "green_book" }, null, null, "Objects", 26, 14, SkinVariationType.None, "green book", new string[2] { "book", "green" });

	public static readonly Emoji BlueBook = new Emoji("1F4D8", "BLUE BOOK", "\ud83d\udcd8", "blue_book", new string[1] { "blue_book" }, null, null, "Objects", 26, 15, SkinVariationType.None, "blue book", new string[2] { "blue", "book" });

	public static readonly Emoji OrangeBook = new Emoji("1F4D9", "ORANGE BOOK", "\ud83d\udcd9", "orange_book", new string[1] { "orange_book" }, null, null, "Objects", 26, 16, SkinVariationType.None, "orange book", new string[2] { "book", "orange" });

	public static readonly Emoji Books = new Emoji("1F4DA", "BOOKS", "\ud83d\udcda", "books", new string[1] { "books" }, null, null, "Objects", 26, 17, SkinVariationType.None, "books", new string[2] { "book", "books" });

	public static readonly Emoji Notebook = new Emoji("1F4D3", "NOTEBOOK", "\ud83d\udcd3", "notebook", new string[1] { "notebook" }, null, null, "Objects", 26, 10, SkinVariationType.None, "notebook", new string[1] { "notebook" });

	public static readonly Emoji Ledger = new Emoji("1F4D2", "LEDGER", "\ud83d\udcd2", "ledger", new string[1] { "ledger" }, null, null, "Objects", 26, 9, SkinVariationType.None, "ledger", new string[2] { "ledger", "notebook" });

	public static readonly Emoji PageWithCurl = new Emoji("1F4C3", "PAGE WITH CURL", "\ud83d\udcc3", "page_with_curl", new string[1] { "page_with_curl" }, null, null, "Objects", 25, 46, SkinVariationType.None, "page with curl", new string[4] { "curl", "document", "page", "page with curl" });

	public static readonly Emoji Scroll = new Emoji("1F4DC", "SCROLL", "\ud83d\udcdc", "scroll", new string[1] { "scroll" }, null, null, "Objects", 26, 19, SkinVariationType.None, "scroll", new string[2] { "paper", "scroll" });

	public static readonly Emoji PageFacingUp = new Emoji("1F4C4", "PAGE FACING UP", "\ud83d\udcc4", "page_facing_up", new string[1] { "page_facing_up" }, null, null, "Objects", 25, 47, SkinVariationType.None, "page facing up", new string[3] { "document", "page", "page facing up" });

	public static readonly Emoji Newspaper = new Emoji("1F4F0", "NEWSPAPER", "\ud83d\udcf0", "newspaper", new string[1] { "newspaper" }, null, null, "Objects", 26, 39, SkinVariationType.None, "newspaper", new string[3] { "news", "newspaper", "paper" });

	public static readonly Emoji RolledUpNewspaper = new Emoji("1F5DE-FE0F", null, "\ud83d\uddde\ufe0f", "rolled_up_newspaper", new string[1] { "rolled_up_newspaper" }, null, null, "Objects", 30, 12, SkinVariationType.None, null, null);

	public static readonly Emoji BookmarkTabs = new Emoji("1F4D1", "BOOKMARK TABS", "\ud83d\udcd1", "bookmark_tabs", new string[1] { "bookmark_tabs" }, null, null, "Objects", 26, 8, SkinVariationType.None, "bookmark tabs", new string[4] { "bookmark", "mark", "marker", "tabs" });

	public static readonly Emoji Bookmark = new Emoji("1F516", "BOOKMARK", "\ud83d\udd16", "bookmark", new string[1] { "bookmark" }, null, null, "Objects", 27, 24, SkinVariationType.None, "bookmark", new string[2] { "bookmark", "mark" });

	public static readonly Emoji Label = new Emoji("1F3F7-FE0F", null, "\ud83c\udff7\ufe0f", "label", new string[1] { "label" }, null, null, "Objects", 12, 21, SkinVariationType.None, null, null);

	public static readonly Emoji Moneybag = new Emoji("1F4B0", "MONEY BAG", "\ud83d\udcb0", "moneybag", new string[1] { "moneybag" }, null, null, "Objects", 25, 27, SkinVariationType.None, "money bag", new string[4] { "bag", "dollar", "money", "moneybag" });

	public static readonly Emoji Yen = new Emoji("1F4B4", "BANKNOTE WITH YEN SIGN", "\ud83d\udcb4", "yen", new string[1] { "yen" }, null, null, "Objects", 25, 31, SkinVariationType.None, "yen banknote", new string[7] { "bank", "banknote", "bill", "currency", "money", "note", "yen" });

	public static readonly Emoji Dollar = new Emoji("1F4B5", "BANKNOTE WITH DOLLAR SIGN", "\ud83d\udcb5", "dollar", new string[1] { "dollar" }, null, null, "Objects", 25, 32, SkinVariationType.None, "dollar banknote", new string[7] { "bank", "banknote", "bill", "currency", "dollar", "money", "note" });

	public static readonly Emoji Euro = new Emoji("1F4B6", "BANKNOTE WITH EURO SIGN", "\ud83d\udcb6", "euro", new string[1] { "euro" }, null, null, "Objects", 25, 33, SkinVariationType.None, "euro banknote", new string[7] { "bank", "banknote", "bill", "currency", "euro", "money", "note" });

	public static readonly Emoji Pound = new Emoji("1F4B7", "BANKNOTE WITH POUND SIGN", "\ud83d\udcb7", "pound", new string[1] { "pound" }, null, null, "Objects", 25, 34, SkinVariationType.None, "pound banknote", new string[7] { "bank", "banknote", "bill", "currency", "money", "note", "pound" });

	public static readonly Emoji MoneyWithWings = new Emoji("1F4B8", "MONEY WITH WINGS", "\ud83d\udcb8", "money_with_wings", new string[1] { "money_with_wings" }, null, null, "Objects", 25, 35, SkinVariationType.None, "money with wings", new string[9] { "bank", "banknote", "bill", "dollar", "fly", "money", "money with wings", "note", "wings" });

	public static readonly Emoji CreditCard = new Emoji("1F4B3", "CREDIT CARD", "\ud83d\udcb3", "credit_card", new string[1] { "credit_card" }, null, null, "Objects", 25, 30, SkinVariationType.None, "credit card", new string[4] { "bank", "card", "credit", "money" });

	public static readonly Emoji Chart = new Emoji("1F4B9", "CHART WITH UPWARDS TREND AND YEN SIGN", "\ud83d\udcb9", "chart", new string[1] { "chart" }, null, null, "Objects", 25, 36, SkinVariationType.None, "chart increasing with yen", new string[12]
	{
		"bank", "chart", "chart increasing with yen", "currency", "graph", "growth", "market", "money", "rise", "trend",
		"upward", "yen"
	});

	public static readonly Emoji CurrencyExchange = new Emoji("1F4B1", "CURRENCY EXCHANGE", "\ud83d\udcb1", "currency_exchange", new string[1] { "currency_exchange" }, null, null, "Objects", 25, 28, SkinVariationType.None, "currency exchange", new string[4] { "bank", "currency", "exchange", "money" });

	public static readonly Emoji HeavyDollarSign = new Emoji("1F4B2", "HEAVY DOLLAR SIGN", "\ud83d\udcb2", "heavy_dollar_sign", new string[1] { "heavy_dollar_sign" }, null, null, "Objects", 25, 29, SkinVariationType.None, "heavy dollar sign", new string[4] { "currency", "dollar", "heavy dollar sign", "money" });

	public static readonly Emoji Email = new Emoji("2709-FE0F", "ENVELOPE", "✉\ufe0f", "email", new string[2] { "email", "envelope" }, null, null, "Objects", 49, 17, SkinVariationType.None, null, null);

	public static readonly Emoji EMail = new Emoji("1F4E7", "E-MAIL SYMBOL", "\ud83d\udce7", "e-mail", new string[1] { "e-mail" }, null, null, "Objects", 26, 30, SkinVariationType.None, "e-mail", new string[4] { "e-mail", "email", "letter", "mail" });

	public static readonly Emoji IncomingEnvelope = new Emoji("1F4E8", "INCOMING ENVELOPE", "\ud83d\udce8", "incoming_envelope", new string[1] { "incoming_envelope" }, null, null, "Objects", 26, 31, SkinVariationType.None, "incoming envelope", new string[7] { "e-mail", "email", "envelope", "incoming", "letter", "mail", "receive" });

	public static readonly Emoji EnvelopeWithArrow = new Emoji("1F4E9", "ENVELOPE WITH DOWNWARDS ARROW ABOVE", "\ud83d\udce9", "envelope_with_arrow", new string[1] { "envelope_with_arrow" }, null, null, "Objects", 26, 32, SkinVariationType.None, "envelope with arrow", new string[10] { "arrow", "down", "e-mail", "email", "envelope", "envelope with arrow", "letter", "mail", "outgoing", "sent" });

	public static readonly Emoji OutboxTray = new Emoji("1F4E4", "OUTBOX TRAY", "\ud83d\udce4", "outbox_tray", new string[1] { "outbox_tray" }, null, null, "Objects", 26, 27, SkinVariationType.None, "outbox tray", new string[6] { "box", "letter", "mail", "outbox", "sent", "tray" });

	public static readonly Emoji InboxTray = new Emoji("1F4E5", "INBOX TRAY", "\ud83d\udce5", "inbox_tray", new string[1] { "inbox_tray" }, null, null, "Objects", 26, 28, SkinVariationType.None, "inbox tray", new string[6] { "box", "inbox", "letter", "mail", "receive", "tray" });

	public static readonly Emoji Package = new Emoji("1F4E6", "PACKAGE", "\ud83d\udce6", "package", new string[1] { "package" }, null, null, "Objects", 26, 29, SkinVariationType.None, "package", new string[3] { "box", "package", "parcel" });

	public static readonly Emoji Mailbox = new Emoji("1F4EB", "CLOSED MAILBOX WITH RAISED FLAG", "\ud83d\udceb", "mailbox", new string[1] { "mailbox" }, null, null, "Objects", 26, 34, SkinVariationType.None, "closed mailbox with raised flag", new string[5] { "closed", "closed mailbox with raised flag", "mail", "mailbox", "postbox" });

	public static readonly Emoji MailboxClosed = new Emoji("1F4EA", "CLOSED MAILBOX WITH LOWERED FLAG", "\ud83d\udcea", "mailbox_closed", new string[1] { "mailbox_closed" }, null, null, "Objects", 26, 33, SkinVariationType.None, "closed mailbox with lowered flag", new string[6] { "closed", "closed mailbox with lowered flag", "lowered", "mail", "mailbox", "postbox" });

	public static readonly Emoji MailboxWithMail = new Emoji("1F4EC", "OPEN MAILBOX WITH RAISED FLAG", "\ud83d\udcec", "mailbox_with_mail", new string[1] { "mailbox_with_mail" }, null, null, "Objects", 26, 35, SkinVariationType.None, "open mailbox with raised flag", new string[5] { "mail", "mailbox", "open", "open mailbox with raised flag", "postbox" });

	public static readonly Emoji MailboxWithNoMail = new Emoji("1F4ED", "OPEN MAILBOX WITH LOWERED FLAG", "\ud83d\udced", "mailbox_with_no_mail", new string[1] { "mailbox_with_no_mail" }, null, null, "Objects", 26, 36, SkinVariationType.None, "open mailbox with lowered flag", new string[6] { "lowered", "mail", "mailbox", "open", "open mailbox with lowered flag", "postbox" });

	public static readonly Emoji Postbox = new Emoji("1F4EE", "POSTBOX", "\ud83d\udcee", "postbox", new string[1] { "postbox" }, null, null, "Objects", 26, 37, SkinVariationType.None, "postbox", new string[3] { "mail", "mailbox", "postbox" });

	public static readonly Emoji BallotBoxWithBallot = new Emoji("1F5F3-FE0F", null, "\ud83d\uddf3\ufe0f", "ballot_box_with_ballot", new string[1] { "ballot_box_with_ballot" }, null, null, "Objects", 30, 17, SkinVariationType.None, null, null);

	public static readonly Emoji Pencil2 = new Emoji("270F-FE0F", "PENCIL", "✏\ufe0f", "pencil2", new string[1] { "pencil2" }, null, null, "Objects", 49, 42, SkinVariationType.None, null, null);

	public static readonly Emoji BlackNib = new Emoji("2712-FE0F", "BLACK NIB", "✒\ufe0f", "black_nib", new string[1] { "black_nib" }, null, null, "Objects", 49, 43, SkinVariationType.None, null, null);

	public static readonly Emoji LowerLeftFountainPen = new Emoji("1F58B-FE0F", null, "\ud83d\udd8b\ufe0f", "lower_left_fountain_pen", new string[1] { "lower_left_fountain_pen" }, null, null, "Objects", 29, 29, SkinVariationType.None, null, null);

	public static readonly Emoji LowerLeftBallpointPen = new Emoji("1F58A-FE0F", null, "\ud83d\udd8a\ufe0f", "lower_left_ballpoint_pen", new string[1] { "lower_left_ballpoint_pen" }, null, null, "Objects", 29, 28, SkinVariationType.None, null, null);

	public static readonly Emoji LowerLeftPaintbrush = new Emoji("1F58C-FE0F", null, "\ud83d\udd8c\ufe0f", "lower_left_paintbrush", new string[1] { "lower_left_paintbrush" }, null, null, "Objects", 29, 30, SkinVariationType.None, null, null);

	public static readonly Emoji LowerLeftCrayon = new Emoji("1F58D-FE0F", null, "\ud83d\udd8d\ufe0f", "lower_left_crayon", new string[1] { "lower_left_crayon" }, null, null, "Objects", 29, 31, SkinVariationType.None, null, null);

	public static readonly Emoji Memo = new Emoji("1F4DD", "MEMO", "\ud83d\udcdd", "memo", new string[2] { "memo", "pencil" }, null, null, "Objects", 26, 20, SkinVariationType.None, "memo", new string[2] { "memo", "pencil" });

	public static readonly Emoji Briefcase = new Emoji("1F4BC", "BRIEFCASE", "\ud83d\udcbc", "briefcase", new string[1] { "briefcase" }, null, null, "Objects", 25, 39, SkinVariationType.None, "briefcase", new string[1] { "briefcase" });

	public static readonly Emoji FileFolder = new Emoji("1F4C1", "FILE FOLDER", "\ud83d\udcc1", "file_folder", new string[1] { "file_folder" }, null, null, "Objects", 25, 44, SkinVariationType.None, "file folder", new string[2] { "file", "folder" });

	public static readonly Emoji OpenFileFolder = new Emoji("1F4C2", "OPEN FILE FOLDER", "\ud83d\udcc2", "open_file_folder", new string[1] { "open_file_folder" }, null, null, "Objects", 25, 45, SkinVariationType.None, "open file folder", new string[3] { "file", "folder", "open" });

	public static readonly Emoji CardIndexDividers = new Emoji("1F5C2-FE0F", null, "\ud83d\uddc2\ufe0f", "card_index_dividers", new string[1] { "card_index_dividers" }, null, null, "Objects", 30, 4, SkinVariationType.None, null, null);

	public static readonly Emoji Date = new Emoji("1F4C5", "CALENDAR", "\ud83d\udcc5", "date", new string[1] { "date" }, null, null, "Objects", 25, 48, SkinVariationType.None, "calendar", new string[2] { "calendar", "date" });

	public static readonly Emoji Calendar = new Emoji("1F4C6", "TEAR-OFF CALENDAR", "\ud83d\udcc6", "calendar", new string[1] { "calendar" }, null, null, "Objects", 25, 49, SkinVariationType.None, "tear-off calendar", new string[2] { "calendar", "tear-off calendar" });

	public static readonly Emoji SpiralNotePad = new Emoji("1F5D2-FE0F", null, "\ud83d\uddd2\ufe0f", "spiral_note_pad", new string[1] { "spiral_note_pad" }, null, null, "Objects", 30, 8, SkinVariationType.None, null, null);

	public static readonly Emoji SpiralCalendarPad = new Emoji("1F5D3-FE0F", null, "\ud83d\uddd3\ufe0f", "spiral_calendar_pad", new string[1] { "spiral_calendar_pad" }, null, null, "Objects", 30, 9, SkinVariationType.None, null, null);

	public static readonly Emoji CardIndex = new Emoji("1F4C7", "CARD INDEX", "\ud83d\udcc7", "card_index", new string[1] { "card_index" }, null, null, "Objects", 25, 50, SkinVariationType.None, "card index", new string[3] { "card", "index", "rolodex" });

	public static readonly Emoji ChartWithUpwardsTrend = new Emoji("1F4C8", "CHART WITH UPWARDS TREND", "\ud83d\udcc8", "chart_with_upwards_trend", new string[1] { "chart_with_upwards_trend" }, null, null, "Objects", 25, 51, SkinVariationType.None, "chart increasing", new string[6] { "chart", "chart increasing", "graph", "growth", "trend", "upward" });

	public static readonly Emoji ChartWithDownwardsTrend = new Emoji("1F4C9", "CHART WITH DOWNWARDS TREND", "\ud83d\udcc9", "chart_with_downwards_trend", new string[1] { "chart_with_downwards_trend" }, null, null, "Objects", 26, 0, SkinVariationType.None, "chart decreasing", new string[5] { "chart", "chart decreasing", "down", "graph", "trend" });

	public static readonly Emoji BarChart = new Emoji("1F4CA", "BAR CHART", "\ud83d\udcca", "bar_chart", new string[1] { "bar_chart" }, null, null, "Objects", 26, 1, SkinVariationType.None, "bar chart", new string[3] { "bar", "chart", "graph" });

	public static readonly Emoji Clipboard = new Emoji("1F4CB", "CLIPBOARD", "\ud83d\udccb", "clipboard", new string[1] { "clipboard" }, null, null, "Objects", 26, 2, SkinVariationType.None, "clipboard", new string[1] { "clipboard" });

	public static readonly Emoji Pushpin = new Emoji("1F4CC", "PUSHPIN", "\ud83d\udccc", "pushpin", new string[1] { "pushpin" }, null, null, "Objects", 26, 3, SkinVariationType.None, "pushpin", new string[2] { "pin", "pushpin" });

	public static readonly Emoji RoundPushpin = new Emoji("1F4CD", "ROUND PUSHPIN", "\ud83d\udccd", "round_pushpin", new string[1] { "round_pushpin" }, null, null, "Objects", 26, 4, SkinVariationType.None, "round pushpin", new string[3] { "pin", "pushpin", "round pushpin" });

	public static readonly Emoji Paperclip = new Emoji("1F4CE", "PAPERCLIP", "\ud83d\udcce", "paperclip", new string[1] { "paperclip" }, null, null, "Objects", 26, 5, SkinVariationType.None, "paperclip", new string[1] { "paperclip" });

	public static readonly Emoji LinkedPaperclips = new Emoji("1F587-FE0F", null, "\ud83d\udd87\ufe0f", "linked_paperclips", new string[1] { "linked_paperclips" }, null, null, "Objects", 29, 27, SkinVariationType.None, null, null);

	public static readonly Emoji StraightRuler = new Emoji("1F4CF", "STRAIGHT RULER", "\ud83d\udccf", "straight_ruler", new string[1] { "straight_ruler" }, null, null, "Objects", 26, 6, SkinVariationType.None, "straight ruler", new string[3] { "ruler", "straight edge", "straight ruler" });

	public static readonly Emoji TriangularRuler = new Emoji("1F4D0", "TRIANGULAR RULER", "\ud83d\udcd0", "triangular_ruler", new string[1] { "triangular_ruler" }, null, null, "Objects", 26, 7, SkinVariationType.None, "triangular ruler", new string[4] { "ruler", "set", "triangle", "triangular ruler" });

	public static readonly Emoji Scissors = new Emoji("2702-FE0F", "BLACK SCISSORS", "✂\ufe0f", "scissors", new string[1] { "scissors" }, null, null, "Objects", 49, 14, SkinVariationType.None, null, null);

	public static readonly Emoji CardFileBox = new Emoji("1F5C3-FE0F", null, "\ud83d\uddc3\ufe0f", "card_file_box", new string[1] { "card_file_box" }, null, null, "Objects", 30, 5, SkinVariationType.None, null, null);

	public static readonly Emoji FileCabinet = new Emoji("1F5C4-FE0F", null, "\ud83d\uddc4\ufe0f", "file_cabinet", new string[1] { "file_cabinet" }, null, null, "Objects", 30, 6, SkinVariationType.None, null, null);

	public static readonly Emoji Wastebasket = new Emoji("1F5D1-FE0F", null, "\ud83d\uddd1\ufe0f", "wastebasket", new string[1] { "wastebasket" }, null, null, "Objects", 30, 7, SkinVariationType.None, null, null);

	public static readonly Emoji Lock = new Emoji("1F512", "LOCK", "\ud83d\udd12", "lock", new string[1] { "lock" }, null, null, "Objects", 27, 20, SkinVariationType.None, "locked", new string[2] { "closed", "locked" });

	public static readonly Emoji Unlock = new Emoji("1F513", "OPEN LOCK", "\ud83d\udd13", "unlock", new string[1] { "unlock" }, null, null, "Objects", 27, 21, SkinVariationType.None, "unlocked", new string[4] { "lock", "open", "unlock", "unlocked" });

	public static readonly Emoji LockWithInkPen = new Emoji("1F50F", "LOCK WITH INK PEN", "\ud83d\udd0f", "lock_with_ink_pen", new string[1] { "lock_with_ink_pen" }, null, null, "Objects", 27, 17, SkinVariationType.None, "locked with pen", new string[6] { "ink", "lock", "locked with pen", "nib", "pen", "privacy" });

	public static readonly Emoji ClosedLockWithKey = new Emoji("1F510", "CLOSED LOCK WITH KEY", "\ud83d\udd10", "closed_lock_with_key", new string[1] { "closed_lock_with_key" }, null, null, "Objects", 27, 18, SkinVariationType.None, "locked with key", new string[5] { "closed", "key", "lock", "locked with key", "secure" });

	public static readonly Emoji Key = new Emoji("1F511", "KEY", "\ud83d\udd11", "key", new string[1] { "key" }, null, null, "Objects", 27, 19, SkinVariationType.None, "key", new string[3] { "key", "lock", "password" });

	public static readonly Emoji OldKey = new Emoji("1F5DD-FE0F", null, "\ud83d\udddd\ufe0f", "old_key", new string[1] { "old_key" }, null, null, "Objects", 30, 11, SkinVariationType.None, null, null);

	public static readonly Emoji Hammer = new Emoji("1F528", "HAMMER", "\ud83d\udd28", "hammer", new string[1] { "hammer" }, null, null, "Objects", 27, 42, SkinVariationType.None, "hammer", new string[2] { "hammer", "tool" });

	public static readonly Emoji Pick = new Emoji("26CF-FE0F", null, "⛏\ufe0f", "pick", new string[1] { "pick" }, null, null, "Objects", 48, 32, SkinVariationType.None, null, null);

	public static readonly Emoji HammerAndPick = new Emoji("2692-FE0F", null, "⚒\ufe0f", "hammer_and_pick", new string[1] { "hammer_and_pick" }, null, null, "Objects", 48, 11, SkinVariationType.None, null, null);

	public static readonly Emoji HammerAndWrench = new Emoji("1F6E0-FE0F", null, "\ud83d\udee0\ufe0f", "hammer_and_wrench", new string[1] { "hammer_and_wrench" }, null, null, "Objects", 37, 8, SkinVariationType.None, null, null);

	public static readonly Emoji DaggerKnife = new Emoji("1F5E1-FE0F", null, "\ud83d\udde1\ufe0f", "dagger_knife", new string[1] { "dagger_knife" }, null, null, "Objects", 30, 13, SkinVariationType.None, null, null);

	public static readonly Emoji CrossedSwords = new Emoji("2694-FE0F", null, "⚔\ufe0f", "crossed_swords", new string[1] { "crossed_swords" }, null, null, "Objects", 48, 13, SkinVariationType.None, null, null);

	public static readonly Emoji Gun = new Emoji("1F52B", "PISTOL", "\ud83d\udd2b", "gun", new string[1] { "gun" }, null, null, "Objects", 27, 45, SkinVariationType.None, "pistol", new string[6] { "gun", "handgun", "pistol", "revolver", "tool", "weapon" });

	public static readonly Emoji BowAndArrow = new Emoji("1F3F9", "BOW AND ARROW", "\ud83c\udff9", "bow_and_arrow", new string[1] { "bow_and_arrow" }, null, null, "Objects", 12, 23, SkinVariationType.None, "bow and arrow", new string[9] { "archer", "archery", "arrow", "bow", "bow and arrow", "Sagittarius", "tool", "weapon", "zodiac" });

	public static readonly Emoji Shield = new Emoji("1F6E1-FE0F", null, "\ud83d\udee1\ufe0f", "shield", new string[1] { "shield" }, null, null, "Objects", 37, 9, SkinVariationType.None, null, null);

	public static readonly Emoji Wrench = new Emoji("1F527", "WRENCH", "\ud83d\udd27", "wrench", new string[1] { "wrench" }, null, null, "Objects", 27, 41, SkinVariationType.None, "wrench", new string[3] { "spanner", "tool", "wrench" });

	public static readonly Emoji NutAndBolt = new Emoji("1F529", "NUT AND BOLT", "\ud83d\udd29", "nut_and_bolt", new string[1] { "nut_and_bolt" }, null, null, "Objects", 27, 43, SkinVariationType.None, "nut and bolt", new string[4] { "bolt", "nut", "nut and bolt", "tool" });

	public static readonly Emoji Gear = new Emoji("2699-FE0F", null, "⚙\ufe0f", "gear", new string[1] { "gear" }, null, null, "Objects", 48, 17, SkinVariationType.None, null, null);

	public static readonly Emoji Compression = new Emoji("1F5DC-FE0F", null, "\ud83d\udddc\ufe0f", "compression", new string[1] { "compression" }, null, null, "Objects", 30, 10, SkinVariationType.None, null, null);

	public static readonly Emoji Scales = new Emoji("2696-FE0F", null, "⚖\ufe0f", "scales", new string[1] { "scales" }, null, null, "Objects", 48, 15, SkinVariationType.None, null, null);

	public static readonly Emoji Link = new Emoji("1F517", "LINK SYMBOL", "\ud83d\udd17", "link", new string[1] { "link" }, null, null, "Objects", 27, 25, SkinVariationType.None, "link", new string[1] { "link" });

	public static readonly Emoji Chains = new Emoji("26D3-FE0F", null, "⛓\ufe0f", "chains", new string[1] { "chains" }, null, null, "Objects", 48, 34, SkinVariationType.None, null, null);

	public static readonly Emoji Alembic = new Emoji("2697-FE0F", null, "⚗\ufe0f", "alembic", new string[1] { "alembic" }, null, null, "Objects", 48, 16, SkinVariationType.None, null, null);

	public static readonly Emoji Microscope = new Emoji("1F52C", "MICROSCOPE", "\ud83d\udd2c", "microscope", new string[1] { "microscope" }, null, null, "Objects", 27, 46, SkinVariationType.None, "microscope", new string[3] { "microscope", "science", "tool" });

	public static readonly Emoji Telescope = new Emoji("1F52D", "TELESCOPE", "\ud83d\udd2d", "telescope", new string[1] { "telescope" }, null, null, "Objects", 27, 47, SkinVariationType.None, "telescope", new string[3] { "science", "telescope", "tool" });

	public static readonly Emoji SatelliteAntenna = new Emoji("1F4E1", "SATELLITE ANTENNA", "\ud83d\udce1", "satellite_antenna", new string[1] { "satellite_antenna" }, null, null, "Objects", 26, 24, SkinVariationType.None, "satellite antenna", new string[3] { "antenna", "dish", "satellite" });

	public static readonly Emoji Syringe = new Emoji("1F489", "SYRINGE", "\ud83d\udc89", "syringe", new string[1] { "syringe" }, null, null, "Objects", 24, 35, SkinVariationType.None, "syringe", new string[7] { "doctor", "medicine", "needle", "shot", "sick", "syringe", "tool" });

	public static readonly Emoji Pill = new Emoji("1F48A", "PILL", "\ud83d\udc8a", "pill", new string[1] { "pill" }, null, null, "Objects", 24, 36, SkinVariationType.None, "pill", new string[4] { "doctor", "medicine", "pill", "sick" });

	public static readonly Emoji Door = new Emoji("1F6AA", "DOOR", "\ud83d\udeaa", "door", new string[1] { "door" }, null, null, "Travel & Places", 35, 15, SkinVariationType.None, "door", new string[1] { "door" });

	public static readonly Emoji Bed = new Emoji("1F6CF-FE0F", null, "\ud83d\udecf\ufe0f", "bed", new string[1] { "bed" }, null, null, "Travel & Places", 37, 4, SkinVariationType.None, null, null);

	public static readonly Emoji CouchAndLamp = new Emoji("1F6CB-FE0F", null, "\ud83d\udecb\ufe0f", "couch_and_lamp", new string[1] { "couch_and_lamp" }, null, null, "Travel & Places", 36, 47, SkinVariationType.None, null, null);

	public static readonly Emoji Toilet = new Emoji("1F6BD", "TOILET", "\ud83d\udebd", "toilet", new string[1] { "toilet" }, null, null, "Travel & Places", 36, 33, SkinVariationType.None, "toilet", new string[1] { "toilet" });

	public static readonly Emoji Shower = new Emoji("1F6BF", "SHOWER", "\ud83d\udebf", "shower", new string[1] { "shower" }, null, null, "Travel & Places", 36, 35, SkinVariationType.None, "shower", new string[2] { "shower", "water" });

	public static readonly Emoji Bathtub = new Emoji("1F6C1", "BATHTUB", "\ud83d\udec1", "bathtub", new string[1] { "bathtub" }, null, null, "Travel & Places", 36, 42, SkinVariationType.None, "bathtub", new string[2] { "bath", "bathtub" });

	public static readonly Emoji ShoppingTrolley = new Emoji("1F6D2", "SHOPPING TROLLEY", "\ud83d\uded2", "shopping_trolley", new string[1] { "shopping_trolley" }, null, null, "Objects", 37, 7, SkinVariationType.None, "shopping cart", new string[3] { "cart", "shopping", "trolley" });

	public static readonly Emoji Smoking = new Emoji("1F6AC", "SMOKING SYMBOL", "\ud83d\udeac", "smoking", new string[1] { "smoking" }, null, null, "Objects", 35, 17, SkinVariationType.None, "cigarette", new string[2] { "cigarette", "smoking" });

	public static readonly Emoji Coffin = new Emoji("26B0-FE0F", null, "⚰\ufe0f", "coffin", new string[1] { "coffin" }, null, null, "Objects", 48, 24, SkinVariationType.None, null, null);

	public static readonly Emoji FuneralUrn = new Emoji("26B1-FE0F", null, "⚱\ufe0f", "funeral_urn", new string[1] { "funeral_urn" }, null, null, "Objects", 48, 25, SkinVariationType.None, null, null);

	public static readonly Emoji Moyai = new Emoji("1F5FF", "MOYAI", "\ud83d\uddff", "moyai", new string[1] { "moyai" }, null, null, "Objects", 30, 23, SkinVariationType.None, "moai", new string[4] { "face", "moai", "moyai", "statue" });

	public static readonly Emoji Atm = new Emoji("1F3E7", "AUTOMATED TELLER MACHINE", "\ud83c\udfe7", "atm", new string[1] { "atm" }, null, null, "Symbols", 12, 4, SkinVariationType.None, "ATM sign", new string[5] { "atm", "ATM sign", "automated", "bank", "teller" });

	public static readonly Emoji PutLitterInItsPlace = new Emoji("1F6AE", "PUT LITTER IN ITS PLACE SYMBOL", "\ud83d\udeae", "put_litter_in_its_place", new string[1] { "put_litter_in_its_place" }, null, null, "Symbols", 35, 19, SkinVariationType.None, "litter in bin sign", new string[3] { "litter", "litter bin", "litter in bin sign" });

	public static readonly Emoji PotableWater = new Emoji("1F6B0", "POTABLE WATER SYMBOL", "\ud83d\udeb0", "potable_water", new string[1] { "potable_water" }, null, null, "Symbols", 35, 21, SkinVariationType.None, "potable water", new string[3] { "drinking", "potable", "water" });

	public static readonly Emoji Wheelchair = new Emoji("267F", "WHEELCHAIR SYMBOL", "♿", "wheelchair", new string[1] { "wheelchair" }, null, null, "Symbols", 48, 10, SkinVariationType.None, "wheelchair symbol", new string[2] { "access", "wheelchair symbol" });

	public static readonly Emoji Mens = new Emoji("1F6B9", "MENS SYMBOL", "\ud83d\udeb9", "mens", new string[1] { "mens" }, null, null, "Symbols", 36, 29, SkinVariationType.None, "men’s room", new string[5] { "lavatory", "man", "men’s room", "restroom", "wc" });

	public static readonly Emoji Womens = new Emoji("1F6BA", "WOMENS SYMBOL", "\ud83d\udeba", "womens", new string[1] { "womens" }, null, null, "Symbols", 36, 30, SkinVariationType.None, "women’s room", new string[5] { "lavatory", "restroom", "wc", "woman", "women’s room" });

	public static readonly Emoji Restroom = new Emoji("1F6BB", "RESTROOM", "\ud83d\udebb", "restroom", new string[1] { "restroom" }, null, null, "Symbols", 36, 31, SkinVariationType.None, "restroom", new string[3] { "lavatory", "restroom", "WC" });

	public static readonly Emoji BabySymbol = new Emoji("1F6BC", "BABY SYMBOL", "\ud83d\udebc", "baby_symbol", new string[1] { "baby_symbol" }, null, null, "Symbols", 36, 32, SkinVariationType.None, "baby symbol", new string[3] { "baby", "baby symbol", "changing" });

	public static readonly Emoji Wc = new Emoji("1F6BE", "WATER CLOSET", "\ud83d\udebe", "wc", new string[1] { "wc" }, null, null, "Symbols", 36, 34, SkinVariationType.None, "water closet", new string[5] { "closet", "lavatory", "restroom", "water", "wc" });

	public static readonly Emoji PassportControl = new Emoji("1F6C2", "PASSPORT CONTROL", "\ud83d\udec2", "passport_control", new string[1] { "passport_control" }, null, null, "Symbols", 36, 43, SkinVariationType.None, "passport control", new string[2] { "control", "passport" });

	public static readonly Emoji Customs = new Emoji("1F6C3", "CUSTOMS", "\ud83d\udec3", "customs", new string[1] { "customs" }, null, null, "Symbols", 36, 44, SkinVariationType.None, "customs", new string[1] { "customs" });

	public static readonly Emoji BaggageClaim = new Emoji("1F6C4", "BAGGAGE CLAIM", "\ud83d\udec4", "baggage_claim", new string[1] { "baggage_claim" }, null, null, "Symbols", 36, 45, SkinVariationType.None, "baggage claim", new string[2] { "baggage", "claim" });

	public static readonly Emoji LeftLuggage = new Emoji("1F6C5", "LEFT LUGGAGE", "\ud83d\udec5", "left_luggage", new string[1] { "left_luggage" }, null, null, "Symbols", 36, 46, SkinVariationType.None, "left luggage", new string[4] { "baggage", "left luggage", "locker", "luggage" });

	public static readonly Emoji Warning = new Emoji("26A0-FE0F", "WARNING SIGN", "⚠\ufe0f", "warning", new string[1] { "warning" }, null, null, "Symbols", 48, 20, SkinVariationType.None, null, null);

	public static readonly Emoji ChildrenCrossing = new Emoji("1F6B8", "CHILDREN CROSSING", "\ud83d\udeb8", "children_crossing", new string[1] { "children_crossing" }, null, null, "Symbols", 36, 28, SkinVariationType.None, "children crossing", new string[5] { "child", "children crossing", "crossing", "pedestrian", "traffic" });

	public static readonly Emoji NoEntry = new Emoji("26D4", "NO ENTRY", "⛔", "no_entry", new string[1] { "no_entry" }, null, null, "Symbols", 48, 35, SkinVariationType.None, "no entry", new string[6] { "entry", "forbidden", "no", "not", "prohibited", "traffic" });

	public static readonly Emoji NoEntrySign = new Emoji("1F6AB", "NO ENTRY SIGN", "\ud83d\udeab", "no_entry_sign", new string[1] { "no_entry_sign" }, null, null, "Symbols", 35, 16, SkinVariationType.None, "prohibited", new string[5] { "entry", "forbidden", "no", "not", "prohibited" });

	public static readonly Emoji NoBicycles = new Emoji("1F6B3", "NO BICYCLES", "\ud83d\udeb3", "no_bicycles", new string[1] { "no_bicycles" }, null, null, "Symbols", 35, 24, SkinVariationType.None, "no bicycles", new string[7] { "bicycle", "bike", "forbidden", "no", "no bicycles", "not", "prohibited" });

	public static readonly Emoji NoSmoking = new Emoji("1F6AD", "NO SMOKING SYMBOL", "\ud83d\udead", "no_smoking", new string[1] { "no_smoking" }, null, null, "Symbols", 35, 18, SkinVariationType.None, "no smoking", new string[5] { "forbidden", "no", "not", "prohibited", "smoking" });

	public static readonly Emoji DoNotLitter = new Emoji("1F6AF", "DO NOT LITTER SYMBOL", "\ud83d\udeaf", "do_not_litter", new string[1] { "do_not_litter" }, null, null, "Symbols", 35, 20, SkinVariationType.None, "no littering", new string[6] { "forbidden", "litter", "no", "no littering", "not", "prohibited" });

	public static readonly Emoji NonPotableWater = new Emoji("1F6B1", "NON-POTABLE WATER SYMBOL", "\ud83d\udeb1", "non-potable_water", new string[1] { "non-potable_water" }, null, null, "Symbols", 35, 22, SkinVariationType.None, "non-potable water", new string[3] { "non-drinking", "non-potable", "water" });

	public static readonly Emoji NoPedestrians = new Emoji("1F6B7", "NO PEDESTRIANS", "\ud83d\udeb7", "no_pedestrians", new string[1] { "no_pedestrians" }, null, null, "Symbols", 36, 27, SkinVariationType.None, "no pedestrians", new string[6] { "forbidden", "no", "no pedestrians", "not", "pedestrian", "prohibited" });

	public static readonly Emoji NoMobilePhones = new Emoji("1F4F5", "NO MOBILE PHONES", "\ud83d\udcf5", "no_mobile_phones", new string[1] { "no_mobile_phones" }, null, null, "Symbols", 26, 44, SkinVariationType.None, "no mobile phones", new string[9] { "cell", "forbidden", "mobile", "no", "no mobile phones", "not", "phone", "prohibited", "telephone" });

	public static readonly Emoji Underage = new Emoji("1F51E", "NO ONE UNDER EIGHTEEN SYMBOL", "\ud83d\udd1e", "underage", new string[1] { "underage" }, null, null, "Symbols", 27, 32, SkinVariationType.None, "no one under eighteen", new string[9] { "18", "age restriction", "eighteen", "forbidden", "no", "no one under eighteen", "not", "prohibited", "underage" });

	public static readonly Emoji RadioactiveSign = new Emoji("2622-FE0F", null, "☢\ufe0f", "radioactive_sign", new string[1] { "radioactive_sign" }, null, null, "Symbols", 47, 33, SkinVariationType.None, null, null);

	public static readonly Emoji BiohazardSign = new Emoji("2623-FE0F", null, "☣\ufe0f", "biohazard_sign", new string[1] { "biohazard_sign" }, null, null, "Symbols", 47, 34, SkinVariationType.None, null, null);

	public static readonly Emoji ArrowUp = new Emoji("2B06-FE0F", "UPWARDS BLACK ARROW", "⬆\ufe0f", "arrow_up", new string[1] { "arrow_up" }, null, null, "Symbols", 50, 18, SkinVariationType.None, null, null);

	public static readonly Emoji ArrowUpperRight = new Emoji("2197-FE0F", "NORTH EAST ARROW", "↗\ufe0f", "arrow_upper_right", new string[1] { "arrow_upper_right" }, null, null, "Symbols", 46, 36, SkinVariationType.None, null, null);

	public static readonly Emoji ArrowRight = new Emoji("27A1-FE0F", "BLACK RIGHTWARDS ARROW", "➡\ufe0f", "arrow_right", new string[1] { "arrow_right" }, null, null, "Symbols", 50, 12, SkinVariationType.None, null, null);

	public static readonly Emoji ArrowLowerRight = new Emoji("2198-FE0F", "SOUTH EAST ARROW", "↘\ufe0f", "arrow_lower_right", new string[1] { "arrow_lower_right" }, null, null, "Symbols", 46, 37, SkinVariationType.None, null, null);

	public static readonly Emoji ArrowDown = new Emoji("2B07-FE0F", "DOWNWARDS BLACK ARROW", "⬇\ufe0f", "arrow_down", new string[1] { "arrow_down" }, null, null, "Symbols", 50, 19, SkinVariationType.None, null, null);

	public static readonly Emoji ArrowLowerLeft = new Emoji("2199-FE0F", "SOUTH WEST ARROW", "↙\ufe0f", "arrow_lower_left", new string[1] { "arrow_lower_left" }, null, null, "Symbols", 46, 38, SkinVariationType.None, null, null);

	public static readonly Emoji ArrowLeft = new Emoji("2B05-FE0F", "LEFTWARDS BLACK ARROW", "⬅\ufe0f", "arrow_left", new string[1] { "arrow_left" }, null, null, "Symbols", 50, 17, SkinVariationType.None, null, null);

	public static readonly Emoji ArrowUpperLeft = new Emoji("2196-FE0F", "NORTH WEST ARROW", "↖\ufe0f", "arrow_upper_left", new string[1] { "arrow_upper_left" }, null, null, "Symbols", 46, 35, SkinVariationType.None, null, null);

	public static readonly Emoji ArrowUpDown = new Emoji("2195-FE0F", "UP DOWN ARROW", "↕\ufe0f", "arrow_up_down", new string[1] { "arrow_up_down" }, null, null, "Symbols", 46, 34, SkinVariationType.None, null, null);

	public static readonly Emoji LeftRightArrow = new Emoji("2194-FE0F", "LEFT RIGHT ARROW", "↔\ufe0f", "left_right_arrow", new string[1] { "left_right_arrow" }, null, null, "Symbols", 46, 33, SkinVariationType.None, null, null);

	public static readonly Emoji LeftwardsArrowWithHook = new Emoji("21A9-FE0F", "LEFTWARDS ARROW WITH HOOK", "↩\ufe0f", "leftwards_arrow_with_hook", new string[1] { "leftwards_arrow_with_hook" }, null, null, "Symbols", 46, 39, SkinVariationType.None, null, null);

	public static readonly Emoji ArrowRightHook = new Emoji("21AA-FE0F", "RIGHTWARDS ARROW WITH HOOK", "↪\ufe0f", "arrow_right_hook", new string[1] { "arrow_right_hook" }, null, null, "Symbols", 46, 40, SkinVariationType.None, null, null);

	public static readonly Emoji ArrowHeadingUp = new Emoji("2934-FE0F", "ARROW POINTING RIGHTWARDS THEN CURVING UPWARDS", "⤴\ufe0f", "arrow_heading_up", new string[1] { "arrow_heading_up" }, null, null, "Symbols", 50, 15, SkinVariationType.None, null, null);

	public static readonly Emoji ArrowHeadingDown = new Emoji("2935-FE0F", "ARROW POINTING RIGHTWARDS THEN CURVING DOWNWARDS", "⤵\ufe0f", "arrow_heading_down", new string[1] { "arrow_heading_down" }, null, null, "Symbols", 50, 16, SkinVariationType.None, null, null);

	public static readonly Emoji ArrowsClockwise = new Emoji("1F503", "CLOCKWISE DOWNWARDS AND UPWARDS OPEN CIRCLE ARROWS", "\ud83d\udd03", "arrows_clockwise", new string[1] { "arrows_clockwise" }, null, null, "Symbols", 27, 5, SkinVariationType.None, "clockwise vertical arrows", new string[4] { "arrow", "clockwise", "clockwise vertical arrows", "reload" });

	public static readonly Emoji ArrowsCounterclockwise = new Emoji("1F504", "ANTICLOCKWISE DOWNWARDS AND UPWARDS OPEN CIRCLE ARROWS", "\ud83d\udd04", "arrows_counterclockwise", new string[1] { "arrows_counterclockwise" }, null, null, "Symbols", 27, 6, SkinVariationType.None, "counterclockwise arrows button", new string[5] { "anticlockwise", "arrow", "counterclockwise", "counterclockwise arrows button", "withershins" });

	public static readonly Emoji Back = new Emoji("1F519", "BACK WITH LEFTWARDS ARROW ABOVE", "\ud83d\udd19", "back", new string[1] { "back" }, null, null, "Symbols", 27, 27, SkinVariationType.None, "BACK arrow", new string[3] { "arrow", "back", "BACK arrow" });

	public static readonly Emoji End = new Emoji("1F51A", "END WITH LEFTWARDS ARROW ABOVE", "\ud83d\udd1a", "end", new string[1] { "end" }, null, null, "Symbols", 27, 28, SkinVariationType.None, "END arrow", new string[3] { "arrow", "end", "END arrow" });

	public static readonly Emoji On = new Emoji("1F51B", "ON WITH EXCLAMATION MARK WITH LEFT RIGHT ARROW ABOVE", "\ud83d\udd1b", "on", new string[1] { "on" }, null, null, "Symbols", 27, 29, SkinVariationType.None, "ON! arrow", new string[4] { "arrow", "mark", "on", "ON! arrow" });

	public static readonly Emoji Soon = new Emoji("1F51C", "SOON WITH RIGHTWARDS ARROW ABOVE", "\ud83d\udd1c", "soon", new string[1] { "soon" }, null, null, "Symbols", 27, 30, SkinVariationType.None, "SOON arrow", new string[3] { "arrow", "soon", "SOON arrow" });

	public static readonly Emoji Top = new Emoji("1F51D", "TOP WITH UPWARDS ARROW ABOVE", "\ud83d\udd1d", "top", new string[1] { "top" }, null, null, "Symbols", 27, 31, SkinVariationType.None, "TOP arrow", new string[4] { "arrow", "top", "TOP arrow", "up" });

	public static readonly Emoji PlaceOfWorship = new Emoji("1F6D0", "PLACE OF WORSHIP", "\ud83d\uded0", "place_of_worship", new string[1] { "place_of_worship" }, null, null, "Symbols", 37, 5, SkinVariationType.None, "place of worship", new string[3] { "place of worship", "religion", "worship" });

	public static readonly Emoji AtomSymbol = new Emoji("269B-FE0F", null, "⚛\ufe0f", "atom_symbol", new string[1] { "atom_symbol" }, null, null, "Symbols", 48, 18, SkinVariationType.None, null, null);

	public static readonly Emoji OmSymbol = new Emoji("1F549-FE0F", null, "\ud83d\udd49\ufe0f", "om_symbol", new string[1] { "om_symbol" }, null, null, "Symbols", 28, 12, SkinVariationType.None, null, null);

	public static readonly Emoji StarOfDavid = new Emoji("2721-FE0F", null, "✡\ufe0f", "star_of_david", new string[1] { "star_of_david" }, null, null, "Symbols", 49, 47, SkinVariationType.None, null, null);

	public static readonly Emoji WheelOfDharma = new Emoji("2638-FE0F", null, "☸\ufe0f", "wheel_of_dharma", new string[1] { "wheel_of_dharma" }, null, null, "Symbols", 47, 39, SkinVariationType.None, null, null);

	public static readonly Emoji YinYang = new Emoji("262F-FE0F", null, "☯\ufe0f", "yin_yang", new string[1] { "yin_yang" }, null, null, "Symbols", 47, 38, SkinVariationType.None, null, null);

	public static readonly Emoji LatinCross = new Emoji("271D-FE0F", null, "✝\ufe0f", "latin_cross", new string[1] { "latin_cross" }, null, null, "Symbols", 49, 46, SkinVariationType.None, null, null);

	public static readonly Emoji OrthodoxCross = new Emoji("2626-FE0F", null, "☦\ufe0f", "orthodox_cross", new string[1] { "orthodox_cross" }, null, null, "Symbols", 47, 35, SkinVariationType.None, null, null);

	public static readonly Emoji StarAndCrescent = new Emoji("262A-FE0F", null, "☪\ufe0f", "star_and_crescent", new string[1] { "star_and_crescent" }, null, null, "Symbols", 47, 36, SkinVariationType.None, null, null);

	public static readonly Emoji PeaceSymbol = new Emoji("262E-FE0F", null, "☮\ufe0f", "peace_symbol", new string[1] { "peace_symbol" }, null, null, "Symbols", 47, 37, SkinVariationType.None, null, null);

	public static readonly Emoji MenorahWithNineBranches = new Emoji("1F54E", "MENORAH WITH NINE BRANCHES", "\ud83d\udd4e", "menorah_with_nine_branches", new string[1] { "menorah_with_nine_branches" }, null, null, "Symbols", 28, 17, SkinVariationType.None, "menorah", new string[4] { "candelabrum", "candlestick", "menorah", "religion" });

	public static readonly Emoji SixPointedStar = new Emoji("1F52F", "SIX POINTED STAR WITH MIDDLE DOT", "\ud83d\udd2f", "six_pointed_star", new string[1] { "six_pointed_star" }, null, null, "Symbols", 27, 49, SkinVariationType.None, "dotted six-pointed star", new string[3] { "dotted six-pointed star", "fortune", "star" });

	public static readonly Emoji Aries = new Emoji("2648", "ARIES", "♈", "aries", new string[1] { "aries" }, null, null, "Symbols", 47, 44, SkinVariationType.None, "Aries", new string[3] { "Aries", "ram", "zodiac" });

	public static readonly Emoji Taurus = new Emoji("2649", "TAURUS", "♉", "taurus", new string[1] { "taurus" }, null, null, "Symbols", 47, 45, SkinVariationType.None, "Taurus", new string[4] { "bull", "ox", "Taurus", "zodiac" });

	public static readonly Emoji Gemini = new Emoji("264A", "GEMINI", "♊", "gemini", new string[1] { "gemini" }, null, null, "Symbols", 47, 46, SkinVariationType.None, "Gemini", new string[3] { "Gemini", "twins", "zodiac" });

	public static readonly Emoji Cancer = new Emoji("264B", "CANCER", "♋", "cancer", new string[1] { "cancer" }, null, null, "Symbols", 47, 47, SkinVariationType.None, "Cancer", new string[3] { "Cancer", "crab", "zodiac" });

	public static readonly Emoji Leo = new Emoji("264C", "LEO", "♌", "leo", new string[1] { "leo" }, null, null, "Symbols", 47, 48, SkinVariationType.None, "Leo", new string[3] { "Leo", "lion", "zodiac" });

	public static readonly Emoji Virgo = new Emoji("264D", "VIRGO", "♍", "virgo", new string[1] { "virgo" }, null, null, "Symbols", 47, 49, SkinVariationType.None, "Virgo", new string[2] { "Virgo", "zodiac" });

	public static readonly Emoji Libra = new Emoji("264E", "LIBRA", "♎", "libra", new string[1] { "libra" }, null, null, "Symbols", 47, 50, SkinVariationType.None, "Libra", new string[5] { "balance", "justice", "Libra", "scales", "zodiac" });

	public static readonly Emoji Scorpius = new Emoji("264F", "SCORPIUS", "♏", "scorpius", new string[1] { "scorpius" }, null, null, "Symbols", 47, 51, SkinVariationType.None, "Scorpio", new string[4] { "Scorpio", "scorpion", "scorpius", "zodiac" });

	public static readonly Emoji Sagittarius = new Emoji("2650", "SAGITTARIUS", "♐", "sagittarius", new string[1] { "sagittarius" }, null, null, "Symbols", 48, 0, SkinVariationType.None, "Sagittarius", new string[3] { "archer", "Sagittarius", "zodiac" });

	public static readonly Emoji Capricorn = new Emoji("2651", "CAPRICORN", "♑", "capricorn", new string[1] { "capricorn" }, null, null, "Symbols", 48, 1, SkinVariationType.None, "Capricorn", new string[3] { "Capricorn", "goat", "zodiac" });

	public static readonly Emoji Aquarius = new Emoji("2652", "AQUARIUS", "♒", "aquarius", new string[1] { "aquarius" }, null, null, "Symbols", 48, 2, SkinVariationType.None, "Aquarius", new string[4] { "Aquarius", "bearer", "water", "zodiac" });

	public static readonly Emoji Pisces = new Emoji("2653", "PISCES", "♓", "pisces", new string[1] { "pisces" }, null, null, "Symbols", 48, 3, SkinVariationType.None, "Pisces", new string[3] { "fish", "Pisces", "zodiac" });

	public static readonly Emoji Ophiuchus = new Emoji("26CE", "OPHIUCHUS", "⛎", "ophiuchus", new string[1] { "ophiuchus" }, null, null, "Symbols", 48, 31, SkinVariationType.None, "Ophiuchus", new string[5] { "bearer", "Ophiuchus", "serpent", "snake", "zodiac" });

	public static readonly Emoji TwistedRightwardsArrows = new Emoji("1F500", "TWISTED RIGHTWARDS ARROWS", "\ud83d\udd00", "twisted_rightwards_arrows", new string[1] { "twisted_rightwards_arrows" }, null, null, "Symbols", 27, 2, SkinVariationType.None, "shuffle tracks button", new string[3] { "arrow", "crossed", "shuffle tracks button" });

	public static readonly Emoji Repeat = new Emoji("1F501", "CLOCKWISE RIGHTWARDS AND LEFTWARDS OPEN CIRCLE ARROWS", "\ud83d\udd01", "repeat", new string[1] { "repeat" }, null, null, "Symbols", 27, 3, SkinVariationType.None, "repeat button", new string[4] { "arrow", "clockwise", "repeat", "repeat button" });

	public static readonly Emoji RepeatOne = new Emoji("1F502", "CLOCKWISE RIGHTWARDS AND LEFTWARDS OPEN CIRCLE ARROWS WITH CIRCLED ONE OVERLAY", "\ud83d\udd02", "repeat_one", new string[1] { "repeat_one" }, null, null, "Symbols", 27, 4, SkinVariationType.None, "repeat single button", new string[4] { "arrow", "clockwise", "once", "repeat single button" });

	public static readonly Emoji ArrowForward = new Emoji("25B6-FE0F", "BLACK RIGHT-POINTING TRIANGLE", "▶\ufe0f", "arrow_forward", new string[1] { "arrow_forward" }, null, null, "Symbols", 47, 10, SkinVariationType.None, null, null);

	public static readonly Emoji FastForward = new Emoji("23E9", "BLACK RIGHT-POINTING DOUBLE TRIANGLE", "⏩", "fast_forward", new string[1] { "fast_forward" }, null, null, "Symbols", 46, 45, SkinVariationType.None, "fast-forward button", new string[5] { "arrow", "double", "fast", "fast-forward button", "forward" });

	public static readonly Emoji BlackRightPointingDoubleTriangleWithVerticalBar = new Emoji("23ED-FE0F", null, "⏭\ufe0f", "black_right_pointing_double_triangle_with_vertical_bar", new string[1] { "black_right_pointing_double_triangle_with_vertical_bar" }, null, null, "Symbols", 46, 49, SkinVariationType.None, null, null);

	public static readonly Emoji BlackRightPointingTriangleWithDoubleVerticalBar = new Emoji("23EF-FE0F", null, "⏯\ufe0f", "black_right_pointing_triangle_with_double_vertical_bar", new string[1] { "black_right_pointing_triangle_with_double_vertical_bar" }, null, null, "Symbols", 46, 51, SkinVariationType.None, null, null);

	public static readonly Emoji ArrowBackward = new Emoji("25C0-FE0F", "BLACK LEFT-POINTING TRIANGLE", "◀\ufe0f", "arrow_backward", new string[1] { "arrow_backward" }, null, null, "Symbols", 47, 11, SkinVariationType.None, null, null);

	public static readonly Emoji Rewind = new Emoji("23EA", "BLACK LEFT-POINTING DOUBLE TRIANGLE", "⏪", "rewind", new string[1] { "rewind" }, null, null, "Symbols", 46, 46, SkinVariationType.None, "fast reverse button", new string[4] { "arrow", "double", "fast reverse button", "rewind" });

	public static readonly Emoji BlackLeftPointingDoubleTriangleWithVerticalBar = new Emoji("23EE-FE0F", null, "⏮\ufe0f", "black_left_pointing_double_triangle_with_vertical_bar", new string[1] { "black_left_pointing_double_triangle_with_vertical_bar" }, null, null, "Symbols", 46, 50, SkinVariationType.None, null, null);

	public static readonly Emoji ArrowUpSmall = new Emoji("1F53C", "UP-POINTING SMALL RED TRIANGLE", "\ud83d\udd3c", "arrow_up_small", new string[1] { "arrow_up_small" }, null, null, "Symbols", 28, 10, SkinVariationType.None, "upwards button", new string[4] { "arrow", "button", "red", "upwards button" });

	public static readonly Emoji ArrowDoubleUp = new Emoji("23EB", "BLACK UP-POINTING DOUBLE TRIANGLE", "⏫", "arrow_double_up", new string[1] { "arrow_double_up" }, null, null, "Symbols", 46, 47, SkinVariationType.None, "fast up button", new string[3] { "arrow", "double", "fast up button" });

	public static readonly Emoji ArrowDownSmall = new Emoji("1F53D", "DOWN-POINTING SMALL RED TRIANGLE", "\ud83d\udd3d", "arrow_down_small", new string[1] { "arrow_down_small" }, null, null, "Symbols", 28, 11, SkinVariationType.None, "downwards button", new string[5] { "arrow", "button", "down", "downwards button", "red" });

	public static readonly Emoji ArrowDoubleDown = new Emoji("23EC", "BLACK DOWN-POINTING DOUBLE TRIANGLE", "⏬", "arrow_double_down", new string[1] { "arrow_double_down" }, null, null, "Symbols", 46, 48, SkinVariationType.None, "fast down button", new string[4] { "arrow", "double", "down", "fast down button" });

	public static readonly Emoji DoubleVerticalBar = new Emoji("23F8-FE0F", null, "⏸\ufe0f", "double_vertical_bar", new string[1] { "double_vertical_bar" }, null, null, "Symbols", 47, 4, SkinVariationType.None, null, null);

	public static readonly Emoji BlackSquareForStop = new Emoji("23F9-FE0F", null, "⏹\ufe0f", "black_square_for_stop", new string[1] { "black_square_for_stop" }, null, null, "Symbols", 47, 5, SkinVariationType.None, null, null);

	public static readonly Emoji BlackCircleForRecord = new Emoji("23FA-FE0F", null, "⏺\ufe0f", "black_circle_for_record", new string[1] { "black_circle_for_record" }, null, null, "Symbols", 47, 6, SkinVariationType.None, null, null);

	public static readonly Emoji Eject = new Emoji("23CF-FE0F", null, "⏏\ufe0f", "eject", new string[1] { "eject" }, null, null, "Symbols", 46, 44, SkinVariationType.None, null, null);

	public static readonly Emoji Cinema = new Emoji("1F3A6", "CINEMA", "\ud83c\udfa6", "cinema", new string[1] { "cinema" }, null, null, "Symbols", 9, 0, SkinVariationType.None, "cinema", new string[4] { "camera", "cinema", "film", "movie" });

	public static readonly Emoji LowBrightness = new Emoji("1F505", "LOW BRIGHTNESS SYMBOL", "\ud83d\udd05", "low_brightness", new string[1] { "low_brightness" }, null, null, "Symbols", 27, 7, SkinVariationType.None, "dim button", new string[4] { "brightness", "dim", "dim button", "low" });

	public static readonly Emoji HighBrightness = new Emoji("1F506", "HIGH BRIGHTNESS SYMBOL", "\ud83d\udd06", "high_brightness", new string[1] { "high_brightness" }, null, null, "Symbols", 27, 8, SkinVariationType.None, "bright button", new string[3] { "bright", "bright button", "brightness" });

	public static readonly Emoji SignalStrength = new Emoji("1F4F6", "ANTENNA WITH BARS", "\ud83d\udcf6", "signal_strength", new string[1] { "signal_strength" }, null, null, "Symbols", 26, 45, SkinVariationType.None, "antenna bars", new string[8] { "antenna", "antenna bars", "bar", "cell", "mobile", "phone", "signal", "telephone" });

	public static readonly Emoji VibrationMode = new Emoji("1F4F3", "VIBRATION MODE", "\ud83d\udcf3", "vibration_mode", new string[1] { "vibration_mode" }, null, null, "Symbols", 26, 42, SkinVariationType.None, "vibration mode", new string[6] { "cell", "mobile", "mode", "phone", "telephone", "vibration" });

	public static readonly Emoji MobilePhoneOff = new Emoji("1F4F4", "MOBILE PHONE OFF", "\ud83d\udcf4", "mobile_phone_off", new string[1] { "mobile_phone_off" }, null, null, "Symbols", 26, 43, SkinVariationType.None, "mobile phone off", new string[5] { "cell", "mobile", "off", "phone", "telephone" });

	public static readonly Emoji FemaleSign = new Emoji("2640-FE0F", null, "♀\ufe0f", "female_sign", new string[1] { "female_sign" }, null, null, "Symbols", 47, 42, SkinVariationType.None, null, null);

	public static readonly Emoji MaleSign = new Emoji("2642-FE0F", null, "♂\ufe0f", "male_sign", new string[1] { "male_sign" }, null, null, "Symbols", 47, 43, SkinVariationType.None, null, null);

	public static readonly Emoji MedicalSymbol = new Emoji("2695-FE0F", null, "⚕\ufe0f", "medical_symbol", new string[2] { "medical_symbol", "staff_of_aesculapius" }, null, null, "Symbols", 48, 14, SkinVariationType.None, null, null);

	public static readonly Emoji Recycle = new Emoji("267B-FE0F", "BLACK UNIVERSAL RECYCLING SYMBOL", "♻\ufe0f", "recycle", new string[1] { "recycle" }, null, null, "Symbols", 48, 9, SkinVariationType.None, null, null);

	public static readonly Emoji FleurDeLis = new Emoji("269C-FE0F", null, "⚜\ufe0f", "fleur_de_lis", new string[1] { "fleur_de_lis" }, null, null, "Symbols", 48, 19, SkinVariationType.None, null, null);

	public static readonly Emoji Trident = new Emoji("1F531", "TRIDENT EMBLEM", "\ud83d\udd31", "trident", new string[1] { "trident" }, null, null, "Symbols", 27, 51, SkinVariationType.None, "trident emblem", new string[5] { "anchor", "emblem", "ship", "tool", "trident" });

	public static readonly Emoji NameBadge = new Emoji("1F4DB", "NAME BADGE", "\ud83d\udcdb", "name_badge", new string[1] { "name_badge" }, null, null, "Symbols", 26, 18, SkinVariationType.None, "name badge", new string[2] { "badge", "name" });

	public static readonly Emoji Beginner = new Emoji("1F530", "JAPANESE SYMBOL FOR BEGINNER", "\ud83d\udd30", "beginner", new string[1] { "beginner" }, null, null, "Symbols", 27, 50, SkinVariationType.None, "Japanese symbol for beginner", new string[8] { "beginner", "chevron", "green", "Japanese", "Japanese symbol for beginner", "leaf", "tool", "yellow" });

	public static readonly Emoji O = new Emoji("2B55", "HEAVY LARGE CIRCLE", "⭕", "o", new string[1] { "o" }, null, null, "Symbols", 50, 23, SkinVariationType.None, "heavy large circle", new string[3] { "circle", "heavy large circle", "o" });

	public static readonly Emoji WhiteCheckMark = new Emoji("2705", "WHITE HEAVY CHECK MARK", "✅", "white_check_mark", new string[1] { "white_check_mark" }, null, null, "Symbols", 49, 15, SkinVariationType.None, "white heavy check mark", new string[3] { "check", "mark", "white heavy check mark" });

	public static readonly Emoji BallotBoxWithCheck = new Emoji("2611-FE0F", "BALLOT BOX WITH CHECK", "☑\ufe0f", "ballot_box_with_check", new string[1] { "ballot_box_with_check" }, null, null, "Symbols", 47, 22, SkinVariationType.None, null, null);

	public static readonly Emoji HeavyCheckMark = new Emoji("2714-FE0F", "HEAVY CHECK MARK", "✔\ufe0f", "heavy_check_mark", new string[1] { "heavy_check_mark" }, null, null, "Symbols", 49, 44, SkinVariationType.None, null, null);

	public static readonly Emoji HeavyMultiplicationX = new Emoji("2716-FE0F", "HEAVY MULTIPLICATION X", "✖\ufe0f", "heavy_multiplication_x", new string[1] { "heavy_multiplication_x" }, null, null, "Symbols", 49, 45, SkinVariationType.None, null, null);

	public static readonly Emoji X = new Emoji("274C", "CROSS MARK", "❌", "x", new string[1] { "x" }, null, null, "Symbols", 50, 1, SkinVariationType.None, "cross mark", new string[6] { "cancel", "cross mark", "mark", "multiplication", "multiply", "x" });

	public static readonly Emoji NegativeSquaredCrossMark = new Emoji("274E", "NEGATIVE SQUARED CROSS MARK", "❎", "negative_squared_cross_mark", new string[1] { "negative_squared_cross_mark" }, null, null, "Symbols", 50, 2, SkinVariationType.None, "cross mark button", new string[3] { "cross mark button", "mark", "square" });

	public static readonly Emoji HeavyPlusSign = new Emoji("2795", "HEAVY PLUS SIGN", "➕", "heavy_plus_sign", new string[1] { "heavy_plus_sign" }, null, null, "Symbols", 50, 9, SkinVariationType.None, "heavy plus sign", new string[3] { "heavy plus sign", "math", "plus" });

	public static readonly Emoji HeavyMinusSign = new Emoji("2796", "HEAVY MINUS SIGN", "➖", "heavy_minus_sign", new string[1] { "heavy_minus_sign" }, null, null, "Symbols", 50, 10, SkinVariationType.None, "heavy minus sign", new string[3] { "heavy minus sign", "math", "minus" });

	public static readonly Emoji HeavyDivisionSign = new Emoji("2797", "HEAVY DIVISION SIGN", "➗", "heavy_division_sign", new string[1] { "heavy_division_sign" }, null, null, "Symbols", 50, 11, SkinVariationType.None, "heavy division sign", new string[3] { "division", "heavy division sign", "math" });

	public static readonly Emoji CurlyLoop = new Emoji("27B0", "CURLY LOOP", "➰", "curly_loop", new string[1] { "curly_loop" }, null, null, "Symbols", 50, 13, SkinVariationType.None, "curly loop", new string[3] { "curl", "curly loop", "loop" });

	public static readonly Emoji Loop = new Emoji("27BF", "DOUBLE CURLY LOOP", "➿", "loop", new string[1] { "loop" }, null, null, "Symbols", 50, 14, SkinVariationType.None, "double curly loop", new string[4] { "curl", "double", "double curly loop", "loop" });

	public static readonly Emoji PartAlternationMark = new Emoji("303D-FE0F", "PART ALTERNATION MARK", "〽\ufe0f", "part_alternation_mark", new string[1] { "part_alternation_mark" }, null, null, "Symbols", 50, 25, SkinVariationType.None, null, null);

	public static readonly Emoji EightSpokedAsterisk = new Emoji("2733-FE0F", "EIGHT SPOKED ASTERISK", "✳\ufe0f", "eight_spoked_asterisk", new string[1] { "eight_spoked_asterisk" }, null, null, "Symbols", 49, 49, SkinVariationType.None, null, null);

	public static readonly Emoji EightPointedBlackStar = new Emoji("2734-FE0F", "EIGHT POINTED BLACK STAR", "✴\ufe0f", "eight_pointed_black_star", new string[1] { "eight_pointed_black_star" }, null, null, "Symbols", 49, 50, SkinVariationType.None, null, null);

	public static readonly Emoji Sparkle = new Emoji("2747-FE0F", "SPARKLE", "❇\ufe0f", "sparkle", new string[1] { "sparkle" }, null, null, "Symbols", 50, 0, SkinVariationType.None, null, null);

	public static readonly Emoji Bangbang = new Emoji("203C-FE0F", "DOUBLE EXCLAMATION MARK", "‼\ufe0f", "bangbang", new string[1] { "bangbang" }, null, null, "Symbols", 46, 29, SkinVariationType.None, null, null);

	public static readonly Emoji Interrobang = new Emoji("2049-FE0F", "EXCLAMATION QUESTION MARK", "⁉\ufe0f", "interrobang", new string[1] { "interrobang" }, null, null, "Symbols", 46, 30, SkinVariationType.None, null, null);

	public static readonly Emoji Question = new Emoji("2753", "BLACK QUESTION MARK ORNAMENT", "❓", "question", new string[1] { "question" }, null, null, "Symbols", 50, 3, SkinVariationType.None, "question mark", new string[3] { "mark", "punctuation", "question" });

	public static readonly Emoji GreyQuestion = new Emoji("2754", "WHITE QUESTION MARK ORNAMENT", "❔", "grey_question", new string[1] { "grey_question" }, null, null, "Symbols", 50, 4, SkinVariationType.None, "white question mark", new string[5] { "mark", "outlined", "punctuation", "question", "white question mark" });

	public static readonly Emoji GreyExclamation = new Emoji("2755", "WHITE EXCLAMATION MARK ORNAMENT", "❕", "grey_exclamation", new string[1] { "grey_exclamation" }, null, null, "Symbols", 50, 5, SkinVariationType.None, "white exclamation mark", new string[5] { "exclamation", "mark", "outlined", "punctuation", "white exclamation mark" });

	public static readonly Emoji Exclamation = new Emoji("2757", "HEAVY EXCLAMATION MARK SYMBOL", "❗", "exclamation", new string[2] { "exclamation", "heavy_exclamation_mark" }, null, null, "Symbols", 50, 6, SkinVariationType.None, "exclamation mark", new string[3] { "exclamation", "mark", "punctuation" });

	public static readonly Emoji WavyDash = new Emoji("3030-FE0F", "WAVY DASH", "〰\ufe0f", "wavy_dash", new string[1] { "wavy_dash" }, null, null, "Symbols", 50, 24, SkinVariationType.None, null, null);

	public static readonly Emoji Copyright = new Emoji("00A9-FE0F", "COPYRIGHT SIGN", "©\ufe0f", "copyright", new string[1] { "copyright" }, null, null, "Symbols", 0, 12, SkinVariationType.None, null, null);

	public static readonly Emoji Registered = new Emoji("00AE-FE0F", "REGISTERED SIGN", "®\ufe0f", "registered", new string[1] { "registered" }, null, null, "Symbols", 0, 13, SkinVariationType.None, null, null);

	public static readonly Emoji Tm = new Emoji("2122-FE0F", "TRADE MARK SIGN", "™\ufe0f", "tm", new string[1] { "tm" }, null, null, "Symbols", 46, 31, SkinVariationType.None, null, null);

	public static readonly Emoji Hash = new Emoji("0023-FE0F-20E3", "HASH KEY", "#\ufe0f\u20e3", "hash", new string[1] { "hash" }, null, null, "Symbols", 0, 0, SkinVariationType.None, null, null);

	public static readonly Emoji KeycapStar = new Emoji("002A-FE0F-20E3", null, "*\ufe0f\u20e3", "keycap_star", new string[1] { "keycap_star" }, null, null, "Symbols", 0, 1, SkinVariationType.None, null, null);

	public static readonly Emoji Zero = new Emoji("0030-FE0F-20E3", "KEYCAP 0", "0\ufe0f\u20e3", "zero", new string[1] { "zero" }, null, null, "Symbols", 0, 2, SkinVariationType.None, null, null);

	public static readonly Emoji One = new Emoji("0031-FE0F-20E3", "KEYCAP 1", "1\ufe0f\u20e3", "one", new string[1] { "one" }, null, null, "Symbols", 0, 3, SkinVariationType.None, null, null);

	public static readonly Emoji Two = new Emoji("0032-FE0F-20E3", "KEYCAP 2", "2\ufe0f\u20e3", "two", new string[1] { "two" }, null, null, "Symbols", 0, 4, SkinVariationType.None, null, null);

	public static readonly Emoji Three = new Emoji("0033-FE0F-20E3", "KEYCAP 3", "3\ufe0f\u20e3", "three", new string[1] { "three" }, null, null, "Symbols", 0, 5, SkinVariationType.None, null, null);

	public static readonly Emoji Four = new Emoji("0034-FE0F-20E3", "KEYCAP 4", "4\ufe0f\u20e3", "four", new string[1] { "four" }, null, null, "Symbols", 0, 6, SkinVariationType.None, null, null);

	public static readonly Emoji Five = new Emoji("0035-FE0F-20E3", "KEYCAP 5", "5\ufe0f\u20e3", "five", new string[1] { "five" }, null, null, "Symbols", 0, 7, SkinVariationType.None, null, null);

	public static readonly Emoji Six = new Emoji("0036-FE0F-20E3", "KEYCAP 6", "6\ufe0f\u20e3", "six", new string[1] { "six" }, null, null, "Symbols", 0, 8, SkinVariationType.None, null, null);

	public static readonly Emoji Seven = new Emoji("0037-FE0F-20E3", "KEYCAP 7", "7\ufe0f\u20e3", "seven", new string[1] { "seven" }, null, null, "Symbols", 0, 9, SkinVariationType.None, null, null);

	public static readonly Emoji Eight = new Emoji("0038-FE0F-20E3", "KEYCAP 8", "8\ufe0f\u20e3", "eight", new string[1] { "eight" }, null, null, "Symbols", 0, 10, SkinVariationType.None, null, null);

	public static readonly Emoji Nine = new Emoji("0039-FE0F-20E3", "KEYCAP 9", "9\ufe0f\u20e3", "nine", new string[1] { "nine" }, null, null, "Symbols", 0, 11, SkinVariationType.None, null, null);

	public static readonly Emoji KeycapTen = new Emoji("1F51F", "KEYCAP TEN", "\ud83d\udd1f", "keycap_ten", new string[1] { "keycap_ten" }, null, null, "Symbols", 27, 33, SkinVariationType.None, null, null);

	public static readonly Emoji OneHundred = new Emoji("1F4AF", "HUNDRED POINTS SYMBOL", "\ud83d\udcaf", "100", new string[1] { "100" }, null, null, "Symbols", 25, 26, SkinVariationType.None, "hundred points", new string[5] { "100", "full", "hundred", "hundred points", "score" });

	public static readonly Emoji CapitalAbcd = new Emoji("1F520", "INPUT SYMBOL FOR LATIN CAPITAL LETTERS", "\ud83d\udd20", "capital_abcd", new string[1] { "capital_abcd" }, null, null, "Symbols", 27, 34, SkinVariationType.None, "input latin uppercase", new string[5] { "ABCD", "input", "latin", "letters", "uppercase" });

	public static readonly Emoji Abcd = new Emoji("1F521", "INPUT SYMBOL FOR LATIN SMALL LETTERS", "\ud83d\udd21", "abcd", new string[1] { "abcd" }, null, null, "Symbols", 27, 35, SkinVariationType.None, "input latin lowercase", new string[5] { "abcd", "input", "latin", "letters", "lowercase" });

	public static readonly Emoji OneTwoThreeFour = new Emoji("1F522", "INPUT SYMBOL FOR NUMBERS", "\ud83d\udd22", "1234", new string[1] { "1234" }, null, null, "Symbols", 27, 36, SkinVariationType.None, "input numbers", new string[3] { "1234", "input", "numbers" });

	public static readonly Emoji Symbols = new Emoji("1F523", "INPUT SYMBOL FOR SYMBOLS", "\ud83d\udd23", "symbols", new string[1] { "symbols" }, null, null, "Symbols", 27, 37, SkinVariationType.None, "input symbols", new string[3] { "〒♪&%", "input", "input symbols" });

	public static readonly Emoji Abc = new Emoji("1F524", "INPUT SYMBOL FOR LATIN LETTERS", "\ud83d\udd24", "abc", new string[1] { "abc" }, null, null, "Symbols", 27, 38, SkinVariationType.None, "input latin letters", new string[5] { "abc", "alphabet", "input", "latin", "letters" });

	public static readonly Emoji A = new Emoji("1F170-FE0F", "NEGATIVE SQUARED LATIN CAPITAL LETTER A", "\ud83c\udd70\ufe0f", "a", new string[1] { "a" }, null, null, "Symbols", 0, 16, SkinVariationType.None, null, null);

	public static readonly Emoji Ab = new Emoji("1F18E", "NEGATIVE SQUARED AB", "\ud83c\udd8e", "ab", new string[1] { "ab" }, null, null, "Symbols", 0, 20, SkinVariationType.None, "AB button (blood type)", new string[3] { "ab", "AB button (blood type)", "blood type" });

	public static readonly Emoji B = new Emoji("1F171-FE0F", "NEGATIVE SQUARED LATIN CAPITAL LETTER B", "\ud83c\udd71\ufe0f", "b", new string[1] { "b" }, null, null, "Symbols", 0, 17, SkinVariationType.None, null, null);

	public static readonly Emoji Cl = new Emoji("1F191", "SQUARED CL", "\ud83c\udd91", "cl", new string[1] { "cl" }, null, null, "Symbols", 0, 21, SkinVariationType.None, "CL button", new string[2] { "cl", "CL button" });

	public static readonly Emoji Cool = new Emoji("1F192", "SQUARED COOL", "\ud83c\udd92", "cool", new string[1] { "cool" }, null, null, "Symbols", 0, 22, SkinVariationType.None, "COOL button", new string[2] { "cool", "COOL button" });

	public static readonly Emoji Free = new Emoji("1F193", "SQUARED FREE", "\ud83c\udd93", "free", new string[1] { "free" }, null, null, "Symbols", 0, 23, SkinVariationType.None, "FREE button", new string[2] { "free", "FREE button" });

	public static readonly Emoji InformationSource = new Emoji("2139-FE0F", "INFORMATION SOURCE", "ℹ\ufe0f", "information_source", new string[1] { "information_source" }, null, null, "Symbols", 46, 32, SkinVariationType.None, null, null);

	public static readonly Emoji Id = new Emoji("1F194", "SQUARED ID", "\ud83c\udd94", "id", new string[1] { "id" }, null, null, "Symbols", 0, 24, SkinVariationType.None, "ID button", new string[3] { "id", "ID button", "identity" });

	public static readonly Emoji M = new Emoji("24C2-FE0F", "CIRCLED LATIN CAPITAL LETTER M", "Ⓜ\ufe0f", "m", new string[1] { "m" }, null, null, "Symbols", 47, 7, SkinVariationType.None, null, null);

	public static readonly Emoji New = new Emoji("1F195", "SQUARED NEW", "\ud83c\udd95", "new", new string[1] { "new" }, null, null, "Symbols", 0, 25, SkinVariationType.None, "NEW button", new string[2] { "new", "NEW button" });

	public static readonly Emoji Ng = new Emoji("1F196", "SQUARED NG", "\ud83c\udd96", "ng", new string[1] { "ng" }, null, null, "Symbols", 0, 26, SkinVariationType.None, "NG button", new string[2] { "ng", "NG button" });

	public static readonly Emoji O2 = new Emoji("1F17E-FE0F", "NEGATIVE SQUARED LATIN CAPITAL LETTER O", "\ud83c\udd7e\ufe0f", "o2", new string[1] { "o2" }, null, null, "Symbols", 0, 18, SkinVariationType.None, null, null);

	public static readonly Emoji Ok = new Emoji("1F197", "SQUARED OK", "\ud83c\udd97", "ok", new string[1] { "ok" }, null, null, "Symbols", 0, 27, SkinVariationType.None, "OK button", new string[2] { "OK", "OK button" });

	public static readonly Emoji Parking = new Emoji("1F17F-FE0F", "NEGATIVE SQUARED LATIN CAPITAL LETTER P", "\ud83c\udd7f\ufe0f", "parking", new string[1] { "parking" }, null, null, "Symbols", 0, 19, SkinVariationType.None, null, null);

	public static readonly Emoji Sos = new Emoji("1F198", "SQUARED SOS", "\ud83c\udd98", "sos", new string[1] { "sos" }, null, null, "Symbols", 0, 28, SkinVariationType.None, "SOS button", new string[3] { "help", "sos", "SOS button" });

	public static readonly Emoji Up = new Emoji("1F199", "SQUARED UP WITH EXCLAMATION MARK", "\ud83c\udd99", "up", new string[1] { "up" }, null, null, "Symbols", 0, 29, SkinVariationType.None, "UP! button", new string[3] { "mark", "up", "UP! button" });

	public static readonly Emoji Vs = new Emoji("1F19A", "SQUARED VS", "\ud83c\udd9a", "vs", new string[1] { "vs" }, null, null, "Symbols", 0, 30, SkinVariationType.None, "VS button", new string[3] { "versus", "vs", "VS button" });

	public static readonly Emoji Koko = new Emoji("1F201", "SQUARED KATAKANA KOKO", "\ud83c\ude01", "koko", new string[1] { "koko" }, null, null, "Symbols", 5, 29, SkinVariationType.None, "Japanese “here” button", new string[5] { "“here”", "Japanese", "Japanese “here” button", "katakana", "ココ" });

	public static readonly Emoji Sa = new Emoji("1F202-FE0F", "SQUARED KATAKANA SA", "\ud83c\ude02\ufe0f", "sa", new string[1] { "sa" }, null, null, "Symbols", 5, 30, SkinVariationType.None, null, null);

	public static readonly Emoji U6708 = new Emoji("1F237-FE0F", "SQUARED CJK UNIFIED IDEOGRAPH-6708", "\ud83c\ude37\ufe0f", "u6708", new string[1] { "u6708" }, null, null, "Symbols", 5, 38, SkinVariationType.None, null, null);

	public static readonly Emoji U6709 = new Emoji("1F236", "SQUARED CJK UNIFIED IDEOGRAPH-6709", "\ud83c\ude36", "u6709", new string[1] { "u6709" }, null, null, "Symbols", 5, 37, SkinVariationType.None, "Japanese “not free of charge” button", new string[5] { "“not free of charge”", "ideograph", "Japanese", "Japanese “not free of charge” button", "有" });

	public static readonly Emoji U6307 = new Emoji("1F22F", "SQUARED CJK UNIFIED IDEOGRAPH-6307", "\ud83c\ude2f", "u6307", new string[1] { "u6307" }, null, null, "Symbols", 5, 32, SkinVariationType.None, "Japanese “reserved” button", new string[5] { "“reserved”", "ideograph", "Japanese", "Japanese “reserved” button", "指" });

	public static readonly Emoji IdeographAdvantage = new Emoji("1F250", "CIRCLED IDEOGRAPH ADVANTAGE", "\ud83c\ude50", "ideograph_advantage", new string[1] { "ideograph_advantage" }, null, null, "Symbols", 5, 42, SkinVariationType.None, "Japanese “bargain” button", new string[5] { "“bargain”", "ideograph", "Japanese", "Japanese “bargain” button", "得" });

	public static readonly Emoji U5272 = new Emoji("1F239", "SQUARED CJK UNIFIED IDEOGRAPH-5272", "\ud83c\ude39", "u5272", new string[1] { "u5272" }, null, null, "Symbols", 5, 40, SkinVariationType.None, "Japanese “discount” button", new string[5] { "“discount”", "ideograph", "Japanese", "Japanese “discount” button", "割" });

	public static readonly Emoji U7121 = new Emoji("1F21A", "SQUARED CJK UNIFIED IDEOGRAPH-7121", "\ud83c\ude1a", "u7121", new string[1] { "u7121" }, null, null, "Symbols", 5, 31, SkinVariationType.None, "Japanese “free of charge” button", new string[5] { "“free of charge”", "ideograph", "Japanese", "Japanese “free of charge” button", "無" });

	public static readonly Emoji U7981 = new Emoji("1F232", "SQUARED CJK UNIFIED IDEOGRAPH-7981", "\ud83c\ude32", "u7981", new string[1] { "u7981" }, null, null, "Symbols", 5, 33, SkinVariationType.None, "Japanese “prohibited” button", new string[5] { "“prohibited”", "ideograph", "Japanese", "Japanese “prohibited” button", "禁" });

	public static readonly Emoji Accept = new Emoji("1F251", "CIRCLED IDEOGRAPH ACCEPT", "\ud83c\ude51", "accept", new string[1] { "accept" }, null, null, "Symbols", 5, 43, SkinVariationType.None, "Japanese “acceptable” button", new string[5] { "“acceptable”", "ideograph", "Japanese", "Japanese “acceptable” button", "可" });

	public static readonly Emoji U7533 = new Emoji("1F238", "SQUARED CJK UNIFIED IDEOGRAPH-7533", "\ud83c\ude38", "u7533", new string[1] { "u7533" }, null, null, "Symbols", 5, 39, SkinVariationType.None, "Japanese “application” button", new string[5] { "“application”", "ideograph", "Japanese", "Japanese “application” button", "申" });

	public static readonly Emoji U5408 = new Emoji("1F234", "SQUARED CJK UNIFIED IDEOGRAPH-5408", "\ud83c\ude34", "u5408", new string[1] { "u5408" }, null, null, "Symbols", 5, 35, SkinVariationType.None, "Japanese “passing grade” button", new string[5] { "“passing grade”", "ideograph", "Japanese", "Japanese “passing grade” button", "合" });

	public static readonly Emoji U7a7a = new Emoji("1F233", "SQUARED CJK UNIFIED IDEOGRAPH-7A7A", "\ud83c\ude33", "u7a7a", new string[1] { "u7a7a" }, null, null, "Symbols", 5, 34, SkinVariationType.None, "Japanese “vacancy” button", new string[5] { "“vacancy”", "ideograph", "Japanese", "Japanese “vacancy” button", "空" });

	public static readonly Emoji Congratulations = new Emoji("3297-FE0F", "CIRCLED IDEOGRAPH CONGRATULATION", "㊗\ufe0f", "congratulations", new string[1] { "congratulations" }, null, null, "Symbols", 50, 26, SkinVariationType.None, null, null);

	public static readonly Emoji Secret = new Emoji("3299-FE0F", "CIRCLED IDEOGRAPH SECRET", "㊙\ufe0f", "secret", new string[1] { "secret" }, null, null, "Symbols", 50, 27, SkinVariationType.None, null, null);

	public static readonly Emoji U55b6 = new Emoji("1F23A", "SQUARED CJK UNIFIED IDEOGRAPH-55B6", "\ud83c\ude3a", "u55b6", new string[1] { "u55b6" }, null, null, "Symbols", 5, 41, SkinVariationType.None, "Japanese “open for business” button", new string[5] { "“open for business”", "ideograph", "Japanese", "Japanese “open for business” button", "営" });

	public static readonly Emoji U6e80 = new Emoji("1F235", "SQUARED CJK UNIFIED IDEOGRAPH-6E80", "\ud83c\ude35", "u6e80", new string[1] { "u6e80" }, null, null, "Symbols", 5, 36, SkinVariationType.None, "Japanese “no vacancy” button", new string[5] { "“no vacancy”", "ideograph", "Japanese", "Japanese “no vacancy” button", "満" });

	public static readonly Emoji RedCircle = new Emoji("1F534", "LARGE RED CIRCLE", "\ud83d\udd34", "red_circle", new string[1] { "red_circle" }, null, null, "Symbols", 28, 2, SkinVariationType.None, "red circle", new string[3] { "circle", "geometric", "red" });

	public static readonly Emoji LargeBlueCircle = new Emoji("1F535", "LARGE BLUE CIRCLE", "\ud83d\udd35", "large_blue_circle", new string[1] { "large_blue_circle" }, null, null, "Symbols", 28, 3, SkinVariationType.None, "blue circle", new string[3] { "blue", "circle", "geometric" });

	public static readonly Emoji WhiteCircle = new Emoji("26AA", "MEDIUM WHITE CIRCLE", "⚪", "white_circle", new string[1] { "white_circle" }, null, null, "Symbols", 48, 22, SkinVariationType.None, "white circle", new string[3] { "circle", "geometric", "white circle" });

	public static readonly Emoji BlackCircle = new Emoji("26AB", "MEDIUM BLACK CIRCLE", "⚫", "black_circle", new string[1] { "black_circle" }, null, null, "Symbols", 48, 23, SkinVariationType.None, "black circle", new string[3] { "black circle", "circle", "geometric" });

	public static readonly Emoji WhiteLargeSquare = new Emoji("2B1C", "WHITE LARGE SQUARE", "⬜", "white_large_square", new string[1] { "white_large_square" }, null, null, "Symbols", 50, 21, SkinVariationType.None, "white large square", new string[3] { "geometric", "square", "white large square" });

	public static readonly Emoji BlackLargeSquare = new Emoji("2B1B", "BLACK LARGE SQUARE", "⬛", "black_large_square", new string[1] { "black_large_square" }, null, null, "Symbols", 50, 20, SkinVariationType.None, "black large square", new string[3] { "black large square", "geometric", "square" });

	public static readonly Emoji BlackMediumSquare = new Emoji("25FC-FE0F", "BLACK MEDIUM SQUARE", "◼\ufe0f", "black_medium_square", new string[1] { "black_medium_square" }, null, null, "Symbols", 47, 13, SkinVariationType.None, null, null);

	public static readonly Emoji WhiteMediumSquare = new Emoji("25FB-FE0F", "WHITE MEDIUM SQUARE", "◻\ufe0f", "white_medium_square", new string[1] { "white_medium_square" }, null, null, "Symbols", 47, 12, SkinVariationType.None, null, null);

	public static readonly Emoji WhiteMediumSmallSquare = new Emoji("25FD", "WHITE MEDIUM SMALL SQUARE", "◽", "white_medium_small_square", new string[1] { "white_medium_small_square" }, null, null, "Symbols", 47, 14, SkinVariationType.None, "white medium-small square", new string[3] { "geometric", "square", "white medium-small square" });

	public static readonly Emoji BlackMediumSmallSquare = new Emoji("25FE", "BLACK MEDIUM SMALL SQUARE", "◾", "black_medium_small_square", new string[1] { "black_medium_small_square" }, null, null, "Symbols", 47, 15, SkinVariationType.None, "black medium-small square", new string[3] { "black medium-small square", "geometric", "square" });

	public static readonly Emoji WhiteSmallSquare = new Emoji("25AB-FE0F", "WHITE SMALL SQUARE", "▫\ufe0f", "white_small_square", new string[1] { "white_small_square" }, null, null, "Symbols", 47, 9, SkinVariationType.None, null, null);

	public static readonly Emoji BlackSmallSquare = new Emoji("25AA-FE0F", "BLACK SMALL SQUARE", "▪\ufe0f", "black_small_square", new string[1] { "black_small_square" }, null, null, "Symbols", 47, 8, SkinVariationType.None, null, null);

	public static readonly Emoji LargeOrangeDiamond = new Emoji("1F536", "LARGE ORANGE DIAMOND", "\ud83d\udd36", "large_orange_diamond", new string[1] { "large_orange_diamond" }, null, null, "Symbols", 28, 4, SkinVariationType.None, "large orange diamond", new string[4] { "diamond", "geometric", "large orange diamond", "orange" });

	public static readonly Emoji LargeBlueDiamond = new Emoji("1F537", "LARGE BLUE DIAMOND", "\ud83d\udd37", "large_blue_diamond", new string[1] { "large_blue_diamond" }, null, null, "Symbols", 28, 5, SkinVariationType.None, "large blue diamond", new string[4] { "blue", "diamond", "geometric", "large blue diamond" });

	public static readonly Emoji SmallOrangeDiamond = new Emoji("1F538", "SMALL ORANGE DIAMOND", "\ud83d\udd38", "small_orange_diamond", new string[1] { "small_orange_diamond" }, null, null, "Symbols", 28, 6, SkinVariationType.None, "small orange diamond", new string[4] { "diamond", "geometric", "orange", "small orange diamond" });

	public static readonly Emoji SmallBlueDiamond = new Emoji("1F539", "SMALL BLUE DIAMOND", "\ud83d\udd39", "small_blue_diamond", new string[1] { "small_blue_diamond" }, null, null, "Symbols", 28, 7, SkinVariationType.None, "small blue diamond", new string[4] { "blue", "diamond", "geometric", "small blue diamond" });

	public static readonly Emoji SmallRedTriangle = new Emoji("1F53A", "UP-POINTING RED TRIANGLE", "\ud83d\udd3a", "small_red_triangle", new string[1] { "small_red_triangle" }, null, null, "Symbols", 28, 8, SkinVariationType.None, "red triangle pointed up", new string[3] { "geometric", "red", "red triangle pointed up" });

	public static readonly Emoji SmallRedTriangleDown = new Emoji("1F53B", "DOWN-POINTING RED TRIANGLE", "\ud83d\udd3b", "small_red_triangle_down", new string[1] { "small_red_triangle_down" }, null, null, "Symbols", 28, 9, SkinVariationType.None, "red triangle pointed down", new string[4] { "down", "geometric", "red", "red triangle pointed down" });

	public static readonly Emoji DiamondShapeWithADotInside = new Emoji("1F4A0", "DIAMOND SHAPE WITH A DOT INSIDE", "\ud83d\udca0", "diamond_shape_with_a_dot_inside", new string[1] { "diamond_shape_with_a_dot_inside" }, null, null, "Symbols", 25, 6, SkinVariationType.None, "diamond with a dot", new string[5] { "comic", "diamond", "diamond with a dot", "geometric", "inside" });

	public static readonly Emoji RadioButton = new Emoji("1F518", "RADIO BUTTON", "\ud83d\udd18", "radio_button", new string[1] { "radio_button" }, null, null, "Symbols", 27, 26, SkinVariationType.None, "radio button", new string[3] { "button", "geometric", "radio" });

	public static readonly Emoji BlackSquareButton = new Emoji("1F532", "BLACK SQUARE BUTTON", "\ud83d\udd32", "black_square_button", new string[1] { "black_square_button" }, null, null, "Symbols", 28, 0, SkinVariationType.None, "black square button", new string[4] { "black square button", "button", "geometric", "square" });

	public static readonly Emoji WhiteSquareButton = new Emoji("1F533", "WHITE SQUARE BUTTON", "\ud83d\udd33", "white_square_button", new string[1] { "white_square_button" }, null, null, "Symbols", 28, 1, SkinVariationType.None, "white square button", new string[5] { "button", "geometric", "outlined", "square", "white square button" });

	public static readonly Emoji CheckeredFlag = new Emoji("1F3C1", "CHEQUERED FLAG", "\ud83c\udfc1", "checkered_flag", new string[1] { "checkered_flag" }, null, null, "Flags", 9, 27, SkinVariationType.None, "chequered flag", new string[4] { "checkered", "chequered", "chequered flag", "racing" });

	public static readonly Emoji TriangularFlagOnPost = new Emoji("1F6A9", "TRIANGULAR FLAG ON POST", "\ud83d\udea9", "triangular_flag_on_post", new string[1] { "triangular_flag_on_post" }, null, null, "Flags", 35, 14, SkinVariationType.None, "triangular flag", new string[2] { "post", "triangular flag" });

	public static readonly Emoji CrossedFlags = new Emoji("1F38C", "CROSSED FLAGS", "\ud83c\udf8c", "crossed_flags", new string[1] { "crossed_flags" }, null, null, "Flags", 8, 31, SkinVariationType.None, "crossed flags", new string[5] { "celebration", "cross", "crossed", "crossed flags", "Japanese" });

	public static readonly Emoji WavingBlackFlag = new Emoji("1F3F4", "WAVING BLACK FLAG", "\ud83c\udff4", "waving_black_flag", new string[1] { "waving_black_flag" }, null, null, "Flags", 12, 19, SkinVariationType.None, "black flag", new string[2] { "black flag", "waving" });

	public static readonly Emoji WavingWhiteFlag = new Emoji("1F3F3-FE0F", null, "\ud83c\udff3\ufe0f", "waving_white_flag", new string[1] { "waving_white_flag" }, null, null, "Flags", 12, 15, SkinVariationType.None, null, null);

	public static readonly Emoji RainbowFlag = new Emoji("1F3F3-FE0F-200D-1F308", null, "\ud83c\udff3\ufe0f\u200d\ud83c\udf08", "rainbow-flag", new string[1] { "rainbow-flag" }, null, null, "Flags", 12, 14, SkinVariationType.None, null, null);

	public static readonly Emoji FlagAc = new Emoji("1F1E6-1F1E8", "Ascension Island Flag", "\ud83c\udde6\ud83c\udde8", "flag-ac", new string[1] { "flag-ac" }, null, null, "Flags", 0, 31, SkinVariationType.None, null, null);

	public static readonly Emoji FlagAd = new Emoji("1F1E6-1F1E9", "Andorra Flag", "\ud83c\udde6\ud83c\udde9", "flag-ad", new string[1] { "flag-ad" }, null, null, "Flags", 0, 32, SkinVariationType.None, null, null);

	public static readonly Emoji FlagAe = new Emoji("1F1E6-1F1EA", "United Arab Emirates Flag", "\ud83c\udde6\ud83c\uddea", "flag-ae", new string[1] { "flag-ae" }, null, null, "Flags", 0, 33, SkinVariationType.None, null, null);

	public static readonly Emoji FlagAf = new Emoji("1F1E6-1F1EB", "Afghanistan Flag", "\ud83c\udde6\ud83c\uddeb", "flag-af", new string[1] { "flag-af" }, null, null, "Flags", 0, 34, SkinVariationType.None, null, null);

	public static readonly Emoji FlagAg = new Emoji("1F1E6-1F1EC", "Antigua & Barbuda Flag", "\ud83c\udde6\ud83c\uddec", "flag-ag", new string[1] { "flag-ag" }, null, null, "Flags", 0, 35, SkinVariationType.None, null, null);

	public static readonly Emoji FlagAi = new Emoji("1F1E6-1F1EE", "Anguilla Flag", "\ud83c\udde6\ud83c\uddee", "flag-ai", new string[1] { "flag-ai" }, null, null, "Flags", 0, 36, SkinVariationType.None, null, null);

	public static readonly Emoji FlagAl = new Emoji("1F1E6-1F1F1", "Albania Flag", "\ud83c\udde6\ud83c\uddf1", "flag-al", new string[1] { "flag-al" }, null, null, "Flags", 0, 37, SkinVariationType.None, null, null);

	public static readonly Emoji FlagAm = new Emoji("1F1E6-1F1F2", "Armenia Flag", "\ud83c\udde6\ud83c\uddf2", "flag-am", new string[1] { "flag-am" }, null, null, "Flags", 0, 38, SkinVariationType.None, null, null);

	public static readonly Emoji FlagAo = new Emoji("1F1E6-1F1F4", "Angola Flag", "\ud83c\udde6\ud83c\uddf4", "flag-ao", new string[1] { "flag-ao" }, null, null, "Flags", 0, 39, SkinVariationType.None, null, null);

	public static readonly Emoji FlagAq = new Emoji("1F1E6-1F1F6", "Antarctica Flag", "\ud83c\udde6\ud83c\uddf6", "flag-aq", new string[1] { "flag-aq" }, null, null, "Flags", 0, 40, SkinVariationType.None, null, null);

	public static readonly Emoji FlagAr = new Emoji("1F1E6-1F1F7", "Argentina Flag", "\ud83c\udde6\ud83c\uddf7", "flag-ar", new string[1] { "flag-ar" }, null, null, "Flags", 0, 41, SkinVariationType.None, null, null);

	public static readonly Emoji FlagAs = new Emoji("1F1E6-1F1F8", "American Samoa Flag", "\ud83c\udde6\ud83c\uddf8", "flag-as", new string[1] { "flag-as" }, null, null, "Flags", 0, 42, SkinVariationType.None, null, null);

	public static readonly Emoji FlagAt = new Emoji("1F1E6-1F1F9", "Austria Flag", "\ud83c\udde6\ud83c\uddf9", "flag-at", new string[1] { "flag-at" }, null, null, "Flags", 0, 43, SkinVariationType.None, null, null);

	public static readonly Emoji FlagAu = new Emoji("1F1E6-1F1FA", "Australia Flag", "\ud83c\udde6\ud83c\uddfa", "flag-au", new string[1] { "flag-au" }, null, null, "Flags", 0, 44, SkinVariationType.None, null, null);

	public static readonly Emoji FlagAw = new Emoji("1F1E6-1F1FC", "Aruba Flag", "\ud83c\udde6\ud83c\uddfc", "flag-aw", new string[1] { "flag-aw" }, null, null, "Flags", 0, 45, SkinVariationType.None, null, null);

	public static readonly Emoji FlagAx = new Emoji("1F1E6-1F1FD", "Åland Islands Flag", "\ud83c\udde6\ud83c\uddfd", "flag-ax", new string[1] { "flag-ax" }, null, null, "Flags", 0, 46, SkinVariationType.None, null, null);

	public static readonly Emoji FlagAz = new Emoji("1F1E6-1F1FF", "Azerbaijan Flag", "\ud83c\udde6\ud83c\uddff", "flag-az", new string[1] { "flag-az" }, null, null, "Flags", 0, 47, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBa = new Emoji("1F1E7-1F1E6", "Bosnia & Herzegovina Flag", "\ud83c\udde7\ud83c\udde6", "flag-ba", new string[1] { "flag-ba" }, null, null, "Flags", 0, 48, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBb = new Emoji("1F1E7-1F1E7", "Barbados Flag", "\ud83c\udde7\ud83c\udde7", "flag-bb", new string[1] { "flag-bb" }, null, null, "Flags", 0, 49, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBd = new Emoji("1F1E7-1F1E9", "Bangladesh Flag", "\ud83c\udde7\ud83c\udde9", "flag-bd", new string[1] { "flag-bd" }, null, null, "Flags", 0, 50, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBe = new Emoji("1F1E7-1F1EA", "Belgium Flag", "\ud83c\udde7\ud83c\uddea", "flag-be", new string[1] { "flag-be" }, null, null, "Flags", 0, 51, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBf = new Emoji("1F1E7-1F1EB", "Burkina Faso Flag", "\ud83c\udde7\ud83c\uddeb", "flag-bf", new string[1] { "flag-bf" }, null, null, "Flags", 1, 0, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBg = new Emoji("1F1E7-1F1EC", "Bulgaria Flag", "\ud83c\udde7\ud83c\uddec", "flag-bg", new string[1] { "flag-bg" }, null, null, "Flags", 1, 1, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBh = new Emoji("1F1E7-1F1ED", "Bahrain Flag", "\ud83c\udde7\ud83c\udded", "flag-bh", new string[1] { "flag-bh" }, null, null, "Flags", 1, 2, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBi = new Emoji("1F1E7-1F1EE", "Burundi Flag", "\ud83c\udde7\ud83c\uddee", "flag-bi", new string[1] { "flag-bi" }, null, null, "Flags", 1, 3, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBj = new Emoji("1F1E7-1F1EF", "Benin Flag", "\ud83c\udde7\ud83c\uddef", "flag-bj", new string[1] { "flag-bj" }, null, null, "Flags", 1, 4, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBl = new Emoji("1F1E7-1F1F1", "St. Barthélemy Flag", "\ud83c\udde7\ud83c\uddf1", "flag-bl", new string[1] { "flag-bl" }, null, null, "Flags", 1, 5, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBm = new Emoji("1F1E7-1F1F2", "Bermuda Flag", "\ud83c\udde7\ud83c\uddf2", "flag-bm", new string[1] { "flag-bm" }, null, null, "Flags", 1, 6, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBn = new Emoji("1F1E7-1F1F3", "Brunei Flag", "\ud83c\udde7\ud83c\uddf3", "flag-bn", new string[1] { "flag-bn" }, null, null, "Flags", 1, 7, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBo = new Emoji("1F1E7-1F1F4", "Bolivia Flag", "\ud83c\udde7\ud83c\uddf4", "flag-bo", new string[1] { "flag-bo" }, null, null, "Flags", 1, 8, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBq = new Emoji("1F1E7-1F1F6", "Caribbean Netherlands Flag", "\ud83c\udde7\ud83c\uddf6", "flag-bq", new string[1] { "flag-bq" }, null, null, "Flags", 1, 9, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBr = new Emoji("1F1E7-1F1F7", "Brazil Flag", "\ud83c\udde7\ud83c\uddf7", "flag-br", new string[1] { "flag-br" }, null, null, "Flags", 1, 10, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBs = new Emoji("1F1E7-1F1F8", "Bahamas Flag", "\ud83c\udde7\ud83c\uddf8", "flag-bs", new string[1] { "flag-bs" }, null, null, "Flags", 1, 11, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBt = new Emoji("1F1E7-1F1F9", "Bhutan Flag", "\ud83c\udde7\ud83c\uddf9", "flag-bt", new string[1] { "flag-bt" }, null, null, "Flags", 1, 12, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBv = new Emoji("1F1E7-1F1FB", "Bouvet Island Flag", "\ud83c\udde7\ud83c\uddfb", "flag-bv", new string[1] { "flag-bv" }, null, null, "Flags", 1, 13, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBw = new Emoji("1F1E7-1F1FC", "Botswana Flag", "\ud83c\udde7\ud83c\uddfc", "flag-bw", new string[1] { "flag-bw" }, null, null, "Flags", 1, 14, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBy = new Emoji("1F1E7-1F1FE", "Belarus Flag", "\ud83c\udde7\ud83c\uddfe", "flag-by", new string[1] { "flag-by" }, null, null, "Flags", 1, 15, SkinVariationType.None, null, null);

	public static readonly Emoji FlagBz = new Emoji("1F1E7-1F1FF", "Belize Flag", "\ud83c\udde7\ud83c\uddff", "flag-bz", new string[1] { "flag-bz" }, null, null, "Flags", 1, 16, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCa = new Emoji("1F1E8-1F1E6", "Canada Flag", "\ud83c\udde8\ud83c\udde6", "flag-ca", new string[1] { "flag-ca" }, null, null, "Flags", 1, 17, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCc = new Emoji("1F1E8-1F1E8", "Cocos (Keeling) Islands Flag", "\ud83c\udde8\ud83c\udde8", "flag-cc", new string[1] { "flag-cc" }, null, null, "Flags", 1, 18, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCd = new Emoji("1F1E8-1F1E9", "Congo - Kinshasa Flag", "\ud83c\udde8\ud83c\udde9", "flag-cd", new string[1] { "flag-cd" }, null, null, "Flags", 1, 19, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCf = new Emoji("1F1E8-1F1EB", "Central African Republic Flag", "\ud83c\udde8\ud83c\uddeb", "flag-cf", new string[1] { "flag-cf" }, null, null, "Flags", 1, 20, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCg = new Emoji("1F1E8-1F1EC", "Congo - Brazzaville Flag", "\ud83c\udde8\ud83c\uddec", "flag-cg", new string[1] { "flag-cg" }, null, null, "Flags", 1, 21, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCh = new Emoji("1F1E8-1F1ED", "Switzerland Flag", "\ud83c\udde8\ud83c\udded", "flag-ch", new string[1] { "flag-ch" }, null, null, "Flags", 1, 22, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCi = new Emoji("1F1E8-1F1EE", "Côte d’Ivoire Flag", "\ud83c\udde8\ud83c\uddee", "flag-ci", new string[1] { "flag-ci" }, null, null, "Flags", 1, 23, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCk = new Emoji("1F1E8-1F1F0", "Cook Islands Flag", "\ud83c\udde8\ud83c\uddf0", "flag-ck", new string[1] { "flag-ck" }, null, null, "Flags", 1, 24, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCl = new Emoji("1F1E8-1F1F1", "Chile Flag", "\ud83c\udde8\ud83c\uddf1", "flag-cl", new string[1] { "flag-cl" }, null, null, "Flags", 1, 25, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCm = new Emoji("1F1E8-1F1F2", "Cameroon Flag", "\ud83c\udde8\ud83c\uddf2", "flag-cm", new string[1] { "flag-cm" }, null, null, "Flags", 1, 26, SkinVariationType.None, null, null);

	public static readonly Emoji Cn = new Emoji("1F1E8-1F1F3", "China Flag", "\ud83c\udde8\ud83c\uddf3", "cn", new string[2] { "cn", "flag-cn" }, null, null, "Flags", 1, 27, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCo = new Emoji("1F1E8-1F1F4", "Colombia Flag", "\ud83c\udde8\ud83c\uddf4", "flag-co", new string[1] { "flag-co" }, null, null, "Flags", 1, 28, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCp = new Emoji("1F1E8-1F1F5", "Clipperton Island Flag", "\ud83c\udde8\ud83c\uddf5", "flag-cp", new string[1] { "flag-cp" }, null, null, "Flags", 1, 29, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCr = new Emoji("1F1E8-1F1F7", "Costa Rica Flag", "\ud83c\udde8\ud83c\uddf7", "flag-cr", new string[1] { "flag-cr" }, null, null, "Flags", 1, 30, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCu = new Emoji("1F1E8-1F1FA", "Cuba Flag", "\ud83c\udde8\ud83c\uddfa", "flag-cu", new string[1] { "flag-cu" }, null, null, "Flags", 1, 31, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCv = new Emoji("1F1E8-1F1FB", "Cape Verde Flag", "\ud83c\udde8\ud83c\uddfb", "flag-cv", new string[1] { "flag-cv" }, null, null, "Flags", 1, 32, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCw = new Emoji("1F1E8-1F1FC", "Curaçao Flag", "\ud83c\udde8\ud83c\uddfc", "flag-cw", new string[1] { "flag-cw" }, null, null, "Flags", 1, 33, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCx = new Emoji("1F1E8-1F1FD", "Christmas Island Flag", "\ud83c\udde8\ud83c\uddfd", "flag-cx", new string[1] { "flag-cx" }, null, null, "Flags", 1, 34, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCy = new Emoji("1F1E8-1F1FE", "Cyprus Flag", "\ud83c\udde8\ud83c\uddfe", "flag-cy", new string[1] { "flag-cy" }, null, null, "Flags", 1, 35, SkinVariationType.None, null, null);

	public static readonly Emoji FlagCz = new Emoji("1F1E8-1F1FF", "Czechia Flag", "\ud83c\udde8\ud83c\uddff", "flag-cz", new string[1] { "flag-cz" }, null, null, "Flags", 1, 36, SkinVariationType.None, null, null);

	public static readonly Emoji De = new Emoji("1F1E9-1F1EA", "Germany Flag", "\ud83c\udde9\ud83c\uddea", "de", new string[2] { "de", "flag-de" }, null, null, "Flags", 1, 37, SkinVariationType.None, null, null);

	public static readonly Emoji FlagDg = new Emoji("1F1E9-1F1EC", "Diego Garcia Flag", "\ud83c\udde9\ud83c\uddec", "flag-dg", new string[1] { "flag-dg" }, null, null, "Flags", 1, 38, SkinVariationType.None, null, null);

	public static readonly Emoji FlagDj = new Emoji("1F1E9-1F1EF", "Djibouti Flag", "\ud83c\udde9\ud83c\uddef", "flag-dj", new string[1] { "flag-dj" }, null, null, "Flags", 1, 39, SkinVariationType.None, null, null);

	public static readonly Emoji FlagDk = new Emoji("1F1E9-1F1F0", "Denmark Flag", "\ud83c\udde9\ud83c\uddf0", "flag-dk", new string[1] { "flag-dk" }, null, null, "Flags", 1, 40, SkinVariationType.None, null, null);

	public static readonly Emoji FlagDm = new Emoji("1F1E9-1F1F2", "Dominica Flag", "\ud83c\udde9\ud83c\uddf2", "flag-dm", new string[1] { "flag-dm" }, null, null, "Flags", 1, 41, SkinVariationType.None, null, null);

	public static readonly Emoji FlagDo = new Emoji("1F1E9-1F1F4", "Dominican Republic Flag", "\ud83c\udde9\ud83c\uddf4", "flag-do", new string[1] { "flag-do" }, null, null, "Flags", 1, 42, SkinVariationType.None, null, null);

	public static readonly Emoji FlagDz = new Emoji("1F1E9-1F1FF", "Algeria Flag", "\ud83c\udde9\ud83c\uddff", "flag-dz", new string[1] { "flag-dz" }, null, null, "Flags", 1, 43, SkinVariationType.None, null, null);

	public static readonly Emoji FlagEa = new Emoji("1F1EA-1F1E6", "Ceuta & Melilla Flag", "\ud83c\uddea\ud83c\udde6", "flag-ea", new string[1] { "flag-ea" }, null, null, "Flags", 1, 44, SkinVariationType.None, null, null);

	public static readonly Emoji FlagEc = new Emoji("1F1EA-1F1E8", "Ecuador Flag", "\ud83c\uddea\ud83c\udde8", "flag-ec", new string[1] { "flag-ec" }, null, null, "Flags", 1, 45, SkinVariationType.None, null, null);

	public static readonly Emoji FlagEe = new Emoji("1F1EA-1F1EA", "Estonia Flag", "\ud83c\uddea\ud83c\uddea", "flag-ee", new string[1] { "flag-ee" }, null, null, "Flags", 1, 46, SkinVariationType.None, null, null);

	public static readonly Emoji FlagEg = new Emoji("1F1EA-1F1EC", "Egypt Flag", "\ud83c\uddea\ud83c\uddec", "flag-eg", new string[1] { "flag-eg" }, null, null, "Flags", 1, 47, SkinVariationType.None, null, null);

	public static readonly Emoji FlagEh = new Emoji("1F1EA-1F1ED", "Western Sahara Flag", "\ud83c\uddea\ud83c\udded", "flag-eh", new string[1] { "flag-eh" }, null, null, "Flags", 1, 48, SkinVariationType.None, null, null);

	public static readonly Emoji FlagEr = new Emoji("1F1EA-1F1F7", "Eritrea Flag", "\ud83c\uddea\ud83c\uddf7", "flag-er", new string[1] { "flag-er" }, null, null, "Flags", 1, 49, SkinVariationType.None, null, null);

	public static readonly Emoji Es = new Emoji("1F1EA-1F1F8", "Spain Flag", "\ud83c\uddea\ud83c\uddf8", "es", new string[2] { "es", "flag-es" }, null, null, "Flags", 1, 50, SkinVariationType.None, null, null);

	public static readonly Emoji FlagEt = new Emoji("1F1EA-1F1F9", "Ethiopia Flag", "\ud83c\uddea\ud83c\uddf9", "flag-et", new string[1] { "flag-et" }, null, null, "Flags", 1, 51, SkinVariationType.None, null, null);

	public static readonly Emoji FlagEu = new Emoji("1F1EA-1F1FA", "European Union Flag", "\ud83c\uddea\ud83c\uddfa", "flag-eu", new string[1] { "flag-eu" }, null, null, "Flags", 2, 0, SkinVariationType.None, null, null);

	public static readonly Emoji FlagFi = new Emoji("1F1EB-1F1EE", "Finland Flag", "\ud83c\uddeb\ud83c\uddee", "flag-fi", new string[1] { "flag-fi" }, null, null, "Flags", 2, 1, SkinVariationType.None, null, null);

	public static readonly Emoji FlagFj = new Emoji("1F1EB-1F1EF", "Fiji Flag", "\ud83c\uddeb\ud83c\uddef", "flag-fj", new string[1] { "flag-fj" }, null, null, "Flags", 2, 2, SkinVariationType.None, null, null);

	public static readonly Emoji FlagFk = new Emoji("1F1EB-1F1F0", "Falkland Islands Flag", "\ud83c\uddeb\ud83c\uddf0", "flag-fk", new string[1] { "flag-fk" }, null, null, "Flags", 2, 3, SkinVariationType.None, null, null);

	public static readonly Emoji FlagFm = new Emoji("1F1EB-1F1F2", "Micronesia Flag", "\ud83c\uddeb\ud83c\uddf2", "flag-fm", new string[1] { "flag-fm" }, null, null, "Flags", 2, 4, SkinVariationType.None, null, null);

	public static readonly Emoji FlagFo = new Emoji("1F1EB-1F1F4", "Faroe Islands Flag", "\ud83c\uddeb\ud83c\uddf4", "flag-fo", new string[1] { "flag-fo" }, null, null, "Flags", 2, 5, SkinVariationType.None, null, null);

	public static readonly Emoji Fr = new Emoji("1F1EB-1F1F7", "France Flag", "\ud83c\uddeb\ud83c\uddf7", "fr", new string[2] { "fr", "flag-fr" }, null, null, "Flags", 2, 6, SkinVariationType.None, null, null);

	public static readonly Emoji FlagGa = new Emoji("1F1EC-1F1E6", "Gabon Flag", "\ud83c\uddec\ud83c\udde6", "flag-ga", new string[1] { "flag-ga" }, null, null, "Flags", 2, 7, SkinVariationType.None, null, null);

	public static readonly Emoji Gb = new Emoji("1F1EC-1F1E7", "United Kingdom Flag", "\ud83c\uddec\ud83c\udde7", "gb", new string[3] { "gb", "uk", "flag-gb" }, null, null, "Flags", 2, 8, SkinVariationType.None, null, null);

	public static readonly Emoji FlagGd = new Emoji("1F1EC-1F1E9", "Grenada Flag", "\ud83c\uddec\ud83c\udde9", "flag-gd", new string[1] { "flag-gd" }, null, null, "Flags", 2, 9, SkinVariationType.None, null, null);

	public static readonly Emoji FlagGe = new Emoji("1F1EC-1F1EA", "Georgia Flag", "\ud83c\uddec\ud83c\uddea", "flag-ge", new string[1] { "flag-ge" }, null, null, "Flags", 2, 10, SkinVariationType.None, null, null);

	public static readonly Emoji FlagGf = new Emoji("1F1EC-1F1EB", "French Guiana Flag", "\ud83c\uddec\ud83c\uddeb", "flag-gf", new string[1] { "flag-gf" }, null, null, "Flags", 2, 11, SkinVariationType.None, null, null);

	public static readonly Emoji FlagGg = new Emoji("1F1EC-1F1EC", "Guernsey Flag", "\ud83c\uddec\ud83c\uddec", "flag-gg", new string[1] { "flag-gg" }, null, null, "Flags", 2, 12, SkinVariationType.None, null, null);

	public static readonly Emoji FlagGh = new Emoji("1F1EC-1F1ED", "Ghana Flag", "\ud83c\uddec\ud83c\udded", "flag-gh", new string[1] { "flag-gh" }, null, null, "Flags", 2, 13, SkinVariationType.None, null, null);

	public static readonly Emoji FlagGi = new Emoji("1F1EC-1F1EE", "Gibraltar Flag", "\ud83c\uddec\ud83c\uddee", "flag-gi", new string[1] { "flag-gi" }, null, null, "Flags", 2, 14, SkinVariationType.None, null, null);

	public static readonly Emoji FlagGl = new Emoji("1F1EC-1F1F1", "Greenland Flag", "\ud83c\uddec\ud83c\uddf1", "flag-gl", new string[1] { "flag-gl" }, null, null, "Flags", 2, 15, SkinVariationType.None, null, null);

	public static readonly Emoji FlagGm = new Emoji("1F1EC-1F1F2", "Gambia Flag", "\ud83c\uddec\ud83c\uddf2", "flag-gm", new string[1] { "flag-gm" }, null, null, "Flags", 2, 16, SkinVariationType.None, null, null);

	public static readonly Emoji FlagGn = new Emoji("1F1EC-1F1F3", "Guinea Flag", "\ud83c\uddec\ud83c\uddf3", "flag-gn", new string[1] { "flag-gn" }, null, null, "Flags", 2, 17, SkinVariationType.None, null, null);

	public static readonly Emoji FlagGp = new Emoji("1F1EC-1F1F5", "Guadeloupe Flag", "\ud83c\uddec\ud83c\uddf5", "flag-gp", new string[1] { "flag-gp" }, null, null, "Flags", 2, 18, SkinVariationType.None, null, null);

	public static readonly Emoji FlagGq = new Emoji("1F1EC-1F1F6", "Equatorial Guinea Flag", "\ud83c\uddec\ud83c\uddf6", "flag-gq", new string[1] { "flag-gq" }, null, null, "Flags", 2, 19, SkinVariationType.None, null, null);

	public static readonly Emoji FlagGr = new Emoji("1F1EC-1F1F7", "Greece Flag", "\ud83c\uddec\ud83c\uddf7", "flag-gr", new string[1] { "flag-gr" }, null, null, "Flags", 2, 20, SkinVariationType.None, null, null);

	public static readonly Emoji FlagGs = new Emoji("1F1EC-1F1F8", "South Georgia & South Sandwich Islands Flag", "\ud83c\uddec\ud83c\uddf8", "flag-gs", new string[1] { "flag-gs" }, null, null, "Flags", 2, 21, SkinVariationType.None, null, null);

	public static readonly Emoji FlagGt = new Emoji("1F1EC-1F1F9", "Guatemala Flag", "\ud83c\uddec\ud83c\uddf9", "flag-gt", new string[1] { "flag-gt" }, null, null, "Flags", 2, 22, SkinVariationType.None, null, null);

	public static readonly Emoji FlagGu = new Emoji("1F1EC-1F1FA", "Guam Flag", "\ud83c\uddec\ud83c\uddfa", "flag-gu", new string[1] { "flag-gu" }, null, null, "Flags", 2, 23, SkinVariationType.None, null, null);

	public static readonly Emoji FlagGw = new Emoji("1F1EC-1F1FC", "Guinea-Bissau Flag", "\ud83c\uddec\ud83c\uddfc", "flag-gw", new string[1] { "flag-gw" }, null, null, "Flags", 2, 24, SkinVariationType.None, null, null);

	public static readonly Emoji FlagGy = new Emoji("1F1EC-1F1FE", "Guyana Flag", "\ud83c\uddec\ud83c\uddfe", "flag-gy", new string[1] { "flag-gy" }, null, null, "Flags", 2, 25, SkinVariationType.None, null, null);

	public static readonly Emoji FlagHk = new Emoji("1F1ED-1F1F0", "Hong Kong SAR China Flag", "\ud83c\udded\ud83c\uddf0", "flag-hk", new string[1] { "flag-hk" }, null, null, "Flags", 2, 26, SkinVariationType.None, null, null);

	public static readonly Emoji FlagHm = new Emoji("1F1ED-1F1F2", "Heard & McDonald Islands Flag", "\ud83c\udded\ud83c\uddf2", "flag-hm", new string[1] { "flag-hm" }, null, null, "Flags", 2, 27, SkinVariationType.None, null, null);

	public static readonly Emoji FlagHn = new Emoji("1F1ED-1F1F3", "Honduras Flag", "\ud83c\udded\ud83c\uddf3", "flag-hn", new string[1] { "flag-hn" }, null, null, "Flags", 2, 28, SkinVariationType.None, null, null);

	public static readonly Emoji FlagHr = new Emoji("1F1ED-1F1F7", "Croatia Flag", "\ud83c\udded\ud83c\uddf7", "flag-hr", new string[1] { "flag-hr" }, null, null, "Flags", 2, 29, SkinVariationType.None, null, null);

	public static readonly Emoji FlagHt = new Emoji("1F1ED-1F1F9", "Haiti Flag", "\ud83c\udded\ud83c\uddf9", "flag-ht", new string[1] { "flag-ht" }, null, null, "Flags", 2, 30, SkinVariationType.None, null, null);

	public static readonly Emoji FlagHu = new Emoji("1F1ED-1F1FA", "Hungary Flag", "\ud83c\udded\ud83c\uddfa", "flag-hu", new string[1] { "flag-hu" }, null, null, "Flags", 2, 31, SkinVariationType.None, null, null);

	public static readonly Emoji FlagIc = new Emoji("1F1EE-1F1E8", "Canary Islands Flag", "\ud83c\uddee\ud83c\udde8", "flag-ic", new string[1] { "flag-ic" }, null, null, "Flags", 2, 32, SkinVariationType.None, null, null);

	public static readonly Emoji FlagId = new Emoji("1F1EE-1F1E9", "Indonesia Flag", "\ud83c\uddee\ud83c\udde9", "flag-id", new string[1] { "flag-id" }, null, null, "Flags", 2, 33, SkinVariationType.None, null, null);

	public static readonly Emoji FlagIe = new Emoji("1F1EE-1F1EA", "Ireland Flag", "\ud83c\uddee\ud83c\uddea", "flag-ie", new string[1] { "flag-ie" }, null, null, "Flags", 2, 34, SkinVariationType.None, null, null);

	public static readonly Emoji FlagIl = new Emoji("1F1EE-1F1F1", "Israel Flag", "\ud83c\uddee\ud83c\uddf1", "flag-il", new string[1] { "flag-il" }, null, null, "Flags", 2, 35, SkinVariationType.None, null, null);

	public static readonly Emoji FlagIm = new Emoji("1F1EE-1F1F2", "Isle of Man Flag", "\ud83c\uddee\ud83c\uddf2", "flag-im", new string[1] { "flag-im" }, null, null, "Flags", 2, 36, SkinVariationType.None, null, null);

	public static readonly Emoji FlagIn = new Emoji("1F1EE-1F1F3", "India Flag", "\ud83c\uddee\ud83c\uddf3", "flag-in", new string[1] { "flag-in" }, null, null, "Flags", 2, 37, SkinVariationType.None, null, null);

	public static readonly Emoji FlagIo = new Emoji("1F1EE-1F1F4", "British Indian Ocean Territory Flag", "\ud83c\uddee\ud83c\uddf4", "flag-io", new string[1] { "flag-io" }, null, null, "Flags", 2, 38, SkinVariationType.None, null, null);

	public static readonly Emoji FlagIq = new Emoji("1F1EE-1F1F6", "Iraq Flag", "\ud83c\uddee\ud83c\uddf6", "flag-iq", new string[1] { "flag-iq" }, null, null, "Flags", 2, 39, SkinVariationType.None, null, null);

	public static readonly Emoji FlagIr = new Emoji("1F1EE-1F1F7", "Iran Flag", "\ud83c\uddee\ud83c\uddf7", "flag-ir", new string[1] { "flag-ir" }, null, null, "Flags", 2, 40, SkinVariationType.None, null, null);

	public static readonly Emoji FlagIs = new Emoji("1F1EE-1F1F8", "Iceland Flag", "\ud83c\uddee\ud83c\uddf8", "flag-is", new string[1] { "flag-is" }, null, null, "Flags", 2, 41, SkinVariationType.None, null, null);

	public static readonly Emoji It = new Emoji("1F1EE-1F1F9", "Italy Flag", "\ud83c\uddee\ud83c\uddf9", "it", new string[2] { "it", "flag-it" }, null, null, "Flags", 2, 42, SkinVariationType.None, null, null);

	public static readonly Emoji FlagJe = new Emoji("1F1EF-1F1EA", "Jersey Flag", "\ud83c\uddef\ud83c\uddea", "flag-je", new string[1] { "flag-je" }, null, null, "Flags", 2, 43, SkinVariationType.None, null, null);

	public static readonly Emoji FlagJm = new Emoji("1F1EF-1F1F2", "Jamaica Flag", "\ud83c\uddef\ud83c\uddf2", "flag-jm", new string[1] { "flag-jm" }, null, null, "Flags", 2, 44, SkinVariationType.None, null, null);

	public static readonly Emoji FlagJo = new Emoji("1F1EF-1F1F4", "Jordan Flag", "\ud83c\uddef\ud83c\uddf4", "flag-jo", new string[1] { "flag-jo" }, null, null, "Flags", 2, 45, SkinVariationType.None, null, null);

	public static readonly Emoji Jp = new Emoji("1F1EF-1F1F5", "Japan Flag", "\ud83c\uddef\ud83c\uddf5", "jp", new string[2] { "jp", "flag-jp" }, null, null, "Flags", 2, 46, SkinVariationType.None, null, null);

	public static readonly Emoji FlagKe = new Emoji("1F1F0-1F1EA", "Kenya Flag", "\ud83c\uddf0\ud83c\uddea", "flag-ke", new string[1] { "flag-ke" }, null, null, "Flags", 2, 47, SkinVariationType.None, null, null);

	public static readonly Emoji FlagKg = new Emoji("1F1F0-1F1EC", "Kyrgyzstan Flag", "\ud83c\uddf0\ud83c\uddec", "flag-kg", new string[1] { "flag-kg" }, null, null, "Flags", 2, 48, SkinVariationType.None, null, null);

	public static readonly Emoji FlagKh = new Emoji("1F1F0-1F1ED", "Cambodia Flag", "\ud83c\uddf0\ud83c\udded", "flag-kh", new string[1] { "flag-kh" }, null, null, "Flags", 2, 49, SkinVariationType.None, null, null);

	public static readonly Emoji FlagKi = new Emoji("1F1F0-1F1EE", "Kiribati Flag", "\ud83c\uddf0\ud83c\uddee", "flag-ki", new string[1] { "flag-ki" }, null, null, "Flags", 2, 50, SkinVariationType.None, null, null);

	public static readonly Emoji FlagKm = new Emoji("1F1F0-1F1F2", "Comoros Flag", "\ud83c\uddf0\ud83c\uddf2", "flag-km", new string[1] { "flag-km" }, null, null, "Flags", 2, 51, SkinVariationType.None, null, null);

	public static readonly Emoji FlagKn = new Emoji("1F1F0-1F1F3", "St. Kitts & Nevis Flag", "\ud83c\uddf0\ud83c\uddf3", "flag-kn", new string[1] { "flag-kn" }, null, null, "Flags", 3, 0, SkinVariationType.None, null, null);

	public static readonly Emoji FlagKp = new Emoji("1F1F0-1F1F5", "North Korea Flag", "\ud83c\uddf0\ud83c\uddf5", "flag-kp", new string[1] { "flag-kp" }, null, null, "Flags", 3, 1, SkinVariationType.None, null, null);

	public static readonly Emoji Kr = new Emoji("1F1F0-1F1F7", "South Korea Flag", "\ud83c\uddf0\ud83c\uddf7", "kr", new string[2] { "kr", "flag-kr" }, null, null, "Flags", 3, 2, SkinVariationType.None, null, null);

	public static readonly Emoji FlagKw = new Emoji("1F1F0-1F1FC", "Kuwait Flag", "\ud83c\uddf0\ud83c\uddfc", "flag-kw", new string[1] { "flag-kw" }, null, null, "Flags", 3, 3, SkinVariationType.None, null, null);

	public static readonly Emoji FlagKy = new Emoji("1F1F0-1F1FE", "Cayman Islands Flag", "\ud83c\uddf0\ud83c\uddfe", "flag-ky", new string[1] { "flag-ky" }, null, null, "Flags", 3, 4, SkinVariationType.None, null, null);

	public static readonly Emoji FlagKz = new Emoji("1F1F0-1F1FF", "Kazakhstan Flag", "\ud83c\uddf0\ud83c\uddff", "flag-kz", new string[1] { "flag-kz" }, null, null, "Flags", 3, 5, SkinVariationType.None, null, null);

	public static readonly Emoji FlagLa = new Emoji("1F1F1-1F1E6", "Laos Flag", "\ud83c\uddf1\ud83c\udde6", "flag-la", new string[1] { "flag-la" }, null, null, "Flags", 3, 6, SkinVariationType.None, null, null);

	public static readonly Emoji FlagLb = new Emoji("1F1F1-1F1E7", "Lebanon Flag", "\ud83c\uddf1\ud83c\udde7", "flag-lb", new string[1] { "flag-lb" }, null, null, "Flags", 3, 7, SkinVariationType.None, null, null);

	public static readonly Emoji FlagLc = new Emoji("1F1F1-1F1E8", "St. Lucia Flag", "\ud83c\uddf1\ud83c\udde8", "flag-lc", new string[1] { "flag-lc" }, null, null, "Flags", 3, 8, SkinVariationType.None, null, null);

	public static readonly Emoji FlagLi = new Emoji("1F1F1-1F1EE", "Liechtenstein Flag", "\ud83c\uddf1\ud83c\uddee", "flag-li", new string[1] { "flag-li" }, null, null, "Flags", 3, 9, SkinVariationType.None, null, null);

	public static readonly Emoji FlagLk = new Emoji("1F1F1-1F1F0", "Sri Lanka Flag", "\ud83c\uddf1\ud83c\uddf0", "flag-lk", new string[1] { "flag-lk" }, null, null, "Flags", 3, 10, SkinVariationType.None, null, null);

	public static readonly Emoji FlagLr = new Emoji("1F1F1-1F1F7", "Liberia Flag", "\ud83c\uddf1\ud83c\uddf7", "flag-lr", new string[1] { "flag-lr" }, null, null, "Flags", 3, 11, SkinVariationType.None, null, null);

	public static readonly Emoji FlagLs = new Emoji("1F1F1-1F1F8", "Lesotho Flag", "\ud83c\uddf1\ud83c\uddf8", "flag-ls", new string[1] { "flag-ls" }, null, null, "Flags", 3, 12, SkinVariationType.None, null, null);

	public static readonly Emoji FlagLt = new Emoji("1F1F1-1F1F9", "Lithuania Flag", "\ud83c\uddf1\ud83c\uddf9", "flag-lt", new string[1] { "flag-lt" }, null, null, "Flags", 3, 13, SkinVariationType.None, null, null);

	public static readonly Emoji FlagLu = new Emoji("1F1F1-1F1FA", "Luxembourg Flag", "\ud83c\uddf1\ud83c\uddfa", "flag-lu", new string[1] { "flag-lu" }, null, null, "Flags", 3, 14, SkinVariationType.None, null, null);

	public static readonly Emoji FlagLv = new Emoji("1F1F1-1F1FB", "Latvia Flag", "\ud83c\uddf1\ud83c\uddfb", "flag-lv", new string[1] { "flag-lv" }, null, null, "Flags", 3, 15, SkinVariationType.None, null, null);

	public static readonly Emoji FlagLy = new Emoji("1F1F1-1F1FE", "Libya Flag", "\ud83c\uddf1\ud83c\uddfe", "flag-ly", new string[1] { "flag-ly" }, null, null, "Flags", 3, 16, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMa = new Emoji("1F1F2-1F1E6", "Morocco Flag", "\ud83c\uddf2\ud83c\udde6", "flag-ma", new string[1] { "flag-ma" }, null, null, "Flags", 3, 17, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMc = new Emoji("1F1F2-1F1E8", "Monaco Flag", "\ud83c\uddf2\ud83c\udde8", "flag-mc", new string[1] { "flag-mc" }, null, null, "Flags", 3, 18, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMd = new Emoji("1F1F2-1F1E9", "Moldova Flag", "\ud83c\uddf2\ud83c\udde9", "flag-md", new string[1] { "flag-md" }, null, null, "Flags", 3, 19, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMe = new Emoji("1F1F2-1F1EA", "Montenegro Flag", "\ud83c\uddf2\ud83c\uddea", "flag-me", new string[1] { "flag-me" }, null, null, "Flags", 3, 20, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMf = new Emoji("1F1F2-1F1EB", "St. Martin Flag", "\ud83c\uddf2\ud83c\uddeb", "flag-mf", new string[1] { "flag-mf" }, null, null, "Flags", 3, 21, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMg = new Emoji("1F1F2-1F1EC", "Madagascar Flag", "\ud83c\uddf2\ud83c\uddec", "flag-mg", new string[1] { "flag-mg" }, null, null, "Flags", 3, 22, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMh = new Emoji("1F1F2-1F1ED", "Marshall Islands Flag", "\ud83c\uddf2\ud83c\udded", "flag-mh", new string[1] { "flag-mh" }, null, null, "Flags", 3, 23, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMk = new Emoji("1F1F2-1F1F0", "Macedonia Flag", "\ud83c\uddf2\ud83c\uddf0", "flag-mk", new string[1] { "flag-mk" }, null, null, "Flags", 3, 24, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMl = new Emoji("1F1F2-1F1F1", "Mali Flag", "\ud83c\uddf2\ud83c\uddf1", "flag-ml", new string[1] { "flag-ml" }, null, null, "Flags", 3, 25, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMm = new Emoji("1F1F2-1F1F2", "Myanmar (Burma) Flag", "\ud83c\uddf2\ud83c\uddf2", "flag-mm", new string[1] { "flag-mm" }, null, null, "Flags", 3, 26, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMn = new Emoji("1F1F2-1F1F3", "Mongolia Flag", "\ud83c\uddf2\ud83c\uddf3", "flag-mn", new string[1] { "flag-mn" }, null, null, "Flags", 3, 27, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMo = new Emoji("1F1F2-1F1F4", "Macau SAR China Flag", "\ud83c\uddf2\ud83c\uddf4", "flag-mo", new string[1] { "flag-mo" }, null, null, "Flags", 3, 28, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMp = new Emoji("1F1F2-1F1F5", "Northern Mariana Islands Flag", "\ud83c\uddf2\ud83c\uddf5", "flag-mp", new string[1] { "flag-mp" }, null, null, "Flags", 3, 29, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMq = new Emoji("1F1F2-1F1F6", "Martinique Flag", "\ud83c\uddf2\ud83c\uddf6", "flag-mq", new string[1] { "flag-mq" }, null, null, "Flags", 3, 30, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMr = new Emoji("1F1F2-1F1F7", "Mauritania Flag", "\ud83c\uddf2\ud83c\uddf7", "flag-mr", new string[1] { "flag-mr" }, null, null, "Flags", 3, 31, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMs = new Emoji("1F1F2-1F1F8", "Montserrat Flag", "\ud83c\uddf2\ud83c\uddf8", "flag-ms", new string[1] { "flag-ms" }, null, null, "Flags", 3, 32, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMt = new Emoji("1F1F2-1F1F9", "Malta Flag", "\ud83c\uddf2\ud83c\uddf9", "flag-mt", new string[1] { "flag-mt" }, null, null, "Flags", 3, 33, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMu = new Emoji("1F1F2-1F1FA", "Mauritius Flag", "\ud83c\uddf2\ud83c\uddfa", "flag-mu", new string[1] { "flag-mu" }, null, null, "Flags", 3, 34, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMv = new Emoji("1F1F2-1F1FB", "Maldives Flag", "\ud83c\uddf2\ud83c\uddfb", "flag-mv", new string[1] { "flag-mv" }, null, null, "Flags", 3, 35, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMw = new Emoji("1F1F2-1F1FC", "Malawi Flag", "\ud83c\uddf2\ud83c\uddfc", "flag-mw", new string[1] { "flag-mw" }, null, null, "Flags", 3, 36, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMx = new Emoji("1F1F2-1F1FD", "Mexico Flag", "\ud83c\uddf2\ud83c\uddfd", "flag-mx", new string[1] { "flag-mx" }, null, null, "Flags", 3, 37, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMy = new Emoji("1F1F2-1F1FE", "Malaysia Flag", "\ud83c\uddf2\ud83c\uddfe", "flag-my", new string[1] { "flag-my" }, null, null, "Flags", 3, 38, SkinVariationType.None, null, null);

	public static readonly Emoji FlagMz = new Emoji("1F1F2-1F1FF", "Mozambique Flag", "\ud83c\uddf2\ud83c\uddff", "flag-mz", new string[1] { "flag-mz" }, null, null, "Flags", 3, 39, SkinVariationType.None, null, null);

	public static readonly Emoji FlagNa = new Emoji("1F1F3-1F1E6", "Namibia Flag", "\ud83c\uddf3\ud83c\udde6", "flag-na", new string[1] { "flag-na" }, null, null, "Flags", 3, 40, SkinVariationType.None, null, null);

	public static readonly Emoji FlagNc = new Emoji("1F1F3-1F1E8", "New Caledonia Flag", "\ud83c\uddf3\ud83c\udde8", "flag-nc", new string[1] { "flag-nc" }, null, null, "Flags", 3, 41, SkinVariationType.None, null, null);

	public static readonly Emoji FlagNe = new Emoji("1F1F3-1F1EA", "Niger Flag", "\ud83c\uddf3\ud83c\uddea", "flag-ne", new string[1] { "flag-ne" }, null, null, "Flags", 3, 42, SkinVariationType.None, null, null);

	public static readonly Emoji FlagNf = new Emoji("1F1F3-1F1EB", "Norfolk Island Flag", "\ud83c\uddf3\ud83c\uddeb", "flag-nf", new string[1] { "flag-nf" }, null, null, "Flags", 3, 43, SkinVariationType.None, null, null);

	public static readonly Emoji FlagNg = new Emoji("1F1F3-1F1EC", "Nigeria Flag", "\ud83c\uddf3\ud83c\uddec", "flag-ng", new string[1] { "flag-ng" }, null, null, "Flags", 3, 44, SkinVariationType.None, null, null);

	public static readonly Emoji FlagNi = new Emoji("1F1F3-1F1EE", "Nicaragua Flag", "\ud83c\uddf3\ud83c\uddee", "flag-ni", new string[1] { "flag-ni" }, null, null, "Flags", 3, 45, SkinVariationType.None, null, null);

	public static readonly Emoji FlagNl = new Emoji("1F1F3-1F1F1", "Netherlands Flag", "\ud83c\uddf3\ud83c\uddf1", "flag-nl", new string[1] { "flag-nl" }, null, null, "Flags", 3, 46, SkinVariationType.None, null, null);

	public static readonly Emoji FlagNo = new Emoji("1F1F3-1F1F4", "Norway Flag", "\ud83c\uddf3\ud83c\uddf4", "flag-no", new string[1] { "flag-no" }, null, null, "Flags", 3, 47, SkinVariationType.None, null, null);

	public static readonly Emoji FlagNp = new Emoji("1F1F3-1F1F5", "Nepal Flag", "\ud83c\uddf3\ud83c\uddf5", "flag-np", new string[1] { "flag-np" }, null, null, "Flags", 3, 48, SkinVariationType.None, null, null);

	public static readonly Emoji FlagNr = new Emoji("1F1F3-1F1F7", "Nauru Flag", "\ud83c\uddf3\ud83c\uddf7", "flag-nr", new string[1] { "flag-nr" }, null, null, "Flags", 3, 49, SkinVariationType.None, null, null);

	public static readonly Emoji FlagNu = new Emoji("1F1F3-1F1FA", "Niue Flag", "\ud83c\uddf3\ud83c\uddfa", "flag-nu", new string[1] { "flag-nu" }, null, null, "Flags", 3, 50, SkinVariationType.None, null, null);

	public static readonly Emoji FlagNz = new Emoji("1F1F3-1F1FF", "New Zealand Flag", "\ud83c\uddf3\ud83c\uddff", "flag-nz", new string[1] { "flag-nz" }, null, null, "Flags", 3, 51, SkinVariationType.None, null, null);

	public static readonly Emoji FlagOm = new Emoji("1F1F4-1F1F2", "Oman Flag", "\ud83c\uddf4\ud83c\uddf2", "flag-om", new string[1] { "flag-om" }, null, null, "Flags", 4, 0, SkinVariationType.None, null, null);

	public static readonly Emoji FlagPa = new Emoji("1F1F5-1F1E6", "Panama Flag", "\ud83c\uddf5\ud83c\udde6", "flag-pa", new string[1] { "flag-pa" }, null, null, "Flags", 4, 1, SkinVariationType.None, null, null);

	public static readonly Emoji FlagPe = new Emoji("1F1F5-1F1EA", "Peru Flag", "\ud83c\uddf5\ud83c\uddea", "flag-pe", new string[1] { "flag-pe" }, null, null, "Flags", 4, 2, SkinVariationType.None, null, null);

	public static readonly Emoji FlagPf = new Emoji("1F1F5-1F1EB", "French Polynesia Flag", "\ud83c\uddf5\ud83c\uddeb", "flag-pf", new string[1] { "flag-pf" }, null, null, "Flags", 4, 3, SkinVariationType.None, null, null);

	public static readonly Emoji FlagPg = new Emoji("1F1F5-1F1EC", "Papua New Guinea Flag", "\ud83c\uddf5\ud83c\uddec", "flag-pg", new string[1] { "flag-pg" }, null, null, "Flags", 4, 4, SkinVariationType.None, null, null);

	public static readonly Emoji FlagPh = new Emoji("1F1F5-1F1ED", "Philippines Flag", "\ud83c\uddf5\ud83c\udded", "flag-ph", new string[1] { "flag-ph" }, null, null, "Flags", 4, 5, SkinVariationType.None, null, null);

	public static readonly Emoji FlagPk = new Emoji("1F1F5-1F1F0", "Pakistan Flag", "\ud83c\uddf5\ud83c\uddf0", "flag-pk", new string[1] { "flag-pk" }, null, null, "Flags", 4, 6, SkinVariationType.None, null, null);

	public static readonly Emoji FlagPl = new Emoji("1F1F5-1F1F1", "Poland Flag", "\ud83c\uddf5\ud83c\uddf1", "flag-pl", new string[1] { "flag-pl" }, null, null, "Flags", 4, 7, SkinVariationType.None, null, null);

	public static readonly Emoji FlagPm = new Emoji("1F1F5-1F1F2", "St. Pierre & Miquelon Flag", "\ud83c\uddf5\ud83c\uddf2", "flag-pm", new string[1] { "flag-pm" }, null, null, "Flags", 4, 8, SkinVariationType.None, null, null);

	public static readonly Emoji FlagPn = new Emoji("1F1F5-1F1F3", "Pitcairn Islands Flag", "\ud83c\uddf5\ud83c\uddf3", "flag-pn", new string[1] { "flag-pn" }, null, null, "Flags", 4, 9, SkinVariationType.None, null, null);

	public static readonly Emoji FlagPr = new Emoji("1F1F5-1F1F7", "Puerto Rico Flag", "\ud83c\uddf5\ud83c\uddf7", "flag-pr", new string[1] { "flag-pr" }, null, null, "Flags", 4, 10, SkinVariationType.None, null, null);

	public static readonly Emoji FlagPs = new Emoji("1F1F5-1F1F8", "Palestinian Territories Flag", "\ud83c\uddf5\ud83c\uddf8", "flag-ps", new string[1] { "flag-ps" }, null, null, "Flags", 4, 11, SkinVariationType.None, null, null);

	public static readonly Emoji FlagPt = new Emoji("1F1F5-1F1F9", "Portugal Flag", "\ud83c\uddf5\ud83c\uddf9", "flag-pt", new string[1] { "flag-pt" }, null, null, "Flags", 4, 12, SkinVariationType.None, null, null);

	public static readonly Emoji FlagPw = new Emoji("1F1F5-1F1FC", "Palau Flag", "\ud83c\uddf5\ud83c\uddfc", "flag-pw", new string[1] { "flag-pw" }, null, null, "Flags", 4, 13, SkinVariationType.None, null, null);

	public static readonly Emoji FlagPy = new Emoji("1F1F5-1F1FE", "Paraguay Flag", "\ud83c\uddf5\ud83c\uddfe", "flag-py", new string[1] { "flag-py" }, null, null, "Flags", 4, 14, SkinVariationType.None, null, null);

	public static readonly Emoji FlagQa = new Emoji("1F1F6-1F1E6", "Qatar Flag", "\ud83c\uddf6\ud83c\udde6", "flag-qa", new string[1] { "flag-qa" }, null, null, "Flags", 4, 15, SkinVariationType.None, null, null);

	public static readonly Emoji FlagRe = new Emoji("1F1F7-1F1EA", "Réunion Flag", "\ud83c\uddf7\ud83c\uddea", "flag-re", new string[1] { "flag-re" }, null, null, "Flags", 4, 16, SkinVariationType.None, null, null);

	public static readonly Emoji FlagRo = new Emoji("1F1F7-1F1F4", "Romania Flag", "\ud83c\uddf7\ud83c\uddf4", "flag-ro", new string[1] { "flag-ro" }, null, null, "Flags", 4, 17, SkinVariationType.None, null, null);

	public static readonly Emoji FlagRs = new Emoji("1F1F7-1F1F8", "Serbia Flag", "\ud83c\uddf7\ud83c\uddf8", "flag-rs", new string[1] { "flag-rs" }, null, null, "Flags", 4, 18, SkinVariationType.None, null, null);

	public static readonly Emoji Ru = new Emoji("1F1F7-1F1FA", "Russia Flag", "\ud83c\uddf7\ud83c\uddfa", "ru", new string[2] { "ru", "flag-ru" }, null, null, "Flags", 4, 19, SkinVariationType.None, null, null);

	public static readonly Emoji FlagRw = new Emoji("1F1F7-1F1FC", "Rwanda Flag", "\ud83c\uddf7\ud83c\uddfc", "flag-rw", new string[1] { "flag-rw" }, null, null, "Flags", 4, 20, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSa = new Emoji("1F1F8-1F1E6", "Saudi Arabia Flag", "\ud83c\uddf8\ud83c\udde6", "flag-sa", new string[1] { "flag-sa" }, null, null, "Flags", 4, 21, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSb = new Emoji("1F1F8-1F1E7", "Solomon Islands Flag", "\ud83c\uddf8\ud83c\udde7", "flag-sb", new string[1] { "flag-sb" }, null, null, "Flags", 4, 22, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSc = new Emoji("1F1F8-1F1E8", "Seychelles Flag", "\ud83c\uddf8\ud83c\udde8", "flag-sc", new string[1] { "flag-sc" }, null, null, "Flags", 4, 23, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSd = new Emoji("1F1F8-1F1E9", "Sudan Flag", "\ud83c\uddf8\ud83c\udde9", "flag-sd", new string[1] { "flag-sd" }, null, null, "Flags", 4, 24, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSe = new Emoji("1F1F8-1F1EA", "Sweden Flag", "\ud83c\uddf8\ud83c\uddea", "flag-se", new string[1] { "flag-se" }, null, null, "Flags", 4, 25, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSg = new Emoji("1F1F8-1F1EC", "Singapore Flag", "\ud83c\uddf8\ud83c\uddec", "flag-sg", new string[1] { "flag-sg" }, null, null, "Flags", 4, 26, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSh = new Emoji("1F1F8-1F1ED", "St. Helena Flag", "\ud83c\uddf8\ud83c\udded", "flag-sh", new string[1] { "flag-sh" }, null, null, "Flags", 4, 27, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSi = new Emoji("1F1F8-1F1EE", "Slovenia Flag", "\ud83c\uddf8\ud83c\uddee", "flag-si", new string[1] { "flag-si" }, null, null, "Flags", 4, 28, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSj = new Emoji("1F1F8-1F1EF", "Svalbard & Jan Mayen Flag", "\ud83c\uddf8\ud83c\uddef", "flag-sj", new string[1] { "flag-sj" }, null, null, "Flags", 4, 29, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSk = new Emoji("1F1F8-1F1F0", "Slovakia Flag", "\ud83c\uddf8\ud83c\uddf0", "flag-sk", new string[1] { "flag-sk" }, null, null, "Flags", 4, 30, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSl = new Emoji("1F1F8-1F1F1", "Sierra Leone Flag", "\ud83c\uddf8\ud83c\uddf1", "flag-sl", new string[1] { "flag-sl" }, null, null, "Flags", 4, 31, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSm = new Emoji("1F1F8-1F1F2", "San Marino Flag", "\ud83c\uddf8\ud83c\uddf2", "flag-sm", new string[1] { "flag-sm" }, null, null, "Flags", 4, 32, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSn = new Emoji("1F1F8-1F1F3", "Senegal Flag", "\ud83c\uddf8\ud83c\uddf3", "flag-sn", new string[1] { "flag-sn" }, null, null, "Flags", 4, 33, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSo = new Emoji("1F1F8-1F1F4", "Somalia Flag", "\ud83c\uddf8\ud83c\uddf4", "flag-so", new string[1] { "flag-so" }, null, null, "Flags", 4, 34, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSr = new Emoji("1F1F8-1F1F7", "Suriname Flag", "\ud83c\uddf8\ud83c\uddf7", "flag-sr", new string[1] { "flag-sr" }, null, null, "Flags", 4, 35, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSs = new Emoji("1F1F8-1F1F8", "South Sudan Flag", "\ud83c\uddf8\ud83c\uddf8", "flag-ss", new string[1] { "flag-ss" }, null, null, "Flags", 4, 36, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSt = new Emoji("1F1F8-1F1F9", "São Tomé & Príncipe Flag", "\ud83c\uddf8\ud83c\uddf9", "flag-st", new string[1] { "flag-st" }, null, null, "Flags", 4, 37, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSv = new Emoji("1F1F8-1F1FB", "El Salvador Flag", "\ud83c\uddf8\ud83c\uddfb", "flag-sv", new string[1] { "flag-sv" }, null, null, "Flags", 4, 38, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSx = new Emoji("1F1F8-1F1FD", "Sint Maarten Flag", "\ud83c\uddf8\ud83c\uddfd", "flag-sx", new string[1] { "flag-sx" }, null, null, "Flags", 4, 39, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSy = new Emoji("1F1F8-1F1FE", "Syria Flag", "\ud83c\uddf8\ud83c\uddfe", "flag-sy", new string[1] { "flag-sy" }, null, null, "Flags", 4, 40, SkinVariationType.None, null, null);

	public static readonly Emoji FlagSz = new Emoji("1F1F8-1F1FF", "Swaziland Flag", "\ud83c\uddf8\ud83c\uddff", "flag-sz", new string[1] { "flag-sz" }, null, null, "Flags", 4, 41, SkinVariationType.None, null, null);

	public static readonly Emoji FlagTa = new Emoji("1F1F9-1F1E6", "Tristan da Cunha Flag", "\ud83c\uddf9\ud83c\udde6", "flag-ta", new string[1] { "flag-ta" }, null, null, "Flags", 4, 42, SkinVariationType.None, null, null);

	public static readonly Emoji FlagTc = new Emoji("1F1F9-1F1E8", "Turks & Caicos Islands Flag", "\ud83c\uddf9\ud83c\udde8", "flag-tc", new string[1] { "flag-tc" }, null, null, "Flags", 4, 43, SkinVariationType.None, null, null);

	public static readonly Emoji FlagTd = new Emoji("1F1F9-1F1E9", "Chad Flag", "\ud83c\uddf9\ud83c\udde9", "flag-td", new string[1] { "flag-td" }, null, null, "Flags", 4, 44, SkinVariationType.None, null, null);

	public static readonly Emoji FlagTf = new Emoji("1F1F9-1F1EB", "French Southern Territories Flag", "\ud83c\uddf9\ud83c\uddeb", "flag-tf", new string[1] { "flag-tf" }, null, null, "Flags", 4, 45, SkinVariationType.None, null, null);

	public static readonly Emoji FlagTg = new Emoji("1F1F9-1F1EC", "Togo Flag", "\ud83c\uddf9\ud83c\uddec", "flag-tg", new string[1] { "flag-tg" }, null, null, "Flags", 4, 46, SkinVariationType.None, null, null);

	public static readonly Emoji FlagTh = new Emoji("1F1F9-1F1ED", "Thailand Flag", "\ud83c\uddf9\ud83c\udded", "flag-th", new string[1] { "flag-th" }, null, null, "Flags", 4, 47, SkinVariationType.None, null, null);

	public static readonly Emoji FlagTj = new Emoji("1F1F9-1F1EF", "Tajikistan Flag", "\ud83c\uddf9\ud83c\uddef", "flag-tj", new string[1] { "flag-tj" }, null, null, "Flags", 4, 48, SkinVariationType.None, null, null);

	public static readonly Emoji FlagTk = new Emoji("1F1F9-1F1F0", "Tokelau Flag", "\ud83c\uddf9\ud83c\uddf0", "flag-tk", new string[1] { "flag-tk" }, null, null, "Flags", 4, 49, SkinVariationType.None, null, null);

	public static readonly Emoji FlagTl = new Emoji("1F1F9-1F1F1", "Timor-Leste Flag", "\ud83c\uddf9\ud83c\uddf1", "flag-tl", new string[1] { "flag-tl" }, null, null, "Flags", 4, 50, SkinVariationType.None, null, null);

	public static readonly Emoji FlagTm = new Emoji("1F1F9-1F1F2", "Turkmenistan Flag", "\ud83c\uddf9\ud83c\uddf2", "flag-tm", new string[1] { "flag-tm" }, null, null, "Flags", 4, 51, SkinVariationType.None, null, null);

	public static readonly Emoji FlagTn = new Emoji("1F1F9-1F1F3", "Tunisia Flag", "\ud83c\uddf9\ud83c\uddf3", "flag-tn", new string[1] { "flag-tn" }, null, null, "Flags", 5, 0, SkinVariationType.None, null, null);

	public static readonly Emoji FlagTo = new Emoji("1F1F9-1F1F4", "Tonga Flag", "\ud83c\uddf9\ud83c\uddf4", "flag-to", new string[1] { "flag-to" }, null, null, "Flags", 5, 1, SkinVariationType.None, null, null);

	public static readonly Emoji FlagTr = new Emoji("1F1F9-1F1F7", "Turkey Flag", "\ud83c\uddf9\ud83c\uddf7", "flag-tr", new string[1] { "flag-tr" }, null, null, "Flags", 5, 2, SkinVariationType.None, null, null);

	public static readonly Emoji FlagTt = new Emoji("1F1F9-1F1F9", "Trinidad & Tobago Flag", "\ud83c\uddf9\ud83c\uddf9", "flag-tt", new string[1] { "flag-tt" }, null, null, "Flags", 5, 3, SkinVariationType.None, null, null);

	public static readonly Emoji FlagTv = new Emoji("1F1F9-1F1FB", "Tuvalu Flag", "\ud83c\uddf9\ud83c\uddfb", "flag-tv", new string[1] { "flag-tv" }, null, null, "Flags", 5, 4, SkinVariationType.None, null, null);

	public static readonly Emoji FlagTw = new Emoji("1F1F9-1F1FC", "Taiwan Flag", "\ud83c\uddf9\ud83c\uddfc", "flag-tw", new string[1] { "flag-tw" }, null, null, "Flags", 5, 5, SkinVariationType.None, null, null);

	public static readonly Emoji FlagTz = new Emoji("1F1F9-1F1FF", "Tanzania Flag", "\ud83c\uddf9\ud83c\uddff", "flag-tz", new string[1] { "flag-tz" }, null, null, "Flags", 5, 6, SkinVariationType.None, null, null);

	public static readonly Emoji FlagUa = new Emoji("1F1FA-1F1E6", "Ukraine Flag", "\ud83c\uddfa\ud83c\udde6", "flag-ua", new string[1] { "flag-ua" }, null, null, "Flags", 5, 7, SkinVariationType.None, null, null);

	public static readonly Emoji FlagUg = new Emoji("1F1FA-1F1EC", "Uganda Flag", "\ud83c\uddfa\ud83c\uddec", "flag-ug", new string[1] { "flag-ug" }, null, null, "Flags", 5, 8, SkinVariationType.None, null, null);

	public static readonly Emoji FlagUn = new Emoji("1F1FA-1F1F3", "United Nations Flag", "\ud83c\uddfa\ud83c\uddf3", "flag-un", new string[1] { "flag-un" }, null, null, "Flags", 5, 10, SkinVariationType.None, null, null);

	public static readonly Emoji Us = new Emoji("1F1FA-1F1F8", "United States Flag", "\ud83c\uddfa\ud83c\uddf8", "us", new string[2] { "us", "flag-us" }, null, null, "Flags", 5, 11, SkinVariationType.None, null, null);

	public static readonly Emoji FlagUy = new Emoji("1F1FA-1F1FE", "Uruguay Flag", "\ud83c\uddfa\ud83c\uddfe", "flag-uy", new string[1] { "flag-uy" }, null, null, "Flags", 5, 12, SkinVariationType.None, null, null);

	public static readonly Emoji FlagUz = new Emoji("1F1FA-1F1FF", "Uzbekistan Flag", "\ud83c\uddfa\ud83c\uddff", "flag-uz", new string[1] { "flag-uz" }, null, null, "Flags", 5, 13, SkinVariationType.None, null, null);

	public static readonly Emoji FlagVa = new Emoji("1F1FB-1F1E6", "Vatican City Flag", "\ud83c\uddfb\ud83c\udde6", "flag-va", new string[1] { "flag-va" }, null, null, "Flags", 5, 14, SkinVariationType.None, null, null);

	public static readonly Emoji FlagVc = new Emoji("1F1FB-1F1E8", "St. Vincent & Grenadines Flag", "\ud83c\uddfb\ud83c\udde8", "flag-vc", new string[1] { "flag-vc" }, null, null, "Flags", 5, 15, SkinVariationType.None, null, null);

	public static readonly Emoji FlagVe = new Emoji("1F1FB-1F1EA", "Venezuela Flag", "\ud83c\uddfb\ud83c\uddea", "flag-ve", new string[1] { "flag-ve" }, null, null, "Flags", 5, 16, SkinVariationType.None, null, null);

	public static readonly Emoji FlagVg = new Emoji("1F1FB-1F1EC", "British Virgin Islands Flag", "\ud83c\uddfb\ud83c\uddec", "flag-vg", new string[1] { "flag-vg" }, null, null, "Flags", 5, 17, SkinVariationType.None, null, null);

	public static readonly Emoji FlagVi = new Emoji("1F1FB-1F1EE", "U.S. Virgin Islands Flag", "\ud83c\uddfb\ud83c\uddee", "flag-vi", new string[1] { "flag-vi" }, null, null, "Flags", 5, 18, SkinVariationType.None, null, null);

	public static readonly Emoji FlagVn = new Emoji("1F1FB-1F1F3", "Vietnam Flag", "\ud83c\uddfb\ud83c\uddf3", "flag-vn", new string[1] { "flag-vn" }, null, null, "Flags", 5, 19, SkinVariationType.None, null, null);

	public static readonly Emoji FlagVu = new Emoji("1F1FB-1F1FA", "Vanuatu Flag", "\ud83c\uddfb\ud83c\uddfa", "flag-vu", new string[1] { "flag-vu" }, null, null, "Flags", 5, 20, SkinVariationType.None, null, null);

	public static readonly Emoji FlagWf = new Emoji("1F1FC-1F1EB", "Wallis & Futuna Flag", "\ud83c\uddfc\ud83c\uddeb", "flag-wf", new string[1] { "flag-wf" }, null, null, "Flags", 5, 21, SkinVariationType.None, null, null);

	public static readonly Emoji FlagWs = new Emoji("1F1FC-1F1F8", "Samoa Flag", "\ud83c\uddfc\ud83c\uddf8", "flag-ws", new string[1] { "flag-ws" }, null, null, "Flags", 5, 22, SkinVariationType.None, null, null);

	public static readonly Emoji FlagXk = new Emoji("1F1FD-1F1F0", "Kosovo Flag", "\ud83c\uddfd\ud83c\uddf0", "flag-xk", new string[1] { "flag-xk" }, null, null, "Flags", 5, 23, SkinVariationType.None, null, null);

	public static readonly Emoji FlagYe = new Emoji("1F1FE-1F1EA", "Yemen Flag", "\ud83c\uddfe\ud83c\uddea", "flag-ye", new string[1] { "flag-ye" }, null, null, "Flags", 5, 24, SkinVariationType.None, null, null);

	public static readonly Emoji FlagYt = new Emoji("1F1FE-1F1F9", "Mayotte Flag", "\ud83c\uddfe\ud83c\uddf9", "flag-yt", new string[1] { "flag-yt" }, null, null, "Flags", 5, 25, SkinVariationType.None, null, null);

	public static readonly Emoji FlagZa = new Emoji("1F1FF-1F1E6", "South Africa Flag", "\ud83c\uddff\ud83c\udde6", "flag-za", new string[1] { "flag-za" }, null, null, "Flags", 5, 26, SkinVariationType.None, null, null);

	public static readonly Emoji FlagZm = new Emoji("1F1FF-1F1F2", "Zambia Flag", "\ud83c\uddff\ud83c\uddf2", "flag-zm", new string[1] { "flag-zm" }, null, null, "Flags", 5, 27, SkinVariationType.None, null, null);

	public static readonly Emoji FlagZw = new Emoji("1F1FF-1F1FC", "Zimbabwe Flag", "\ud83c\uddff\ud83c\uddfc", "flag-zw", new string[1] { "flag-zw" }, null, null, "Flags", 5, 28, SkinVariationType.None, null, null);

	public static readonly Emoji FlagEngland = new Emoji("1F3F4-E0067-E0062-E0065-E006E-E0067-E007F", "England Flag", "\ud83c\udff4\udb40\udc67\udb40\udc62\udb40\udc65\udb40\udc6e\udb40\udc67\udb40\udc7f", "flag-england", new string[1] { "flag-england" }, null, null, "Flags", 12, 16, SkinVariationType.None, null, null);

	public static readonly Emoji FlagScotland = new Emoji("1F3F4-E0067-E0062-E0073-E0063-E0074-E007F", "Scotland Flag", "\ud83c\udff4\udb40\udc67\udb40\udc62\udb40\udc73\udb40\udc63\udb40\udc74\udb40\udc7f", "flag-scotland", new string[1] { "flag-scotland" }, null, null, "Flags", 12, 17, SkinVariationType.None, null, null);

	public static readonly Emoji FlagWales = new Emoji("1F3F4-E0067-E0062-E0077-E006C-E0073-E007F", "Wales Flag", "\ud83c\udff4\udb40\udc67\udb40\udc62\udb40\udc77\udb40\udc6c\udb40\udc73\udb40\udc7f", "flag-wales", new string[1] { "flag-wales" }, null, null, "Flags", 12, 18, SkinVariationType.None, null, null);

	internal static Emoji GetByUnicodeId(string unicodeId)
	{
		_EnsureLookup();
		if (!_lookup.TryGetValue(unicodeId, out var value))
		{
			return null;
		}
		return value;
	}

	private static void _EnsureLookup()
	{
		if (_lookup != null)
		{
			return;
		}
		lock (_lock)
		{
			if (_lookup == null)
			{
				_lookup = (from f in typeof(Emojis).GetTypeInfo().DeclaredFields
					where f.IsPublic
					select (Emoji)f.GetValue(null)).ToDictionary((Emoji f) => f.Unified, (Emoji f) => f);
			}
		}
	}
}
