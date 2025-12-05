using LuShop.Core.Models;
using LuShop.Core.Requests.Vouchers;
using LuShop.Core.Responses;

namespace LuShop.Core.Handlers;

public interface IVoucherHandler
{
    Task<Response<Voucher?>> CreateAsync(CreateVoucherRequest request);

    // Handler para atualizar
    Task<Response<Voucher?>> UpdateAsync(UpdateVoucherRequest request);

    // Handler para deletar
    Task<Response<Voucher?>> DeleteAsync(DeleteVoucherRequest request);

    // Handler para buscar por número
    Task<Response<Voucher?>> GetByNumberAsync(GetVoucherByNumberRequest request);
    
    // Handler para listar todas os Vouchers
    Task<Response<List<Voucher>?>> GetAllAsync(GetAllVouchersRequest request);
}