using System;
using System.Threading.Tasks;
using LineItem.Models;
using LineItem.Repositories.Interfaces;
using LineItem.Services;
using LineItem.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;

namespace LineItem.Test
{
    public class ExampleTest
    {
        private readonly Mock<IExampleRepository> _exampleRepository;

        private readonly IExampleService _exampleService;

        public ExampleTest()
        {
            _exampleRepository = new Mock<IExampleRepository>();
            var logger = new Mock<ILogger<ExampleService>>();

            _exampleService = new ExampleService(
                _exampleRepository.Object,
                logger.Object
            );
        }

        [Fact]
        public async Task CreateExample_WithInvalidName_ThrowsValidationException()
        {
            var example = new ExampleModel
            {
                Name = "Test",
                Description = "Test",
            };

            Exception ex = await Assert.ThrowsAsync<Exception>(() =>
                _exampleService.CreateAsync(example, default)
            );

            Assert.Contains(ex.Message, "my message");
        }
    }
}
