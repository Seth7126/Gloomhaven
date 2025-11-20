using System;

namespace UnityEngine.InputSystem.Utilities;

internal static class DelegateHelpers
{
	public static void InvokeCallbacksSafe(ref CallbackArray<Action> callbacks, string callbackName, object context = null)
	{
		if (callbacks.length == 0)
		{
			return;
		}
		callbacks.LockForChanges();
		for (int i = 0; i < callbacks.length; i++)
		{
			try
			{
				callbacks[i]();
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				if (context != null)
				{
					Debug.LogError($"{ex.GetType().Name} while executing '{callbackName}' callbacks of '{context}'");
				}
				else
				{
					Debug.LogError(ex.GetType().Name + " while executing '" + callbackName + "' callbacks");
				}
			}
		}
		callbacks.UnlockForChanges();
	}

	public static void InvokeCallbacksSafe<TValue>(ref CallbackArray<Action<TValue>> callbacks, TValue argument, string callbackName, object context = null)
	{
		if (callbacks.length == 0)
		{
			return;
		}
		callbacks.LockForChanges();
		for (int i = 0; i < callbacks.length; i++)
		{
			try
			{
				callbacks[i](argument);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				if (context != null)
				{
					Debug.LogError($"{ex.GetType().Name} while executing '{callbackName}' callbacks of '{context}'");
				}
				else
				{
					Debug.LogError(ex.GetType().Name + " while executing '" + callbackName + "' callbacks");
				}
			}
		}
		callbacks.UnlockForChanges();
	}

	public static void InvokeCallbacksSafe<TValue1, TValue2>(ref CallbackArray<Action<TValue1, TValue2>> callbacks, TValue1 argument1, TValue2 argument2, string callbackName, object context = null)
	{
		if (callbacks.length == 0)
		{
			return;
		}
		callbacks.LockForChanges();
		for (int i = 0; i < callbacks.length; i++)
		{
			try
			{
				callbacks[i](argument1, argument2);
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				if (context != null)
				{
					Debug.LogError($"{ex.GetType().Name} while executing '{callbackName}' callbacks of '{context}'");
				}
				else
				{
					Debug.LogError(ex.GetType().Name + " while executing '" + callbackName + "' callbacks");
				}
			}
		}
		callbacks.UnlockForChanges();
	}

	public static bool InvokeCallbacksSafe_AnyCallbackReturnsTrue<TValue1, TValue2>(ref CallbackArray<Func<TValue1, TValue2, bool>> callbacks, TValue1 argument1, TValue2 argument2, string callbackName, object context = null)
	{
		if (callbacks.length == 0)
		{
			return true;
		}
		callbacks.LockForChanges();
		for (int i = 0; i < callbacks.length; i++)
		{
			try
			{
				if (callbacks[i](argument1, argument2))
				{
					return true;
				}
			}
			catch (Exception ex)
			{
				Debug.LogException(ex);
				if (context != null)
				{
					Debug.LogError($"{ex.GetType().Name} while executing '{callbackName}' callbacks of '{context}'");
				}
				else
				{
					Debug.LogError(ex.GetType().Name + " while executing '" + callbackName + "' callbacks");
				}
			}
		}
		callbacks.UnlockForChanges();
		return false;
	}
}
