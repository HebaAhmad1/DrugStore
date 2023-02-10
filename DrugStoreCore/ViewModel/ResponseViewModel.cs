using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrugStoreCore.ViewModel
{
    public class ResponseViewModel<T>
    {
        public bool Status { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public ResponseViewModel(bool status, string message, T data)
        {
            Status = status;
            Message = message;
            Data = data;
        }
        public ResponseViewModel()
        {
        }
    }
}
