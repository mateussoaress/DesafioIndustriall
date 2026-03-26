using PurchaseOrderApi.Application.DTOs.Requests;
using PurchaseOrderApi.Application.DTOs.Responses;
using PurchaseOrderApi.Domain.Entities;
using PurchaseOrderApi.Domain.Enums;
using PurchaseOrderApi.Domain.Interfaces;

namespace PurchaseOrderApi.Application.Services;

/// <summary>
/// Serviço de aplicação para pedidos de compra.
/// Contém todas as regras de negócio do fluxo de aprovação hierárquica (RN1 a RN8).
/// As entidades são anêmicas — apenas carregam dados.
/// </summary>
public class PurchaseOrderService : IPurchaseOrderService
{
    private readonly IPurchaseOrderRepository _repository;
    private readonly ILogger<PurchaseOrderService> _logger;

    public PurchaseOrderService(IPurchaseOrderRepository repository, ILogger<PurchaseOrderService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Cria um novo pedido de compra no estado Draft (RN1, RN2, RN6).
    /// </summary>
    public async Task<PurchaseOrderResponse> CreateAsync(CreatePurchaseOrderRequest request)
    {
        var items = request.Items
            .Select(i => new OrderItem(i.ProductName, i.Quantity, i.UnitPrice))
            .ToList();

        var order = new PurchaseOrder(request.CreatorUserId, items);

        // RN6: Registra criação no histórico
        order.History.Add(new OrderHistory(HistoryAction.Created, request.CreatorUserId, "Pedido de compra criado."));

        await _repository.AddAsync(order);

        _logger.LogInformation("Pedido de compra {PurchaseOrderId} criado pelo usuário {CreatorUserId}. Valor total: {TotalValue:C}",
            order.Id, order.CreatorUserId, order.TotalValue);

        return await ReloadAndRespond(order.Id);
    }

    /// <summary>
    /// Retorna os dados completos de um pedido pelo identificador.
    /// </summary>
    public async Task<PurchaseOrderResponse> GetByIdAsync(Guid id)
    {
        var order = await GetOrderOrThrowAsync(id);
        return PurchaseOrderResponse.FromEntity(order);
    }

    /// <summary>
    /// Retorna todos os pedidos de compra ordenados pela data de criação.
    /// </summary>
    public async Task<IEnumerable<PurchaseOrderResponse>> GetAllAsync()
    {
        var orders = await _repository.GetAllAsync();
        return orders.Select(PurchaseOrderResponse.FromEntity);
    }

    /// <summary>
    /// Atualiza os itens de um pedido em rascunho ou revisão (RN2).
    /// </summary>
    public async Task<PurchaseOrderResponse> UpdateAsync(Guid id, UpdatePurchaseOrderRequest request)
    {
        var order = await GetOrderOrThrowAsync(id);

        if (order.Status != OrderStatus.Draft && order.Status != OrderStatus.UnderReview)
            throw new InvalidOperationException(
                $"Não é possível atualizar o pedido. Status atual: {order.Status}. Permitido apenas em Draft ou UnderReview.");

        var items = request.Items
            .Select(i => new OrderItem(i.ProductName, i.Quantity, i.UnitPrice))
            .ToList();

        if (items.Count == 0)
            throw new ArgumentException("O pedido deve conter pelo menos 1 item.");

        order.Items.Clear();
        order.Items.AddRange(items);
        order.TotalValue = items.Sum(i => i.TotalPrice);

        await _repository.UpdateAsync(order);

        _logger.LogInformation("Pedido de compra {PurchaseOrderId} atualizado. Novo valor total: {TotalValue:C}",
            order.Id, order.TotalValue);

        return await ReloadAndRespond(order.Id);
    }

    /// <summary>
    /// Envia o pedido para aprovação, criando a cadeia de aprovação conforme alçada (RN3, RN4, RN6).
    /// </summary>
    public async Task<PurchaseOrderResponse> SubmitAsync(Guid id, SubmitRequest request)
    {
        var order = await GetOrderOrThrowAsync(id);

        ValidateTransition(order, OrderStatus.Draft, "enviar para aprovação");
        ValidateCreator(order, request.UserId, "Apenas o elaborador pode enviar o pedido para aprovação.");

        order.Status = OrderStatus.AwaitingApproval;

        // RN3: Cria cadeia de aprovação conforme alçada de valor
        order.Approvals.Clear();
        CreateApprovalChain(order);

        // RN6: Registra no histórico
        order.History.Add(new OrderHistory(HistoryAction.Submitted, request.UserId, "Pedido enviado para aprovação."));

        await _repository.UpdateAsync(order);

        _logger.LogInformation("Pedido {PurchaseOrderId} enviado para aprovação por {UserId}. Alçada: {Level}",
            order.Id, request.UserId, GetRequiredApprovalLevel(order.TotalValue));

        return await ReloadAndRespond(order.Id);
    }

    /// <summary>
    /// Aprova o pedido no nível de aprovação atual (RN4).
    /// Se todas as aprovações foram obtidas, conclui o pedido (RN7).
    /// </summary>
    public async Task<PurchaseOrderResponse> ApproveAsync(Guid id, ApprovalActionRequest request)
    {
        var order = await GetOrderOrThrowAsync(id);

        ValidateTransition(order, OrderStatus.AwaitingApproval, "aprovar");

        // RN4: Aprovação sequencial — valida o nível correto
        var currentApproval = GetCurrentPendingApproval(order);
        if (currentApproval.Level != request.ApproverLevel)
            throw new InvalidOperationException(
                $"Aprovação fora de ordem. Nível esperado: {currentApproval.Level}. Nível informado: {request.ApproverLevel}.");

        // Atualiza a aprovação
        currentApproval.Status = ApprovalStatus.Approved;
        currentApproval.ApproverUserId = request.UserId;
        currentApproval.ApprovalDate = DateTime.UtcNow;
        currentApproval.Comments = request.Comments?.Trim();

        // RN6: Registra no histórico
        order.History.Add(new OrderHistory(HistoryAction.Approved, request.UserId,
            $"Pedido aprovado pelo nível: {request.ApproverLevel}."));

        // RN7: Verifica se todas as aprovações foram obtidas
        var nextPending = order.Approvals.FirstOrDefault(a => a.Status == ApprovalStatus.Pending);
        if (nextPending == null)
        {
            order.Status = OrderStatus.Approved;
            order.History.Add(new OrderHistory(HistoryAction.Approved, request.UserId,
                "Pedido concluído — todas as aprovações obtidas."));
        }

        await _repository.UpdateAsync(order);

        _logger.LogInformation("Pedido {PurchaseOrderId} aprovado no nível {Level} por {UserId}. Status: {Status}",
            order.Id, request.ApproverLevel, request.UserId, order.Status);

        return await ReloadAndRespond(order.Id);
    }

    /// <summary>
    /// Solicita revisão do pedido, devolvendo ao elaborador (RN5).
    /// </summary>
    public async Task<PurchaseOrderResponse> RequestRevisionAsync(Guid id, ApprovalActionRequest request)
    {
        var order = await GetOrderOrThrowAsync(id);

        ValidateTransition(order, OrderStatus.AwaitingApproval, "solicitar revisão");

        if (string.IsNullOrWhiteSpace(request.Comments))
            throw new InvalidOperationException("O motivo da revisão é obrigatório.");

        // RN4: Valida o nível correto
        var currentApproval = GetCurrentPendingApproval(order);
        if (currentApproval.Level != request.ApproverLevel)
            throw new InvalidOperationException(
                $"Aprovação fora de ordem. Nível esperado: {currentApproval.Level}. Nível informado: {request.ApproverLevel}.");

        // Atualiza a aprovação
        currentApproval.Status = ApprovalStatus.RevisionRequested;
        currentApproval.ApproverUserId = request.UserId;
        currentApproval.ApprovalDate = DateTime.UtcNow;
        currentApproval.Comments = request.Comments.Trim();

        // RN5: Retorna ao elaborador
        order.Status = OrderStatus.UnderReview;

        // RN6: Registra no histórico
        order.History.Add(new OrderHistory(HistoryAction.RevisionRequested, request.UserId,
            $"Revisão solicitada pelo nível {request.ApproverLevel}. Motivo: {request.Comments.Trim()}"));

        await _repository.UpdateAsync(order);

        _logger.LogInformation("Revisão solicitada para pedido {PurchaseOrderId} pelo nível {Level}",
            order.Id, request.ApproverLevel);

        return await ReloadAndRespond(order.Id);
    }

    /// <summary>
    /// Reenvia o pedido após revisão, reiniciando toda a cadeia de aprovação (RN5).
    /// </summary>
    public async Task<PurchaseOrderResponse> ResubmitAsync(Guid id, ResubmitRequest request)
    {
        var order = await GetOrderOrThrowAsync(id);

        ValidateTransition(order, OrderStatus.UnderReview, "reenviar");
        ValidateCreator(order, request.UserId, "Apenas o elaborador pode reenviar o pedido após revisão.");

        // Atualiza itens se fornecidos
        if (request.Items != null && request.Items.Count > 0)
        {
            var updatedItems = request.Items
                .Select(i => new OrderItem(i.ProductName, i.Quantity, i.UnitPrice))
                .ToList();

            order.Items.Clear();
            order.Items.AddRange(updatedItems);
        }

        if (order.Items.Count == 0)
            throw new ArgumentException("O pedido deve conter pelo menos 1 item.");

        // RN2: Recalcula valor total
        order.TotalValue = order.Items.Sum(i => i.TotalPrice);

        // RN5: Reinicia cadeia de aprovação
        order.Status = OrderStatus.AwaitingApproval;
        order.Approvals.Clear();
        CreateApprovalChain(order);

        // RN6: Registra no histórico
        order.History.Add(new OrderHistory(HistoryAction.Resubmitted, request.UserId,
            "Pedido reenviado após revisão — fluxo de aprovação reiniciado."));

        await _repository.UpdateAsync(order);

        _logger.LogInformation("Pedido {PurchaseOrderId} reenviado por {UserId}. Fluxo de aprovação reiniciado.",
            order.Id, request.UserId);

        return await ReloadAndRespond(order.Id);
    }

    /// <summary>
    /// Cancela o pedido de compra (RN8).
    /// </summary>
    public async Task<PurchaseOrderResponse> CancelAsync(Guid id, ApprovalActionRequest request)
    {
        var order = await GetOrderOrThrowAsync(id);

        ValidateTransition(order, OrderStatus.AwaitingApproval, "cancelar");

        if (string.IsNullOrWhiteSpace(request.Comments))
            throw new InvalidOperationException("O motivo do cancelamento é obrigatório.");

        // RN4: Valida o nível correto
        var currentApproval = GetCurrentPendingApproval(order);
        if (currentApproval.Level != request.ApproverLevel)
            throw new InvalidOperationException(
                $"Aprovação fora de ordem. Nível esperado: {currentApproval.Level}. Nível informado: {request.ApproverLevel}.");

        // Atualiza a aprovação
        currentApproval.Status = ApprovalStatus.Rejected;
        currentApproval.ApproverUserId = request.UserId;
        currentApproval.ApprovalDate = DateTime.UtcNow;
        currentApproval.Comments = request.Comments.Trim();

        // RN8: Cancela o pedido
        order.Status = OrderStatus.Cancelled;

        // RN6: Registra no histórico
        order.History.Add(new OrderHistory(HistoryAction.Cancelled, request.UserId,
            $"Pedido cancelado pelo nível {request.ApproverLevel}. Motivo: {request.Comments.Trim()}"));

        await _repository.UpdateAsync(order);

        _logger.LogInformation("Pedido {PurchaseOrderId} cancelado pelo nível {Level}",
            order.Id, request.ApproverLevel);

        return await ReloadAndRespond(order.Id);
    }

    // --- Regras de negócio auxiliares ---

    /// <summary>
    /// Retorna o nível máximo de aprovação exigido com base no valor total (RN3).
    /// </summary>
    public static ApprovalLevel GetRequiredApprovalLevel(decimal totalValue)
    {
        return totalValue switch
        {
            <= 100m => ApprovalLevel.Supplies,
            <= 1000m => ApprovalLevel.Manager,
            _ => ApprovalLevel.Director
        };
    }

    /// <summary>
    /// Cria as etapas de aprovação conforme a alçada de valor (RN3).
    /// </summary>
    private static void CreateApprovalChain(PurchaseOrder order)
    {
        var requiredLevel = GetRequiredApprovalLevel(order.TotalValue);

        order.Approvals.Add(new Approval(ApprovalLevel.Supplies));

        if (requiredLevel >= ApprovalLevel.Manager)
            order.Approvals.Add(new Approval(ApprovalLevel.Manager));

        if (requiredLevel >= ApprovalLevel.Director)
            order.Approvals.Add(new Approval(ApprovalLevel.Director));
    }

    /// <summary>
    /// Retorna a próxima aprovação pendente na cadeia sequencial.
    /// </summary>
    private static Approval GetCurrentPendingApproval(PurchaseOrder order)
    {
        var pending = order.Approvals
            .OrderBy(a => a.Level)
            .FirstOrDefault(a => a.Status == ApprovalStatus.Pending);

        if (pending == null)
            throw new InvalidOperationException("Não há aprovação pendente para este pedido.");

        return pending;
    }

    /// <summary>
    /// Valida se o pedido está no status esperado para a transição solicitada.
    /// </summary>
    private static void ValidateTransition(PurchaseOrder order, OrderStatus expectedStatus, string action)
    {
        if (order.Status != expectedStatus)
            throw new InvalidOperationException(
                $"Não é possível {action} o pedido. Status atual: {order.Status}. Status esperado: {expectedStatus}.");
    }

    /// <summary>
    /// Valida se a ação está sendo executada pelo criador do pedido.
    /// </summary>
    private static void ValidateCreator(PurchaseOrder order, Guid userId, string errorMessage)
    {
        if (order.CreatorUserId != userId)
            throw new InvalidOperationException(errorMessage);
    }

    private async Task<PurchaseOrder> GetOrderOrThrowAsync(Guid id)
    {
        var order = await _repository.GetByIdAsync(id);
        if (order == null)
            throw new KeyNotFoundException($"Pedido de compra com ID '{id}' não encontrado.");
        return order;
    }

    private async Task<PurchaseOrderResponse> ReloadAndRespond(Guid id)
    {
        var order = await _repository.GetByIdAsync(id);
        return PurchaseOrderResponse.FromEntity(order!);
    }
}
