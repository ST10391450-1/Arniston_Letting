using Arniston_Letting_API.Data;
using Arniston_Letting_API.DTOs.Auth;
using Arniston_Letting_API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Arniston_Letting_API.Services;

/*
 *          Authentication Service Setup
 *
 *      AuthService Implements IAuthService and provides the
 *      authentication functionality used by the application.
 *
 *      ApplicationDbContext Provides access to the database,
 *      including the Users table.
 *
 *      IConfiguration Provides access to application configuration
 *      settings, including JWT authentication settings.
 *
 *      This service handles user login, registration, logout
 *      verification and JWT token generation.
 *
 */

public class AuthService : IAuthService
{
    // Provides access to the application's database.
    private readonly ApplicationDbContext _context;

    // Provides access to application configuration settings.
    private readonly IConfiguration _configuration;

    // A valid BCrypt hash of a throw-away value. It is verified against when a
    // login email does not exist so that unknown and known accounts take a
    // similar amount of time to reject, which stops attackers using response
    // timing to discover which email addresses are registered.
    private static readonly string TimingEqualisationHash =
        BCrypt.Net.BCrypt.HashPassword("timing-equalisation-placeholder");

    // Receives the database context and configuration through
    // dependency injection.
    public AuthService(
        ApplicationDbContext context,
        IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    #region Login User

    /*
     *          Functionality for authenticating an existing user.
     *
     *      LoginAsync() Receives a LoginRequest containing the
     *      user's email address and password.
     *
     *      The method first checks whether the request is null.
     *
     *      NormalizeEmail() Removes leading and trailing whitespace
     *      and converts the email address to lowercase.
     *
     *      IsNullOrWhiteSpace() Checks whether the email or password
     *      is missing or contains only whitespace.
     *
     *      AsNoTracking() Retrieves user information without
     *      tracking the database entity because this operation
     *      only reads data.
     *
     *      FirstOrDefaultAsync() Finds an active user matching
     *      the normalized email address.
     *
     *      BCrypt.Verify() Compares the supplied password against
     *      the stored password hash.
     *
     *      If the user does not exist, the password verification
     *      fails, or an exception occurs during verification,
     *      the method returns null.
     *
     *      GenerateToken() Creates a JWT authentication token
     *      for the successfully authenticated user.
     *
     *      Returns a LoginResponse containing the user's details,
     *      role, authentication status and JWT token.
     *
     */

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        // Check whether the login request was supplied.
        if (request == null)
        {
            return null;
        }

        // Normalize the email address before querying the database.
        var email = NormalizeEmail(request.Email);

        // Check that the email and password contain valid values.
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        // Find an active user matching the supplied email address.
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u =>
                u.Email == email &&
                u.IsActive);

        // Stop authentication if no active user was found.
        if (user == null)
        {
            // Spend comparable time to a real password check (see
            // TimingEqualisationHash) before rejecting the request.
            _ = BCrypt.Net.BCrypt.Verify(
                request.Password,
                TimingEqualisationHash);

            return null;
        }

        // Store the result of the password verification.
        bool passwordValid;

        try
        {
            // Verify the supplied password against the stored BCrypt hash.
            passwordValid = BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash);
        }
        catch
        {
            // Return null if password verification causes an exception.
            return null;
        }

        // Stop authentication if the supplied password is incorrect.
        if (!passwordValid)
        {
            return null;
        }

        // Generate a JWT token for the authenticated user.
        var token = GenerateToken(user);

        // Return the successful login response with user details and token.
        return new LoginResponse
        {
            Success = true,
            Message = "Login successful.",
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Token = token
        };
    }

    #endregion

    #region Register User

    /*
     *          Functionality for registering a new user account.
     *
     *      RegisterAsync() Receives a RegisterRequest containing
     *      the new user's email address, password, first name
     *      and last name.
     *
     *      The method first checks whether the request is null.
     *
     *      NormalizeEmail() Removes leading and trailing whitespace
     *      and converts the email address to lowercase.
     *
     *      IsNullOrWhiteSpace() Checks whether the required
     *      registration fields contain valid values.
     *
     *      AsNoTracking() Retrieves existing user information
     *      without tracking the database entity.
     *
     *      FirstOrDefaultAsync() Checks whether a user already
     *      exists with the supplied email address.
     *
     *      BCrypt.HashPassword() Generates a secure password hash
     *      using a BCrypt salt with a work factor of 12.
     *
     *      A new User entity is created with the supplied details.
     *      The new account is assigned the default "User" role
     *      and is marked as active.
     *
     *      Add() Adds the new user to the database context.
     *
     *      SaveChangesAsync() Saves the new user account to
     *      the database.
     *
     *      DbUpdateException Handles database update failures
     *      by returning null.
     *
     *      GenerateToken() Creates a JWT token for the newly
     *      registered user.
     *
     *      Returns a LoginResponse containing the new user's details,
     *      role, account creation message and JWT token.
     *
     */

    public async Task<LoginResponse?> RegisterAsync(
        RegisterRequest request)
    {
        // Check whether the registration request was supplied.
        if (request == null)
        {
            return null;
        }

        // Normalize the email address before checking the database.
        var email = NormalizeEmail(request.Email);

        // Check that all required registration fields contain valid values.
        if (string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(request.Password) ||
            string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return null;
        }

        // Check whether an account already exists with this email address.
        var existingUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == email);

        // Prevent registration if the email address is already in use.
        if (existingUser != null)
        {
            return null;
        }

        // Hash the supplied password using BCrypt with a work factor of 12.
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(
            request.Password,
            BCrypt.Net.BCrypt.GenerateSalt(12));

        // Create a new user account using the supplied registration details.
        var user = new Models.User
        {
            Email = email,
            PasswordHash = passwordHash,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Role = "User",
            IsActive = true
        };

        // Add the new user account to the database context.
        _context.Users.Add(user);

        try
        {
            // Save the new user account to the database.
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Return null if the database update fails.
            return null;
        }

        // Generate a JWT token for the newly registered user.
        var token = GenerateToken(user);

        // Return the successful registration response with user details and token.
        return new LoginResponse
        {
            Success = true,
            Message = "Account created successfully.",
            UserId = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role,
            Token = token
        };
    }

    #endregion

    #region Logout User

    /*
     *          Functionality for verifying a user's logout request.
     *
     *      LogoutAsync() Receives the ID of the user requesting logout.
     *
     *      The method first checks whether the supplied user ID
     *      is greater than zero.
     *
     *      AsNoTracking() Checks the database without tracking
     *      the user entity.
     *
     *      AnyAsync() Checks whether a user exists with the
     *      supplied user ID.
     *
     *      Returns false if the supplied ID is invalid.
     *
     *      Returns true if the user exists, otherwise false.
     *
     */

    public async Task<bool> LogoutAsync(int userId)
    {
        // Check whether the supplied user ID is valid.
        if (userId <= 0)
        {
            return false;
        }

        // Check whether the user exists in the database.
        var userExists = await _context.Users
            .AsNoTracking()
            .AnyAsync(u => u.UserId == userId);

        // Return whether the requested user exists.
        return userExists;
    }

    #endregion

    #region Generate JWT Token

    /*
     *          Functionality for generating a JWT authentication token.
     *
     *      GenerateToken() Receives the authenticated User entity
     *      and creates a signed JSON Web Token.
     *
     *      IConfiguration Retrieves the JWT settings from application
     *      configuration:
     *
     *          Jwt:Key       Secret key used to sign the token.
     *          Jwt:Issuer    Identifies the token issuer.
     *          Jwt:Audience  Identifies the intended token recipient.
     *
     *      The method checks that all required JWT configuration
     *      values are present.
     *
     *      GetByteCount() Checks that the secret key contains
     *      at least 32 bytes.
     *
     *      GetValue() Retrieves the configured token expiration
     *      time in minutes. If the value is missing, the default
     *      is 60 minutes.
     *
     *      Claims Stores the user's ID, email address, full name
     *      and role inside the JWT token.
     *
     *      SymmetricSecurityKey Creates the cryptographic key
     *      used to sign the token.
     *
     *      SigningCredentials Configures the token to use
     *      the HMAC SHA-256 signing algorithm.
     *
     *      JwtSecurityToken Creates the JWT using the issuer,
     *      audience, claims, issue time and expiration time.
     *
     *      JwtSecurityTokenHandler().WriteToken() Serializes
     *      the generated JWT into a string.
     *
     *      Returns the signed JWT token as a string.
     *
     */

    private string GenerateToken(Models.User user)
    {
        // Retrieve the JWT secret key, issuer and audience from configuration.
        var key = _configuration["Jwt:Key"];
        var issuer = _configuration["Jwt:Issuer"];
        var audience = _configuration["Jwt:Audience"];

        // Ensure that a JWT signing key has been configured.
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException(
                "JWT key is not configured.");
        }

        // Ensure that a JWT issuer has been configured.
        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException(
                "JWT issuer is not configured.");
        }

        // Ensure that a JWT audience has been configured.
        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException(
                "JWT audience is not configured.");
        }

        // Ensure that the JWT signing key contains at least 32 bytes.
        if (Encoding.UTF8.GetByteCount(key) < 32)
        {
            throw new InvalidOperationException(
                "JWT key must be at least 32 bytes long.");
        }

        // Retrieve the configured token lifetime, defaulting to 60 minutes.
        var expiresInMinutes = _configuration.GetValue<int>(
            "Jwt:ExpiresInMinutes",
            60);

        // Use 60 minutes if the configured expiration time is invalid.
        if (expiresInMinutes <= 0)
        {
            expiresInMinutes = 60;
        }

        // Create the claims that identify the authenticated user.
        var claims = new List<Claim>
        {
            // Stores the user's database ID as the name identifier.
            new Claim(
                ClaimTypes.NameIdentifier,
                user.UserId.ToString()),

            // Stores the user's email address.
            new Claim(
                ClaimTypes.Email,
                user.Email),

            // Stores the user's full name.
            new Claim(
                ClaimTypes.Name,
                $"{user.FirstName} {user.LastName}"),

            // Stores the user's assigned application role.
            new Claim(
                ClaimTypes.Role,
                user.Role)
        };

        // Create the symmetric security key from the configured secret.
        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        // Configure the token to use HMAC SHA-256 signing.
        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        // Create the JWT with its issuer, audience, claims and validity period.
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(expiresInMinutes),
            signingCredentials: credentials);

        // Serialize the JWT into a string and return it.
        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }

    #endregion

    #region Normalize Email

    /*
     *          Functionality for normalizing email addresses.
     *
     *      NormalizeEmail() Receives an optional email address.
     *
     *      Trim() Removes leading and trailing whitespace.
     *
     *      ToLowerInvariant() Converts the email address to
     *      lowercase using culture-independent casing rules.
     *
     *      The null-conditional operator prevents an error if
     *      the supplied email address is null.
     *
     *      The null-coalescing operator returns an empty string
     *      when the supplied email address is null.
     *
     *      Returns the normalized email address as a string.
     *
     */

    private static string NormalizeEmail(string? email)
    {
        // Trim whitespace, convert to lowercase, or return an empty string.
        return email?.Trim().ToLowerInvariant() ?? string.Empty;
    }

    #endregion
}