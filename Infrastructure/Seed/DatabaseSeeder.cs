using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seed;

public class DatabaseSeeder
{
    public async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        // Una sola vez por ejecución
        var now = DateTime.UtcNow;

        // 1) Usuarios
        var vendedor = await EnsureUsuarioAsync(context, "vendedor@test.com", "Vendedor", "seed-hash", now, cancellationToken);
        var comprador1 = await EnsureUsuarioAsync(context, "comprador1@test.com", "Comprador1", "seed-hash", now, cancellationToken);
        var comprador2 = await EnsureUsuarioAsync(context, "comprador2@test.com", "Comprador2", "seed-hash", now, cancellationToken);
        var sinfondos = await EnsureUsuarioAsync(context, "sinfondos@test.com", "SinFondos", "seed-hash", now, cancellationToken);

        // 2) Billeteras (crear o actualizar saldos a los valores requeridos)
        var billeVendedor = await EnsureBilleteraAsync(context, vendedor.Id, 0m, 0m, 0m, cancellationToken);
        var billeComprador1 = await EnsureBilleteraAsync(context, comprador1.Id, 150000m, 45000m, 105000m, cancellationToken);
        var billeComprador2 = await EnsureBilleteraAsync(context, comprador2.Id, 200000m, 0m, 200000m, cancellationToken);
        var billeSinFondos = await EnsureBilleteraAsync(context, sinfondos.Id, 500m, 0m, 500m, cancellationToken);

        // 3) Categorías
        var catTec = await EnsureCategoriaAsync(context, "Tecnología", "/icons/tecnologia.png", cancellationToken);
        var catCol = await EnsureCategoriaAsync(context, "Coleccionables", "/icons/coleccionables.png", cancellationToken);
        var catInd = await EnsureCategoriaAsync(context, "Indumentaria", "/icons/indumentaria.png", cancellationToken);
        var catVeh = await EnsureCategoriaAsync(context, "Vehículos", "/icons/vehiculos.png", cancellationToken);

        // 4) Subastas seed (crear o actualizar fechas/estado)
        // Títulos fijos para identificación
        var sActivaEstandar = await EnsureSubastaAsync(context,
            titulo: "SEED_ACTIVA_Estandar",
            vendedorId: vendedor.Id,
            categoriaId: catTec.Id,
            tituloVisible: "Smartphone prueba - activa estándar",
            descripcion: "Subasta seed activa estándar",
            urlImagen: "/images/phone.png",
            precioBase: 30000m,
            incrementoMinimo: 1000m,
            fechaInicio: now.AddMinutes(-1),
            fechaFin: now.AddMinutes(25),
            estado: EstadoSubasta.ACTIVA,
            cancellationToken: cancellationToken);

        var sActivaCritica = await EnsureSubastaAsync(context,
            titulo: "SEED_ACTIVA_Critica",
            vendedorId: vendedor.Id,
            categoriaId: catTec.Id,
            tituloVisible: "Lote crítico - activa crítica",
            descripcion: "Subasta seed activa crítica",
            urlImagen: "/images/critical.png",
            precioBase: 10000m,
            incrementoMinimo: 500m,
            fechaInicio: now.AddMinutes(-1),
            fechaFin: now.AddSeconds(90),
            estado: EstadoSubasta.ACTIVA,
            cancellationToken: cancellationToken);

        var sProgramada = await EnsureSubastaAsync(context,
            titulo: "SEED_PROGRAMADA",
            vendedorId: vendedor.Id,
            categoriaId: catCol.Id,
            tituloVisible: "Colección futura - programada",
            descripcion: "Subasta seed programada",
            urlImagen: "/images/collectible.png",
            precioBase: 5000m,
            incrementoMinimo: 250m,
            fechaInicio: now.AddHours(24),
            fechaFin: now.AddHours(48),
            estado: EstadoSubasta.PROGRAMADA,
            cancellationToken: cancellationToken);

        var sVencidaGanador = await EnsureSubastaAsync(context,
            titulo: "SEED_VENCIDA_GANADOR",
            vendedorId: vendedor.Id,
            categoriaId: catInd.Id,
            tituloVisible: "Chaqueta vintage - vencida con ganador",
            descripcion: "Subasta seed vencida con ganador",
            urlImagen: "/images/jacket.png",
            precioBase: 2000m,
            incrementoMinimo: 100m,
            fechaInicio: now.AddDays(-2),
            fechaFin: now.AddDays(-1),
            estado: EstadoSubasta.ACTIVA,
            cancellationToken: cancellationToken);

        var sVencidaDesierta = await EnsureSubastaAsync(context,
            titulo: "SEED_VENCIDA_DESIERTA",
            vendedorId: vendedor.Id,
            categoriaId: catVeh.Id,
            tituloVisible: "Auto antiguo - vencida desierta",
            descripcion: "Subasta seed vencida desierta",
            urlImagen: "/images/car.png",
            precioBase: 100000m,
            incrementoMinimo: 5000m,
            fechaInicio: now.AddDays(-3),
            fechaFin: now.AddDays(-2),
            estado: EstadoSubasta.ACTIVA,
            cancellationToken: cancellationToken);

        // 5) Pujas seed
        // Activa estándar: exactamente 2 pujas, última comprador1 45000
        await EnsurePujaAsync(context, sActivaEstandar.Id, comprador2.Id, 40000m, now.AddMinutes(-0.5), cancellationToken);
        await EnsurePujaAsync(context, sActivaEstandar.Id, comprador1.Id, 45000m, now.AddMinutes(-0.2), cancellationToken);

        // Vencida con ganador: al menos una puja ganadora (por ejemplo comprador1)
        await EnsurePujaAsync(context, sVencidaGanador.Id, comprador1.Id, 3000m, now.AddDays(-1).AddHours(-1), cancellationToken);

        // No crear pujas para vencida desierta

        // 6) Ledger seed
        // Depósitos para explicar saldos
        await EnsureTransaccionDepositoAsync(context, billeComprador1.Id, 150000m, cancellationToken);
        await EnsureTransaccionDepositoAsync(context, billeComprador2.Id, 200000m, cancellationToken);
        await EnsureTransaccionDepositoAsync(context, billeSinFondos.Id, 500m, cancellationToken);
        // Retención de 45000 para comprador1 asociada a la subasta activa estándar
        await EnsureTransaccionRetencionAsync(context, billeComprador1.Id, 45000m, sActivaEstandar.Id, cancellationToken);

        // Guardar todo y terminar
        await context.SaveChangesAsync(cancellationToken);
    }

    // Helpers
    private async Task<Usuario> EnsureUsuarioAsync(AppDbContext ctx, string email, string nombre, string passwordHash, DateTime fechaRegistro, CancellationToken ct)
    {
        var existing = await ctx.Usuarios.SingleOrDefaultAsync(u => u.Email == email, ct);
        if (existing != null) return existing;

        var u = new Usuario(nombre, email, passwordHash, fechaRegistro);
        ctx.Usuarios.Add(u);
        await ctx.SaveChangesAsync(ct);
        return u;
    }

    private async Task<Billetera> EnsureBilleteraAsync(AppDbContext ctx, int usuarioId, decimal total, decimal retenido, decimal disponible, CancellationToken ct)
    {
        var existing = await ctx.Billeteras.SingleOrDefaultAsync(b => b.UsuarioId == usuarioId, ct);
        if (existing != null)
        {
            // Si ya existe, devolverla sin modificar datos de negocio
            return existing;
        }

        var bille = new Billetera(usuarioId, total, retenido, disponible, 1);
        ctx.Billeteras.Add(bille);
        await ctx.SaveChangesAsync(ct);
        return bille;
    }

    private async Task<Categoria> EnsureCategoriaAsync(AppDbContext ctx, string nombre, string urlIcono, CancellationToken ct)
    {
        var existing = await ctx.Categorias.SingleOrDefaultAsync(c => c.Nombre == nombre, ct);
        if (existing != null) return existing;

        var cat = new Categoria(nombre, urlIcono);
        ctx.Categorias.Add(cat);
        await ctx.SaveChangesAsync(ct);
        return cat;
    }

    private async Task<Subasta> EnsureSubastaAsync(AppDbContext ctx,
        string titulo,
        int vendedorId,
        int categoriaId,
        string tituloVisible,
        string descripcion,
        string urlImagen,
        decimal precioBase,
        decimal incrementoMinimo,
        DateTime fechaInicio,
        DateTime fechaFin,
        EstadoSubasta estado,
        CancellationToken cancellationToken)
    {
        var existing = await ctx.Subastas.SingleOrDefaultAsync(s => s.Titulo == titulo, cancellationToken);
        if (existing != null)
        {
            // Si ya existe la subasta, devolverla sin modificar fechas/estado
            return existing;
        }

        // Usamos el titulo fijo en el campo Titulo para identificar
        var s = new Subasta(vendedorId, categoriaId, titulo, descripcion, urlImagen, precioBase, incrementoMinimo, fechaInicio, fechaFin);
        // Ajustar campo Titulo visible: el constructor puso el Titulo (seed id)
        // Si queremos un texto más amigable lo podríamos usar en Descripcion o similar; aquí dejamos Titulo como identificador.
        ctx.Subastas.Add(s);
        // Establecer Estado explícito ANTES de guardar para que el objeto creado tenga el estado correcto
        ctx.Entry(s).Property("Estado").CurrentValue = estado;
        await ctx.SaveChangesAsync(cancellationToken);
        return s;
    }

    private async Task EnsurePujaAsync(AppDbContext ctx, int subastaId, int compradorId, decimal monto, DateTime fechaPuja, CancellationToken ct)
    {
        var existing = await ctx.Pujas.SingleOrDefaultAsync(p => p.SubastaId == subastaId && p.CompradorId == compradorId && p.Monto == monto, ct);
        if (existing != null)
        {
            // Si ya existe la puja, no tocar la FechaPuja ni otros campos
            return;
        }

        var puja = new Puja(subastaId, compradorId, monto, fechaPuja);
        ctx.Pujas.Add(puja);
        await ctx.SaveChangesAsync(ct);
    }

    private async Task EnsureTransaccionDepositoAsync(AppDbContext ctx, int billeteraId, decimal monto, CancellationToken ct)
    {
        var existing = await ctx.Transacciones.SingleOrDefaultAsync(t => t.BilleteraId == billeteraId && t.Tipo == TipoMovimiento.DEPOSITO && t.Monto == monto && t.SubastaId == null, ct);
        if (existing != null) return;

        var tx = new TransaccionLedger(billeteraId, TipoMovimiento.DEPOSITO, monto, DateTime.UtcNow, null);
        ctx.Transacciones.Add(tx);
        await ctx.SaveChangesAsync(ct);
    }

    private async Task EnsureTransaccionRetencionAsync(AppDbContext ctx, int billeteraId, decimal monto, int subastaId, CancellationToken ct)
    {
        var existing = await ctx.Transacciones.SingleOrDefaultAsync(t => t.BilleteraId == billeteraId && t.Tipo == TipoMovimiento.RETENCION && t.Monto == monto && t.SubastaId == subastaId, ct);
        if (existing != null) return;

        var tx = new TransaccionLedger(billeteraId, TipoMovimiento.RETENCION, monto, DateTime.UtcNow, subastaId);
        ctx.Transacciones.Add(tx);
        await ctx.SaveChangesAsync(ct);
    }
}
