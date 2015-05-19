using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts.Utils
{
    class Geometry
    {
        public Vector3 findNormal(Vector3 point1, Vector3 point2, Vector3 point3)
        {
            return Vector3.Cross(point2-point1,point3-point2);    
        }

        public static bool getLineIntersection(ref Vector2 intersection, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
	    {
		    double A1, B1, C1, A2, B2, C2;

		    A1 = p1.y - p0.y;
		    B1 = p0.x - p1.x;
		    C1 = A1 * p0.x + B1 * p0.y;

		    A2 = p3.y - p2.y;
		    B2 = p2.x - p3.x;
		    C2 = A2 * p2.x + B2 * p2.y;

		    double delta = A1*B2 - A2*B1;

		    if (Math.Abs(delta) < 0.001)
		    {
			    return false;
		    }

		    float x = (float)((B2*C1 - B1*C2) / delta);
		    float y = (float)((A1*C2 - A2*C1) / delta);

		    if (Math.Min(p0.x, p1.x) <= x && Math.Max(p0.x, p1.x) >= x && Math.Min(p0.y, p1.y) <= y && Math.Max(p0.y, p1.y) >= y &&			
			    Math.Min(p2.x, p3.x) <= x && Math.Max(p2.x, p3.x) >= x && Math.Min(p2.y, p3.y) <= y && Math.Max(p2.y, p3.y) >= y)
		    {
			    intersection.x = x;
			    intersection.y = y;
			    return true;
		    }
		    else
			    return false;
	

	}

        public static bool getInfiniteLineIntersection(ref Vector2 intersection, Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3)
	    {
		    double A1, B1, C1, A2, B2, C2;

		    A1 = p1.y - p0.y;
		    B1 = p0.x - p1.x;
		    C1 = A1 * p0.x + B1 * p0.y;

		    A2 = p3.y - p2.y;
		    B2 = p2.x - p3.x;
		    C2 = A2 * p2.x + B2 * p2.y;

		    double delta = A1*B2 - A2*B1;

		    if (Math.Abs(delta) < 0.001)
		    {
			    return false;
		    }

		    float x = (float)((B2*C1 - B1*C2) / delta);
		    float y = (float)((A1*C2 - A2*C1) / delta);

		    intersection.x = x;
		    intersection.y = y;
		    return true;
	    }



    }
}
