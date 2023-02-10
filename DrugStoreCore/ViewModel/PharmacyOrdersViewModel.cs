using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreCore.ViewModel
{
    public class PharmacyOrdersViewModel
    {
        public DateTime CreatedAt { get; set; }
        public PharmacyViewModel Pharmacy { get; set; }
        public float OrdersTotalPrice { get; set; }
    }
}
