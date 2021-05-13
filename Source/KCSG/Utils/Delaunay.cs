﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KCSG.Utils
{
    public static class Delaunay
    {
        private static IEnumerable<Triangle> border;
        public static IEnumerable<Triangle> Run(IEnumerable<KVector> points, double maxX, double maxY)
        {
            KVector point0 = new KVector(0, 0);
            KVector point1 = new KVector(0, maxY);
            KVector point2 = new KVector(maxX, maxY);
            KVector point3 = new KVector(maxX, 0);
            Triangle triangle1 = new Triangle(point0, point1, point2);
            Triangle triangle2 = new Triangle(point0, point2, point3);
            border = new List<Triangle>() { triangle1, triangle2 };

            HashSet<Triangle> triangulation = new HashSet<Triangle>(border);

            foreach (KVector point in points)
            {
                ISet<Triangle> badTriangles = FindBadTriangles(point, triangulation);
                List<Edge> polygon = FindHoleBoundaries(badTriangles);

                foreach (Triangle triangle in badTriangles)
                {
                    foreach (KVector vertex in triangle.Vertices)
                    {
                        vertex.AdjacentTriangles.Remove(triangle);
                    }
                }
                triangulation.RemoveWhere(o => badTriangles.Contains(o));

                foreach (Edge edge in polygon.Where(possibleEdge => possibleEdge.Point1 != point && possibleEdge.Point2 != point))
                {
                    Triangle triangle = new Triangle(point, edge.Point1, edge.Point2);
                    triangulation.Add(triangle);
                }
            }
            triangulation.RemoveWhere(t => t.Vertices.ToList().Find(v => v.X == 0 || v.Y == 0 || v.X == maxX || v.Y == maxY) != null);
            return triangulation;
        }

        private static ISet<Triangle> FindBadTriangles(KVector point, HashSet<Triangle> triangles)
        {
            IEnumerable<Triangle> badTriangles = triangles.Where(o => o.IsPointInsideCircumcircle(point));
            return new HashSet<Triangle>(badTriangles);
        }

        private static List<Edge> FindHoleBoundaries(ISet<Triangle> badTriangles)
        {
            List<Edge> edges = new List<Edge>();
            foreach (var triangle in badTriangles)
            {
                edges.Add(new Edge(triangle.Vertices[0], triangle.Vertices[1]));
                edges.Add(new Edge(triangle.Vertices[1], triangle.Vertices[2]));
                edges.Add(new Edge(triangle.Vertices[2], triangle.Vertices[0]));
            }
            IEnumerable<IGrouping<Edge, Edge>> grouped = edges.GroupBy(o => o);
            IEnumerable<Edge> boundaryEdges = edges.GroupBy(o => o).Where(o => o.Count() == 1).Select(o => o.First());
            return boundaryEdges.ToList();
        }
    }

    public class Edge
    {
        public Edge(KVector point1, KVector point2)
        {
            Point1 = point1;
            Point2 = point2;
        }

        public KVector Point1 { get; }
        public KVector Point2 { get; }
        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            if (obj.GetType() != GetType()) return false;
            Edge edge = obj as Edge;

            bool samePoints = Point1 == edge.Point1 && Point2 == edge.Point2;
            bool samePointsReversed = Point1 == edge.Point2 && Point2 == edge.Point1;
            return samePoints || samePointsReversed;
        }

        public override int GetHashCode()
        {
            int hCode = (int)Point1.X ^ (int)Point1.Y ^ (int)Point2.X ^ (int)Point2.Y;
            return hCode.GetHashCode();
        }
    }

    public class Triangle
    {
        public double RadiusSquared;
        public Triangle(KVector point1, KVector point2, KVector point3)
        {
            Vertices[0] = point1;
            if (!IsCounterClockwise(point1, point2, point3))
            {
                Vertices[1] = point3;
                Vertices[2] = point2;
            }
            else
            {
                Vertices[1] = point2;
                Vertices[2] = point3;
            }

            Vertices[0].AdjacentTriangles.Add(this);
            Vertices[1].AdjacentTriangles.Add(this);
            Vertices[2].AdjacentTriangles.Add(this);
            UpdateCircumcircle();
        }

        public KVector Circumcenter { get; private set; }
        public KVector[] Vertices { get; } = new KVector[3];
        public bool IsPointInsideCircumcircle(KVector point)
        {
            double d_squared = (point.X - Circumcenter.X) * (point.X - Circumcenter.X) + (point.Y - Circumcenter.Y) * (point.Y - Circumcenter.Y);
            return d_squared < RadiusSquared;
        }

        private bool IsCounterClockwise(KVector point1, KVector point2, KVector point3)
        {
            double result = (point2.X - point1.X) * (point3.Y - point1.Y) - (point3.X - point1.X) * (point2.Y - point1.Y);
            return result > 0;
        }

        private void UpdateCircumcircle()
        {
            // https://codefound.wordpress.com/2013/02/21/how-to-compute-a-circumcircle/#more-58
            KVector p0 = Vertices[0];
            KVector p1 = Vertices[1];
            KVector p2 = Vertices[2];
            double dA = p0.X * p0.X + p0.Y * p0.Y;
            double dB = p1.X * p1.X + p1.Y * p1.Y;
            double dC = p2.X * p2.X + p2.Y * p2.Y;

            double aux1 = dA * (p2.Y - p1.Y) + dB * (p0.Y - p2.Y) + dC * (p1.Y - p0.Y);
            double aux2 = -(dA * (p2.X - p1.X) + dB * (p0.X - p2.X) + dC * (p1.X - p0.X));
            double div = 2 * (p0.X * (p2.Y - p1.Y) + p1.X * (p0.Y - p2.Y) + p2.X * (p1.Y - p0.Y));

            if (div == 0)
            {
                throw new DivideByZeroException();
            }

            KVector center = new KVector(aux1 / div, aux2 / div);
            Circumcenter = center;
            RadiusSquared = (center.X - p0.X) * (center.X - p0.X) + (center.Y - p0.Y) * (center.Y - p0.Y);
        }
    }
}
