namespace Hhs.Shared.Helper.Consts;

public static class DefaultRoleNames
{
    public const string SystemAdmin = $"{DefaultDomainNames.System}-{IdentityConsts.Admin}";
    public const string SystemManager = $"{DefaultDomainNames.System}-{IdentityConsts.Manager}";
    public const string SystemUser = $"{DefaultDomainNames.System}-{IdentityConsts.User}";
    public const string AppUser = $"{DefaultDomainNames.PublicApp}-registered";
    public const string TenantManager = $"tenant-{IdentityConsts.Manager}";
    public const string TenantUser = $"tenant-{IdentityConsts.User}";
}