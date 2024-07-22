using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace localgen.Helpers
{
    public class ImageHelper
    {
        public static byte[] ConvertToPng(byte[] ImageData)
        {
            try
            {
                var imageStream = new MemoryStream();
                SKBitmap MainImage = SKBitmap.Decode(ImageData);
                SKImageInfo info = new SKImageInfo(MainImage.Width, MainImage.Height);

                using (var surface = SKSurface.Create(info))
                using (var canvas = surface.Canvas)
                {
                    // draw the svg
                    canvas.Clear(SKColors.Transparent);
                    canvas.DrawBitmap(MainImage, new SKPoint(0, 0));
                    canvas.Flush();

                    using (var data = surface.Snapshot())
                    using (var pngImage = data.Encode(SKEncodedImageFormat.Png, 100))
                    {
                        pngImage.SaveTo(imageStream);
                    }
                    return imageStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return default;
            }
            
        }
        public static byte[] DrawPolygon(IList<System.Drawing.PointF[]> polygons, IList<string> Desc, byte[] ImageData, string Remark)
        {
            try
            {
                var bmp = SKBitmap.Decode(ImageData);
                var g = new SKCanvas(bmp);

                var paint = new SKPaint();
                paint.Color = SKColors.DarkRed;
                paint.StrokeWidth = 2;
                paint.Style = SKPaintStyle.Stroke;
                var paintText = new SKPaint();
                paintText.TextSize = 20;
                paintText.Color = SKColors.Black;
                var counter = 0;


                foreach (var poly in polygons)
                {
                    if (poly.Length > 0)
                    {
                        var points = poly.Select(x => new SKPoint(x.X, x.Y)).ToArray();
                        g.DrawPoints(SKPointMode.Polygon, points, paint);
                        counter++;
                        if (counter < Desc.Count)
                        {
                            g.DrawText(Desc[counter], poly[0].X, poly[0].Y, paintText);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(Remark))
                {
                    g.DrawText(Remark, 5, 5, paintText);
                }

                g.Dispose();
                if (bmp != null)
                    return bmp.Encode(SKEncodedImageFormat.Jpeg, 100).ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }
            return default;
        }



        public static string ConvertToBase64Image(byte[] imgData)
        {
            var base64String = Convert.ToBase64String(imgData);

            return "data:image/png;base64," + base64String;
        }
        public static async Task<byte[]> MakeTransparent(byte[] ImageData, byte[] Selection)
        {
            try
            {
                SKBitmap MaskImage = SKBitmap.Decode(Selection);
                SKBitmap MainImage = SKBitmap.Decode(ImageData);
                if (MaskImage.Width != MainImage.Width || MaskImage.Height != MainImage.Height) throw new Exception("image size is not same");
                SKImageInfo info = new SKImageInfo(MainImage.Width, MainImage.Height);
                SKSurface surface = SKSurface.Create(info);
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.Empty);
               
                var col = SKColors.Empty;
                for (var x = 0; x < MaskImage.Width; x++)
                    for (var y = 0; y < MaskImage.Height; y++)
                    {
                        if (MaskImage.GetPixel(x, y) != SKColor.Empty)
                        {
                            MainImage.SetPixel(x, y, col);
                        }
                    }
                canvas.DrawBitmap(MainImage, 0, 0, null);
                surface.Draw(canvas, 0, 0, null);

                var encoded = surface.Snapshot().Encode();
                byte[] surfaceData = encoded.ToArray();
                return surfaceData;
            }
            catch (Exception ex)
            {
                return default;
            }

        }
        public static async Task<byte[]> GenerateWholeMaskImage(byte[] ImageData)
        {
            try
            {
              
                SKBitmap MainImage = SKBitmap.Decode(ImageData);
                SKImageInfo info = new SKImageInfo(MainImage.Width, MainImage.Height);
                SKSurface surface = SKSurface.Create(info);
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.Empty);

                var col = SKColors.Empty;
                for (var x = 10; x < MainImage.Width - 10; x++)
                    for (var y = 10; y < MainImage.Height - 10; y++)
                    {
                        MainImage.SetPixel(x, y, col);
                    }
                canvas.DrawBitmap(MainImage, 0, 0, null);
                surface.Draw(canvas, 0, 0, null);

                var encoded = surface.Snapshot().Encode();
                byte[] surfaceData = encoded.ToArray();
                return surfaceData;
            }
            catch (Exception ex)
            {
                return default;
            }

        }

        public static async Task<byte[]> CreateEmptyPic(int W=800,int H=600)
        {
            try
            {
                SKBitmap MainImage = new SKBitmap(W, H);
                SKImageInfo info = new SKImageInfo(MainImage.Width, MainImage.Height);
                SKSurface surface = SKSurface.Create(info);
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.Transparent);
                canvas.DrawBitmap(MainImage, 0, 0, null);
                surface.Draw(canvas, 0, 0, null);

                var encoded = surface.Snapshot().Encode();
                byte[] surfaceData = encoded.ToArray();
                return surfaceData;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return default;

        }

        public static async Task<bool> SaveToDisk(byte[] ImageData, string FilePath)
        {
            try
            {
                SKBitmap MainImage = SKBitmap.Decode(ImageData);
                SKImageInfo info = new SKImageInfo(MainImage.Width, MainImage.Height);
                SKSurface surface = SKSurface.Create(info);
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.Empty);
                canvas.DrawBitmap(MainImage, 0, 0, null);
                surface.Draw(canvas, 0, 0, null);

                var encoded = surface.Snapshot().Encode();
                byte[] surfaceData = encoded.ToArray();
                File.WriteAllBytes(FilePath, surfaceData);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return false;

        }

        public static async Task<byte[]> ChangeColor(byte[] ImageData, byte[] Selection, SKColor col)
        {
            try
            {
                SKBitmap MaskImage = SKBitmap.Decode(Selection);
                SKBitmap MainImage = SKBitmap.Decode(ImageData);
                if (MaskImage.Width != MainImage.Width || MaskImage.Height != MainImage.Height) throw new Exception("image size is not same");
                SKImageInfo info = new SKImageInfo(MainImage.Width, MainImage.Height);
                SKSurface surface = SKSurface.Create(info);
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.Empty);

                //var col = SKColors.Empty;
                for (var x = 0; x < MaskImage.Width; x++)
                    for (var y = 0; y < MaskImage.Height; y++)
                    {
                        if (MaskImage.GetPixel(x, y) != SKColor.Empty)
                        {
                            MainImage.SetPixel(x, y, col);
                        }
                    }
                canvas.DrawBitmap(MainImage, 0, 0, null);
                surface.Draw(canvas, 0, 0, null);

                var encoded = surface.Snapshot().Encode();
                byte[] surfaceData = encoded.ToArray();
                return surfaceData;
            }
            catch (Exception ex)
            {
                return default;
            }

        }
        public static async Task<byte[]> ChangeToFuschia(byte[] ImageData)
        {
            try
            {
                SKBitmap webBitmap = SKBitmap.Decode(ImageData);
                SKImageInfo info = new SKImageInfo(webBitmap.Width, webBitmap.Height);
                SKSurface surface = SKSurface.Create(info);
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.White);
                var col = SKColors.Fuchsia;
                for (var x = 0; x < webBitmap.Width; x++)
                    for (var y = 0; y < webBitmap.Height; y++)
                    {
                        if (webBitmap.GetPixel(x, y) != SKColor.Empty)
                        {
                            webBitmap.SetPixel(x, y, col);
                        } 
                    }
                canvas.DrawBitmap(webBitmap, 0, 0, null);
                surface.Draw(canvas, 0, 0, null);
                
                var encoded = surface.Snapshot().Encode();
                byte[] surfaceData = encoded.ToArray();
                return surfaceData;
            }
            catch (Exception ex)
            {
                return default;
            }

        }
        public static async Task<byte[]> GetImageAsBytes(string url, int width, int height)
        {
            try
            {
                SKImageInfo info = new SKImageInfo(width, height);
                SKSurface surface = SKSurface.Create(info);
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.White);
                var httpClient = new HttpClient();
                using (Stream stream = await httpClient.GetStreamAsync(url))
                using (MemoryStream memStream = new MemoryStream())
                {
                    await stream.CopyToAsync(memStream);
                    memStream.Seek(0, SeekOrigin.Begin);
                    SKBitmap webBitmap = SKBitmap.Decode(memStream);
                    
                    canvas.DrawBitmap(webBitmap, 0, 0, null);
                    surface.Draw(canvas, 0, 0, null);
                };
                var encoded = surface.Snapshot().Encode();
                byte[] surfaceData = encoded.ToArray();
                return surfaceData;
            }
            catch (Exception ex)
            {
                return default;
            }
            
        }

        public static async Task<byte[]> ChangeBackground(byte[] ImageData, byte[] Foreground, SKColor backgroundColor)
        {
            try
            {
                SKBitmap MaskImage = SKBitmap.Decode(Foreground);
                SKBitmap MainImage = SKBitmap.Decode(ImageData);
                if (MaskImage.Width != MainImage.Width || MaskImage.Height != MainImage.Height) throw new Exception("image size is not same");
                SKImageInfo info = new SKImageInfo(MainImage.Width, MainImage.Height);
                SKSurface surface = SKSurface.Create(info);
                SKCanvas canvas = surface.Canvas;
                canvas.Clear(SKColors.Empty);

                //var col = SKColors.Empty;
                for (var x = 0; x < MaskImage.Width; x++)
                    for (var y = 0; y < MaskImage.Height; y++)
                    {
                        if (MaskImage.GetPixel(x, y) == SKColors.Black)
                        {
                            MainImage.SetPixel(x, y, backgroundColor);
                        }
                    }
                canvas.DrawBitmap(MainImage, 0, 0, null);
                surface.Draw(canvas, 0, 0, null);

                var encoded = surface.Snapshot().Encode();
                byte[] surfaceData = encoded.ToArray();
                return surfaceData;
            }
            catch (Exception ex)
            {
                return default;
            }

        }
    }
}
