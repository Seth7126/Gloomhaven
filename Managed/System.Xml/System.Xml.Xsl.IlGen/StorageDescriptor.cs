using System.Reflection;
using System.Reflection.Emit;

namespace System.Xml.Xsl.IlGen;

internal struct StorageDescriptor
{
	private ItemLocation location;

	private object locationObject;

	private Type itemStorageType;

	private bool isCached;

	public ItemLocation Location => location;

	public int ParameterLocation => (int)locationObject;

	public LocalBuilder LocalLocation => locationObject as LocalBuilder;

	public LocalBuilder CurrentLocation => locationObject as LocalBuilder;

	public MethodInfo GlobalLocation => locationObject as MethodInfo;

	public bool IsCached => isCached;

	public Type ItemStorageType => itemStorageType;

	public static StorageDescriptor None()
	{
		return default(StorageDescriptor);
	}

	public static StorageDescriptor Stack(Type itemStorageType, bool isCached)
	{
		return new StorageDescriptor
		{
			location = ItemLocation.Stack,
			itemStorageType = itemStorageType,
			isCached = isCached
		};
	}

	public static StorageDescriptor Parameter(int paramIndex, Type itemStorageType, bool isCached)
	{
		return new StorageDescriptor
		{
			location = ItemLocation.Parameter,
			locationObject = paramIndex,
			itemStorageType = itemStorageType,
			isCached = isCached
		};
	}

	public static StorageDescriptor Local(LocalBuilder loc, Type itemStorageType, bool isCached)
	{
		return new StorageDescriptor
		{
			location = ItemLocation.Local,
			locationObject = loc,
			itemStorageType = itemStorageType,
			isCached = isCached
		};
	}

	public static StorageDescriptor Current(LocalBuilder locIter, Type itemStorageType)
	{
		return new StorageDescriptor
		{
			location = ItemLocation.Current,
			locationObject = locIter,
			itemStorageType = itemStorageType
		};
	}

	public static StorageDescriptor Global(MethodInfo methGlobal, Type itemStorageType, bool isCached)
	{
		return new StorageDescriptor
		{
			location = ItemLocation.Global,
			locationObject = methGlobal,
			itemStorageType = itemStorageType,
			isCached = isCached
		};
	}

	public StorageDescriptor ToStack()
	{
		return Stack(itemStorageType, isCached);
	}

	public StorageDescriptor ToLocal(LocalBuilder loc)
	{
		return Local(loc, itemStorageType, isCached);
	}

	public StorageDescriptor ToStorageType(Type itemStorageType)
	{
		StorageDescriptor result = this;
		result.itemStorageType = itemStorageType;
		return result;
	}
}
