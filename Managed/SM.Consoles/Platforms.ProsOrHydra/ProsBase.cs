using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using Hydra.Api.Auth;
using Platforms.Utils;
using Pros.Sdk.Components.Account;
using Pros.Sdk.Components.Authorization;
using Pros.Sdk.Components.CrossSave;
using RedLynx.Api.Account;
using RedLynx.Api.CrossSave;
using UnityEngine;

namespace Platforms.ProsOrHydra;

public abstract class ProsBase : HydraProsBase, IProsProvider, IKsivaProvider
{
	private const int ExpirationThreshold = 10;

	private string _code;

	private IUpdater _updater;

	private long? _cooldown;

	private SaveSnapshot _savedSnapshot;

	private Action<OperationResult> _resultCallback;

	protected AuthorizationComponent Authorization => ((ProsSDKAdapter)base.SDK).AuthorizationComponent;

	protected CrossSaveComponent CrossSaves => ((ProsSDKAdapter)base.SDK).CrossSaveComponent;

	protected AccountComponent AccountComponent => ((ProsSDKAdapter)base.SDK).AccountComponent;

	protected ProsBase(IUpdater updater, IAppFlowInformer appFlowInformer, IProsSettingsProvider prosSettingsProvider, DebugFlags debugPFlags = DebugFlags.Error)
		: base("PROS", updater, appFlowInformer, new ProsSDKProvider(prosSettingsProvider, debugPFlags), debugPFlags)
	{
		_updater = updater;
	}

	public override ValueTask DisposeAsync()
	{
		_updater.UnsubscribeFromCooldown(OnSubmitSaveCooldownCompleted);
		return base.DisposeAsync();
	}

	public async void GetQRCodeTexture(Action<OperationResult, byte[], int> resultCallback)
	{
		try
		{
			GetRegistrationQRCodeResponse getRegistrationQRCodeResponse = await AccountComponent.GetRegistrationQRCode();
			_code = getRegistrationQRCodeResponse.Code;
			if (getRegistrationQRCodeResponse.LinkStatus == LinkStatus.NotLinked)
			{
				byte[] arg = getRegistrationQRCodeResponse.QrCodePng.ToByteArray();
				resultCallback?.Invoke(OperationResult.NotLinked, arg, Mathf.Max(1, getRegistrationQRCodeResponse.ExpirationInSeconds - 10));
			}
			else
			{
				OperationResult arg2 = LinkStatusToResult(getRegistrationQRCodeResponse.LinkStatus);
				resultCallback?.Invoke(arg2, null, 0);
			}
		}
		catch (Exception exception)
		{
			base.SDK.LogException(exception);
			resultCallback?.Invoke(OperationResult.UnspecifiedError, null, 0);
		}
	}

	public async void GetLinkStatus(Action<OperationResult> resultCallback)
	{
		try
		{
			if (string.IsNullOrEmpty(_code))
			{
				resultCallback?.Invoke(OperationResult.CodeIsNotInitialized);
			}
			OperationResult obj = LinkStatusToResult((await AccountComponent.GetRegistrationStatus(_code)).LinkStatus);
			resultCallback?.Invoke(obj);
		}
		catch (Exception exception)
		{
			base.SDK.LogException(exception);
			resultCallback?.Invoke(OperationResult.UnspecifiedError);
		}
	}

	public async void SubmitSnapshot(string key, string contentType, string dataDescription, byte[] data, Action<OperationResult> resultCallback)
	{
		SaveSnapshot saveSnapshot = CreateSaveSnapshot(key, contentType, dataDescription, data);
		if (_cooldown.HasValue)
		{
			if (_resultCallback != null)
			{
				_resultCallback?.Invoke(OperationResult.RemovedFromQueue);
			}
			_savedSnapshot = saveSnapshot;
			_resultCallback = resultCallback;
		}
		else
		{
			await SubmitInternal(saveSnapshot, resultCallback);
		}
	}

	public async void CheckForTransfer(Action<OperationResult, string, Platform, string, string, DateTime> resultCallback)
	{
		try
		{
			GetTransferInfoResponse getTransferInfoResponse = await CrossSaves.GetTransferInfo();
			if (string.IsNullOrEmpty(getTransferInfoResponse.TransferId))
			{
				resultCallback?.Invoke(OperationResult.NoTransfers, string.Empty, Platform.Unknown, string.Empty, string.Empty, DateTime.Now);
			}
			else
			{
				resultCallback?.Invoke(OperationResult.PendingTransfer, getTransferInfoResponse.TransferId, getTransferInfoResponse.Platform, getTransferInfoResponse.ContentType, getTransferInfoResponse.DataDescriptionJson, getTransferInfoResponse.TransferCreatedAt.ToDateTime());
			}
		}
		catch (Exception exception)
		{
			base.SDK.LogException(exception);
			resultCallback?.Invoke(OperationResult.UnspecifiedError, string.Empty, Platform.Unknown, string.Empty, string.Empty, DateTime.Now);
		}
	}

	public async void SkipTransfer(string transferId, Action<OperationResult> resultCallback)
	{
		try
		{
			await CrossSaves.SkipTransfer(transferId);
			resultCallback?.Invoke(OperationResult.Success);
		}
		catch (Exception exception)
		{
			base.SDK.LogException(exception);
			resultCallback?.Invoke(OperationResult.UnspecifiedError);
		}
	}

	public async void CompleteTransfer(string transferId, string backupKey, string backupContentType, string backupDataDescription, byte[] backupData, Action<OperationResult, string, string, string, byte[]> resultCallback)
	{
		try
		{
			SaveSnapshot backupSnapshot = CreateSaveSnapshot(backupKey, backupContentType, backupDataDescription, backupData);
			CompleteTransferResponse completeTransferResponse = await CrossSaves.CompleteTransfer(transferId, backupSnapshot);
			if (completeTransferResponse.TransferSnapshot.Saves == null || completeTransferResponse.TransferSnapshot.Saves.Count != 1)
			{
				resultCallback?.Invoke(OperationResult.InvalidSaveData, string.Empty, string.Empty, string.Empty, null);
				return;
			}
			SaveData saveData = completeTransferResponse.TransferSnapshot.Saves.First();
			resultCallback?.Invoke(OperationResult.Success, saveData.Key, completeTransferResponse.TransferSnapshot.ContentType, completeTransferResponse.TransferSnapshot.DataDescriptionJson, saveData.Data.ToByteArray());
		}
		catch (Exception exception)
		{
			base.SDK.LogException(exception);
			resultCallback?.Invoke(OperationResult.UnspecifiedError, string.Empty, string.Empty, string.Empty, null);
		}
	}

	private void OnSubmitSaveCooldownCompleted()
	{
		_updater.UnsubscribeFromCooldown(OnSubmitSaveCooldownCompleted);
		CheckQueueToSubmit();
	}

	private async void CheckQueueToSubmit()
	{
		if (_savedSnapshot != null)
		{
			SaveSnapshot savedSnapshot = _savedSnapshot;
			Action<OperationResult> resultCallback = _resultCallback;
			_savedSnapshot = null;
			_resultCallback = null;
			await SubmitInternal(savedSnapshot, resultCallback);
		}
		else
		{
			_cooldown = null;
		}
	}

	private async Task SubmitInternal(SaveSnapshot saveSnapshot, Action<OperationResult> resultCallback)
	{
		try
		{
			_cooldown = long.MaxValue;
			long seconds = (await CrossSaves.SubmitSnapshot(saveSnapshot)).Cooldown.Seconds;
			if (seconds > 0)
			{
				_cooldown = seconds;
				_updater.SubscribeForCooldown(OnSubmitSaveCooldownCompleted, seconds);
			}
			else
			{
				_cooldown = null;
				CheckQueueToSubmit();
			}
			resultCallback?.Invoke(OperationResult.Success);
		}
		catch (Exception exception)
		{
			_cooldown = null;
			base.SDK.LogException(exception);
			resultCallback?.Invoke(OperationResult.UnspecifiedError);
		}
	}

	private OperationResult LinkStatusToResult(LinkStatus linkStatus)
	{
		return linkStatus switch
		{
			LinkStatus.Linked => OperationResult.Linked, 
			LinkStatus.CodeRead => OperationResult.CodeRead, 
			LinkStatus.CodeTimeout => OperationResult.CodeTimeout, 
			LinkStatus.Unknown => OperationResult.UnspecifiedError, 
			LinkStatus.NotLinked => OperationResult.NotLinked, 
			_ => throw new Exception($"Invalid status {linkStatus}"), 
		};
	}

	private SaveSnapshot CreateSaveSnapshot(string key, string contentType, string dataDescription, byte[] data)
	{
		return new SaveSnapshot
		{
			ContentType = contentType,
			DataDescriptionJson = dataDescription,
			Saves = 
			{
				new SaveData
				{
					Data = ByteString.FromStream(new MemoryStream(data)),
					Key = key
				}
			}
		};
	}
}
