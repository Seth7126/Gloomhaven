using System;
using Hydra.Api.Auth;

namespace Platforms.ProsOrHydra;

public interface IProsProvider : IKsivaProvider
{
	void GetQRCodeTexture(Action<OperationResult, byte[], int> resultCallback);

	void GetLinkStatus(Action<OperationResult> resultCallback);

	void SubmitSnapshot(string key, string contentType, string dataDescription, byte[] data, Action<OperationResult> resultCallback);

	void CheckForTransfer(Action<OperationResult, string, Platform, string, string, DateTime> resultCallback);

	void SkipTransfer(string transferId, Action<OperationResult> resultCallback);

	void CompleteTransfer(string transferId, string backupKey, string backupContentType, string backupDataDescription, byte[] backupData, Action<OperationResult, string, string, string, byte[]> resultCallback);
}
