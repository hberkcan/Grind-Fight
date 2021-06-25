using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityCommon.Runtime.Utility
{
	public class SceneLoader : MonoBehaviour
	{
		public bool useBuildIndex = false;

		[HideIf("$useBuildIndex")]
		public string sceneName = "Main";

		[ShowIf("$useBuildIndex")]
		public int buildIndex = 1;

		public void Load()
		{
			if (useBuildIndex)
			{
				SceneManager.LoadScene(buildIndex);
			}
			else
			{
				SceneManager.LoadScene(sceneName);
			}
		}
	}
}
