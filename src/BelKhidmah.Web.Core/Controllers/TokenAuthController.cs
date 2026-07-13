using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BelKhidmah.Authentication;
using BelKhidmah.Models.TokenAuth;

namespace BelKhidmah.Controllers
{
    [Route("api/[controller]/[action]")]
    public class TokenAuthController : BelKhidmahControllerBase
    {
        private readonly MobileAuthService _authService;

        public TokenAuthController(MobileAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        public Task<AuthenticateResultModel> Authenticate([FromBody] AuthenticateModel model)
            => _authService.AuthenticateAsync(model);

        [HttpPost]
        public Task<LoginResultDto> Register([FromBody] MobileRegisterInput model)
            => _authService.RegisterAsync(model);

        [HttpPost]
        public Task<LoginResultDto> ResendVerificationCode([FromBody] SendCodeInput model)
            => _authService.ResendVerificationCodeAsync(model);

        [HttpPost]
        public Task<LoginResultDto> Login([FromBody] SendCodeInput model)
            => _authService.LoginAsync(model);

        [HttpPost]
        public Task<AuthenticateResultModel> VerifyCode([FromBody] VerifyCodeInput model)
            => _authService.VerifyCodeAsync(model);
    }
}
