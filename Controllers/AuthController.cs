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

                
                return Ok(new { RequiresTwoFactor = true, Message = "2FA token sent to email." });
            }

            if (result.IsLockedOut)
                return BadRequest("Account is locked out.");

            if (!result.Succeeded)
                return Unauthorized("Invalid credentials.");

            
            return Ok(new { Message = "Login successful." });
        }


        [HttpPost("verify-2fa")]
        public async Task<IActionResult> VerifyTwoFactor([FromBody] TwoFactorDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return BadRequest("Invalid request.");

            var valid = await _userManager.VerifyTwoFactorTokenAsync(user, "Email", dto.Code);
            if (!valid) return BadRequest("Invalid 2FA code.");

            // Inside your login or 2FA success flow:
            var loginTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss UTC");
            var notificationBody = $"<h3>Security Alert</h3><p>A new login to your account was detected on {loginTime}. If this wasn't you, please secure your account immediately.</p>";

            await _emailSender.SendEmailAsync(user.Email!, "New Login Notification", notificationBody);

            return Ok(new { Message = "2FA Verification successful. Logged in." });

            
        }


        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Return success even if user doesn't exist to prevent email enumeration attacks
                return Ok(new { message = "If your email is registered, a password reset link has been sent." });
            }

            // 1. Generate the token FIRST before using it
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            // 2. Use Uri.EscapeDataString (with the dot)
            var resetLink = $"https://localhost:5114/api/Auth/reset-password?email={Uri.EscapeDataString(user.Email!)}&token={Uri.EscapeDataString(token)}";

            var emailBody = $"<h3>Password Reset Request</h3><p>Click the link below to reset your password:</p><a href='{resetLink}'>Reset Password</a>";
            await _emailSender.SendEmailAsync(user.Email!, "Reset Your Password", emailBody);

            return Ok(new { message = "Password reset link sent successfully.", token });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return BadRequest(new {message = "Invalid request"});

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { Message = "Password has been reset successfully" });
        }
    }
}


