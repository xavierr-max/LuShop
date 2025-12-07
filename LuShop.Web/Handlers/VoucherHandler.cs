using System.Net.Http.Json;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Vouchers;
using LuShop.Core.Responses;

namespace LuShop.Web.Handlers;

public class VoucherHandler(IHttpClientFactory httpClientFactory) : IVoucherHandler
{
    private readonly HttpClient _client = httpClientFactory.CreateClient(Configuration.HttpClientName);

    public async Task<Response<Voucher?>> CreateAsync(CreateVoucherRequest request)
    {
        var result = await _client.PostAsJsonAsync("v1/vouchers", request);
        return await result.Content.ReadFromJsonAsync<Response<Voucher?>>()
               ?? new Response<Voucher?>(null, 400, "Falha ao criar voucher");
    }

    public async Task<Response<Voucher?>> UpdateAsync(UpdateVoucherRequest request)
    {
        var result = await _client.PutAsJsonAsync($"v1/vouchers/{request.Id}", request);
        return await result.Content.ReadFromJsonAsync<Response<Voucher?>>()
               ?? new Response<Voucher?>(null, 400, "Falha ao atualizar voucher");
    }

    public async Task<Response<Voucher?>> DeleteAsync(DeleteVoucherRequest request)
    {
        var result = await _client.DeleteAsync($"v1/vouchers/{request.Id}");
        return await result.Content.ReadFromJsonAsync<Response<Voucher?>>()
               ?? new Response<Voucher?>(null, 400, "Falha ao excluir voucher");
    }

    public async Task<Response<Voucher?>> GetByNumberAsync(GetVoucherByNumberRequest request)
    {
        // Rota baseada no número do voucher (string)
        return await _client.GetFromJsonAsync<Response<Voucher?>>($"v1/vouchers/{request.Number}")
               ?? new Response<Voucher?>(null, 400, "Voucher não encontrado");
    }

    public async Task<Response<List<Voucher>?>> GetAllAsync(GetAllVouchersRequest request)
    {
        var url = $"v1/vouchers?pageNumber={request.PageNumber}&pageSize={request.PageSize}";
        
        return await _client.GetFromJsonAsync<Response<List<Voucher>?>>(url)
               ?? new Response<List<Voucher>?>(null, 400, "Não foi possível listar os vouchers");
    }
}