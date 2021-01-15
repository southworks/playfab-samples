namespace FantasySoccer.Models.Responses
{
    public class ResponseWrapper<T> : BaseResponseWrapper
    {
        public T Response { get; set; }
    }
}
