using System;
using System.Collections.Generic;

namespace Zerosum.SDKManager
{
	[System.Serializable]
	public class PackageInfo
	{
		public string name;
		public string id;
		public string version;
		public List<string> dependencies;

		[NonSerialized]
		public PackageLocalState localState;

		[NonSerialized]
		public string localVersion;

		[NonSerialized]
		public List<PackageInfo> aliveDependencies;

		public override bool Equals(object obj)
		{
			if (obj is PackageInfo other)
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

	[System.Serializable]
	public class PackageInfoCollection
	{
		public List<PackageInfo> packageInfos;
	}

	public enum PackageLocalState
	{
		None = 0,
		NotInstalled = 1,
		RequiresUpdate = 2,
		UpToDate = 3
	}
}
