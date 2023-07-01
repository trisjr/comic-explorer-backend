using ApplicationCore.Entities.User;
using ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PublicApi.Requests;
using PublicApi.Responses;
using PublicApi.Responses.AuthResponse;
using Swashbuckle.AspNetCore.Annotations;

namespace PublicApi.Controllers;

[Route("api/auth")]
[SwaggerTag("Authentication")]
public class AuthController : BaseController
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ITokenClaimsService _tokenClaims;
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthController(
        ITokenClaimsService tokenClaims,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager
    )
    {
        _tokenClaims = tokenClaims;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Login",
        Description = "Login with username and password",
        OperationId = "auth.login"
    )]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Login successful",
        typeof(BaseResponseWithData<LoginResponseData>)
    )]
    [SwaggerResponse(
        StatusCodes.Status400BadRequest,
        "Invalid username or password",
        typeof(BaseResponse)
    )]
    [SwaggerResponse(StatusCodes.Status401Unauthorized, "Email not verified", typeof(BaseResponse))]
    [SwaggerResponse(
        StatusCodes.Status500InternalServerError,
        "Internal server error",
        typeof(BaseResponse)
    )]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
                return BadRequest(
                    new BaseResponse { Message = "Request is invalid", Status = "error" }
                );

            var user = await _userManager.FindByNameAsync(request.Email);
            if (user == null)
                return BadRequest(
                    new BaseResponse { Message = "Invalid username or password", Status = "error" }
                );

            if (!await _userManager.CheckPasswordAsync(user, request.Password))
            {
                await _userManager.AccessFailedAsync(user);
                return BadRequest(
                    new BaseResponse { Message = "Invalid username or password", Status = "error" }
                );
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return BadRequest(
                    new BaseResponse { Status = "error", Message = "Email not verified" }
                );

            var refreshToken = string.Empty;
            var token = await _tokenClaims.GetTokenAsync(user.UserName);
            await _userManager.ResetAccessFailedCountAsync(user);

            if (request.RememberMe)
                refreshToken = await _tokenClaims.GenerateRefreshTokenAsync(user.UserName);

            var data = new LoginResponseData
            {
                Token = token,
                RefreshToken = refreshToken,
                TokenType = "bearer"
            };

            return Ok(
                new BaseResponseWithData<LoginResponseData>
                {
                    Data = data,
                    Message = "Login successful",
                    Status = "success"
                }
            );
        }
        catch (Exception e)
        {
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new BaseResponse { Message = e.Message, Status = "error" }
            );
        }
    }

    [HttpPost("register")]
    [SwaggerOperation(
        Summary = "Register",
        Description = "Register with username, email and password",
        OperationId = "auth.register"
    )]
    [SwaggerResponse(
        StatusCodes.Status200OK,
        "Register successful",
        typeof(BaseResponseWithData<RegisterResponseData>)
    )]
    [SwaggerResponse(
        StatusCodes.Status400BadRequest,
        "Invalid username or password",
        typeof(BaseResponse)
    )]
    [SwaggerResponse(
        StatusCodes.Status500InternalServerError,
        "Internal server error",
        typeof(BaseResponse)
    )]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var userExists = await _userManager.FindByEmailAsync(request.Email);
        if (userExists != null)
            return StatusCode(
                StatusCodes.Status400BadRequest,
                new BaseResponse { Status = "Error", Message = "User already exists!" }
            );

        ApplicationUser user =
            new()
            {
                Email = request.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName
            };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new BaseResponse { Status = "Error", Message = "Password Invalid" }
            );

        if (await _roleManager.RoleExistsAsync(ApplicationRoles.User))
            await _userManager.AddToRoleAsync(user, ApplicationRoles.User);
        else
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new BaseResponse { Status = "Error", Message = "Role does not exist" }
            );

        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var callbackUrl = Url.Action(
            "VerifyEmail",
            "Auth",
            new { token, email = user.Email },
            Request.Scheme
        );
        if (string.IsNullOrEmpty(callbackUrl))
            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new BaseResponse { Status = "Error", Message = "Failed to generate callback url" }
            );

        Console.WriteLine(callbackUrl);
        return Ok(
            new BaseResponseWithData<RegisterResponseData>
            {
                Data = new RegisterResponseData { ConfirmUrl = callbackUrl, ExpiresIn = 3600 },
                Message = "Register successful",
                Status = "success"
            }
        );
    }

    [HttpGet]
    [Route("verify-account/verify")]
    [SwaggerOperation(Summary = "Verify email", Description = "Verify email")]
    public async Task<ActionResult> VerifyEmail([FromQuery] VerifyAccountRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Content("<script>window.close();</script>", "text/html");

            var token = Uri.UnescapeDataString(request.Token);
            await _userManager.ConfirmEmailAsync(user, token);

            return Content("<script>window.close();</script>", "text/html");
        }
        catch (Exception e)
        {
            return Content("<script>window.close();</script>", "text/html");
        }
    }
}
