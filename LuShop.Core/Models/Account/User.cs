namespace LuShop.Core.Models.Account;

//modelo é usado pelo CookieAuthenticationStateProvider.cs no frontend para obter informações do usuário após a autenticação.
public class User
{
    public string Email { get; set; } = string.Empty;
    public bool IsEmailConfirmed { get; set; }
    public Dictionary<string, string> Claims { get; set; } = [];
}