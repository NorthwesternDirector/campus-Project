using System;
using System.Collections.Generic;
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
using CPSSnew.Data;
using Microsoft.EntityFrameworkCore;
using System.Drawing;

namespace OUCMap_yushan_WebApi.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class testAPIController : Controller
    {

        private ApplicationDbContext _context;
        private IHttpContextAccessor _httpContextAccessor;
        private IHostingEnvironment _environment;
        private readonly IHostingEnvironment _hostingEnvironment;

        public testAPIController(IHttpContextAccessor httpContextAccessor, ApplicationDbContext context, IHostingEnvironment hostingEnvironment, IHostingEnvironment environment)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _hostingEnvironment = hostingEnvironment;
            _environment = environment;
        }

        // GET api/values
        [HttpGet]
        /*public IActionResult Get()
        {

           //维修数据加载
            var WXProjects = _context.WXProjects.ToListAsync();
            string WXProject = Newtonsoft.Json.JsonConvert.SerializeObject(WXProjects);
            ViewBag.WXProjects = WXProject;
            return Json(WXProjects);

            //基建数据加载
           var jjProjects = _context.JJProjects.ToListAsync();
           string jjProject = Newtonsoft.Json.JsonConvert.SerializeObject(jjProjects);
           ViewBag.jjProjects = jjProject;
           

            
           ;
        //建筑数据加载
        var buildings = _context.Buildings.ToListAsync();
            string building = Newtonsoft.Json.JsonConvert.SerializeObject(buildings);
            ViewBag.buildings = building;
            return Json(buildings);
        }*/
        public IActionResult Get()
        {
            int level = 8;
            int row = 16017;
            int col = 752;
            string type = "shape";
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


                //await Response.Body.WriteAsync(result, 0, length);
                //Response.BinaryWrite(result);

                //Response.End();
                return Json(result);
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
                return Json(result);
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

    }
}