using System;
using System.Linq;
using System.Threading;
using AddressableMisc;
using JetBrains.Annotations;
using ScenarioRuleLibrary;
using ScenarioRuleLibrary.YML;
using SpriteMemoryManagement;
using UnityEngine;
using UnityEngine.UI;

public class UIQuestEnemy : MonoBehaviour
{
	[SerializeField]
	private Image enemyPortrait;

	[SerializeField]
	private Graphic eliteMask;

	[SerializeField]
	private ImageSpriteLoader _imageSpriteLoader;

	private Action<CMonsterClass, bool> onHovered;

	private CMonsterClass monster;

	private CancellationTokenSource _spriteLoadCancellationTokenSource;

	public bool EliteMaskActivated => eliteMask.gameObject.activeInHierarchy;

	[UsedImplicitly]
	private void OnDestroy()
	{
		AddressableLoaderHelper.UnloadAssetsByContext(this);
		onHovered = null;
	}

	public void ShowEnemy(string enemyClass, int level, Action<CMonsterClass, bool> onHovered = null)
	{
		ShowEnemy(MonsterClassManager.Find(enemyClass), level, onHovered);
	}

	public async void ShowEnemy(CMonsterClass monster, int enemyLevel, Action<CMonsterClass, bool> onHovered = null)
	{
		this.monster = monster;
		this.onHovered = onHovered;
		eliteMask.gameObject.SetActive(monster.NonEliteVariant != null && monster.NonEliteVariant.DefaultModel != monster.DefaultModel);
		string customPortrait = ScenarioRuleClient.SRLYML.MonsterConfigs.SingleOrDefault((MonsterConfigYMLData s) => monster.MonsterYML.CustomConfig == s.ID)?.Portrait;
		ReferenceToSprite actorPortraitRef = UIInfoTools.Instance.GetActorPortraitRef(monster.DefaultModel, customPortrait);
		EnsureCancellationTokenSourceCreated();
		CancelLoad();
		AddressableLoaderHelper.UnloadAssetsByContext(this);
		if (!(actorPortraitRef.SpecialSprite != null))
		{
			await enemyPortrait.LoadSpriteAsyncAddressable(this, actorPortraitRef.SpriteReference, _spriteLoadCancellationTokenSource.Token);
		}
		else
		{
			enemyPortrait.sprite = actorPortraitRef.SpecialSprite;
		}
		ShowUnlocked(isUnlocked: true);
	}

	public void ShowUnlocked(bool isUnlocked)
	{
		Image image = enemyPortrait;
		Material material = (eliteMask.material = (isUnlocked ? null : UIInfoTools.Instance.greyedOutMaterial));
		image.material = material;
	}

	public void OnHovered(bool hovered)
	{
		onHovered?.Invoke(monster, hovered);
	}

	private void EnsureCancellationTokenSourceCreated()
	{
		if (_spriteLoadCancellationTokenSource == null)
		{
			_spriteLoadCancellationTokenSource = new CancellationTokenSource();
		}
	}

	public void CancelLoad()
	{
		if (_spriteLoadCancellationTokenSource != null)
		{
			_spriteLoadCancellationTokenSource.Cancel();
			_spriteLoadCancellationTokenSource.Dispose();
		}
		_spriteLoadCancellationTokenSource = new CancellationTokenSource();
	}
}
