using AutoMapper;
using AutoMapper.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PhotoGalleryAPI.Models;
using PhotoGalleryAPI.Services;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PhotoGalleryAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ILogger<SendGridInboundParceController> _logger;

        private readonly IRepositoryAdmin _db;
       
        private readonly IWebHostEnvironment _webHost;
      

        private readonly string ClientFoldersDir = "ClientFolders";

        public AdminController(
                                IRepositoryAdmin db,
                                ILogger<SendGridInboundParceController> logger,
                                IWebHostEnvironment webHost
                             
                               
                                )
        {
            _logger = logger;
            _db = db;
            _webHost = webHost;
          
           

        }

        [HttpPost]
        [Route("/CreateFolder")]
        public async Task<IActionResult> Post([FromBody] FolderModel folderModel)
        {
            //_logger.LogInformation("Request for folder to be created recevied");

            if (ModelState.IsValid)
            {
                await _db.RecordFolderInfo(folderModel);
                Directory.CreateDirectory(Path.Combine(_webHost.WebRootPath, ClientFoldersDir, folderModel.Guid.ToString()));
                return Ok(folderModel);
            }
            else
            {
                return BadRequest(folderModel);
            }
        }

        [HttpGet]
        [Route("/GetFolders")]
        public async Task<IEnumerable<FolderModel>> Get()
        {
            return await _db.GetFolders();
        }

        [HttpDelete]
        [Route("/DeleteFolder/{folderID}")]
        public async Task<IActionResult> Delete(int? folderID)
        {
            _logger.LogInformation("Request for folder to be deleted recevied");

            if (folderID != null)
            {
                var folderInfo = await _db.GetFolderInfo(folderID);
                await _db.DeleteFolder(folderID);
                Directory.Delete(Path.Combine(_webHost.WebRootPath, ClientFoldersDir, folderInfo.Guid.ToString()), true);
                return Ok();
            }
            else
            {
                return BadRequest();
            }

        }

        [HttpDelete]
        [Route("/DeleteFile/{filePath}/{fileName}")]
        public IActionResult DeleteFile(string filePath, string fileName)
        {
            _logger.LogInformation("Request for file to be deleted recevied");

            if (filePath != "" && fileName != "")
            {
                if (new FileInfo(Path.Combine(_webHost.WebRootPath, ClientFoldersDir, filePath, fileName)).Exists)
                {
                    System.IO.File.Delete(Path.Combine(_webHost.WebRootPath, ClientFoldersDir, filePath, fileName));
                    return Ok();
                }
                else
                {
                    return NotFound();
                }
            }
            else
            {
                return BadRequest();
            }

        }
       


        [AllowAnonymous]
        [HttpGet]
        [Route("/GetImages/{folderID}")]
        public async Task<IActionResult> GetImages(Guid? folderID)
        {


            if (folderID != null)
            {
                string directoryPath = Path.Combine(_webHost.WebRootPath, ClientFoldersDir, folderID.ToString());
                if (Directory.Exists(directoryPath))
                {

                    var files = Directory.GetFiles(directoryPath);
                    List<ImageInfoServer> imageInfos = new List<ImageInfoServer>();


                    foreach (var file in files)
                    {
                        using (var filestream = new FileStream(file, FileMode.Open, FileAccess.Read))
                        {

                            var memoryStream = new MemoryStream();
                            await filestream.CopyToAsync(memoryStream);
                            var buffer = memoryStream.ToArray();


                            imageInfos.Add(new ImageInfoServer()
                            {
                                //Base64Url = $"data:{Path.GetExtension(file)};base64,{Convert.ToBase64String(buffer)}"
                                Base64Url = "",
                                FileName = Path.GetFileName(file),
                                IsLocalFile = false
                            });
                        };
                    }
                    return Ok(imageInfos.OrderBy(x=>x.FileName));
                }
                else
                {
                    return BadRequest("Directory do not exist");
                }
            }
            else return BadRequest();
        }

       
        [HttpGet]
        [Route("/GetImageBase64/{folderID}/{filename}")]
        public async Task<IActionResult> GetImageBase64(Guid? folderID,string filename)
        {


            if (folderID != null)
            {
                string directoryPath = Path.Combine(_webHost.WebRootPath, ClientFoldersDir, folderID.ToString());
                if (Directory.Exists(directoryPath))
                {
                    using (var filestream = new FileStream(Path.Combine(directoryPath, filename), FileMode.Open, FileAccess.Read))
                    {
                            var memoryStream = new MemoryStream();
                            await filestream.CopyToAsync(memoryStream);
                            var buffer = memoryStream.ToArray();
                       
                        return Ok(new ImageInfoServer()
                        {
                            Base64Url = $"data:{Path.GetExtension(filename)};base64,{Convert.ToBase64String(buffer)}",
                            FileName = filename,
                            IsLocalFile = false
                        });
                    };
                }
                else
                {
                    return BadRequest("Directory do not exist");
                }
            }
            else return BadRequest();
        }

        [HttpPost]
        [Route("/UploadWaterMark")]
        public async Task<HttpResponseMessage> UploadWaterMark([FromForm] IFormFile files)
        {
            try
            {
                var path = Path.Combine(_webHost.WebRootPath, "Logo.png");
                using MemoryStream ms = new MemoryStream();
                await files.CopyToAsync(ms);
                await System.IO.File.WriteAllBytesAsync(path, ms.ToArray());
                return new HttpResponseMessage(HttpStatusCode.Created);

            }
            catch
            {
                return new HttpResponseMessage(HttpStatusCode.BadRequest);
            }

        }

        [HttpGet]
        [Route("/GetWatermark")]
        public async Task<IActionResult> GetWatermark()
        {
            if (System.IO.File.Exists(Path.Combine(_webHost.WebRootPath, "Logo.png")))
            {
                string Base64Url;
                var fileInfo = new FileInfo(Path.Combine(_webHost.WebRootPath, "Logo.png"));
                using (var filestream = new FileStream(Path.Combine(_webHost.WebRootPath, "Logo.png"), FileMode.Open, FileAccess.Read))
                {
                    var memoryStream = new MemoryStream();
                    await filestream.CopyToAsync(memoryStream);
                    var buffer = memoryStream.ToArray();

                    Base64Url = $"data:{fileInfo.Extension};base64,{Convert.ToBase64String(buffer)}";
                }
                return Ok(Base64Url);
            }
            else
            {
                return BadRequest("Directory do not exist");
            }
        }

    
        [HttpGet]
        [Route("/GetImageStream/{folderID}/{imageName}")]
        public async Task<byte[]> GetImageStream(string folderID, string imageName)
        {
            //Byte[] b = System.IO.File.ReadAllBytes(Path.Combine(_webHost.WebRootPath, ClientFoldersDir, folderID, imageName));           
            //return File(b, "image/jpeg");




            if (folderID != null)
            {
                string filePath = Path.Combine(_webHost.WebRootPath, ClientFoldersDir, folderID, imageName);
                if (System.IO.File.Exists(filePath))
                {

                    using (var filestream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {

                        var memoryStream = new MemoryStream();
                        await filestream.CopyToAsync(memoryStream);
                        return memoryStream.ToArray();

                        //HttpResponseMessage response = new HttpResponseMessage();
                        //response.Content = new StreamContent(memoryStream);
                        //response.Content.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                        //return response;
                    };


                }
                else
                {
                    return null;
                }
            }
            else return null;
        }

        [HttpPost]
        [Route("/UploadFile/{dir}/{WaterMark}")]
        public async Task<IActionResult> PostFile([FromForm] IFormFile files, string dir, bool WaterMark)
        {
            try
            {
                var path = Path.Combine(_webHost.WebRootPath, ClientFoldersDir, dir, files.FileName);
                using MemoryStream ms = new MemoryStream();
                await files.CopyToAsync(ms);
                if (WaterMark)
                {
                   Bitmap bitmap =  await new HelperMethods(_webHost.WebRootPath).WaterMarkImages(ms);
                   bitmap.Save(path, ImageFormat.Jpeg);
                   bitmap.Dispose();
                }
                else { 
                    await System.IO.File.WriteAllBytesAsync(path, ms.ToArray()); 
                }
                
                return Ok("File uploaded succesfully");

            }
            catch 
            {
                return BadRequest("Fail upload error");
            }

        }



        [HttpGet]
        public async Task<ActionResult<String>> CreateUser([FromQuery] Guid folderGuid) 
        {


           return await new HttpClient().GetStringAsync("https://localhost:4001/CreateUser/" + folderGuid);
           
        }



    }
}
