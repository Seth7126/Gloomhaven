using MapRuleLibrary.Source.YML.Debug;
using MapRuleLibrary.YML.Achievements;
using MapRuleLibrary.YML.BattleGoals;
using MapRuleLibrary.YML.Events;
using MapRuleLibrary.YML.Locations;
using MapRuleLibrary.YML.Message;
using MapRuleLibrary.YML.PersonalQuests;
using MapRuleLibrary.YML.Quest;
using MapRuleLibrary.YML.VisibilitySpheres;

public class CMRLYMLModeData
{
	public DifficultyYML Difficulty { get; private set; }

	public AchievementYML Achievements { get; private set; }

	public BattleGoalYML BattleGoals { get; private set; }

	public PersonalQuestYML PersonalQuests { get; private set; }

	public RoadEventYML RoadEvents { get; private set; }

	public CityEventYML CityEvents { get; private set; }

	public VisibilitySphereYML VisibilitySpheres { get; private set; }

	public TempleYML Temples { get; private set; }

	public VillageYML Villages { get; private set; }

	public StoreLocationYML StoreLocations { get; private set; }

	public QuestYML Quests { get; private set; }

	public MapMessagesYML MapMessages { get; private set; }

	public HeadquartersYML Headquarters { get; private set; }

	public CInitialEventsYML InitialEvents { get; private set; }

	public AutoCompleteYML AutoCompletes { get; private set; }

	public CMRLYMLModeData()
	{
		Difficulty = new DifficultyYML();
		Achievements = new AchievementYML();
		BattleGoals = new BattleGoalYML();
		PersonalQuests = new PersonalQuestYML();
		RoadEvents = new RoadEventYML();
		CityEvents = new CityEventYML();
		InitialEvents = new CInitialEventsYML();
		VisibilitySpheres = new VisibilitySphereYML();
		Temples = new TempleYML();
		Villages = new VillageYML();
		StoreLocations = new StoreLocationYML();
		Quests = new QuestYML();
		MapMessages = new MapMessagesYML();
		Headquarters = new HeadquartersYML();
		AutoCompletes = new AutoCompleteYML();
	}
}
