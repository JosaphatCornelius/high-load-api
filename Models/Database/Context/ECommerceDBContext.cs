using Microsoft.EntityFrameworkCore;

namespace high_load_api.Models.Database.Context
{
    public class ECommerceDBContext : DbContext
    {
        public ECommerceDBContext(DbContextOptions<ECommerceDBContext> options) : base(options) { }
        public DbSet<UsersModel> Users { get; set; }
        public DbSet<CartsModel> Carts { get; set; }
        public DbSet<CartItemsModel> CartItems { get; set; }
        public DbSet<ProductsModel> Products { get; set; }
        public DbSet<CategoriesModel> Categories { get; set; }
        public DbSet<OrdersModel> Orders { get; set; }
        public DbSet<PaymentsModel> Payments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<UsersModel>()
                .HasOne(user => user.Cart)
                .WithOne(cart => cart.User)
                .HasForeignKey<CartsModel>(c => c.UserID);
        }
    }
}
