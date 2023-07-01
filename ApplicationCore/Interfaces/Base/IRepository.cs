using ApplicationCore.Entities.Model;
using Ardalis.Specification;

namespace ApplicationCore.Interfaces.Base;

public interface IRepository<T> : IRepositoryBase<T>
    where T : BaseEntity { }
