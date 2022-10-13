using System;
using UnityEngine;
using Xsolla.Core;
using Xsolla.Core.Popup;

namespace Xsolla.Demo
{
	public partial class AccountLinkingManagerManual : MonoBehaviour
	{
		[SerializeField] private SimpleButton accountLinkingButton = default;
		[SerializeField] private SimpleButton getAccountLinkButton = default;

		partial void InitInternal()
		{
			if (Token.Instance.IsMasterAccount())
				GetAccountLinkButtonEnable();
			else
				LinkingButtonEnable();
		}

		private void GetAccountLinkButtonEnable()
		{
			getAccountLinkButton.onClick += () =>
			{
				SdkUserAccountLogic.Instance.RequestLinkingCode(
					code => StoreDemoPopup.ShowSuccess($"YOUR CODE: {code.code}"),
					StoreDemoPopup.ShowError);
			};

			accountLinkingButton.transform.parent.gameObject.SetActive(false);
			getAccountLinkButton.transform.parent.gameObject.SetActive(true);
		}

		private void LinkingButtonEnable()
		{
			accountLinkingButton.onClick += () =>
			{
				ShowCodeConfirmation(code =>
				{
					var linkingResult = new LinkingResultContainer();
					PopupFactory.Instance.CreateWaiting().SetCloseCondition(() => linkingResult.IsLinked != null);

					SdkUserAccountLogic.Instance.LinkConsoleAccount(
						userId: DemoSettings.UsernameFromConsolePlatform,
						platform: DemoSettings.Platform.GetString(),
						confirmationCode: code,
						onSuccess: () => LinkingAccountHandler(linkingResult),
						onError: error => { linkingResult.IsLinked = false; StoreDemoPopup.ShowError(error); });
				});
			};

			getAccountLinkButton.transform.parent.gameObject.SetActive(false);
			accountLinkingButton.transform.parent.gameObject.SetActive(true);
		}

		private void LinkingAccountHandler(LinkingResultContainer linkingResult)
		{
			SdkAuthLogic.Instance.SignInConsoleAccount(
				userId: DemoSettings.UsernameFromConsolePlatform,
				platform: DemoSettings.Platform.GetString(),
				onSuccess: newToken => OnSuccessConsoleLogin(newToken, linkingResult),
				onError: error => { linkingResult.IsLinked = false; StoreDemoPopup.ShowError(error); }
			);
		}

		private void OnSuccessConsoleLogin(string newToken, LinkingResultContainer linkingResult)
		{
			SdkAuthLogic.Instance.ValidateToken(
				token: newToken,
				onSuccess: validToken => ApplyNewToken(validToken, linkingResult),
				onError: error => { linkingResult.IsLinked = false; StoreDemoPopup.ShowError(error); });
		}

		private void ApplyNewToken(string newToken, LinkingResultContainer linkingResult)
		{
			Token.Instance = Token.Create(newToken);

			if (!DemoMarker.IsInventoryPartAvailable)
				FindObjectOfType<UserInfoDrawer>()?.Refresh();
			else
				UserInventory.Instance.Refresh(onSuccess: () => GoToInventory(linkingResult), onError: error => { linkingResult.IsLinked = false; StoreDemoPopup.ShowError(error); });
		}

		private void GoToInventory(LinkingResultContainer linkingResult)
		{
			linkingResult.IsLinked = true;
			DemoController.Instance.SetState(MenuState.Inventory);
			PopupFactory.Instance.CreateSuccess();
		}

		private void ShowCodeConfirmation(Action<string> callback)
		{
			PopupFactory.Instance.CreateCodeConfirmation().SetConfirmCallback(callback);
		}

		private class LinkingResultContainer
		{
			public bool? IsLinked;
		}
	}
}
