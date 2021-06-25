using System;

namespace UnityCommon.Utilities
{

	public static class TimeUtility
	{
		public static long UnixTimestamp => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

	}

}
