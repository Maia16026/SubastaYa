using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Evento)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Detalles)
            .HasMaxLength(2000);

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

        builder.HasIndex(x => x.FechaHora);
    }
}