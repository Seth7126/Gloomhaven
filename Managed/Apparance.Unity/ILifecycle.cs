internal interface ILifecycle
{
	void ApplicationStart();

	void EditorStart();

	void CodeStart();

	void EditorTick();

	void EditorStop();

	void GameStart();

	void GameTick();

	void GameStop();

	void ApplicationStop();
}
