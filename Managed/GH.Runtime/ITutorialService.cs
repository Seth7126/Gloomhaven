using System.Collections.Generic;

public interface ITutorialService
{
	List<ITutorial> GetTutorials();

	void StartTutorial(ITutorial tutorial);

	bool IsTutorialComplete(ITutorial tutorial);
}
