using System;

namespace Assets.Scripts.Models.Responses
{
    [Serializable]
    public class ResponseWrapper<T>
    {
        public StatusCode statusCode;

        public string message;

        public T response;
    }
}
