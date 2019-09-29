using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using XCode.Membership;

namespace Easy.Admin.Areas.Admin.ResponseParams
{
    /// <summary>
    /// 
    /// </summary>
    public class ResponseMenu
    {
        public ResponseMenu(IMenu menu)
        {
            Menu = menu;
        }

        /// <summary>编号</summary>
        public int? ID => Menu?.ID;

        /// <summary>名称</summary>
        public string Name => Menu?.Name;

        /// <summary>显示名</summary>
        public string DisplayName => Menu?.DisplayName;

        /// <summary>父编号</summary>
        public int? ParentID => Menu?.ParentID;

        /// <summary>可选权限子项</summary>
        public Dictionary<int, string> Permissions => Menu?.Permissions;

        /// <summary>子节点</summary>
        public List<ResponseMenu> Childs => Menu?.Childs.Select(s => new ResponseMenu(s)).ToList();

        [JsonIgnore]
        public IMenu Menu;
    }
}