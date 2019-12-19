using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NewLife.Reflection;
using Newtonsoft.Json.Linq;
using XCode;

namespace Easy.Admin.ModelBinders
{
    public class EntityModelBinder : IModelBinder
    {
        public EntityModelBinder(IDictionary<ModelMetadata, IModelBinder> propertyBinders, ILoggerFactory loggerFactory)
        {
            _propertyBinders = propertyBinders ?? throw new ArgumentNullException(nameof(propertyBinders));
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));

        }

        private readonly IDictionary<ModelMetadata, IModelBinder> _propertyBinders;
        private readonly ILoggerFactory _loggerFactory;
        private JObject _jObj;

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var modelType = bindingContext.ModelType;
            if (!modelType.As<IEntity>()) return Task.CompletedTask;

            var fact = EntityFactory.CreateOperate(modelType);
            if (fact == null) return Task.CompletedTask;

            SetRequestValue(bindingContext.HttpContext.Request);

            var pks = fact.Table.PrimaryKeys;
            var uk = fact.Unique;

            IEntity entity = null;
            if (uk != null)
            {
                var ukValue = GetRequestValue(uk.Name);
                // 查询实体对象用于编辑
                if (ukValue != null)
                    entity =
                        fact.FindByKeyForEdit(ukValue); // 从session取回来的实体全部被设置了脏属性，每次保存所有数据，因此从数据查找
            }
            else if (pks.Length > 0)
            {
                // 查询实体对象用于编辑
                var exp = new WhereExpression();
                foreach (var item in pks)
                {
                    exp &= item.Equal(GetRequestValue(item.Name).ChangeType(item.Type));
                }

                entity = fact.Find(exp);
            }

            if (entity == null)
            {
                entity = fact.Create();
            }

            // 填充值
            foreach (var item in fact.Fields)
            {
                var value = GetRequestValue(item.Name);
                if (value == null)
                {
                    continue;
                }

                if (item.Field != null) entity.SetItem(item.Name, value);
            }

            bindingContext.Result = ModelBindingResult.Success(entity);

            // // 为Model赋值，为下面BindProperty方法做准备
            // bindingContext.Model = bindingContext.Result.Model;

            // // 填充Model
            // await BindProperty(bindingContext);

            return Task.CompletedTask;
        }

        private async Task BindProperty(ModelBindingContext bindingContext)
        {
            // 使用复杂类型模型绑定器ComplexTypeModelBinder
            var complexTypeModelBinder = new ComplexTypeModelBinder(_propertyBinders, _loggerFactory);

            await complexTypeModelBinder.BindModelAsync(bindingContext);
        }

        /// <summary>
        /// 从请求获取值，并解析成JObject
        /// </summary>
        private void SetRequestValue(HttpRequest req)
        {
            // 允许同步IO
            var ft = req.HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpBodyControlFeature>();
            if (ft != null) ft.AllowSynchronousIO = true;

            var stream = new MemoryStream();
            req.Body.CopyTo(stream);

            stream.Position = 0;
            var str = stream.ToStr();

            stream.Position = 0;
            req.Body = stream;

            _jObj = JObject.Parse(str);
        }

        /// <summary>
        /// 从请求里获取值
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private string GetRequestValue(string key)
        {
            var value = _jObj.Value<string>(key);
            return value;
        }
    }


    /// <summary>实体模型绑定器提供者，为所有XCode实体类提供实体模型绑定器</summary>
    public class EntityModelBinderProvider : IModelBinderProvider
    {
        /// <summary>
        /// 获取绑定器
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.Metadata.ModelType.As<IEntity>()) return null;
            var propertyBinders = new Dictionary<ModelMetadata, IModelBinder>();
            foreach (var property in context.Metadata.Properties)
            {
                propertyBinders.Add(property, context.CreateBinder(property));
            }

            var loggerFactory = context.Services.GetRequiredService<ILoggerFactory>();
            return new EntityModelBinder(propertyBinders, loggerFactory);
        }
    }
}