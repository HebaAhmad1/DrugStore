using DrugStoreCore.Enums;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreData.Models
{
    public class Pharmacy : IdentityUser
    {
        [MaxLength(150)]
        public string PharmacyName { get; set; }
        public int? AccountNumber { get; set; }
        [MaxLength(100)]
        public string PharmacyRole{ get; set; }
        [MaxLength(150)]
        public string Address { get; set; }
        [MaxLength(150)]
        public string Longitude { get; set; }
        [MaxLength(150)]
        public string Latitude { get; set; }
        public int? SupplierId { get; set; }
        public PharmacyStatus PharmacyStatus { get; set; }
        public Supplier Supplier { get; set; }
        public List<PharmacyNotes> PharmacyNotes { get; set; }
    }
}
