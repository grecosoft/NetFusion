using RefArch.Api.Models;

namespace RefArch.Domain.Samples.WebApi
{
    public interface IPrincipalDependentService
    {
        UserInfo AccessedPrincipal();
    }
}
