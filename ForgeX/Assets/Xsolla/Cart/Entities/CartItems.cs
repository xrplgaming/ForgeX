using System;
using Xsolla.Core;

namespace Xsolla.Cart
{
	[Serializable]
	public class CartItems
	{
		public string cart_id;
		public bool is_free;
		public Price price;
		public CartItem[] items;
	}
}
