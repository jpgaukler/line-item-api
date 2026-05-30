using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;

namespace LineItem.Repositories.Interfaces;

public interface IExampleRepository
{
    public Task<long> CreateAsync(
        ExampleModel example,
        CancellationToken cancellationToken
    );

    public Task<ExampleModel?> GetByIdAsync(int id, CancellationToken cancellationToken);

    public Task<bool> UpdateAsync(
        ExampleModel example,
        CancellationToken cancellationToken
    );

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
