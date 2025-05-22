using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartTask.Application.Wrappers
{
    public class Response<T>
    {
        public Response()
        {
        }
        public Response(T data, string message = null)
        {
            Succeeded = true;
            Message = message;
            Data = data;
        }
        public Response(string message)
        {
            Succeeded = false;
            Message = message;
        }
        public int StatusCode { get; set; }
        public string ResponseCode { get; set; }
        public bool Succeeded { get; set; }
        public string Message { get; set; }
        public List<string> Errors { get; set; }
        public T Data { get; set; }
    }

    public class MessageClass
    {
        public int StatusId { get; set; }
        public string StatusMessage { get; set; }
        public string RecordResponseObject { get; set; }
        //public T RecordResponseObject { get; set; }
        public bool IsSuccessful => StatusId == 1;
    }
}
