using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;
using System.Threading;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace ZZ
{
    class IMGFormat
    {
        public Bitmap Img;

        public Bitmap KoncowyImg; // obraz wynikowy 
        private static readonly object Imglock = new object(); // mechanizm lock
        public int maxh;
        public int maxw;

        public void CzytajObraz(Stream str)
        {
            Img = new Bitmap(str);
        }

        private void ZnajdzKolor (byte[] tab, int x1, int y1, int x2, int y2, int width, int depth) //Daga
        {
            for (int i = x1; i < x2; i++)
            {
                for (int j = y1; j < y2; j++)
                {
                    int offset = ((j * width) + i) * depth;
                    float hue, sat, bri;
                    RGBtoHSL(tab[offset + 2], tab[offset + 1], tab[offset + 0], out hue, out bri, out sat);
                    int pixelR, pixelG, pixelB;
                    if (160 < hue && hue < 300 && bri > 0.20 && sat > 0.10 && sat>(-bri+0.7))
                    {
                        pixelR = 0;
                        pixelG = 0;
                        pixelB = 255;
                    }
                    else
                    {
                        pixelR = tab[offset + 2];
                        pixelG = tab[offset + 1] - 127;
                        pixelB = tab[offset + 0] - 255;
                        pixelG = Math.Max(pixelG, 0);
                        pixelB = Math.Max(pixelB, 0);
                    }
                    tab[offset + 2] = (byte)pixelR;
                    tab[offset + 1] = (byte)pixelG;
                    tab[offset + 0] = (byte)pixelB;
                }
            }
        }
        private void RGBtoHSL(int r, int g, int b, out float Hue, out float Bri, out float Sat)
        {
            float R = (float)r / 255, G = (float)g / 255, B = (float)b / 255;
            float cmax = Math.Max(R, Math.Max(G, B));
            float cmin = Math.Min(R, Math.Min(G, B));
            float delta = cmax - cmin;
            Bri = (cmax + cmin) / 2;
            if (delta == 0) { Hue = 0; Sat = 0; }
            else
            {
                Sat = delta / (1 - Math.Abs(2 * Bri - 1));
                if (cmax == R)
                    Hue = 60 * ((G - B) / delta) % 6;
                else if (cmax == G)
                    Hue = 60 * ((B - R) / delta + 2);
                else Hue = 60 * ((R - G) / delta + 4);
            }
            //System.Windows.Forms.MessageBox.Show("r:" + r + " g:" + g + " b:" + b + "\nR:" + R + " G:" + G + " B:" + B + "\nH:" + Hue + " Br:" + Bri + " Sa:" + Sat);
        } //Daga
        public Bitmap ZnajdzKolorSzybko() //Daga
        {
            Bitmap newImg = new Bitmap(Img);
            Rectangle rect = new Rectangle(0, 0, newImg.Width, newImg.Height);
            BitmapData data = newImg.LockBits(rect, ImageLockMode.ReadOnly, newImg.PixelFormat);
            int depth = Bitmap.GetPixelFormatSize(data.PixelFormat) / 8;
            byte[] buffer = new byte[data.Width * data.Height * depth];
            Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);
            //asynchroniczne wywołanie wątków
            Parallel.Invoke(
                () => { ZnajdzKolor(buffer, 0, 0, data.Width / 2, data.Height / 2, data.Width, depth); }, //top left
                () => { ZnajdzKolor(buffer, data.Width / 2, 0, data.Width, data.Height, data.Width, depth); }, //top - right
                () => { ZnajdzKolor(buffer, 0, data.Height / 2, data.Width, data.Height, data.Width, depth); }, //bottom - left
                () => { ZnajdzKolor(buffer, data.Width / 2, data.Height / 2, data.Width, data.Height, data.Width, depth); }  //bottom - right
            );
            Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);
            newImg.UnlockBits(data);
            return newImg;
        }

        public Bitmap Filtr()
        {
            Bitmap newImg = new Bitmap(Img);
            Color c;
            for (int i = 0; i < newImg.Width; i++)
            {
                for (int j = 0; j < newImg.Height; j++)
                {
                    c = newImg.GetPixel(i, j);
                    int pixelR = c.R;
                    int pixelG = c.G - 127;
                    int pixelB = c.B - 255;
                    pixelG = Math.Max(pixelG, 0);
                    pixelB = Math.Max(pixelB, 0);

                    newImg.SetPixel(i, j, Color.FromArgb((byte)pixelR, (byte)pixelG, (byte)pixelB));
                }
            }
            return newImg;
        }
        public Bitmap Kontrast () 
        {
            double contrast = 50; //-100 - 100
            Bitmap newImg = new Bitmap(Img);
            if (contrast < -100) contrast = -100;
            if (contrast > 100) contrast = 100;
            contrast = (100.0 + contrast) / 100.0;
            contrast *= contrast;
            Color c;
            for (int i = 0; i < newImg.Width; i++)
            {
                for (int j = 0; j < newImg.Height; j++)
                {
                    c = newImg.GetPixel(i, j);
                    double pR = c.R / 255.0;
                    pR -= 0.5;
                    pR *= contrast;
                    pR += 0.5;
                    pR *= 255;
                    if (pR < 0) pR = 0;
                    if (pR > 255) pR = 255;

                    double pG = c.G / 255.0;
                    pG -= 0.5;
                    pG *= contrast;
                    pG += 0.5;
                    pG *= 255;
                    if (pG < 0) pG = 0;
                    if (pG > 255) pG = 255;

                    double pB = c.B / 255.0;
                    pB -= 0.5;
                    pB *= contrast;
                    pB += 0.5;
                    pB *= 255;
                    if (pB < 0) pB = 0;
                    if (pB > 255) pB = 255;

                    newImg.SetPixel(i, j, Color.FromArgb((byte)pR, (byte)pG, (byte)pB));
                }
            }
            return newImg;
        }

        public Bitmap ZnajdzNiebieski() 
        {
            Bitmap orginal = new Bitmap(Img);
            //zmieniamy kopie na pomaranczowy
            for (int i = 0; i < orginal.Width; i++)
            {
                for (int j = 0; j < orginal.Height; j++)
                {
                    orginal.SetPixel(i, j, zmienpikselek(orginal.GetPixel(i, j)));
                }
            }
            return orginal;// i zwracamy pomaranczowy obraz z niebieskim wyostrzonym
        }

        public Color zmienpikselek(Color c) //  sprawdzenie piksela i zmiana jego koloru na niebieski lub odcien pomaranczowego 
        {
            int pixelR, pixelG, pixelB;
            float hue = c.GetHue();
            float sat = c.GetSaturation();
            float bri = c.GetBrightness();
            if ((160 < hue && hue < 330 && bri > 0.30 && bri < 0.75 && sat > 0.35))
            // 160-300 jasny niebieski,niebieski, niebiesko-fioletowy
            {
                pixelR = 0;
                pixelG = 0;
                pixelB = 255;// wyostrzamy niebieski
            }
            else
            {
                pixelR = c.R;
                pixelG = c.G - 127;
                pixelB = c.B - 255;
                pixelR = Math.Max(pixelR, 0);
                pixelR = Math.Min(255, pixelR);
                pixelG = Math.Max(pixelG, 0);
                pixelG = Math.Min(255, pixelG);
                pixelB = Math.Max(pixelB, 0);
                pixelB = Math.Min(255, pixelB);
            }
            return  Color.FromArgb((byte)pixelR, (byte)pixelG, (byte)pixelB);
        }
 
        public void watek1()
        { 
                for (int i = 0; i < maxw ; i++)
                {
                    for (int j = 0; j < maxh / 2; j++)
                    {
                        lock (Imglock)
                        {
                            KoncowyImg.SetPixel(i, j, zmienpikselek(KoncowyImg.GetPixel(i, j)));
                        }
                    }
                }
        }
        public void watek3()
        {
                for (int i = 0; i < maxw ; i++)
                {
                    for (int j = maxh / 2; j < maxh; j++)
                    {
                        lock (Imglock)
                        {
                            KoncowyImg.SetPixel(i, j, zmienpikselek(KoncowyImg.GetPixel(i, j)));
                        }
                    }
                }
        }


        /*
             public void watek1()
        { 
                for (int i = 0; i < maxw / 2; i++)
                {
                    for (int j = 0; j < maxh / 2; j++)
                    {
                        lock (Imglock)
                        {
                            KoncowyImg.SetPixel(i, j, zmienpikselek(KoncowyImg.GetPixel(i, j)));
                        }
                    }
                }
        }
        public void watek2()
        {
                for (int i = maxw / 2; i < maxw; i++)
                {
                    for (int j = 0; j < maxh / 2; j++)
                    {
                        lock (Imglock)
                        {
                            KoncowyImg.SetPixel(i, j, zmienpikselek(KoncowyImg.GetPixel(i, j)));
                        }
                    }
                }
        }
        public void watek3()
        {
                for (int i = 0; i < maxw ; i++)// /2
                {
                    for (int j = maxh / 2; j < maxh; j++)
                    {
                        lock (Imglock)
                        {
                            KoncowyImg.SetPixel(i, j, zmienpikselek(KoncowyImg.GetPixel(i, j)));
                        }
                    }
                }
        }
        public void watek4()
        {
                for (int i = maxw / 2; i < maxw; i++)
                {
                    for (int j = maxh / 2; j < maxh; j++)
                    {
                        lock (Imglock)
                        {
                            KoncowyImg.SetPixel(i, j, zmienpikselek(KoncowyImg.GetPixel(i, j)));
                        }
                    }
                }
        }
         * */

        public Bitmap ZnajdzNiebieskiWielowatkowo()
        {
            KoncowyImg = new Bitmap(Img);
            maxw = KoncowyImg.Width;
            maxh = KoncowyImg.Height;
            Thread thread1 = new Thread(watek1);
            Thread thread3 = new Thread(watek3);
            thread1.Start();
            thread3.Start();
            thread1.Join();  //czekanie na zakonczenie watku 1
            thread3.Join();  //czekanie na zakonczenie watku 3
            return KoncowyImg;// zwracamy pomaranczowy obraz z niebieskim wyostrzonym
        }
    }
}
            