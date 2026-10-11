using System.ComponentModel.DataAnnotations;

namespace IdentityAuthSystem.DTOs
{
    public record RegisterDto(
        [Required, EmailAddress] string Email,
        [Required] string Password,
        string? FirstName,
        string? LastName);

    public record LoginDto(
        [Required, EmailAddress] string Email,
        [Required] string Password);

    public record ForgotPasswordDto(
        [Required, EmailAddress] string Email);

    public record ResetPasswordDto(
        [Required, EmailAddress] string Email,
        [Required] string Token,
        [Required] string NewPassword);

    public record TwoFactorDto(
        [Required, EmailAddress] string Email,
        [Required] string Code);

    public record AssignRoleDto(
        [Required, EmailAddress] string Email,
        [Required] string RoleName);

}


