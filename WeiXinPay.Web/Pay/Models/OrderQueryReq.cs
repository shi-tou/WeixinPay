using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WeiXinPay.Lib
{
    [Serializable]
    public class OrderQueryReq
    {
        public string appid { set; get; }
        public string mch_id { set; get; }
        public string transaction_id { set; get; }
        public string out_trade_no { set; get; }
        public string nonce_str { set; get; }
        public string sign { set; get; }
    }
}
