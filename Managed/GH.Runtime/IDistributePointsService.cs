using System.Collections.Generic;
using FFSNet;
using UnityEngine;

public interface IDistributePointsService
{
	List<IDistributePointsActor> GetActors();

	Sprite GetTitleIcon();

	string GetTitleText();

	bool CanRemovePointsFrom(IDistributePointsActor actor);

	bool CanAddPointsTo(IDistributePointsActor actor);

	void AddPoint(IDistributePointsActor actor);

	void RemovePoint(IDistributePointsActor actor);

	int GetMaxPoints(IDistributePointsActor actor);

	int GetCurrentPoints(IDistributePointsActor actor);

	int GetAssignedPoints(IDistributePointsActor actor);

	IDistributePointsActor GetActor(GameAction gameAction);
}
