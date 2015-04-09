using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace WeiXinPay.Lib
{
    [Serializable]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "xml")]
    public class WeiXinOderInfo
    {
        public string return_code { set; get; }
        public string return_msg { set; get; }
        public string result_code { set; get; }
        public string appid { set; get; }
        public string mch_id { set; get; }
        public string nonce_str { set; get; }
        public string sign { set; get; }
        public string err_code { set; get; }

        public string attach { set; get; }
        public string bank_type { set; get; }
        public string fee_type { set; get; }
        public string is_subscribe { set; get; }
        public string openid { set; get; }
        public string out_trade_no { set; get; }
        public string sub_mch_id { set; get; }
        public string time_end { set; get; }
        public string total_fee { set; get; }
        public string trade_type { set; get; }
        public string transaction_id { set; get; }
    }
}
