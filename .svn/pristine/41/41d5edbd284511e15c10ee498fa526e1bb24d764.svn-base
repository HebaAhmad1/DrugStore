using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreData.Models
{
    public class PharmacyNotes
    {
        public int Id { get; set; }
        [MaxLength(150)]
        public string Title { get; set; }
        [MaxLength(450)]
        public string Body { get; set; }
        public string PharmacyId { get; set; }
        public Pharmacy Pharmacy { get; set; }
    }
}
