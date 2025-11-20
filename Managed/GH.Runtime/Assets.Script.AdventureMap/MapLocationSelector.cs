using Code.State;
using Script.GUI.SMNavigation;
using Script.GUI.SMNavigation.States.CampaignMapStates;
using Script.GUI.SMNavigation.States.PopupStates;
using UnityEngine;

namespace Assets.Script.AdventureMap;

public class MapLocationSelector : MonoBehaviour
{
	private readonly LayerMask _layerMask = 32768;

	private readonly float _maxDistance = 1000f;

	private readonly Vector2 _screenCenterPosition = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);

	private readonly RaycastHit[] _results = new RaycastHit[1];

	private Camera _camera;

	private MapLocation _mapLocation;

	private void Awake()
	{
		_camera = Camera.main;
	}

	private void Update()
	{
		if (Singleton<MapFTUEManager>.Instance != null && Singleton<MapFTUEManager>.Instance.HasToShowFTUEOnNoneOrInitialStep)
		{
			return;
		}
		IState currentState = Singleton<UINavigation>.Instance.StateMachine.CurrentState;
		if (!(currentState is LoadoutState) && !(currentState is LocationHoverState) && !(currentState is WorldMapState))
		{
			if (currentState is PersonalQuestCompletedState || currentState is TravelMapState)
			{
				_mapLocation?.OnPointerExit(null);
				_mapLocation = null;
			}
		}
		else if (Physics.RaycastNonAlloc(_camera.ScreenPointToRay(_screenCenterPosition), _results, _maxDistance, _layerMask) > 0)
		{
			if (!(_mapLocation != null) && _results[0].collider.TryGetComponent<MapLocation>(out var component))
			{
				_mapLocation = component;
				_mapLocation.OnPointerEnter(null);
				Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.LocationHover, new MapLocationStateData(_mapLocation));
			}
		}
		else if (_mapLocation != null)
		{
			_mapLocation.OnPointerExit(null);
			_mapLocation = null;
			Singleton<UINavigation>.Instance.StateMachine.Enter(CampaignMapStateTag.WorldMap);
		}
	}
}
