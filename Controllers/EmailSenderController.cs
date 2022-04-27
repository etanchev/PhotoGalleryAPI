using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PhotoGalleryAPI.Models;
using PhotoGalleryAPI.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PhotoGalleryAPI.Controllers
{
    [Route("/EmailSender")]
    [ApiController]
    [Authorize]
    public class EmailSenderController : ControllerBase
    {
       readonly private  ILogger<EmailSenderController> _logger;
        readonly private EmailSender _emailSender;
        readonly private IConfiguration Configuration;
        readonly private IMapper _mapper;
        readonly private IRepository _db;
       readonly private  IWebHostEnvironment env;

        public EmailSenderController(ILogger<EmailSenderController> logger, IWebHostEnvironment env, IRepository db, EmailSender emailSender, IConfiguration configuration ,IMapper mapper)
        {
            _logger = logger;
            _emailSender = emailSender;
            Configuration = configuration;
            _mapper = mapper;
            _db = db;
            this.env = env;
        }

        [HttpPost]
        public  async Task<IActionResult> Post([FromBody]/*[Bind(include: "Dkim,Charsets")]*/ EmailModel emailForm)
        {
          

            if (ModelState.IsValid)
            {
                _logger.LogInformation("Valid Model");

                try
                {
                    await _emailSender.SendGridSMTP(Configuration["Passwords:SendGridApi"], emailForm.Subject, emailForm.Body, emailForm.To, emailForm.Attachments);
                }
                catch (Exception e)
                {
                    _logger.LogInformation(e.ToString());
                    return new JsonResult("Error");
                }

                var sentMail = _mapper.Map<EmailDbModel>(emailForm);
                sentMail.DateTime = DateTime.Now.Truncate(TimeSpan.FromMinutes(1));
                await _db.RecordSentMail(sentMail);
                return Ok();

            }
            else
            {
                return BadRequest();
            }

           
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("/EmailSender/EmailFromBlazor")]
        public async Task<IActionResult> EmailFromBlazor([FromBody] EmailModelBlazor emailModelBlazor)
        {
           
            _logger.LogInformation("Email request from blazor ");
            try
            {
               StringBuilder sb = new StringBuilder();

                sb.Append("<html>");
                sb.Append("<body>");
                sb.Append("<ul>");

                foreach (var image in emailModelBlazor.Body)
                {
                    sb.AppendLine("<li>" + image + "</li>");
                }

                sb.Append("</ul>");

                sb.AppendLine("<br>");
                sb.AppendLine("<br>");
                sb.AppendLine("<div>Име: " + emailModelBlazor.Name + "</div>");
                sb.AppendLine("<div>Имеил: " + emailModelBlazor.Email + "</div>");
                sb.AppendLine("<div>Брой Снимки: " + emailModelBlazor.Body.Count().ToString() + "</div>");

                sb.Append("</body>");
                sb.Append("</html>");


                //Thread.Sleep(30000);

               var result =  await _emailSender.SendBlazorEmail(Configuration["Passwords:SendGridApi"], "Избрани снимки - " + emailModelBlazor.Name, sb.ToString(), emailModelBlazor.Email);

                if (result)
                {
                    return StatusCode(StatusCodes.Status200OK); 
                }
                else
                {
                    return StatusCode(StatusCodes.Status500InternalServerError); 
                 
                }

            }
            catch 
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet]
        public async Task<JsonResult> Get()
        {
            return new JsonResult(await _db.GetSentEmails());
        }

        [HttpGet]
        [Route("{mailId}")]
        public IActionResult Get(int? mailId)
        {

            if (mailId == null)
            {
                return BadRequest();
            }
            var mailEntity = _db.GetSentMailById(mailId);
            if (mailEntity == null)
            {
                return NoContent();
            }

            return new JsonResult(mailEntity);
        }

        [HttpDelete]
        [Route("{mailId}")]
        public async Task<IActionResult> Delete(int? mailId)
        {
            if (mailId == null)
            {
                return BadRequest();
            }
            var mailEntity = _db.GetSentMailById(mailId);
            if (mailEntity == null)
            {
                return NoContent();
            }
            else
            {

                await _db.DeleteSentMailById(mailEntity);
                return Ok();
            }

        }


      


        [HttpPost]
        [Route("/EmailSender/PostFile")]
        public async Task<ActionResult<IList<UploadResult>>> PostFile([FromForm] IEnumerable<IFormFile> files)
        {
            var maxAllowedFiles = 3;
            long maxFileSize = 1024 * 1024 * 15;
            var filesProcessed = 0;
            var resourcePath = new Uri($"{Request.Scheme}://{Request.Host}/");
            IList<UploadResult> uploadResults = new List<UploadResult>();

            Directory.CreateDirectory(Path.Combine(env.WebRootPath, "Attachments"));

            foreach (var file in files)
            {
                var uploadResult = new UploadResult();
                string trustedFileNameForFileStorage;
                var untrustedFileName = file.FileName;
                uploadResult.FileName = untrustedFileName;
                var trustedFileNameForDisplay =
                    WebUtility.HtmlEncode(untrustedFileName);

                if (filesProcessed < maxAllowedFiles)
                {
                    if (file.Length == 0)
                    {
                        _logger.LogInformation("{FileName} length is 0",
                            trustedFileNameForDisplay);
                        uploadResult.ErrorCode = 1;
                    }
                    else if (file.Length > maxFileSize)
                    {
                        _logger.LogInformation("{FileName} of {Length} bytes is " +
                            "larger than the limit of {Limit} bytes",
                            trustedFileNameForDisplay, file.Length, maxFileSize);
                        uploadResult.ErrorCode = 2;
                    }
                    else
                    {
                        try
                        {
                            trustedFileNameForFileStorage = Path.GetRandomFileName();
                                var path = Path.Combine(env.WebRootPath, "Attachments", file.FileName /*trustedFileNameForFileStorage*/);
                                using MemoryStream ms = new MemoryStream();
                                await file.CopyToAsync(ms);
                                await System.IO.File.WriteAllBytesAsync(path, ms.ToArray());
                                _logger.LogInformation("{FileName} saved at {Path}",
                                    trustedFileNameForDisplay, path);
                                uploadResult.Uploaded = true;
                                uploadResult.StoredFileName = trustedFileNameForFileStorage;
                        }
                        catch (IOException ex)
                        {
                            _logger.LogError("{FileName} error on upload: {Message}",
                                trustedFileNameForDisplay, ex.Message);
                            uploadResult.ErrorCode = 3;
                        }
                    }

                    filesProcessed++;
                }
                else
                {
                    _logger.LogInformation("{FileName} not uploaded because the " +
                        "request exceeded the allowed {Count} of files",
                        trustedFileNameForDisplay, maxAllowedFiles);
                    uploadResult.ErrorCode = 4;
                }

                uploadResults.Add(uploadResult);
            }

            return new CreatedResult(resourcePath, uploadResults);
        }

        

    }
}
