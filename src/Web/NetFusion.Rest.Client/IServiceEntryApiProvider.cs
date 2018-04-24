using NetFusion.Rest.Client.Resources;
using System.Threading.Tasks;

namespace NetFusion.Rest.Client
{
    public interface IServiceEntryApiProvider
    {
        Task<HalEntryPointResource> GetEntryPointResource();
    }
}
