using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Easy.Admin.Entities;
using Easy.Admin.Filters;
using Microsoft.AspNetCore.Mvc;
using NewLife.Data;
using NewLife.Reflection;
using XCode;
using XCode.Membership;

namespace Easy.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 基类Api
    /// </summary>
    public class EntityController<TEntity> : AdminControllerBase where TEntity : Entity<TEntity>, new()
    {
        /// <summary>
        /// 获取实体列表
        /// </summary>
        /// <param name="p">分页</param>
        /// <param name="key">搜索关键字</param>
        /// <returns></returns>
        [Route("Search")]
        [HttpPost]
        [ApiAuthorizeFilter(PermissionFlags.Detail)]
        [DisplayName("搜索{type}")]
        public virtual ApiResult<IList<TEntity>> Search([FromQuery]PageParameter p, [FromQuery]string key)
        {
            var exp = Entity<TEntity>.SearchWhereByKey(key);
            var list = Entity<TEntity>.FindAll(exp, p);
            return ApiResult.Ok(list, p);
        }

        /// <summary>
        /// 获取单对象
        /// </summary>
        /// <param name="id">对象id</param>
        /// <returns><see cref="T:TEntity" /></returns>
        [HttpGet("{id}")]
        [ApiAuthorizeFilter(PermissionFlags.Detail)]
        [DisplayName("查看{type}")]
        public virtual ApiResult<TEntity> Get([FromRoute]string id)
        {
            var entity = Entity<TEntity>.FindByKey(id);
            if (entity == null)
            {
                throw new ApiException(402, "未找到实体");
            }

            return ApiResult<TEntity>.Ok(entity);
        }

        /// <summary>
        /// 添加
        /// </summary>
        /// <param name="value">需要添加的对象</param>
        [HttpPost]
        [ApiAuthorizeFilter(PermissionFlags.Insert)]
        [DisplayName("添加{type}")]
        public virtual ApiResult Post([FromBody]TEntity value)
        {
            value.Insert();

            var key = Entity<TEntity>.Meta.Unique;

            var id = value[key].ToInt();
            return ApiResult.Ok(id);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="value">需要更新的对象</param>
        /// <returns></returns>
        [HttpPut]
        [ApiAuthorizeFilter(PermissionFlags.Update)]
        [DisplayName("更新{type}")]
        public virtual ApiResult Put([FromBody]TEntity value)
        {
            var key = Entity<TEntity>.Meta.Unique;
            var entity = Entity<TEntity>.FindByKey(value[key]);

            if (entity == null)
            {
                throw new ApiException(402, "未找到实体");
            }

            value.Update();
            var id = value[key];
            return ApiResult.Ok(id);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id">需要删除对象的id</param>
        [HttpDelete("{id}")]
        [ApiAuthorizeFilter(PermissionFlags.Delete)]
        [DisplayName("删除{type}")]
        public virtual ApiResult Delete([FromRoute]string id)
        {
            var entity = Entity<TEntity>.FindByKey(id);
            if (entity == null)
            {
                throw new ApiException(402, "未找到实体");
            }
            entity.Delete();

            return ApiResult.Ok(true);
        }

        /// <summary>
        /// 获取模型列信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetColumns")]
        [ApiAuthorizeFilter(PermissionFlags.Detail)]
        [DisplayName("列信息{type}")]
        public virtual ApiResult<List<TableColumnDto>> GetColumns()
        {
            var fields = Entity<TEntity>.Meta.AllFields.Where(
                w => !w.Type.IsGenericType
                && !typeof(EntityBase).IsAssignableFrom(w.Type)
                && !w.Type.IsInterface
                && !w.Type.IsArray
            ).ToList();
            var fieldDtoList = new List<TableColumnDto>(fields.Count);

            foreach (var field in fields)
            {
                var fieldDto = new TableColumnDto();
                fieldDto.Copy(field);

                // 处理成小驼峰命名规则
                // fieldDto.Name = fieldDto.Name.ToLower()[0].ToString() + fieldDto.Name.Substring(1);

                // 非数据库字段设置为只读
                if (field.Field == null)
                {
                    fieldDto.ReadOnly = true;
                }

                if (field.PrimaryKey)
                {
                    fieldDtoList.Insert(0, fieldDto);
                }
                else
                {
                    fieldDtoList.Add(fieldDto);
                }

            }

            return ApiResult.Ok(fieldDtoList);
        }
    }
}