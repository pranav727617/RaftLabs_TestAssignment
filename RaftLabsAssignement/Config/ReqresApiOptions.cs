using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaftLabsAssignement.Config
{
    public class ReqresApiOptions
    {
        public string BaseUrl { get; set; } = "https://reqres.in/api/";
        public int CacheExpirationSeconds { get; set; } = 300; 
    }
}
