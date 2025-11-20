namespace FFSNet;

public enum DataActionType
{
	NONE,
	SendInitialGameState,
	CompareScenarioStates,
	SendSimpleLogFromHost,
	SendSimpleLogFromClient,
	SendModdedRuleset,
	SendRulesetInstance,
	SendCSVFiles,
	SendPlayerLogFromHost,
	SendPlayerLogFromClient
}
