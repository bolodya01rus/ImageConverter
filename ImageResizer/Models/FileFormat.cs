using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer.Models
{
    public class FileFormat
    {
        public string Name { get; set; }
        public ImageFormat Format { get; set; }
    }
}
