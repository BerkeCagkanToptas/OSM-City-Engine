using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Assets.Scripts.Utils;
using Assets.Scripts.OpenStreetMap;
using System.IO;

namespace Assets.Scripts.HeightMap
{

    public struct HeightmapInfo
    {
        public int Lat, Lon;
        public Vector2 coordmin, coordmax;
        public  float sizeX, sizeZ;  //deltaX, deltaZ -- Sample size in meter
        public float height;
        public float width;
    };


    public enum HeightmapContinent
    {
        North_America,
        South_America,
        Africa,
        Eurasia,
        Australia
    }

    public class HeighmapLoader
    {
        //HeightmapInfo from loaded SRTM data
        //public HeightmapInfo heightMapInfo;
        //Specify which continent is heightmap inside (For final url)
        public HeightmapContinent continent;
        //Base url of NASA srtm3 heightmap 
        public const string baseURL = "http://dds.cr.usgs.gov/srtm/version2_1/SRTM3/";
        //Heightmap Array 1201x1201 
        public short[,] heightmap;

        //Constructor create heightmap object
        public HeighmapLoader(BBox bbox,HeightmapContinent _continent)
        {

            if (!Directory.Exists(Path.Combine(Application.persistentDataPath, "HeightmapFiles/")))
                Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, "HeightmapFiles/"));

            heightmap = new short[1201,1201];
            continent = _continent;

            if ((Math.Floor(bbox.left) != Math.Floor(bbox.right)) || (Math.Floor(bbox.bottom) != Math.Floor(bbox.top)))
            {
                Debug.Log("<color=red>HEIGHTMAP ERROR:</color> Specified area requires multiple heightmap files!");
                return;
            }

            string filename = "";
 
            if(Math.Floor(bbox.bottom) >= 0.0f)
                filename = filename + "N" + Math.Floor(bbox.bottom).ToString("00");
            else
                filename = filename + "S" + Math.Floor(bbox.bottom).ToString("00");

            if(Math.Floor(bbox.left) >= 0.0f)
                filename = filename + "E" + Math.Floor(bbox.left).ToString("000");
            else
                filename = filename + "W" + Math.Floor(bbox.left).ToString("000");

            string savedFilename = filename + ".hgt";
            filename = filename + ".hgt.zip";

            Debug.Log("<color=blue>HEIGHTMAP</color> Filename: " + filename);

            string fullURL = baseURL + continent.ToString("G") + "/" + filename;


            string savePath = Path.Combine(Application.persistentDataPath, "HeightmapFiles/" + filename);

            FileDownloader.downloadfromURL(fullURL,savePath);

            Debug.Log("<color=blue>HEIGHTMAP</color> Download Complete!!");

            string extractPath = Path.Combine(Application.persistentDataPath, "HeightmapFiles");
            if (!File.Exists(extractPath))
                UniZip.Unzip(savePath, extractPath);
            
            Debug.Log("<color=blue>HEIGHTMAP</color> Filemap Uncompress Complete!!");

            fillHeightmap(extractPath + "/" + savedFilename);

            Debug.Log("<color=blue>HEIGHTMAP</color> Filemap Loading Complete!!");
          
               
        }
      	
        //Read raw data from .hgt file write it into heightmap array
        private void fillHeightmap(string savePath)
        {
            try
            {
                using (var stream = new FileStream(savePath, FileMode.Open))
                {
                   
                    byte[] bytebuffer = new byte[stream.Length];
                    bytebuffer = ReadFully(stream);

                    byte[] buffer = new byte[2];
                    int it = 0;
                    for (int i = 0; i < 1201; i++)
                    {

                        for(int j=0 ; j < 1201; j++)
                        {
                            buffer[0] = bytebuffer[it + 1];
                            buffer[1] = bytebuffer[it];                          
                            short number = BitConverter.ToInt16(buffer, 0);

                            if (number < -1000 && j > 0)
                                number = heightmap[i, j];
                            if (number < -1000 && j == 0)
                                number = heightmap[i - 1, j];

                            heightmap[i, j] = number;
                            it += 2;
                        }
                    }

                }
            }
            catch(FileNotFoundException)
            {
                Debug.Log("<color=red>ERROR</color> Heightmap is not exist");
            }
        }

        //Converts binary raw data to byteArray
        private byte[] ReadFully(FileStream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

    }
}
