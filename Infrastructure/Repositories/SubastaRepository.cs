using Application.Interfaces.Persistence;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Infrastructure.Persistence;

namespace Infrastructure.Repositories;

public class SubastaRepository
    : Repository<Subasta>, ISubastaRepository
{
    public SubastaRepository(AppDbContext context)
        : base(context)
    {
    }
}