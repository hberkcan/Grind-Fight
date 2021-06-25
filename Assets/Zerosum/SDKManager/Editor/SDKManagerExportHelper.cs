using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Zerosum.SDKManager
{
	public static class SDKManagerExportHelper
	{
		
		[MenuItem("Zerosum/Packages/Export Manifest")]
		public static void ExportPackagesManifest()
		{
			var dir = Directory.GetParent(Application.dataPath).FullName;
			var filePath = dir + "/" + "manifest.json";

			var allLocalMetas = AssetDatabase.FindAssets("t:LocalPackageMeta").Select(
				guid => AssetDatabase.LoadAssetAtPath<LocalPackageMeta>(AssetDatabase.GUIDToAssetPath(guid)));

			var col = new PackageInfoCollection();
			col.packageInfos = new List<PackageInfo>();
			foreach (var meta in allLocalMetas)
			{
				var inf = new PackageInfo();
				inf.name = meta.visibleName;
				inf.id = meta.id;
				inf.version = meta.version;
				inf.dependencies = meta.dependencies.Select(pkg => pkg.id).ToList();
				col.packageInfos.Add(inf);
			}

			var jsonData = JsonUtility.ToJson(col, true);
			File.WriteAllText(filePath, jsonData);
			EditorUtility.RevealInFinder(filePath);
		}
	}
}