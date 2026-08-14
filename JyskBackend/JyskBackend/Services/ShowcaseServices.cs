using JyskBackend.Database;
using JyskBackend.Entities;
using JyskBackend.Extensions;
using JyskBackend.Interfaces;
using JyskBackend.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace JyskBackend.Services;

public class RoomsService(JyskDbContext context) : IRoomsService
{
    public async Task<List<ShowcaseShortResponse>> GetAllAsync() =>
        await context.Rooms
            .OrderBy(r => r.Name)
            .Select(r => new ShowcaseShortResponse(r.Id, r.Name, r.Description, r.CoverImageUrl))
            .ToListAsync();

    public async Task<ShowcaseDetailResponse?> GetByIdAsync(int id)
    {
        var room = await context.Rooms
            .AsSplitQuery()
            .Include(r => r.RoomProducts).ThenInclude(rp => rp.Product).ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (room == null) return null;

        return new ShowcaseDetailResponse(room.Id, room.Name, room.Description, room.CoverImageUrl,
            room.RoomProducts
                .Where(rp => rp.Product.IsActive)
                .Select(rp => rp.Product.ToShowcaseProduct())
                .ToList());
    }

    public async Task<ShowcaseShortResponse> CreateAsync(CreateShowcaseRequest request)
    {
        var room = new Room { Name = request.Name, Description = request.Description, CoverImageUrl = request.CoverImageUrl };
        context.Rooms.Add(room);
        await context.SaveChangesAsync();
        return new ShowcaseShortResponse(room.Id, room.Name, room.Description, room.CoverImageUrl);
    }

    public async Task<bool> AddProductAsync(int showcaseId, Guid productId)
    {
        var exists = await context.Rooms.AnyAsync(r => r.Id == showcaseId)
                     && await context.Products.AnyAsync(p => p.Id == productId);
        if (!exists) return false;

        // Повторне додавання того самого товару не має валити запит помилкою ключа.
        if (await context.RoomProducts.AnyAsync(rp => rp.RoomId == showcaseId && rp.ProductId == productId))
            return true;

        context.RoomProducts.Add(new RoomProduct { RoomId = showcaseId, ProductId = productId });
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveProductAsync(int showcaseId, Guid productId)
    {
        var link = await context.RoomProducts
            .FirstOrDefaultAsync(rp => rp.RoomId == showcaseId && rp.ProductId == productId);
        if (link == null) return false;

        context.RoomProducts.Remove(link);
        await context.SaveChangesAsync();
        return true;
    }
}

public class CollectionsService(JyskDbContext context) : ICollectionsService
{
    public async Task<List<ShowcaseShortResponse>> GetAllAsync() =>
        await context.Collections
            .OrderBy(c => c.Name)
            .Select(c => new ShowcaseShortResponse(c.Id, c.Name, c.Description, c.CoverImageUrl))
            .ToListAsync();

    public async Task<ShowcaseDetailResponse?> GetByIdAsync(int id)
    {
        var collection = await context.Collections
            .AsSplitQuery()
            .Include(c => c.CollectionProducts).ThenInclude(cp => cp.Product).ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (collection == null) return null;

        return new ShowcaseDetailResponse(collection.Id, collection.Name, collection.Description, collection.CoverImageUrl,
            collection.CollectionProducts
                .Where(cp => cp.Product.IsActive)
                .Select(cp => cp.Product.ToShowcaseProduct())
                .ToList());
    }

    public async Task<ShowcaseShortResponse> CreateAsync(CreateShowcaseRequest request)
    {
        var collection = new Collection { Name = request.Name, Description = request.Description, CoverImageUrl = request.CoverImageUrl };
        context.Collections.Add(collection);
        await context.SaveChangesAsync();
        return new ShowcaseShortResponse(collection.Id, collection.Name, collection.Description, collection.CoverImageUrl);
    }

    public async Task<bool> AddProductAsync(int showcaseId, Guid productId)
    {
        var exists = await context.Collections.AnyAsync(c => c.Id == showcaseId)
                     && await context.Products.AnyAsync(p => p.Id == productId);
        if (!exists) return false;

        if (await context.CollectionProducts.AnyAsync(cp => cp.CollectionId == showcaseId && cp.ProductId == productId))
            return true;

        context.CollectionProducts.Add(new CollectionProduct { CollectionId = showcaseId, ProductId = productId });
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveProductAsync(int showcaseId, Guid productId)
    {
        var link = await context.CollectionProducts
            .FirstOrDefaultAsync(cp => cp.CollectionId == showcaseId && cp.ProductId == productId);
        if (link == null) return false;

        context.CollectionProducts.Remove(link);
        await context.SaveChangesAsync();
        return true;
    }
}
