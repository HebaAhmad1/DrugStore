using DrugStoreCore.Enums;
using System;

namespace DrugStoreInfrastructure.PaginationHelpers
{
    public class Query
    {
        //public string DrugName { get; set; }
        //public string PharmacyName { get; set; }
        public int DrugId { get; set; }
        public string PharmacyId { get; set; }
        public float MinTotalPrice { get; set; }
        public float MaxTotalPrice { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string MonthName { get; set; }
        public string PeriodOfTime { get; set; }
        public Status Status { get; set; }
    }
}
