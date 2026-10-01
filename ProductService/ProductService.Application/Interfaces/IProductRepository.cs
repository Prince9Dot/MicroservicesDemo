using ProductService.Domain.Entities;

namespace ProductService.Application.Interfaces
{
    public interface IProductRepository
    {
        Task<List<Product>> GetProductsAsync();
        Task<Product?> GetProductAsync(int id);
        Task AddAsync(Product product);
    }
}
