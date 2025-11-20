using MapRuleLibrary.YML.Locations;

public static class EHQStoreExtensions
{
	public static EGuildmasterMode Convert(this EHQStores store)
	{
		return store switch
		{
			EHQStores.Enhancer => EGuildmasterMode.Enchantress, 
			EHQStores.Merchant => EGuildmasterMode.Merchant, 
			EHQStores.Temple => EGuildmasterMode.Temple, 
			EHQStores.Trainer => EGuildmasterMode.Trainer, 
			_ => EGuildmasterMode.None, 
		};
	}
}
