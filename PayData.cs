using System;
using System.Collections;
using System.Diagnostics.Eventing.Reader;
using System.Dynamic;
using System.Security.Cryptography;
using System.Text;
using DotNetNuke.Common;
using DotNetNuke.Entities.Portals;
using Nevoweb.DNN.NBrightBuy.Components;

namespace NBrightBuySipsApi.DNN.NBrightStore
{

    public class PayData
    {

        public PayData(OrderData oInfo)
        {
            LoadSettings(oInfo);
        }

        public void LoadSettings(OrderData oInfo)
        {
            var settings = ProviderUtils.GetProviderSettings("NBrightBuySipsApipayment");
            var appliedtotal = oInfo.PurchaseInfo.GetXmlPropertyDouble("genxml/appliedtotal");
            var alreadypaid = oInfo.PurchaseInfo.GetXmlPropertyDouble("genxml/alreadypaid");

            var PostUrl = oInfo.PurchaseInfo.GetXmlPropertyDouble("genxml/posturl");

            var orderTotal = (appliedtotal - alreadypaid).ToString("0.00");

        }

        public string PostUrl { get; set; }


    }


}
