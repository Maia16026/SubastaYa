using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class PujaConfiguration : IEntityTypeConfiguration<Puja>
{
    public void Configure(EntityTypeBuilder<Puja> builder)
    {
        builder.ToTable("Pujas");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Monto)
            .IsRequired()
            .HasPrecision(12, 2);

        builder.Property(x => x.FechaHora)
            .IsRequired();

        builder.HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Subasta>()
            .WithMany()
            .HasForeignKey(x => x.SubastaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new
        {
            x.SubastaId,
            x.FechaHora
        });
    }
}