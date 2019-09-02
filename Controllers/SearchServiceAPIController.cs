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
using Microsoft.Extensions.Configuration;

namespace OUCMap_yushan_WebApi.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class SearchServiceAPIController : Controller
    {
        private IHostingEnvironment _environment;
        private ApplicationDbContext _context;
        private IConfiguration _configuration;

        public SearchServiceAPIController(
            IHostingEnvironment environment,ApplicationDbContext context, IConfiguration configuration)
        {
            _environment = environment;
            _context = context;
            _configuration = configuration;
        }
        // GET api/values
        [HttpGet]
       public async Task<IActionResult> Get()
        {   ArrayList list = new ArrayList();
            ArrayList imglist = new ArrayList();

            var buildings = await _context.Buildings.ToListAsync();
            // Dictionary<string,string> dic = new Dictionary<string,string>();
            foreach(Building building in buildings){
                Dictionary<string,string> dic = new Dictionary<string,string>();
            dic.Add("name",building.MC);
            if(building.XLB.Length > 0){
                dic.Add("type",building.XLB);
            }
            else{
                dic.Add("type",building.LB);
            }
           
            string latitude = Convert.ToString(building.Y86);
            string longitude = Convert.ToString(building.X86);
            dic.Add("longitude",longitude);
            dic.Add("latitude",latitude);
                // 视频和图片？
            string URL = _configuration["URL:videoURL"];
            string videoUrl = $"{URL}/api/SearchServiceAPI/getvideo?ID={building.BSM}";
            dic.Add("videoUrl",videoUrl);
            dic.Add("brief",building.WZMS);
            dic.Add("ID",building.BSM);
            dic.Add("superType",building.XLB);
            dic.Add("remarks",building.BZ);
            string webRootPath = _environment.WebRootPath;
            string imgDir = webRootPath + "//Source//"+ building.BSM + "//pic";
            int imgCount = 0;
            if(Directory.Exists(imgDir)){
            DirectoryInfo root = new DirectoryInfo(imgDir);
            FileInfo[] files = root.GetFiles();
            imgCount = files.Length;
            }
            
            dic.Add("imgCount",Convert.ToString(imgCount));
            list.Add(dic);
            }

            // Dictionary<string,string> dic10 = new Dictionary<string, string>();
            // dic10.Add("name","敏行馆");
            // dic10.Add("type","校内单位");
            // dic10.Add("longitude","120.3362850233");
            // dic10.Add("latitude","36.0632380662");
            // dic10.Add("videoUrl","");
            // dic10.Add("brief","");
            // dic10.Add("ID","XDW0002");
            // dic10.Add("superType","校内单位");
            // dic10.Add("remarks","");
            // dic10.Add("imgCount","7");
            // list.Add(dic10);

             return Json(list);
        }


        // GET api/values/5
        [Route("getimage")]
        public IActionResult Get(string ID,string imgIndex)
        {
            string webRootPath = _environment.WebRootPath;
            FileStream imageStream = null;
            try{
            string imgDir = webRootPath + "//Source//"+ ID + "//pic";
            if(!Directory.Exists(imgDir))
             {
                Directory.CreateDirectory(imgDir);
             }
            int imgindexInt = Convert.ToInt16(imgIndex) - 1;
            DirectoryInfo root = new DirectoryInfo(imgDir);
            FileInfo[] files = root.GetFiles();
            string fileName = files[imgindexInt].Name;
            string fileFullName = files[imgindexInt].FullName;
            string extension = fileName.Split('.')[1];
            imageStream = new FileStream(fileFullName,FileMode.Open,FileAccess.Read);
            int length = (int)imageStream.Length;
            byte[] result = new byte[length];
            imageStream.Read(result,0,(int)imageStream.Length - 1);
            string contentType = "image/" + extension;
            return File(result,contentType);
            }
            catch{
                
            }
            finally{
                imageStream.Close();
            }
           
            return null;
        }




        [Route("getvideo")]
        public async Task<FileStreamResult> Get(string ID)
        {
            AppContext.SetSwitch("Switch.Microsoft.AspNetCore.Mvc.EnableRangeProcessing", 
   true);
            string[] videoFormatArray = { "mp4","MP4","mpg","mpeg","avi","mov","asf","wmv","navi","3gp","mkv","f4v","rmvb" };
            string webRootPath = _environment.WebRootPath;
            string imgDir = webRootPath + "//Source//"+ ID + "//video";
            if(!Directory.Exists(imgDir))
             {
                Directory.CreateDirectory(imgDir);
             }
            DirectoryInfo root = new DirectoryInfo(imgDir);
            FileInfo[] files = root.GetFiles();
            string fileName = "";
            string fileFullName = "";
            foreach(FileInfo file in files){
            fileName = file.Name;
            fileFullName = file.FullName;
            string extension = fileName.Split('.')[1];
            if(videoFormatArray.Contains(extension))
            {
            var stream = new FileStream(fileFullName,FileMode.Open,FileAccess.Read);
            string contentType = "video/" + extension;
            return File(System.IO.File.OpenRead(fileFullName),contentType);
            }
            }
             return null;
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