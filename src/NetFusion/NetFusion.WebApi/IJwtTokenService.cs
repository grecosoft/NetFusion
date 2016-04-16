using NetFusion.Common;

namespace NetFusion.WebApi
{
    /// <summary>
    /// Plug-in service that allows the application host to build and 
    /// verify a JWT security token.
    /// </summary>
    public interface IJwtTokenService
    {
        /// <summary>
        /// Creates a security JWT token value populated with values
        /// specified by the JWT token value providers.
        /// </summary>
        /// <returns>The encoded security token to be returned to the client.</returns>
        string CreateSecurityToken(object claimObj);

        /// <summary>
        /// Creates a claim principal from a jwt security token string.
        /// If the token is not valid, an exception is thrown.
        /// </summary>
        /// <typeparam name="TPrincipal">The type of the principal.</typeparam>
        /// <param name="jwtToken">The security token value to verify.</param>
        /// <returns>Created principal if the token is valid.</returns>
        TPrincipal CreatePrincipal<TPrincipal>(string jwtToken)
            where TPrincipal : FusionPrincipal;

        /// <summary>
        /// Creates a claim principal from a jwt security token string.
        /// If the token is not valid, an exception is thrown.
        /// </summary>
        /// <param name="jwtToken">The security token value to verify.</param>
        /// <returns>Created principal if the token is valid.</returns>
        FusionPrincipal CreatePrincipal(string jwtToken);
    }
}
