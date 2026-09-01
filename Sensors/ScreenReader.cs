using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Eliza.Sensors
{
    public class ScreenReader
    {
        // Захватывает полное 2K разрешение без сжатия
        public Bitmap CaptureScreen()
        {
            int width = (int)SystemParameters.PrimaryScreenWidth;
            int height = (int)SystemParameters.PrimaryScreenHeight;
            
            var bmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            using (var g = Graphics.FromImage(bmp))
            {
                g.CopyFromScreen(0, 0, 0, 0, bmp.Size);
            }
            return bmp;
        }

        public async Task<string> ExtractTextAsync(Bitmap bitmap)
        {
            if (bitmap == null) return string.Empty;
            
            try
            {
                using var stream = new MemoryStream();
                bitmap.Save(stream, ImageFormat.Bmp);
                stream.Position = 0;

                // Динамический вызов WinRT API для Windows 10/11 без привязки жестких DLL
                Type decoderType = Type.GetType("Windows.Graphics.Imaging.BitmapDecoder, Windows.Foundation.UniversalApiContract");
                if (decoderType == null) return "[OCR недоступен]";

                var method = decoderType.GetMethod("CreateAsync", new[] { Type.GetType("Windows.Storage.Streams.IRandomAccessStream, Windows.Foundation.UniversalApiContract") });
                dynamic decoderOp = method.Invoke(null, new object[] { stream.AsRandomAccessStream() });
                var decoder = await decoderOp;

                using var softwareBitmap = await decoder.GetSoftwareBitmapAsync();
                
                Type ocrEngineType = Type.GetType("Windows.Media.Ocr.OcrEngine, Windows.Foundation.UniversalApiContract");
                dynamic ocrEngine = ocrEngineType?.GetMethod("TryCreateFromUserProfileLanguages")?.Invoke(null, null);
                
                if (ocrEngine != null)
                {
                    dynamic ocrResult = await ocrEngine.RecognizeAsync(softwareBitmap);
                    return ocrResult.Text;
                }
            }
            catch 
            {
                // Подавляем сбои парсинга (например, на пустом черном экране)
            }
            
            return string.Empty;
        }
    }
}