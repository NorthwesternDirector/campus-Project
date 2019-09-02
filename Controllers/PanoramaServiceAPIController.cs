using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.IO;
using System.Xml.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using CPSSnew.Models;
using CPSSnew.Models.MapViewModels;
using CPSSnew.Data;
using System.Diagnostics;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Drawing;
using System.Data;
using System.Web;


namespace OUCMap_yushan_WebApi.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class PanoramaServiceAPIController : Controller
    {
         private IHostingEnvironment _environment;
       private ApplicationDbContext _context;
        public PanoramaServiceAPIController(
            IHostingEnvironment environment,ApplicationDbContext context)
        {
            _environment = environment;
            _context = context;
        }

       
       public  async Task<IActionResult> Get()
        {      
            string webRootPath = _environment.WebRootPath;
            FileStream imageStream = null;
            byte[] fileBytes = null;
            try{
            string imgDir = webRootPath + "\\Source"+ "\\Panorama\\";
            if(!Directory.Exists(imgDir))
             {
                Directory.CreateDirectory(imgDir);
             }
    
            string fileFullName = imgDir + "panoramic.zip";
           
            // int length = (int)imageStream.Length;
            // byte[] result = new byte[length];
            // // imageStream.Read(result,0,(int)imageStream.Length - 1);
            // await imageStream.ReadAsync(result,0,(int)imageStream.Length - 1);
            fileBytes = await System.IO.File.ReadAllBytesAsync(fileFullName);
            string contentType = "application/x-zip-compressed";
            return File(fileBytes,contentType);
            }
            catch(Exception ex){
                imageStream.Close();
                return NotFound();
            }
            finally{
                fileBytes = null;
                // imageStream.Close();
            }
           
            //return null;
        }
        // GET api/values
        
       [Route("getimage")]
       public async Task<IActionResult> Get(string imageID)
        {   
            string webRootPath = _environment.WebRootPath;
            byte[] fileBytes = null;
            try{
            string imgDir = webRootPath + "\\Source"+ "\\Panorama\\";
            if(!Directory.Exists(imgDir))
             {
                Directory.CreateDirectory(imgDir);
             }
    
            string fileFullName = imgDir + imageID + ".jpg";
            fileBytes = await System.IO.File.ReadAllBytesAsync(fileFullName);
            string contentType = "image/" + "jpg";
            return File(fileBytes,contentType);
            }
            catch(Exception ex){
                return NotFound();
            }
            finally{
                fileBytes = null;
            }
           
            //return null;
        }


        //POST api/values
        [HttpPost]
        [AllowAnonymous]
        // public async Task<HttpResponse> Post(string row,string column,string scale)
        // {   
        //      byte[] result = getTile(int.Parse(scale),int.Parse(row),int.Parse(column),"satellite");
        //      Response.ContentType = "image/png";
        //      await Response.Body.WriteAsync(result, 0, result.Length);
        //      return Response;
            
        // }

         public IActionResult Post(string row,string column,string scale)
        {   
            return null;
            
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }


       
} 
}