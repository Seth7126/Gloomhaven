using System;

public interface IApparanceResourceListLoaderAsync
{
	ApparanceResourceList LoadCheck(string asset_path);

	bool LoadAsync(string asset_path, Action<ApparanceResourceList> onCompleted);
}
