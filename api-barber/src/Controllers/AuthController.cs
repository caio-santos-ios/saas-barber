using api_barber.Handlers;
using api_barber.Interfaces;
using api_barber.Models;
using api_barber.Models.Enums;
using api_barber.Requests.Auth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace api_barber.Controllers
{
    [ApiController]
    [Route("auth")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly FirebaseAuthHandler _firebaseAuthHandler;
        private readonly IBaseRepository<User> _userRepo;
        private readonly IBaseRepository<Barbershop> _barbershopRepo;
        private readonly IAsaasService _asaasService;

        public AuthController(
            IAuthService authService,
            FirebaseAuthHandler firebaseAuthHandler,
            IBaseRepository<User> userRepo,
            IBaseRepository<Barbershop> barbershopRepo,
            IAsaasService asaasService)
        {
            _authService = authService;
            _firebaseAuthHandler = firebaseAuthHandler;
            _userRepo = userRepo;
            _barbershopRepo = barbershopRepo;
            _asaasService = asaasService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var decodedToken = await _firebaseAuthHandler.VerifyIdTokenAsync(request.FirebaseToken);
            var firebaseUid = decodedToken.Uid;

            var allUsers = await _userRepo.GetAllAsync(string.Empty);
            var user = allUsers.FirstOrDefault(u => u.FirebaseUid == firebaseUid);

            if (user == null)
            {
                return Unauthorized();
            }

            var jwt = _authService.GenerateJwtToken(user.Id, user.Role.ToString(), user.BarbershopId);

            return Ok(new AuthResponse
            {
                Token = jwt,
                Role = user.Role.ToString(),
                BarbershopId = user.BarbershopId
            });
        }

        [HttpPost("customers/register")]
        public async Task<IActionResult> RegisterCustomer([FromBody] CreateCustomerRequest request)
        {
            var user = new User
            {
                Name = request.Name,
                Email = request.Email,
                WhatsApp = request.WhatsApp,
                Password = request.Password, 
                FirebaseUid = request.FirebaseUid,
                Role = RoleUserEnum.Customer,
                BarbershopId = request.BarbershopId,
                Active = true
            };

            await _userRepo.CreateAsync(user);

            var jwt = _authService.GenerateJwtToken(user.Id, user.Role.ToString(), user.BarbershopId);

            return Ok(new AuthResponse
            {
                Token = jwt,
                Role = user.Role.ToString(),
                BarbershopId = user.BarbershopId
            });
        }

        [HttpPost("admins/register")]
        public async Task<IActionResult> RegisterAdmin([FromBody] CreateAdminRequest request)
        {
            Enum.TryParse<TypePersonEnum>(request.TypePerson, out var typePerson);

            // Integração Asaas
            var asaasCustomerId = await _asaasService.CreateCustomerAsync(request.BarbershopName, request.Document, request.Email);
            
            // Valor fixo simbólico para criação do plano via Asaas (ex: 99.90 plano default)
            var planIdDefault = "plano_gold";
            var subscriptionId = await _asaasService.CreateSubscriptionAsync(asaasCustomerId, planIdDefault, 99.90m);

            var barbershop = new Barbershop
            {
                Name = request.BarbershopName,
                Email = request.Email,
                Document = request.Document,
                TypePerson = typePerson,
                SubscriptionStatus = SubscriptionStatusEnum.Ativa,
                Active = true,
                AsaasCustomerId = asaasCustomerId,
                PlanId = planIdDefault
            };

            await _barbershopRepo.CreateAsync(barbershop);

            var user = new User
            {
                Name = request.BarbershopName,
                Email = request.Email,
                Password = request.Password,
                FirebaseUid = request.FirebaseUid,
                Role = RoleUserEnum.Admin,
                BarbershopId = barbershop.Id,
                Active = true
            };

            await _userRepo.CreateAsync(user);

            var jwt = _authService.GenerateJwtToken(user.Id, user.Role.ToString(), user.BarbershopId);

            return Ok(new AuthResponse
            {
                Token = jwt,
                Role = user.Role.ToString(),
                BarbershopId = user.BarbershopId
            });
        }
    }
}
