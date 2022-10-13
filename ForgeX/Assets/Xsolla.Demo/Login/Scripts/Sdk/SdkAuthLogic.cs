using System;
using System.Collections.Generic;
using Xsolla.Auth;
using Xsolla.Core;
using Xsolla.UserAccount;

namespace Xsolla.Demo
{
	public class SdkAuthLogic : MonoSingleton<SdkAuthLogic>
	{
		public event Action RegistrationEvent;
		public event Action LoginEvent;
		public event Action UpdateUserInfoEvent;

		#region Token
		public void ValidateToken(string token, Action<string> onSuccess = null, Action<Error> onError = null)
		{
			GetUserInfo(token, useCache: false, onSuccess: _ => onSuccess?.Invoke(token), onError: onError);
		}
		#endregion

		#region User
		public void GetUserInfo(string token, Action<UserInfo> onSuccess = null, Action<Error> onError = null)
		{
			GetUserInfo(token, useCache: true, onSuccess, onError);
		}

		private readonly Dictionary<string, UserInfo> _userCache = new Dictionary<string, UserInfo>();
		public void GetUserInfo(string token, bool useCache, Action<UserInfo> onSuccess, Action<Error> onError = null)
		{
			if (useCache && _userCache.ContainsKey(token))
				onSuccess?.Invoke(_userCache[token]);
			else
				XsollaAuth.Instance.GetUserInfo(token, info =>
				{
					_userCache[token] = info;
					onSuccess?.Invoke(info);
				}, onError);
		}

		public void UpdateUserInfo(string token, UserInfoUpdate info, Action<UserInfo> onSuccess, Action<Error> onError = null)
		{
			Action<UserInfo> successCallback = userInfo =>
			{
				_userCache[token] = userInfo;
				onSuccess?.Invoke(userInfo);
				UpdateUserInfoEvent?.Invoke();
			};

			XsollaUserAccount.Instance.UpdateUserInfo(token, info, successCallback, onError);
		}

		public void Register(string username, string password, string email, string state = null, Action onSuccess = null, Action<Error> onError = null)
		{
			Action successCallback = () =>
			{
				onSuccess?.Invoke();
				RegistrationEvent?.Invoke();
			};

			XsollaAuth.Instance.Register(username, password, email,
			oauthState:state, acceptConsent:true, promoEmailAgreement:true, onSuccess:successCallback, onError:onError);
		}

		public void Register(string username, string password, string email, string state = null, Action<int> onSuccess = null, Action<Error> onError = null)
		{
			Action<int> successCallback = responseCode =>
			{
				onSuccess?.Invoke(responseCode);
				RegistrationEvent?.Invoke();
			};

			XsollaAuth.Instance.Register(username, password, email,
			oauthState:state, acceptConsent:true, promoEmailAgreement:true, onSuccess:successCallback, onError:onError);
		}

		public void Register(string username, string password, string email, string state = null, Action<LoginUrlResponse> onSuccess = null, Action<Error> onError = null)
		{
			Action<LoginUrlResponse> successCallback = response =>
			{
				onSuccess?.Invoke(response);
				RegistrationEvent?.Invoke();
			};

			XsollaAuth.Instance.Register(username, password, email,
			oauthState:state, acceptConsent:true, promoEmailAgreement:true, onSuccess:successCallback, onError:onError);
		}

		public void SignIn(string username, string password, bool rememberUser, Action<string> onSuccess, Action<Error> onError = null)
		{
			Action<string> successCallback = token =>
			{
				onSuccess?.Invoke(token);
				LoginEvent?.Invoke();
			};

			XsollaAuth.Instance.SignIn(username, password, rememberUser, onSuccess:successCallback, onError:onError);
		}
		
		public void ResetPassword(string username, Action onSuccess = null, Action<Error> onError = null)
		{
			XsollaAuth.Instance.ResetPassword(username, onSuccess:onSuccess, onError:onError);
		}

		public void ResendConfirmationLink(string username, Action onSuccess = null, Action<Error> onError = null)
		{
			XsollaAuth.Instance.ResendConfirmationLink(username, onSuccess:onSuccess, onError:onError);
		}
		#endregion

		#region Social
		public void SilentAuth(string providerName, string appId, string sessionTicket, string state = null, Action<string> onSuccess = null, Action<Error> onError = null)
		{
			Action<string> successCallback = token =>
			{
				onSuccess?.Invoke(token);
				LoginEvent?.Invoke();
			};

			XsollaAuth.Instance.SilentAuth(providerName: providerName, appId: appId, sessionTicket: sessionTicket, oauthState: state, onSuccess: successCallback, onError: onError);
		}

		public void AuthViaDeviceID(Core.DeviceType deviceType, string deviceName, string deviceId, string payload = null, string state = null, Action<string> onSuccess = null, Action<Error> onError = null)
		{
			Action<string> successCallback = token =>
			{
				onSuccess?.Invoke(token);
				LoginEvent?.Invoke();
			};

			XsollaAuth.Instance.AuthViaDeviceID(deviceType, deviceName, deviceId, payload, state, successCallback, onError);
		}

		public string GetSocialNetworkAuthUrl(SocialProvider socialProvider)
		{
			return XsollaAuth.Instance.GetSocialNetworkAuthUrl(socialProvider);
		}
		#endregion

		#region AccountLinking
		public void SignInConsoleAccount(string userId, string platform, Action<string> onSuccess, Action<Error> onError)
		{
			Action<string> successCallback = token =>
			{
				onSuccess?.Invoke(token);
				LoginEvent?.Invoke();
			};

			XsollaAuth.Instance.SignInConsoleAccount(userId, platform, successCallback, onError);
		}
		#endregion

		#region OAuth2.0
		public void RefreshOAuthToken(Action<string> onSuccess, Action<Error> onError)
		{
			Action<string> successCallback = token =>
			{
				onSuccess?.Invoke(token);
				LoginEvent?.Invoke();
			};

			XsollaAuth.Instance.RefreshOAuthToken(successCallback, onError);
		}

		public void ExchangeCodeToToken(string code, Action<string> onSuccessExchange = null, Action<Error> onError = null)
		{
			XsollaAuth.Instance.ExchangeCodeToToken(code, onSuccessExchange, onError);
		}
		#endregion

		#region Passwordless
		public void StartAuthByPhoneNumber(string phoneNumber, string linkUrl, bool sendLink, Action<string> onSuccess, Action<Error> onError = null)
		{
			XsollaAuth.Instance.StartAuthByPhoneNumber(phoneNumber, linkUrl, sendLink, onSuccess, onError);
		}

		public void CompleteAuthByPhoneNumber(string phoneNumber, string confirmationCode, string operationId, Action<string> onSuccess, Action<Error> onError = null)
		{
			Action<string> successCallback = token =>
			{
				onSuccess?.Invoke(token);
				LoginEvent?.Invoke();
			};

			XsollaAuth.Instance.CompleteAuthByPhoneNumber(phoneNumber, confirmationCode, operationId, successCallback, onError);
		}

		public void StartAuthByEmail(string email, string linkUrl, bool sendLink, Action<string> onSuccess, Action<Error> onError = null)
		{
			XsollaAuth.Instance.StartAuthByEmail(email, linkUrl, sendLink, onSuccess, onError);
		}

		public void CompleteAuthByEmail(string email, string confirmationCode, string operationId, Action<string> onSuccess, Action<Error> onError = null)
		{
			Action<string> successCallback = token =>
			{
				onSuccess?.Invoke(token);
				LoginEvent?.Invoke();
			};

			XsollaAuth.Instance.CompleteAuthByEmail(email, confirmationCode, operationId, successCallback, onError);
		}
		#endregion
	}
}
