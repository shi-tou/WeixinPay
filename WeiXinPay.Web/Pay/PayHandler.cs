using System;
using System.Web;
using Newtonsoft.Json;
using System.Collections;

namespace WeiXinPay.Lib
{
    /// <summary>
    /// 您将需要在网站的 Web.config 文件中配置此处理程序 
    /// 并向 IIS 注册它，然后才能使用它。有关详细信息，
    /// 请参见下面的链接: http://go.microsoft.com/?linkid=8101007
    /// </summary>
    public class PayHandler : IHttpHandler
    {
        private string UserOpenId { set; get; }
        private static object syncObj = new object();
        public void ProcessRequest(HttpContext context)
        {
            //在此处写入您的处理程序实现。
            HttpRequest request = context.Request;
            HttpResponse response = context.Response;
            PayModel payModel = JsonConvert.DeserializeObject<PayModel>(request["payinfo"]);
            if (string.IsNullOrEmpty(payModel.OpenId))
            {
                string redirect_uri = HttpUtility.UrlEncode(request.Url.ToString());
                GetUserOpenId(payModel, redirect_uri);
                payModel.OpenId = UserOpenId;
                response.Redirect(payModel.ToString());
            }
        }
        /// <summary>
        /// 获取当前用户的微信 OpenId，如果知道用户的OpenId请不要使用该函数
        /// </summary>
        private void GetUserOpenId(PayModel payModel, string redirect_uri)
        {

            if (!string.IsNullOrEmpty(payModel.AppId) && !string.IsNullOrEmpty(payModel.AppKey) && !string.IsNullOrEmpty(payModel.AppSecret))
            {
                HttpRequest Request = HttpContext.Current.Request;
                HttpResponse Response = HttpContext.Current.Response;
                string code = Request.QueryString["code"];
                if (string.IsNullOrEmpty(code))
                {
                    string code_url = string.Format("https://open.weixin.qq.com/connect/oauth2/authorize?appid={0}&redirect_uri={1}&response_type=code&scope=snsapi_base&state=lk#wechat_redirect", payModel.AppId, redirect_uri);
                    Response.Redirect(code_url);
                }
                else
                {
                    #region =======获取支付用户 OpenID=========
                    string url = string.Format("https://api.weixin.qq.com/sns/oauth2/access_token?appid={0}&secret={1}&code={2}&grant_type=authorization_code", payModel.AppId, payModel.AppSecret, code);
                    string returnStr = PayUtil.Send("", url);
                    PayUtil.SaveLog("获取token返回信息：", returnStr);
                    var obj = JsonConvert.DeserializeObject<OpenModel>(returnStr);

                    url = string.Format("https://api.weixin.qq.com/sns/oauth2/refresh_token?appid={0}&grant_type=refresh_token&refresh_token={1}", payModel.AppId, obj.refresh_token);
                    returnStr = PayUtil.Send("", url);
                    obj = JsonConvert.DeserializeObject<OpenModel>(returnStr);
                    PayUtil.SaveLog("用户access_token/openid：", obj.access_token + "/" + obj.openid);

                    //url = string.Format("https://api.weixin.qq.com/sns/userinfo?access_token={0}&openid={1}", obj.access_token, obj.openid);
                    //returnStr = HttpUtil.Send("", url);
                    //PayUtil.SaveLog("用户信息：" , returnStr);

                    UserOpenId = obj.openid;
                    #endregion
                }
            }
            else
            {
                PayUtil.SaveLog("获取用户的微信用户OpenId失败", "参数->AppId:" + payModel.AppId + ";AppKey:" + payModel.AppKey + ";AppSecret:" + payModel.AppSecret);
            }
        }
        /// <summary>
        /// 查询微信支付订单信息
        /// </summary>
        /// <param name="appid"></param>
        /// <param name="out_trade_no"></param>
        /// <returns></returns>
        public WeiXinOderInfo ReqQueryOrder(string appId,string appKey,string mchId, string out_trade_no)
        {
            WeiXinOderInfo oder = null;

            if (!string.IsNullOrEmpty(appId) && !string.IsNullOrEmpty(appKey) && !string.IsNullOrEmpty(mchId))
            {
                //初始化
                Hashtable hs = new Hashtable();
                hs.Add("appid", appId);
                hs.Add("mch_id",mchId);
                hs.Add("nonce_str", PayUtil.GetNoncestr().ToLower());
                hs.Add("out_trade_no", out_trade_no);
                string sign = PayUtil.CreateMd5Sign(hs, appKey, HttpContext.Current.Request.ContentEncoding.BodyName);
                PayUtil.SaveLog("ReqQueryOrder  sign：", sign);
                hs.Add("sign", sign);
                string data = PayUtil.ParseXML(hs);
                PayUtil.SaveLog("ReqQueryOrder  package（XML）：", data);

                string orderQueryResponseXml = PayUtil.Send(data, "https://api.mch.weixin.qq.com/pay/orderquery");
                PayUtil.SaveLog("ReqQueryOrder  package（Back_XML）：", orderQueryResponseXml);

                oder = PayUtil.XmlDeSerialize<WeiXinOderInfo>(orderQueryResponseXml, System.Text.Encoding.UTF8);
            }
            return oder;
        }

        public bool IsReusable
        {
            // 如果无法为其他请求重用托管处理程序，则返回 false。
            // 如果按请求保留某些状态信息，则通常这将为 false。
            get { return true; }
        }

       
    }
}
