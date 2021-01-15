using System.Collections.Generic;
using System.Linq;

namespace PlayFabLoginWithB2C.Services
{
    public class PlayFabClaims
    {
        public const string PlayFabId = "PlayFabId";
        public const string City = "city";
        public const string Country = "country";
        public const string Name = "name";
        public const string JobTitle = "jobTitle";

        public static List<string> Claims
        {
            get
            {
                return typeof(PlayFabClaims).GetFields().Select(f => f.GetValue(f).ToString()).ToList();
            }
        }
    }
}
