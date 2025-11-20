using System;
using System.Collections.Generic;
using UnityEngine.Playables;

namespace UnityEngine.Timeline;

internal static class NotificationUtilities
{
	public static ScriptPlayable<TimeNotificationBehaviour> CreateNotificationsPlayable(PlayableGraph graph, IEnumerable<IMarker> markers, double duration, DirectorWrapMode extrapolationMode)
	{
		ScriptPlayable<TimeNotificationBehaviour> result = ScriptPlayable<TimeNotificationBehaviour>.Null;
		foreach (IMarker marker in markers)
		{
			if (marker is INotification payload)
			{
				if (result.Equals(ScriptPlayable<TimeNotificationBehaviour>.Null))
				{
					result = TimeNotificationBehaviour.Create(graph, duration, extrapolationMode);
				}
				DiscreteTime discreteTime = (DiscreteTime)marker.time;
				DiscreteTime discreteTime2 = (DiscreteTime)duration;
				if (discreteTime >= discreteTime2 && discreteTime <= discreteTime2.OneTickAfter() && discreteTime2 != 0)
				{
					discreteTime = discreteTime2.OneTickBefore();
				}
				if (marker is INotificationOptionProvider notificationOptionProvider)
				{
					result.GetBehaviour().AddNotification((double)discreteTime, payload, notificationOptionProvider.flags);
				}
				else
				{
					result.GetBehaviour().AddNotification((double)discreteTime, payload);
				}
			}
		}
		return result;
	}

	public static bool TrackTypeSupportsNotifications(Type type)
	{
		TrackBindingTypeAttribute trackBindingTypeAttribute = (TrackBindingTypeAttribute)Attribute.GetCustomAttribute(type, typeof(TrackBindingTypeAttribute));
		if (trackBindingTypeAttribute != null)
		{
			if (!typeof(Component).IsAssignableFrom(trackBindingTypeAttribute.type))
			{
				return typeof(GameObject).IsAssignableFrom(trackBindingTypeAttribute.type);
			}
			return true;
		}
		return false;
	}
}
