using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApiAngular.Data;
using WebApiAngular.Models;

namespace WebApiAngular.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public ProductController(ApplicationDbContext db)
        {
            _db = db;
        }



        [HttpGet("[action]")]

        [Authorize(Policy = "RequiredLoggedIn")]
        public IActionResult GetProducts()
        {
            return Ok(_db.Product.ToList());
        }


        [HttpPost("[action]")]
        [Authorize(Policy = "RequiredAdministratorRole")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductModel formdata)
        {
            var newProduct = new ProductModel
            {
                Name = formdata.Name,
                Description = formdata.Description,
                OutOfStock = formdata.OutOfStock,
                ImageUrl = formdata.ImageUrl,
                Price = formdata.Price
            };

            await _db.Product.AddAsync(newProduct);
            await _db.SaveChangesAsync();

            return Ok(new JsonResult("Product Added Succesffully"));
        }


        [HttpPut("[action]/{id}")]
        [Authorize(Policy = "RequiredAdministratorRole")]
        public async Task<IActionResult> UpdateProduct([FromRoute]int id, [FromBody] ProductModel formdata)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var findProduct = _db.Product.FirstOrDefault(p => p.ProductId == id);

            if(findProduct == null)
            {
                return NotFound();
            }

            //if the product was found den update

            findProduct.Name = formdata.Name;
            findProduct.Description = formdata.Description;
            findProduct.OutOfStock = formdata.OutOfStock;
            findProduct.ImageUrl = formdata.ImageUrl;
            findProduct.Price = formdata.Price;

            _db.Entry(findProduct).State = EntityState.Modified;
            await _db.SaveChangesAsync();

            return Ok(new JsonResult("The Product with" + id + "is updated"));
        }


        [HttpDelete("[action]/{id}")]
        [Authorize(Policy = "RequiredAdministratorRole")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //Find the product

            var findProduct = await _db.Product.FindAsync( id);

            if(findProduct == null)
            {
                return NotFound();
            }

            _db.Product.Remove(findProduct);
            await  _db.SaveChangesAsync();

            return Ok(new JsonResult("Product with" + id + "has been deleted"));

        }
    }
}