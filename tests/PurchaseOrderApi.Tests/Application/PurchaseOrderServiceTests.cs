using Moq;
using Microsoft.Extensions.Logging;
using PurchaseOrderApi.Application.DTOs.Requests;
using PurchaseOrderApi.Application.Services;
using PurchaseOrderApi.Domain.Entities;
using PurchaseOrderApi.Domain.Enums;
using PurchaseOrderApi.Domain.Interfaces;

namespace PurchaseOrderApi.Tests.Application;

/// <summary>
/// Testes unitários para o PurchaseOrderService.
/// Todas as regras de negócio (RN1 a RN8) são testadas aqui.
/// </summary>
public class PurchaseOrderServiceTests
{
    private readonly Mock<IPurchaseOrderRepository> _mockRepo;
    private readonly PurchaseOrderService _service;

    private static readonly Guid CreatorId = Guid.NewGuid();
    private static readonly Guid SuppliesUserId = Guid.NewGuid();
    private static readonly Guid ManagerUserId = Guid.NewGuid();
    private static readonly Guid DirectorUserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();

    public PurchaseOrderServiceTests()
    {
        _mockRepo = new Mock<IPurchaseOrderRepository>();
        var mockLogger = new Mock<ILogger<PurchaseOrderService>>();
        _service = new PurchaseOrderService(_mockRepo.Object, mockLogger.Object);
    }

    // --- Helpers ---

    private PurchaseOrder CreateOrder(decimal totalValue = 50m)
    {
        var items = new List<OrderItem> { new("Item teste", 1, totalValue) };
        var order = new PurchaseOrder(CreatorId, items);
        order.History.Add(new OrderHistory(HistoryAction.Created, CreatorId, "Pedido de compra criado."));
        return order;
    }

    private PurchaseOrder CreateSubmittedOrder(decimal totalValue = 50m)
    {
        var order = CreateOrder(totalValue);
        order.Status = OrderStatus.AwaitingApproval;

        var requiredLevel = PurchaseOrderService.GetRequiredApprovalLevel(order.TotalValue);
        order.Approvals.Add(new Approval(ApprovalLevel.Supplies));
        if (requiredLevel >= ApprovalLevel.Manager)
            order.Approvals.Add(new Approval(ApprovalLevel.Manager));
        if (requiredLevel >= ApprovalLevel.Director)
            order.Approvals.Add(new Approval(ApprovalLevel.Director));

        order.History.Add(new OrderHistory(HistoryAction.Submitted, CreatorId, "Pedido enviado para aprovação."));
        return order;
    }

    private void SetupRepo(PurchaseOrder order)
    {
        _mockRepo.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
        _mockRepo.Setup(r => r.UpdateAsync(It.IsAny<PurchaseOrder>())).Returns(Task.CompletedTask);
        _mockRepo.Setup(r => r.AddAsync(It.IsAny<PurchaseOrder>())).Returns(Task.CompletedTask);
    }

    // --- RN3: Alçada de aprovação ---

    [Theory]
    [InlineData(50, ApprovalLevel.Supplies)]
    [InlineData(100, ApprovalLevel.Supplies)]
    [InlineData(101, ApprovalLevel.Manager)]
    [InlineData(500, ApprovalLevel.Manager)]
    [InlineData(1000, ApprovalLevel.Manager)]
    [InlineData(1001, ApprovalLevel.Director)]
    [InlineData(5000, ApprovalLevel.Director)]
    public void GetRequiredApprovalLevel_ShouldReturnCorrectLevel(decimal total, ApprovalLevel expected)
    {
        Assert.Equal(expected, PurchaseOrderService.GetRequiredApprovalLevel(total));
    }

    // --- RN4: Submit ---

    [Fact]
    public async Task SubmitAsync_ShouldSetAwaitingApprovalAndCreateApprovals()
    {
        var order = CreateOrder(500m);
        SetupRepo(order);

        await _service.SubmitAsync(order.Id, new SubmitRequest { UserId = CreatorId });

        Assert.Equal(OrderStatus.AwaitingApproval, order.Status);
        Assert.True(order.Approvals.Count >= 1);
        Assert.Equal(ApprovalStatus.Pending, order.Approvals.First(a => a.Level == ApprovalLevel.Supplies).Status);
        _mockRepo.Verify(r => r.UpdateAsync(order), Times.Once);
    }

    [Fact]
    public async Task SubmitAsync_WhenAlreadyAwaitingApproval_ShouldThrowException()
    {
        var order = CreateSubmittedOrder(50m);
        SetupRepo(order);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SubmitAsync(order.Id, new SubmitRequest { UserId = CreatorId }));
    }

    [Fact]
    public async Task SubmitAsync_WhenApproved_ShouldThrowException()
    {
        var order = CreateSubmittedOrder(50m);
        order.Status = OrderStatus.Approved;
        SetupRepo(order);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SubmitAsync(order.Id, new SubmitRequest { UserId = CreatorId }));
    }

    [Fact]
    public async Task SubmitAsync_ByNonCreator_ShouldThrowException()
    {
        var order = CreateOrder(50m);
        SetupRepo(order);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.SubmitAsync(order.Id, new SubmitRequest { UserId = OtherUserId }));
    }

    // --- RN4: Aprovação sequencial ---

    [Fact]
    public async Task ApproveAsync_AtSuppliesLevel_ShouldKeepAwaitingApproval_WhenManagerRequired()
    {
        var order = CreateSubmittedOrder(500m);
        SetupRepo(order);

        await _service.ApproveAsync(order.Id, new ApprovalActionRequest
        {
            UserId = SuppliesUserId,
            ApproverLevel = ApprovalLevel.Supplies
        });

        Assert.Equal(OrderStatus.AwaitingApproval, order.Status);
        var managerApproval = order.Approvals.First(a => a.Level == ApprovalLevel.Manager);
        Assert.Equal(ApprovalStatus.Pending, managerApproval.Status);
    }

    [Fact]
    public async Task ApproveAsync_AtManagerLevel_ShouldKeepAwaitingApproval_WhenDirectorRequired()
    {
        var order = CreateSubmittedOrder(2000m);
        SetupRepo(order);

        // Aprovar Supplies primeiro
        await _service.ApproveAsync(order.Id, new ApprovalActionRequest
        {
            UserId = SuppliesUserId,
            ApproverLevel = ApprovalLevel.Supplies
        });

        // Aprovar Manager
        await _service.ApproveAsync(order.Id, new ApprovalActionRequest
        {
            UserId = ManagerUserId,
            ApproverLevel = ApprovalLevel.Manager
        });

        Assert.Equal(OrderStatus.AwaitingApproval, order.Status);
        var directorApproval = order.Approvals.First(a => a.Level == ApprovalLevel.Director);
        Assert.Equal(ApprovalStatus.Pending, directorApproval.Status);
    }

    [Fact]
    public async Task ApproveAsync_OutOfOrder_ShouldThrowException()
    {
        var order = CreateSubmittedOrder(500m);
        SetupRepo(order);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ApproveAsync(order.Id, new ApprovalActionRequest
            {
                UserId = ManagerUserId,
                ApproverLevel = ApprovalLevel.Manager
            }));
    }

    [Fact]
    public async Task ApproveAsync_WhenNotAwaitingApproval_ShouldThrowException()
    {
        var order = CreateOrder(50m); // Draft
        SetupRepo(order);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ApproveAsync(order.Id, new ApprovalActionRequest
            {
                UserId = SuppliesUserId,
                ApproverLevel = ApprovalLevel.Supplies
            }));
    }

    // --- RN5: Revisão e reinício do fluxo ---

    [Fact]
    public async Task RequestRevisionAsync_ShouldSetUnderReviewStatus()
    {
        var order = CreateSubmittedOrder(500m);
        SetupRepo(order);

        await _service.RequestRevisionAsync(order.Id, new ApprovalActionRequest
        {
            UserId = SuppliesUserId,
            ApproverLevel = ApprovalLevel.Supplies,
            Comments = "Dados incompletos"
        });

        Assert.Equal(OrderStatus.UnderReview, order.Status);
    }

    [Fact]
    public async Task RequestRevisionAsync_WithoutReason_ShouldThrowException()
    {
        var order = CreateSubmittedOrder(500m);
        SetupRepo(order);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RequestRevisionAsync(order.Id, new ApprovalActionRequest
            {
                UserId = SuppliesUserId,
                ApproverLevel = ApprovalLevel.Supplies,
                Comments = ""
            }));
    }

    [Fact]
    public async Task ResubmitAsync_ShouldRestartApprovalFromSupplies()
    {
        var order = CreateSubmittedOrder(500m);
        order.Status = OrderStatus.UnderReview;
        SetupRepo(order);

        await _service.ResubmitAsync(order.Id, new ResubmitRequest { UserId = CreatorId });

        Assert.Equal(OrderStatus.AwaitingApproval, order.Status);
        var suppliesApproval = order.Approvals.First(a => a.Level == ApprovalLevel.Supplies);
        Assert.Equal(ApprovalStatus.Pending, suppliesApproval.Status);
    }

    [Fact]
    public async Task ResubmitAsync_AfterPartialApproval_ShouldRestartFromBeginning()
    {
        var order = CreateSubmittedOrder(2000m);
        SetupRepo(order);

        // Aprovar Supplies
        await _service.ApproveAsync(order.Id, new ApprovalActionRequest
        {
            UserId = SuppliesUserId,
            ApproverLevel = ApprovalLevel.Supplies
        });

        // Manager solicita revisão
        await _service.RequestRevisionAsync(order.Id, new ApprovalActionRequest
        {
            UserId = ManagerUserId,
            ApproverLevel = ApprovalLevel.Manager,
            Comments = "Valores errados"
        });

        // Reenviar
        await _service.ResubmitAsync(order.Id, new ResubmitRequest { UserId = CreatorId });

        // Deve reiniciar do Supplies
        var suppliesApproval = order.Approvals.First(a => a.Level == ApprovalLevel.Supplies);
        Assert.Equal(ApprovalStatus.Pending, suppliesApproval.Status);
    }

    [Fact]
    public async Task ResubmitAsync_ByNonCreator_ShouldThrowException()
    {
        var order = CreateSubmittedOrder(500m);
        order.Status = OrderStatus.UnderReview;
        SetupRepo(order);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.ResubmitAsync(order.Id, new ResubmitRequest { UserId = OtherUserId }));
    }

    // --- RN6: Histórico de ações ---

    [Fact]
    public async Task SubmitAsync_ShouldTrackHistoryAction()
    {
        var order = CreateOrder(50m);
        SetupRepo(order);

        await _service.SubmitAsync(order.Id, new SubmitRequest { UserId = CreatorId });

        var historyActions = order.History.Select(h => h.Action).ToList();
        Assert.Contains(HistoryAction.Created, historyActions);
        Assert.Contains(HistoryAction.Submitted, historyActions);
    }

    [Fact]
    public async Task ApproveAsync_ShouldTrackHistoryAction()
    {
        var order = CreateSubmittedOrder(50m);
        SetupRepo(order);

        await _service.ApproveAsync(order.Id, new ApprovalActionRequest
        {
            UserId = SuppliesUserId,
            ApproverLevel = ApprovalLevel.Supplies
        });

        var historyActions = order.History.Select(h => h.Action).ToList();
        Assert.Contains(HistoryAction.Approved, historyActions);
    }

    [Fact]
    public async Task RevisionFlow_ShouldTrackAllHistoryActions()
    {
        var order = CreateSubmittedOrder(500m);
        order.Status = OrderStatus.UnderReview;
        order.Approvals.First(a => a.Level == ApprovalLevel.Supplies).Status = ApprovalStatus.RevisionRequested;
        order.History.Add(new OrderHistory(HistoryAction.RevisionRequested, SuppliesUserId, "Revisão solicitada."));
        SetupRepo(order);

        await _service.ResubmitAsync(order.Id, new ResubmitRequest { UserId = CreatorId });

        var historyActions = order.History.Select(h => h.Action).ToList();
        Assert.Contains(HistoryAction.RevisionRequested, historyActions);
        Assert.Contains(HistoryAction.Resubmitted, historyActions);
    }

    [Fact]
    public void History_Entries_ShouldHaveUserIdAndTimestamp()
    {
        var order = CreateOrder(50m);
        var entry = order.History.First();

        Assert.Equal(CreatorId, entry.UserId);
        Assert.True(entry.CreatedAt <= DateTime.UtcNow);
        Assert.NotEmpty(entry.Description);
    }

    // --- RN7: Conclusão ---

    [Fact]
    public async Task ApproveAsync_LastLevel_Supplies_ShouldSetApproved()
    {
        var order = CreateSubmittedOrder(50m);
        SetupRepo(order);

        await _service.ApproveAsync(order.Id, new ApprovalActionRequest
        {
            UserId = SuppliesUserId,
            ApproverLevel = ApprovalLevel.Supplies
        });

        Assert.Equal(OrderStatus.Approved, order.Status);
    }

    [Fact]
    public async Task ApproveAsync_LastLevel_Manager_ShouldSetApproved()
    {
        var order = CreateSubmittedOrder(500m);
        SetupRepo(order);

        await _service.ApproveAsync(order.Id, new ApprovalActionRequest
        {
            UserId = SuppliesUserId,
            ApproverLevel = ApprovalLevel.Supplies
        });

        await _service.ApproveAsync(order.Id, new ApprovalActionRequest
        {
            UserId = ManagerUserId,
            ApproverLevel = ApprovalLevel.Manager
        });

        Assert.Equal(OrderStatus.Approved, order.Status);
    }

    [Fact]
    public async Task ApproveAsync_LastLevel_Director_ShouldSetApproved()
    {
        var order = CreateSubmittedOrder(2000m);
        SetupRepo(order);

        await _service.ApproveAsync(order.Id, new ApprovalActionRequest
        {
            UserId = SuppliesUserId,
            ApproverLevel = ApprovalLevel.Supplies
        });

        await _service.ApproveAsync(order.Id, new ApprovalActionRequest
        {
            UserId = ManagerUserId,
            ApproverLevel = ApprovalLevel.Manager
        });

        await _service.ApproveAsync(order.Id, new ApprovalActionRequest
        {
            UserId = DirectorUserId,
            ApproverLevel = ApprovalLevel.Director
        });

        Assert.Equal(OrderStatus.Approved, order.Status);
    }

    // --- RN8: Cancelamento ---

    [Fact]
    public async Task CancelAsync_ShouldSetCancelledStatus()
    {
        var order = CreateSubmittedOrder(500m);
        SetupRepo(order);

        await _service.CancelAsync(order.Id, new ApprovalActionRequest
        {
            UserId = SuppliesUserId,
            ApproverLevel = ApprovalLevel.Supplies,
            Comments = "Pedido duplicado"
        });

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task CancelAsync_ByAnyLevel_ShouldWork()
    {
        var order = CreateSubmittedOrder(2000m);
        SetupRepo(order);

        // Aprovar Supplies
        await _service.ApproveAsync(order.Id, new ApprovalActionRequest
        {
            UserId = SuppliesUserId,
            ApproverLevel = ApprovalLevel.Supplies
        });

        // Manager cancela
        await _service.CancelAsync(order.Id, new ApprovalActionRequest
        {
            UserId = ManagerUserId,
            ApproverLevel = ApprovalLevel.Manager,
            Comments = "Orçamento cancelado"
        });

        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }

    [Fact]
    public async Task CancelAsync_WithoutReason_ShouldThrowException()
    {
        var order = CreateSubmittedOrder(500m);
        SetupRepo(order);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CancelAsync(order.Id, new ApprovalActionRequest
            {
                UserId = SuppliesUserId,
                ApproverLevel = ApprovalLevel.Supplies,
                Comments = ""
            }));
    }

    [Fact]
    public async Task CancelAsync_WhenNotAwaitingApproval_ShouldThrowException()
    {
        var order = CreateOrder(50m); // Draft
        SetupRepo(order);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CancelAsync(order.Id, new ApprovalActionRequest
            {
                UserId = SuppliesUserId,
                ApproverLevel = ApprovalLevel.Supplies,
                Comments = "Motivo"
            }));
    }

    // --- UpdateItems ---

    [Fact]
    public async Task UpdateAsync_InDraft_ShouldUpdateItemsAndRecalculateTotal()
    {
        var order = CreateOrder(50m);
        SetupRepo(order);

        var request = new UpdatePurchaseOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new() { ProductName = "Novo item", Quantity = 3, UnitPrice = 200m }
            }
        };

        await _service.UpdateAsync(order.Id, request);

        Assert.Single(order.Items);
        Assert.Equal(600m, order.TotalValue);
    }

    [Fact]
    public async Task UpdateAsync_WhenAwaitingApproval_ShouldThrowException()
    {
        var order = CreateSubmittedOrder(50m);
        SetupRepo(order);

        var request = new UpdatePurchaseOrderRequest
        {
            Items = new List<OrderItemRequest>
            {
                new() { ProductName = "Item", Quantity = 1, UnitPrice = 100m }
            }
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateAsync(order.Id, request));
    }
}
