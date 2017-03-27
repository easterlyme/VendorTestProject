using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VendorTestProject.Controllers;

namespace VendorTestProject.Models.Cart
{
    public class AuthorizeViewModel
    {
        public string QRCode { get; set; }

        public string AuthorizationCode { get; set; }

        public string State { get; set; }

        public CartController.AuthorizationStatus Status { get; set; }
    }
}
