using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Easy.Admin.Entities;
using Easy.Admin.Filters;
using Microsoft.AspNetCore.Authorization;
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
        public virtual ApiResult<IList<TEntity>> Search([FromQuery] PageParameter p, [FromQuery] string key)
        {
            return GetList(p, key);
        }

        /// <summary>
        /// 搜索列表数据，方便重载方法使用
        /// </summary>
        /// <param name="p"></param>
        /// <param name="key"></param>
        /// <param name="exp"></param>
        /// <returns></returns>
        protected ApiResult<IList<TEntity>> GetList(PageParameter p, string key, WhereExpression exp = null)
        {
            exp &= Entity<TEntity>.SearchWhereByKey(key);
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
        public virtual ApiResult<TEntity> Get([FromRoute] string id)
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
        /// <param name="entity">需要添加的对象</param>
        [HttpPost]
        [ApiAuthorizeFilter(PermissionFlags.Insert)]
        [DisplayName("添加{type}")]
        public virtual ApiResult Post([FromBody] TEntity entity)
        {
            entity.Insert();

            var key = Entity<TEntity>.Meta.Unique;

            var id = entity[key].ToInt();
            return ApiResult.Ok(id);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity">需要更新的对象</param>
        /// <returns></returns>
        [HttpPut]
        [ApiAuthorizeFilter(PermissionFlags.Update)]
        [DisplayName("更新{type}")]
        public virtual ApiResult Put([FromBody] TEntity entity)
        {
            var key = Entity<TEntity>.Meta.Unique;
            var m = Entity<TEntity>.FindByKey(entity[key]);

            if (m == null)
            {
                throw new ApiException(402, "未找到实体");
            }

            entity.Update();
            var id = entity[key];
            return ApiResult.Ok(id);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id">需要删除对象的id</param>
        [HttpDelete("{id}")]
        [ApiAuthorizeFilter(PermissionFlags.Delete)]
        [DisplayName("删除{type}")]
        public virtual ApiResult Delete([FromRoute] string id)
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
    public static class StreamToMD5
    {
        public static string ToMD5(this Stream stream)
        {
            //计算文件的MD5值
            System.Security.Cryptography.MD5 calculator = System.Security.Cryptography.MD5.Create();
            Byte[] buffer = calculator.ComputeHash(stream);
            calculator.Clear();

            //将字节数组转换成十六进制的字符串形式
            StringBuilder stringBuilder = new StringBuilder();
            for (int i = 0; i < buffer.Length; i++)
            {
                stringBuilder.Append(buffer[i].ToString("x2"));
            }
            var hashMD5 = stringBuilder.ToString();

            stream.Position = 0; // 重置位置，避免下一个使用者使用报错

            return hashMD5;
        }
    }
}