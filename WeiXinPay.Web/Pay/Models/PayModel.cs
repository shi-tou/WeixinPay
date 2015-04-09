using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace WeiXinPay.Lib
{
    //作用：微信支付相关支付信息实体类，支付信息通过该类封装，微信支付版本V3.3.7
    public class PayModel
    {
        #region 微商家配置
        public string AppId { set; get; }
        public string AppSecret { set; get; }
        public string MchId { set; get; }
        public string AppKey { set; get; }
        #endregion

        #region 其他信息
        /// <summary>
        /// 商户自己的订单号（必填）
        /// </summary>
        public string OrderSN { get; set; }
        /// <summary>
        /// 订单支付金额单位为分（必填）
        /// </summary>
        public int TotalFee { get; set; }
        /// <summary>
        /// 商品信息（必填，长度不能大于127字符）
        /// </summary>
        public string Body { get; set; }
        /// <summary>
        /// 支付用户微信OpenId（必填）
        /// </summary>
        public string OpenId { get; set; }
        /// <summary>
        /// 用户自定义参数原样返回，不能有中文不然调用Notify页面会有错误。（长度不能大于127字符）
        /// </summary>
        public string Attach { get; set; }
        #endregion

        /// <summary>
        /// 重写ToString函数，获取跳转到支付页面的url其中附带参数
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("WeixinPay.aspx?showwxpaytitle=1"); //?showwxpaytitle=1显示标题"微信安全支付"
            sb.AppendFormat("&AppId={0}", AppId);
            sb.AppendFormat("&AppSecret={0}", AppSecret);
            sb.AppendFormat("&AppKey={0}", AppKey);
            sb.AppendFormat("&MchId={0}", MchId);

            sb.AppendFormat("&OrderSN={0}", OrderSN);
            sb.AppendFormat("&Body={0}", Body);
            sb.AppendFormat("&TotalFee={0}", TotalFee);
            sb.AppendFormat("&UserOpenId={0}", OpenId);
            sb.AppendFormat("&Attach={0}", Attach);

            return sb.ToString();
        }
    }
}
