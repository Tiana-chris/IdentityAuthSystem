using IdentityAuthSystem.DTOs;
using IdentityAuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAuthSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            //Assign default User role
            await _userManager.AddToRoleAsync(user, "User");

            //Enable 2FA for testing
            await _userManager.SetTwoFactorEnabledAsync(user, true);
            return Ok(new { Message = "User registered successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Unauthorized("Invalid credentials.");

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, dto.Password, false, lockoutOnFailure: true);

            if (result.RequiresTwoFactor)
            {
                var token = await _userManager.GenerateTwoFactorTokenAsync(user, "Email");
                await _emailSender.SendEmailAsync(user.Email!, "Your 2FA Code", $"Your verification code is: <b>{token}</b>");
                return Ok(new { RequirewoFactor = true, Message = "2FA token sent to email." });

            }

            if (result.IsLockedOut)
                return BadRequest("Account is locked out.");

            if (!result.Succeeded)
                return Unauthorized("Invalid credentials.");

            return Ok(new { Message = "Login successsful." });
        }

        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] TwoFactorDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return BadRequest("Invaid request.");

            var valid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", dto.Code);
            if (!valid) return BadRequest("Invalid 2FA code.");

            return Ok(new { Message = "2FA Verification successful. Logged in." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return Ok(new { Message = "If the email exists, a reset token has been sent." });

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            await _emailSender.SendEmailAsync(user.Email!, "Password Reset Token", $"Your password rest token is: <b>{token}</b>");

            return Ok(new { Message = "If the email exists, a reset token has been sent." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return BadRequest("Inalid request");

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { Message = "Password reset successful" });
        }
    }
}


