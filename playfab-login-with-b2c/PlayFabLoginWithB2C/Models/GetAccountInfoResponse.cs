using PlayFabLoginWithB2C.Models;
using System.Collections.Generic;

namespace PlayFabLoginWithB2C.Services
{
    public class GetAccountInfoResponse
    {
        public List<LinkedAccount> LinkedAccounts { get; set; }

        public string PlayFabEmail { get; set; }
    }
}