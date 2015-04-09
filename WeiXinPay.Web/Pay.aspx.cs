using System;
using System.Collections;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Xml.Linq;
using System.Xml;

using Newtonsoft.Json;
using System.IO;
using System.Text;
using WeiXinPay.Lib;

namespace WeiXinPay.Web
{
    /// <summary>
    /// 微信支付核心页面，该页面获取用户的支付信息显示网页上。
    /// 通过其它配置参数和支付参数调用微信支付Api获取相关其它数据。
    /// 通过点击页面“确认支付”按钮来发起支付操作
    /// </summary>
    public partial class Pay : System.Web.UI.Page
    {
        #region 支付相关参数 ，以下参数请从需要支付的页面通过get方式传递过来
        //自定义code
        public string Code
        {
            get { return GetRequest("code", ""); }
        }
        //AppId
        protected string AppId
        {
            get { return GetRequest("AppId", ""); }
        }
        //AppKey
        protected string AppKey
        {
            get { return GetRequest("AppKey", ""); }
        }
        //AppSecret
        protected string AppSecret
        {
            get { return GetRequest("AppSecret", ""); }
        }
        //MchId
        protected string MchId
        {
            get { return GetRequest("MchId", ""); }
        }
        //商户自己订单号
        protected string OrderSN
        {
            get { return GetRequest("OrderSN", ""); }
        }
        //商品描述
        protected string Body
        {
            get { return GetRequest("Body", ""); }
        }
        //总支付金额，单位为：分，不能有小数
        protected string TotalFee
        {
            get { return GetRequest("TotalFee", ""); }
        }
        //用户自定义参数，原样返回
        protected string Attach
        {
            get { return GetRequest("Attach", ""); }
        }
        //微信用户openid
        protected string UserOpenId
        {
            get { return GetRequest("UserOpenId", ""); }
        }
        #endregion

        #region 生成的支付相差参数
        //预支付ID
        public string PrepayId = "";
        //为了获取预支付ID的签名
        public string Sign = "";
        //进行支付需要的签名
        public string PaySign = "";
        //进行支付需要的包
        public string Package = "";
        //时间戳 程序生成 无需填写
        public string TimeStamp
        {
            get { return PayUtil.GetTimestamp(); }
        }
        //随机字符串  程序生成 无需填写
        public string NonceStr
        {
            get { return PayUtil.GetNoncestr(); }
        }
        #endregion

        /// <summary>
        /// 页面加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(AppId) && !string.IsNullOrEmpty(AppKey) && !string.IsNullOrEmpty(MchId))
            {
                PayUtil.SaveLog("单次支付开始", "");
                PayUtil.SaveLog("传递支付参数", string.Format("OrderSN={0}、Body={1}、TotalFee={2}、Attach={3}、UserOpenId={4}", this.OrderSN, this.Body, this.TotalFee, this.Attach, this.UserOpenId));

                #region 基本参数===========================

                //设置package订单参数  具体参数列表请参考官方pdf文档，请勿随意设置
                Hashtable hsRequest = new Hashtable();
                hsRequest.Add("body", this.Body); //商品信息 127字符
                hsRequest.Add("appid", AppId);
                hsRequest.Add("mch_id", MchId);
                hsRequest.Add("nonce_str", NonceStr.ToLower());
                hsRequest.Add("notify_url", PayConfig.NotifyUrl);
                hsRequest.Add("openid", this.UserOpenId);
                hsRequest.Add("out_trade_no", this.OrderSN); //商家订单号
                hsRequest.Add("spbill_create_ip", Page.Request.UserHostAddress); //用户的公网ip，不是商户服务器IP
                hsRequest.Add("total_fee", this.TotalFee); //商品金额,以分为单位(money * 100).ToString()
                hsRequest.Add("trade_type", "JSAPI");
                if (!string.IsNullOrEmpty(this.Attach))
                    hsRequest.Add("attach", this.Attach);//自定义参数 127字符

                #endregion

                #region ===========生成签名==========
                PayUtil.CreateMd5Sign(hsRequest, AppKey, Request.ContentEncoding.BodyName);
                PayUtil.SaveLog("Pay页面Sign：", Sign);
                #endregion

                #region ===========获取package扩展包==========
                hsRequest.Add("sign", Sign);
                string data = PayUtil.ParseXML(hsRequest);
                PayUtil.SaveLog("Pay页面package（XML）：", data);

                string prepayXml = PayUtil.Send(data, "https://api.mch.weixin.qq.com/pay/unifiedorder");
                PayUtil.SaveLog("Pay页面package（Back_XML）：", prepayXml);

                //获取预支付ID
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(prepayXml);
                XmlNode xn = xmlDoc.SelectSingleNode("xml");
                XmlNodeList xnl = xn.ChildNodes;
                if (xnl.Count > 7)
                {
                    PrepayId = xnl[7].InnerText;
                    Package = string.Format("prepay_id={0}", PrepayId);
                    PayUtil.SaveLog("Pay页面package：", Package);
                }
                #endregion

                #region =======生成【微信支付签名】=======
                Hashtable hsPaySign = new Hashtable();
                hsPaySign.Add("appId", AppId);
                hsPaySign.Add("timeStamp", TimeStamp);
                hsPaySign.Add("nonceStr", NonceStr);
                hsPaySign.Add("package", Package);
                hsPaySign.Add("signType", "MD5");
                PaySign = PayUtil.CreateMd5Sign(hsPaySign, AppKey, Request.ContentEncoding.BodyName);
                PayUtil.SaveLog("Pay页面paySign：", PaySign);
                #endregion
                //页面数据显示
                BindData();
            }
            else
            {
                Response.Write("参数不正确");
                Response.End();
            }
        }
        /// <summary>
        ///绑定页面数据
        /// </summary>
        private void BindData()
        {
            this.lblPrice.Text = this.TotalFee;
            this.lblBody.Text = this.Body;
            this.lblOrderSN.Text = this.OrderSN;
            this.lblAttach.Text = this.Attach;
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
