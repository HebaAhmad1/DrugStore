using DrugStoreCore.Enums;
using System;

namespace DrugStoreCore.ViewModel
{
    public class GetOrdersDataTable
    {
        public DateTime? CreatedAt { get; set; }

        public string CreatedAtString { get; set; }

        public int OrderId { get; set; }

        public int PharmacyOrderId { get; set; }
        public int? DrugId { get; set; }
        public string DrugName { get; set; }
        public int Quantity { get; set; }
        public float PricePerUnit { get; set; }
        public float TotalPrice { get; set; }
        public float? Stock { get; set; }



        public string PharmacyName { get; set; }

        public string PharmacyId { get; set; }  

        public string Action { get; set; }

        public float TotalPricePharmacyOrders { get; set; }

        public float ArchivedTotalPricePharmacyOrders { get; set; }

        public Status Status { get; set; }
    }
}
