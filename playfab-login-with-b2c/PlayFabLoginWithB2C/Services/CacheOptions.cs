namespace PlayFabLoginWithB2C.Services
{
    public class CacheOptions
    {
        public int SlidingExpiration { get; set; }
        public int AbsoluteExpirationRelativeToNow { get; set; }
    }
}