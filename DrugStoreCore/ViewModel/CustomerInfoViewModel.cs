using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreCore.ViewModel
{
    public class CustomerInfoViewModel
    {
        public int? AccountNumber { get; set; }
        public string PharmacyName { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
    }
}
