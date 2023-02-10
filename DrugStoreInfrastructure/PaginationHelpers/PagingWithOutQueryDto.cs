using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreInfrastructure.PaginationHelpers
{
    public class PagingWithOutQueryDto
    {
        public int PerPage { get; set; }
        public int Page { get; set; }
    }
}
