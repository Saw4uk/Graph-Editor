using System;
using UnityEngine;

public static class CustomMath
{
    public static bool SegmentsIsIntersects(Vector2 v11, Vector2 v12, Vector2 v21, Vector2 v22)
    {
        var cut1 = v12 - v11;
        var cut2 = v22 - v21;

        var prod1 = Vector3.Cross(cut1, v21 - v11);
        var prod2 = Vector3.Cross(cut1, v22 - v11);

        if (Math.Sign(prod1.z) == Math.Sign(prod2.z))
            return false;
        
        prod1 = Vector3.Cross(cut2, v11 - v21);
        prod2 = Vector3.Cross(cut2, v12 - v21);
        
        if (Math.Sign(prod1.z) == Math.Sign(prod2.z))
            return false;

        return true;

        // var a1 = p2.x - p1.x;
        // var a2 = s1.x - s2.x;
        // var x = s1.x - p1.x;
        //
        // var b1 = p2.y - p1.y;
        // var b2 = s1.y - s2.y;
        // var y = s1.y - p1.y;
        //
        // var determinant = a1 * b2 - b1 * a2;
        // if (determinant == 0)
        //     return false;
        // var determinantX = x * b2 - y * a2;
        // var determinantY = a1 * y - b1 * x;
        //
        // var s = determinantX / determinant;
        // var t = determinantY / determinant;
        //
        // return s >= 0 && t <= 1;
    }
}