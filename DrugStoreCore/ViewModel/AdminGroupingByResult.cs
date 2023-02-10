using DrugStoreData.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreCore.ViewModel
{
    public class AdminGroupingByResult
    {
        public AdminKey Key { get; set; }
        public List<CurrentAndArchivedOrdersViewModel> OrdersDetails{ get; set; }
    }
}
