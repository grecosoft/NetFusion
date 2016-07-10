using RefArch.Domain.Samples.Settings;
using System.Threading.Tasks;

namespace RefArch.Domain.Samples.MongoDb
{
    public interface ISettingsInitService
    {
        Task<MongoInitializedSettings> InitMongoDbStoredSettings();
    }
}
