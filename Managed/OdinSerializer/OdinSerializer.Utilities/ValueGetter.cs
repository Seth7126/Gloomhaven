namespace OdinSerializer.Utilities;

public delegate FieldType ValueGetter<InstanceType, FieldType>(ref InstanceType instance);
