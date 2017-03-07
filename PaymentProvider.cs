using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Web;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using NBrightDNN;
using Nevoweb.DNN.NBrightBuy.Components;

namespace NBrightBuySipsApi.DNN.NBrightStore
{
    public class NBrightBuySipsApiPaymentProvider : Nevoweb.DNN.NBrightBuy.Components.Interfaces.PaymentsInterface
    {
        public override string Paymentskey { get; set; }

        public override string GetTemplate(NBrightInfo cartInfo)
        {
            var info = ProviderUtils.GetProviderSettings("NBrightBuySipsApipayment");
            var templ = ProviderUtils.GetTemplateData(info.GetXmlProperty("genxml/textbox/checkouttemplate"), info);

            return templ;
        }

        public override string RedirectForPayment(OrderData orderData)
        {
            orderData.OrderStatus = "020";
            orderData.PurchaseInfo.SetXmlProperty("genxml/paymenterror", "");
            orderData.PurchaseInfo.Lang = Utils.GetCurrentCulture();
            orderData.SavePurchaseData();
            try
            {
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.Write(ProviderUtils.GetBankRemotePost(orderData));
            }
            catch (Exception ex)
            {
                // rollback transaction
                orderData.PurchaseInfo.SetXmlProperty("genxml/paymenterror", "<div>ERROR: Invalid payment data </div><div>" + ex + "</div>");
                orderData.PaymentFail();
                var param = new string[3];
                param[0] = "orderid=" + orderData.PurchaseInfo.ItemID.ToString("");
                param[1] = "status=0";
                HttpContext.Current.Response.Clear();
                HttpContext.Current.Response.Write("<html>" + orderData.PurchaseInfo.GetXmlProperty("genxml/paymenterror") + "</html>");
            }

            try
            {
                HttpContext.Current.Response.End();
            }
            catch (Exception ex)
            {
                // this try/catch to avoid sending error 'ThreadAbortException'  
            }

            return "";
        }

        public override string ProcessPaymentReturn(HttpContext context)
        {
            var orderid = Utils.RequestQueryStringParam(context, "orderid");
            if (Utils.IsNumeric(orderid))
            {
                var status = Utils.RequestQueryStringParam(context, "status");
                if (status == "0")
                {
                    var orderData = new OrderData(Convert.ToInt32(orderid));
                    var rtnerr = orderData.PurchaseInfo.GetXmlProperty("genxml/paymenterror");
                    if (rtnerr == "") rtnerr = "fail"; // to return this so a fail is activated.

                    // check we have a waiting for bank status (IPN may have altered status already)
                    if (orderData.OrderStatus == "020") orderData.PaymentFail();

                    return rtnerr;
                }
            }
            return "";
        }

    }
}
