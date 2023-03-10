using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreCore.ViewModel
{
    public class PharmacyInfoWithCoordinatesViewModel
    {
        public string PharmacyId { get; set; }
        public string PharmacyName { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public string SupplierName { get; set; }
        public int? Status { get; set; }
        public List<PharmacyNotesViewModel> PharmacyNotes { get; set; }
    }
}
