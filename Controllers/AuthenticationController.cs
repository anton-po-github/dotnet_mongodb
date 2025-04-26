using System.Net;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/v1/authenticate")]
public class AuthenticationController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly EmailService _emailService;


    public AuthenticationController(
        UserManager<IdentityUser> userManager,
         RoleManager<IdentityRole> roleManager,
         EmailService emailService
         )
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _emailService = emailService;

    }

    [Authorize(Roles = "Admin")]
    [HttpGet("secret")]
    public string GetSomeSecretText()
    {
        return "some secret text";
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public IEnumerable<IdentityUser> GetAllUsers()
    {
        return _userManager.Users.ToList();
    }

    [HttpGet("confirm-email")]
    public async Task<IResult> ConfirmEmail([FromQuery] string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user is null) return Results.NotFound("User not found.");

        user.EmailConfirmed = true;

        await _userManager.UpdateAsync(user);

        return Results.Ok("Confirmed!");
    }

    [HttpPost]
    [Route("roles/add")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        var appRole = new IdentityRole { Name = request.Role };
        var createRole = await _roleManager.CreateAsync(appRole);

        return Ok(new { message = "role created succesfully" });
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await RegisterAsync(request);

        return result.Success ? Ok(result) : BadRequest(result.Message);

    }

    private async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
    {
        try
        {
            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null) return new RegisterResponse { Message = "User already exists", Success = false };

            //if we get here, no user with this email..

            userExists = new IdentityUser
            {
                FullName = request.FullName,
                Email = request.Email,
                ConcurrencyStamp = Guid.NewGuid().ToString(),
                UserName = request.Username,

            };
            var createUserResult = await _userManager.CreateAsync(userExists, request.Password);
            if (!createUserResult.Succeeded) return new RegisterResponse { Message = $"Create user failed {createUserResult?.Errors?.First()?.Description}", Success = false };
            //user is created...
            //then add user to a role...
            var addUserToRoleResult = await _userManager.AddToRoleAsync(userExists, userExists.UserName == "Bob" ? "Admin" : "User");
            if (!addUserToRoleResult.Succeeded) return new RegisterResponse { Message = $"Create user succeeded but could not add user to role {addUserToRoleResult?.Errors?.First()?.Description}", Success = false };


            var sendEmail = new EmailModel();

            sendEmail.ToEmail = request.Email;

            sendEmail.Subject = "My tests";

            await _emailService.SendEmail(sendEmail);

            //all is still well..
            return new RegisterResponse
            {
                Success = true,
                Message = "User registered successfully"
            };

        }
        catch (Exception ex)
        {
            return new RegisterResponse { Message = ex.Message, Success = false };
        }
    }

    [HttpPost]
    [Route("login")]
    [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(LoginResponse))]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await LoginAsync(request);

        return result.Success ? Ok(result) : BadRequest(result.Message);
    }

    private async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user is null) return new LoginResponse { Message = "Invalid email/password", Success = false };

            //all is well if ew reach here
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            var roles = await _userManager.GetRolesAsync(user);

            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x));

            claims.AddRange(roleClaims);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("P=9tJAm[Dkq6w#bNKySvF!}Lxf~Z4r]V5`z2RpH/-($,)%Xh8M"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expires = DateTime.Now.AddMinutes(30);

            var token = new JwtSecurityToken(
                issuer: "https://localhost:5001", // issuer + TokenValidationParameters.ValidIssuer = the SAME important
                audience: "https://localhost:5001", // audience + TokenValidationParameters.ValidAudience = the SAME important
                claims: claims,
                expires: expires,
                signingCredentials: creds
                );

            return new LoginResponse
            {
                AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
                Message = "Login Successful",
                Email = user?.Email,
                Success = true,
                UserId = user?.Id.ToString(),
                Role = await _userManager.GetRolesAsync(user)
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return new LoginResponse { Success = false, Message = ex.Message };
        }


    }
}


