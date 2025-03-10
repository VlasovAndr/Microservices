using ProductAPI.Data;
using ProductAPI.Models;

namespace ProductAPI.Repository
{
    public class ProductRepository : IProductRepository
    {
        private readonly ProductDbContext _db;

        public ProductRepository(ProductDbContext context)
        {
            this._db = context;
        }

        public List<Product> GetAllProducts()
        {
            return _db.Products.ToList();
        }

        public Product GetProductById(int id)
        {
           return _db.Products.First(x => x.ProductId == id);
        }

        public void AddProduct(Product product)
        {
            _db.Products.Add(product);
            _db.SaveChanges();
        }

        public void UpdateProduct(Product product)
        {
            _db.Products.Update(product);
            _db.SaveChanges();
        }

        public void DeleteProduct(int id)
        {
            Product product = _db.Products.First(x => x.ProductId == id);
            _db.Products.Remove(product);
            _db.SaveChanges();
        }

    }
}
