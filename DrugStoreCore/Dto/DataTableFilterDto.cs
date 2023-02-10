using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreCore.Dto
{
    public class DataTableFilterDto
    {
        public int DrugId { get; set; }
        public string PharmacyId { get; set; }
        public float? MinTotalPrice { get; set; }
        public float? MaxTotalPrice { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
    }
}


