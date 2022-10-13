using System.Collections;
using UnityEngine;

namespace Xsolla.Orders
{
	public class OrderTrackerByShortPolling : OrderTracker
	{
		private const float CHECK_STATUS_COOLDOWN = 3f;

		private Coroutine checkStatusCoroutine;

		public override void Start()
		{
			checkStatusCoroutine = StartCoroutine(CheckOrderStatus());
		}

		public override void Stop()
		{
			if (checkStatusCoroutine != null)
				StopCoroutine(checkStatusCoroutine);
		}

		private IEnumerator CheckOrderStatus()
		{
			while (true)
			{
				yield return new WaitForSeconds(CHECK_STATUS_COOLDOWN);

				CheckOrderStatus(
					onDone: () =>
					{
						trackingData.successCallback?.Invoke();
						RemoveSelfFromTracking();
					},
					onCancel: RemoveSelfFromTracking,
					onError: error =>
					{
						trackingData.errorCallback?.Invoke(error);
						RemoveSelfFromTracking();
					}
				);
			}
		}

		public OrderTrackerByShortPolling(OrderTrackingData trackingData, OrderTracking orderTracking) : base(trackingData, orderTracking)
		{
		}
	}
}
