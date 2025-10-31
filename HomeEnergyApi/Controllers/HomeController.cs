using Microsoft.AspNetCore.Mvc;
using HomeEnergyApi.Models;
using HomeEnergyApi.Pagination;

namespace HomeEnergyApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HomesController : ControllerBase
    {
        private IPaginatedReadRepository<int, Home> repository;

        public HomesController(IPaginatedReadRepository<int, Home> repository)
        {
            this.repository = repository;
        }

        [HttpGet]
        public IActionResult Get([FromQuery] string? ownerLastName, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            PaginatedResult<Home> paginatedResult;

            if (ownerLastName != null)
            {
                paginatedResult = repository.FindPaginatedByOwnerLastName(ownerLastName, pageNumber, pageSize);
            }
            else
            {
                paginatedResult = repository.FindPaginated(pageNumber, pageSize);
            }

            var totalPages = (int)Math.Ceiling((double)paginatedResult.TotalCount / pageSize);

            var nextPageUrl = pageNumber < totalPages
                    ? Url.Action(nameof(Get), new { pageNumber = pageNumber + 1, pageSize })
                    : null;

            return Ok(new
            {
                Homes = paginatedResult.Items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = paginatedResult.TotalCount,
                TotalPages = paginatedResult.TotalPages,
                NextPage = nextPageUrl
            });
        }

        [HttpGet("{id}")]
        public IActionResult FindById(int id)
        {
            if (id > repository.FindAll().Count)
            {
                return NotFound();
            }
            var home = repository.FindById(id);

            return Ok(home);
        }

        [HttpGet("Bang")]
        public IActionResult Bang()
        {
            throw new InvalidOperationException("You caused a loud bang.");
        }
    }
}