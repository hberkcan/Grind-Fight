using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Zerosum.SDKManager
{
	[CreateAssetMenu(menuName = "Zerosum/Packages/Local Package Metadata", fileName = "LocalPackageMeta")]
	public class LocalPackageMeta : ScriptableObject
	{
		public string visibleName;
		public string id;
		public string version;

		public List<LocalPackageMeta> dependencies;

		// public List<string> pathsToExport;


		public override bool Equals(object obj)
		{
			if (obj is LocalPackageMeta other)
			{
				return string.CompareOrdinal(other.id, id) == 0;
			}

			return false;
		}

		public override int GetHashCode()
		{
			return id.GetHashCode();
		}
	}
}

// #if UNITY_EDITOR
// namespace Zerosum.SDKManager.Editor
// {
// 	[CustomEditor(typeof(LocalPackageMeta))]
// 	public class LocalPackageMetaEditor : UnityEditor.Editor
// 	{
// 		private LocalPackageMeta meta;
//
// 		private void OnEnable()
// 		{
// 			meta = (LocalPackageMeta) target;
// 		}
//
// 		public override void OnInspectorGUI()
// 		{
// 			base.OnInspectorGUI();
//
// 			if (meta.pathsToExport == null)
// 			{
// 				meta.pathsToExport = new List<string>();
// 				EditorUtility.SetDirty(meta);
// 			}
//
// 		}
// 	}
// }
// #endif
