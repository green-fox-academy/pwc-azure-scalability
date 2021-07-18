using System.Collections.Generic;
using System.Linq;
using InvoiceProcessor.Api.Response;
using Microsoft.AspNetCore.Mvc;

namespace InvoiceProcessor.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ApiBackendController : ControllerBase
    {
        [HttpGet]
        [Route("employees")]
        public ActionResult<List<Employee>> GetEmployees()
        {

            var employees = Enumerable.Range(1, 100).Select(index => Employee.CreateRandom());

            return Ok(employees);
        }
    }
}
