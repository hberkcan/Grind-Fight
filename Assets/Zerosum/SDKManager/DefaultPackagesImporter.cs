#if UNITY_EDITOR

using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;


[InitializeOnLoad]
public static class DefaultPackagesImporter
{
	private static readonly string[] packages =
	{
		//"https://gitlab.com/zerosum1/base-package-container-project.git?path=/Assets/ZerosumBase",
		"com.unity.recorder",
		"com.unity.mathematics",
		"com.unity.editorcoroutines",
		"com.unity.2d.sprite"
	};

	//"com.coffee.ui-particle": "https://github.com/mob-sakai/ParticleEffectForUGUI.git",

	private static ListRequest listRequest;

	static DefaultPackagesImporter()
	{
		listRequest = Client.List();

		EditorApplication.update += ListProgress;
	}


	private static void ListProgress()
	{
		if (listRequest.IsCompleted)
		{
			if (listRequest.Status == StatusCode.Success)
			{
				EditorApplication.update -= ListProgress;

				var included = listRequest.Result;
				for (var i = 0; i < packages.Length; i++)
				{
					var package = packages[i];
					if (included.All(p => p.name != package))
					{
						Client.Add(package);
						Debug.Log($"Zerosum SDK Manager: Automatically installing/updating {package}");
						break;
					}
				}
			}
		}
	}
}

#endif
