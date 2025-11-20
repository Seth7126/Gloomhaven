using System;
using System.IO;
using System.Threading;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.Exceptions;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.ResourceManagement.Util;

namespace UnityEngine.ResourceManagement.ResourceProviders;

internal class AssetBundleResource : IAssetBundleResource, IUpdateReceiver
{
	internal enum LoadType
	{
		None,
		Local,
		Web
	}

	private AssetBundle m_AssetBundle;

	private DownloadHandlerAssetBundle m_downloadHandler;

	private AsyncOperation m_RequestOperation;

	private WebRequestQueueOperation m_WebRequestQueueOperation;

	internal ProvideHandle m_ProvideHandle;

	internal AssetBundleRequestOptions m_Options;

	[NonSerialized]
	private bool m_WebRequestCompletedCallbackCalled;

	private int m_Retries;

	private long m_BytesToDownload;

	private long m_DownloadedBytes;

	private bool m_Completed;

	private const int k_WaitForWebRequestMainThreadSleep = 1;

	private string m_TransformedInternalId;

	private AssetBundleRequest m_PreloadRequest;

	private bool m_PreloadCompleted;

	private ulong m_LastDownloadedByteCount;

	private float m_TimeoutTimer;

	private int m_TimeoutOverFrames;

	private bool HasTimedOut
	{
		get
		{
			if (m_TimeoutTimer >= (float)m_Options.Timeout)
			{
				return m_TimeoutOverFrames > 5;
			}
			return false;
		}
	}

	internal long BytesToDownload
	{
		get
		{
			if (m_BytesToDownload == -1)
			{
				if (m_Options != null)
				{
					m_BytesToDownload = m_Options.ComputeSize(m_ProvideHandle.Location, m_ProvideHandle.ResourceManager);
				}
				else
				{
					m_BytesToDownload = 0L;
				}
			}
			return m_BytesToDownload;
		}
	}

	internal UnityWebRequest CreateWebRequest(IResourceLocation loc)
	{
		string url = m_ProvideHandle.ResourceManager.TransformInternalId(loc);
		return CreateWebRequest(url);
	}

	internal UnityWebRequest CreateWebRequest(string url)
	{
		if (m_Options == null)
		{
			return UnityWebRequestAssetBundle.GetAssetBundle(url);
		}
		UnityWebRequest unityWebRequest;
		if (!string.IsNullOrEmpty(m_Options.Hash))
		{
			CachedAssetBundle cachedAssetBundle = new CachedAssetBundle(m_Options.BundleName, Hash128.Parse(m_Options.Hash));
			unityWebRequest = ((!m_Options.UseCrcForCachedBundle && Caching.IsVersionCached(cachedAssetBundle)) ? UnityWebRequestAssetBundle.GetAssetBundle(url, cachedAssetBundle) : UnityWebRequestAssetBundle.GetAssetBundle(url, cachedAssetBundle, m_Options.Crc));
		}
		else
		{
			unityWebRequest = UnityWebRequestAssetBundle.GetAssetBundle(url, m_Options.Crc);
		}
		if (m_Options.RedirectLimit > 0)
		{
			unityWebRequest.redirectLimit = m_Options.RedirectLimit;
		}
		if (m_ProvideHandle.ResourceManager.CertificateHandlerInstance != null)
		{
			unityWebRequest.certificateHandler = m_ProvideHandle.ResourceManager.CertificateHandlerInstance;
			unityWebRequest.disposeCertificateHandlerOnDispose = false;
		}
		m_ProvideHandle.ResourceManager.WebRequestOverride?.Invoke(unityWebRequest);
		return unityWebRequest;
	}

	internal AssetBundleRequest GetAssetPreloadRequest()
	{
		if (m_PreloadCompleted || GetAssetBundle() == null)
		{
			return null;
		}
		if (m_Options.AssetLoadMode == AssetLoadMode.AllPackedAssetsAndDependencies)
		{
			if (m_PreloadRequest == null)
			{
				m_PreloadRequest = m_AssetBundle.LoadAllAssetsAsync();
				m_PreloadRequest.completed += delegate
				{
					m_PreloadCompleted = true;
				};
			}
			return m_PreloadRequest;
		}
		return null;
	}

	private float PercentComplete()
	{
		if (m_RequestOperation == null)
		{
			return 0f;
		}
		return m_RequestOperation.progress;
	}

	private DownloadStatus GetDownloadStatus()
	{
		if (m_Options == null)
		{
			return default(DownloadStatus);
		}
		DownloadStatus result = new DownloadStatus
		{
			TotalBytes = BytesToDownload,
			IsDone = (PercentComplete() >= 1f)
		};
		if (BytesToDownload > 0)
		{
			if (m_WebRequestQueueOperation != null && string.IsNullOrEmpty(m_WebRequestQueueOperation.m_WebRequest.error))
			{
				m_DownloadedBytes = (long)m_WebRequestQueueOperation.m_WebRequest.downloadedBytes;
			}
			else if (m_RequestOperation != null && m_RequestOperation is UnityWebRequestAsyncOperation unityWebRequestAsyncOperation && string.IsNullOrEmpty(unityWebRequestAsyncOperation.webRequest.error))
			{
				m_DownloadedBytes = (long)unityWebRequestAsyncOperation.webRequest.downloadedBytes;
			}
		}
		result.DownloadedBytes = m_DownloadedBytes;
		return result;
	}

	public AssetBundle GetAssetBundle()
	{
		if (m_AssetBundle == null)
		{
			if (m_downloadHandler != null)
			{
				m_AssetBundle = m_downloadHandler.assetBundle;
				m_downloadHandler.Dispose();
				m_downloadHandler = null;
			}
			else if (m_RequestOperation is AssetBundleCreateRequest)
			{
				m_AssetBundle = (m_RequestOperation as AssetBundleCreateRequest).assetBundle;
			}
		}
		return m_AssetBundle;
	}

	internal void Start(ProvideHandle provideHandle)
	{
		m_Retries = 0;
		m_AssetBundle = null;
		m_downloadHandler = null;
		m_RequestOperation = null;
		m_WebRequestCompletedCallbackCalled = false;
		m_ProvideHandle = provideHandle;
		m_Options = m_ProvideHandle.Location.Data as AssetBundleRequestOptions;
		m_BytesToDownload = -1L;
		m_ProvideHandle.SetProgressCallback(PercentComplete);
		m_ProvideHandle.SetDownloadProgressCallbacks(GetDownloadStatus);
		m_ProvideHandle.SetWaitForCompletionCallback(WaitForCompletionHandler);
		BeginOperation();
	}

	private bool WaitForCompletionHandler()
	{
		if (m_RequestOperation == null)
		{
			if (m_WebRequestQueueOperation == null)
			{
				return false;
			}
			WebRequestQueue.WaitForRequestToBeActive(m_WebRequestQueueOperation, 1);
		}
		if (m_RequestOperation is UnityWebRequestAsyncOperation op)
		{
			while (!UnityWebRequestUtilities.IsAssetBundleDownloaded(op))
			{
				Thread.Sleep(1);
			}
		}
		if (m_RequestOperation is UnityWebRequestAsyncOperation && !m_WebRequestCompletedCallbackCalled)
		{
			WebRequestOperationCompleted(m_RequestOperation);
			m_RequestOperation.completed -= WebRequestOperationCompleted;
		}
		GetAssetBundle();
		if (!m_Completed && m_RequestOperation.isDone)
		{
			m_ProvideHandle.Complete(this, m_AssetBundle != null, null);
			m_Completed = true;
		}
		return m_Completed;
	}

	private void AddCallbackInvokeIfDone(AsyncOperation operation, Action<AsyncOperation> callback)
	{
		if (operation.isDone)
		{
			callback(operation);
		}
		else
		{
			operation.completed += callback;
		}
	}

	internal static void GetLoadInfo(ProvideHandle handle, out LoadType loadType, out string path)
	{
		GetLoadInfo(handle.Location, handle.ResourceManager, out loadType, out path);
	}

	internal static void GetLoadInfo(IResourceLocation location, ResourceManager resourceManager, out LoadType loadType, out string path)
	{
		if (!(location?.Data is AssetBundleRequestOptions assetBundleRequestOptions))
		{
			loadType = LoadType.None;
			path = null;
			return;
		}
		path = resourceManager.TransformInternalId(location);
		if (Application.platform == RuntimePlatform.Android && path.StartsWith("jar:"))
		{
			loadType = ((!assetBundleRequestOptions.UseUnityWebRequestForLocalBundles) ? LoadType.Local : LoadType.Web);
		}
		else if (ResourceManagerConfig.ShouldPathUseWebRequest(path))
		{
			loadType = LoadType.Web;
		}
		else if (assetBundleRequestOptions.UseUnityWebRequestForLocalBundles)
		{
			path = "file:///" + Path.GetFullPath(path);
			loadType = LoadType.Web;
		}
		else
		{
			loadType = LoadType.Local;
		}
	}

	private void BeginOperation()
	{
		m_DownloadedBytes = 0L;
		GetLoadInfo(m_ProvideHandle, out var loadType, out m_TransformedInternalId);
		switch (loadType)
		{
		case LoadType.Local:
			m_RequestOperation = AssetBundle.LoadFromFileAsync(m_TransformedInternalId, (m_Options != null) ? m_Options.Crc : 0u);
			AddCallbackInvokeIfDone(m_RequestOperation, LocalRequestOperationCompleted);
			break;
		case LoadType.Web:
		{
			m_WebRequestCompletedCallbackCalled = false;
			UnityWebRequest unityWebRequest = CreateWebRequest(m_TransformedInternalId);
			((DownloadHandlerAssetBundle)unityWebRequest.downloadHandler).autoLoadAssetBundle = !(m_ProvideHandle.Location is DownloadOnlyLocation);
			unityWebRequest.disposeDownloadHandlerOnDispose = false;
			m_WebRequestQueueOperation = WebRequestQueue.QueueRequest(unityWebRequest);
			if (m_WebRequestQueueOperation.IsDone)
			{
				BeginWebRequestOperation(m_WebRequestQueueOperation.Result);
				break;
			}
			WebRequestQueueOperation webRequestQueueOperation = m_WebRequestQueueOperation;
			webRequestQueueOperation.OnComplete = (Action<UnityWebRequestAsyncOperation>)Delegate.Combine(webRequestQueueOperation.OnComplete, (Action<UnityWebRequestAsyncOperation>)delegate(UnityWebRequestAsyncOperation asyncOp)
			{
				BeginWebRequestOperation(asyncOp);
			});
			break;
		}
		default:
			m_RequestOperation = null;
			m_ProvideHandle.Complete<AssetBundleResource>(null, status: false, new RemoteProviderException($"Invalid path in AssetBundleProvider: '{m_TransformedInternalId}'.", m_ProvideHandle.Location));
			m_Completed = true;
			break;
		}
	}

	private void BeginWebRequestOperation(AsyncOperation asyncOp)
	{
		m_TimeoutTimer = 0f;
		m_TimeoutOverFrames = 0;
		m_LastDownloadedByteCount = 0uL;
		m_RequestOperation = asyncOp;
		if (m_RequestOperation == null || m_RequestOperation.isDone)
		{
			WebRequestOperationCompleted(m_RequestOperation);
			return;
		}
		if (m_Options.Timeout > 0)
		{
			m_ProvideHandle.ResourceManager.AddUpdateReceiver(this);
		}
		m_RequestOperation.completed += WebRequestOperationCompleted;
	}

	public void Update(float unscaledDeltaTime)
	{
		if (m_RequestOperation == null || !(m_RequestOperation is UnityWebRequestAsyncOperation { isDone: false } unityWebRequestAsyncOperation))
		{
			return;
		}
		if (m_LastDownloadedByteCount != unityWebRequestAsyncOperation.webRequest.downloadedBytes)
		{
			m_TimeoutTimer = 0f;
			m_TimeoutOverFrames = 0;
			m_LastDownloadedByteCount = unityWebRequestAsyncOperation.webRequest.downloadedBytes;
			return;
		}
		m_TimeoutTimer += unscaledDeltaTime;
		if (HasTimedOut)
		{
			unityWebRequestAsyncOperation.webRequest.Abort();
		}
		m_TimeoutOverFrames++;
	}

	private void LocalRequestOperationCompleted(AsyncOperation op)
	{
		CompleteBundleLoad((op as AssetBundleCreateRequest).assetBundle);
	}

	private void CompleteBundleLoad(AssetBundle bundle)
	{
		m_AssetBundle = bundle;
		if (m_AssetBundle != null)
		{
			m_ProvideHandle.Complete(this, status: true, null);
		}
		else
		{
			m_ProvideHandle.Complete<AssetBundleResource>(null, status: false, new RemoteProviderException($"Invalid path in AssetBundleProvider: '{m_TransformedInternalId}'.", m_ProvideHandle.Location));
		}
		m_Completed = true;
	}

	private void WebRequestOperationCompleted(AsyncOperation op)
	{
		if (m_WebRequestCompletedCallbackCalled)
		{
			return;
		}
		if (m_Options.Timeout > 0)
		{
			m_ProvideHandle.ResourceManager.RemoveUpdateReciever(this);
		}
		m_WebRequestCompletedCallbackCalled = true;
		UnityWebRequest unityWebRequest = (op as UnityWebRequestAsyncOperation)?.webRequest;
		m_downloadHandler = unityWebRequest?.downloadHandler as DownloadHandlerAssetBundle;
		UnityWebRequestResult result = null;
		if (unityWebRequest != null && !UnityWebRequestUtilities.RequestHasErrors(unityWebRequest, out result))
		{
			if (!m_Completed)
			{
				m_ProvideHandle.Complete(this, status: true, null);
				m_Completed = true;
			}
			if (!string.IsNullOrEmpty(m_Options.Hash) && m_Options.ClearOtherCachedVersionsWhenLoaded)
			{
				Caching.ClearOtherCachedVersions(m_Options.BundleName, Hash128.Parse(m_Options.Hash));
			}
		}
		else
		{
			if (HasTimedOut)
			{
				result.Error = "Request timeout";
			}
			unityWebRequest = m_WebRequestQueueOperation.m_WebRequest;
			if (result == null)
			{
				result = new UnityWebRequestResult(m_WebRequestQueueOperation.m_WebRequest);
			}
			m_downloadHandler = unityWebRequest.downloadHandler as DownloadHandlerAssetBundle;
			m_downloadHandler.Dispose();
			m_downloadHandler = null;
			bool flag = false;
			string format = $"Web request failed, retrying ({m_Retries}/{m_Options.RetryCount})...\n{result}";
			if (!string.IsNullOrEmpty(m_Options.Hash))
			{
				CachedAssetBundle cachedBundle = new CachedAssetBundle(m_Options.BundleName, Hash128.Parse(m_Options.Hash));
				if (Caching.IsVersionCached(cachedBundle))
				{
					format = $"Web request failed to load from cache. The cached AssetBundle will be cleared from the cache and re-downloaded. Retrying...\n{result}";
					Caching.ClearCachedVersion(cachedBundle.name, cachedBundle.hash);
					if (m_Options.RetryCount == 0 && m_Retries == 0)
					{
						Debug.LogFormat(format);
						BeginOperation();
						m_Retries++;
						flag = true;
					}
				}
			}
			if (!flag)
			{
				if (m_Retries < m_Options.RetryCount && result.ShouldRetryDownloadError())
				{
					m_Retries++;
					Debug.LogFormat(format);
					BeginOperation();
				}
				else
				{
					RemoteProviderException exception = new RemoteProviderException("Unable to load asset bundle from : " + unityWebRequest.url, m_ProvideHandle.Location, result);
					m_ProvideHandle.Complete<AssetBundleResource>(null, status: false, exception);
					m_Completed = true;
				}
			}
		}
		unityWebRequest.Dispose();
	}

	public void Unload()
	{
		if (m_AssetBundle != null)
		{
			m_AssetBundle.Unload(unloadAllLoadedObjects: true);
			m_AssetBundle = null;
		}
		if (m_downloadHandler != null)
		{
			m_downloadHandler.Dispose();
			m_downloadHandler = null;
		}
		m_RequestOperation = null;
	}
}
