using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Drawing;

namespace ZZ
{
    class IMGFormat
    {
        public Bitmap Img;
        // nagłówek pliku
        /*char[] ID = new char[2];
        UInt32 FileSize;
        UInt16 Reserved1;
        UInt16 Reserved2;
        UInt32 Offset;
        // informacje bitmapy
        UInt32 BMPInfoSize;
        Int32 Width;
        Int32 Height;
        UInt16 Planes;
        UInt16 BitCount;
        UInt32 Compression;
        UInt32 SizeImage;
        Int32 XPelsPerMeter;
        Int32 YPelsPerMeter;
        UInt32 ColorsUsed;
        UInt32 ColorsImportant;
        
        public void CzytajNaglowek(BinaryReader plik)
        {
            ID = plik.ReadChars(2);
            FileSize = plik.ReadUInt32();
            Reserved1 = plik.ReadUInt16();
            Reserved2 = plik.ReadUInt16();
            Offset = plik.ReadUInt32();
            BMPInfoSize = plik.ReadUInt32();
            Width = plik.ReadInt32();
            Height = plik.ReadInt32();
            Planes = plik.ReadUInt16();
            BitCount = plik.ReadUInt16();
            Compression = plik.ReadUInt32();
            SizeImage = plik.ReadUInt32();
            XPelsPerMeter = plik.ReadInt32();
            YPelsPerMeter = plik.ReadInt32();
            ColorsUsed = plik.ReadUInt32();
            ColorsImportant = plik.ReadUInt32();
        }
        public String[] WypiszNaglowek()
        {
            String[] tekst = new String[2];
            tekst[0] = String.Format("Nagłówek pliku BMP:\n Typ:\n Rozmiar danych:\n Zarezerwowane:\n Zarezerwowane:\n Przesuniecie danych:\n Opis: \n" +
                "      Rozmiar:\n      Szerokość [px]:\n      Wysokość [px]:\n      Płaszczyzna:\n      Liczba bitów na piksel:\n      Typ kompresji:\n" +
                "      Rozmiar mapy:\n      Rozdzielczość X:\n      Rozdzielczość Y:\n      Liczba użytych kolorów:\n      Liczba kolorów wymaganych:\n");
            tekst[1] = String.Format("\n{0}\n{1}\n{2}\n{3}\n{4}\n\n{5}\n{6}\n{7}\n{8}\n{9}\n{10}\n{11}\n{12}\n{13}\n{14}\n{15}\n",
                new string(ID), FileSize, Reserved1, Reserved2, Offset, BMPInfoSize, Width, Height, Planes, BitCount, Compression, SizeImage,
                XPelsPerMeter, YPelsPerMeter, ColorsUsed, ColorsImportant);
            return tekst;
        }*/
        public void CzytajObraz(Stream str)
        {
            Img = new Bitmap(str);
        }
        public Bitmap Filtr ()
        {
            Bitmap newImg = new Bitmap(Img);
            Color c;
            for (int i = 0; i < newImg.Width; i++)
            {
                for (int j = 0; j < newImg.Height; j++)
                {
                    c = newImg.GetPixel(i, j);
                    float hue = c.GetHue();
                    if (hue<290 && hue>170) //niebieski 270-170
                    {
                        int pixelR = 255;
                        int pixelG = 255;
                        int pixelB = 255;
                        newImg.SetPixel(i, j, Color.FromArgb((byte)pixelR,(byte)pixelG, (byte)pixelB));
                    }
                    /*int pixelR = c.R;
                    int pixelG = c.G - 255;
                    int pixelB = c.B - 255; 
                    pixelR = Math.Max(pixelR, 0);
                    pixelR = Math.Min(255, pixelR);
                    pixelG = Math.Max(pixelG, 0);
                    pixelG = Math.Min(255, pixelG);
                    pixelB = Math.Max(pixelB, 0);
                    pixelB = Math.Min(255, pixelB);
                    */
                }
            }
            return newImg;
        }

        public Bitmap Contrast () 
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
                    double pR = c.R / 255.0; ;
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
    }
}
            