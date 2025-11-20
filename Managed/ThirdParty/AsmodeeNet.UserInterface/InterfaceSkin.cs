using UnityEngine;

namespace AsmodeeNet.UserInterface;

[CreateAssetMenu]
public class InterfaceSkin : ScriptableObject
{
	public GameObject alertControllerPrefab;

	[Header("SSO")]
	public GameObject ssoPrefab;

	public GameObject updateEmailPopUpPrefab;

	[Header("Cross-Promotion")]
	public GameObject BannerPrefab;

	public GameObject InterstitialPopupPrefab;

	public GameObject MoreGamesPopupPrefab;

	public GameObject GameDetailsPopupPrefab;
}
