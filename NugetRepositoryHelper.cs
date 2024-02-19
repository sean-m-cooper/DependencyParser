using NuGet.Common;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace DependencyParser
{
    public class NugetRepositoryHelper
    {
        private ILogger _logger { get; set; }
        private CancellationToken _cancellationToken { get; set; }
        private SourceCacheContext _cache { get; set; }
        private SourceRepository _repository { get; set; }

        public NugetRepositoryHelper()
        {
            _logger = NullLogger.Instance;
            _cancellationToken = CancellationToken.None;
            _cache = new SourceCacheContext();

            SetRepository();
        }

        private void SetRepository()
        {
            _repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");
        }

        public async Task<IEnumerable<NuGetVersion>> GetPackageVersionsAsync( string packageName)
        {
            FindPackageByIdResource resource = await _repository.GetResourceAsync<FindPackageByIdResource>();
            return await resource.GetAllVersionsAsync(packageName, _cache, _logger, _cancellationToken);
        }
    }
}
