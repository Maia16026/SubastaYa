using Domain.Exceptions;
using System.Text.Json;

namespace Api.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ConcurrencyException ex)
        {
            await ManejarExcepcion(
                context,
                StatusCodes.Status409Conflict,
                ex.Message);
        }
        catch (DomainException ex)
        {
            await ManejarExcepcion(
                context,
                StatusCodes.Status400BadRequest,
                ex.Message);
        }
        catch (NotFoundException ex)
        {
            await ManejarExcepcion(
                context,
                StatusCodes.Status404NotFound,
                ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await ManejarExcepcion(
                context,
                StatusCodes.Status409Conflict,
                ex.Message);
        }
        catch (Exception)
        {
            await ManejarExcepcion(
                context,
                StatusCodes.Status500InternalServerError,
                "Ocurrió un error interno en el servidor.");
        }
    }

    private static async Task ManejarExcepcion(
        HttpContext context,
        int statusCode,
        string mensaje)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        var respuesta = new
        {
            error = mensaje
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(respuesta));
    }
}