using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using LineItem.Models;
using LineItem.Repositories.Interfaces;
using LineItem.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace LineItem.Services;

public class ExampleService : IExampleService
{
    private readonly ILogger<ExampleService> _logger;
    private readonly IExampleRepository _exampleRepository;

    public ExampleService(
        IExampleRepository exampleRepository,
        ILogger<ExampleService> logger
    )
    {
        _exampleRepository = exampleRepository;
        _logger = logger;
    }

    public Task<int> CreateAsync(ExampleModel example, CancellationToken cancellation)
    {
        throw new NotImplementedException();
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<ExampleModel?> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<bool> UpdateAsync(
        ExampleModel example,
        CancellationToken cancellationToken
    )
    {
        throw new NotImplementedException();
    }
}
