using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImageResizer.Contracts.Services;
using ImageMagick;
using System.Windows.Shapes;
using UglyToad.PdfPig;
using Path = System.IO.Path;
using Rectangle = System.Drawing.Rectangle;
using UglyToad.PdfPig.Writer;
using Spire.Pdf;
using Spire.Pdf.Graphics;
using Spire.Pdf.Exporting.XPS.Schema;
using PdfSharp.Pdf.IO;
using PdfSharp.Drawing;
using System.Reflection.Metadata;
using PdfSharp.Pdf;
using System.Xml.Linq;

namespace ImageResizer.Services
{
    internal class ImageResizeServices : IImageResizeServices
    {
        public async void ResizeImages(string filePath, string destPath, double scale)
        {
            var format = Path.GetExtension(filePath);
            if (format.ToLower() == ".heic")
            {
                string imgName = Path.GetFileNameWithoutExtension(filePath);
                var destFile = Path.Combine(destPath, imgName + DateTime.Now.ToString("dd-MM-yyyy-H-mm-ss") + ".jpg");
                using MagickImage magickImage = new(filePath);

                magickImage.Resize(new Percentage(scale));
                await magickImage.WriteAsync(destFile);
            }
            else
            {
                Image imgPhoto = Image.FromFile(filePath);


                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;

                int destionatonWidth = (int)(sourceWidth * scale * 0.01);
                int destionatonHeight = (int)(sourceHeight * scale * 0.01);

                Bitmap processedImage = ProcessBitmap((Bitmap)imgPhoto,
                    sourceWidth, sourceHeight,
                    destionatonWidth, destionatonHeight);
                SaveImage(filePath, destPath, processedImage);
            }

        }

        public async void ResizeImages(string filePath, string destPath, int destionatonWidth, int destionatonHeight)
        {
            var format = Path.GetExtension(filePath);
            if (format.ToLower() == ".heic")
            {
                string imgName = Path.GetFileNameWithoutExtension(filePath);
                var destFile = Path.Combine(destPath, imgName + DateTime.Now.ToString("dd-MM-yyyy-H-mm-ss") + ".jpg");
                using MagickImage magickImage = new(filePath);
                magickImage.Resize(destionatonWidth, destionatonHeight);
                await magickImage.WriteAsync(destFile);
            }
            else
            {
                Image imgPhoto = Image.FromFile(filePath);


                int sourceWidth = imgPhoto.Width;
                int sourceHeight = imgPhoto.Height;

                Bitmap processedImage = ProcessBitmap((Bitmap)imgPhoto,
                    sourceWidth, sourceHeight,
                    destionatonWidth, destionatonHeight);

                SaveImage(filePath, destPath, processedImage);
            }
        }

        private Bitmap ProcessBitmap(Bitmap img, int srcWidth, int srcHeight, int newWidth, int newHeight)
        {
            Bitmap resizedbitmap = new Bitmap(newWidth, newHeight);
            Graphics g = Graphics.FromImage(resizedbitmap);
            g.InterpolationMode = InterpolationMode.High;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.Clear(Color.Transparent);
            g.DrawImage(img,
                new Rectangle(0, 0, newWidth, newHeight),
                new Rectangle(0, 0, srcWidth, srcHeight),
                GraphicsUnit.Pixel);

            return resizedbitmap;
        }
        public void CheckFolderForSave(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        public void SaveImage(string filePath, string destPath, Bitmap processedImage)
        {
            var format = Path.GetExtension(filePath);
            string imgName = Path.GetFileNameWithoutExtension(filePath);
            var destFile = Path.Combine(destPath, imgName + DateTime.Now.ToString("dd-MM-yyyy-H-mm-ss") + format);
            switch (format.ToLower())
            {
                case ".jpeg":
                    processedImage.Save(destFile, ImageFormat.Jpeg);
                    break;
                case ".jpg":
                    processedImage.Save(destFile, ImageFormat.Jpeg);
                    break;
                case ".png":
                    processedImage.Save(destFile, ImageFormat.Png);
                    break;
                case ".bmp":
                    processedImage.Save(destFile, ImageFormat.Bmp);
                    break;
                case ".heic":
                    processedImage.Save(Path.Combine(destPath, imgName + DateTime.Now.ToString("dd-MM-yyyy-H-mm-ss") + ".jpeg"), ImageFormat.Jpeg);
                    break;
            }
        }

        public async void ConvertImage(string filePath, string destPath, ImageFormat imageFormat)
        {
            var format = System.IO.Path.GetExtension(filePath);
            string imgName = Path.GetFileNameWithoutExtension(filePath);
            var destFile = Path.Combine(destPath, imgName + DateTime.Now.ToString("dd-MM-yyyy-H-mm-ss") + "." + imageFormat.ToString().ToLower());
            if (format.ToLower() == ".heic")
            {
                using MagickImage img = new MagickImage(filePath);
                await img.WriteAsync(destFile);
            }
            else
            {
                Image image = Image.FromFile(filePath);
                image.Save(destFile, imageFormat);
            }
        }

        public async Task PdfToJpeg(string filePath, string destPath, ImageFormat imageFormat)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            var dd = System.IO.File.ReadAllBytes(filePath);

            using UglyToad.PdfPig.PdfDocument document = UglyToad.PdfPig.PdfDocument.Open(filePath);
            int countPage = document.GetPages().Count();
            for (int i = 1; i <= countPage; i++)
            {

                byte[] pngByte = Freeware.Pdf2Png.Convert(dd, i);
                using MemoryStream memory = new MemoryStream();
                await memory.WriteAsync(pngByte);
                Image image = Image.FromStream(memory);
                image.Save(Path.Combine(destPath, fileName + DateTime.Now.ToString("dd-MM-yyyy-H-mm-ss") + "." + imageFormat.ToString().ToLower()), ImageFormat.Jpeg);

            }
        }
        public void ImageToPdf(string[] filePath, string destPath)
        {
            PdfSharp.Pdf.PdfDocument pdf = new();
            foreach (string file in filePath)
            {
                var format = Path.GetExtension(file);

                Image image;
                if (format.ToLower() != ".pdf")
                {
                    if (format.ToLower() == ".heic")
                    {

                        image = Image.FromStream(GetHeicStream(file));
                    }
                    else
                    {
                        image = Image.FromFile(file);
                    }
                    float width = image.PhysicalDimension.Width;
                    float height = image.PhysicalDimension.Height;
                    PdfPage page = pdf.AddPage(new PdfPage() { Height = height, Width = width });
                    XGraphics gfx = XGraphics.FromPdfPage(page);
                    DrawImage(gfx, file, 0, 0, (int)page.Width, (int)page.Height);
                }
                else
                {
                    PdfSharp.Pdf.PdfDocument inputDocument = PdfReader.Open(file, PdfDocumentOpenMode.Import);
                    int count = inputDocument.PageCount;
                    for (int idx = 0; idx < count; idx++)
                    {
                        PdfPage page = inputDocument.Pages[idx];
                        pdf.AddPage(page);
                    }
                }
            }
            pdf.Save(Path.Combine(destPath, DateTime.Now.ToString("dd-MM-yyyy-H-mm-ss") + ".pdf"));
            pdf.Dispose();
        }

        private Stream GetHeicStream(string file)
        {
            MagickImage magickImage = new MagickImage(file);

            var memStream = new MemoryStream();

            magickImage.Write(memStream, MagickFormat.Jpeg);
            return memStream;
        }

        private void DrawImage(XGraphics gfx, string path, int x, int y, int width, int height)
        {
            XImage image;
            if (Path.GetExtension(path).ToLower() == ".heic")          
               image = XImage.FromStream(GetHeicStream(path));           
            else                           
               image = XImage.FromFile(path);                          
            gfx.DrawImage(image, x, y, width, height);
        }
    }
}
