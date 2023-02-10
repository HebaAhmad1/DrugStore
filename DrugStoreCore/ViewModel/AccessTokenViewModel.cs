using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreCore.ViewModel
{
    public class AccessTokenViewModel
    {
        public string BearerToken { get; set; }
        public DateTime ExpiringDate { get; set; }
    }
}
