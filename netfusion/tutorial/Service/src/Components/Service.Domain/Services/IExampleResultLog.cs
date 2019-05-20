namespace Service.Domain.Services
{
    public interface IExampleResultLog
    {
        string[] GetLogs();
        void AddResult(string value);
    }
}