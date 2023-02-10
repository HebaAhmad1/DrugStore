using DrugStoreCore.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreData.Models
{
    public class OrderDetails
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public float UnitPrice { get; set; }
        public int DrugId { get; set; }
        public Drug Drug { get; set; }
        public int PharmacyOrdersId { get; set; }
        public PharmacyOrders PharmacyOrders { get; set; }
        public Status Status { get; set; }
        public bool IsHanging { get; set; }

    }
}
