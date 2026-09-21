namespace Cuidanet.Services;

/// <summary>Marca visible (nombre y logo). Los colores se aplican en brand.js antes del primer pintado.</summary>
public sealed class BrandProfile
{
    public string Id { get; private set; } = "cuidamed";

    public string Name { get; private set; } = "Cuidanet";

    public string Logo { get; private set; } = "LogoCuidaNet.png";

    public string LogoAlt { get; private set; } = "CuidaNet";

    public string Slogan { get; private set; } = "Lo que necesitas en un solo lugar";

    public string Hero { get; private set; } = "img/cuidamed-login-completo.png";

    public string HomeImage { get; private set; } = "img/home-desktop.jpg";

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
        if (CleanLabel(snapshot.Slogan, 60) is string slogan)
            Slogan = slogan;
        if (SafeAsset(snapshot.Hero) is string hero)
            Hero = hero;
        if (SafeAsset(snapshot.HomeImage) is string home)
            HomeImage = home;
    }

    private static string? CleanLabel(string? value, int max = 40)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        if (trimmed.Length < 1 || trimmed.Length > max)
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

    public string? Slogan { get; set; }

    public string? Hero { get; set; }

    public string? HomeImage { get; set; }
}
