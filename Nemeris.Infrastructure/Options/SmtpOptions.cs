namespace Nemeris.Infrastructure.Options;

/// <summary>Bound from the "Smtp" section. When Host is empty, EmailService logs instead of sending (dev mode).</summary>
public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public bool UseStartTls { get; set; } = true;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromAddress { get; set; } = "noreply@nemeris.example";

    public string FromName { get; set; } = "Nemeris";
}
