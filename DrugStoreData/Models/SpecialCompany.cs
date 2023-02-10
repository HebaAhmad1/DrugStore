using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreData.Models
{
    [Index(nameof(CID), IsUnique = true)]
    public class SpecialCompany
    {
        public int Id { get; set; }
        public string CID { get; set; }
        public string Name { get; set; }
        public List<SpecialLocations> SpecialLocations { get; set; }
    }
}
