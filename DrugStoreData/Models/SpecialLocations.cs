using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreData.Models
{
    public class SpecialLocations
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string LocationName { get; set; }
        public string Latitude { get; set; }
        public string Longitude { get; set; }
        public string CID { get; set; }
        public SpecialCompany SpecialCompany { get; set; }
    }
}
