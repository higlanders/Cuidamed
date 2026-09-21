namespace Cuidanet.Services;

/// <summary>Marca visible (nombre y logo). Los colores se aplican en brand.js antes del primer pintado.</summary>
public sealed class BrandProfile
{
    public string Id { get; private set; } = "cuidamed";

    public string Name { get; private set; } = "Cuidanet";

    public string Logo { get; private set; } = "LogoCuidaNet.png";

    public string LogoAlt { get; private set; } = "CuidaNet";

    public string Title(string? page = null) =>
        string.IsNullOrWhiteSpace(page) ? Name : $"{Name} — {page.Trim()}";

    public void Apply(BrandSnapshot? snapshot)
    {
        if (snapshot?.Id is not ("cuidamed" or "latitud"))
            return;

        Id = snapshot.Id;
        if (CleanLabel(snapshot.Name) is string name)
            Name = name;
        if (CleanLabel(snapshot.LogoAlt) is string alt)
            LogoAlt = alt;
        if (SafeAsset(snapshot.Logo) is string logo)
            Logo = logo;
    }

    private static string? CleanLabel(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        if (trimmed.Length is < 1 or > 40)
            return null;

        foreach (var c in trimmed)
        {
            if (char.IsLetterOrDigit(c) || c is ' ' or '.' or '-')
                continue;
            return null;
        }

        return trimmed;
    }

    private static string? SafeAsset(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var trimmed = path.Trim();
        if (trimmed.Length > 120 || trimmed.Contains("..", StringComparison.Ordinal))
            return null;

        foreach (var c in trimmed)
        {
            if (char.IsLetterOrDigit(c) || c is '/' or '.' or '-' or '_')
                continue;
            return null;
        }

        return trimmed;
    }
}

public sealed class BrandSnapshot
{
    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? Logo { get; set; }

    public string? LogoAlt { get; set; }
}
