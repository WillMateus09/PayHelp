using System;

namespace PayHelp.Client;

public class ApiUnauthorizedException : Exception
{
    public int? StatusCode { get; }

    public ApiUnauthorizedException(string? message = null, int? statusCode = null)
        : base(message ?? "Acesso não autorizado à API (401/403).")
    {
        StatusCode = statusCode;
    }
}
