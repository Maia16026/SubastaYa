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
            ;

        builder.Property(x => x.FechaPuja)
            .IsRequired();

        builder.HasOne(p => p.Comprador)
            .WithMany(u => u.Pujas)
            .HasForeignKey(p => p.CompradorId);

        builder.HasOne(p => p.Subasta)
            .WithMany(s => s.Pujas)
            .HasForeignKey(p => p.SubastaId);


    }
}