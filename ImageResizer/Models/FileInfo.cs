using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer.Models
{
    public class FileInfo
    {
        public string FullName { get; set; }
        public string ShortName { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string? Size { get; set; }
        public int? CountPage { get; set; }


    }
}
