using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

#pragma warning disable 0414

namespace Zerosum.SDKManager
{
	public class SDKManagerWindow : EditorWindow
	{
		public const string URL_DEV_GUIDE =
			"https://docs.google.com/document/d/1fLTNBggrv3VjxXGa_ZSO5ifVRhCeh_NFjS9--ZwNCwk/edit?usp=sharing";


		[MenuItem("Zerosum/SDK Manager %#&z")]
		public static void OpenWindow()
		{
			SDKManagerWindow win = GetWindow<SDKManagerWindow>();
			win.titleContent = new GUIContent("Zerosum SDK Manager");
			win.position = new Rect(400, 300, 650, 700);
			win.minSize = new Vector2(300, 300);
		}


		public static SDKManagerWindow Instance;


		private Dictionary<PackageInfo, VisualElement> itemsByPackage;

		private Button btnRefresh;
		private Button btnInstall;

		private bool guiCreated = false;

		private Texture2D _iconDone;

		private Texture2D iconDone
		{
			get
			{
				if (_iconDone == null)
				{
					_iconDone = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Zerosum/SDKManager/Editor/done.png");
				}

				return _iconDone;
			}
		}

		private Texture2D _iconUpdate;

		private Texture2D iconUpdate
		{
			get
			{
				if (_iconUpdate == null)
				{
					_iconUpdate =
						AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Zerosum/SDKManager/Editor/update.png");
				}

				return _iconUpdate;
			}
		}

		private VisualTreeAsset _listItemVisualTree;

		private VisualTreeAsset listItemVisualTree
		{
			get
			{
				if (_listItemVisualTree == null)
				{
					_listItemVisualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
						"Assets/Zerosum/SDKManager/Editor/SDKManagerListItem.uxml");
				}

				return _listItemVisualTree;
			}
		}

		private List<PackageInfo> selectedPackages;

		private ProgressBar progressBar;
		private Label lblProgress;

		private void OnEnable()
		{
			Instance = this;

			SDKManager.onFetchStart += OnFetchStart;
			SDKManager.onFetchComplete += OnFetchComplete;
			SDKManager.onInstallStart += OnInstallStart;
			SDKManager.onInstallFail += OnInstallFail;
			SDKManager.onInstallComplete += OnInstallComplete;
			SDKManager.onInstallProgress += OnInstallProgress;
		}

		private void OnFocus()
		{
			SDKManager.StartFetch();
		}

		private void OnDisable()
		{
			SDKManager.onFetchStart -= OnFetchStart;
			SDKManager.onFetchComplete -= OnFetchComplete;
			SDKManager.onInstallStart -= OnInstallStart;
			SDKManager.onInstallFail -= OnInstallFail;
			SDKManager.onInstallComplete -= OnInstallComplete;
			SDKManager.onInstallProgress -= OnInstallProgress;

			Instance = null;
		}


		public void CreateGUI()
		{
			guiCreated = true;

			VisualElement root = rootVisualElement;

			var visualTree =
				AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
					"Assets/Zerosum/SDKManager/Editor/SDKManagerWindow.uxml");

			visualTree.CloneTree(root);

			var styleSheet =
				AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/Zerosum/SDKManager/Editor/SDKManagerWindow.uss");
			root.styleSheets.Add(styleSheet);

			SetupInternalContent(root);

			SDKManager.StartFetch();
		}

		private void SetupInternalContent(VisualElement root)
		{
			Button btnDevGuide = root.Q<Button>("btn-dev-guide");
			btnDevGuide.clicked += () => { Application.OpenURL(URL_DEV_GUIDE); };

			btnRefresh = root.Q<Button>("btn-refresh");
			btnRefresh.clicked += SDKManager.StartFetch;

			btnInstall = root.Q<Button>("btn-install-selected");
			btnInstall.clicked += OnClickInstall;

			var footer = root.Q<VisualElement>("footer");
			progressBar = new ProgressBar();
			progressBar.AddToClassList("progress-bar");
			progressBar.Children().First().Children().First().style.minHeight = 6f;
			lblProgress = new Label();

			footer.Add(progressBar);
			footer.Add(lblProgress);

			SetupListView();
		}

		private void SetupListView()
		{
			var listView = rootVisualElement.Q<ListView>("lst-view-packages");
			listView.showAlternatingRowBackgrounds = AlternatingRowBackground.None;
			listView.itemHeight = 30;
			listView.selectionType = SelectionType.Multiple;
			listView.showBorder = true;
			listView.makeItem = MakeListViewItem;
			listView.bindItem = BindListViewItem;
			listView.unbindItem = UnbindListViewItem;
			listView.onSelectionChange += OnListSelectionChanged;
		}


		private void RefreshViews()
		{
			var listView = rootVisualElement.Q<ListView>("lst-view-packages");

			itemsByPackage = new Dictionary<PackageInfo, VisualElement>();
			if (listView != null)
			{
				listView.itemsSource = SDKManager.packageInfoList ?? new List<PackageInfo>();
				//listView.Refresh();
			}

			btnRefresh?.SetEnabled(!SDKManager.isFetching && !SDKManager.isProcessingInstallQueue);

			RefreshInstallButtonState();

			if (progressBar != null)
				progressBar.visible = false;

			if (lblProgress != null)
				lblProgress.visible = false;
		}

		private void RefreshInstallButtonState()
		{
			if (btnInstall == null)
				return;

			if (SDKManager.isProcessingInstallQueue)
			{
				btnInstall.SetEnabled(false);
				return;
			}

			int countRequiresUpdate = selectedPackages?.Count(pkg => pkg.localState < PackageLocalState.UpToDate) ?? 0;

			btnInstall.text = countRequiresUpdate <= 1
				? "Install Selected"
				: $"Install Selected ({countRequiresUpdate})";

			btnInstall.SetEnabled(countRequiresUpdate > 0);
		}

		private VisualElement MakeListViewItem()
		{
			var row = listItemVisualTree.Instantiate();

			// var lblName = row.Q<Label>("lbl-name");
			// var lblVer = row.Q<Label>("lbl-version");
			// var img = row.Q<Image>("img-state");

			return row;
		}

		private void BindListViewItem(VisualElement el, int i)
		{
			var inf = SDKManager.packageInfoList[i];
			var lblName = el.Q<Label>("lbl-name");
			var lblVersion = el.Q<Label>("lbl-version");
			var img = el.Q<Image>();

			el.userData = inf;

			el.RemoveFromClassList("list-item-active");

			itemsByPackage[inf] = el;

			var deps = new List<PackageInfo>();
			SDKManager.ResolveDependencies(deps, inf, false);
			deps.Reverse();
			if (inf.dependencies.Count > 0)
			{
				el.tooltip = "Depends on:\n\t" + deps.Select(dep => dep.name)
				                                     .Aggregate((s1, s2) => s1 + "\n\t" + s2);
			}

			lblName.text = inf.name;


			switch (inf.localState)
			{
				case PackageLocalState.NotInstalled:
					img.image = null;
					lblVersion.text = inf.version;
					break;
				case PackageLocalState.RequiresUpdate:
					img.image = iconUpdate;
					lblVersion.text = $"{inf.localVersion}   \u2192   {inf.version}";
					break;
				case PackageLocalState.UpToDate:
					img.image = iconDone;
					lblVersion.text = inf.version;
					break;
			}
		}

		private void UnbindListViewItem(VisualElement el, int i)
		{
			var lblName = el.Q<Label>("lbl-name");
			var lblVersion = el.Q<Label>("lbl-ver");
		}

		// private void OnListViewItemClicked(Button btn)
		// {
		// 	var inf = (PackageInfo) btn.userData;
		//
		// 	if (inf.localState < PackageLocalState.UpToDate)
		// 	{
		// 		// Install or update
		// 		var yes = EditorUtility.DisplayDialog("Installation",
		// 		                                      $"This will install {inf.name} package along with all its dependencies. Are you sure you want to continue?",
		// 		                                      "Yes", "No");
		// 		if (yes)
		// 		{
		// 			SDKManager.TryInstallOrUpdatePackage(inf);
		// 		}
		// 	}
		// }

		public void SetActionButtonsEnabled(bool e)
		{
			var fetchButtons = rootVisualElement.Query<Button>(null, new[] {"fetch-element"}).ToList();

			foreach (var btn in fetchButtons)
			{
				btn.SetEnabled(e);
			}
		}

		private void OnListSelectionChanged(IEnumerable<object> objects)
		{
			selectedPackages = objects.Select(obj => (PackageInfo) obj).ToList();

			RefreshInstallButtonState();
		}

		private void OnClickInstall()
		{
			var requiredPackages = selectedPackages.Where(pkg => pkg.localState < PackageLocalState.UpToDate).ToList();

			string message = null;
			if (requiredPackages.Count > 1)
			{
				message =
					$"This will install {requiredPackages.Count} packages along with all their dependencies. Are you sure you want to continue?";
			}
			else
			{
				var inf = requiredPackages.First();
				message =
					$"This will install {inf.name} package along with all its dependencies. Are you sure you want to continue?";
			}

			var yes = EditorUtility.DisplayDialog("Installation",
			                                      message,
			                                      "Yes", "No");
			if (yes)
			{
				SDKManager.TryInstallOrUpdatePackages(requiredPackages);
			}
		}

		private void OnFetchStart()
		{
			RefreshViews();
		}

		private void OnFetchComplete()
		{
			itemsByPackage = new Dictionary<PackageInfo, VisualElement>();
			selectedPackages = new List<PackageInfo>();
			RefreshViews();
		}

		private void OnInstallStart(PackageInfo obj)
		{
			RefreshViews();
			SetProgress(obj, 0f);
		}

		private void OnInstallFail(PackageInfo obj)
		{
			RefreshViews();
		}

		private void OnInstallComplete(PackageInfo obj)
		{
			RefreshViews();
		}


		private void OnInstallProgress(PackageInfo inf, float progress)
		{
			SetProgress(inf, progress);
		}

		private void SetProgress(PackageInfo inf, float progress)
		{
			if (itemsByPackage == null || !itemsByPackage.TryGetValue(inf, out var el))
				return;

			if (!el.ClassListContains("list-item-active"))
			{
				el.AddToClassList("list-item-active");
				lblProgress.text = $"Installing {inf.name}: ";
			}

			progressBar.value = progress * 100.01f;

			lblProgress.visible = true;
			progressBar.visible = true;

			//
			// btnInstall.text = $"{inf.name} | {(int) (progress * 100 + 0.01f)}%";
		}
	}
}
