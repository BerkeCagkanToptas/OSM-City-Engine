using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Assets.Scripts.Utils;
using Assets.Scripts.OpenStreetMap;
using UnityEngine;
using System.IO;

namespace Assets.Scripts.HeightMap
{

    public enum MapProvider
    {
        BingMapAerial,
        BingMapStreet,
        MapQuest,
        OpenStreetMap,
        OpenStreetMapNoLabel
    }

   
    class TerrainTextureHandler
    {

        public int zoomLevel;
        public string tileFolder;
        private int tileSize = 256;

        public TerrainTextureHandler()
        {
            zoomLevel = 18;   

            tileFolder = Application.persistentDataPath + "/Textures/Tiles/";           
                      
            if(!Directory.Exists(tileFolder))
                Directory.CreateDirectory(tileFolder);

        }
        public Texture2D generateTexture(MapProvider provider, BBox bbox, int i, int j, string OSMFileName)
        {
            string[] proj = OSMFileName.Split(new char[] {'/', '\\'});
            string projectName = proj[proj.Length - 1];

            if (File.Exists(tileFolder + "/final/" + provider.ToString("G") + "_" + projectName + "_" + i + "_" + j + ".png"))
            {
                byte[] fileData;
                fileData = File.ReadAllBytes(tileFolder + "/final/" + provider.ToString("G") + "_" + projectName + "_" + i + "_" + j + ".png");
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(fileData);
                return texture;
            }

            Texture2D uncroppedTexture = generateUncroppedTexture(bbox, provider, i, j);
            Rect cropWindow = generateCroppingRect(bbox);
            Texture2D finalTexture = CropTexture(uncroppedTexture, cropWindow);

            if (!Directory.Exists(tileFolder + "/final/"))
                Directory.CreateDirectory(tileFolder + "/final/");

            var tex = new Texture2D(finalTexture.width, finalTexture.height);
            tex.SetPixels32(finalTexture.GetPixels32());
            tex.Apply(false);
            File.WriteAllBytes(tileFolder + "/final/" + provider.ToString("G") + "_" + projectName + "_" + i + "_" + j + ".png", tex.EncodeToPNG());

            return finalTexture;
        }
        private Texture2D generateUncroppedTexture(BBox bbox, MapProvider provider, int ii, int jj)
        {

            Vector2 mintileCoord = new Vector2();
            Vector2 maxtileCoord = new Vector2();

            FindTiles(bbox, ref mintileCoord, ref maxtileCoord);

            Debug.Log("<color=green>TEXTURE:</color> mintile X:" + mintileCoord.x + " Y:" + mintileCoord.y +
                " maxtile X:" + maxtileCoord.x + " Y:" + maxtileCoord.y);

            int ColumnCount = (int)(1 + maxtileCoord.x - mintileCoord.x);
            int RowCount = (int)(1 + mintileCoord.y - maxtileCoord.y);

            Texture2D[,] textures = new Texture2D[RowCount, ColumnCount];
            
            for (int i = (int)maxtileCoord.y, m=0; i <= (int)mintileCoord.y ; i++,m++)
            {
                for(int j= (int) mintileCoord.x, n=0 ; j <= (int) maxtileCoord.x ; j++, n++)
                {
                    textures[m,n] = DownloadTile(j, i, provider);
                }
            }

            Texture2D finalTexture = ConcatTexture(textures, ColumnCount, RowCount);
            return finalTexture;

        }
        private Rect generateCroppingRect(BBox bbox)
        {
            Geography geo = new Geography();

            Vector2 mintileCoord = new Vector2();
            Vector2 maxtileCoord = new Vector2();
            FindTiles(bbox, ref mintileCoord, ref maxtileCoord);

            float left, bottom, right, top;
            double res = geo.Resolution(zoomLevel);

            left = (float)Math.Round((bbox.meterLeft + geo.originShift) / res) - (mintileCoord.x * tileSize);
            right = (float)Math.Round((bbox.meterRight + geo.originShift) / res) - (mintileCoord.x * tileSize);

            bottom = (float)Math.Round((-bbox.meterBottom + geo.originShift) / res) - (maxtileCoord.y * tileSize);
            top = (float)Math.Round((-bbox.meterTop + geo.originShift) / res) - (maxtileCoord.y * tileSize);

            Rect croppingRect = new Rect(left, top, right - left, bottom - top);

            return croppingRect;
        }
        private void FindTiles(BBox bbox, ref Vector2 mintileCoord, ref Vector2 maxtileCoord)
        {         
            Geography geo = new Geography();

            mintileCoord = geo.MetersToTile(bbox.meterBottom,bbox.meterLeft,zoomLevel);
            maxtileCoord = geo.MetersToTile(bbox.meterTop, bbox.meterRight,zoomLevel);
        }
        private Texture2D DownloadTile(int tilex,int tiley, MapProvider provider)
        {
            string _URL = "";
            if(!Directory.Exists(tileFolder + provider.ToString("G")))
                Directory.CreateDirectory(tileFolder+ provider.ToString("G"));

            

            string savedfileName =  + zoomLevel + "_" + tilex + "_" + tiley;

            switch (provider)
            {
                case MapProvider.MapQuest:
                    _URL = "http://otile1.mqcdn.com/tiles/1.0.0/osm/" + zoomLevel.ToString() + "/" + tilex.ToString() + "/" + tiley.ToString() + ".png";
                    
                    savedfileName = savedfileName + ".png";
                    break;
                case MapProvider.OpenStreetMap:
                    _URL = "http://" + "a" + ".tile.openstreetmap.org/" + zoomLevel.ToString() + "/" + tilex.ToString() + "/" + tiley.ToString() + ".png";
                    savedfileName = savedfileName + ".png";
                    break;
            
                case MapProvider.OpenStreetMapNoLabel:
                    _URL = "http://a.tiles.wmflabs.org/osm-no-labels/" + zoomLevel.ToString() + "/" + tilex.ToString() + "/" + tiley.ToString() + ".png";
                    savedfileName = savedfileName + ".png";
                    break;

                case MapProvider.BingMapStreet:
                    _URL = "http://ecn.t" + ((tilex + tiley) % 7).ToString() + ".tiles.virtualearth.net/tiles/" + "r";
                    for (int i = zoomLevel - 1; i >= 0; i--)
                    {
                        _URL = _URL + (((((tiley >> i) & 1) << 1) + ((tilex >> i) & 1)));
                    }
                    _URL = _URL + ".png" + "?g=409&mkt=en-us";
                    savedfileName = savedfileName + ".png";
                    break;

                case MapProvider.BingMapAerial:
                    _URL = "http://ecn.t" + ((tilex + tiley) % 7).ToString() + ".tiles.virtualearth.net/tiles/" + "a";
                    for (int i = zoomLevel - 1; i >= 0; i--)
                    {
                        _URL = _URL + (((((tiley >> i) & 1) << 1) + ((tilex >> i) & 1)));
                    }
                    _URL = _URL + ".jpeg" + "?g=409&mkt=en-us";
                    savedfileName = savedfileName + ".jpg";
                    break;

                default:
                    return null;
            }

            try
            {
                FileDownloader.downloadfromURL(_URL, tileFolder + provider.ToString("G") + "/" + savedfileName);
            }
            catch
            {
                if(provider == MapProvider.OpenStreetMapNoLabel)
                {
                    _URL = "http://" + "a" + ".tile.openstreetmap.org/" + zoomLevel.ToString() + "/" + tilex.ToString() + "/" + tiley.ToString() + ".png";
                    FileDownloader.downloadfromURL(_URL, tileFolder + provider.ToString("G") + "/" + savedfileName);
                }

            }

            byte[] fileData;
            fileData = File.ReadAllBytes(tileFolder + provider.ToString("G") + "/" + savedfileName);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            return texture;
 
        }
        private Texture2D CropTexture(Texture2D originalTexture, Rect cropRect)
        {
            int bottom = originalTexture.height - (int)(cropRect.y + cropRect.height);

            Texture2D newTexture = new Texture2D((int)cropRect.width, (int)cropRect.height, TextureFormat.RGBA32, false);
            Color[] pixels = originalTexture.GetPixels((int)cropRect.x, bottom, (int)cropRect.width, (int)cropRect.height, 0);
            newTexture.SetPixels(pixels);
            newTexture.Apply();
            return newTexture;
        }
        private Texture2D ConcatTexture(Texture2D[,] textures, int ColumnCount, int RowCount)
        {
            Texture2D finalTexture = new Texture2D(ColumnCount * tileSize, RowCount * tileSize);

            for (int i = 0; i < RowCount; i++)
                for(int j=0 ; j < ColumnCount ; j++)
                    finalTexture.SetPixels(j * tileSize, (RowCount-i-1) * tileSize, tileSize, tileSize, textures[i, j].GetPixels());

            return finalTexture;
        }
    }

}
