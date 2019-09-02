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

namespace APIURLManager
{
    public class APIURLManager
    {
       static string localip = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces()
  .Select(p => p.GetIPProperties())
  .SelectMany(p => p.UnicastAddresses)
  .Where(p => p.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !System.Net.IPAddress.IsLoopback(p.Address))
  .FirstOrDefault()?.Address.ToString();
        public static string transformURL = $"http://{localip}:8091";
        public static string videoURL = $"http://{localip}:44366";
        public static string mapURL= $"http://{localip}:8080/geoserver/pipeline/wms";
        public static string roadURL = $"http://{localip}:8080/geoserver/CPSS/wms";
    }
}