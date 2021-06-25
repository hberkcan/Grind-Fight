#if UNITY_EDITOR
using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AnalyticsSDKPreProcessorDefinitions : AssetPostprocessor
{
	private static readonly string ELEPHANT_TYPE_NAME = "ElephantSDK.Elephant";
	private static readonly string FB_TYPE_NAME = "Facebook.Unity.FB, Facebook.Unity";
	private static readonly string SYMBOL_ELEPHANT_EXISTS = "ELEPHANT_SDK_EXISTS";
	private static readonly string SYMBOL_FB_EXISTS = "FB_SDK_EXISTS";


	private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
	                                           string[] movedFromAssetPaths)
	{
		var elephantExists = Type.GetType(ELEPHANT_TYPE_NAME) != null;

		SetSymbol(BuildTargetGroup.iOS, SYMBOL_ELEPHANT_EXISTS, elephantExists);
		SetSymbol(BuildTargetGroup.Android, SYMBOL_ELEPHANT_EXISTS, elephantExists);


		var fbExists = Type.GetType(FB_TYPE_NAME) != null;

		SetSymbol(BuildTargetGroup.iOS, SYMBOL_FB_EXISTS, fbExists);
		SetSymbol(BuildTargetGroup.Android, SYMBOL_FB_EXISTS, fbExists);
	}

	private static void SetSymbol(BuildTargetGroup group, string symbol, bool enabled)
	{
		PlayerSettings.GetScriptingDefineSymbolsForGroup(group, out var defines);

		var definesList = defines.ToList();
		if (definesList.Contains(symbol))
		{
			if (enabled)
				return;

			definesList.Remove(symbol);
		}
		else
		{
			if (!enabled)
				return;

			definesList.Add(symbol);
		}


		PlayerSettings.SetScriptingDefineSymbolsForGroup(group, definesList.ToArray());
	}
}

#endif