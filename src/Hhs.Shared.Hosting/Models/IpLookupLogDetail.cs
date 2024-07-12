namespace Hhs.Shared.Hosting.Models;

public sealed class IpLookupLogDetail
{
    public string? status { get; set; }
    public string? country { get; set; }
    public string? region { get; set; }
    public string? regionName { get; set; }
    public string? city { get; set; }
    public string? zip { get; set; }
    public double? lat { get; set; }
    public double? lon { get; set; }
    public string? isp { get; set; }
    public string? asname { get; set; }
    public string? reverse { get; set; }
    public bool? mobile { get; set; }
    public bool? proxy { get; set; }
    public bool? hosting { get; set; }
    public string? query { get; set; }
}