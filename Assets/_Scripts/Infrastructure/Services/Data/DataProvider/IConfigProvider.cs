using System.Threading;
using Cysharp.Threading.Tasks;

namespace Farmway.Infrastructure
{
    public interface IConfigProvider
    {
        UniTask Initialize(CancellationToken ct);
        TData GetConfig<TData>();
    }
}
