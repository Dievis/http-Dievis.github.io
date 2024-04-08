using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.EntityFramework;

namespace ShopBanDoDienTu_Nhom1.Identity
{
    public class AppUser : IdentityUser
    {
        [DisplayFormat(DataFormatString = "{0: dd/MM/yyyy}")]
        public DateTime? Birthday { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
    }
}