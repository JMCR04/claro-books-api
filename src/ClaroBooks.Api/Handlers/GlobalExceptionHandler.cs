using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;

namespace ClaroBooks.Api.Handlers;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title, detail) = exception switch
        {
            HttpRequestException
            {
                StatusCode: HttpStatusCode.NotFound
            } => (
                StatusCodes.Status404NotFound,
                "Recurso no encontrado",
                "El recurso solicitado no existe en la API externa."),

            HttpRequestException => (
                StatusCodes.Status502BadGateway,
                "Error de comunicación",
                "No se pudo completar la solicitud a la API externa."),

            JsonException => (
                StatusCodes.Status502BadGateway,
                "Respuesta externa inválida",
                "La API externa devolvió datos con un formato inesperado."),

            OperationCanceledException
                when !httpContext.RequestAborted.IsCancellationRequested => (
                    StatusCodes.Status504GatewayTimeout,
                    "Tiempo de espera agotado",
                    "La API externa tardó demasiado en responder."),

            _ => (
                StatusCodes.Status500InternalServerError,
                "Error interno",
                "Ocurrió un error inesperado al procesar la solicitud.")
        };

        if (statusCode >= 500)
        {
            _logger.LogError(
                exception,
                "Error al procesar {Method} {Path}",
                httpContext.Request.Method,
                httpContext.Request.Path);
        }

        await Results.Problem(
            statusCode: statusCode,
            title: title,
            detail: detail,
            instance: httpContext.Request.Path.Value)
            .ExecuteAsync(httpContext);

        return true;
    }
}