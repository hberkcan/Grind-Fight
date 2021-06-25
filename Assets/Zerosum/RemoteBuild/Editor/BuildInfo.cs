using System;
using System.Collections.Generic;
using System.Globalization;

namespace Zerosum
{
	public enum BuildState
	{
		None = 0,
		Working = 1,
		Success = 2,
		Failure = 3,
	}

	public enum BuildMethod
	{
		Diawi = 1,
		AppStore = 2
	}

	[System.Serializable]
	public class BuildInfo
	{
		public string branch;
		public string id;
		public string start;
		public string end;
		public string currentOperation;
		public string diawiLink;
		public BuildMethod method;
		public BuildState state;

		public string GetDurationFormatted()
		{
			TimeSpan span = DateTime.ParseExact(end, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture) -
			                DateTime.ParseExact(start, "yyyy/MM/dd HH:mm:ss", CultureInfo.InvariantCulture);
			return $"{(span.Hours > 0 ? $"{span.Hours}h " : "")}{span.Minutes}m {span.Seconds}s";
		}
	}

	[System.Serializable]
	public class BuildInfoCollection
	{
		public string projectName;
		public string url;

		public List<BuildInfo> builds = new List<BuildInfo>();
	}
}
