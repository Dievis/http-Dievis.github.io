using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ShopBanDoDienTu_Nhom1.ViewModel
{
    public class ShoppingCartVM
    {
        public List<CartItemVM> CartItems { get; set; }
        public decimal CartTotal { get; set; }
    }
}