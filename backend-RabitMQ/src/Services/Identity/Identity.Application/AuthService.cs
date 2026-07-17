using Identity.Domain;

namespace Identity.Application;

public class AuthService(IUserRepository users, IPasswordHasher hasher, ITokenGenerator tokens)
{
    public async Task<AuthResult> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
    {
        var existing = await users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (existing is not null)
            return new AuthResult(false, null, "A user with this email already exists.");

        if (request.Password.Length < 6)
            return new AuthResult(false, null, "Password must be at least 6 characters.");

        var user = User.Register(request.Email, hasher.Hash(request.Password), request.FullName);
        await users.AddAsync(user, ct);
        await users.SaveChangesAsync(ct);

        return new AuthResult(true, ToResponse(user), null);
    }

    public async Task<AuthResult> LoginAsync(LoginRequest request, CancellationToken ct = default)
    {
        var user = await users.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), ct);
        if (user is null || !hasher.Verify(request.Password, user.PasswordHash))
            return new AuthResult(false, null, "Invalid email or password.");

        return new AuthResult(true, ToResponse(user), null);
    }

    private AuthResponse ToResponse(User user) =>
        new(user.Id, user.Email, user.FullName, tokens.Generate(user));
}
