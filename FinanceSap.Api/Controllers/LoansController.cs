using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FinanceSap.Api.Extensions;
using FinanceSap.Application.Commands;
using FinanceSap.Application.Queries;
using FinanceSap.Domain.Common;
using FinanceSap.Domain.Entities;
using FinanceSap.Api.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace FinanceSap.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [EnableRateLimiting(ApiServiceExtensions.GlobalRateLimitPolicy)]
    public class LoansController : ControllerBase
    {
        private readonly IMediator _mediator;

        public LoansController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Creates a new loan request.
        /// Returns 201 Created with the loan on success, or appropriate error response on failure.
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateLoan([FromBody] CreateLoanCommand command)
        {
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(new { message = result.Error }),
                    ErrorType.Validation => BadRequest(new { message = result.Error }),
                    ErrorType.Conflict => Conflict(new { message = result.Error }),
                    _ => StatusCode(500, new { message = "Erro interno do servidor." })
                };
            }

            // At this point, result.Value is guaranteed to be non-null
            var loan = result.Value!;
            return CreatedAtAction(nameof(GetLoan), new { id = loan.Id }, loan);
        }

        /// <summary>
        /// Retrieves a loan by ID with IDOR protection.
        /// Users can only access their own loans unless they have Admin/Analyst role.
        /// </summary>
        [HttpGet("{id:guid}")]
        [Authorize]
        public async Task<IActionResult> GetLoan([FromRoute] Guid id)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            bool isAdmin = User.IsInRole("Admin") || User.IsInRole("Analyst");

            var query = new GetLoanByIdQuery(id, userId.Value, isAdmin);
            var result = await _mediator.Send(query);

            if (!result.IsSuccess)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(new { message = result.Error }),
                    ErrorType.Validation => BadRequest(new { message = result.Error }),
                    _ => StatusCode(500, new { message = "Erro interno do servidor." })
                };
            }

            return Ok(result.Value);
        }

        /// <summary>
        /// Approves a loan request.
        /// </summary>
        [HttpPut("{id}/approve")]
        [Authorize]
        public async Task<IActionResult> ApproveLoan([FromRoute] Guid id)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var command = new ApproveLoanCommand(id, userId.Value);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(new { message = result.Error }),
                    ErrorType.Validation => BadRequest(new { message = result.Error }),
                    ErrorType.Conflict => Conflict(new { message = result.Error }),
                    _ => StatusCode(500, new { message = "Erro interno do servidor." })
                };
            }

            return NoContent();
        }

        /// <summary>
        /// Rejects a loan request.
        /// </summary>
        [HttpPut("{id}/reject")]
        [Authorize]
        public async Task<IActionResult> RejectLoan([FromRoute] Guid id, [FromBody] RejectLoanRequest? request = null)
        {
            var userId = GetUserId();
            if (userId is null) return Unauthorized();

            var reason = request?.Reason;
            var command = new RejectLoanCommand(id, userId.Value, reason);
            var result = await _mediator.Send(command);

            if (!result.IsSuccess)
            {
                return result.ErrorType switch
                {
                    ErrorType.NotFound => NotFound(new { message = result.Error }),
                    ErrorType.Validation => BadRequest(new { message = result.Error }),
                    ErrorType.Conflict => Conflict(new { message = result.Error }),
                    _ => StatusCode(500, new { message = "Erro interno do servidor." })
                };
            }

            return NoContent();
        }

        private Guid? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                        ?? User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }
}