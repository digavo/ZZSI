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
        public int MaxHue = 300, MinHue = 160;
        public double MinVal = 20, MinSat = 10;

        public Bitmap KoncowyImg; // obraz wynikowy 
        private static readonly object Imglock = new object(); // mechanizm lock
        public int maxh;
        public int maxw;

        public void CzytajObraz(Stream str)
        {
            Img = new Bitmap(str);
        }
        private void RGBtoHSV(int r, int g, int b, out float Hue, out float Val, out float Sat)
        {
            float R = (float)r, G = (float)g, B = (float)b;
            float cmax = Math.Max(R, Math.Max(G, B));
            float cmin = Math.Min(R, Math.Min(G, B));
            float delta = cmax - cmin;
            if (cmin == cmax) Hue = 0;
            else 
            {
                if (cmax == R)
                    Hue = ((G - B) * 60) / delta;
                else if (cmax == G)
                    Hue = 120 + ((B - R) * 60) / delta;
                else Hue = 240 + ((R - G) * 60) / delta;
            }
            if (Hue < 0) Hue = Hue + 360;
            if (cmax == 0) Sat = 0;
            else Sat = (delta * 100) / cmax;
            Val = (100 * cmax) / 255;
            //System.Windows.Forms.MessageBox.Show("r:" + r + " g:" + g + " b:" + b + "\nR:" + R + " G:" + G + " B:" + B + "\nH:" + Hue + " V:" + Val + " Sa:" + Sat);
        }

        private void ZnajdzKolor (byte[] tab, int x1, int y1, int x2, int y2, int width, int depth) 
        {
            for (int i = x1; i < x2; i++)
            {
                for (int j = y1; j < y2; j++)
                {
                    int offset = ((j * width) + i) * depth;
                    float hue, sat, val;
                    RGBtoHSV(tab[offset + 2], tab[offset + 1], tab[offset + 0], out hue, out val, out sat);
                    int pixelR, pixelG, pixelB;
                    if (hue > MinHue && hue < MaxHue && val > MinVal && sat > MinSat)
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

        public Bitmap ZnajdzNiebieskiBez()
        {
            Bitmap newImg = new Bitmap(Img);
            Rectangle rect = new Rectangle(0, 0, newImg.Width, newImg.Height);
            BitmapData data = newImg.LockBits(rect, ImageLockMode.ReadOnly, newImg.PixelFormat); //lepsza wydajnośc w dużej skali od bitmap.GetPixel itp
            int depth = Bitmap.GetPixelFormatSize(data.PixelFormat) / 8; // 32 / 8 = 4
            byte[] buffer = new byte[data.Width * data.Height * depth];
            Marshal.Copy(data.Scan0, buffer, 0, buffer.Length); //kopiowanie bloku pamięci niezarządzanej 
            ZnajdzKolor(buffer, 0, 0, data.Width, data.Height, data.Width, depth);
            Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);
            newImg.UnlockBits(data);
            return newImg;
        }
        //raport tytuł, problem zarządzania opisać, i jak realizujemy, plik - opis, czasy bez i z wątkami , przyspieszenie
        //testy kilka razy
        public Bitmap ZnajdzKolorWatki() 
        {
            Bitmap newImg = new Bitmap(Img);
            Rectangle rect = new Rectangle(0, 0, newImg.Width, newImg.Height);
            BitmapData data = newImg.LockBits(rect, ImageLockMode.ReadOnly, newImg.PixelFormat); //lepsza wydajnośc w dużej skali od bitmap.GetPixel itp
            int depth = Bitmap.GetPixelFormatSize(data.PixelFormat) / 8; // 32 / 8 = 4
            byte[] buffer = new byte[data.Width * data.Height * depth];
            Marshal.Copy(data.Scan0, buffer, 0, buffer.Length); //kopiowanie bloku pamięci niezarządzanej 
            //asynchroniczne wywołanie wątków na wspólnych danych
            //Note that with Invoke, you simply express which actions you want to run concurrently, and the runtime handles all thread scheduling details, including scaling automatically to the number of cores on the host computer.
            Parallel.Invoke(
                () => { ZnajdzKolor(buffer, 0, 0, data.Width / 2, data.Height / 2, data.Width, depth); }, //top left
                () => { ZnajdzKolor(buffer, data.Width / 2, 0, data.Width, data.Height / 2, data.Width, depth); }, //top - right
                () => { ZnajdzKolor(buffer, 0, data.Height / 2, data.Width / 2, data.Height, data.Width, depth); }, //bottom - left
                () => { ZnajdzKolor(buffer, data.Width / 2, data.Height / 2, data.Width, data.Height, data.Width, depth); }  //bottom - right
            );
            Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);
            newImg.UnlockBits(data);
            return newImg;
        }

        public void Filtr(byte[] tab, int x1, int y1, int x2, int y2, int width, int depth)
        {
            for (int i = x1; i < x2; i++)
            {
                for (int j = y1; j < y2; j++)
                {
                    int offset = ((j * width) + i) * depth;
                    int pixelR = tab[offset + 2];
                    int pixelG = tab[offset + 1] - 127;
                    int pixelB = tab[offset + 0] - 255;
                    pixelG = Math.Max(pixelG, 0);
                    pixelB = Math.Max(pixelB, 0);
                    tab[offset + 2] = (byte)pixelR;
                    tab[offset + 1] = (byte)pixelG;
                    tab[offset + 0] = (byte)pixelB;
                }
            }
        }
        public Bitmap FiltrWatki()
        {
            Bitmap newImg = new Bitmap(Img);
            Rectangle rect = new Rectangle(0, 0, newImg.Width, newImg.Height);
            BitmapData data = newImg.LockBits(rect, ImageLockMode.ReadOnly, newImg.PixelFormat); //lepsza wydajnośc w dużej skali od bitmap.GetPixel itp
            int depth = Bitmap.GetPixelFormatSize(data.PixelFormat) / 8; // 32 / 8 = 4
            byte[] buffer = new byte[data.Width * data.Height * depth];
            Marshal.Copy(data.Scan0, buffer, 0, buffer.Length); //kopiowanie bloku pamięci niezarządzanej 
            //asynchroniczne wywołanie wątków na wspólnych danych
            //Note that with Invoke, you simply express which actions you want to run concurrently, and the runtime handles all thread scheduling details, including scaling automatically to the number of cores on the host computer.
            Parallel.Invoke(
                () => { Filtr(buffer, 0, 0, data.Width / 2, data.Height / 2, data.Width, depth); }, //top left
                () => { Filtr(buffer, data.Width / 2, 0, data.Width, data.Height / 2, data.Width, depth); }, //top - right
                () => { Filtr(buffer, 0, data.Height / 2, data.Width / 2, data.Height, data.Width, depth); }, //bottom - left
                () => { Filtr(buffer, data.Width / 2, data.Height / 2, data.Width, data.Height, data.Width, depth); }  //bottom - right
            );
            Marshal.Copy(buffer, 0, data.Scan0, buffer.Length);
            newImg.UnlockBits(data);
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
            float sat = c.GetBrightness();
            float bri = c.GetBrightness();
            if (hue > MinHue && hue < MaxHue && bri > MinVal && sat > MinSat)
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
            