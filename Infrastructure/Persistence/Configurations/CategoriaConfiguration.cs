using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("Categorias");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Nombre)
            .IsRequired();

        builder.Property(x => x.UrlIcono)
            .IsRequired();

        builder.HasMany(c => c.Subastas)
            .WithOne(s => s.Categoria)
            .HasForeignKey(s => s.CategoriaId);
    }
}
