using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Persistence.Configurations;

public class AuditoriaLogConfiguration : IEntityTypeConfiguration<AuditoriaLog>
{
    public void Configure(EntityTypeBuilder<AuditoriaLog> builder)
    {
        builder.ToTable("AuditoriaLogs");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Entidad)
            .IsRequired();

        builder.Property(x => x.EntidadId)
            .IsRequired();

        builder.Property(x => x.Accion)
            .IsRequired();

        builder.Property(x => x.DetalleJson)
            .IsRequired();

        builder.Property(x => x.Fecha)
            .IsRequired();

        // Relación opcional con Usuario
        builder.HasOne(a => a.Usuario)
            .WithMany(u => u.AuditoriaLogs)
            .HasForeignKey(a => a.UsuarioId)
            .IsRequired(false);
    }
}
