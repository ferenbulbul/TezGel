using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TezGel.Application.DTOs.Auth.Comman;
using TezGel.Application.Interfaces.Services;

namespace TezGel.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RedisTestController : ControllerBase
    {
        private readonly IRedisService _redisService;

        public RedisTestController(IRedisService redisService)
        {
            _redisService = redisService;
        }

        [HttpPost("set")]
        public async Task<IActionResult> Set(string key, string value)
        {
            await _redisService.SetAsync(key, value, TimeSpan.FromMinutes(5));
            return Ok(ApiResponse<string>.SuccessResponse(null, "Key ayarlandı."));
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get(string key)
        {
            var value = await _redisService.GetAsync(key);
            return Ok(ApiResponse<string>.SuccessResponse(value, "Key bulundu."));
        }
    }
}