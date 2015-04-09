using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Xml;
using WeiXinPay.Lib;

namespace WeiXinPay.Web
{
    public partial class Notify : System.Web.UI.Page
    {
        /// <summary>
        /// 状态锁
        /// </summary>
        private static object syncObj = new object();
        /// <summary>
        /// 页面加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                PayNotifyCallbak();
            }
        }
        /// <summary>
        /// 微信支付->回调处理
        /// </summary
        protected void PayNotifyCallbak()
        {
            lock (syncObj)
            {
                try
                {
                    //配置信息
                    PayModel payModel = GetPayModel();
                    if (payModel == null)
                    {
                        return;
                    }

                    #region ======获取返回数据包，并验证签名======
                    Hashtable hsMap = new Hashtable();
                    if (Request.InputStream.Length > 0)
                    {
                        var xmlDoc = new XmlDocument();
                        xmlDoc.Load(Request.InputStream);
                        XmlNode root = xmlDoc.SelectSingleNode("xml");
                        XmlNodeList xnl = root.ChildNodes;
                        foreach (XmlNode xnf in xnl)
                        {
                            hsMap.Add(xnf.Name, xnf.InnerText);
                        }
                    }
                    string error = "";
                    if (!PayUtil.isWXsign(hsMap, payModel.AppKey, out error))
                    {
                        PayUtil.SaveLog("Notify页面,验证签名失败。错误信息：", error);
                        return;
                    }
                    #endregion

                    #region ==========协议参数===========================
                    //--------------协议参数------------------------------
                    //SUCCESS/FAIL此字段是通信标识，非交易标识，交易是否成功需要查
                    string return_code = GetRequest("return_code", "");
                    //返回信息，如非空，为错误原因签名失败参数格式校验错误
                    string return_msg = GetRequest("return_msg", "");
                    //微信分配的公众账号 ID
                    //string appid = GetRequest("appid");

                    //以下字段在 return_code 为 SUCCESS 的时候有返回---------
                    //微信支付分配的商户号
                    string mch_id = GetRequest("mch_id", "");
                    //微信支付分配的终端设备号
                    string device_info = GetRequest("device_info", "");
                    //微信分配的公众账号 ID
                    string nonce_str = GetRequest("nonce_str", "");
                    //业务结果 SUCCESS/FAIL
                    string result_code = GetRequest("result_code", "");
                    //错误代码 
                    string err_code = GetRequest("err_code", "");
                    //结果信息描述
                    string err_code_des = GetRequest("err_code_des", "");

                    //以下字段在 return_code 和 result_code 都为 SUCCESS 的时候有返回------------
                    //-------------业务参数---------------------------------------------------------
                    //用户在商户 appid 下的唯一标识
                    string openid = GetRequest("openid", "");
                    //用户是否关注公众账号，Y-关注，N-未关注，仅在公众账号类型支付有效
                    string is_subscribe = GetRequest("is_subscribe", "");
                    //JSAPI、NATIVE、MICROPAY、APP
                    string trade_type = GetRequest("trade_type", "");
                    //银行类型，采用字符串类型的银行标识
                    string bank_type = GetRequest("bank_type", "");
                    //订单总金额，单位为分
                    string total_fee = GetRequest("total_fee", "");
                    //货币类型，符合 ISO 4217 标准的三位字母代码，默认人民币：CNY
                    string fee_type = GetRequest("fee_type", "");
                    //微信支付订单号e
                    string transaction_id = GetRequest("transaction_id", "");
                    //商户系统的订单号，与请求一致。
                    string out_trade_no = GetRequest("out_trade_no", "");
                    //商家数据包，原样返回
                    string attach = GetRequest("attach", "");
                    //支付完成时间，格式为yyyyMMddhhmmss，如2009年12月27日9点10分10秒表示为 20091227091010。时区为 GMT+8 beijing。该时间取自微信支付服务器
                    string time_end = GetRequest("time_end", "");

                    //记录支付信息
                    Hashtable hs = new Hashtable();
                    string str = "";
                    foreach (string key in Request.QueryString)
                    {
                        hs.Add(key, Request.QueryString[key]);
                        str += key + "=" + Request.QueryString[key] + "&";
                    }
                    #endregion

                    //支付成功
                    if (!out_trade_no.Equals("") && return_code.Equals("SUCCESS") && result_code.Equals("SUCCESS"))
                    {
                        PayUtil.SaveLog("Notify页面->支付成功，支付信息.", "商家订单号：" + out_trade_no + "、支付金额(分)：" + total_fee + "、自定义参数：" + attach);

                        /**
                         *  这里输入用户逻辑操作，比如更新订单的支付状态、记录微信支付订单号
                         *  
                         * 
                         * 
                         * 
                         * 
                         * 
                         * **/

                        HttpContext.Current.Response.Write("success");
                        return;
                    }
                    else
                    {
                        PayUtil.SaveLog("Notify页面->支付失败，支付信息: ", str);
                    }
                }
                catch (Exception ee)
                {
                    PayUtil.SaveLog("Notify 页面  发送异常错误：" , ee.Message);
                }
            }

            HttpContext.Current.Response.End();
        }
        /// <summary>
        /// 获取配置信息
        /// </summary>
        /// <returns></returns>
        public PayModel GetPayModel()
        {
            //如果有多个商家，可从数据库取商家配置信息
            //string appId=GetRequest("AppId","");
            //DataTable dt = new DataTable();
            //DataRow dr = dt.Rows[0];
            //PayModel p = new PayModel();
            //p.AppId = appId;
            //p.AppKey = dr["AppKey"].ToString();
            
            //本示例使用配置信息（单商家）
            if (!string.IsNullOrEmpty(PayConfig.AppId) && !string.IsNullOrEmpty(PayConfig.AppKey))
            {
                PayModel p = new PayModel();
                p.AppId = PayConfig.AppId;
                p.AppKey = PayConfig.AppKey;
                return p;
            }
            else
            {
                PayUtil.SaveLog("Notify页面->获取配置信息失败", "AppId:" + PayConfig.AppId + ";AppKey:" + PayConfig.AppKey);
                return null;
            }
        }

        /// <summary>
        /// 获得request参数的string类型值
        /// </summary>
        /// <param name="strName">参数</param>
        /// <param name="defValue">缺省值</param>
        /// <returns>参数的string类型值</returns>
        public string GetRequest(string strName, string defaultValue)
        {
            string vaule = Convert.ToString(HttpContext.Current.Request[strName]);
            if (vaule != null && vaule != "" && vaule.ToLower() != "null")
                return vaule;
            else
                return defaultValue;
        }
    }
}