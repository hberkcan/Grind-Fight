using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine;

namespace Zerosum
{
	public static class RemoteBuildHelpers
	{
		public static string EncodeParams(this string url, params (string, string)[] kvs)
		{
			StringBuilder sb = new StringBuilder();
			sb.Append(url).Append('?');
			for (var i = 0; i < kvs.Length; i++)
			{
				var kv = kvs[i];
				if (i > 0)
					sb.Append('&');
				sb.Append(Uri.EscapeDataString(kv.Item1)).Append("=").Append(Uri.EscapeDataString(kv.Item2));
			}

			return sb.ToString();
		}

		public static (string url, string branch, bool hasUncommitted) GetRepoInfo()
		{
			var repoPath = Application.dataPath.Replace("/Assets", "");
			string remoteUrl = null;
			string branch = null;
			bool hasUncommitted = false;
			ExecuteProcess(repoPath, "git", "config --get remote.origin.url", output => { remoteUrl = output; });
			ExecuteProcess(repoPath, "git", "branch --show-current", output => { branch = output; });
			ExecuteProcess(repoPath, "git", "log origin/master..HEAD", output => hasUncommitted = true);
			ExecuteProcess(repoPath, "git", "status --porcelain", output => hasUncommitted = true);
			
			return (remoteUrl, branch, hasUncommitted);
		}

		public static int ExecuteProcess(string workingDir, string fileName, string arguments,
		                                 Action<string> onOutput = null)
		{
			try
			{
				var startInfo = new ProcessStartInfo(fileName, arguments);
				startInfo.WorkingDirectory = workingDir;
				startInfo.RedirectStandardOutput = true;
				startInfo.RedirectStandardError = true;
				startInfo.UseShellExecute = false;

				var process = Process.Start(startInfo);

				ReadStreamLines(process.StandardOutput, onOutput);

				ReadStreamLines(process.StandardError, onOutput);

				process.WaitForExit();

				return process.ExitCode;
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
				return -9;
			}
		}

		public static void ReadStreamLines(StreamReader reader, Action<string> onOutput = null)
		{
			string line = null;
			while ((line = reader.ReadLine()) != null)
			{
				// Console.WriteLine(line);
				onOutput?.Invoke(line);
			}
		}


		public static string GetProjectName(string gitUrl)
		{
			return Path.GetFileNameWithoutExtension(gitUrl);
		}
	}
}
