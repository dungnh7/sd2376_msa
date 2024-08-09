using Jaeger;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using MotoFacts.Repository;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OpenTracing;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace MotoFacts.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IRepository<products> _repository;
    private readonly ITracer _tracer;
    private ILogger<ProductsController> _logger;

    public ProductsController(IRepository<products> repository, ILogger<ProductsController> logger, ITracer tracer)
    {
        _repository = repository;
        _logger = logger;
        _tracer = tracer;
    }

    [HttpGet(Name = "products")]
    public async Task<IEnumerable<products>> GetAll()
    {
        using (var scope = _tracer.BuildSpan($"Get Products").StartActive())
        {
            _logger.LogInformation($"Start get products");
            var result = await _repository.GetAlls();
            _logger.LogInformation(JsonConvert.SerializeObject(result));
            _logger.LogInformation($"End get products");
            return result;
        }
    }

    [HttpPost]
    public async Task<ActionResult> Post(products product)
    {
        _logger.LogInformation($"Start Post Product");
        string commandText = $"INSERT INTO products (id, name, price) VALUES (@id, @name, @price)";
        var jsonProduct = JsonConvert.DeserializeObject<JObject>(JsonConvert.SerializeObject(product));
        var queryArguments = new products
        {
            id = Int32.Parse(jsonProduct["id"]?.ToString()),
            name = jsonProduct["name"]?.ToString(),
            price = double.Parse(jsonProduct["price"]?.ToString())
        };
        await _repository.Create(queryArguments, commandText);
        _logger.LogInformation($"End Post Product");
        return Ok(queryArguments);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Get(int id)
    {
        using (var scope = _tracer.BuildSpan($"Get Product by id").StartActive())
        {
            _logger.LogInformation($"Get product by {id}");
            var result = await _repository.GetById(id);
            if (result != null)
                return Ok(result);
            else
                return NotFound();
        }
    }

    [HttpPut]
    public async Task<ActionResult> Put(dynamic product,int id)
    {
        using (var scope = _tracer.BuildSpan($"Update Product").StartActive())
        {
            _logger.LogInformation($"Start update product {id}");
            var getItem = await _repository.GetById(id);
            if (getItem != null)
            {
                string commandText = $"UPDATE products SET name = @name, price = @price WHERE id = @id";
                var jsonProduct = JsonConvert.DeserializeObject<JObject>(product.ToString());
                var queryArguments = new products
                {
                    id = Int32.Parse(jsonProduct["id"]?.ToString()),
                    name = jsonProduct["name"]?.ToString(),
                    price = double.Parse(jsonProduct["price"]?.ToString())
                };
                await _repository.Update(queryArguments, commandText);
                _logger.LogInformation($"End update product {id}");
                return Ok(queryArguments);
            }else
            {
                _logger.LogInformation($"Not found product with id: {id}");
                _logger.LogInformation($"End update product {id}");
                return BadRequest();
            }    
        }
    }

    [HttpDelete]
    public async Task<ActionResult> Delete(int id)
    {
        _logger.LogInformation($"Start delete product {id}");
        var getItem = await _repository.GetById(id);
        if (getItem != null)
        {
            await _repository.Delete(getItem, id);
            return Ok();
        }
        _logger.LogInformation($"End delete product {id}");
        return BadRequest();
    }
}
