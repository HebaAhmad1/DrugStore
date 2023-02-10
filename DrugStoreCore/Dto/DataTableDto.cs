using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DrugStoreCore.Dto
{
    public class DataTableDto
    {
        public int length { get; set; }
        public int start { get; set; }
        public DataTableFilterDto DataTableFilterDto { get; set; }
    }
}
