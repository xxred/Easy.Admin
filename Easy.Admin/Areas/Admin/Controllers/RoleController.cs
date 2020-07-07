using System;
using System.Collections.Generic;
using System.ComponentModel;
using Easy.Admin.Areas.Admin.Models;
using Easy.Admin.Entities;
using Microsoft.AspNetCore.Mvc;
using NewLife;
using NewLife.Reflection;
using Newtonsoft.Json.Linq;
using XCode;
using XCode.Membership;

namespace Easy.Admin.Areas.Admin.Controllers
{
    /// <summary>
    /// 角色
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [DisplayName("角色")]
    public class RoleController : EntityController<ApplicationRole>
    {
        /// <summary>
        /// 更新
        /// </summary>
        /// <param name="entity"></param>
        /// <remarks>所有权限子项传值方式
        /// {
        ///     "id": 0,
        ///     "name": "string",
        ///     "p24": false
        ///     "pf24_1": false
        ///     "pf24_2": false
        ///     "pf24_4": false
        ///     "pf24_8": false
        /// }
        /// 相当于将这些子项当做实体的属性
        /// </remarks>
        /// <returns></returns>
        public override ApiResult<string> Put(ApplicationRole entity)
        {
            var model = ApplicationRole.FindByID(entity.ID);
            // 反射获取脏属性，得到修改的属性集合
            var dirtys = entity.GetValue("Dirtys") as DirtyCollection;

            foreach (var item in dirtys)
            {
                if (ApplicationRole._.ID.Name == item)
                {
                    continue;
                }

                model.SetItem(item, entity[item]);
            }
            entity = model;

            // 保存权限项
            var menus = Menu.Root.AllChilds;
            //var pfs = EnumHelper.GetDescriptions<PermissionFlags>().Where(e => e.Key > PermissionFlags.None);
            var dels = new List<Int32>();

            var body = GetBody();

            // 遍历所有权限资源
            foreach (var item in menus)
            {
                var ps = "p" + item.ID;
                // 是否授权该项
                var has = GetBool(ps,body[ps]?.Value<string>());
                if (!has)
                    dels.Add(item.ID);
                else
                {
                    // 遍历所有权限子项
                    var any = false;
                    foreach (var pf in item.Permissions)
                    {
                        var pfs = "pf" + item.ID + "_" + pf.Key;
                        var has2 = GetBool(pfs, body[pfs]?.Value<string>());

                        if (has2)
                            entity.Set(item.ID, (PermissionFlags)pf.Key);
                        else
                            entity.Reset(item.ID, (PermissionFlags)pf.Key);
                        any |= has2;
                    }
                    // 如果原来没有权限，这是首次授权，且右边没有勾选任何子项，则授权全部
                    //if (!any & !entity.Has(item.ID)) entity.Set(item.ID);
                }
            }
            // 删除已经被放弃权限的项
            foreach (var item in dels)
            {
                if (entity.Has(item)) entity.Permissions.Remove(item);
            }

            // 将权限项保存为字符串
            entity.Update();

            return ApiResult.Ok(entity.ID.ToString());
        }

        bool GetBool(string name, string v)
        {
            if (v.IsNullOrEmpty()) return false;

            v = v.Split(",")[0];

            if (!v.EqualIgnoreCase("true", "false")) throw new XException("非法布尔值Request[{0}]={1}", name, v);

            return v.ToBoolean();
        }

        JObject GetBody()
        {
            var stream = Request.Body;
            stream.Position = 0;
            var str = stream.ToStr();
            var jObject = JObject.Parse(str);
            return jObject;
        }
    }
}