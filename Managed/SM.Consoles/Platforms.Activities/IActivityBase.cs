namespace Platforms.Activities;

public interface IActivityBase
{
	string ID { get; }

	bool SaveIndependent { get; }

	string[] FilterTags { get; }
}
