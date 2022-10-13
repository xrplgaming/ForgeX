using System;
using System.Collections.Generic;
using Xsolla.Core;
using Xsolla.Core.Popup;

namespace Xsolla.Demo
{
	public class DemoShop : MonoSingleton<DemoShop>
	{
		public void PurchaseForRealMoney(CatalogItemModel item, Action<CatalogItemModel> onSuccess = null, Action<Error> onError = null)
		{
			var restrictedPaymentAllower = GenerateAllower();
			SdkPurchaseLogic.Instance.PurchaseForRealMoney(item,
				restrictedPaymentAllower,
				OnSuccessPurchase(onSuccess),
				OnPurchaseError(onError));
		}

		public void PurchaseForVirtualCurrency(CatalogItemModel item, Action<CatalogItemModel> onSuccess = null, Action<Error> onError = null,
			bool isConfirmationRequired = true, bool isShowResultToUser = true)
		{
			var onConfirmation = new Action(() =>
			{
				var isPurchaseComplete = false;
				PopupFactory.Instance.CreateWaiting().SetCloseCondition(() => isPurchaseComplete);

				SdkPurchaseLogic.Instance.PurchaseForVirtualCurrency(item,
					itemModel =>
					{
						isPurchaseComplete = true;
						OnSuccessPurchase(onSuccess, isShowResultToUser)?.Invoke(itemModel);
					},
					error =>
					{
						isPurchaseComplete = true;
						OnPurchaseError(onError)?.Invoke(error);
					});
			});

			if (isConfirmationRequired)
				StoreDemoPopup.ShowConfirm(onConfirmation);
			else
				onConfirmation.Invoke();
		}

		public void PurchaseCart(List<UserCartItem> items, Action<List<UserCartItem>> onSuccess, Action<Error> onError = null, bool isShowResultToUser = true)
		{
			var restrictedPaymentAllower = GenerateAllower();

			Action<List<UserCartItem>> onSuccessPurchase = purchasedItems =>
			{
				if (isShowResultToUser)
					CompletePurchase(popupCallback: () => onSuccess?.Invoke(purchasedItems));
				else
				{
					UserInventory.Instance.Refresh(onError: StoreDemoPopup.ShowError);
					onSuccess?.Invoke(purchasedItems);
				}
			};

			SdkPurchaseLogic.Instance.PurchaseCart(items,
				restrictedPaymentAllower,
				onSuccessPurchase,
				OnPurchaseError(onError));
		}

		private RestrictedPaymentAllower GenerateAllower()
		{
			var restrictedPaymentAllower = new RestrictedPaymentAllower();
			restrictedPaymentAllower.OnRestrictedPayment = _ =>
			{
				PopupFactory.Instance.CreateConfirmation()
					.SetMessage("This payment method is not available for in-game browser. Open browser app to continue purchase?")
					.SetConfirmCallback(() => restrictedPaymentAllower.OnAllowed?.Invoke(true))
					.SetCancelCallback(() => restrictedPaymentAllower.OnAllowed?.Invoke(false));
			};

			return restrictedPaymentAllower;
		}

		private Action<CatalogItemModel> OnSuccessPurchase(Action<CatalogItemModel> onSuccess, bool isShowResult = true)
		{
			return purchasedItem =>
			{
				CompletePurchase(purchasedItem, isShowResultToUser: isShowResult);
				onSuccess?.Invoke(purchasedItem);
			};
		}

		private Action<Error> OnPurchaseError(Action<Error> onError)
		{
			return error =>
			{
				StoreDemoPopup.ShowError(error);
				onError?.Invoke(error);
			};
		}

		private static void CompletePurchase(CatalogItemModel item = null, bool isShowResultToUser = true, Action popupCallback = null)
		{
			UserInventory.Instance.Refresh(onError: StoreDemoPopup.ShowError);

			if (!isShowResultToUser)
				return;

			Action callback = () =>
			{
				if (item != null)
					StoreDemoPopup.ShowSuccess($"You have purchased '{item.Name}'", popupCallback);
				else
					StoreDemoPopup.ShowSuccess(null, popupCallback);
			};

			var inAppBrowser = BrowserHelper.Instance.InAppBrowser;
			if (inAppBrowser != null && inAppBrowser.IsOpened)
				inAppBrowser.AddCloseHandler(callback);
			else
				callback.Invoke();
		}
	}
}