using Microsoft.AspNetCore.Components.Authorization;

namespace LuShop.Web.Security;

//provedor de estado de autenticacao baseado em cookies
public interface ICookieAuthenticationStateProvider
{
    //verifica se o usuario esta autenticado 
    Task<bool> CheckAuthenticatedAsync();
    //obtem o estado completo do usuario
    Task<AuthenticationState> GetAuthenticationStateAsync();
    //notificar para a aplicacao sobre o estado do usuario
    void NotifyAuthenticationStateChanged();
}