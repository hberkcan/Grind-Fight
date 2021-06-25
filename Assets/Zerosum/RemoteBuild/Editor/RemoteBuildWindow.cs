using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Recorder;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using Zerosum;
using PopupWindow = UnityEditor.PopupWindow;


public class RemoteBuildWindow : EditorWindow
{
	[MenuItem("Zerosum/Remote Build")]
	public static void ShowExample()
	{
		RemoteBuildWindow win = GetWindow<RemoteBuildWindow>();
		win.titleContent = new GUIContent("Zerosum Remote Build");
		win.position = new Rect(450, 300, 650, 700);
		win.minSize = new Vector2(350, 300);
	}


	public string gitUrl;

	private VisualTreeAsset buildHistoryItemTree;

	private Label gitLbl;

	private ScrollView scrollView;

	private BuildMethod method;

	private bool guiCreated = false;

	private BuildMethod SelectedBuildMethod => (BuildMethod) rootVisualElement.Q<EnumField>().value;

	public void CreateGUI()
	{
		// Each editor window contains a root VisualElement object
		VisualElement root = rootVisualElement;

		// Import UXML
		var visualTree =
			AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/Zerosum/RemoteBuild/Editor/RemoteBuildWindow.uxml");
		visualTree.CloneTree(root);

		buildHistoryItemTree =
			AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
				"Assets/Zerosum/RemoteBuild/Editor/BuildHistoryItemElement.uxml");

		var styleSheet =
			AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Zerosum/RemoteBuild/Editor/RemoteBuildWindow.uss");
		root.styleSheets.Add(styleSheet);

		var btnBuild = root.Q<Button>("btn-build");

		btnBuild.clicked += OnClickRequestBuild;

		var right = root.Q("header-bg-right");
		var enumField = new EnumField(BuildMethod.Diawi);
		enumField.AddToClassList("field-build-method");
		right.Add(enumField);

		guiCreated = true;

		CacheElements();
		RefreshContent();
	}

	private void CacheElements()
	{
		var root = rootVisualElement;
		gitUrl = RemoteBuildHelpers.GetRepoInfo().url;
		gitLbl = root.Q<Label>("lbl-git-info");
		scrollView = root.Q<ScrollView>();
	}


	private void OnEnable()
	{
		CacheElements();
		RefreshContent();
	}

	private void OnFocus()
	{
		CacheElements();
		RefreshContent();
	}

	private void RefreshContent()
	{
		if (gitLbl == null)
			return;

		var git = RemoteBuildHelpers.GetRepoInfo();
		var infoTxt = $"{git.url} ({git.branch})";
		if (git.hasUncommitted)
		{
			infoTxt += "  ⚠️";
			gitLbl.tooltip = "You have uncommitted changes.";
		}

		gitLbl.text = infoTxt;

		PopulateBuildHistory();
	}

	private void PopulateBuildHistory()
	{
		var projName = Path.GetFileNameWithoutExtension(gitUrl);
		RemoteBuild.RequestBuildInfoCollection(projName, col => { PopulateBuildHistoryInternal(col); });
	}

	private void PopulateBuildHistoryInternal(BuildInfoCollection col)
	{
		if (col == null)
		{
			return;
		}

		var historyContainer = scrollView.contentContainer;
		var children = historyContainer.Children().ToList();
		foreach (var child in children)
		{
			child.RemoveFromHierarchy();
		}

		if (col == null || col.builds == null)
		{
			var lbl = new Label("Could not connect to Remote Build server.");
			lbl.style.alignSelf = Align.Center;
			lbl.style.marginTop = 32f;
			historyContainer.Add(lbl);
			return;
		}

		foreach (var buildInfo in col.builds)
		{
			var el = CreateBuildInfoElement(buildInfo, historyContainer);
			historyContainer.Add(el);
		}
	}

	private VisualElement CreateBuildInfoElement(BuildInfo info, VisualElement container)
	{
		buildHistoryItemTree.CloneTree(container);
		var itemRoot = container.Q<VisualElement>("history-item-root");
		itemRoot.name = info.id;

		var lblBranch = itemRoot.Q<Label>("lbl-branch");
		var lblMethod = itemRoot.Q<Label>("lbl-method");
		var lblStart = itemRoot.Q<Label>("lbl-start");
		var lblEnd = itemRoot.Q<Label>("lbl-end");
		var lblDuration = itemRoot.Q<Label>("lbl-duration");
		var lblState = itemRoot.Q<Label>("lbl-state");
		var lblCurrentOp = itemRoot.Q<Label>("lbl-current-op");
		var btnViewLog = itemRoot.Q<Button>("btn-view-logs");

		lblBranch.text = $"Branch: {info.branch}";
		lblStart.text = $"Started at: {info.start}";
		lblMethod.text = $"Method: {info.method.ToString()}";

		var logLink = RemoteBuild.GetBuildLogLink(info.id);

		switch (info.state)
		{
			case BuildState.None:
			case BuildState.Working:
				lblEnd.RemoveFromHierarchy();
				lblDuration.RemoveFromHierarchy();
				btnViewLog.RemoveFromHierarchy();
				lblState.text = $"State: {info.state.ToString()}";
				lblCurrentOp.text = $"Current operation: " +
				                    $"{(string.IsNullOrEmpty(info.currentOperation) ? "Starting build..." : info.currentOperation)}";
				break;
			case BuildState.Success:
				itemRoot.AddToClassList("build-success");
				lblCurrentOp.RemoveFromHierarchy();
				lblEnd.text = $"Finished at: {info.end}";
				lblDuration.text = $"Duration: {info.GetDurationFormatted()}";
				lblState.text = $"State: {info.state.ToString()}";
				btnViewLog.tooltip = $"Go to {logLink}";
				btnViewLog.clicked += () => { Application.OpenURL(logLink); };
				break;
			case BuildState.Failure:
				itemRoot.AddToClassList("build-failure");
				lblCurrentOp.RemoveFromHierarchy();
				var url = RemoteBuild.GetBuildLogLink(info.id);
				btnViewLog.tooltip = $"Go to {logLink}";
				btnViewLog.clicked += () => { Application.OpenURL(logLink); };
				lblEnd.text = $"Finished at: {info.end}";
				lblDuration.text = $"Duration: {info.GetDurationFormatted()}";
				lblState.text = $"State: {info.state.ToString()}";
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(info.state));
		}

		return itemRoot;

	}

	private void OnClickRequestBuild()
	{
		var git = RemoteBuildHelpers.GetRepoInfo();
		if (git.hasUncommitted)
		{
			var ok = EditorUtility.DisplayDialog("Remote Build",
			                                     "You have uncommitted changes. Do you want send the build request?",
			                                     "Yes",
			                                     "No");
			if (!ok)
			{
				return;
			}
		}

		if (SelectedBuildMethod == BuildMethod.AppStore)
		{
			var okBuild = EditorUtility.DisplayDialog("Remote Build",
			                                          $"You are sending a build request to upload the project to App Store Connect. Are you sure?",
			                                          "Yes",
			                                          "No");

			if (!okBuild)
			{
				return;
			}
		}

		Debug.Log("Build requested.");
		var btnBuild = rootVisualElement.Q<Button>("btn-build");
		btnBuild.SetEnabled(false);
		btnBuild.schedule.Execute(() => btnBuild.SetEnabled(true)).ExecuteLater(1000);

		SendBuildRequestInternal(git.url, git.branch);
	}


	private void SendBuildRequestInternal(string url, string branch)
	{
		var repo = RemoteBuildHelpers.GetRepoInfo();
		RemoteBuild.RequestRemoteBuild(repo.url, repo.branch, SelectedBuildMethod);
		rootVisualElement.schedule.Execute(PopulateBuildHistory)
		                 .ExecuteLater(500);
	}
}
