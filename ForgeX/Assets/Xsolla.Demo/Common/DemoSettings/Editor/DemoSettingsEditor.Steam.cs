﻿using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Xsolla.Demo
{
	public partial class DemoSettingsEditor
	{
		private const string STEAM_AUTH_LABEL = "Use Steam Authorization";
		private const string STEAM_AUTH_TOOLTIP = "If enabled, Login try find Steam client and get `session_ticket`." +
		                                          "Then this ticket will be changed to JWT.";

		private const string STEAM_APP_ID_LABEL = "Steam App ID";
		private const string STEAM_APP_ID_TOOLTIP = "You will need to restart Unity Editor after changing Steam App ID";

		private const string PAYMENTS_FLOW_LABEL = "Payments Flow";
		private const string PAYMENTS_FLOW_TOOLTIP = "You need to sign an additional partner agreement to use Steam as a payment system. "
		                                             + "Contact the Integration or Account Manager";

		private void SteamSettings()
		{
			EditorGUILayout.Space();
			EditorGUILayout.BeginVertical(GroupAreaStyle);
			EditorGUILayout.LabelField("Steam", GroupHeaderStyle);

			var useSteam = EditorGUILayout.Toggle(new GUIContent(STEAM_AUTH_LABEL, STEAM_AUTH_TOOLTIP), DemoSettings.UseSteamAuth);
			if (useSteam != DemoSettings.UseSteamAuth)
			{
				DemoSettings.UseSteamAuth = useSteam;

				if (useSteam)
				{
					DemoSettings.Platform = PlatformType.PC_Other;
					DemoSettings.UseConsoleAuth = false;
				}
				else
				{
					if (DemoSettings.Platform == PlatformType.PC_Other)
						DemoSettings.Platform = PlatformType.Xsolla;
				}
			}

			if (!DemoSettings.UseSteamAuth)
			{
				EditorGUILayout.EndVertical();
				return;
			}

			var appId = EditorGUILayout.TextField(new GUIContent(STEAM_APP_ID_LABEL, STEAM_APP_ID_TOOLTIP), DemoSettings.SteamAppId);
			if (appId != DemoSettings.SteamAppId)
			{
				DemoSettings.SteamAppId = appId;

				var steamAppIdFiles = new[]{
					Application.dataPath.Replace("Assets", "steam_appid.txt"),
					Path.Combine(Application.dataPath, "Plugins", "Steamworks.NET", "redist", "steam_appid.txt")
				};

				try
				{
					foreach (var filePath in steamAppIdFiles)
					{
						File.WriteAllText(filePath, appId);
					}

					Debug.Log("[Steamworks.NET] Files \"steam_appid.txt\" successfully updated. Please relaunch Unity");
				}
				catch (Exception)
				{
					var filePathsArray = string.Join("\n", steamAppIdFiles);
					Debug.LogWarning($"[Steamworks.NET] Could not write SteamAppId in the files \"steam_appid.txt\". Please edit them manually and relaunch Unity. Files:\n{filePathsArray}\n");
				}
			}

			var paymentsFlow = (PaymentsFlow) EditorGUILayout.EnumPopup(new GUIContent(PAYMENTS_FLOW_LABEL, PAYMENTS_FLOW_TOOLTIP), DemoSettings.PaymentsFlow);
			if (paymentsFlow != DemoSettings.PaymentsFlow)
			{
				DemoSettings.PaymentsFlow = paymentsFlow;
			}

			EditorGUILayout.EndVertical();
		}
	}
}