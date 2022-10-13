using UnityEngine;
using Xsolla.Core;
using JetBrains.Annotations;

namespace Xsolla.Auth
{
	[PublicAPI]
	public partial class XsollaAuth : MonoSingleton<XsollaAuth>
	{
		private const string DEFAULT_OAUTH_STATE = "xsollatest";
		

		public override void Init()
		{
			base.Init();
			SetupOAuthRefresh();
		}

		protected override void OnDestroy()
		{
			TeardownOAuthRefresh();
			base.OnDestroy();
		}

		public void DeleteToken(string key)
		{
			if (!string.IsNullOrEmpty(key) && PlayerPrefs.HasKey(key))
			{
				PlayerPrefs.DeleteKey(key);
			}
		}

		public void SaveToken(string key, string token)
		{
			if (!string.IsNullOrEmpty(token))
			{
				PlayerPrefs.SetString(key, token);
			}
		}

		public string LoadToken(string key)
		{
			return PlayerPrefs.GetString(key, string.Empty);
		}

		string GetLocaleUrlParam(string locale)
		{
			if (string.IsNullOrEmpty(locale))
			{
				return string.Empty;
			}
			return string.Format("&locale={0}", locale);
		}
	}
}
