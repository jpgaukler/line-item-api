using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;

namespace LineItem.Services.Interfaces;

public interface IExampleService
{
    public Task<int> CreateAsync(ExampleModel example, CancellationToken cancellation);

    public Task<ExampleModel?> GetByIdAsync(int id, CancellationToken cancellationToken);

    public Task<bool> UpdateAsync(
        ExampleModel example,
        CancellationToken cancellationToken
    );

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
