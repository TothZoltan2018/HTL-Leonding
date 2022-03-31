using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.EntityFrameworkCore;
using ODataOrders.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ODataOrders.Controllers
{
    //[Route("api/[controller]")]
    //[ApiController]

    //public class CustomersController : ControllerBase

    // The new route will be odata/customers
    public class CustomersController : ODataController
    {
        private readonly OdataOrdersContext context;

        public CustomersController(OdataOrdersContext context)
        {
            this.context = context;
        }

        //[HttpGet]        
        //public async Task<IActionResult> GetAll()
        //{
        //    return Ok(await context.Customers.ToArrayAsync());
        //}
        
        [EnableQuery]
        public IActionResult Get()
        {
            // We are returning a reference to EF (DbSet, implements IQueryAble)
            // OData can access directly to EF: E.g.: $filter will be translated to EF query.
            // So OData will buld the EF query.
            return Ok(context.Customers);
            // Usage examples:
            // https://localhost:5001/Odata/customers?$filter=CountryId eq 'CH'
            // https://localhost:5001/Odata/customers?$filter=CountryId eq 'CH' & $select= customername
            // 
            // https://localhost:5001/Odata/customers?$select= customername&$expand=orders
        }

        [HttpPost]
        public async Task<IActionResult> Add(Customer c)
        {
            context.Customers.Add(c);
            await context.SaveChangesAsync();
            return Ok(c);
        }

        private readonly List<string> demoCustomers = new List<string>
        {
            "Foo",
            "Bar",
            "Acme",
            "King of Tech",
            "Awesomeness"
        };

        private readonly List<string> demoProducts = new List<string>
        {
            "Bike",
            "Car",
            "Apple",
            "Spaceship"
        };

        private readonly List<string> demoCountries = new List<string>
        {
            "AT",
            "DE",
            "CH"
        };

        [HttpPost]
        [Route("fill")]
        public async Task<IActionResult> Fill()
        {
            var rand = new Random();
            for (var i = 0; i < 10; i++)
            {
                var c = new Customer
                {
                    CustomerName = demoCustomers[rand.Next(demoCustomers.Count)],
                    CountryId = demoCountries[rand.Next(demoCountries.Count)]
                };
                context.Customers.Add(c);

                for (var j = 0; j < 10; j++)
                {
                    var o = new Order
                    {
                        OrderDate = DateTime.Today,
                        Product = demoProducts[rand.Next(demoProducts.Count)],
                        Quantity = rand.Next(1, 5),
                        Revenue = rand.Next(100, 5000),
                        Customer = c
                    };
                    context.Orders.Add(o);
                }
            }

            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
