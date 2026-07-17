namespace ShoppingCart.Web.Services;

/// <summary>
/// Holds the signed-in session for the current Blazor circuit.
/// POC note: this is in-memory per circuit — a full page refresh logs the user out.
/// For a real app, persist via ProtectedLocalStorage or an auth cookie.
/// </summary>
public class TokenProvider
{
    public AuthResponse? Session { get; private set; }
    public bool IsAuthenticated => Session is not null;

    public event Action? Changed;

    public void Set(AuthResponse session)
    {
        Session = session;
        Changed?.Invoke();
    }

    public void Clear()
    {
        Session = null;
        Changed?.Invoke();
    }
}
