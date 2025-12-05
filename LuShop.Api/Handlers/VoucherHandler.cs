using LuShop.Api.Data;
using LuShop.Core.Handlers;
using LuShop.Core.Models;
using LuShop.Core.Requests.Vouchers;
using LuShop.Core.Responses;
using Microsoft.EntityFrameworkCore;

namespace LuShop.Api.Handlers;

public class VoucherHandler(AppDbContext context) : IVoucherHandler
{
    public async Task<Response<Voucher?>> CreateAsync(CreateVoucherRequest request)
    {
        try
        {
            var voucher = new Voucher
            {
                Title = request.Title,
                Number = request.Number,
                Amount = request.Amount,
                IsActive = true // Padrão ao criar
            };

            await context.Vouchers.AddAsync(voucher);
            await context.SaveChangesAsync();

            return new Response<Voucher?>(voucher, 201, "Voucher criado com sucesso");
        }
        catch
        {
            return new Response<Voucher?>(null, 500, "Não foi possível criar o voucher");
        }
    }

    public async Task<Response<Voucher?>> UpdateAsync(UpdateVoucherRequest request)
    {
        try
        {
            var voucher = await context.Vouchers.FirstOrDefaultAsync(x => x.Id == request.Id);

            if (voucher is null)
                return new Response<Voucher?>(null, 404, "Voucher não encontrado");

            voucher.Title = request.Title;
            voucher.Number = request.Number;
            voucher.Amount = request.Amount;

            context.Vouchers.Update(voucher);
            await context.SaveChangesAsync();

            return new Response<Voucher?>(voucher, 200, "Voucher atualizado com sucesso");
        }
        catch
        {
            return new Response<Voucher?>(null, 500, "Não foi possível atualizar o voucher");
        }
    }

    public async Task<Response<Voucher?>> DeleteAsync(DeleteVoucherRequest request)
    {
        try
        {
            var voucher = await context.Vouchers.FirstOrDefaultAsync(x => x.Id == request.Id);

            if (voucher is null)
                return new Response<Voucher?>(null, 404, "Voucher não encontrado");

            context.Vouchers.Remove(voucher);
            await context.SaveChangesAsync();

            return new Response<Voucher?>(voucher, 200, "Voucher excluído com sucesso");
        }
        catch
        {
            return new Response<Voucher?>(null, 500, "Não foi possível excluir o voucher");
        }
    }

    public async Task<Response<Voucher?>> GetByNumberAsync(GetVoucherByNumberRequest request)
    {
        try
        {
            // AsNoTracking melhora performance pois o EF não precisa gerenciar estado para leitura
            var voucher = await context
                .Vouchers
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Number == request.Number);

            if (voucher is null)
                return new Response<Voucher?>(null, 404, "Voucher não encontrado");

            return new Response<Voucher?>(voucher, 200, "Voucher encontrado");
        }
        catch
        {
            return new Response<Voucher?>(null, 500, "Não foi possível recuperar o voucher");
        }
    }

    public async Task<Response<List<Voucher>?>> GetAllAsync(GetAllVouchersRequest request)
    {
        try
        {
            var vouchers = await context
                .Vouchers
                .AsNoTracking()
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync();

            return new Response<List<Voucher>?>(vouchers, 200, "Lista de vouchers obtida com sucesso");
        }
        catch
        {
            return new Response<List<Voucher>?>(null, 500, "Não foi possível obter a lista de vouchers");
        }
    }
}