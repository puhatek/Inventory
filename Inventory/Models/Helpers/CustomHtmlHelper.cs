using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections;
using System.Web.Routing;
using System.Linq.Expressions;

namespace Inventory.Models.Helpers
{
    public static class CustomHtmlHelpers
    {
        private static List<string> headerNames = new List<string> { "AssetName", "Qty", "UM" };

        public static String EncodeMultiLineText(this HtmlHelper helper, string text)
        {
            if (String.IsNullOrEmpty(text))
            {
                return String.Empty;
            }

            //var result = string.Join(
            //"<br/>",
            //text.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Select(x => HttpUtility.HtmlEncode(x))
            //);

            return Regex.Replace(helper.Encode(text), Environment.NewLine, "<br/>");
            // return MvcHtmlString.Create(result);
        }

        #region table_1

        public static MvcHtmlString Table<TModel, TProperty>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TProperty>> expression, object attributes)
        {
            if (expression == null)
            {
                return MvcHtmlString.Empty;
            }
            return BuildTable(helper, ModelMetadata.FromLambdaExpression(expression, helper.ViewData), ExpressionHelper.GetExpressionText(expression), new RouteValueDictionary(attributes));
        }

        private static MvcHtmlString BuildTable(HtmlHelper helper, ModelMetadata modelMetadata, string name, IDictionary<string, object> attributes)
        {
            Message m = (Message)helper.ViewData.Model;
            StringBuilder sb = m == null ? new StringBuilder() : new StringBuilder(m.MessageText);
            BuildTableHeader(sb);
            BuildTableRow(sb);

            string value = string.Empty;

            if (modelMetadata.Model != null)
            {
                value = modelMetadata.Model.ToString();
            }
            else
            {
                value = String.Empty;
            }

            TagBuilder builder = new TagBuilder("table");
            builder.MergeAttributes(attributes);
            builder.MergeAttribute("name", name, true);
            builder.MergeAttribute("id", name);
            builder.InnerHtml = sb.ToString();

            return MvcHtmlString.Create(builder.ToString(TagRenderMode.Normal));
        }

        private static void BuildTableRow(StringBuilder sb)
        {
            List<OrderItem> orderItems = (List<OrderItem>)HttpContext.Current.Session["OrderDetailsBySupplier"];

            if (orderItems != null)
            {
                foreach (var item in orderItems)
                {
                    sb.AppendLine("\t<tr>");
                    sb.AppendFormat("\t\t<td>{0}</td>\n", item.AssetName);
                    sb.AppendFormat("\t\t<td>{0}</td>\n", item.Qty);
                    sb.AppendFormat("\t\t<td>{0}</td>\n", item.UM);
                    sb.AppendLine("\t</tr>");
                }
            }
        }

        private static void BuildTableHeader(StringBuilder sb)
        {
            sb.AppendLine("\t<tr>");
            foreach (var item in headerNames)
            {
                sb.AppendFormat("\t\t<th>{0}</th>\n", item);
            }
            sb.AppendLine("\t</tr>");
        }
        #endregion
    }







}