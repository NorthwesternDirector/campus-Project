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
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;

namespace OUCMap_yushan_WebApi.Controllers
{
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class ProjectServiceAPIController : Controller
    {
        private IHostingEnvironment _environment;
        private IConfiguration _configuration;

        private ApplicationDbContext _context;
        public ProjectServiceAPIController(
            IHostingEnvironment environment,ApplicationDbContext context, IConfiguration configuration)
        {
            _environment = environment;
            _context = context;
            _configuration = configuration;
        }
        // GET api/values
        [HttpGet]
       public async Task<IActionResult> Get()
        {       ArrayList list = new ArrayList();
                var jjProjects = await _context.JJProjects.ToListAsync();
                foreach(JJProject project in jjProjects){
                 Dictionary<string,string> dic = new Dictionary<string, string>();
                    dic.Add("ID",project.ID);
                    dic.Add("name",project.MC);
                    dic.Add("startTime",project.KGRQ);
                    dic.Add("type",project.JSXZ);
                    dic.Add("status",project.JSZT);
                    dic.Add("address",project.XMDD);
                    dic.Add("endTime",project.WGRQ);
                    dic.Add("dipict",project.XMMS);
                    dic.Add("builder",project.SGDW);
                    string budget = Convert.ToString(project.YSZJ);
                    string costFund = Convert.ToString(project.SYZJ);
                    dic.Add("budget",budget);
                    dic.Add("costFund",costFund);
                    dic.Add("responser",project.FZR);
                    dic.Add("phoneNumber",project.LXDH);
                    dic.Add("addition",project.BZ);
                    //* **********修改************************************************************ */
                    // 中心坐标和坐标串解析
                    //中心坐标
                    string centerXYString = project.XMZBZX; 
                    string[] centerXYArray = Regex.Split(centerXYString,",",RegexOptions.IgnoreCase);
                    string centerX96 = centerXYArray[0];
                    string centerY96 = centerXYArray[1];
                    //string URL = APIURLManager.APIURLManager.transformURL;
                    string URL = _configuration["URL:transformURL"];
                    string towgsUrl = $"{URL}/Web.asmx/Qingdao96ToWGS84?x={centerX96}&y={centerY96}";
                    //string towgsUrl = $"{URL}/transfer/XY2BL?fromCoord=qingdao96&toCoord=wgs84&x={centerX96}&y={centerY96}&callback=";
                    //string centerXYJsonString =await HttpGetAsync(towgsUrl,System.Text.Encoding.GetEncoding("utf-8"));
                    /*centerXYJsonString = centerXYJsonString.Substring(1,centerXYJsonString.Length - 2);
                    JObject centerXYlocaitonJson = (JObject)JsonConvert.DeserializeObject(centerXYJsonString);
                    string centerX84string = centerXYlocaitonJson["lat"].ToString();
                    string centerY84stirng = centerXYlocaitonJson["lon"].ToString();*/
                    Dictionary<string, string> centerXYJsonString = await HttpGetAsync(towgsUrl, System.Text.Encoding.GetEncoding("utf-8"));
                    string centerX84string = centerXYJsonString["latitude"];
                    string centerY84stirng = centerXYJsonString["longitude"];
                    dic.Add("centerlatitude",centerX84string);
                    dic.Add("centerlongitude",centerY84stirng);
                    //dic.Add("centerlatitude", "0");
                    //dic.Add("centerlongitude", "0");
                    //坐标类型
                    string shapeType = project.XMZBLX;
                    shapeType = shapeType.Substring(1,shapeType.Length-2);
                    dic.Add("locationType",shapeType);
                    //坐标串
                    string XYString = project.XMZBC; 
                    if(shapeType == "Polygon"){
                        XYString = XYString.Substring(2,XYString.Length - 4);
                    }
                    else if(shapeType == "LineString"){
                        XYString = XYString.Substring(1,XYString.Length - 2);
                    }
                    else if(shapeType == "Point"){

                    }
                    if(shapeType == "Polygon" || shapeType == "LineString"){
                    string[] xyArray = Regex.Split(XYString,",",RegexOptions.IgnoreCase);
                    string wgs84XYString = "";
                    for (int i = 0 ;i <xyArray.Length ; i = i + 2){
                        string x96string = xyArray[i];
                        x96string = x96string.Substring(1,x96string.Length-1);
                        string y96string = xyArray[i+1];
                        y96string = y96string.Substring(0,y96string.Length - 1);   
                        string towgsUrl2 = $"{URL}/Web.asmx/Qingdao96ToWGS84?x={x96string}&y={y96string}";
                    //string towgsUrl2 = $"{URL}/transfer/XY2BL?fromCoord=qingdao96&toCoord=wgs84&x={x96string}&y={y96string}&callback=";
                    //string wgs84XYJsonString =await HttpGetAsync(towgsUrl2,System.Text.Encoding.GetEncoding("utf-8"));
                    /*wgs84XYJsonString = wgs84XYJsonString.Substring(1,wgs84XYJsonString.Length - 2);
                    JObject wgs84XYlocaitonJson = (JObject)JsonConvert.DeserializeObject(wgs84XYJsonString);
                    string x84stirng  = wgs84XYlocaitonJson["lat"].ToString();
                    string y84string = wgs84XYlocaitonJson["lon"].ToString();*/
                    Dictionary<string, string> wgs84XYJsonString = await HttpGetAsync(towgsUrl2, System.Text.Encoding.GetEncoding("utf-8"));
                    string x84stirng = wgs84XYJsonString["latitude"];
                    string y84string = wgs84XYJsonString["longitude"];
                    wgs84XYString = wgs84XYString + x84stirng + "," + y84string + ",";
                     }
                    wgs84XYString = wgs84XYString.Substring(0,wgs84XYString.Length - 1);
                    dic.Add("locationPoints",wgs84XYString);
                }
                
                     
                list.Add(dic);
                };
                
             return Json(list);
        }

        public IActionResult Get(string type){

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
        // public async Task<HttpResponse> Post(string row,string column,string scale)
        // {   
        //      byte[] result = getTile(int.Parse(scale),int.Parse(row),int.Parse(column),"satellite");
        //      Response.ContentType = "image/png";
        //      await Response.Body.WriteAsync(result, 0, result.Length);
        //      return Response;
            
        // }

          public async Task<IActionResult> Post()
        {   
             //身份标识
            string ID = Request.Form["身份标识"];

            //开工日期
            string startTime = Request.Form["开工日期"];

            //完工日期
            string endTime = Request.Form["完工日期"];

            //建设状态
            string status = Request.Form["建设状态"];

            //建设性质
            string type = Request.Form["建设性质"];

            //update
            //1先查询要修改的原数据
            JJProject customer =  _context.JJProjects.Find(ID);
            //2设置修改后的值
            customer.ID = ID;
            customer.JSZT = status;
            customer.JSXZ = type;
            customer.KGRQ = startTime;
            customer.WGRQ = endTime;

            //3更新到数据库
            _context.SaveChanges();

            return Json($"{ID}:更改成功");
            
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




    } 
}
