using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;

namespace AuthorizePay.Controllers
{
    public class HomeController : Controller
    {
        private const string ApiLoginId = "4n7K8JwF";
        private const string TransactionKey = "87f74Jz3ReM6wFVk";

        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ProcessPayment(string cardNumber, string expirationDate, decimal amount)
        {
            string strStatus = string.Empty;
            string strMessage = string.Empty;
            try
            {
                var creditCard = new creditCardType
                {
                    cardNumber = cardNumber,
                    expirationDate = expirationDate
                };

                var paymentType = new paymentType { Item = creditCard };

                var transactionRequest = new transactionRequestType
                {
                    transactionType = transactionTypeEnum.authCaptureTransaction.ToString(),
                    amount = amount,
                    payment = paymentType
                };

                var request = new createTransactionRequest { transactionRequest = transactionRequest };

                ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = AuthorizeNet.Environment.SANDBOX; // Change to AuthorizeNet.Environment.PRODUCTION for live transactions
                ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType
                {
                    name = ApiLoginId,
                    ItemElementName = ItemChoiceType.transactionKey,
                    Item = TransactionKey
                };

                var controller = new createTransactionController(request);
                controller.Execute();

                var response = controller.GetApiResponse();

                if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
                {
                    // Payment successful
                    strStatus = "01";
                    strMessage = "Payment successful!";
                }
                else
                {
                    // Payment failed
                    strStatus = "00";
                    strMessage = $"Payment failed: {response?.messages?.message?[0]?.text}";
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                strStatus = "00";
                strMessage = $"An error occurred: {ex.Message}";
            }

            return Json(new { Status = strStatus, Message = strMessage });
        }
    }
}