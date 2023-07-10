using System;
using System.IO;
using Unity.Collections;

class BitmapEncoder
{
    /// <summary>
    /// Writes raw bytes (i.e Texture2D.GetRawTextureData) as a bmp file
    /// </summary>
    /// <param name="path"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="imageData"></param>
    public static void WriteBitmapFile(string path, int width, int height, byte[] imageData)
    {
        using (FileStream fileStream = new FileStream(path, FileMode.Create))
        {
            using (BinaryWriter bw = new BinaryWriter(fileStream))
            {
                // define the bitmap file header
                bw.Write((UInt16)0x4D42);                               // bfType;
                bw.Write((UInt32)(14 + 40 + (width * height * 4)));     // bfSize;
                bw.Write((UInt16)0);                                    // bfReserved1;
                bw.Write((UInt16)0);                                    // bfReserved2;
                bw.Write((UInt32)14 + 40);                              // bfOffBits;

                // define the bitmap information header
                bw.Write((UInt32)40);                                      // biSize;
                bw.Write((Int32)width);                                 // biWidth;
                bw.Write((Int32)height);                                // biHeight;
                bw.Write((UInt16)1);                                    // biPlanes;
                bw.Write((UInt16)32);                                   // biBitCount;
                bw.Write((UInt32)0);                                    // biCompression;
                bw.Write((UInt32)(width * height * 4));                 // biSizeImage;
                bw.Write((Int32)0);                                     // biXPelsPerMeter;
                bw.Write((Int32)0);                                     // biYPelsPerMeter;
                bw.Write((UInt32)0);                                    // biClrUsed;
                bw.Write((UInt32)0);                                    // biClrImportant;

                // switch the image data from RGB to BGR
                for (int imageIdx = 0; imageIdx < imageData.Length; imageIdx += 3)
                {
                    bw.Write(imageData[imageIdx + 2]);
                    bw.Write(imageData[imageIdx + 1]);
                    bw.Write(imageData[imageIdx + 0]);
                    bw.Write((byte)255);
                }
            }
            fileStream.Close();
        }
    }

    /// <summary>
    /// Save as WriteBitmapFile but for NativeArray<byte>
    /// </summary>
    /// <param name="path"></param>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="imageData"></param>
    public static void WriteBitmapFile(string path, int width, int height, NativeArray<byte> imageData)
    {
        using (FileStream fileStream = new FileStream(path, FileMode.Create))
        {
            using (BinaryWriter bw = new BinaryWriter(fileStream))
            {
                // define the bitmap file header
                bw.Write((UInt16)0x4D42);                               // bfType;
                bw.Write((UInt32)(14 + 40 + (width * height * 4)));     // bfSize;
                bw.Write((UInt16)0);                                    // bfReserved1;
                bw.Write((UInt16)0);                                    // bfReserved2;
                bw.Write((UInt32)14 + 40);                              // bfOffBits;

                // define the bitmap information header
                bw.Write((UInt32)40);                                      // biSize;
                bw.Write((Int32)width);                                 // biWidth;
                bw.Write((Int32)height);                                // biHeight;
                bw.Write((UInt16)1);                                    // biPlanes;
                bw.Write((UInt16)32);                                   // biBitCount;
                bw.Write((UInt32)0);                                    // biCompression;
                bw.Write((UInt32)(width * height * 4));                 // biSizeImage;
                bw.Write((Int32)0);                                     // biXPelsPerMeter;
                bw.Write((Int32)0);                                     // biYPelsPerMeter;
                bw.Write((UInt32)0);                                    // biClrUsed;
                bw.Write((UInt32)0);                                    // biClrImportant;

                // switch the image data from RGB to BGR
                for (int imageIdx = 0; imageIdx < imageData.Length; imageIdx += 3)
                {
                    bw.Write(imageData[imageIdx + 2]);
                    bw.Write(imageData[imageIdx + 1]);
                    bw.Write(imageData[imageIdx + 0]);
                    bw.Write((byte)255);
                }
            }

            fileStream.Close();
        }
    }
}