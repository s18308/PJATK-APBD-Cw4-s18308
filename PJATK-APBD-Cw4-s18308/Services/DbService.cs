using PJATK_APBD_Cw4_s18308.DTOs;
using PJATK_APBD_Cw4_s18308.Entities;
using PJATK_APBD_Cw4_s18308.Exceptions;
using PJATK_APBD_Cw4_s18308.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace PJATK_APBD_Cw4_s18308.Services;

public class DbService(DatabaseContext ctx) : IDbService
{
    public async Task<ICollection<PcResponse>> GetAllPcsAsync(CancellationToken cancellationToken)
    {
        return await ctx.PCs
            .Select(e => new PcResponse
            {
                Id = e.Id,
                Name = e.Name,
                Weight = e.Weight,
                Warranty = e.Warranty,
                CreatedAt = e.CreatedAt,
                Stock = e.Stock
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<PcWithComponentsResponse> GetPcWithComponentsAsync(int id, CancellationToken cancellationToken)
    {
        return await ctx.PCs
            .Where(e => e.Id == id)
            .Select(e => new PcWithComponentsResponse
            {
                Id = e.Id,
                Name = e.Name,
                Weight = e.Weight,
                Warranty = e.Warranty,
                CreatedAt = e.CreatedAt,
                Stock = e.Stock,
                Components = e.PCComponents.Select(pc => new PcComponentResponse
                {
                    Amount = pc.Amount,
                    Component = new ComponentResponse
                    {
                        Code = pc.Component.Code,
                        Name = pc.Component.Name,
                        Description = pc.Component.Description,
                        Manufacturer = new ManufacturerResponse
                        {
                            Id = pc.Component.ComponentManufacturer.Id,
                            Abbreviation = pc.Component.ComponentManufacturer.Abbreviation,
                            FullName = pc.Component.ComponentManufacturer.FullName,
                            FoundationDate = pc.Component.ComponentManufacturer.FoundationDate
                        },
                        Type = new ComponentTypeResponse
                        {
                            Id = pc.Component.ComponentType.Id,
                            Abbreviation = pc.Component.ComponentType.Abbreviation,
                            Name = pc.Component.ComponentType.Name
                        }
                    }
                }).ToList()
            })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"PC with id {id} not found");
    }

    public async Task<PcResponse> AddPcAsync(PcRequest request, CancellationToken cancellationToken)
    {
        var pc = new PC
        {
            Name = request.Name,
            Weight = request.Weight,
            Warranty = request.Warranty,
            CreatedAt = request.CreatedAt,
            Stock = request.Stock
        };

        await ctx.PCs.AddAsync(pc, cancellationToken);
        await ctx.SaveChangesAsync(cancellationToken);

        return new PcResponse
        {
            Id = pc.Id,
            Name = pc.Name,
            Weight = pc.Weight,
            Warranty = pc.Warranty,
            CreatedAt = pc.CreatedAt,
            Stock = pc.Stock
        };
    }

    public async Task UpdatePcAsync(int id, PcRequest request, CancellationToken cancellationToken)
    {
        var pc = await ctx.PCs.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

        pc!.Name = request.Name;
        pc.Weight = request.Weight;
        pc.Warranty = request.Warranty;
        pc.CreatedAt = request.CreatedAt;
        pc.Stock = request.Stock;

        await ctx.SaveChangesAsync(cancellationToken);
    }

    public async Task DeletePcAsync(int id, CancellationToken cancellationToken)
    {
        var transaction = await ctx.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await ctx.PCComponents.Where(e => e.PCId == id).ExecuteDeleteAsync(cancellationToken);
            var removedRows = await ctx.PCs.Where(e => e.Id == id).ExecuteDeleteAsync(cancellationToken);

            if (removedRows == 0)
                throw new NotFoundException($"PC with id {id} not found");

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
