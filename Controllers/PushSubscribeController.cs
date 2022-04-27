using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PhotoGalleryAPI.Data;
using PhotoGalleryAPI.Models;
using System.Threading.Tasks;



namespace PhotoGalleryAPI.Controllers
{
    //[DisableCors]
    [Route("/PushSubscribe")]
    [ApiController]
    [Authorize]
    public class PushSubscribeController : ControllerBase
    {
        private readonly ApplicationDbContext _dbCntext;
        private readonly ILogger<PushSubscribeController> _logger;
        private readonly IMapper _mapper;
       
        

        public PushSubscribeController( ApplicationDbContext dbContext, ILogger<PushSubscribeController> logger, IMapper mapper)
        {
            //_env = env;
            _dbCntext = dbContext;
            _logger = logger;
            _mapper = mapper;
            _dbCntext = dbContext;
           
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PushSubscribe pushSubscribe)
        {
            _logger.LogInformation("Subscribtion  received from app");

            if (ModelState.IsValid)
            {
                var _pushSubscribe = _mapper.Map<PushSubscribeDbModel>(pushSubscribe);
                _logger.LogInformation("Valid Model");
                _dbCntext.Entry(_pushSubscribe).State = EntityState.Added;
                _dbCntext.Add(_pushSubscribe);
                await _dbCntext.SaveChangesAsync();
                return Ok(pushSubscribe);
            }
            else
            {
                return BadRequest(pushSubscribe);
            }
        }
    }
}
