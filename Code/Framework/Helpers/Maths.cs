using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text; 
using System.Threading.Tasks;
using Godot;
namespace AssGameFramework.Helpers
{
    public static class Maths
    {
        public static double SlopeToAngle(double x, double y)
        {
            return System.Math.Atan(y/x);
        }
        
        public static Vector2 VectorAbs(Vector2 v)
        {
            return new Vector2(System.Math.Abs(v.x),System.Math.Abs(v.y));
        }
    }
}