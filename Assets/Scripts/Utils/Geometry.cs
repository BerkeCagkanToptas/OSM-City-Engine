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

        public static bool getLineIntersection(ref Vector2 intersection, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector2 _p0 = new Vector2(p0.x, p0.z);
            Vector2 _p1 = new Vector2(p1.x, p1.z);
            Vector2 _p2 = new Vector2(p2.x, p2.z);
            Vector2 _p3 = new Vector2(p3.x, p3.z);

            return getLineIntersection(ref intersection, _p0, _p1, _p2, _p3);
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

		    if (Math.Abs(delta) < 0.01)
		    {
			    return false;
		    }

		    float x = (float)((B2*C1 - B1*C2) / delta);
		    float y = (float)((A1*C2 - A2*C1) / delta);

		    intersection.x = x;
		    intersection.y = y;
		    return true;
	    }

        public static bool getVectorIntersection(ref Vector2 intersection,Vector2 p0, Vector2 p1,Vector3 forward1, Vector2 p2, Vector2 p3, Vector3 forward2)
        {
            Vector2 p0n = p0 + -100.0f * new Vector2(forward1.x,forward1.z).normalized;
            Vector2 p1n = p0 + 100.0f * new Vector2(forward1.x, forward1.z).normalized;
            Vector2 p2n = p2 + -100.0f * new Vector2(forward2.x, forward2.z).normalized;
            Vector2 p3n = p2 + 100.0f * new Vector2(forward2.x,forward2.z).normalized;
            return getLineIntersection(ref intersection, p0n, p1n, p2n, p3n);
        }

        public static bool getHalfVectorIntersection(ref Vector2 intersection, Vector2 p0, Vector2 p1, Vector3 forward1, Vector2 p2, Vector2 p3, Vector3 forward2)
        {
            Vector2 p1n = p0 + 100.0f * new Vector2(forward1.x, forward1.z).normalized;
            Vector2 p3n = p2 + 100.0f * new Vector2(forward2.x, forward2.z).normalized;
            return getLineIntersection(ref intersection, p0, p1n, p2, p3n);
        }

    }
}
