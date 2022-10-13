using System;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Xsolla.Core.Editor
{
	public class AndroidManifestPreprocessor : IPreprocessBuildWithReport
	{
		const string MainManifestPath = "Plugins/Android/AndroidManifest.xml";
		const string XsollaManifestLabel = "xsolla";

		public int callbackOrder
		{
			get { return 2000; }
		}

		public void OnPreprocessBuild(BuildReport report)
		{
#if UNITY_ANDROID
			Debug.Log("Xsolla SDK is now preprocessing your AndroidManifest.xml");
			SetupWeChat();
			SetupPaymentsProxyActivity();
#endif
		}

		void SetupWeChat()
		{
			var manifestPath = Path.Combine(Application.dataPath, MainManifestPath);
			var manifestExists = File.Exists(manifestPath);

			if (!manifestExists)
			{
				if (string.IsNullOrEmpty(XsollaSettings.WeChatAppId))
				{
					return;
				}

				RestoreAndroidManifest(manifestPath);
			}

			var manifest = new AndroidManifestWrapper(manifestPath);

			var androidPackageName = Application.identifier;
			var wechatActivityName = string.Format("{0}.wxapi.WXEntryActivity", androidPackageName);

			var wechatActivity = new ActivityNode(wechatActivityName);
			wechatActivity.AddAttribute(AndroidManifestConstants.ExportedAttribute, "true");

			var manifestChanged = false;

			// cleanup manifest in case WeChat activity was added previously
			if (manifest.ContainsNode(new FindByTag(AndroidManifestConstants.ApplicationTag), new FindByName(wechatActivity)))
			{
				manifest.RemoveNode(new FindByTag(AndroidManifestConstants.ApplicationTag), new FindByName(wechatActivity));
				manifestChanged = true;
			}

			if (!string.IsNullOrEmpty(XsollaSettings.WeChatAppId))
			{
				manifest.AddNode(wechatActivity, new FindByTag(AndroidManifestConstants.ApplicationTag));
				manifestChanged = true;
			}

			if (manifestChanged)
			{
				manifest.SaveManifest();
			}
		}

		private void SetupPaymentsProxyActivity()
		{
			var manifestPath = Path.Combine(Application.dataPath, MainManifestPath);
			var manifestExists = File.Exists(manifestPath);

			if (!manifestExists)
			{
				RestoreAndroidManifest(manifestPath);
			}

			var manifestWrapper = new AndroidManifestWrapper(manifestPath);
			var activityName = $"{Application.identifier}.androidProxies.AndroidPaymentsProxy";

			var activityNode = new ActivityNode(activityName);
			activityNode.AddAttribute(AndroidManifestConstants.ExportedAttribute, "true");

			// cleanup manifest in case activity node was added previously
			if (manifestWrapper.ContainsNode(new FindByTag(AndroidManifestConstants.ApplicationTag), new FindByName(activityNode)))
				manifestWrapper.RemoveNode(new FindByTag(AndroidManifestConstants.ApplicationTag), new FindByName(activityNode));

			manifestWrapper.AddNode(activityNode, new FindByTag(AndroidManifestConstants.ApplicationTag));
			manifestWrapper.SaveManifest();
		}

		static void RestoreAndroidManifest(string manifestPath)
		{
			var backupManifestPath = Path.Combine(FindAndroidManifestBackup(Application.dataPath).Replace("\\", "/"), "AndroidManifest.xml");

			var manifestDirectoryPath = Path.GetDirectoryName(manifestPath);
			if (!Directory.Exists(manifestDirectoryPath))
			{
				Directory.CreateDirectory(manifestDirectoryPath);
			}

			File.Copy(backupManifestPath, manifestPath);
		}

		static string FindAndroidManifestBackup(string path)
		{
			foreach (var dir in Directory.GetDirectories(path))
			{
				if (dir.Contains("AndroidManifestBackup"))
				{
					return dir;
				}

				var rec = FindAndroidManifestBackup(dir);
				if (rec != null)
				{
					return rec;
				}
			}

			return null;
		}
	}
}
