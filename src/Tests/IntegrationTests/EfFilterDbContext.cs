using Microsoft.EntityFrameworkCore;

public class EfFilterDbContext :
    DbContext
{
    public DbSet<ParentEntity> ParentEntities { get; set; }
    public DbSet<ChildEntity> ChildEntities { get; set; }
    public DbQuery<ParentEntityView> ParentEntityView { get; set; }

    public EfFilterDbContext(DbContextOptions options) :
        base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder  .Query<ParentEntityView>()
            .ToView("ParentEntityView")
            .Property(v => v.Property).HasColumnName("Property");
        modelBuilder.Entity<ParentEntity>();
        modelBuilder.Entity<ChildEntity>();
    }
}