using DrugStoreCore.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreCore.Dto
{
    public class CreateOrderDto
    {
        public int DrugId { get; set; }
        public int Quantity { get; set; }
        //public float UnitPrice { get; set; }
        //public float TotalPrice { get; set; }
    }
}
