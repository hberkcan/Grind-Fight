using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Zerosum
{
#if UNITY_EDITOR
	public static class BatchModeBuildHelper
	{
		private static readonly string[] scenes = FindEnabledEditorScenes();

		private static void SetupiOSManualProvProfile()
		{
			PlayerSettings.iOS.appleEnableAutomaticSigning = true;
			PlayerSettings.iOS.appleDeveloperTeamID = "ZSS8452R72";
		}

		public static void BuildIOS()
		{
			SetupiOSManualProvProfile();

			File.WriteAllText(Application.dataPath.Replace("Assets", "") + "bundle_id.txt",
			                  PlayerSettings.applicationIdentifier);

			EditorApplication.ExecuteMenuItem("File/Save Project");

			var locationPath = Directory.GetParent(Application.dataPath).FullName + "/Build";
			Console.WriteLine($"Build Started {locationPath}");
			var opt = new BuildPlayerOptions();
			opt.options = BuildOptions.CompressWithLz4;
			opt.scenes = scenes;
			opt.target = BuildTarget.iOS;
			opt.locationPathName = locationPath;
			var report = BuildPipeline.BuildPlayer(opt);

			if (report.summary.result == BuildResult.Succeeded)
			{
				EditorApplication.Exit(10);
			}
			else
			{
				Console.WriteLine("\r\nError Building Unity Project \r\n\r\n" +
				                  string.Join("\n-----------\n",
				                              report.steps.Select(bs => string.Join("\n", bs.messages))));
				EditorApplication.Exit(-1);
			}
		}

		private static string[] FindEnabledEditorScenes()
		{
			List<string> EditorScenes = new List<string>();
			foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
				if (scene.enabled)
					EditorScenes.Add(scene.path);

			return EditorScenes.ToArray();
		}
	}
#endif
}
