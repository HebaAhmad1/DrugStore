using DrugStoreCore.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreData.Models
{
    public class PharmacyOrders
    {
        public int Id { get; set; }
        public string PharmacyId { get; set; }
        public Pharmacy Pharmacy { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<OrderDetails> Orders { get; set; }
        public float OrdersTotalPrice { get; set; }
        //public bool IsDelete { get; set; }
        //public Status Status { get; set; }
        public PharmacyOrders()
        {
            CreatedAt = DateTime.Now;
        }
    }


}
