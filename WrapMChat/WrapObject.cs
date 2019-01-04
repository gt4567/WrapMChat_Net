using System;
using System.Collections.Generic;
using System.Text;

namespace WrapMChat
{
    class WrapObject
    {
        WrapMChat MChat = WrapMChat.GetInstance();

        public string GetQRCode()
        {
            return MChat.GetQRCode();
        }

        public string GetPayStStr()
        {
            return MChat.GetPayStStr();
        }

        public void InitPayment(string _url, string _apiKey, string _paymentSecret)
        {
            MChat.InitPayment(_url, _apiKey, _paymentSecret);
        }

        public bool SetBillInfo(string _title, string _subTitle, double _totalPrice, string _billid, string _info)
        {
            return MChat.SetBillInfo(_title, _subTitle, _totalPrice, _billid, _info);
        }

        public bool GenerateQRCode()
        {
            return MChat.GenerateQRCode();
        }

        public bool CheckQRCode()
        {
            return MChat.CheckQRCode();
        }

        public bool CancelTrans()
        {
            return MChat.CancelTrans();
        }
    }
}
