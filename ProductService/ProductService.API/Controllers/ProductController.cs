using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductService.Application.DTOs;
using ProductService.Application.Interfaces;
using ProductService.Application.Services;
using ProductService.Domain.Entities;

namespace ProductService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ProductCacheService _cacheService;

        public ProductController(IProductService productService,ProductCacheService cacheService)
        {
            _productService = productService;
            _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var cacheKey = $"products";

            var cacheProduct = await _cacheService.GetAsync<IEnumerable<ProductResponse>>(cacheKey);

            if (cacheProduct is not null)
            {
                return Ok(cacheProduct);
            }
            var result = await _productService.GetProductsAsync();

            await _cacheService.SetAsync<IEnumerable<ProductResponse>>(cacheKey, result, TimeSpan.FromMinutes(5));

            return Ok(result);
        }
                
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var cacheKey = $"product:{id}";

            var cacheProduct = await _cacheService.GetAsync<ProductResponse>(cacheKey);

            if(cacheProduct is not null)
            {
                return Ok(cacheProduct);
            }

            var result =
                await _productService.GetProductAsync(id);

            await _cacheService.SetAsync<ProductResponse>(cacheKey, result, TimeSpan.FromMinutes(5));

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct(
            CreateProductRequest request)
        {
            var cacheKey = $"products";
            await _cacheService.RemoveAsync(cacheKey);

            var result =
                await _productService.CreateProductAsync(request);

            return Ok(result);
        }
    }
}
