namespace OdinSerializer;

public interface IOverridesSerializationFormat
{
	DataFormat GetFormatToSerializeAs(bool isPlayer);
}
