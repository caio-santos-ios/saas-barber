namespace api_barber.Interfaces
{
    public interface IAuthService
    {
        string GenerateJwtToken(string userId, string role, string barbershopId);
    }
}
