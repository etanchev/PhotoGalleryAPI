using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using PhotoGalleryAPI.Models;
using Microsoft.AspNetCore.Hosting;
using System;
using PhotoGalleryAPI.Data;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using AutoMapper;
using PhotoGalleryAPI.Dto;
using System.Collections.Generic;
using PhotoGalleryAPI.Services;
using Microsoft.AspNetCore.JsonPatch;
using WebPush;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
//using Newtonsoft.Json;


namespace PhotoGalleryAPI.Controllers
{

    [Route("/SendGridInboundParce")]
    [Authorize]
    [ApiController]
    public class SendGridInboundParceController : ControllerBase
    {

        
        private readonly ILogger<SendGridInboundParceController> _logger;
        private readonly IMapper _mapper;
        private readonly IRepository _db;
        private readonly IConfiguration Confuguration;
      

        public SendGridInboundParceController( IConfiguration confuguration, IRepository db,  ILogger<SendGridInboundParceController> logger, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;
            _db = db;
            Confuguration = confuguration;
          
        }

        [AllowAnonymous]
        // POST api/inbound
        [HttpPost]
        public async Task<IActionResult> Post([FromForm]/*[Bind(include: "Dkim,Charsets")]*/ SendGridPostModel emailForm)
        {
            _logger.LogInformation("Email received from sengrid");

            if (ModelState.IsValid)
            {
                var emailDbModel = _mapper.Map<SendGridDbModel>(emailForm);

                emailDbModel.DateTime = DateTime.Now.Truncate(TimeSpan.FromMinutes(1));
                emailDbModel.IsRed = false;

                _logger.LogInformation("Email received from sengrid Valid Model");

                await _db.RecordReceivedEmail(emailDbModel);


                var subscriptionDb =  await _db.GetSubscribtions();
                subscriptionDb.ForEach(async sub => {
                    PushSubscription subscription = new PushSubscription()
                    {
                        
                        Auth = sub.Auth,
                        Endpoint = sub.Endpoint,
                        P256DH = sub.P256dh,
                    };

                    var webPushClient = new WebPushClient() { };
                    var vapidDetails = new VapidDetails(Confuguration["VAPID:subject"], Confuguration["VAPID:publicKey"], Confuguration["VAPID:privateKey"]);

                    await webPushClient.SendNotificationAsync(subscription, emailDbModel.From, vapidDetails);
                });
                
              

                return Ok(emailForm);
            }
            else
            {
                return BadRequest(emailForm);
            }
        }
       
        [HttpGet]
        public async Task<JsonResult> Get()
        {
            return new JsonResult(_mapper.Map<IEnumerable<SendGridParserDto>>(await _db.GetEmails()));
        }

        [HttpGet]
        [Route("{mailId}")]
        public IActionResult Get(int? mailId)
        {

            if (mailId == null)
            {
                return BadRequest();
            }
            var mailEntity =  _db.GetMailById(mailId);
            if (mailEntity == null)
            {
                return NoContent();
            }

            return new JsonResult(_mapper.Map<SendGridParserDto>(mailEntity));
        }


        [HttpDelete]
        [Route("{mailId}")]
        public async Task<IActionResult> Delete(int? mailId)
        {
            if(mailId == null)
            {
                return BadRequest();
            }
            var mailEntity = _db.GetMailById(mailId);
            if (mailEntity == null)
            {
                return NoContent();
            }
            else
            {

                await  _db.DeleteMailById(mailEntity);
                return Ok();
            }
            
        }

        
        [HttpPatch]
        [Route("{mailId}")]
        public async Task<IActionResult> Patch(int? mailId,[FromBody] JsonPatchDocument patchEntity)
        {
            if (mailId == null)
            {
                return BadRequest();
            }
            var mailEntity = _db.GetMailById(mailId);
            if (mailEntity == null)
            {
                return NoContent();
            }
            else
            {

                await _db.UpdateMailById(mailEntity, patchEntity);
                return Ok();
            }

        }


    }
}
