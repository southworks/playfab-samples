using System.Collections.Generic;
using System.Linq;

namespace FantasySoccer.Models.Authentication
{
    public class PlayFabClaims
    {
        public const string PlayFabId = "PlayFabId";
        public const string City = "city";
        public const string Country = "country";
        public const string Name = "name";
        public const string JobTitle = "jobTitle";
        public const string Budget = "Budget";

        public static List<string> Claims => typeof(PlayFabClaims).GetFields().Select(f => f.GetValue(f).ToString()).ToList();
    }
}
