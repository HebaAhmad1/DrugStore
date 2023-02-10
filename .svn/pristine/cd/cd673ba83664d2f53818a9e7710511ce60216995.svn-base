using DrugStoreCore.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreData.Models
{
    public class Drug
    {
        public int Id { get; set; }
        public int? DrugId { get; set; }
        [MaxLength(150)]
        public string DrugName { get; set; }
        [MaxLength(150)]
        public string Unit { get; set; }
        public float? SellPrice { get; set; }
        public float? Stock { get; set; }
        public DrugStatus DrugStatus { get; set; }
    }
}
