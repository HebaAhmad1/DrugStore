using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreCore.ViewModel
{
    public class ValidAndInvalidOrdersViewModel
    {
        public List<OrderViewModel> validOrders { get; set; }
        public List<OrderViewModel> InvalidOrders { get; set; }
    }
}
