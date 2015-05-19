using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.HeightMap
{
    class Geography
    {

    public Geography()
	{
		tileSize = 256;
		initialResolution = 2.0f * (float)Math.PI * 6378137.0f / (float)tileSize;  
	}

	//"Converts given lat/lon in WGS84 Datum to XY in Spherical Mercator EPSG:900913"
	public  Vector2 LatLontoMeters(float lat, float lon)
    {
        float meterx, meterz;
	    meterz = lon * originShift / 180.0f;
        meterx = (float)(Math.Log(Math.Tan((90.0f + lat) * Math.PI / 360.0f)) / (Math.PI / 180.0f));
	    meterx = meterx * originShift / 180.0f;

	    return new Vector2(meterx,meterz);

    }
	//"Converts XY point from Spherical Mercator EPSG:900913 to lat/lon in WGS84 Datum" 
	public  Vector2 meterstoLatLon(float meterx, float meterz)
    {
        float lat, lon;

        lon = (meterz / originShift) * 180.0f;
        lat = (meterx / originShift) * 180.0f;
        lat = 180.0f / (float)Math.PI * (float)(2.0f * Math.Atan(Math.Exp(lat * Math.PI / 180.0f)) - (float)Math.PI / 2.0f);

        return new Vector2(lat, lon);
    }

    public Vector2 meterstoLatLonDouble(float meterx,float meterz)
    {
        double lat, lon;

        lon = (meterz / originShift) * 180.0f;
        lat = (meterx / originShift) * 180.0f;
        lat = 180.0 / Math.PI *(2.0 * Math.Atan(Math.Exp(lat * Math.PI / 180.0)) - Math.PI / 2.0);

        return new Vector2((float)lat, (float)lon);
    }

	//"Converts EPSG:900913 to pyramid pixel Vector2s in given zoom level"
	public  Vector2 metertoPixel(float meterx, float meterz, int zoom)
    {
        float res = Resolution(zoom);
        float pixelX, pixelY;

        pixelY = (-meterx + originShift) / res;
        pixelX = (meterz + originShift) / res;

        return new Vector2(pixelX, pixelY);

    }

	//Converts pixel Vector2s in given zoom level of pyramid to EPSG:900913 
	public  Vector2 pixeltoMeter(float pixelX, float pixelY, int zoom)
    {
        float res = Resolution(zoom);
        float meterX = (pixelY * res) - originShift;
        float meterZ = (pixelX * res) - originShift;
        return new Vector2(meterX, meterZ);
    }

	//"Returns a tile covering region in given pixel Vector2s"
	public  Vector2 pixeltoTile(float pixelX, float pixelY)
    {
        int Tilex = (int)(Math.Ceiling(pixelX / (double)tileSize)) - 1;
        int Tiley = (int)(Math.Ceiling(pixelY / (double)tileSize)) - 1;

        return new Vector2(Tilex, Tiley);
    }

    //"Returns tile for given mercator coordinates"
    public Vector2  MetersToTile(float meterx, float meterz, int zoom)
    {
        Vector2 tempResult = metertoPixel(meterx, meterz, zoom);
        return pixeltoTile(tempResult.x, tempResult.y);
    }

    //"Resolution (meters/pixel) for given zoom level (measured at Equator)"
    public float Resolution(int zoom)
    {
        return initialResolution / (float)Math.Pow(2,zoom);
    }

    public readonly float originShift = (float)Math.PI * 6378137.0f;
	private int tileSize;
	private float initialResolution;




    }
}
