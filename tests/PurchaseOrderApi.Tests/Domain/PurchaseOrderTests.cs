using PurchaseOrderApi.Domain.Entities;
using PurchaseOrderApi.Domain.Enums;

namespace PurchaseOrderApi.Tests.Domain;

/// <summary>
/// Testes unitários para validações da entidade PurchaseOrder.
/// Testa apenas criação e validação — regras de negócio ficam nos testes do Service.
/// </summary>
public class PurchaseOrderTests
{
    private static readonly Guid CreatorId = Guid.NewGuid();

    private static List<OrderItem> CreateItems(decimal totalValue)
    {
        return new List<OrderItem> { new("Item teste", 1, totalValue) };
    }

    // --- RN1: Pedido deve conter pelo menos 1 item ---

    [Fact]
    public void Create_WithNoItems_ShouldThrowException()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            new PurchaseOrder(CreatorId, new List<OrderItem>()));
        Assert.Contains("pelo menos 1 item", ex.Message);
    }

    [Fact]
    public void Create_WithNullItems_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() =>
            new PurchaseOrder(CreatorId, null!));
    }

    [Fact]
    public void Create_WithValidItems_ShouldCreateInDraftStatus()
    {
        var order = new PurchaseOrder(CreatorId, CreateItems(50m));

        Assert.Equal(OrderStatus.Draft, order.Status);
        Assert.Single(order.Items);
        Assert.Equal(CreatorId, order.CreatorUserId);
    }

    // --- RN2: Cálculo do valor total ---

    [Fact]
    public void TotalValue_ShouldBeCalculatedCorrectly()
    {
        var items = new List<OrderItem>
        {
            new("Item A", 2, 30m),   // 60
            new("Item B", 3, 100m),  // 300
            new("Item C", 1, 40m)    // 40
        };
        var order = new PurchaseOrder(CreatorId, items);
        Assert.Equal(400m, order.TotalValue);
    }

    [Fact]
    public void TotalValue_SingleItem_ShouldMatchQuantityTimesPrice()
    {
        var order = new PurchaseOrder(CreatorId,
            new List<OrderItem> { new("Papel", 5, 10m) });
        Assert.Equal(50m, order.TotalValue);
    }
}
