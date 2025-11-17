using FlashEng.Domain.Exceptions;
using System.Net;
using System.Text.Json;

namespace FlashEng.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var problemDetails = new ProblemDetails();

            switch (exception)
            {
                case NotFoundException notFoundEx:
                    problemDetails.Status = (int)HttpStatusCode.NotFound;
                    problemDetails.Title = "Resource not found";
                    problemDetails.Detail = notFoundEx.Message;
                    break;

                case ValidationException validationEx:
                    problemDetails.Status = (int)HttpStatusCode.BadRequest;
                    problemDetails.Title = "Validation error";
                    problemDetails.Detail = validationEx.Message;
                    break;

                case BusinessConflictException conflictEx:
                    problemDetails.Status = (int)HttpStatusCode.Conflict;
                    problemDetails.Title = "Business rule violation";
                    problemDetails.Detail = conflictEx.Message;
                    break;

                default:
                    problemDetails.Status = (int)HttpStatusCode.InternalServerError;
                    problemDetails.Title = "An error occurred";
                    problemDetails.Detail = "An unexpected error occurred. Please try again later.";
                    break;
            }

            problemDetails.Instance = context.Request.Path;
            problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";

            context.Response.StatusCode = problemDetails.Status.Value;

            var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }

    public class ProblemDetails
    {
        public string? Type { get; set; }
        public string? Title { get; set; }
        public int? Status { get; set; }
        public string? Detail { get; set; }
        public string? Instance { get; set; }
    }
}
