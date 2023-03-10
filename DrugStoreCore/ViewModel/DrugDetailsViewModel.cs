using DrugStoreCore.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreCore.ViewModel
{
    public class DrugDetailsViewModel
    {
        public int Id { get; set; }
        public int DrugId { get; set; }
        public string DrugName { get; set; }
        public int Stock { get; set; }
        public float SellPrice { get; set; }
        public DrugStatus DrugStatus { get; set; }
    }
}
