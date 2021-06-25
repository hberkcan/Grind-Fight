using System;
using System.Collections;
using System.IO;
using Unity.EditorCoroutines.Editor;
using UnityEngine;
using UnityEngine.Networking;

namespace Zerosum.SDKManager
{
	public static class SDKManagerUtil
	{
		public static void DownloadText(string url, Action<string> onResult)
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(DownloadTextInternal(url, onResult));
		}

		private static IEnumerator DownloadTextInternal(string url, Action<string> onResult)
		{
			var req = UnityWebRequest.Get(url);

			yield return req.SendWebRequest();

			var success = req.result == UnityWebRequest.Result.Success;

			if (success)
			{
				onResult?.Invoke(req.downloadHandler.text);
			}
			else
			{
				Debug.LogError($"{url} - Error: {req.error}");
				Debug.LogError(req.downloadHandler.error);
				onResult?.Invoke(null);
			}
		}

		public static void DownloadFile(string url, string filePath, Action<float> onProgress, Action<bool> onResult)
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(DownloadFileInternal(url, filePath, onProgress, onResult));
		}

		private static IEnumerator DownloadFileInternal(string url, string filePath, Action<float> onProgress,
		                                                Action<bool> onResult)
		{
			using var req = UnityWebRequest.Get(url);

			req.SendWebRequest();

			while (!req.isDone)
			{
				onProgress?.Invoke(req.downloadProgress);
				yield return null;
			}

			onProgress?.Invoke(req.downloadProgress);

			var success = req.result == UnityWebRequest.Result.Success;

			if (success)
			{
				var bytes = req.downloadHandler.data;
				File.WriteAllBytes(filePath, bytes);
			}
			else
			{
				Debug.LogError($"{url} - Error: {req.error}");
			}

			onResult?.Invoke(success);
		}

		public static int CompareVersionStrings(string ver1, string ver2)
		{
			return new Version(ver1).CompareTo(new Version(ver2));
		}
	}
}
