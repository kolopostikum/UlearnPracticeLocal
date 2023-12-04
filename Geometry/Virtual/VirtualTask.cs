using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Headers;
using Microsoft.VisualBasic;

namespace Inheritance.Geometry.Virtual
{
    public abstract class Body
    {
        public Vector3 Position { get; }

        protected Body(Vector3 position)
        {
            Position = position;
        }

        public abstract bool ContainsPoint(Vector3 point);

        public abstract RectangularCuboid GetBoundingBox();
    }

    public class Ball : Body
    {
        public double Radius { get; }

        public Ball(Vector3 position, double radius) : base(position)
        {
            Radius = radius;
        }

        public override bool ContainsPoint(Vector3 point)
        {
            var vector = point - Position;
            var length2 = vector.GetLength2();
            return length2 <= this.Radius * this.Radius;
        }

        public override RectangularCuboid GetBoundingBox()
        {
            return new RectangularCuboid
            (this.Position,
            this.Radius * 2,
            this.Radius * 2,
            this.Radius * 2);
        }
    }

    public class RectangularCuboid : Body
    {
        public double SizeX { get; }
        public double SizeY { get; }
        public double SizeZ { get; }

        public RectangularCuboid(Vector3 position, double sizeX, double sizeY, double sizeZ) : base(position)
        {
            SizeX = sizeX;
            SizeY = sizeY;
            SizeZ = sizeZ;
        }

        public override bool ContainsPoint(Vector3 point)
        {
            var minPoint = new Vector3(
            Position.X - this.SizeX / 2,
            Position.Y - this.SizeY / 2,
            Position.Z - this.SizeZ / 2);
            var maxPoint = new Vector3(
            Position.X + this.SizeX / 2,
            Position.Y + this.SizeY / 2,
            Position.Z + this.SizeZ / 2);

            return point >= minPoint && point <= maxPoint;
        }

        public override RectangularCuboid GetBoundingBox()
        {
            return this;
        }
    }

    public class Cylinder : Body
    {
        public double SizeZ { get; }

        public double Radius { get; }

        public Cylinder(Vector3 position, double sizeZ, double radius) : base(position)
        {
            SizeZ = sizeZ;
            Radius = radius;
        }

        public override bool ContainsPoint(Vector3 point)
        {
            var vectorX = point.X - Position.X;
            var vectorY = point.Y - Position.Y;
            var length2 = vectorX * vectorX + vectorY * vectorY;
            var minZ = Position.Z - this.SizeZ / 2;
            var maxZ = minZ + this.SizeZ;

            return length2 <= this.Radius * this.Radius && point.Z >= minZ && point.Z <= maxZ;
        }

        public override RectangularCuboid GetBoundingBox()
        {
            return new RectangularCuboid
            (this.Position,
            this.Radius * 2,
            this.Radius * 2,
            this.SizeZ);
        }
    }

    public class CompoundBody : Body
    {
        public IReadOnlyList<Body> Parts { get; }
        public double XMax 
        {
            get 
            {
                return this.Parts.Select(x=> x.GetBoundingBox().SizeX/2 + x.Position.X).Max();
            }
        }
        public double XMin
        {
            get 
            {
                return this.Parts.Select(x=> x.Position.X - x.GetBoundingBox().SizeX/2).Min();
            }
        }
        public double YMax 
        {
            get 
            {
                return this.Parts.Select(x=> x.GetBoundingBox().SizeY/2 + x.Position.Y).Max();
            }
        }
        public double YMin 
        {
            get 
            {
                return this.Parts.Select(x=> x.Position.Y - x.GetBoundingBox().SizeY/2).Min();
            }
        }
        public double ZMax 
        {
            get 
            {
                return this.Parts.Select(x=> x.GetBoundingBox().SizeZ/2 + x.Position.Z).Max();
            }
        }
        public double ZMin
        {
            get 
            {
                return this.Parts.Select(x=> x.Position.Z - x.GetBoundingBox().SizeZ/2).Min();
            }
        }
        public CompoundBody(IReadOnlyList<Body> parts) : 
        base(CompoundBody.GetBodyPos(parts))
        {
            Parts = parts;
        }

        private static Vector3 GetBodyPos(IReadOnlyList<Body> parts)
        {
            var xMax = parts.Select(x=> x.GetBoundingBox().SizeX/2 + x.Position.X).Max();
            var yMax = parts.Select(x=> x.GetBoundingBox().SizeY/2 + x.Position.Y).Max();
            var zMax = parts.Select(x=> x.GetBoundingBox().SizeZ/2 + x.Position.Z).Max();
            var xMin = parts.Select(x=> x.Position.X - x.GetBoundingBox().SizeX/2).Min();
            var yMin = parts.Select(x=> x.Position.Y - x.GetBoundingBox().SizeY/2).Min();
            var zMin = parts.Select(x=> x.Position.Z - x.GetBoundingBox().SizeZ/2).Min();
            var vectorX = (xMax + xMin)/2;
            var vectorY = (yMax + yMin)/2;
            var vectorZ = (zMax + zMin)/2;
            return new Vector3(vectorX, vectorY, vectorZ);
        }

        public override bool ContainsPoint(Vector3 point)
        {
            return this.Parts.Any(body => body.ContainsPoint(point));
        }

        public override RectangularCuboid GetBoundingBox()
        {
            return new RectangularCuboid(Position,
            XMax - XMin,
            YMax - YMin,
            ZMax - ZMin);
        }
    }
}