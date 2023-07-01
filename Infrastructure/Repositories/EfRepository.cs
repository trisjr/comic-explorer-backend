using ApplicationCore.Entities.Model;
using ApplicationCore.Interfaces.Base;
using Ardalis.Specification;
using Ardalis.Specification.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class EfRepository<T> : RepositoryBase<T>, IRepository<T>
    where T : BaseEntity
{
    public EfRepository(DbContext dbContext)
        : base(dbContext) { }

    public EfRepository(DbContext dbContext, ISpecificationEvaluator specificationEvaluator)
        : base(dbContext, specificationEvaluator) { }
}
