using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;


namespace Inventory.Models.Helpers
{
    public class MessageBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {
            HttpRequestBase request = controllerContext.HttpContext.Request;
            
            var controller = controllerContext.Controller;

            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(controllerContext, "_MessageContainerText");
                var viewContext = new ViewContext(controllerContext, viewResult.View, controller.ViewData, controller.TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(controller.ControllerContext, viewResult.View);
                
                return new Message
                {
                    MessageText = sw.GetStringBuilder().ToString(),
                    Recipient = request.Form.Get("Recipient"),
                    SupplierId =  Convert.ToInt32(request.Form.Get("SupplierId"))
                };
            }
        }                   
    }
}