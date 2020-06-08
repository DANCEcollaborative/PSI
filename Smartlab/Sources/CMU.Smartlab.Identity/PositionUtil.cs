﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMU.Smartlab.Identity
{
    public class Point3D
    {
        public double x;
        public double y;
        public double z;

        public Point3D()
        {
            this.x = 0.0;
            this.y = 0.0;
            this.z = 0.0;
        }

        public Point3D(double x, double y, double z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Point3D(Point3D p)
        {
            this.x = p.x;
            this.y = p.y;
            this.z = p.z;
        }

        public static Point3D Zero()
        {
            return new Point3D();
        }

        public double Length()
        {
            return Math.Sqrt(this.x * this.x + this.y * this.y + this.z * this.z);
        }

        public Point3D Normalize()
        {
            double l = this.Length();
            return this / l;
        }

        public Point3D Abs()
        {
            Point3D p = new Point3D(Math.Abs(this.x), Math.Abs(this.y), Math.Abs(this.z));
            return p;
        }

        public override String ToString()
        {
            String s = $"Point3D:({this.x},{this.y},{this.z})";
            return s;
        }

        public override bool Equals(object obj)
        {
            return obj is Point3D d &&
                   x == d.x &&
                   y == d.y &&
                   z == d.z;
        }

        public override int GetHashCode()
        {
            var hashCode = 373119288;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            hashCode = hashCode * -1521134295 + z.GetHashCode();
            return hashCode;
        }

        public static Point3D operator +(Point3D p1, Point3D p2)
        {
            return new Point3D(p1.x + p2.x, p1.y + p2.y, p1.z + p2.z);
        }

        public static Point3D operator -(Point3D p1, Point3D p2)
        {
            return new Point3D(p1.x - p2.x, p1.y - p2.y, p1.z - p2.z);
        }

        // Dot product.
        public static double operator *(Point3D p1, Point3D p2)
        {
            return p1.x * p2.x + p1.y * p2.y + p1.z * p2.z;
        }

        public static Point3D operator *(Point3D p1, double lambda)
        {
            return new Point3D(p1.x * lambda, p1.y * lambda, p1.z * lambda);
        }

        public static Point3D operator *(double lambda, Point3D p1)
        {
            return p1 * lambda;
        }

        public static Point3D operator /(Point3D p1, double lambda)
        {
            return p1 / lambda;
        }

        public static bool operator ==(Point3D p1, Point3D p2)
        {
            return p1.x == p2.x && p1.y == p2.y && p1.z == p2.z;
        }

        public static bool operator !=(Point3D p1, Point3D p2)
        {
            return !(p1 == p2);
        }
    }
    public class Point2D
    {
        public double x;
        public double y;

        public Point2D()
        {
            this.x = 0.0;
            this.y = 0.0;
        }

        public Point2D(double x, double y)
        {
            this.x = x;
            this.y = y;
        }

        public Point2D(Point2D p)
        {
            this.x = p.x;
            this.y = p.y;
        }

        public Point2D Zero()
        {
            return new Point2D();
        }

        public double Length()
        {
            return Math.Sqrt(this.x * this.x + this.y * this.y);
        }

        public Point2D Normalize()
        {
            double l = this.Length();
            return this / l;
        }

        public Point2D Abs()
        {            
            Point2D p = new Point2D(Math.Abs(this.x), Math.Abs(this.y));
            return p;
        }

        public override String ToString()
        {
            String s = $"Point2D:({this.x}, {this.y})" ;
            return s;
        }

        public override bool Equals(object obj)
        {
            return obj is Point2D d &&
                   x == d.x &&
                   y == d.y;
        }

        public override int GetHashCode()
        {
            var hashCode = 1502939027;
            hashCode = hashCode * -1521134295 + x.GetHashCode();
            hashCode = hashCode * -1521134295 + y.GetHashCode();
            return hashCode;
        }

        public static Point2D operator +(Point2D p1, Point2D p2)
        {
            return new Point2D(p1.x + p2.x, p1.y + p2.y);
        }

        public static Point2D operator -(Point2D p1, Point2D p2)
        {
            return new Point2D(p1.x - p2.x, p1.y - p2.y);
        }

        // Dot product.
        public static double operator *(Point2D p1, Point2D p2)
        {
            return p1.x * p2.x + p1.y * p2.y;
        }

        public static Point2D operator *(Point2D p1, double lambda)
        {
            return new Point2D(p1.x * lambda, p1.y * lambda);
        }

        public static Point2D operator *(double lambda, Point2D p2)
        {
            return p2 * lambda;
        }

        public static Point2D operator /(Point2D p1, double lambda)
        {
            return new Point2D(p1.x / lambda, p1.y / lambda);
        }

        public static bool operator ==(Point2D p1, Point2D p2)
        {
            return p1.x == p2.x && p1.y == p2.y;
        }

        public static bool operator !=(Point2D p1, Point2D p2)
        {
            return !(p1 == p2);
        }
    }

    public class Line3D
    {
        public double x;
        public double y;
        public double z;
        public Point3D p0;
        public Point3D t;
        public Line3D(Point3D p0, Point3D t)
        {
            this.p0 = p0;
            this.t = t;
        }

        public Point3D Find_point_by_lambda(double lam)
        {
            return this.p0 + (this.t * lam);
        }

        public Point3D Find_point_by_x(double x)
        {
            if (x != 0)
            {
                double lam = (x - this.p0.x) / this.t.x;
                Point3D p = this.p0 + this.t * lam;
                return p;
            }
            else
            {
                Console.WriteLine("Can't find point by x for a line vertical to x axis!");
                return this.p0;
            }            
        }
        public Point3D Find_point_by_y(double y)
        {
            if (x != 0)
            {
                double lam = (y - this.p0.y) / this.t.y;
                Point3D p = this.p0 + this.t * lam;
                return p;
            }
            else
            {
                Console.WriteLine("Can't find point by x for a line vertical to y axis!");
                return this.p0;
            }
        }
        public Point3D Find_point_by_z(double z)
        {
            if (x != 0)
            {
                double lam = (z - this.p0.z) / this.t.z;
                Point3D p = this.p0 + this.t * lam;
                return p;
            }
            else
            {
                Console.WriteLine("Can't find point by x for a line vertical to z axis!");
                return this.p0;
            }
        }
        public String Str()
        {
            String s = $"Line3D:(\n\tp0:{this.p0}\n\tt:{this.t}\n)" ;
            return s;
        }
    }
}
