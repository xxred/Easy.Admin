using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using NewLife.Data;
using Easy.Admin.Entities;
using XCode;
namespace Easy.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 基类Api
    /// </summary>
    // [ApiAuthorize]
    public class EntityController<TEntity> : BaseController where TEntity : Entity<TEntity>, new()
    {
        /// <summary>
        /// 获取实体列表
        /// </summary>
        /// <param name="p">分页</param>
        /// <param name="search">条件</param>
        /// <returns></returns>
        [Route("Search")]
        [HttpPost]
        public virtual ApiResult Search([FromQuery]PageParameter p, [FromBody]Search search)
        {
            var exp = search?.Expressions<TEntity>();
            var list = Entity<TEntity>.FindAll(exp, p);
            return ApiResult.Ok(list, p);
        }

        /// <summary>
        /// 获取单对象
        /// </summary>
        /// <param name="id">对象id</param>
        /// <returns><see cref="T:TEntity" /></returns>
        [HttpGet("{id}")]
        public virtual ApiResult Get([FromRoute]int id)
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
        public virtual ApiResult Post(TEntity value)
        {
            value.Insert();
            var id = value["ID"].ToInt();
            return ApiResult.Ok(id);
        }

        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="value">需要删除的对象</param>
        /// <returns></returns>
        [HttpPut]
        public virtual ApiResult Put(TEntity value)
        {
            value.Update();
            var id = value["ID"].ToInt();
            return ApiResult.Ok(id);
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="id">需要删除对象的id</param>
        [HttpDelete("{id}")]
        public virtual ApiResult Delete(int id)
        {
            var entity = Entity<TEntity>.FindByKey(id);
            if (entity == null)
            {
                throw new ApiException(402, "未找到实体");
            }
            entity.Delete();

            return ApiResult<Boolean>.Ok(true);
        }
    }
}