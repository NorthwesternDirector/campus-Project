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

namespace OUCMap_yushan_WebApi.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class MapServiceAPIController : Controller
    {
        private IHostingEnvironment _environment;
        public MapServiceAPIController(
            IHostingEnvironment environment)
        {
            _environment = environment;
        }
        // GET api/values
        [HttpGet]
       public IActionResult Get()
        {
           //byte[] result = getTile(7,8008,376,"landscape");
        //    var image = new System.Drawing.Bitmap(result);
             
             //Console.WriteLine(result.Length);
             return Json("map service");
        }
        [Route("getmap")]
        [AllowAnonymous]
        public IActionResult Get(long row,long column,long scale,string type){
            byte[] result = getTile(scale,row,column,type);
             //Console.WriteLine(result.Length);
             if(result != null){
             return File(result,"image/png");
             }
             return null;
        }
        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "userId - " + id;
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
             byte[] result = getTile(int.Parse(scale),int.Parse(row),int.Parse(column),"satellite");
             Console.WriteLine(result.Length);
             return File(result,"image/png");
            
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

       public byte[] getTile(long level, long row, long col, string type)
        {
            int size = 128;
            byte[] result = null;
            FileStream isBundle = null;
            FileStream isBundlx = null;
            string webRootPath = _environment.WebRootPath;
            // string contentRootPath = _environment.ContentRootPath;

            // string nullpicpath = webRootPath +"\\tile_null.png";
            try
            {
                string bundlesDir = webRootPath + "\\Map\\"+ type + "\\Layers\\_alllayers";
                Console.WriteLine(bundlesDir);
                string l = "0" + level;
                int lLength = l.Length;
                if (lLength > 2)
                {
                    l = l.Substring(lLength - 2);
                }
                l = "L" + l;

                long rGroup = size * (row / size);
                string r = rGroup.ToString("X");
                //string r = "000" + rGroup.ToString("X");
                //int rLength = r.Length;
                while (r.Length < 4)
                {
                    r = "0" + r;
                }
                //if (rLength > 4)
                //{
                //    r = r.Substring(rLength - 4);
                //}
                r = "R" + r;

                long cGroup = size * (col / size);
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

                long index = size * (col - cGroup) + (row - rGroup);
                //行列号是整个范围内的，在某个文件中需要先减去前面文件所占有的行列号（都是128的整数）这样就得到在文件中的真是行列号
                isBundlx = new FileStream(bundlxFileName, FileMode.Open, FileAccess.Read);
                isBundlx.Seek(16 + 5 * index, SeekOrigin.Begin);
                byte[] buffer = new byte[5];
                isBundlx.Read(buffer, 0, 5);
                long offset = (long)(buffer[0] & 0xff)
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

                System.IO.MemoryStream ms = new System.IO.MemoryStream(result);
                // System.Drawing.image img = System.Drawing.Image.FromStream(ms);
                // img.Save(bundlesDir + "//" + l + "//" + row.ToString() + "_" + col.ToString() + ".png", System.Drawing.Imaging.ImageFormat.Png);

               
                //Response.BinaryWrite(result);
                
                // Response.Body.EndWrite();
                return result;
            }
            catch (Exception ex)
             {
            //     FileStream fs = new FileStream(nullpicpath, FileMode.Open, FileAccess.Read);
            //     Bitmap myImage = new Bitmap(fs);
            //     MemoryStream ms = new MemoryStream();
            //     myImage.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            //     //Response.BinaryWrite(result);
            //     //Response.Body.WriteAsync(ms.ToArray(),0,length);
            //     //Response.End();
            //     return;
                    return null;
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