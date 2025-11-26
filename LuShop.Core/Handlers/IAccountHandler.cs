using LuShop.Core.Requests.Account;
using LuShop.Core.Responses;

namespace LuShop.Core.Handlers;

public interface IAccountHandler
{
    Task<Response<string>> LoginAsync(LoginRequest request);
    Task<Response<string>> RegisterAsync(RegisterRequest request);
    Task LogoutAsync();
}