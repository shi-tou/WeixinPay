using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace WeiXinPay.Lib
{
    /// <summary>
    /// 微信支付公用参数，微信支付版本V3.3.7
    /// </summary>
    public class PayConfig
    {
        /// <summary>
        /// 人民币
        /// </summary>
        public static string Tenpay = "1"; 

        private static string mchId = null;
        /// <summary>
        /// mchid ， 微信支付商户号
        /// </summary>
        public static string MchId
        {
            get
            {
                if (mchId == null)
                {
                    mchId = System.Configuration.ConfigurationManager.AppSettings["mch_id"];
                }
                return mchId;
            }
        }

        private static string appId = null;
        /// <summary>
        /// appid，应用ID， 在微信公众平台中 “开发者中心”栏目可以查看到
        /// </summary>
        public static string AppId
        {
            get
            {
                if (appId == null)
                {
                    appId = System.Configuration.ConfigurationManager.AppSettings["appid"];
                }
                return appId;
            }
        }

        private static string appsecret = null;
        /// <summary>
        /// appsecret ，应用密钥， 在微信公众平台中 “开发者中心”栏目可以查看到
        /// </summary>
        public static string AppSecret
        {
            get
            {
                if (appsecret == null)
                {
                    appsecret = System.Configuration.ConfigurationManager.AppSettings["appsecret"];
                }
                return appsecret;
            }
        }

        private static string appKey = null;
        /// <summary>
        /// paysignkey，API密钥，在微信商户平台中“账户设置”--“账户安全”--“设置API密钥”，只能修改不能查看
        /// </summary>
        public static string AppKey
        {
            get
            {
                if (appKey == null)
                {
                    appKey = System.Configuration.ConfigurationManager.AppSettings["apikey"];
                }
                return appKey;
            }
        }

        /// <summary>
        /// 支付起点页面地址，也就是send.aspx页面完整地址
        /// 用于获取用户OpenId，支付的时候必须有用户OpenId，如果已知可以不用设置
        /// </summary>
        public static string SendUrl = System.Web.HttpUtility.UrlEncode(ConfigurationManager.AppSettings["SitePrefix"] + ConfigurationManager.AppSettings["WeiXinPaySendUrl"]); 

        /// <summary>
        /// 支付页面，请注意测试阶段设置授权目录，在微信公众平台中“微信支付”---“开发配置”--修改测试目录   
        /// 注意目录的层次，比如我的：http://域名/WXPay/
        /// </summary>
        //public static string PayUrl = ConfigurationManager.AppSettings["SitePrefix"] + ConfigurationManager.AppSettings["PayUrl"]; 
        
        /// <summary>
        ///  支付通知页面，请注意测试阶段设置授权目录，在微信公众平台中“微信支付”---“开发配置”--修改测试目录   
        /// 支付完成后的回调处理页面,替换成notify_url.asp所在路径
        /// </summary>
        public static string NotifyUrl = ConfigurationManager.AppSettings["SitePrefix"] + ConfigurationManager.AppSettings["WeiXinPayNotifyUrl"]; 
    }
}
