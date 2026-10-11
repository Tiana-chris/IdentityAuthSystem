using IdentityAuthSystem.DTOs;
using IdentityAuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityAuthSystem.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class RoleManagementController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RoleManagementController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpPost("create-role")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
                return BadRequest("Role Already exists,");

            var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok($"Role '{roleName}' created successfully.");
        }

        [HttpPost("assign-role")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null) return NotFound("User not found.");

            if (!await _roleManager.RoleExistsAsync(dto.RoleName))
                return BadRequest("Role does not exist.");

            var result = await _userManager.AddToRoleAsync(user, dto.RoleName);
            if (!result.Succeeded) return BadRequest(result.Errors);

            return Ok($"Assign role '{dto.RoleName}' to{user.Email}.");
        }
    }
}


