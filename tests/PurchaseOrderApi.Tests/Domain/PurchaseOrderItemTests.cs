using PurchaseOrderApi.Domain.Entities;

namespace PurchaseOrderApi.Tests.Domain;

/// <summary>
/// Testes unitários para validações da entidade OrderItem.
/// </summary>
public class OrderItemTests
{
    [Fact]
    public void TotalPrice_ShouldBeQuantityTimesUnitPrice()
    {
        var item = new OrderItem("Caneta", 10, 2.50m);
        Assert.Equal(25m, item.TotalPrice);
    }

    [Fact]
    public void Create_WithEmptyProductName_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() =>
            new OrderItem("", 1, 10m));
    }

    [Fact]
    public void Create_WithZeroQuantity_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() =>
            new OrderItem("Item", 0, 10m));
    }

    [Fact]
    public void Create_WithNegativePrice_ShouldThrowException()
    {
        Assert.Throws<ArgumentException>(() =>
            new OrderItem("Item", 1, -5m));
    }

    [Fact]
    public void Create_WithValidData_ShouldSetProperties()
    {
        var item = new OrderItem("Teclado", 2, 150m);

        Assert.Equal("Teclado", item.ProductName);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(150m, item.UnitPrice);
        Assert.Equal(300m, item.TotalPrice);
        Assert.NotEqual(Guid.Empty, item.Id);
    }
}
