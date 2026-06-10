namespace Tebloqueo;

internal enum Isp
{
    Todos,
    Movistar,
    Digi,
    Vodafone,
    Orange,
    Masmovil
}

internal static class IspCatalog
{
    public static readonly IReadOnlyList<Isp> All =
    [
        Isp.Todos,
        Isp.Movistar,
        Isp.Digi,
        Isp.Vodafone,
        Isp.Orange,
        Isp.Masmovil
    ];

    public static string DisplayName(Isp isp) => isp switch
    {
        Isp.Digi => "DIGI",
        _ => isp.ToString()
    };

    public static string Endpoint(Isp isp) => isp switch
    {
        Isp.Todos => "https://hayahora.futbol/estado/blocked-any.txt",
        Isp.Movistar => "https://hayahora.futbol/estado/blocked-movistar.txt",
        Isp.Digi => "https://hayahora.futbol/estado/blocked-digi.txt",
        Isp.Vodafone => "https://hayahora.futbol/estado/blocked-vodafone.txt",
        Isp.Orange => "https://hayahora.futbol/estado/blocked-orange.txt",
        Isp.Masmovil => "https://hayahora.futbol/estado/blocked-masmovil.txt",
        _ => throw new ArgumentOutOfRangeException(nameof(isp), isp, null)
    };
}
