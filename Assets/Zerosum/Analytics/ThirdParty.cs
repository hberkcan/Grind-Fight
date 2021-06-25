using UnityEngine;
using UnityCommon.Singletons;
#if FB_SDK_EXISTS
using Facebook.Unity;

#endif

[DefaultExecutionOrder(-300)]
public class ThirdParty : SingletonBehaviour<ThirdParty>
{
	private void Awake()
	{
		if (!SetupInstance())
		{
			return;
		}

		//GameAnalytics.Initialize();

		HandleFBInit();
	}


	private void HandleFBInit()
	{
#if FB_SDK_EXISTS
		if (FB.IsInitialized)
		{
			FB.ActivateApp();
		}
		else
		{
			FB.Init(OnInitCompleted, OnHideUnity);
		}
#endif
	}

	private void OnInitCompleted()
	{
#if FB_SDK_EXISTS
		FB.ActivateApp();
#endif
	}


	private void OnHideUnity(bool isGameShown)
	{
		if (!isGameShown)
		{
			Time.timeScale = 0;
		}
		else
		{
			Time.timeScale = 1;
		}
	}
}
