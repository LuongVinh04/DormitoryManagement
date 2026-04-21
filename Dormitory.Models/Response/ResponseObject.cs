using Microsoft.AspNetCore.Http;

namespace Dormitory.Models.Response
{
    public class ResponseObject<T>
    {
        public int Status { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public int TotalRecord { get; set; }
        //public DataResponseSearchHistory? DataResponseSearch{ get; set; }
        public ResponseObject() { }
        public ResponseObject(int status, string message, T data, int totalRecord = 0)
        {
            Status = status;
            Message = message;
            Data = data;
            TotalRecord = totalRecord;
        }
        public ResponseObject<T> ResponseSuccess(string message, T data)
        {
            return new ResponseObject<T>(StatusCodes.Status200OK, message, data);
        }
        public ResponseObject<T> ResponseSuccess(string message, T data, int totalRecord)
        {
            return new ResponseObject<T>(StatusCodes.Status200OK, message, data, totalRecord);
        }
        public ResponseObject<T> ResponseError(int status, string message, T data)
        {
            return new ResponseObject<T>(status, message, data);
        }
    }
}
