using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Linq.Expressions;

namespace Ddi.Registry.Web.Extensions
{
	public static class HtmlHelpers
	{
		public static MvcHtmlString LabelWithTooltip<TModel, TValue>(this HtmlHelper<TModel> helper, Expression<Func<TModel, TValue>> expression)
		{
			var metaData = ModelMetadata.FromLambdaExpression(expression, helper.ViewData);

			string htmlFieldName = ExpressionHelper.GetExpressionText(expression);
			string labelText = metaData.DisplayName ?? metaData.PropertyName ?? htmlFieldName.Split('.').Last();

			if (String.IsNullOrEmpty(labelText))
				return MvcHtmlString.Empty;

            /*
			var label = new TagBuilder("label");
			label.Attributes.Add("for", helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName));

			if (!string.IsNullOrEmpty(metaData.Description))
				label.Attributes.Add("title", metaData.Description);

			label.SetInnerText(labelText);
			return MvcHtmlString.Create(label.ToString());
             * */

            string label = "<label ";
            label += "for=\"" + helper.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldId(htmlFieldName) + "\"";
            if (!string.IsNullOrEmpty(metaData.Description))
                label+=" title=\"" + metaData.Description + "\"";

            label += ">" + labelText + "</label>";
            return MvcHtmlString.Create(label);

		}

	}
}