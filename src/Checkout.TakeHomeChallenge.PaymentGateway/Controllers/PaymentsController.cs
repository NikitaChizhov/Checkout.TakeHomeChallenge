using Checkout.TakeHomeChallenge.Contracts;
using Checkout.TakeHomeChallenge.Contracts.Requests;
using Checkout.TakeHomeChallenge.Contracts.Requests.SupportingTypes;
using Checkout.TakeHomeChallenge.Contracts.Responses;
using Checkout.TakeHomeChallenge.PaymentGateway.Controllers.Logic;
using Checkout.TakeHomeChallenge.PaymentGateway.Exceptions;
using Checkout.TakeHomeChallenge.PaymentGateway.Model.Application;
using Microsoft.AspNetCore.Mvc;

namespace Checkout.TakeHomeChallenge.PaymentGateway.Controllers;

[ApiController]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentControllerLogic _logic;

    public PaymentsController(IPaymentControllerLogic logic)
    {
        _logic = logic;
    }

    // TODO: annotate all errors (400, 502, ..?) + make sure everything returned as problemDetails
    [HttpPost(Routes.PaymentsResource.Base, Name = nameof(MakePayment))]
    [ProducesResponseType(typeof(PaymentAcceptedResponse), 202)]
    public async Task<IActionResult> MakePayment([FromBody] PaymentRequest request)
    {
        var result = await _logic.MakePaymentAsync(request);
        return result.Success ? Accepted(result.Value) : NotFoundOrThrow(result);
    }

    [HttpGet(Routes.PaymentsResource.WithId, Name = nameof(GetPayment))]
    [ProducesResponseType(typeof(PaymentProcessedResponse), 200)]
    public async Task<IActionResult> GetPayment(
        [FromRoute(Name = Routes.PaymentsResource.WithIdParamName)] PaymentId paymentId,
        CancellationToken cancellationToken)
    {
        var result = await _logic.GetPaymentAsync(paymentId, cancellationToken);
        return result.Success ? Ok(result.Value) : NotFoundOrThrow(result);
    }

    private IActionResult NotFoundOrThrow<T>(Result<T> result)
    {
        if (result.Code is FailureCode.NotFound) return NotFound(result.Reason);
        
        throw new PaymentGatewayException(result.Reason);
    }
}