namespace GestVta.Services.Dtos;

public sealed record AccessTokenResult(string Token, DateTime ExpiresAtUtc);
