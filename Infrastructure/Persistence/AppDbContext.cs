using Domain.Entities;
using Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Subasta> Subastas => Set<Subasta>();
    public DbSet<Puja> Pujas => Set<Puja>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Billetera> Billeteras => Set<Billetera>();
    public DbSet<TransaccionLedger> Transacciones => Set<TransaccionLedger>();
    public DbSet<AuditoriaLog> AuditoriaLogs => Set<AuditoriaLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new SubastaConfiguration());
        modelBuilder.ApplyConfiguration(new PujaConfiguration());
        modelBuilder.ApplyConfiguration(new CategoriaConfiguration());
        modelBuilder.ApplyConfiguration(new BilleteraConfiguration());
        modelBuilder.ApplyConfiguration(new TransaccionLedgerConfiguration());
        modelBuilder.ApplyConfiguration(new AuditoriaLogConfiguration());
    }
    public override int SaveChanges()
    {
        ValidarAuditoriasInmutables();

        return base.SaveChanges();
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ValidarAuditoriasInmutables();

        return base.SaveChangesAsync(cancellationToken);
    }

    private void ValidarAuditoriasInmutables()
    {
        foreach (var entrada in ChangeTracker.Entries<AuditoriaLog>())
        {
            if (entrada.State == EntityState.Modified ||
                entrada.State == EntityState.Deleted)
            {
                throw new InvalidOperationException(
                    "Los registros de auditoría son inmutables y no pueden modificarse ni eliminarse.");
            }
        }
    }
}
