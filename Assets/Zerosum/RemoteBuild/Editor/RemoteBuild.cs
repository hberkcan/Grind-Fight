using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Networking;

namespace Zerosum
{
#if UNITY_EDITOR
	[System.Serializable]
	public class RequestData
	{
		public string type;
		public string url;
		public string branch;
	}

	public static class RemoteBuild
	{
		private const string HOST_RESOLVE_URL =
			"https://storage.googleapis.com/zrs-sdk-kcff5x7cn7lglydgvu8o/remote-build/host.txt";

		private static string _HOST = null;
		public static string HOST
		{
			get
			{
				if(string.IsNullOrEmpty(_HOST))
				{
					using var wc = new WebClient();
					_HOST = wc.DownloadString(HOST_RESOLVE_URL);	
				}

				return _HOST;
			}
		}


		public static void RequestRemoteBuild(string url, string branch, BuildMethod method)
		{
			EditorCoroutineUtility.StartCoroutineOwnerless(RequestRemoteBuildInternal(url, branch, method));
		}

		private static IEnumerator RequestRemoteBuildInternal(string url, string branch, BuildMethod method)
		{
			yield return null;

			var queryUrl =
				$"{HOST}:{11440}/build".EncodeParams(("url", url), ("branch", branch),
				                                     ("method", ((int) method).ToString()));
			Debug.Log(queryUrl);
			var req = UnityWebRequest.Get(queryUrl);
			yield return req.SendWebRequest();

			if (req.result == UnityWebRequest.Result.Success)
			{
				var resp = req.downloadHandler.text;
				Debug.Log(resp);
			}
			else
			{
				Debug.Log(req.result);
				Debug.Log("Error: " + req.error);
			}
		}


		public static void RequestBuildInfoCollection(string projectName, Action<BuildInfoCollection> onReceived)
		{
			Task.Run(() =>
			{
				using var hc = new HttpClient();
				var queryUrl = $"{HOST}:{11440}/info".EncodeParams(("name", projectName));
				var res = hc.GetAsync(queryUrl).Result;
				var text = res.Content.ReadAsStringAsync().Result;
				switch (res.StatusCode)
				{
					case HttpStatusCode.OK:
						EditorApplication.delayCall += () =>
						{
							var col = JsonUtility.FromJson<BuildInfoCollection>(text);
							onReceived.Invoke(col);
						};
						break;
					default:
						EditorApplication.delayCall += () =>
						{
							Debug.Log(text);
							onReceived.Invoke(null);
						};
						break;
				}
			}).ConfigureAwait(false);

			// EditorCoroutineUtility.StartCoroutineOwnerless(RequestBuildInfoCollectionInternal(projectName, onReceived));
		}


		public static string GetBuildLogLink(string id)
		{
			return $"{HOST}:{11440}/log?id={id}";
		}
	}
#endif
}
