using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Authentication;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VendorTestProject.Models.Cart;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace VendorTestProject.Controllers
{
    public class CartController : Controller
    {
        public HiddenSoundOptions HiddenSoundOptions { get; set; }

        public CartController(IOptions<HiddenSoundOptions> hiddenSoundOptions)
        {
            HiddenSoundOptions = hiddenSoundOptions.Value;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Checkout()
        {
            // Instruct the OIDC client middleware to redirect the user agent to the identity provider.
            // Note: the authenticationType parameter must match the value configured in Startup.cs
            return new ChallengeResult(OpenIdConnectDefaults.AuthenticationScheme, new AuthenticationProperties
            {
                RedirectUri = "/"
            });
        }

        [HttpGet]
        public async Task<IActionResult> Authorize(string state)
        {
            var model = new AuthorizeViewModel();

            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", state);

                var response = await httpClient.PostAsync(HiddenSoundOptions.ApiUri + "/Api/Authorization/Create", new StringContent(""));
                var jsonString = await response.Content.ReadAsStringAsync();
                var responseModel = JsonConvert.DeserializeObject<AuthorizationCreateResponse>(jsonString);

                model.QRCode = responseModel.Base64QR;
                model.AuthorizationCode = responseModel.AuthorizationCode;
                model.State = state;
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AuthorizeStatus(AuthorizeViewModel model)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", model.State);

                var response = await httpClient.GetAsync(HiddenSoundOptions.ApiUri + "/Api/Authorization/Info?authorizationCode=" + WebUtility.UrlEncode(model.AuthorizationCode));
                var jsonString = await response.Content.ReadAsStringAsync();
                var responseModel = JsonConvert.DeserializeObject<AuthorizationInfoResponse>(jsonString);

                model.Status = responseModel.Status;
            }

            if (model.Status == AuthorizationStatus.Approved)
            {
                return RedirectToAction("CartRedirectApproved");
            }
            else if (model.Status == AuthorizationStatus.Declined)
            {
                return RedirectToAction("CartRedirectDeclined");
            }

            return PartialView("_AuthorizeStatus", model);
        }

        [HttpGet]
        public ActionResult Approved()
        {
            return View();
        }

        [HttpGet]
        public ActionResult Declined()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CartRedirectApproved()
        {
            return PartialView("_CartRedirectApproved");
        }

        [HttpGet]
        public IActionResult CartRedirectDeclined()
        {
            return PartialView("_CartRedirectDeclined");
        }

        private class AuthorizationInfoResponse
        {
            public AuthorizationStatus Status { get; set; }

            public string Base64QR { get; set; }
        }

        public enum AuthorizationStatus
        {
            Pending = 0,

            Approved = 1,

            Declined = 2,

            Expired = 3
        }

        private class AuthorizationCreateResponse
        {
            public string AuthorizationCode { get; set; }

            public string Base64QR { get; set; }

            public DateTime ExpiresOn { get; set; }
        }
    }
}
