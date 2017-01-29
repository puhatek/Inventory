using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Inventory.Models.Helpers
{
    public class DictionaryBinder : DefaultModelBinder
    {
        public override object BindModel(ControllerContext controllerContext, ModelBindingContext bindingContext)
        {

            var test = bindingContext.ModelState;
            test.Clear();
            test = bindingContext.ModelState;
            var dictionaryBindingContext = new ModelBindingContext()
            {
                ModelMetadata = ModelMetadataProviders.Current.GetMetadataForType(() => null, typeof(IDictionary<long, int>)),
                ModelName = "dataFromView", //The name(s) of the form elements you want going into the dictionary
                ModelState = bindingContext.ModelState,
                PropertyFilter = bindingContext.PropertyFilter,
                ValueProvider = bindingContext.ValueProvider
            };

            var model = base.BindModel(controllerContext, bindingContext);
            return model;
        }

    }
}