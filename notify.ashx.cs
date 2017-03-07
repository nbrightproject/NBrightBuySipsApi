using System;
using System.Collections;
using System.Web;
using DotNetNuke.Entities.Portals;
using NBrightCore.common;
using Nevoweb.DNN.NBrightBuy.Components;

namespace NBrightBuySipsApi.DNN.NBrightStore
{
    /// <summary>
    /// Summary description for XMLconnector
    /// </summary>
    public class NBrightBuySipsApiNotify : IHttpHandler
    {
        private String _lang = "";

        /// <summary>
        /// This function needs to process and returned message from the bank.
        /// Thsi processing may vary widely between banks.
        /// </summary>
        /// <param name="context"></param>
        public void ProcessRequest(HttpContext context)
        {
            var modCtrl = new NBrightBuyController();
            var info = ProviderUtils.GetProviderSettings("NBrightBuySipsApipayment");

            try
            {

                var debugMode = info.GetXmlPropertyBool("genxml/checkbox/debugmode");
                var rtnMsg = "version=2" + Environment.NewLine + "cdr=1";

                // ------------------------------------------------------------------------
                // In this case the payment provider passes back data via form POST.
                // Get the data we need.
                string returnmessage = "";
                int NBrightBuySipsApiStoreOrderID = 0;
                string NBrightBuySipsApiCartID = "";
                string NBrightBuySipsApiClientLang = "";

                if ((context.Request.Form.Get("DATA") != null))
                {
                    returnmessage = "message=" + context.Request.Form.Get("DATA");

                    if (!string.IsNullOrEmpty(returnmessage))
                    {
                        // ------------------------------------------------------------------------
                        var settings = ProviderUtils.GetProviderSettings("NBrightBuySipsApipayment");

                        var controlMapPath = HttpContext.Current.Server.MapPath("/DesktopModules/NBright/NBrightBuySipsApi");
                        var pathfile = "pathfile=" + PortalSettings.Current.HomeDirectoryMapPath.TrimEnd('\\') + "\\" + settings.GetXmlProperty("genxml/textbox/paramfolder") + "\\pathfile";

                        var exepath = controlMapPath.TrimEnd('\\') + "\\sipsbin\\response.exe";
                        var sipsdata = ProviderUtils.CallSipsExec(exepath, pathfile + " " + returnmessage);

                        if (debugMode)
                        {
                            info.SetXmlProperty("genxml/debugmsg", sipsdata);
                            modCtrl.Update(info);
                        }

                        var tableau = sipsdata.Split('!');

                        string code = tableau[1];
                        string error_msg = tableau[2];

                        if (string.IsNullOrEmpty(code) | code == "-1")
                        {
                            info.SetXmlProperty("genxml/debugmsg", error_msg);
                            modCtrl.Update(info);
                        }
                        else
                        {
                            // L'execution s'est bien deroulee
                            // recuperation des donnees de la reponse

                            string merchant_id = tableau[3];
                            string merchant_country = tableau[4];
                            string amount = tableau[5];
                            string transaction_id = tableau[6];
                            string payment_means = tableau[7];
                            string transmission_date = tableau[8];
                            string payment_time = tableau[9];
                            string payment_date = tableau[10];
                            string response_code = tableau[11];
                            string payment_certificate = tableau[12];
                            string authorisation_id = tableau[13];
                            string currency_code = tableau[14];
                            string card_number = tableau[15];
                            string cvv_flag = tableau[16];
                            string cvv_response_code = tableau[17];
                            string bank_response_code = tableau[18];
                            string complementary_code = tableau[19];
                            string complementary_info = tableau[20];
                            string return_context = tableau[21];
                            string caddie = tableau[22];
                            string receipt_complement = tableau[23];
                            string merchant_language = tableau[24];
                            string language = tableau[25];
                            string customer_id = tableau[26];
                            string order_id = tableau[27];
                            string customer_email = tableau[28];
                            string customer_ip_address = tableau[29];
                            string capture_day = tableau[30];
                            string capture_mode = tableau[31];
                            string data = tableau[32];

                            // Sauvegarde des champs de la reponse
                            string Lmsg = null;

                            Lmsg = merchant_id + ",";
                            Lmsg += merchant_country + ",";
                            Lmsg += amount + ",";
                            Lmsg += transaction_id + ",";
                            Lmsg += transmission_date + ",";
                            Lmsg += payment_means + ",";
                            Lmsg += payment_time + ",";
                            Lmsg += payment_date + ",";
                            Lmsg += response_code + ",";
                            Lmsg += payment_certificate + ",";
                            Lmsg += authorisation_id + ",";
                            Lmsg += currency_code + ",";
                            Lmsg += card_number + ",";
                            Lmsg += cvv_flag + ",";
                            Lmsg += cvv_response_code + ",";
                            Lmsg += bank_response_code + ",";
                            Lmsg += complementary_code + ",";
                            Lmsg += complementary_info + ",";
                            Lmsg += return_context + ",";
                            Lmsg += caddie + ",";
                            Lmsg += receipt_complement + ",";
                            Lmsg += merchant_language + ",";
                            Lmsg += language + ",";
                            Lmsg += customer_id + ",";
                            Lmsg += order_id + ",";
                            Lmsg += customer_email + ",";
                            Lmsg += customer_ip_address + ",";
                            Lmsg += capture_day + ",";
                            Lmsg += capture_mode + ",";
                            Lmsg += data + ",";

                            //update database stuff
                            if (Utils.IsNumeric(order_id))
                            {
                                var orderData = new OrderData(Convert.ToInt32(order_id));
                                orderData.AddAuditMessage(Lmsg, "payment", "sipsapi",info.GetXmlProperty("genxml/checkbox/debugmode"));
                                // Status return "00" is payment successful
                                if (response_code == "00")
                                {
                                    //set order status to Payed
                                    orderData.PaymentOk();
                                }
                                else
                                {
                                    orderData.PaymentFail();
                                }
                            }
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                if (!ex.ToString().StartsWith("System.Threading.ThreadAbortException"))  // we expect a thread abort from the End response.
                {
                    info.SetXmlProperty("genxml/debugmsg", "NBrightBuySipsApi ERROR: " + ex.ToString());
                    modCtrl.Update(info);
                }
            }


        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }


    }
}