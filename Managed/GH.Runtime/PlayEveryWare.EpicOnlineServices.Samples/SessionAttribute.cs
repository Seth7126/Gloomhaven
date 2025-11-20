using Epic.OnlineServices;
using Epic.OnlineServices.Sessions;

namespace PlayEveryWare.EpicOnlineServices.Samples;

public class SessionAttribute
{
	public AttributeType ValueType = AttributeType.String;

	public string Key;

	public long? AsInt64 = 0L;

	public double? AsDouble = 0.0;

	public bool? AsBool = false;

	public string AsString;

	public SessionAttributeAdvertisementType Advertisement;

	public AttributeData AsAttribute
	{
		get
		{
			AttributeData result = new AttributeData
			{
				Key = Key
			};
			switch (ValueType)
			{
			case AttributeType.String:
				result.Value = new AttributeDataValue
				{
					AsUtf8 = AsString
				};
				break;
			case AttributeType.Int64:
				result.Value = new AttributeDataValue
				{
					AsInt64 = AsInt64
				};
				break;
			case AttributeType.Double:
				result.Value = new AttributeDataValue
				{
					AsDouble = AsDouble
				};
				break;
			case AttributeType.Boolean:
				result.Value = new AttributeDataValue
				{
					AsBool = AsBool
				};
				break;
			}
			return result;
		}
	}

	public override bool Equals(object other)
	{
		SessionAttribute sessionAttribute = (SessionAttribute)other;
		if (ValueType == sessionAttribute.ValueType && AsInt64 == sessionAttribute.AsInt64 && AsDouble == sessionAttribute.AsDouble && AsBool == sessionAttribute.AsBool && AsString == sessionAttribute.AsString && Key == sessionAttribute.Key)
		{
			return Advertisement == sessionAttribute.Advertisement;
		}
		return false;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
