using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class TransaccionLedgerConfiguration : IEntityTypeConfiguration<TransaccionLedger>
{
    public void Configure(EntityTypeBuilder<TransaccionLedger> builder)
    {
        builder.ToTable("Transacciones");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Tipo)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.Monto)
            .IsRequired();

        builder.Property(x => x.Fecha)
            .IsRequired();

        // Relación obligatoria con Billetera
        builder.HasOne(t => t.Billetera)
            .WithMany(b => b.Transacciones)
            .HasForeignKey(t => t.BilleteraId);

        // Relación opcional con Subasta
        builder.HasOne(t => t.Subasta)
            .WithMany(s => s.Transacciones)
            .HasForeignKey(t => t.SubastaId)
            .IsRequired(false);
    }
}
