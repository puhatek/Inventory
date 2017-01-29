using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Models
{
    [AttributeUsage(AttributeTargets.Method,AllowMultiple=false,Inherited=true)]
    public class MultipleButtonAttribute : ActionNameSelectorAttribute
    {
        public string ButtonName { get; set; }
        public string ButtonValue { get; set; }

        public override bool IsValidName(ControllerContext controllerContext, string actionName, System.Reflection.MethodInfo methodInfo)
        {
            string hmm = controllerContext.HttpContext.Request[ButtonName];
            return controllerContext.HttpContext.Request[ButtonName] != null && controllerContext.HttpContext.Request[ButtonName] == ButtonValue;
        }
    }
}