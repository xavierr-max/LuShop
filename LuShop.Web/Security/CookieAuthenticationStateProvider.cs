using System.Net.Http.Json;
using System.Security.Claims;
using LuShop.Core.Models.Account;
using Microsoft.AspNetCore.Components.Authorization;

namespace LuShop.Web.Security;

public class CookieAuthenticationStateProvider(IHttpClientFactory clientFactory) :
    AuthenticationStateProvider,
    ICookieAuthenticationStateProvider
{
    //define o status de autenticacao
    private bool _isAuthenticated;
    private readonly HttpClient _client = clientFactory.CreateClient(Configuration.HttpClientName);

    //garante que o estado do usuario esta atualizado
    public async Task<bool> CheckAuthenticatedAsync()
    {
        await GetAuthenticationStateAsync();
        return _isAuthenticated;
    }

    //mantem atualizado todos os componentes que dependem de autenticacao
    public void NotifyAuthenticationStateChanged()
        => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    //define um usuario como anonimo ou autenticado
    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        //define o usuario como nao autenticado
        _isAuthenticated = false;
        //cria um "corpo" do usuario 
        var user = new ClaimsPrincipal(new ClaimsIdentity());

        //busca as informacoes do usuario logado
        var userInfo = await GetUser();
        //caso seja nulo, retorna um usuario anonimo
        if (userInfo is null)
            return new AuthenticationState(user);

        //filtra as chaves do usuario logado
        var claims = await GetClaims(userInfo);

        //cria o estado para o usuario logado com base nas suas claims 
        var id = new ClaimsIdentity(claims, nameof(CookieAuthenticationStateProvider));
        user = new ClaimsPrincipal(id);
        
        _isAuthenticated = true;
        //retorna um usuario autenticado ou anonimo
        return new AuthenticationState(user);
    }

    //retorna as informacoes do usuario pelo endpoint do identity
    private async Task<User?> GetUser()
    {
        try
        {
            return await _client.GetFromJsonAsync<User?>("v1/identity/manage/info");
        }
        catch
        {
            return null;
        }
    }

    private async Task<List<Claim>> GetClaims(User user)
    {
        //lista que recebe a claim Name e Email com o email usado para logar
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Email, user.Email)
        };

        //incrementa na lista anteriormente criada, as demais claims que o susuario possui
        claims.AddRange(
            user.Claims.Where(x =>
                    x.Key != ClaimTypes.Name &&
                    x.Key != ClaimTypes.Email)
                .Select(x 
                    => new Claim(x.Key, x.Value))
        );

        //array de RoleClaim
        RoleClaim[]? roles;
        try
        {
            //recebe pelo endpoint as RoleClaims do usuario
            roles = await _client.GetFromJsonAsync<RoleClaim[]>("v1/identity/roles");
        }
        catch
        {
            return claims;
        }
        
        //caso a RoleClaim nao seja nula ou vazia, adiciona a lista anteriormente criada
        foreach (var role in roles ?? [])
            if (!string.IsNullOrEmpty(role.Type) && !string.IsNullOrEmpty(role.Value))
                claims.Add(new Claim(role.Type, role.Value, role.ValueType, role.Issuer, role.OriginalIssuer));

        return claims;
    }
}