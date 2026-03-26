using Microsoft.AspNetCore.Mvc;
using PurchaseOrderApi.Application.DTOs.Requests;
using PurchaseOrderApi.Application.DTOs.Responses;
using PurchaseOrderApi.Application.Services;

namespace PurchaseOrderApi.Controllers;

/// <summary>
/// Controller responsável pelos endpoints do processo de pedido de compras.
/// Segue o padrão REST com controllers finos — toda lógica está no serviço e no domínio.
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class PurchaseOrdersController : ControllerBase
{
    private readonly IPurchaseOrderService _service;

    public PurchaseOrdersController(IPurchaseOrderService service)
    {
        _service = service;
    }

    /// <summary>
    /// Cria um novo pedido de compra com os itens informados.
    /// O pedido é criado no estado Draft e precisa ser enviado para aprovação.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PurchaseOrderResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Retorna todos os pedidos de compra cadastrados.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PurchaseOrderResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    /// <summary>
    /// Retorna os dados completos de um pedido de compra pelo identificador.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PurchaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return Ok(result);
    }

    /// <summary>
    /// Atualiza a descrição e os itens de um pedido em rascunho ou revisão.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PurchaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePurchaseOrderRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return Ok(result);
    }

    /// <summary>
    /// Envia o pedido para o fluxo de aprovação.
    /// O pedido deve estar no estado Draft.
    /// </summary>
    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(PurchaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitRequest request)
    {
        var result = await _service.SubmitAsync(id, request);
        return Ok(result);
    }

    /// <summary>
    /// Aprova o pedido no nível de aprovação atual.
    /// A aprovação é sequencial — o nível informado deve corresponder ao nível pendente (RN4).
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(PurchaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Approve(Guid id, [FromBody] ApprovalActionRequest request)
    {
        var result = await _service.ApproveAsync(id, request);
        return Ok(result);
    }

    /// <summary>
    /// Solicita revisão do pedido, devolvendo ao elaborador para ajustes (RN5).
    /// O campo Comments é obrigatório para informar o motivo da revisão.
    /// </summary>
    [HttpPost("{id:guid}/request-revision")]
    [ProducesResponseType(typeof(PurchaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> RequestRevision(Guid id, [FromBody] ApprovalActionRequest request)
    {
        var result = await _service.RequestRevisionAsync(id, request);
        return Ok(result);
    }

    /// <summary>
    /// Reenvia o pedido após revisão, reiniciando toda a cadeia de aprovação (RN5).
    /// O pedido deve estar no estado UnderReview.
    /// </summary>
    [HttpPost("{id:guid}/resubmit")]
    [ProducesResponseType(typeof(PurchaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Resubmit(Guid id, [FromBody] ResubmitRequest request)
    {
        var result = await _service.ResubmitAsync(id, request);
        return Ok(result);
    }

    /// <summary>
    /// Cancela o pedido de compra. Qualquer nível de aprovação pode cancelar (RN8).
    /// O campo Comments é obrigatório para informar o motivo do cancelamento.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(PurchaseOrderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] ApprovalActionRequest request)
    {
        var result = await _service.CancelAsync(id, request);
        return Ok(result);
    }
}
