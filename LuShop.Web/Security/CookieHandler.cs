using Microsoft.AspNetCore.Components.WebAssembly.Http;

namespace LuShop.Web.Security;

//garante que coolies sejam enviados nas requisicoes HTTP
//DelegatingHandler: intercepta uma requisicao
public class CookieHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        //define que o navegador deve incluir cookies na requisicao
        request.SetBrowserRequestCredentials(BrowserRequestCredentials.Include);
        //distinguir requisicoes AJAX de requisicoes normais, util para tratamento de autenticacao
        request.Headers.Add("X-Requested-With", ["XMLHttpRequest"]);

        //passa a requisicao modificada para o proximo handler
        return base.SendAsync(request, cancellationToken);
    }
}