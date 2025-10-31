using Microsoft.EntityFrameworkCore;
using HomeEnergyApi.Pagination;

namespace HomeEnergyApi.Models
{
    public class HomeRepository : IReadRepository<int, Home>, IWriteRepository<int, Home>, IOwnerLastNameQueryable<Home>, IPaginatedReadRepository<int, Home>
    {
        private HomeDbContext context;

        public HomeRepository(HomeDbContext context)
        {
            this.context = context;
        }

        public Home Save(Home home)
        {
            if (home.HomeUsageData != null)
            {
                var usageData = home.HomeUsageData;
                usageData.Home = home;
                context.HomeUsageDatas.Add(usageData);
            }

            context.Homes.Add(home);
            context.SaveChanges();
            return home;
        }

        public Home Update(int id, Home home)
        {
            home.Id = id;
            context.Homes.Update(home);
            context.SaveChanges();
            return home;
        }

        public List<Home> FindAll()
        {
            return context.Homes
            .Include(h => h.HomeUsageData)
            .Include(h => h.HomeUtilityProviders)
            .ToList();
        }

        public Home FindById(int id)
        {
            return context.Homes.Find(id);
        }

        public Home RemoveById(int id)
        {
            var home = context.Homes.Find(id);
            context.Homes.Remove(home);
            context.SaveChanges();
            return home;
        }

        public int Count()
        {
            return context.Homes.Count();
        }

        public List<Home> FindByOwnerLastName(string ownerLastName)
        {
            Action<string> WriteToConsole = message => Console.WriteLine($"Finding by owner's last name: {message}");
            WriteToConsole(ownerLastName);

            Predicate<string> lastNameNotInitial = lastName => lastName.Length > 1;

            return context.Homes
                .Where(h => h.OwnerLastName == ownerLastName && lastNameNotInitial(ownerLastName))
                .Include(h => h.HomeUsageData)
                .Include(h => h.HomeUtilityProviders)
                .ToList();
        }

        public PaginatedResult<Home> FindPaginated(int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var totalCount = context.Homes.Count();
            var items = context.Homes
                .Include(h => h.HomeUsageData)
                .Include(h => h.HomeUtilityProviders)
                .OrderBy(h => h.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedResult<Home>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasNextPage = pageNumber * pageSize < totalCount
            };
        }

        public PaginatedResult<Home> FindPaginatedByOwnerLastName(string ownerLastName, int pageNumber, int pageSize)
        {
            pageNumber = pageNumber < 1 ? 1 : pageNumber;
            pageSize = pageSize < 1 ? 10 : pageSize;

            var totalCount = context.Homes.Count();
            var items = context.Homes
                .Where(h => h.OwnerLastName == ownerLastName)
                .Include(h => h.HomeUsageData)
                .Include(h => h.HomeUtilityProviders)
                .OrderBy(h => h.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PaginatedResult<Home>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / pageSize),
                HasNextPage = pageNumber * pageSize < totalCount
            };
        }

        public bool IsHomeNull(Home? home)
        {
            if (home == null)
                return true;
            else
                return false; 
        }

        public bool IsHomeIdNull(Home home)
        {
            if (home?.Id == null)
                return true;
            else
                return false; 
        }
    }
}