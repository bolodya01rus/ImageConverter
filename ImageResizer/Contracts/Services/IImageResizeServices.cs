using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageResizer.Contracts.Services
{
    public interface IImageResizeServices
    {
        public void ResizeImages(string filePath, string destPath, double scale);
        public void ResizeImages(string filePath, string destPath, int destionatonWidth, int destionatonHeight);
        public void SaveImage(string filePath, string destPath, Bitmap processedImage);
        public void CheckFolderForSave(string path);
        public void ConvertImage(string filePath, string destPath, ImageFormat imageFormat);
        public Task PdfToJpeg(string filePath, string destPath, ImageFormat imageFormat);
        public void ImageToPdf(string[] filePath, string destPath);
       
    }
}
