using System;
using Xsolla.Core;

namespace Xsolla.UserAccount
{
	public partial class XsollaUserAccount : MonoSingleton<XsollaUserAccount>
	{
		private const string URL_LINKING_CODE_REQUEST = "https://login.xsolla.com/api/users/account/code";
		private const string URL_LINK_ACCOUNT = "https://livedemo.xsolla.com/sdk/sdk-shadow-account/link";

		#region Comment
		/// <summary>
		/// Creates the code for linking the platform account to the existing main account when the user logs in to the game via a gaming console.
		/// The call is used with Link accounts by code request.
		/// </summary>
		/// <remarks> Swagger method name:<c>Create Code for Linking Accounts</c>.</remarks>
		/// <see cref="https://developers.xsolla.com/login-api/users/create-code-for-linking-accounts"/>
		/// <param name="onSuccess">Success operation callback.</param>
		/// <param name="onError">Failed operation callback.</param>
		/// <seealso cref="SignInConsoleAccount"/>
		/// <seealso cref="LinkConsoleAccount"/>
		#endregion
		public void RequestLinkingCode(Action<LinkingCode> onSuccess, Action<Error> onError)
		{
			WebRequestHelper.Instance.PostRequest<LinkingCode>(SdkType.Login, URL_LINKING_CODE_REQUEST, WebRequestHeader.AuthHeader(Token.Instance), onSuccess,
				onError: error => TokenRefresh.Instance.CheckInvalidToken(error, onError, () => RequestLinkingCode(onSuccess, onError)));
		}

		#region Comment
		/// <summary>
		/// This method is used for authenticating users in Xsolla Login,
		/// who play on the consoles and other platforms
		/// where Xsolla Login isn't used. You must implement it
		/// on the your server side.
		/// Integration flow on the server side:
		/// <list type="number">
		///		<item>
		///			<term>Generate server JWT</term>
		///			<description>
		///				<list type="bullet">
		///					<item>
		///						<term>Request credentionals</term>
		///						<description>before write any code, contact with support by email:<see cref="support@xsolla.com"/>
		///						and request <c>ClientID</c> + <c>ClientSecret</c>.
		///						</description>
		///					</item>
		///					<item>
		///						<term>Implement method: </term>
		///						<description>
		///							<see cref="https://developers.xsolla.com/login-api/oauth-20/generate-user-jwt"/>
		///							with application/x-www-form-urlencoded payload parameters:
		///							<list type="bullet">
		///								<item>
		///									<description>client_id=YOUR_CLIENT_ID</description>
		///								</item>
		///								<item>
		///									<description>client_secret=YOUR_CLIENT_SECRET</description>
		///								</item>
		///								<item>
		///									<description>grant_type=client_credentials</description>
		///								</item>
		///							</list>
		///						</description>
		///					</item>
		///				</list>
		///			</description>
		///		</item>
		///		<item>
		///			<term>Implement APIs for account linking</term>
		///			<description>
		///				<see cref="https://developers.xsolla.com/login-api/users/link-accounts-by-code"/>
		///				with:
		///				<list type="bullet">
		///					<item>
		///						<term>Headers</term>
		///						<description>
		///						`Content-Type: application/json` and `X-SERVER-AUTHORIZATION: YourGeneratedJwt`
		///						</description>
		///					</item>
		///					<item>
		///					<term>Body</term>
		///					<description>see documentation.</description>
		///					</item>
		///				</list>
		///			</description>
		///		</item>
		/// </list>
		/// </summary>
		/// <param name="userId">Social platform (XBox, PS4, etc) user unique identifier.</param>
		/// <param name="platform">Platform name (XBox, PS4, etc).</param>
		/// <param name="confirmationCode">Code, taken from unified account.</param>
		/// <param name="onSuccess">Success operation callback.</param>
		/// <param name="onError">Failed operation callback.</param>
		/// <seealso cref="SignInConsoleAccount"/>
		/// <seealso cref="RequestLinkingCode"/>
		#endregion
		public void LinkConsoleAccount(string userId, string platform, string confirmationCode, Action onSuccess, Action<Error> onError)
		{
			var url = $"{URL_LINK_ACCOUNT}?user_id={userId}&platform={platform}&code={confirmationCode}";
			WebRequestHelper.Instance.PostRequest(sdkType: SdkType.Login, url: url, onComplete: onSuccess, onError: onError);
		}
	}
}
