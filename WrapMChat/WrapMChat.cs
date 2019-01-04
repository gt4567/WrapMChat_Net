using System;
using System.Collections;
using System.Threading.Tasks;
using MChatSDK;

namespace WrapMChat
{
    public class WrapMChat
    {
        private static WrapMChat instance;

        MChatScanPaymentBuilder paymentBuilder = new MChatScanPaymentBuilder();
        MChatScanPayment payment;

        MChatGenerateQRCodeRequestBody body = new MChatGenerateQRCodeRequestBody();

        string QRCode = "";
        string stStr = "";

        bool isInit = false;

        public WrapMChat()
        {

        }

        public static WrapMChat GetInstance()
        {
            if (instance == null)
            {
                instance = new WrapMChat();
            }

            return instance;
        }

        public string GetQRCode()
        {
            return QRCode;
        }

        public string GetPayStStr()
        {
            return stStr;
        }

        public void InitPayment(string _url, string _apiKey, string _paymentSecret)
        {
            paymentBuilder.url = _url;
            paymentBuilder.apiKey = _apiKey;
            paymentBuilder.paymentSecret = _paymentSecret;

            payment = paymentBuilder.Build();

            isInit = true;
        }

        public bool SetBillInfo(string _title, string _subTitle, double _totalPrice, string _billid, string _info)
        {
            if (!isInit)
                return false;

            body.totalPrice = _totalPrice;
            body.title = _title;
            body.subTitle = _subTitle;
            body.noat = (_totalPrice * 0.1).ToString();
            body.nhat = "0";
            body.ddtd = "0";
            body.ttd = "0";
            body.lotteryID = "0";
            body.billType = "Huvi";
            body.billID = _billid;
            body.billQRCode = "0";

            Console.WriteLine("["+DateTime.Now.ToString("hh:mm:ss.fff") +"] totalPrice: " + body.totalPrice + ",noat: " + body.noat);

            if (_info == null)
            {
                body.products = null;
            }
            else
            {
                ArrayList products = new ArrayList();

                string[] infoList = _info.Split('|');
                for(int i=0;i<infoList.Length;i++)
                {
                    string tmp = infoList[i];
                    string[] tmpList = tmp.Split(';');

                    if (tmpList.Length == 3)
                    {
                        MChatProduct product = new MChatProduct();
                        product.name = tmpList[0];
                        product.quantity = int.Parse(tmpList[1]);
                        product.unitPrice = double.Parse(tmpList[2]);

                        Console.WriteLine("[" + DateTime.Now.ToString("hh:mm:ss.fff") + "] product:name:" + product.name + ",quantity:" + product.quantity + ",unitPrice:" + product.unitPrice);

                        products.Add(product);
                    }
                }

                body.products = products;
            }

            Console.WriteLine("[" + DateTime.Now.ToString("hh:mm:ss.fff") + "] products count:" + body.products.Count);

            return true;
        }

        public bool GenerateQRCode()
        {
            if (!isInit)
                return false;

            QRCode = "";
            bool ret = false;

            var t = Task.Run(async () =>
            {
                // Generate QR-Code
                MChatResponseGenerateQRCode response = await payment.GenerateNewCodeAsync(body, (MChatScanPayment scanPayment, BNSState state, String generatedQRCode, MChatResponse res) =>
                {
                    if (state == BNSState.Ready)
                    {
                        // Succesfully connected and ready to receive notification from notification service
                        Console.WriteLine("[" + DateTime.Now.ToString("hh:mm:ss.fff") + "] Ready to display QRCode: " + generatedQRCode);

                        QRCode = generatedQRCode;
                    }
                    else if (state == BNSState.Connected)
                    {
                        // Successfully connected to notification service
                        Console.WriteLine("[" + DateTime.Now.ToString("hh:mm:ss.fff") + "] Successfully connected to notification service");
                    }
                    else if (state == BNSState.Disconnected)
                    {
                        // Disconnected from notification service
                        Console.WriteLine("[" + DateTime.Now.ToString("hh:mm:ss.fff") + "] Disconnected from notification service");
                    }
                    else if (state == BNSState.PaymentSuccessful)
                    {
                        // Got response from payment notification service
                        //Console.WriteLine("[" + DateTime.Now.ToString("hh:mm:ss.fff") + "] PaymentSuccesfull: " + generatedQRCode + "\n" + res);
                        Console.WriteLine("[" + DateTime.Now.ToString("hh:mm:ss.fff") + "] PaymentSuccesfull: " + generatedQRCode);
                    }
                    else if (state == BNSState.ErrorOccured)
                    {
                        // Error Occured when connection notification service
                        Console.WriteLine("[" + DateTime.Now.ToString("hh:mm:ss.fff") + "] Error Occured when connection notification service,\n res:" + res);
                    }
                });

                QRCode = response.generatedQRCode;

                if (response.code == 1000)
                {
                    ret = true;
                }

                Console.WriteLine("[" + DateTime.Now.ToString("hh:mm:ss.fff") + "] code: " + response.code + ",msg: " + response.message + ",QRCode: " + response.generatedQRCode);
            });

            try
            {
                t.Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return ret;
        }

        public bool CheckQRCode()
        {
            if (!isInit)
                return false;

            if (QRCode == null)
            {
                return false;
            }

            stStr = "";

            var t = Task.Run(async () =>
            {
                // Check QR-Code Status
                MChatResponseCheckState responseState = await payment.CheckQRCodePaymentState(QRCode);
                Console.WriteLine("[" + DateTime.Now.ToString("hh:mm:ss.fff") + "] CheckQRCode:status: " + responseState.status + ",code: " + responseState.code + ",msg: " + responseState.message);

                stStr = responseState.status;

                if (responseState.code == 2000 || responseState.code == 2006)
                {
                    stStr = "Parameters Error";
                }
            });

            try
            {
                t.Wait();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            return true;
        }

        public bool CancelTrans()
        {
            if (!isInit)
                return false;

            payment.DisconnectFromBusinessNotificationService();
            return true;
        }
    }
}
