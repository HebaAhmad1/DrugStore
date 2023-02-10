using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreData.Models
{
    public class Supplier
    {
        public int Id { get; set; }
        public int SupplierId { get; set; }
        [MaxLength(150)]
        public string Name { get; set; }
        public List<Pharmacy> Pharmacies { get; set; }
    }
}
