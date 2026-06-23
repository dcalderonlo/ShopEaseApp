namespace ShopEaseApp.Api.Domain;

/// <summary>
/// Pure stock-status computation shared by catalog and admin read models.
/// "In Stock" when Stock > MinimumStockLevel,
/// "Low Stock" when 0 &lt; Stock &lt;= MinimumStockLevel,
/// "Out of Stock" when Stock == 0.
/// </summary>
public static class StockStatus
{
    public static string Compute(int stock, int minimumStockLevel) =>
        stock == 0 ? "Out of Stock"
        : stock <= minimumStockLevel ? "Low Stock"
        : "In Stock";
}
