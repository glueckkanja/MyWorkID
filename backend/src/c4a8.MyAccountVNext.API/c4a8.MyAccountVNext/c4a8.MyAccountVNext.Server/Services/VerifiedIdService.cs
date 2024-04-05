
using c4a8.MyAccountVNext.Server.Options;

namespace c4a8.MyAccountVNext.Server.Services
{
    public class VerifiedIdService
    {
        private readonly HttpClient _verifiedIdClient;
        private readonly VerifiedIdOptions _verifiedIdOptions;

        public VerifiedIdService(HttpClient verifiedIdClient, VerifiedIdOptions verifiedIdOptions)
        {
            _verifiedIdClient = verifiedIdClient;
            _verifiedIdOptions = verifiedIdOptions;
        }
        
    }
}
