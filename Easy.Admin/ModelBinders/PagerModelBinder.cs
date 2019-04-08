using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NewLife.Data;

namespace Easy.Admin.ModelBinders
{
    // 如果是数据库实体模型绑定器，可能是需要根据id查到数据库的记录，再从请求参数填充数据，这需要自己创建对象,
    // 或者使用ComplexTypeModelBinder处理数据填充

    /// <summary>分页模型绑定器</summary>
    public class PagerModelBinder : IModelBinder
    {
        private readonly IDictionary<ModelMetadata, IModelBinder> _propertyBinders;
        private readonly ILoggerFactory _loggerFactory;

        public PagerModelBinder(IDictionary<ModelMetadata, IModelBinder> propertyBinders, ILoggerFactory loggerFactory)
        {
            _propertyBinders = propertyBinders ?? throw new ArgumentNullException(nameof(propertyBinders));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        /// <summary>创建模型。对于有Key的请求，使用FindByKeyForEdit方法先查出来数据，而不是直接反射实例化实体对象</summary>
        /// <param name="bindingContext"></param>
        /// <returns></returns>
        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var modelType = bindingContext.ModelType;

            if (modelType == typeof(PageParameter))
            {
                var pager = new PageParameter
                {
                    PageIndex = 1,
                    PageSize = 20
                };

                var complexTypeModelBinder = new ComplexTypeModelBinder(_propertyBinders, _loggerFactory);

                bindingContext.Model = pager;

                await complexTypeModelBinder.BindModelAsync(bindingContext);

                bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            }
        }
    }

    /// <summary>分页模型绑定器提供者</summary>
    public class PagerModelBinderProvider : IModelBinderProvider
    {
        /// <summary>获取绑定器</summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            var modelType = context.Metadata.ModelType;
            if (modelType == typeof(PageParameter))
            {
                var propertyBinders = new Dictionary<ModelMetadata, IModelBinder>();
                foreach (var property in context.Metadata.Properties)
                {
                    propertyBinders.Add(property, context.CreateBinder(property));
                }

                var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
                return new PagerModelBinder(propertyBinders, loggerFactory);
            }

            return null;
        }
    }
}
