using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using XUnity.AutoTranslator.Plugin.Core.Configuration;
using XUnity.AutoTranslator.Plugin.Core.Endpoints;
using XUnity.AutoTranslator.Plugin.Core.Web;
using XUnity.Common.Logging;

namespace XUnity.AutoTranslator.Plugin.Core;

internal class TranslationManager
{
	private readonly List<IMonoBehaviour_Update> _updateCallbacks;

	private readonly List<TranslationEndpointManager> _endpointsWithUnstartedJobs;

	public int OngoingTranslations { get; set; }

	public int UnstartedTranslations { get; set; }

	public List<TranslationEndpointManager> ConfiguredEndpoints { get; private set; }

	public List<TranslationEndpointManager> AllEndpoints { get; private set; }

	public TranslationEndpointManager CurrentEndpoint { get; set; }

	public TranslationEndpointManager FallbackEndpoint { get; set; }

	public TranslationEndpointManager PassthroughEndpoint { get; private set; }

	public event Action<TranslationJob> JobCompleted;

	public event Action<TranslationJob> JobFailed;

	public TranslationManager()
	{
		_updateCallbacks = new List<IMonoBehaviour_Update>();
		_endpointsWithUnstartedJobs = new List<TranslationEndpointManager>();
		ConfiguredEndpoints = new List<TranslationEndpointManager>();
		AllEndpoints = new List<TranslationEndpointManager>();
	}

	public bool IsFallbackAvailableFor(TranslationEndpointManager endpoint)
	{
		if (endpoint != null && FallbackEndpoint != null && endpoint == CurrentEndpoint)
		{
			return FallbackEndpoint != endpoint;
		}
		return false;
	}

	public void InitializeEndpoints()
	{
		try
		{
			HttpSecurity httpSecurity = new HttpSecurity();
			CreateEndpoints(httpSecurity);
			AllEndpoints = (from x in AllEndpoints
				orderby x.Error != null, x.Endpoint.FriendlyName
				select x).ToList();
			PassthroughEndpoint = AllEndpoints.FirstOrDefault((TranslationEndpointManager x) => x.Endpoint is PassthroughTranslateEndpoint);
			TranslationEndpointManager translationEndpointManager = AllEndpoints.FirstOrDefault((TranslationEndpointManager x) => x.Endpoint.Id == Settings.FallbackServiceEndpoint);
			if (translationEndpointManager != null)
			{
				if (translationEndpointManager.Error != null)
				{
					XuaLogger.AutoTranslator.Error(translationEndpointManager.Error, "Error occurred during the initialization of the fallback translate endpoint.");
				}
				else
				{
					FallbackEndpoint = translationEndpointManager;
				}
			}
			TranslationEndpointManager translationEndpointManager2 = AllEndpoints.FirstOrDefault((TranslationEndpointManager x) => x.Endpoint.Id == Settings.ServiceEndpoint);
			if (translationEndpointManager2 != null)
			{
				if (translationEndpointManager2.Error != null)
				{
					XuaLogger.AutoTranslator.Error(translationEndpointManager2.Error, "Error occurred during the initialization of the selected translate endpoint.");
				}
				else
				{
					if (translationEndpointManager == translationEndpointManager2)
					{
						XuaLogger.AutoTranslator.Warn("Cannot use same fallback endpoint as primary.");
					}
					CurrentEndpoint = translationEndpointManager2;
				}
			}
			else if (!string.IsNullOrEmpty(Settings.ServiceEndpoint))
			{
				XuaLogger.AutoTranslator.Error("Could not find the configured endpoint '" + Settings.ServiceEndpoint + "'.");
			}
			if (Settings.DisableCertificateValidation)
			{
				XuaLogger.AutoTranslator.Debug("Disabling certificate checks for endpoints because of configuration.");
				ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, (RemoteCertificateValidationCallback)((object a1, X509Certificate a2, X509Chain a3, SslPolicyErrors a4) => true));
			}
			else
			{
				RemoteCertificateValidationCallback certificateValidationCheck = httpSecurity.GetCertificateValidationCheck();
				if (certificateValidationCheck != null && !ClrFeatures.SupportsNet4x)
				{
					XuaLogger.AutoTranslator.Debug("Disabling certificate checks for endpoints because a .NET 3.x runtime is used.");
					ServicePointManager.ServerCertificateValidationCallback = (RemoteCertificateValidationCallback)Delegate.Combine(ServicePointManager.ServerCertificateValidationCallback, certificateValidationCheck);
				}
				else
				{
					XuaLogger.AutoTranslator.Debug("Not disabling certificate checks for endpoints because a .NET 4.x runtime is used.");
				}
			}
			Settings.Save();
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "An error occurred while constructing endpoints. Shutting plugin down.");
			Settings.IsShutdown = true;
		}
	}

	public void CreateEndpoints(HttpSecurity httpSecurity)
	{
		if (Settings.FromLanguage != Settings.Language)
		{
			List<Type> allTypesOf = AssemblyLoader.GetAllTypesOf<ITranslateEndpoint>(Settings.TranslatorsPath);
			allTypesOf.Add(typeof(PassthroughTranslateEndpoint));
			{
				foreach (Type item in allTypesOf)
				{
					AddEndpoint(httpSecurity, item);
				}
				return;
			}
		}
		XuaLogger.AutoTranslator.Warn("AutoTranslator has been configured to use same destination language as source language. All translators will be disabled!");
	}

	private void AddEndpoint(HttpSecurity httpSecurity, Type type)
	{
		ITranslateEndpoint translateEndpoint;
		try
		{
			translateEndpoint = (ITranslateEndpoint)Activator.CreateInstance(type);
		}
		catch (Exception e)
		{
			XuaLogger.AutoTranslator.Error(e, "Could not instantiate class: " + type.Name);
			return;
		}
		InitializationContext context = new InitializationContext(httpSecurity, Settings.FromLanguage, Settings.Language);
		try
		{
			translateEndpoint.Initialize(context);
			TranslationEndpointManager translationEndpointManager = new TranslationEndpointManager(translateEndpoint, null, context);
			RegisterEndpoint(translationEndpointManager);
		}
		catch (Exception error)
		{
			TranslationEndpointManager translationEndpointManager2 = new TranslationEndpointManager(translateEndpoint, error, context);
			RegisterEndpoint(translationEndpointManager2);
		}
	}

	public void Update()
	{
		int count = _updateCallbacks.Count;
		for (int i = 0; i < count; i++)
		{
			try
			{
				_updateCallbacks[i].Update();
			}
			catch (Exception e)
			{
				XuaLogger.AutoTranslator.Error(e, "An error occurred while calling update on " + _updateCallbacks[i].GetType().Name + ".");
			}
		}
	}

	public void KickoffTranslations()
	{
		List<TranslationEndpointManager> endpointsWithUnstartedJobs = _endpointsWithUnstartedJobs;
		for (int num = endpointsWithUnstartedJobs.Count - 1; num >= 0; num--)
		{
			TranslationEndpointManager translationEndpointManager = endpointsWithUnstartedJobs[num];
			if (Settings.EnableBatching && translationEndpointManager.CanBatch)
			{
				while (translationEndpointManager.HasUnstartedBatch && !translationEndpointManager.IsBusy)
				{
					translationEndpointManager.HandleNextBatch();
				}
			}
			else
			{
				while (translationEndpointManager.HasUnstartedJob && !translationEndpointManager.IsBusy)
				{
					translationEndpointManager.HandleNextJob();
				}
			}
		}
	}

	public void ScheduleUnstartedJobs(TranslationEndpointManager endpoint)
	{
		_endpointsWithUnstartedJobs.Add(endpoint);
	}

	public void UnscheduleUnstartedJobs(TranslationEndpointManager endpoint)
	{
		_endpointsWithUnstartedJobs.Remove(endpoint);
	}

	public void ClearAllJobs()
	{
		foreach (TranslationEndpointManager configuredEndpoint in ConfiguredEndpoints)
		{
			configuredEndpoint.ClearAllJobs();
		}
	}

	public void RebootAllEndpoints()
	{
		foreach (TranslationEndpointManager configuredEndpoint in ConfiguredEndpoints)
		{
			configuredEndpoint.ConsecutiveErrors = 0;
		}
	}

	public void RegisterEndpoint(TranslationEndpointManager translationEndpointManager)
	{
		translationEndpointManager.Manager = this;
		AllEndpoints.Add(translationEndpointManager);
		if (translationEndpointManager.Error == null)
		{
			ConfiguredEndpoints.Add(translationEndpointManager);
		}
		if (translationEndpointManager.Endpoint is IMonoBehaviour_Update item)
		{
			_updateCallbacks.Add(item);
		}
	}

	public void InvokeJobCompleted(TranslationJob job)
	{
		this.JobCompleted?.Invoke(job);
	}

	public void InvokeJobFailed(TranslationJob job)
	{
		this.JobFailed?.Invoke(job);
	}
}
