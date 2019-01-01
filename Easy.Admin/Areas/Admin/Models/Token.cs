using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Bson;

namespace Easy.Admin.Areas.Admin.Models
{
    public class AccessToken
    {
        public ObjectId Id { get; set; }
        public string Token { get ; set ; }
    }
}
