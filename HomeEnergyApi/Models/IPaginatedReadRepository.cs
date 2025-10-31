using System.Collections.Generic;
using HomeEnergyApi.Pagination;

namespace HomeEnergyApi.Models
{
    public interface IPaginatedReadRepository<TId, T> : IReadRepository<TId, T>, IOwnerLastNameQueryable<T>
    {
        PaginatedResult<T> FindPaginated(int pageNumber, int pageSize);
        PaginatedResult<T> FindPaginatedByOwnerLastName(string ownerLastName, int pageNumber, int pageSize);
    }
}