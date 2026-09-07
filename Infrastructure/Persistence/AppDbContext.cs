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
    public DbSet<Movimiento> Movimientos => Set<Movimiento>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        modelBuilder.ApplyConfiguration(new SubastaConfiguration());
        modelBuilder.ApplyConfiguration(new PujaConfiguration());
        modelBuilder.ApplyConfiguration(new MovimientoConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
    }
}