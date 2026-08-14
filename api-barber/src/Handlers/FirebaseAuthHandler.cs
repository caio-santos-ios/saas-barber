using FirebaseAdmin.Auth;
using System.Threading.Tasks;
namespace api_barber.Handlers
{
    public class FirebaseAuthHandler
    {
        public async Task<FirebaseToken> VerifyIdTokenAsync(string idToken)
        {
            return await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(idToken);
        }
    }
}

