using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Xsolla.Core.Editor
{
	public class AndroidScriptsPreprocessor : IPreprocessBuildWithReport
	{
		public int callbackOrder
		{
			get { return 3000; }
		}

		public void OnPreprocessBuild(BuildReport report)
		{
#if UNITY_ANDROID
			Debug.Log("Xsolla SDK is now preprocessing native Android scripts.");
			SetupWechatActivity();
			SetupPaymentsProxyActivity();
#endif
		}

		void SetupWechatActivity()
		{
			var wechatActivityScriptPath = Path.Combine(FindAndroidScripts(Application.dataPath).Replace("\\", "/"), "WXEntryActivity.java");

			if (!File.Exists(wechatActivityScriptPath))
			{
				Debug.LogError("WeChat Android activity script is missing.");
				return;
			}

			var wechatActivityAssetPath = "Assets" + wechatActivityScriptPath.Substring(Application.dataPath.Length);
			var wechatActivityAsset = AssetImporter.GetAtPath(wechatActivityAssetPath) as PluginImporter;
			if (wechatActivityAsset != null)
			{
				wechatActivityAsset.SetCompatibleWithPlatform(BuildTarget.Android, !string.IsNullOrEmpty(XsollaSettings.WeChatAppId));
				wechatActivityAsset.SaveAndReimport();
			}

			var scriptContent = File.ReadAllText(wechatActivityScriptPath);

			var androidPackageName = Application.identifier;
			var editedScriptContent = Regex.Replace(scriptContent, "package.+;", string.Format("package {0}.wxapi;", androidPackageName));

			File.WriteAllText(wechatActivityScriptPath, editedScriptContent);
		}
		
		private void SetupPaymentsProxyActivity()
		{
			var activityScriptPath = Path.Combine(FindAndroidScripts(Application.dataPath).Replace("\\", "/"), "AndroidPaymentsProxy.java");
			if (!File.Exists(activityScriptPath))
			{
				Debug.LogError("Android Payments Proxy activity script is missing.");
				return;
			}

			var assetPath = "Assets" + activityScriptPath.Substring(Application.dataPath.Length);
			var activityAsset = AssetImporter.GetAtPath(assetPath) as PluginImporter;
			if (activityAsset != null)
			{
				activityAsset.SetCompatibleWithPlatform(BuildTarget.Android, true);
				activityAsset.SaveAndReimport();
			}

			var scriptContent = File.ReadAllText(activityScriptPath);
			var androidPackageName = Application.identifier;
			var editedScriptContent = Regex.Replace(scriptContent, "package.+;", $"package {androidPackageName}.androidProxies;");

			File.WriteAllText(activityScriptPath, editedScriptContent);
		}

		static string FindAndroidScripts(string path)
		{
			foreach (var dir in Directory.GetDirectories(path))
			{
				if (dir.Contains("AndroidNativeScripts"))
				{
					return dir;
				}

				var rec = FindAndroidScripts(dir);
				if (rec != null)
				{
					return rec;
				}
			}

			return null;
		}
	}
}
