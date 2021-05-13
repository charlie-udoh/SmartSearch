using Nest;

namespace SmartSearch.Core
{
    public interface ISearchClient
    {
        IElasticClient Client { get; }

        void Initialize();
    }
}