﻿/****************************** Module Header ******************************\ 
Module Name:    TiffImageConverter.cs 
Project:        CSTiffImageConverter
Copyright (c) Microsoft Corporation. 

The class defines the helper methods for converting TIFF from or to JPEG 
file(s)

This source is subject to the Microsoft Public License.
See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
All other rights reserved.

THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE. 
\***************************************************************************/

using System;
using System.IO;
using System.Linq;
using System.Drawing.Imaging;
using System.Drawing;
using System.Windows.Forms;


namespace CSTiffImageConverter
{
    public class ImageConverter
    {

        /// <summary>
        /// Converts image(s) in desired format
        /// </summary>
        /// <param name="fileNames">
        /// String array having full name to image(s).
        /// </param>
        /// <param name="outputFormat">
        /// Desired image(s) ouput format
        /// </param>
        /// <param name="isMultipage">
        /// true to create single multipage tiff file otherwise, false.
        /// </param>
        /// <returns>
        /// String array having full name to images.
        /// </returns>
        public static string[] ConvertImage(string[] fileNames, ImageFormat outputFormat, bool isMultipage)
        {
            
            using (Image imageFile = Image.FromFile(fileNames[0]))
            {
             
                // is output TIFF image and Multipage ?
                if(isMultipage && outputFormat.Equals(System.Drawing.Imaging.ImageFormat.Tiff))
                {
                    EncoderParameters encoderParams = new EncoderParameters(1);
                    ImageCodecInfo tiffCodecInfo = ImageCodecInfo.GetImageEncoders()
                        .First(ie => ie.MimeType == "image/tiff");

                    string[] tiffPaths = null; 

                    tiffPaths = new string[1];
                    Image tiffImg = null;
                    try
                    {
                        for (int i = 0; i < fileNames.Length; i++)
                        {
                            if (i == 0)
                            {
                                tiffPaths[i] = String.Format("{0}\\{1}.tif",
                                    Path.GetDirectoryName(fileNames[i]),
                                    Path.GetFileNameWithoutExtension(fileNames[i]));

                                // Initialize the first frame of multipage tiff.
                                tiffImg = Image.FromFile(fileNames[i]);
                                encoderParams.Param[0] = new EncoderParameter(
                                    Encoder.SaveFlag, (long)EncoderValue.MultiFrame);
                                tiffImg.Save(tiffPaths[i], tiffCodecInfo, encoderParams);
                            }
                            else
                            {
                                // Add additional frames.
                                encoderParams.Param[0] = new EncoderParameter(
                                    Encoder.SaveFlag, (long)EncoderValue.FrameDimensionPage);
                                using (Image frame = Image.FromFile(fileNames[i]))
                                {
                                    tiffImg.SaveAdd(frame, encoderParams);
                                }
                            }

                            if (i == fileNames.Length - 1)
                            {
                                // When it is the last frame, flush the resources and closing.
                                encoderParams.Param[0] = new EncoderParameter(
                                    Encoder.SaveFlag, (long)EncoderValue.Flush);
                                tiffImg.SaveAdd(encoderParams);
                            }
                        }
                    }
                    finally
                    {
                        if (tiffImg != null)
                        {
                            tiffImg.Dispose();
                            tiffImg = null;
                        }
                    }
                     

                    return null;
                } 
                else
                {
                    FrameDimension frameDimensions = new FrameDimension(
                    imageFile.FrameDimensionsList[0]);

                    // Gets the number of pages from the tiff image (if multipage)
                    int frameNum = imageFile.GetFrameCount(frameDimensions);
                    string[] imagePaths = new string[frameNum];


                    for (int frame = 0; frame < frameNum; frame++)
                    {
                        // Selects one frame at a time and save as jpeg.
                        imageFile.SelectActiveFrame(frameDimensions, frame);
                        using (Bitmap bmp = new Bitmap(imageFile))
                        {
                            imagePaths[frame] = String.Format("{0}\\{1}{2}." + outputFormat.ToString().ToLowerInvariant(),
                                Path.GetDirectoryName(fileNames[0]),
                                Path.GetFileNameWithoutExtension(fileNames[0]),
                                frame);
                            bmp.Save(imagePaths[frame], outputFormat);
                        }
                    }
                    

                    return imagePaths;
                }
            }
        }
    }
}