using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace CPSSnew.Models.MapViewModels
{
    public class Building
    {
        [Key]
        public string BSM { get; set; }

        public string LB { get; set; }
        public string XLB { get; set; }
        public string MC { get; set; }
        public string WZMS { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double X86 { get; set; }
        public double Y86 { get; set; }
        public string BZ { get; set; }
    }
}
