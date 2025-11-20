using System;
using System.Collections.Generic;
using AmplitudeNS.MiniJSON;
using UnityEngine;

public class Amplitude
{
	private static Dictionary<string, Amplitude> instances;

	private static readonly object instanceLock = new object();

	public bool logging;

	private string instanceName;

	public static Amplitude Instance => getInstance();

	public static Amplitude getInstance()
	{
		return getInstance(null);
	}

	public static Amplitude getInstance(string instanceName)
	{
		string text = instanceName;
		if (string.IsNullOrEmpty(text))
		{
			text = "$default_instance";
		}
		lock (instanceLock)
		{
			if (instances == null)
			{
				instances = new Dictionary<string, Amplitude>();
			}
			if (instances.TryGetValue(text, out var value))
			{
				return value;
			}
			value = new Amplitude(instanceName);
			instances.Add(text, value);
			return value;
		}
	}

	public Amplitude(string instanceName)
	{
		this.instanceName = instanceName;
	}

	protected void Log(string message)
	{
		if (logging)
		{
			Debug.Log(message);
		}
	}

	public void init(string apiKey)
	{
		Log($"C# init {apiKey}");
	}

	public void init(string apiKey, string userId)
	{
		Log($"C# init {apiKey} with userId {userId}");
	}

	public void setTrackingOptions(IDictionary<string, bool> trackingOptions)
	{
		if (trackingOptions != null)
		{
			string arg = Json.Serialize(trackingOptions);
			Log($"C# setting tracking options {arg}");
		}
	}

	public void logEvent(string evt)
	{
		Log($"C# sendEvent {evt}");
	}

	public void logEvent(string evt, IDictionary<string, object> properties)
	{
		string arg = ((properties == null) ? Json.Serialize(new Dictionary<string, object>()) : Json.Serialize(properties));
		Log($"C# sendEvent {evt} with properties {arg}");
	}

	public void logEvent(string evt, IDictionary<string, object> properties, bool outOfSession)
	{
		string arg = ((properties == null) ? Json.Serialize(new Dictionary<string, object>()) : Json.Serialize(properties));
		Log($"C# sendEvent {evt} with properties {arg} and outOfSession {outOfSession}");
	}

	public void setUserId(string userId)
	{
		Log($"C# setUserId {userId}");
	}

	public void setUserProperties(IDictionary<string, object> properties)
	{
		string arg = ((properties == null) ? Json.Serialize(new Dictionary<string, object>()) : Json.Serialize(properties));
		Log($"C# setUserProperties {arg}");
	}

	public void setOptOut(bool enabled)
	{
		Log($"C# setOptOut {enabled}");
	}

	[Obsolete("Please call setUserProperties instead", false)]
	public void setGlobalUserProperties(IDictionary<string, object> properties)
	{
		setUserProperties(properties);
	}

	public void logRevenue(double amount)
	{
		Log($"C# logRevenue {amount}");
	}

	public void logRevenue(string productId, int quantity, double price)
	{
		Log($"C# logRevenue {productId}, {quantity}, {price}");
	}

	public void logRevenue(string productId, int quantity, double price, string receipt, string receiptSignature)
	{
		Log($"C# logRevenue {productId}, {quantity}, {price} (with receipt)");
	}

	public void logRevenue(string productId, int quantity, double price, string receipt, string receiptSignature, string revenueType, IDictionary<string, object> eventProperties)
	{
		string text = ((eventProperties == null) ? Json.Serialize(new Dictionary<string, object>()) : Json.Serialize(eventProperties));
		Log($"C# logRevenue {productId}, {quantity}, {price}, {revenueType}, {text} (with receipt)");
	}

	public string getDeviceId()
	{
		return null;
	}

	public void regenerateDeviceId()
	{
	}

	public void trackSessionEvents(bool enabled)
	{
		Log($"C# trackSessionEvents {enabled}");
	}

	public long getSessionId()
	{
		return -1L;
	}

	public void clearUserProperties()
	{
		Log($"C# clearUserProperties");
	}

	public void unsetUserProperty(string property)
	{
		Log($"C# unsetUserProperty {property}");
	}

	public void setOnceUserProperty(string property, bool value)
	{
		Log($"C# setOnceUserProperty {property}, {value}");
	}

	public void setOnceUserProperty(string property, double value)
	{
		Log($"C# setOnceUserProperty {property}, {value}");
	}

	public void setOnceUserProperty(string property, float value)
	{
		Log($"C# setOnceUserProperty {property}, {value}");
	}

	public void setOnceUserProperty(string property, int value)
	{
		Log($"C# setOnceUserProperty {property}, {value}");
	}

	public void setOnceUserProperty(string property, long value)
	{
		Log($"C# setOnceUserProperty {property}, {value}");
	}

	public void setOnceUserProperty(string property, string value)
	{
		Log($"C# setOnceUserProperty {property}, {value}");
	}

	public void setOnceUserProperty(string property, IDictionary<string, object> values)
	{
		if (values != null)
		{
			string arg = Json.Serialize(values);
			Log($"C# setOnceUserProperty {property}, {arg}");
		}
	}

	public void setOnceUserProperty<T>(string property, IList<T> values)
	{
		if (values != null)
		{
			string arg = Json.Serialize(new Dictionary<string, object> { { "list", values } });
			Log($"C# setOnceUserProperty {property}, {arg}");
		}
	}

	public void setOnceUserProperty(string property, bool[] array)
	{
		Log($"C# setOnceUserProperty {property}, {array}");
	}

	public void setOnceUserProperty(string property, double[] array)
	{
		Log($"C# setOnceUserProperty {property}, {array}");
	}

	public void setOnceUserProperty(string property, float[] array)
	{
		Log($"C# setOnceUserProperty {property}, {array}");
	}

	public void setOnceUserProperty(string property, int[] array)
	{
		Log($"C# setOnceUserProperty {property}, {array}");
	}

	public void setOnceUserProperty(string property, long[] array)
	{
		Log($"C# setOnceUserProperty {property}, {array}");
	}

	public void setOnceUserProperty(string property, string[] array)
	{
		Log($"C# setOnceUserProperty {property}, {array}");
	}

	public void setUserProperty(string property, bool value)
	{
		Log($"C# setUserProperty {property}, {value}");
	}

	public void setUserProperty(string property, double value)
	{
		Log($"C# setUserProperty {property}, {value}");
	}

	public void setUserProperty(string property, float value)
	{
		Log($"C# setUserProperty {property}, {value}");
	}

	public void setUserProperty(string property, int value)
	{
		Log($"C# setUserProperty {property}, {value}");
	}

	public void setUserProperty(string property, long value)
	{
		Log($"C# setUserProperty {property}, {value}");
	}

	public void setUserProperty(string property, string value)
	{
		Log($"C# setUserProperty {property}, {value}");
	}

	public void setUserProperty(string property, IDictionary<string, object> values)
	{
		if (values != null)
		{
			string arg = Json.Serialize(values);
			Log($"C# setUserProperty {property}, {arg}");
		}
	}

	public void setUserProperty<T>(string property, IList<T> values)
	{
		if (values != null)
		{
			string arg = Json.Serialize(new Dictionary<string, object> { { "list", values } });
			Log($"C# setUserProperty {property}, {arg}");
		}
	}

	public void setUserProperty(string property, bool[] array)
	{
		Log($"C# setUserProperty {property}, {array}");
	}

	public void setUserProperty(string property, double[] array)
	{
		Log($"C# setUserProperty {property}, {array}");
	}

	public void setUserProperty(string property, float[] array)
	{
		Log($"C# setUserProperty {property}, {array}");
	}

	public void setUserProperty(string property, int[] array)
	{
		Log($"C# setUserProperty {property}, {array}");
	}

	public void setUserProperty(string property, long[] array)
	{
		Log($"C# setUserProperty {property}, {array}");
	}

	public void setUserProperty(string property, string[] array)
	{
		Log($"C# setUserProperty {property}, {array}");
	}

	public void addUserProperty(string property, double value)
	{
		Log($"C# addUserProperty {property}, {value}");
	}

	public void addUserProperty(string property, float value)
	{
		Log($"C# addUserProperty {property}, {value}");
	}

	public void addUserProperty(string property, int value)
	{
		Log($"C# addUserProperty {property}, {value}");
	}

	public void addUserProperty(string property, long value)
	{
		Log($"C# addUserProperty {property}, {value}");
	}

	public void addUserProperty(string property, string value)
	{
		Log($"C# addUserProperty {property}, {value}");
	}

	public void addUserProperty(string property, IDictionary<string, object> values)
	{
		if (values != null)
		{
			string arg = Json.Serialize(values);
			Log($"C# addUserProperty {property}, {arg}");
		}
	}

	public void appendUserProperty(string property, bool value)
	{
		Log($"C# appendUserProperty {property}, {value}");
	}

	public void appendUserProperty(string property, double value)
	{
		Log($"C# appendUserProperty {property}, {value}");
	}

	public void appendUserProperty(string property, float value)
	{
		Log($"C# appendUserProperty {property}, {value}");
	}

	public void appendUserProperty(string property, int value)
	{
		Log($"C# appendUserProperty {property}, {value}");
	}

	public void appendUserProperty(string property, long value)
	{
		Log($"C# appendUserProperty {property}, {value}");
	}

	public void appendUserProperty(string property, string value)
	{
		Log($"C# appendUserProperty {property}, {value}");
	}

	public void appendUserProperty(string property, IDictionary<string, object> values)
	{
		if (values != null)
		{
			string arg = Json.Serialize(values);
			Log($"C# appendUserProperty {property}, {arg}");
		}
	}

	public void appendUserProperty<T>(string property, IList<T> values)
	{
		if (values != null)
		{
			string arg = Json.Serialize(new Dictionary<string, object> { { "list", values } });
			Log($"C# appendUserProperty {property}, {arg}");
		}
	}

	public void appendUserProperty(string property, bool[] array)
	{
		Log($"C# appendUserProperty {property}, {array}");
	}

	public void appendUserProperty(string property, double[] array)
	{
		Log($"C# appendUserProperty {property}, {array}");
	}

	public void appendUserProperty(string property, float[] array)
	{
		Log($"C# appendUserProperty {property}, {array}");
	}

	public void appendUserProperty(string property, int[] array)
	{
		Log($"C# appendUserProperty {property}, {array}");
	}

	public void appendUserProperty(string property, long[] array)
	{
		Log($"C# appendUserProperty {property}, {array}");
	}

	public void appendUserProperty(string property, string[] array)
	{
		Log($"C# appendUserProperty {property}, {array}");
	}

	public void startSession()
	{
	}

	public void endSession()
	{
	}
}
