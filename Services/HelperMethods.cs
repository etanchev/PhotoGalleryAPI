using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoGalleryAPI.Services
{
    public class HelperMethods
    {
        private readonly string _webHost;
        public HelperMethods(string webHost)
        {
            _webHost = webHost;
        }
        public async Task<Bitmap> WaterMarkImages(Stream filestream)
        {
            try
            {
                Image targetImage = Image.FromStream(filestream);
                Bitmap waterMarkLogo = (Bitmap)Image.FromFile(Path.Combine(_webHost, "Logo.png"));

                Bitmap bitmap = new Bitmap(targetImage);

                Bitmap resizedLogo = new Bitmap(waterMarkLogo, new Size(bitmap.Width, (int)(bitmap.Width / 3.25f)));

                Graphics imageGraphics = Graphics.FromImage(bitmap);

                ColorMatrix colorMatrix = new ColorMatrix
                {
                    //matrix 33 for alfa channel
                    Matrix33 = 0.2f
                };

                ImageAttributes imageAttributes = new ImageAttributes();

                imageAttributes.SetColorMatrix(colorMatrix);


                //adjust em font dimentions to pixels 12 is picked by the font type normaly 16 px = 1em
                // float fontEM = bitmap.Width / 12f;

                imageGraphics.SmoothingMode = SmoothingMode.HighQuality;

                //draw image
                int x = (bitmap.Width / 2 - resizedLogo.Width / 2);
                int y = (bitmap.Height / 2 - resizedLogo.Height / 2);
                imageGraphics.DrawImage(
                    resizedLogo,
                    new Rectangle(x, y, resizedLogo.Width, resizedLogo.Height),
                    0,
                    0,
                    resizedLogo.Width,
                    resizedLogo.Height,
                    GraphicsUnit.Pixel,
                    imageAttributes
                    );

                bitmap.SetResolution(72f, 72f);

                return bitmap;
            }
            catch
            {
                return null;
            }



        }
    }
}
