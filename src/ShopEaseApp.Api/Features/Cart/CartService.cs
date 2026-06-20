using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using ShopEaseApp.Api.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ShopEaseApp.Api.Features.Cart;

public class CartService
{
    private static readonly TimeSpan CartTtl = TimeSpan.FromDays(7);
    private readonly IDistributedCache _cache;
    private readonly AppDbContext _db;

    public CartService(IDistributedCache cache, AppDbContext db)
    {
        _cache = cache;
        _db = db;
    }

    // ── Key helper ────────────────────────────────────────────────────────────

    private static string Key(string userId) => $"cart:{userId}";

    // ── Read ─────────────────────────────────────────────────────────────────

    public async Task<CartResponse> GetCartAsync(string userId)
    {
        var items = await LoadItemsAsync(userId);
        return ToResponse(userId, items);
    }

    // ── Add item ──────────────────────────────────────────────────────────────

    public async Task<(bool Success, CartResponse? Response, string? Error)> AddItemAsync(
        string userId, AddItemRequest request)
    {
        if (request.Quantity < 1)
            return (false, null, "Quantity must be at least 1.");

        var variant = await _db.ProductVariants
            .AsNoTracking()
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == request.VariantId);

        if (variant is null)
            return (false, null, "Variant not found.");

        var items = await LoadItemsAsync(userId);
        var existing = items.FirstOrDefault(i => i.VariantId == request.VariantId);

        if (existing is not null)
        {
            existing.Quantity += request.Quantity;
        }
        else
        {
            items.Add(new CartItem
            {
                VariantId = variant.Id,
                VariantName = variant.Name,
                ProductName = variant.Product.Name,
                Quantity = request.Quantity,
                PriceSnapshot = variant.Price   // captured at add-time
            });
        }

        await SaveItemsAsync(userId, items);
        return (true, ToResponse(userId, items), null);
    }

    // ── Update quantity ───────────────────────────────────────────────────────

    public async Task<(bool Success, CartResponse? Response, string? Error)> UpdateItemAsync(
        string userId, UpdateItemRequest request)
    {
        if (request.Quantity < 1)
            return (false, null, "Quantity must be at least 1.");

        var items = await LoadItemsAsync(userId);
        var item = items.FirstOrDefault(i => i.VariantId == request.VariantId);

        if (item is null)
            return (false, null, "Item not found in cart.");

        item.Quantity = request.Quantity;
        await SaveItemsAsync(userId, items);
        return (true, ToResponse(userId, items), null);
    }

    // ── Remove item ───────────────────────────────────────────────────────────

    public async Task<CartResponse> RemoveItemAsync(string userId, int variantId)
    {
        var items = await LoadItemsAsync(userId);
        items.RemoveAll(i => i.VariantId == variantId);
        await SaveItemsAsync(userId, items);
        return ToResponse(userId, items);
    }

    // ── Clear ─────────────────────────────────────────────────────────────────

    public async Task ClearAsync(string userId)
    {
        await _cache.RemoveAsync(Key(userId));
    }

    // ── Internals ─────────────────────────────────────────────────────────────

    private async Task<List<CartItem>> LoadItemsAsync(string userId)
    {
        var json = await _cache.GetStringAsync(Key(userId));
        if (string.IsNullOrEmpty(json)) return new List<CartItem>();
        return JsonSerializer.Deserialize<List<CartItem>>(json) ?? new List<CartItem>();
    }

    private async Task SaveItemsAsync(string userId, List<CartItem> items)
    {
        var json = JsonSerializer.Serialize(items);
        await _cache.SetStringAsync(Key(userId), json, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CartTtl
        });
    }

    private static CartResponse ToResponse(string userId, List<CartItem> items) =>
        new(userId, items, items.Sum(i => i.PriceSnapshot * i.Quantity));
}
