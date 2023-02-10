using System;

namespace DrugStoreInfrastructure.PaginationHelpers
{
   public static class PagingHelper
    {
        public static int GetSkipValue(this PagingWithQueryDto pagination, int dataCount)
        {
            //To Check Page Greater Than 1 And Less Than Pagination Pages Count 
            if(pagination.Page < 1 || pagination.Page > pagination.GetPages(dataCount))
            {
                pagination.Page = 1;
            }
           return (pagination.Page - 1) * pagination.PerPage;
        }

        public static int GetPages(this PagingWithQueryDto pagination , int dataCount)
        {
            //To Check PerPage Less Than 1 Make It Equal 10
            if (pagination.PerPage < 1 || pagination.PerPage > 100)
            {
                pagination.PerPage = 15;
            }
            return Convert.ToInt32(Math.Ceiling(dataCount / (float)pagination.PerPage));
        }
    }
}
