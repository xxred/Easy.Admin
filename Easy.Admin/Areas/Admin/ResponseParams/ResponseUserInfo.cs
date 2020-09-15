using System;
using System.Collections.Generic;
using System.Linq;
using Easy.Admin.Areas.Admin.RequestParams;
using NewLife.Reflection;
using Newtonsoft.Json;
using XCode.Membership;

namespace Easy.Admin.Areas.Admin.ResponseParams
{
    public class ResponseUserInfo : RequestUserInfo
    {
        /// <summary>
        /// 角色。主要角色
        /// </summary>
        public string RoleID { get; set; }

        /// <summary>
        /// 角色组。次要角色集合
        /// </summary>
        public string RoleIDs { get; set; }

        /// <summary>
        /// 主要角色名
        /// </summary>
        public string RoleName { get; set; }

        private Role[] _roles;
        /// <summary>
        /// 角色组
        /// </summary>
        public Role[] Roles
        {
            get => _roles;
            set => _roles = value;
        }

        public void SetRoles(IRole[] roles)
        {
            Roles = roles?.Select(s =>
            {
                var r = new Role();
                r.Copy(s);
                return r;
            }).ToArray();
        }

        /// <summary>
        /// 角色
        /// </summary>
        public class Role : IRole
        {
            /// <summary>
            /// 角色ID
            /// </summary>
            public int ID { get; set; }

            /// <summary>
            /// 角色名
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// 是否启用
            /// </summary>
            public bool Enable { get; set; }

            /// <summary>
            /// 是否系统角色
            /// </summary>
            public bool IsSystem { get; set; }

            /// <summary>
            /// 权限
            /// </summary>
            public string Permission { get; set; }

            #region 忽略

            public bool Has(int resid, PermissionFlags flag = PermissionFlags.None)
            {
                throw new NotImplementedException();
            }

            public PermissionFlags Get(int resid)
            {
                throw new NotImplementedException();
            }

            public void Set(int resid, PermissionFlags flag = PermissionFlags.Detail)
            {
                throw new NotImplementedException();
            }

            public void Reset(int resid, PermissionFlags flag)
            {
                throw new NotImplementedException();
            }

            public IRole FindByID(int id)
            {
                throw new NotImplementedException();
            }

            public IRole GetOrAdd(string name)
            {
                throw new NotImplementedException();
            }

            public int Save()
            {
                throw new NotImplementedException();
            }

            [JsonIgnore] public IDictionary<int, PermissionFlags> Permissions { get; }

            [JsonIgnore] public int[] Resources { get; }

            [JsonIgnore] public int Ex1 { get; set; }

            [JsonIgnore] public int Ex2 { get; set; }

            [JsonIgnore] public double Ex3 { get; set; }

            [JsonIgnore] public string Ex4 { get; set; }

            [JsonIgnore] public string Ex5 { get; set; }

            [JsonIgnore] public string Ex6 { get; set; }

            [JsonIgnore] public string CreateUser { get; set; }

            [JsonIgnore] public int CreateUserID { get; set; }

            [JsonIgnore] public string CreateIP { get; set; }

            [JsonIgnore] public DateTime CreateTime { get; set; }

            [JsonIgnore] public string UpdateUser { get; set; }

            [JsonIgnore] public int UpdateUserID { get; set; }

            [JsonIgnore] public string UpdateIP { get; set; }

            [JsonIgnore] public DateTime UpdateTime { get; set; }

            [JsonIgnore] public string Remark { get; set; }

            [JsonIgnore] public object this[string name] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            #endregion
        }
    }
}
