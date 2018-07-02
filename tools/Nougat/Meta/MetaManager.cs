using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;  
using Nougat.Projects;

namespace Nougat.Meta
{
    public class MetaManager : IMetaManager
    {
        private ILogger Logger { get; }
        public HttpClient NugetClient { get; }

        private MetaManager(ILoggerFactory loggerFactory, HttpClient client)
        {
            Logger = loggerFactory.CreateLogger<MetaManager>();
            NugetClient = client;
        }

        public static IMetaManager Create(ILoggerFactory loggerFactory, string searchUrl) 
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));
            if (searchUrl == null) throw new ArgumentNullException(nameof(searchUrl));

            var client = new HttpClient();
            client.BaseAddress = new Uri(searchUrl);

            return new MetaManager(loggerFactory, client); 
        }

        public async Task SetPackageMeta(IEnumerable<Package> installedPackages)
        {
            if (installedPackages == null)
                throw new ArgumentNullException(nameof(installedPackages));

            Logger.LogInformation("Searching Nuget Server: {url}", NugetClient.BaseAddress);

            var requests = GetPackageMetaRequests(installedPackages);
            await Task.WhenAll(requests);

            await SetPackageMeta(installedPackages, requests);
        }

        // Issues a search to the Nuget server for the package metadata.
        private Task<HttpResponseMessage>[] GetPackageMetaRequests(IEnumerable<Package> installedPackages)
        {
            var requests = new List<Task<HttpResponseMessage>>();

            foreach(var package in installedPackages)
            {
               var task = NugetClient.GetAsync($"query?q={package.Name}&prerelease=false");
                requests.Add(task);
            }

            return requests.ToArray();
        }

        private async Task SetPackageMeta(IEnumerable<Package> installedPackages,
            Task<HttpResponseMessage>[] requests)
        {
            foreach (var request in requests)
            {
                if (request.Exception != null)
                {
                    Logger.LogError(request.Exception, "Exception requesting package meta.");
                    continue;
                }

                var searchResults = JsonConvert.DeserializeObject<PackageSearchResult>(
                        await request.Result.Content.ReadAsStringAsync());

                foreach (var meta in searchResults.Data)
                {
                    var packages = installedPackages.Where(p => p.Name == meta.Id);
                    foreach (var package in packages)
                    {
                        package.LatestVersion = meta.Version;
                    }
                }
            }
        }
    }
}