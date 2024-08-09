using Microsoft.Extensions.Logging;
using Moq;
using MotoFacts.Controllers;
using MotoFacts.Repository;
using OpenTracing;
using OpenTracing.Noop;

namespace MotoFacts.Tests;

public class ProductsControllerTest
{
    private Mock<IRepository<products>> _repository;
    private Mock<ILogger<ProductsController>> _logger;
    private readonly ITracer _tracer;
    private ProductsController productsController;

    public ProductsControllerTest()
    {
        _tracer = NoopTracerFactory.Create();
        _repository = new Mock<IRepository<products>>();
        _logger = new Mock<ILogger<ProductsController>>();
        productsController = new ProductsController(_repository.Object, _logger.Object, _tracer);
    }

    [Fact]
    public async Task Get_Products_Success()
    {
        // Arrange
        _repository.Setup(service => service.GetAlls())
            .ReturnsAsync(new List<products> { new products { id=1,name="a",price=10} });
        // Act
        var result = await productsController.GetAll();

        // Assert
        Assert.Equal(1, result.FirstOrDefault()?.id);
    }
}