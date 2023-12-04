using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using Inheritance.Geometry.Virtual;

namespace Inheritance.Geometry.Visitor
{
    public abstract class Body
    {
        public Vector3 Position { get; }

        protected Body(Vector3 position)
        {
            Position = position;
        }

        public abstract Body Accept(IVisitor visitor);
    }

    public class Ball : Body
    {
        public double Radius { get; }

        public Ball(Vector3 position, double radius) : base(position)
        {
            Radius = radius;
        }
        public override Body Accept(IVisitor visitor)
        {
            if (visitor is BoxifyVisitor boxifyVisitor)
                return boxifyVisitor.Visit(this);
            else if(visitor is BoundingBoxVisitor boundingBoxVisitor)
                return boundingBoxVisitor.Visit(this);
            return this;
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
        public override Body Accept(IVisitor visitor)
        {
            if (visitor is BoxifyVisitor boxifyVisitor)
                return boxifyVisitor.Visit(this);
            else if(visitor is BoundingBoxVisitor boundingBoxVisitor)
                return boundingBoxVisitor.Visit(this);
            else throw new Exception();
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
        public override Body Accept(IVisitor visitor)
        {
            if (visitor is BoxifyVisitor boxifyVisitor)
                return boxifyVisitor.Visit(this);
            else if(visitor is BoundingBoxVisitor boundingBoxVisitor)
                return boundingBoxVisitor.Visit(this);
            else throw new Exception();
        }
    }

    public class CompoundBody : Body
    {
        public IReadOnlyList<Body> Parts { get; }
        public CompoundBody(IReadOnlyList<Body> parts) : base(CompoundBody.GetBodyPos(parts))
        {
            Parts = parts;
        }
        private static Vector3 GetBodyPos(IReadOnlyList<Body> parts)
        {
            var visitor = new BoundingBoxVisitor();
            var xMax = parts.Select(x=> x.TryAcceptVisitor<RectangularCuboid>(visitor).SizeX/2 + x.Position.X).Max();
            var yMax = parts.Select(x=> x.TryAcceptVisitor<RectangularCuboid>(visitor).SizeY/2 + x.Position.Y).Max();
            var zMax = parts.Select(x=> x.TryAcceptVisitor<RectangularCuboid>(visitor).SizeZ/2 + x.Position.Z).Max();
            var xMin = parts.Select(x=> x.Position.X - x.TryAcceptVisitor<RectangularCuboid>(visitor).SizeX/2).Min();
            var yMin = parts.Select(x=> x.Position.Y - x.TryAcceptVisitor<RectangularCuboid>(visitor).SizeY/2).Min();
            var zMin = parts.Select(x=> x.Position.Z - x.TryAcceptVisitor<RectangularCuboid>(visitor).SizeZ/2).Min();
            var vectorX = (xMax + xMin)/2;
            var vectorY = (yMax + yMin)/2;
            var vectorZ = (zMax + zMin)/2;
            return new Vector3(vectorX, vectorY, vectorZ);
        }
        public override Body Accept(IVisitor visitor)
        {
            if (visitor is BoxifyVisitor boxifyVisitor)
                return boxifyVisitor.Visit(this);
            else if(visitor is BoundingBoxVisitor boundingBoxVisitor)
                return boundingBoxVisitor.Visit(this);
            else throw new Exception();
        }
    }

    public class BoundingBoxVisitor : IVisitor
    {
        public Body Visit(Ball ball)
        {
            return new RectangularCuboid
            (ball.Position,
            ball.Radius * 2,
            ball.Radius * 2,
            ball.Radius * 2);
        }

        public Body Visit(RectangularCuboid rectangularCuboid)
        {
            return rectangularCuboid;
        }

        public Body Visit(Cylinder cylinder)
        {
            return new RectangularCuboid
            (cylinder.Position,
            cylinder.Radius * 2,
            cylinder.Radius * 2,
            cylinder.SizeZ);
        }

        public Body Visit(CompoundBody compoundBody)
        {
            var xMax = compoundBody.Parts.Select
                (x=> x.TryAcceptVisitor<RectangularCuboid>(new BoundingBoxVisitor()).SizeX/2 + x.Position.X).Max();
            var xMin = compoundBody.Parts.Select(x=> x.Position.X - x.TryAcceptVisitor<RectangularCuboid>(new BoundingBoxVisitor()).SizeX/2).Min();
            var yMax = compoundBody.Parts.Select
                (x=> x.TryAcceptVisitor<RectangularCuboid>(new BoundingBoxVisitor()).SizeY/2 + x.Position.Y).Max();
            var yMin = compoundBody.Parts.Select(x=> x.Position.Y - x.TryAcceptVisitor<RectangularCuboid>(new BoundingBoxVisitor()).SizeY/2).Min();
            var zMax = compoundBody.Parts.Select
                (x=> x.TryAcceptVisitor<RectangularCuboid>(new BoundingBoxVisitor()).SizeZ/2 + x.Position.Z).Max();
            var zMin = compoundBody.Parts.Select(x=> x.Position.Z - x.TryAcceptVisitor<RectangularCuboid>(new BoundingBoxVisitor()).SizeZ/2).Min();
            
            return new RectangularCuboid(compoundBody.Position,
            xMax - xMin,
            yMax - yMin,
            zMax - zMin);
        }
    }

    public class BoxifyVisitor : IVisitor
    {
        //TODO
        public Body Visit(Ball ball)
        {
            return ball.TryAcceptVisitor<RectangularCuboid>(new BoundingBoxVisitor());
        }

        public Body Visit(RectangularCuboid rectangularCuboid)
        {
            return rectangularCuboid.TryAcceptVisitor<RectangularCuboid>(new BoundingBoxVisitor());
        }

        public Body Visit(Cylinder cylinder)
        {
            return cylinder.TryAcceptVisitor<RectangularCuboid>(new BoundingBoxVisitor());
        }

        public Body Visit(CompoundBody compoundBody)
        {
            var bodyList = new List<Body>();
            foreach (var item in compoundBody.Parts)
            {
                if (item is CompoundBody)
                    bodyList.Add(item.TryAcceptVisitor<CompoundBody>(new BoxifyVisitor()));
                else
                    bodyList.Add(item.TryAcceptVisitor<RectangularCuboid>(new BoxifyVisitor()));
            }
            return new CompoundBody(bodyList);
        }
    }

    public interface IVisitor
    {
        Body Visit(Ball ball);
        Body Visit(RectangularCuboid rectangularCuboid);
        Body Visit(Cylinder cylinder);
        Body Visit(CompoundBody compoundBody);
    }
}