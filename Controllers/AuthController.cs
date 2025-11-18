using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
//using Microsoft.AspNetCore.Identity.Data;
using kv_be_csharp_dotnet_dataapi_collections.Repositories;
using kv_be_csharp_dotnet_dataapi_collections.Models;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using tryAGI.OpenAI;

namespace kv_be_csharp_dotnet_dataapi_collections.Controllers;

[ApiController]
[Route("/api/v1/users")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IUserDAL _userDAL;
    private readonly IUserCredentialsDAL _userCredsDAL;

    public AuthController(IConfiguration configuration, IUserDAL userDAL, IUserCredentialsDAL userCredsDAL)
    {
        _configuration = configuration;
        _userDAL = userDAL;
        _userCredsDAL = userCredsDAL;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        Console.WriteLine("Login request: " + loginRequest.email);

        var dbUser = _userCredsDAL.FindByEmail(loginRequest.email);

        if (dbUser is not null)
        {
            string email = dbUser.email;
            string hashedPassword = dbUser.password;
            Guid userid = dbUser.userid;

            // verify password
            if (!VerifyPassword(loginRequest.password, hashedPassword))
            {
                return Unauthorized("Invalid password.");
            }

            var token = GenerateToken(userid, email);
            Console.WriteLine("User " + email + " has logged-in successfully.");
            //return Ok(new { Token = token });
            return Ok(new JwtResponse(token, userid.ToString(), email));
        }

        return Unauthorized("Invalid username or email address");
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserRegistrationRequest req)
    {
        var newUser = Models.User.fromUserRegistrationRequest(req);

        Console.WriteLine("Registration request: " + req.email);

        Guid userid = newUser.userid;
        string email = newUser.email;

        // generate credentials
        UserCredentials creds = new UserCredentials();
        creds.email = email;
        creds.userid = userid;
        creds.password = HashPassword(req.password);

        // save to DB
        _userDAL.SaveUser(newUser);
        _userCredsDAL.SaveUserCreds(creds);

        // gen and return token
        var token = GenerateToken(userid, email);
        //Console.WriteLine("User " + email + " has registered successfully.");

        return Ok(new JwtResponse(token, userid.ToString(), email));
    }

    [HttpGet("me")]
    [ProducesResponseType(typeof(Models.User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [Authorize]
    public async Task<ActionResult<Models.User>> GetCurrentUser()
    {
        Console.WriteLine("Request for user profile data.");

        Guid userid = getUserIdFromAuth(HttpContext.User);

        var user = await _userDAL.FindByUserId(userid);

        if (user is not null)
        {
            Console.WriteLine("found user " + user.email);
            return Ok(user);
        }

        return Unauthorized("Current user not found! Please check your token.");
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UserUpdateRequest req)
    {
        Guid userid = getUserIdFromAuth(HttpContext.User);
        Console.WriteLine("Processing update request for user: " + userid);

        // find user in the database
        var user = await _userDAL.FindByUserId(userid);

        if (user is not null)
        {
            // update only the fields that are present in the request
            if (!string.IsNullOrEmpty(req.firstName))
            {
                user.firstname = req.firstName;
            }

            if (!string.IsNullOrEmpty(req.lastName))
            {
                user.lastname = req.lastName;
            }

            if (!string.IsNullOrEmpty(req.email) && !req.email.Equals(user.email))
            {
                // email should be changed...but is the new email already in use?
                if (await _userDAL.ExistsByEmail(req.email))
                {
                    Console.WriteLine("Email is already in use!");
                    return BadRequest("Error: Email is already in use");
                }
                user.email = req.email;
            }

            if (!string.IsNullOrEmpty(req.password))
            {
                // change password
                UserCredentials creds = new UserCredentials();
                creds.userid = userid;
                creds.email = user.email;
                creds.password = HashPassword(req.password);

                _userCredsDAL.UpdateUserCreds(creds);
            }

            _userDAL.UpdateUser(user);

            return Ok("User " + user.email + " updated successfully!");
        }

        return Unauthorized("Current user not found! Please check your token.");
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(typeof(Models.UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Models.UserResponse>> GetUser(Guid userId) {
        var user = await _userDAL.FindByUserId(userId);
        if (user is not null)
        {

            return Ok(UserResponse.fromUser(user));
        }
        else
        {
            Console.WriteLine("Error locating user: " + userId);
            return NotFound("User " + userId + " could not be found!");
        }
    }

    [HttpGet("email/{email}")]
    [ProducesResponseType(typeof(Models.User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Models.User>> GetUserByEmail(string email) {
        var user = _userDAL.FindByEmail(email);
        if (user is not null)
        {
            return Ok(user);
        }
        else
        {
            Console.WriteLine("Error locating user: " + email);
            return NotFound("User " + email + " could not be found!");
        }
    }

    private Guid getUserIdFromAuth(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier);
        //var userNameClaim = currentUser.FindFirst(ClaimTypes.Email);

        if (userIdClaim is not null)
        {
            return Guid.Parse(userIdClaim.Value);
        }

        return Guid.Empty;
    }
    
    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
    
    private string GenerateToken(Guid userId, string username)
    {
        //Console.WriteLine("Generating a new token for email: " + username + " and userid: " + userId);

        //var jwtSettings = _configuration.GetSection("JwtSettings");
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]));
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.NameId, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["ExpiryMinutes"])),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}