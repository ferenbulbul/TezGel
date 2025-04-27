using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TezGel.Domain.Entities;
using TezGel.Persistence.Context;

namespace TezGel.API.Controller
{

    [ApiController]
    [Route("api/[controller]")]
    public class TestEntityController : ControllerBase
    {
        private readonly TezGelDbContext _context;

        public TestEntityController(TezGelDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var entities = _context.TestEntities.ToList();
            return Ok(entities);
        }

        [HttpPost]
        public IActionResult Create(TestEntity entity)
        {
            _context.TestEntities.Add(entity);
            _context.SaveChanges();
            return Ok(entity);
        }
    }
}
