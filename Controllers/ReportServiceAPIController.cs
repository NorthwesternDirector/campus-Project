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
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Drawing;
using System.Data;
using System.Web;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace OUCMap_yushan_WebApi.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ReportServiceAPIController : Controller
    {  
         private IHostingEnvironment _environment;
       private ApplicationDbContext _context;
        private IConfiguration _configuration;

        private readonly UserManager<ApplicationUser> _userManager;
        public ReportServiceAPIController(
            IHostingEnvironment environment,ApplicationDbContext context,UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _environment = environment;
            _context = context;
            _configuration = configuration;
        }
        // GET api/values
        [HttpGet]
      public async Task<IActionResult> Get(string Email)
        {   var buildings = await _context.WXProjects.ToListAsync();
            List<WXProject> userBuildings = new List<WXProject>();
            ApplicationUser EditUser = await _userManager.FindByEmailAsync(Email);
            var username = EditUser.UserName;
            var userRoles = await _userManager.GetRolesAsync(EditUser);
            //该用户的权限遍历
            foreach(string name in userRoles){
                if(name == "顶级权限"){
                    userBuildings = buildings;
                    break;
                }
                if(name == "维修权限"){
                    //筛选出该用户的上报信息
                    foreach(WXProject building in buildings){
                        if(building.SBRY == username){
                            userBuildings.Add(building);
                        }
                    }
                    break;
                }
            }
            ArrayList list = new ArrayList();
            ArrayList imglist = new ArrayList();

            
            // Dictionary<string,string> dic = new Dictionary<string,string>();
            foreach(WXProject building in userBuildings){
                Dictionary<string,string> dic = new Dictionary<string,string>();
            if(building.SSLX.Length > 0){
                dic.Add("type",building.SSLX);
            }          
            string longitude = Convert.ToString(building.Y84);
            string latitude = Convert.ToString(building.X84);
            dic.Add("longitude",longitude);
            dic.Add("latitude",latitude);
            dic.Add("uploadTime",building.SBRQ);
            dic.Add("location",building.WZ);
            dic.Add("addition",building.BZ);
            dic.Add("uploadUser",building.SBRY);
            dic.Add("status",building.WXZT);
            dic.Add("workTime",building.WXRQ);
            // 视频和图片？
 
            dic.Add("ID",building.ID);
            string webRootPath = _environment.WebRootPath;
            string imgDir = webRootPath + "//Report//"+ building.ID + "//pic";
            int imgCount = 0;
            if(Directory.Exists(imgDir)){
            DirectoryInfo root = new DirectoryInfo(imgDir);
            FileInfo[] files = root.GetFiles();
            imgCount = files.Length;
            if(imgCount > 4){
                imgCount = 4;
            }
            }
            
            dic.Add("imgCount",Convert.ToString(imgCount));
            string videoDir = webRootPath + "//Report//"+ building.ID + "//video";
            if(!Directory.Exists(videoDir))
             {
                Directory.CreateDirectory(videoDir);
             }
            DirectoryInfo videoroot = new DirectoryInfo(videoDir);
            FileInfo[] videofiles = videoroot.GetFiles();
            int videoCount = 0;
            videoCount = videofiles.Length;
            if(videoCount > 4){
                videoCount = 4;
            }
            dic.Add("videoCount",Convert.ToString(videoCount));
            list.Add(dic);

            }
             return Json(list);
        }



         [Route("getmedia")]
        public IActionResult Get(string ID,string Index,string mediaType)
        {   if(mediaType == "image"){
            string webRootPath = _environment.WebRootPath;
            FileStream imageStream = null;
            try{
            string imgDir = _environment.ContentRootPath + "//wwwroot//Report//" + ID + "//pic//";
            int imgindexInt = Convert.ToInt16(Index) - 1;
            DirectoryInfo root = new DirectoryInfo(imgDir);
            FileInfo[] files = root.GetFiles();
            string fileName = files[imgindexInt].Name;
            string fileFullName = files[imgindexInt].FullName;
            string extension = fileName.Split('.')[1];
            string[] videoFormatArray = { "mp4","MP4","mpg","mpeg","avi","mov","asf","wmv","navi","3gp","mkv","f4v","rmvb" };
            if(!videoFormatArray.Contains(extension))
            {
            imageStream = new FileStream(fileFullName,FileMode.Open,FileAccess.Read);
            int length = (int)imageStream.Length;
            byte[] result = new byte[length];
            imageStream.Read(result,0,(int)imageStream.Length - 1);
            string contentType = "image/" + extension;
            return File(result,contentType);
            }
            
            }
            catch{
                
            }
            finally{
                imageStream.Close();
            }
        }
        else if(mediaType == "video"){
             AppContext.SetSwitch("Switch.Microsoft.AspNetCore.Mvc.EnableRangeProcessing", 
   true);
            string[] videoFormatArray = { "mp4","MP4","mpg","mpeg","avi","MOV","mov","asf","wmv","navi","3gp","mkv","f4v","rmvb" ,"m4v"};
            string webRootPath = _environment.WebRootPath;
            string imgDir = _environment.ContentRootPath + "//wwwroot//Report//"+ ID + "//video";
            if(!Directory.Exists(imgDir))
             {
                Directory.CreateDirectory(imgDir);
             }
            DirectoryInfo root = new DirectoryInfo(imgDir);
            FileInfo[] files = root.GetFiles();
            string fileName = "";
            string fileFullName = "";
            int imgindexInt = Convert.ToInt16(Index) - 1;
            FileInfo file = files[imgindexInt];
            fileName = file.Name;
            fileFullName = file.FullName;
            string extension = fileName.Split('.')[1];
            if (videoFormatArray.Contains(extension))
            {
                var stream = new FileStream(fileFullName, FileMode.Open, FileAccess.Read);
                string contentType = "video/";
                if (extension == "MOV" || extension == "mov")
                {
                    contentType = contentType + "quicktime";
                }
                else
                {
                    contentType = contentType + extension;
                }

                return File(System.IO.File.OpenRead(fileFullName), contentType);
            }

            }
            
           
            return null;
        }




        //POST api/values
        [HttpPost]

         public async Task<IActionResult> Post()
        {   string[] pictureFormatArray = { "png", "jpg", "jpeg", "bmp", "gif","ico", "PNG", "JPG", "JPEG", "BMP", "GIF","ICO" };
            //对象标识 & 
            string uploadTime = DateTime.Now.ToString("yyyy/MM/dd");
            string uploadTime1 = DateTime.Now.ToString("yyyyMMdd");
            var projects = await _context.WXProjects.ToListAsync();
             int offset = 1;
            /*foreach(WXProject project in  projects){--------------------20190621
                if(project.SBRQ == uploadTime){
                    offset++;
                }
            }*/
            int maxNum = 0;
            foreach (WXProject project in projects)
            {
                if (project.ID.Substring(0, 8) == uploadTime1)
                {
                    //判断是否有中间数字文件被删除后导致计数与有ID发生冲突
                    string incomingData = project.ID.Substring(8, 4);
                    int parsedResult;

                    if (int.TryParse(incomingData, out parsedResult))
                    {
                        if (maxNum <= parsedResult)
                        {
                            maxNum = parsedResult;
                        }

                    }

                    offset++;
                }
            };
            maxNum = maxNum + 1;

            //string offsetString = Convert.ToString(offset);

            string offsetString = Convert.ToString(maxNum);
            string offsetString2 = "0000" + offsetString;
            offsetString2 = offsetString2.Substring(offsetString.Length);
            string ID = DateTime.Now.ToString("yyyyMMdd") + offsetString2;
            var uploadFile = Request.Form.Files;
            //上报人员
            string user = Request.Form["uploadPerson"];
            //设施类型
            string type = Request.Form["type"];
            //位置
            string locationName = Request.Form["locationName"];
            //X Y坐标
            string latitude = Request.Form["CoordinateLatitude"];
            double latitudeDouble = Convert.ToDouble(latitude);
            string longitude = Request.Form["CoordinateLongitude"];
            double longitudeDouble = Convert.ToDouble(longitude);
            //上报日期
            string uploadDate = DateTime.Now.ToString("yyyy/MM/dd");
            //备注
            string addition = Request.Form["addition"];
            
            WXProject myWXProject = new WXProject();
            myWXProject.ID = ID;
            myWXProject.SSLX = type;
            myWXProject.SBRY = user;
            myWXProject.SBRQ = uploadDate;
            myWXProject.WZ = locationName;
            myWXProject.X84 = latitudeDouble;
            myWXProject.Y84 = longitudeDouble;
            myWXProject.BZ = addition;




            /* **********修改************************************************************ */
            string URL = _configuration["URL:transformURL"];
            string toqdUrl = $"{URL}/Web.asmx/WGS84ToQingdao96?lon={longitude}&lat={latitude}";
            //string toqdUrl = $"{APIURLManager.APIURLManager.transformURL}/transfer/BL2XY?fromCoord=wgs84&toCoord=qingdao96&lon={longitude}&lat={latitude}&callback=";
            //string xylocaiton =await HttpGetAsync(toqdUrl,System.Text.Encoding.GetEncoding("utf-8"));
            /*xylocaiton = xylocaiton.Substring(1,xylocaiton.Length - 2);
            JObject xylocaitonJson = (JObject)JsonConvert.DeserializeObject(xylocaiton);
            string x = xylocaitonJson["x"].ToString();
            string y = xylocaitonJson["y"].ToString();*/
            Dictionary<string, string> xylocaiton = await HttpGetAsync(toqdUrl, System.Text.Encoding.GetEncoding("utf-8"));
            string x = xylocaiton["longitude"];
            string y = xylocaiton["latitude"];
            myWXProject.X = Convert.ToDouble(x);
            myWXProject.Y = Convert.ToDouble(y);
            /* **********修改************************************************************ */

            myWXProject.WXZT = "待处理";
            _context.WXProjects.Add(myWXProject);
            _context.SaveChanges();

            //图片 视频
            if(uploadFile.Count > 0){
                string filePath = _environment.ContentRootPath + "\\wwwroot\\Report\\" + myWXProject.ID ;
             if(!Directory.Exists(filePath))
             {
                Directory.CreateDirectory(filePath);
             }
                foreach(var file in uploadFile){
                    var filename = DateTime.Now.ToString("yyyyMMdd") + "_" + ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.ToString().Trim('"');
                    string extension = filename.Split('.')[1];
                    string imagePath = filePath + "\\";
                    if(!pictureFormatArray.Contains(extension)){
                        imagePath = imagePath + "video"; 
                    }
                    else{
                        imagePath = imagePath + "pic";
                    }
                    if(!Directory.Exists(imagePath))
                    {
                        Directory.CreateDirectory(imagePath);
                    }
                    imagePath = imagePath + "\\" + filename;
                    
                 using(FileStream fs = System.IO.File.Create(imagePath))
                 {
                      await file.CopyToAsync(fs);
                      fs.Flush();
                 }
                }
                    string message = $"{uploadFile.Count} file(s) uploaded successfully!";
                
                    return Json(message);
            }
            
            return Json("上传成功（无视频或图片）");
        }

        public static async Task<Dictionary<string, string>> HttpGetAsync(string url, Encoding encoding = null)
        {
            HttpClient httpClient = new HttpClient();
            var data = await httpClient.GetAsync(url);
            var stream = data.Content.ReadAsStreamAsync().Result;
            var itemXML = XElement.Load(stream);
            string longitude = ((System.Xml.Linq.XText)((System.Xml.Linq.XContainer)itemXML.FirstNode).LastNode).Value;
            string latitude = ((System.Xml.Linq.XText)((System.Xml.Linq.XContainer)itemXML.LastNode).FirstNode).Value;
            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("latitude", latitude);
            dic.Add("longitude", longitude);
            return dic;
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