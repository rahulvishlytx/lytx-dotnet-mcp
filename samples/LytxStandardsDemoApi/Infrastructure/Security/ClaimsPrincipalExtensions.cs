using System.Security.Claims;

namespace LytxStandardsDemoApi.Infrastructure.Security;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUniqueId(this ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var value)
            ? value
            : Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

    public static Guid GetRootGroupId(this ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirstValue("root_group_id"), out var value)
            ? value
            : Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static int GetCompany(this ClaimsPrincipal principal) =>
        int.TryParse(principal.FindFirstValue("company_id"), out var value)
            ? value
            : 101;
}
