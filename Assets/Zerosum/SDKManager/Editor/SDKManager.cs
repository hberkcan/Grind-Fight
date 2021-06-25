using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace Zerosum.SDKManager
{
	public static class SDKManager
	{
		public const string PACKAGE_IMPORT_WINDOW_TYPE_NAME =
			"UnityEditor.PackageImport, UnityEditor.CoreModule, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";

		public const string CACHE_PATH = "Assets/Zerosum/SDKManager/Editor/SDKManagerCache.asset";

		public const string URL_BUCKET = "https://storage.googleapis.com/zrs-sdk-kcff5x7cn7lglydgvu8o/";
		public static readonly string URL_MANIFEST = URL_BUCKET + "manifest.json";

		public static Action onFetchStart;
		public static Action onFetchComplete;
		public static Action onFetchFail;
		public static Action<PackageInfo> onInstallStart;
		public static Action<PackageInfo, float> onInstallProgress;
		public static Action<PackageInfo> onInstallFail;
		public static Action<PackageInfo> onInstallComplete;

		public static bool isFetching;

		public static bool isProcessingInstallQueue;

		public static bool packageImportWindowOpen;

		public static SDKManagerCache cache;

		public static Dictionary<string, LocalPackageMeta> localPackages;
		public static Dictionary<string, PackageInfo> packageInfos;
		public static List<PackageInfo> packageInfoList;

		public static PackageInfo currentInstallation;

		private static void TryProcessInstallQueue()
		{
			if (currentInstallation != null)
				return;

			if (!cache)
			{
				cache = GetCache();
			}

			var hasWaiting = cache.installQueue != null && cache.installQueue.Count > 0;

			if (isProcessingInstallQueue)
			{
				if (hasWaiting)
				{
					ContinueWithNextInstall();
				}
				else
				{
					isProcessingInstallQueue = false;
					currentInstallation = null;
					EditorApplication.delayCall += () =>
					{
						EditorApplication.UnlockReloadAssemblies();
						EditorApplication.delayCall += StartFetch;
					};
				}
			}
			else
			{
				if (hasWaiting)
				{
					EditorApplication.LockReloadAssemblies();
					isProcessingInstallQueue = true;
					ContinueWithNextInstall();
				}
			}
		}

		private static void ContinueWithNextInstall()
		{
			var id = cache.installQueue[0];
			cache.installQueue.RemoveAt(0);
			EditorUtility.SetDirty(cache);
			AssetDatabase.SaveAssets();

			InstallPackageInternal(id);
		}

		private static void InstallPackageInternal(string id)
		{
			var inf = packageInfos[id];

			if (inf.localState >= PackageLocalState.UpToDate)
			{
				currentInstallation = null;
				TryProcessInstallQueue();
				return;
			}

			currentInstallation = inf;
			onInstallStart?.Invoke(inf);

			var url = GetPackageURL(inf);
			var tempPath = Path.GetTempFileName();
			tempPath = Path.GetDirectoryName(tempPath) + "/" + inf.name + ".unitypackage";

			SDKManagerUtil.DownloadFile(url, tempPath, prog => { onInstallProgress?.Invoke(inf, prog); }, success =>
			{
				if (!success)
				{
					onInstallFail?.Invoke(inf);
					currentInstallation = null;
					TryProcessInstallQueue();
					return;
				}

				AddImportPackageCallbacks();

				packageImportWindowOpen = true;

				//EditorCoroutineUtility.StartCoroutineOwnerless(CheckPackageImportWindowClosedManually());

				AssetDatabase.ImportPackage(tempPath, false);
			});
		}


		private static void AddImportPackageCallbacks()
		{
			AssetDatabase.importPackageStarted += OnImportPackageStarted;
			AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
			AssetDatabase.importPackageCancelled += OnImportPackageCancelled;
			AssetDatabase.importPackageFailed += OnImportPackageFailed;
		}

		private static void RemoveImportPackageCallbacks()
		{
			AssetDatabase.importPackageStarted -= OnImportPackageStarted;
			AssetDatabase.importPackageCompleted -= OnImportPackageCompleted;
			AssetDatabase.importPackageCancelled -= OnImportPackageCancelled;
			AssetDatabase.importPackageFailed -= OnImportPackageFailed;
		}

		private static void OnImportPackageStarted(string packagename)
		{
		}

		private static void OnImportPackageCompleted(string packagename)
		{
			RemoveImportPackageCallbacks();

			packageImportWindowOpen = false;

			// currentInstallation.localState = PackageLocalState.UpToDate;
			packageInfoList.First(pkg => pkg.id == currentInstallation.id).localState = PackageLocalState.UpToDate;
			var pkg = currentInstallation;
			currentInstallation = null;
			onInstallComplete?.Invoke(pkg);

			EditorApplication.delayCall += TryProcessInstallQueue;

			// EditorApplication.delayCall += StartFetch;
		}

		private static void OnImportPackageFailed(string packagename, string errormessage)
		{
			RemoveImportPackageCallbacks();
			packageImportWindowOpen = false;


			onInstallFail?.Invoke(currentInstallation);
			currentInstallation = null;
			TryProcessInstallQueue();

			// EditorApplication.delayCall += StartFetch;
		}

		private static void OnImportPackageCancelled(string packagename)
		{
			RemoveImportPackageCallbacks();
			packageImportWindowOpen = false;

			onInstallFail?.Invoke(currentInstallation);
			currentInstallation = null;
			TryProcessInstallQueue();

			// EditorApplication.delayCall += StartFetch;
		}


		public static string GetPackageURL(PackageInfo inf)
		{
			return URL_BUCKET + inf.id + ".unitypackage";
		}

		public static SDKManagerCache GetCache()
		{
			var relativePath = CACHE_PATH;
			var cache = AssetDatabase.LoadAssetAtPath<SDKManagerCache>(relativePath);

			if (!cache)
			{
				cache = ScriptableObject.CreateInstance<SDKManagerCache>();
				var absolutePath = Path.GetFullPath(relativePath);
				var dir = Path.GetDirectoryName(absolutePath);
				Directory.CreateDirectory(dir);
				AssetDatabase.Refresh();
				AssetDatabase.CreateAsset(cache, relativePath);
				AssetDatabase.SaveAssets();
			}

			return cache;
		}


		public static void StartFetch()
		{
			if (isFetching)
				return;

			if (isProcessingInstallQueue || currentInstallation != null)
				return;

			if (!Resources.FindObjectsOfTypeAll<SDKManagerWindow>().Any())
			{
				// No SDK Manager window, no need to fetch.
				return;
			}

			isFetching = true;

			onFetchStart?.Invoke();

			var localMetaGuids = AssetDatabase.FindAssets("t:LocalPackageMeta");
			localPackages = localMetaGuids
			                .Select(guid => AssetDatabase.LoadAssetAtPath<LocalPackageMeta>(
				                        AssetDatabase.GUIDToAssetPath(guid)))
			                .ToDictionary(meta => meta.id);

			// Debug.Log(URL_MANIFEST);
			SDKManagerUtil.DownloadText(URL_MANIFEST, OnManifestReceived);
		}

		private static void OnManifestReceived(string json)
		{
			isFetching = false;
			if (string.IsNullOrEmpty(json))
			{
				Debug.Log("Failed to fetch manifest");
				onFetchFail?.Invoke();
				return;
			}

#if ZEROSUM_SDK_MANAGER_DEBUG
			Debug.Log("Manifest: " + json);
#endif

			var col = JsonUtility.FromJson<PackageInfoCollection>(json);
			packageInfoList = col.packageInfos.OrderBy(pkg => pkg.name).ToList();
			packageInfos = packageInfoList.ToDictionary(pkg => pkg.id);

			var localMetaGuids = AssetDatabase.FindAssets("t:LocalPackageMeta");
			localPackages = localMetaGuids
			                .Select(guid => AssetDatabase.LoadAssetAtPath<LocalPackageMeta>(
				                        AssetDatabase.GUIDToAssetPath(guid)))
			                .ToDictionary(meta => meta.id);


			foreach (var inf in packageInfos.Values)
			{
				if (localPackages.TryGetValue(inf.id, out var localMeta))
				{
					inf.localVersion = localMeta.version;
					inf.localState = SDKManagerUtil.CompareVersionStrings(inf.version, localMeta.version) > 0
						? PackageLocalState.RequiresUpdate
						: PackageLocalState.UpToDate;
				}
				else
				{
					inf.localState = PackageLocalState.NotInstalled;
				}

				inf.aliveDependencies = inf.dependencies.Select(id => packageInfos[id]).ToList();
			}

			onFetchComplete?.Invoke();

			EditorApplication.delayCall += TryProcessInstallQueue;
		}


		public static void TryInstallOrUpdatePackages(List<PackageInfo> packages)
		{
			if (isProcessingInstallQueue)
				return;

			var resolvedPackages = new List<PackageInfo>();

			foreach (var inf in packages)
			{
				ResolveDependencies(resolvedPackages, inf);
			}

			if (resolvedPackages.All(pkg => pkg.localState >= PackageLocalState.UpToDate))
			{
				// Resolved packages are up to date.
				Debug.Log($"Packages are already installed and up to date.");
				return;
			}

			resolvedPackages = resolvedPackages.Where(pkg => pkg.localState < PackageLocalState.UpToDate).ToList();

			var cache = GetCache();
			if (cache.installQueue == null)
				cache.installQueue = new List<string>();
			cache.installQueue.AddRange(resolvedPackages.Select(pkg => pkg.id).ToList());
			EditorUtility.SetDirty(cache);
			AssetDatabase.SaveAssets();
			AssetDatabase.SaveAssets();
			EditorApplication.delayCall += TryProcessInstallQueue;
		}

		public static void ResolveDependencies(List<PackageInfo> resolvedPackages, PackageInfo target,
		                                       bool includeThis = true)
		{
			foreach (var dependency in target.aliveDependencies.Where(dep => resolvedPackages.Contains(dep) == false))
			{
				ResolveDependencies(resolvedPackages, dependency);
			}

			if (includeThis && !resolvedPackages.Contains(target))
			{
				resolvedPackages.Add(target);
			}
		}


		private static IEnumerator CheckPackageImportWindowClosedManually()
		{
			var importWinType = Type.GetType(PACKAGE_IMPORT_WINDOW_TYPE_NAME);

			while (Resources.FindObjectsOfTypeAll(importWinType).Length <= 0)
			{
				yield return null;
			}

			while (Resources.FindObjectsOfTypeAll(importWinType).Length > 0)
			{
				yield return null;
			}

			for (int i = 0; i < 10; i++)
			{
				yield return null;
			}

			if (packageImportWindowOpen)
			{
				// Closed by window close button
				OnImportPackageCancelled(null);
			}
		}

		public static void AbortAllProcesses()
		{
			var cache = GetCache();
			cache.installQueue = new List<string>();
			EditorUtility.SetDirty(cache);
			AssetDatabase.SaveAssets();

			isProcessingInstallQueue = false;
		}
	}
}
