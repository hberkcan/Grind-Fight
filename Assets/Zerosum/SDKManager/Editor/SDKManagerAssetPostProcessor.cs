using UnityEditor;

namespace Zerosum.SDKManager.Editor
{
	public class SDKManagerAssetPostProcessor : AssetPostprocessor
	{
		private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
		                                           string[] movedAssets,
		                                           string[] movedFromAssetPaths)
		{
			// if (SDKManagerWindow.Instance != null)
			// {
				SDKManager.StartFetch();
			// }
		}
	}
}
