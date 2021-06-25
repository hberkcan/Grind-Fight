#if ENABLE_UNITY_EVENTS
#define SEND_UNITY_EVENTS
#endif

#if ELEPHANT_SDK_EXISTS
#define SEND_ELEPHANT_EVENTS
#endif

#if FB_SDK_EXISTS && ENABLE_FB_EVENTS
#define SEND_FB_EVENTS
#endif

#if !ELEPHANT_SDK_EXISTS && (!FB_SDK_EXISTS || !ENABLE_FB_EVENTS) && !ENABLE_UNITY_EVENTS
#define NO_SDK
#endif

using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
#if ELEPHANT_SDK_EXISTS
using ElephantSDK;
#endif
#if FB_SDK_EXISTS
using Facebook.Unity;

#endif

#pragma warning disable 0414
#pragma warning disable 0162

namespace Zerosum
{
#if UNITY_EDITOR
	[InitializeOnLoad]
#endif
	public static class Analytics
	{
		private const string LEVEL = "level";
		private const string LEVEL_START = "level_start";
		private const string LEVEL_FAIL = "level_fail";
		private const string LEVEL_COMPLETE = "level_complete";

		static Analytics()
		{
#if !FB_SDK_EXISTS && ENABLE_FB_EVENTS
			Debug.LogError("ENABLE_FB_EVENTS symbol is defined in the project settings but Facebook Unity SDK does not exist in the project. FB events will not be sent.");
#endif
		}

		public enum ProgressionEventType
		{
			None = 0,
			Started = 1,
			Completed = 2,
			Failed = 3
		}

		private static int lastLevel = -1;

		private static ProgressionEventType lastProgressionEventType = ProgressionEventType.None;

		private static Dictionary<string, object> dict = new Dictionary<string, object>(8);

		public static void ResetProgressionState()
		{
			lastLevel = -1;
			lastProgressionEventType = ProgressionEventType.None;
		}

		public static void LevelStarted(int level)
		{
#if UNITY_EDITOR
			if (lastProgressionEventType == ProgressionEventType.Started)
			{
				throw new Exception("Analytics.LevelStarted cannot be called more than once in a row!");
			}

			if (lastProgressionEventType == ProgressionEventType.Failed)
			{
				if (level != lastLevel)
				{
					throw new Exception(
						$"Analytics.LevelStarted should be called with the same level number after a LevelFailed call. Last call was LevelFailed({lastLevel}) and this call is LevelStarted({level}. This call is expected to be LevelStarted({lastLevel})");
				}
			}
			else if (lastProgressionEventType == ProgressionEventType.Completed)
			{
				if (level != lastLevel + 1)
				{
					throw new Exception(
						$"Analytics.LevelStarted should be called with the incremented level number after a LevelCompleted call. Last call was LevelCompleted({lastLevel}) and this call is LevelStarted({level}). This call is expected to be LevelStarted({lastLevel + 1})");
				}
			}

			lastProgressionEventType = ProgressionEventType.Started;
			lastLevel = level;


			Debug.Log($"Analytics.LevelStarted: {level}");
#endif
			dict[LEVEL] = level;

#if SEND_UNITY_EVENTS
			UnityEngine.Analytics.Analytics.CustomEvent(LEVEL_START, dict);
#endif

#if SEND_ELEPHANT_EVENTS
			Elephant.LevelStarted(level);
#endif

#if SEND_FB_EVENTS
			FBEvent(LEVEL_STARTED, null, dict);
#endif

#if NO_SDK
	Debug.LogError("Analytics.LevelStarted was called but there is no available and enabled SDK in the project!");
#endif

			dict.Clear();
		}


		public static void LevelCompleted(int level)
		{
#if UNITY_EDITOR
			if (lastProgressionEventType == ProgressionEventType.None)
			{
				throw new Exception(
					"Analytics.LevelCompleted can only be called after a LevelStart call! No previous progression calls were made.");
			}

			if (lastProgressionEventType == ProgressionEventType.Failed)
			{
				throw new Exception(
					$"Analytics.LevelCompleted can only be called after a LevelStart call! Last call was a LevelFailed({lastLevel}) call. A LevelStart({lastLevel}) call is expected.");
			}

			if (lastProgressionEventType == ProgressionEventType.Completed)
			{
				throw new Exception("Analytics.LevelCompleted cannot be called more than once in a row!");
			}

			if (level != lastLevel)
			{
				// Does not complete started level
				throw new Exception(
					$"Analytics.LevelCompleted should be called with the same level number after a LevelStart call. Last call was LevelStarted({lastLevel}), this call is LevelCompleted({level}). This call is expected to be LevelCompleted({lastLevel})");
			}

			lastProgressionEventType = ProgressionEventType.Completed;
			Debug.Log($"Analytics.LevelCompleted: {level}");
#endif

			dict[LEVEL] = level;

#if SEND_UNITY_EVENTS
			UnityEngine.Analytics.Analytics.CustomEvent(LEVEL_COMPLETE, dict);
#endif

#if SEND_ELEPHANT_EVENTS
			Elephant.LevelCompleted(level);
#endif

#if SEND_FB_EVENTS
			FBEvent(LEVEL_COMPLETED, null, dict);
#endif

#if NO_SDK
	Debug.Log("Analytics.LevelCompleted was called but there is no available and enabled SDK in the project!");
#endif

			dict.Clear();
		}

		public static void LevelFailed(int level)
		{
#if UNITY_EDITOR
			if (lastProgressionEventType == ProgressionEventType.None)
			{
				throw new Exception(
					"Analytics.LevelFailed can only be called after a LevelStart call! No previous progression calls were made.");
			}

			if (lastProgressionEventType == ProgressionEventType.Failed)
			{
				throw new Exception(
					"Analytics.LevelFailed cannot be called more than once in a row!");
			}

			if (lastProgressionEventType == ProgressionEventType.Completed)
			{
				throw new Exception(
					$"Analytics.LevelFailed can only be called after a LevelStart call! Last call was LevelCompleted({lastLevel}). A LevelStart({lastLevel + 1}) call is expected.");
			}

			if (level != lastLevel)
			{
				// Does not fail started level
				throw new Exception(
					$"Analytics.LevelFailed should be called with the same level number after a LevelStart call. Last call was LevelStarted({lastLevel}), this call is LevelFailed({level}). This call is expected to be LevelFailed({lastLevel})");
			}

			lastProgressionEventType = ProgressionEventType.Failed;
			Debug.Log($"Analytics.LevelFailed: {level}");
#endif

			dict[LEVEL] = level;

#if SEND_UNITY_EVENTS
			UnityEngine.Analytics.Analytics.CustomEvent(LEVEL_FAIL, dict);
#endif

#if SEND_ELEPHANT_EVENTS
			Elephant.LevelFailed(level);
#endif
#if SEND_FB_EVENTS
			FBEvent(LEVEL_FAILED, null, dict);
#endif
#if NO_SDK
	Debug.LogError("Analytics.LevelFailed was called but there is no available and enabled SDK in the project!");
#endif

			dict.Clear();
		}

		public static void ProgressionEvent(ProgressionEventType progressionEventType, int level)
		{
			switch (progressionEventType)
			{
				case ProgressionEventType.Started:
					LevelStarted(level);
					break;
				case ProgressionEventType.Completed:
					LevelCompleted(level);
					break;
				case ProgressionEventType.Failed:
					LevelFailed(level);
					break;
			}
		}

		public static void Event(string eventName, int level)
		{
			if (string.IsNullOrEmpty(eventName))
			{
#if UNITY_EDITOR
				throw new Exception("Analytics.Event cannot be called with a null or empty event name.");
#endif
				return;
			}

#if SEND_ELEPHANT_EVENTS
			var para = Params.New();
			foreach (var kv in dict)
			{
				var key = kv.Key;
				var val = kv.Value;
				if (val is int vali)
				{
					para.Set(key, vali);
				}
				else if (val is string vals)
				{
					para.Set(key, vals);
				}
				else if (val is float valf)
				{
					para.Set(key, valf);
				}
			}

			Elephant.Event(eventName, level, para);
#endif


			dict[LEVEL] = level;

#if UNITY_EDITOR
			Debug.Log($"Analytics.Event: {eventName} @ {level}");
#endif

#if SEND_UNITY_EVENTS
			UnityEngine.Analytics.Analytics.CustomEvent(eventName, dict);
#endif

#if SEND_FB_EVENTS
			FBEvent(eventName, null, dict);
#endif

#if NO_SDK
	Debug.LogError("Analytics.Event was called but there is no available and enabled SDK in the project!");
#endif
			dict.Clear();
		}

		/// <summary>
		/// Set a key value pair for the next event call. The data is cleared after each event call.
		/// </summary>
		public static void SetParam(string key, object val)
		{
			dict[key] = val;
		}

#if FB_SDK_EXISTS
		private static void FBEvent(string eventName, float? valueToSum, Dictionary<string, object> dict)
		{
			if (!FB.IsInitialized)
			{
#if UNITY_EDITOR
				Debug.Log($"Analytics.FBEvent({eventName}) was called but FB SDK is not initialized!");
#endif
				return;
			}

			FB.LogAppEvent(eventName, valueToSum, dict);
		}
#endif
	}
}
