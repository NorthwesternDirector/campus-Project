using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using CPSSnew.Models;
using CPSSnew.Data;
using CPSSnew.Models.MapViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Drawing;
using System.Data;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using System.Xml.Linq;

namespace CPSSnew.Controllers
{
    public class MapController : Controller
    {
        private ApplicationDbContext _context;
        private IHttpContextAccessor _httpContextAccessor;
        private IHostingEnvironment _environment;
        private IConfiguration _configuration;
       private readonly IHostingEnvironment _hostingEnvironment;

        public MapController(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context, IHostingEnvironment hostingEnvironment, IHostingEnvironment environment, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _environment = environment;
            _configuration = configuration;
        }


        public IActionResult Index()
        {
            return View();
        }

        //地图初始化
        public async Task<IActionResult> Map()
        {
            //建筑数据加载
            var buildings = await _context.Buildings.ToListAsync();
            string building = Newtonsoft.Json.JsonConvert.SerializeObject(buildings);
            ViewBag.buildings = building;

            //维修数据加载
            var WXProjects = await _context.WXProjects.ToListAsync();
            string WXProject = Newtonsoft.Json.JsonConvert.SerializeObject(WXProjects);
            ViewBag.WXProjects = WXProject;

            //基建数据加载
            var jjProjects = await _context.JJProjects.ToListAsync();
            string jjProject = Newtonsoft.Json.JsonConvert.SerializeObject(jjProjects);
            ViewBag.jjProjects = jjProject;

            var mapURL = _configuration["URL:mapURL"];

            //var mapURL = APIURLManager.APIURLManager.mapURL;
            ViewBag.mapURLs = mapURL;

            var roadURL = _configuration["URL:roadURL"];
            //var roadURL = APIURLManager.APIURLManager.roadURL;
            ViewBag.roadURLs = roadURL;

            return View();
        }
        
        //获取建筑文件夹下的图片地址
        [HttpGet]
        public List<string> getFileNum(string BSM,string type)
        {
            if (type == "image") {//图片文件
                try
                {
                    List<String> fileName = new List<String>();
                    if (!System.IO.Directory.Exists(@"wwwroot\Source\" + BSM + "\\pic"))
                    {
                        //System.IO.Directory.CreateDirectory(@"wwwroot\Source\" + BSM + "\\pic");//不存在就创建目录
                        return fileName;
                    }
                    string path = @"wwwroot\Source\" + BSM + "\\pic"; // 文件夹目录。。
                    
                    var files = Directory.GetFiles(path);
                    foreach (var file in files)
                        fileName.Add(file);
                    return fileName;
                }
                catch (Exception ex)
                {
                    List<String> error = new List<String>();
                    error.Add(ex.Message);
                    return error;
                }
            }else //视频文件
            {
                try
                {
                    List<String> fileName = new List<String>();
                    if (!System.IO.Directory.Exists(@"wwwroot\Source\" + BSM + "\\video"))
                    {
                        return fileName;
                        //System.IO.Directory.CreateDirectory(@"wwwroot\Source\" + BSM + "\\video");//不存在就创建目录
                    }
                    string path = @"wwwroot\Source\" + BSM + "\\video"; // 文件夹目录。。
                    
                    var files = Directory.GetFiles(path);
                    foreach (var file in files)
                        fileName.Add(file);
                    return fileName;
                }
                catch (Exception ex)
                {
                    List<String> error = new List<String>();
                    error.Add(ex.Message);
                    return error;
                }
            }
            
        }


        //-----------------------------维修类-----------------------------//

        //维修项目数据-增
        [HttpPost]
        public async Task<IActionResult> WXSave(string datas)
        {
            WXProject myWXProject = JsonConvert.DeserializeObject<WXProject>(datas);
            //坐标转换96-84
            string URL = _configuration["URL:transformURL"]; 
            //string towgsUrl = $"{URL}/transfer/XY2BL?fromCoord=qingdao96&toCoord=wgs84&x={myWXProject.X}&y={myWXProject.Y}&callback=";
            string towgsUrl = $"{URL}/Web.asmx/Qingdao96ToWGS84?x={myWXProject.X}&y={myWXProject.Y}";
            Dictionary<string, string> xylocaiton = await HttpGetAsync(towgsUrl, System.Text.Encoding.GetEncoding("utf-8"));
          
            string x = xylocaiton["longitude"];
            string y = xylocaiton["latitude"];
            myWXProject.X84 = Convert.ToDouble(y);
            myWXProject.Y84 = Convert.ToDouble(x);

            _context.WXProjects.Add(myWXProject);
            _context.SaveChanges();

            var wxProjects = await _context.WXProjects.ToListAsync();
            string wxProject = Newtonsoft.Json.JsonConvert.SerializeObject(wxProjects);

            //return wxProject;
            return Json(wxProject);
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



        //维修项目数据-删
        [HttpPost]
        public async Task<IActionResult> WXDelete(string id)
        {
            try
            {   //创建删除对象
                WXProject project = _context.WXProjects.Find(id);
                _context.WXProjects.Remove(project);
                //更新到数据库
                _context.SaveChanges();

                var filepath = _environment.ContentRootPath + "/wwwroot/Report" ;//维修上传根目录
                var filepath1 = _environment.ContentRootPath + "/wwwroot/Report\\"+id;//维修上传子目录

                for (int i = 0; i < Directory.GetDirectories(filepath).ToList().Count; i++)//删除对应的图像与视频文件
                {
                    var ff = Directory.GetDirectories(filepath).ToList();
                    if (Directory.GetDirectories(filepath)[i] == filepath1)
                    {
                        Directory.Delete(filepath1, true);
                    }
                }
            }
            catch (Exception ex)
            {
            }
            var wxProjects = await _context.WXProjects.ToListAsync();
            string wxProject = Newtonsoft.Json.JsonConvert.SerializeObject(wxProjects);
            return Json(wxProject);
        }

        //维修项目数据-改
        [HttpPost]
        public async Task<IActionResult> WXUpdate(string datas)
        {
            WXProject myWXProject = JsonConvert.DeserializeObject<WXProject>(datas);
            //update
            //先查询要修改的原数据
            WXProject customer = _context.WXProjects.Find(myWXProject.ID);
            //设置修改后的值
           // customer.ID = myWXProject.ID;
            customer.SSLX = myWXProject.SSLX;
            customer.SBRY = myWXProject.SBRY;
            customer.WXZT = myWXProject.WXZT;
            customer.WZ = myWXProject.WZ;
            customer.SBRQ = myWXProject.SBRQ;
            customer.WXRQ = myWXProject.WXRQ;
            //customer.X = myWXProject.X;
            //customer.Y = myWXProject.Y;
            //customer.X84 = myWXProject.X84;
            //customer.Y84 = myWXProject.Y84;
            customer.BZ = myWXProject.BZ;
            //更新到数据库
            _context.SaveChanges();

            var wxProjects = await _context.WXProjects.ToListAsync();
            string wxProject = Newtonsoft.Json.JsonConvert.SerializeObject(wxProjects);
            return Json(wxProject);
        }

        //获取数据库同日期的上传条数
        [HttpGet]
        public int getWXUploadNum()
        {
            try
            {
                string uploadTime = DateTime.Now.ToString("yyyy/MM/dd");//从数据库读取当日存在维修项目数量
                string uploadTime1 = DateTime.Now.ToString("yyyyMMdd");
                var projects = _context.WXProjects.ToList();
                int offset = 1;
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
                            if (maxNum <= parsedResult) {
                                maxNum = parsedResult;
                            }

                        }
                      
                        offset++;
                    }
                }
                maxNum = maxNum + 1;
                //return offset;
                return maxNum;
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        //维修上报上传图片至本地
        public JsonResult UpLoadfileImage()
        {
            //var userName = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name).Value;
            uploadResult result = new uploadResult();
            try
            {
                string uploadTime = DateTime.Now.ToString("yyyy/MM/dd");//从数据库读取当日存在维修项目数量
                string uploadTime1 = DateTime.Now.ToString("yyyyMMdd");
                var projects = _context.WXProjects.ToList();
                int offset = 1;
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
                }
                maxNum = maxNum + 1;
                string offsetString = Convert.ToString(maxNum);
                string offsetString2 = "0000" + offsetString;
                offsetString2 = offsetString2.Substring(offsetString.Length);
                string ID = uploadTime1 + offsetString2;
                var folderName = ID;
                result.fileID = folderName;

                IFormFile oFile = Request.Form.Files["uploadfiles"];
                Stream sm = oFile.OpenReadStream();

                result.fileName = DateTime.Now.ToString("yyyyMMddhhmmss") + "_" + oFile.FileName;
                result.filePath = "../../Report/" + folderName + "/pic/" + result.fileName;

                var filepath = _environment.ContentRootPath + "/wwwroot/Report/" + folderName + "/pic";
                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                };
                string filename = filepath + "/" + result.fileName;
                FileStream fs = new FileStream(filename, FileMode.Create);
                byte[] buffer = new byte[sm.Length];
                sm.Read(buffer, 0, buffer.Length);
                fs.Write(buffer, 0, buffer.Length);
                fs.Dispose();
            }
            catch (Exception ex)
            {
            }
            return Json(result);
        }

        //维修上报上传视频至本地
        public JsonResult UpLoadfileVideo()
        {
            //var userName = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.Name).Value;
            uploadResult result = new uploadResult();
            try
            {
                string uploadTime = DateTime.Now.ToString("yyyy/MM/dd");//从数据库读取当日存在维修项目数量
                string uploadTime1 = DateTime.Now.ToString("yyyyMMdd");
                var projects = _context.WXProjects.ToList();
                int offset = 1;
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
                string offsetString = Convert.ToString(maxNum);
                string offsetString2 = "0000" + offsetString;
                offsetString2 = offsetString2.Substring(offsetString.Length);
                string ID = uploadTime1 + offsetString2;
                var folderName = ID;
                IFormFile oFile = Request.Form.Files["uploadfiles"];
                Stream sm = oFile.OpenReadStream();

                result.fileName = DateTime.Now.ToString("yyyyMMddhhmmss") + "_" + oFile.FileName;
                result.filePath = "../../Report/" + folderName + "/video/" + result.fileName;

                var filepath = _environment.ContentRootPath + "/wwwroot/Report/" + folderName + "/video";
                if (!Directory.Exists(filepath))
                {
                    Directory.CreateDirectory(filepath);
                }
                string filename = filepath + "/" + result.fileName;
                FileStream fs = new FileStream(filename, FileMode.Create);
                byte[] buffer = new byte[sm.Length];
                sm.Read(buffer, 0, buffer.Length);
                fs.Write(buffer, 0, buffer.Length);
                fs.Dispose();
            }
            catch (Exception ex)
            {
            }
            return Json(result);
        }
      
        //获取维修图片
        [HttpGet]
        public List<string> getFileWX(string BSM)
        {
            try
            {
                string path = @"wwwroot\Report\" + BSM + "\\pic"; // 文件夹目录。。
                List<String> fileName = new List<String>();
                var files = Directory.GetFiles(path);
                foreach (var file in files)
                    fileName.Add(file);
                return fileName;
            }
            catch (Exception ex)
            {
                List<String> error = new List<String>();
                error.Add(ex.Message);
                return error;
            }

        }

        //获取维修视频
        [HttpGet]
        public List<string> getFileWX_Video(string BSM)
        {
            try
            {
                string path = @"wwwroot\Report\" + BSM + "\\video"; // 文件夹目录。。
                List<String> fileName = new List<String>();
                var files = Directory.GetFiles(path);
                foreach (var file in files)
                    fileName.Add(file);
                return fileName;
            }
            catch (Exception ex)
            {
                List<String> error = new List<String>();
                error.Add(ex.Message);
                return error;
            }

        }

        public class uploadResult
        {
            public string fileName { get; set; }
            public string filePath { get; set; }
            public string fileID { get; set; }

        }


        //-----------------------------基建类-----------------------------//

        //基建项目数据-增
        [HttpPost]
        public async Task<IActionResult> JJSave(string datas)
        {
            JJProject myJJProject = JsonConvert.DeserializeObject<JJProject>(datas);
            _context.JJProjects.Add(myJJProject);
            _context.SaveChanges();

            var jjProjects = await _context.JJProjects.ToListAsync();
            string jjProject = Newtonsoft.Json.JsonConvert.SerializeObject(jjProjects);
            return Json(jjProject);

        }

        //基建项目数据-删
        [HttpPost]
        public async Task<IActionResult> JJDelete(string id)
        {
            try
            {   //创建删除对象
                JJProject project = _context.JJProjects.Find(id);
                _context.JJProjects.Remove(project);
                //更新到数据库
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
            }
            var jjProjects = await _context.JJProjects.ToListAsync();
            string jjProject = Newtonsoft.Json.JsonConvert.SerializeObject(jjProjects);
            return Json(jjProject);
        }

        //基建项目数据-改
        [HttpPost]
        public async Task<IActionResult> JJUpdate(string datas)
        {
            JJProject myJJProject = JsonConvert.DeserializeObject<JJProject>(datas);
            //先查询要修改的原数据
            JJProject customer = _context.JJProjects.Find(myJJProject.ID);
            //设置修改后的值
            //customer.ID = myJJProject.ID;
            //customer.MC = myJJProject.MC;
            customer.XMDD = myJJProject.XMDD;
            customer.SGDW = myJJProject.SGDW;
            customer.FZR = myJJProject.FZR;
            customer.LXDH = myJJProject.LXDH;
            customer.JSZT = myJJProject.JSZT;
            customer.JSXZ = myJJProject.JSXZ;
            customer.KGRQ = myJJProject.KGRQ;
            customer.WGRQ = myJJProject.WGRQ;
            customer.YSZJ = myJJProject.YSZJ;
            customer.SYZJ = myJJProject.SYZJ;
            customer.XMMS = myJJProject.XMMS;
            customer.BZ = myJJProject.BZ;
            //更新到数据库
            _context.SaveChanges();

            var jjProjects = await _context.JJProjects.ToListAsync();
            string jjProject = Newtonsoft.Json.JsonConvert.SerializeObject(jjProjects);
            return Json(jjProject);
        }



        //-----------------------------更新数据-----------------------------//

        [HttpPost]
        public async Task<IActionResult> tableDataUpdate(string type)
        {
           
           if (type == "jj") {
                var jjProjects = await _context.JJProjects.ToListAsync();
                string jjProject = Newtonsoft.Json.JsonConvert.SerializeObject(jjProjects);
                return Json(jjProject);
           }else
            {
                var wxProjects = await _context.WXProjects.ToListAsync();
                string wxProject = Newtonsoft.Json.JsonConvert.SerializeObject(wxProjects);
                return Json(wxProject);
            };
            
        }



        //-----------------------------地图服务-----------------------------//

        [Authorize]
        //获取地图影像
        public async Task getTile(int level, int row, int col, string type)
        {

            int size = 128;
            byte[] result = null;
            FileStream isBundle = null;
            FileStream isBundlx = null;
            string webRootPath = _hostingEnvironment.WebRootPath;
            string contentRootPath = _hostingEnvironment.ContentRootPath;
            string nullpicpath = webRootPath + "//tile_null.png";
            try
            {
                string bundlesDir = webRootPath + "\\Map\\" + type + "\\Layers\\_alllayers";
                Console.WriteLine(bundlesDir);
                string l = "0" + level;
                int lLength = l.Length;
                if (lLength > 2)
                {
                    l = l.Substring(lLength - 2);
                }
                l = "L" + l;

                int rGroup = size * (row / size);
                string r = rGroup.ToString("X");//16进制
                while (r.Length < 4)
                {
                    r = "0" + r;
                }
                //if (rLength > 4)
                //{
                //    r = r.Substring(rLength - 4);
                //}
                r = "R" + r;

                int cGroup = size * (col / size);
                string c = cGroup.ToString("X");
                int cLength = c.Length;
                while (c.Length < 4)
                {
                    c = "0" + c;
                }
                c = "C" + c;

                string bundleBase = bundlesDir + "\\" + l + "\\" + r + c;
                string bundlxFileName = bundleBase + ".bundlx";
                string bundleFileName = bundleBase + ".bundle";

                int index = size * (col - cGroup) + (row - rGroup);
                //行列号是整个范围内的，在某个文件中需要先减去前面文件所占有的行列号（都是128的整数）这样就得到在文件中的真是行列号
                isBundlx = new FileStream(bundlxFileName, FileMode.Open, FileAccess.Read);
                isBundlx.Seek(16 + 5 * index, SeekOrigin.Begin);
                byte[] buffer = new byte[5];
                isBundlx.Read(buffer, 0, 5);
                long offset = (long)(buffer[0] & 0xff)//读取偏移量
                       + (long)(buffer[1] & 0xff) * 256
                       + (long)(buffer[2] & 0xff) * 65536
                       + (long)(buffer[3] & 0xff) * 16777216
                       + (long)(buffer[4] & 0xff) * 4294967296L;

                isBundle = new FileStream(bundleFileName, FileMode.Open, FileAccess.Read);
                isBundle.Seek(offset, SeekOrigin.Begin);
                byte[] lengthBytes = new byte[4];
                isBundle.Read(lengthBytes, 0, 4);
                int length = (int)(lengthBytes[0] & 0xff)
                        + (int)(lengthBytes[1] & 0xff) * 256
                        + (int)(lengthBytes[2] & 0xff) * 65536
                        + (int)(lengthBytes[3] & 0xff) * 16777216;
                result = new byte[length];
                isBundle.Read(result, 0, length);

                //System.IO.MemoryStream ms = new System.IO.MemoryStream(result);
                //System.Drawing.Image img = System.Drawing.Image.FromStream(ms);
                // img.Save(bundlesDir + "//" + l + "//" + row.ToString() + "_" + col.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);

                Response.ContentType = "image/png";


                await Response.Body.WriteAsync(result, 0, length);
                //Response.BinaryWrite(result);

                //Response.End();
                return;
            }
            catch (Exception ex)
            {
                FileStream fs = new FileStream(nullpicpath, FileMode.Open, FileAccess.Read);
                Bitmap myImage = new Bitmap(fs);
                MemoryStream ms = new MemoryStream();
                myImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                //Response.BinaryWrite(result);
                //Response.Body.WriteAsync(ms.ToArray(),0,length);
                //Response.End();
                return;
            }
            finally
            {
                if (isBundle != null)
                {
                    isBundle.Close();
                    isBundlx.Close();
                }
            }
        }


        public async Task<IActionResult> getLatestData(string id)
        {
            if (id == "jj")
            {
                var jjProjects = await _context.JJProjects.ToListAsync();
                string jjProject = Newtonsoft.Json.JsonConvert.SerializeObject(jjProjects);
                return Json(jjProject);
            }else if(id == "wx")
            {
                var jjProjects = await _context.JJProjects.ToListAsync();
                string jjProject = Newtonsoft.Json.JsonConvert.SerializeObject(jjProjects);
                return Json(jjProject);
            }
            return Json("");
        }

    }
}