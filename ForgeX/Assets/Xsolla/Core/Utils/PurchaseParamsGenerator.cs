using System.Collections.Generic;

namespace Xsolla.Core
{
    public static class PurchaseParamsGenerator
    {
		public static TempPurchaseParams GenerateTempPurchaseParams(PurchaseParams purchaseParams)
		{
			var settings = new TempPurchaseParams.Settings();

			settings.ui = PayStationUISettings.GenerateSettings();
			settings.redirect_policy = RedirectPolicySettings.GeneratePolicy();
			if (settings.redirect_policy != null)
			{
				settings.return_url = settings.redirect_policy.return_url;
			}

			//Fix 'The array value is found, but an object is required' in case of empty values.
			if (settings.ui == null && settings.redirect_policy == null && settings.return_url == null)
				settings = null;

			var tempPurchaseParams = new TempPurchaseParams()
			{
				sandbox = XsollaSettings.IsSandbox,
				settings = settings,
				custom_parameters = purchaseParams?.custom_parameters,
				currency = purchaseParams?.currency,
				locale = purchaseParams?.locale,
				quantity = purchaseParams?.quantity,
				shipping_data = purchaseParams?.shipping_data,
				shipping_method = purchaseParams?.shipping_method,
			};

			return tempPurchaseParams;
		}

		/// <summary>
		/// Returns headers list such as <c>AuthHeader</c> and <c>SteamPaymentHeader</c>.
		/// </summary>
		/// <param name="token">Auth token taken from Xsolla Login.</param>
		/// <param name="customHeaders">Custom headers for web request.</param>
		/// <returns></returns>
		public static List<WebRequestHeader> GetPaymentHeaders(Token token, Dictionary<string, string> customHeaders = null)
		{
			var headers = new List<WebRequestHeader>
			{
				WebRequestHeader.AuthHeader(token)
			};

			if (customHeaders != null)
			{
				foreach (var kvp in customHeaders)
				{
					if (!string.IsNullOrEmpty(kvp.Key) && !string.IsNullOrEmpty(kvp.Value))
						headers.Add(new WebRequestHeader(kvp.Key, kvp.Value));
				}
			}

			return headers;
		}
    }
}
