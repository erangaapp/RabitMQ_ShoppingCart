using Identity.Domain;

namespace Identity.Application;

public record RegisterRequest(string Email, string Password, string FullName);
public record LoginRequest(string Email, string Password);
public record AuthResponse(Guid UserId, string Email, string FullName, string Token);
public record AuthResult(bool Succeeded, AuthResponse? Response, string? Error);

public interface IUserRepository
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}

public interface ITokenGenerator
{
    string Generate(User user);
}
