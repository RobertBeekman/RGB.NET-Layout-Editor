using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace LayoutEditor.UI.Editors
{
    public class ShapeEditor
    {
        public Point PendingPoint { get; set; }
        public List<Point> Points { get; set; } = new List<Point>();
        public int? RoundDecimals { get; set; }

        public void Click(Point point)
        {
            point = RoundPoint(point);
            Points.Add(point);
        }

        public void Move(Point point)
        {
            point = RoundPoint(point);
            PendingPoint = point;
        }

        public Geometry GetGeometry(bool includePending)
        {
            if (!Points.Any())
                return new EllipseGeometry(PendingPoint, 0.025, 0.025);
            if (Points.Count == 1)
            {
                return new GeometryGroup() {Children = new GeometryCollection()
                    {
                        new EllipseGeometry(Points.First(), 0.025, 0.025),
                        new EllipseGeometry(PendingPoint, 0.025, 0.025)
                    }
                };
            }

            var pathFigure = new PathFigure(Points.First(), new List<PathSegment>(), true);
            var pathGeometry = new PathGeometry();
            pathGeometry.Figures.Add(pathFigure);
            foreach (var point in Points.Skip(1))
                pathFigure.Segments.Add(new LineSegment(point, true));
            if (includePending)
                pathFigure.Segments.Add(new LineSegment(PendingPoint, true));

            return pathGeometry;
        }

        private Point RoundPoint(Point point)
        {
            return RoundDecimals != null 
                ? new Point(Math.Round(point.X, RoundDecimals.Value, MidpointRounding.AwayFromZero), Math.Round(point.Y, RoundDecimals.Value, MidpointRounding.AwayFromZero)) 
                : point;
        }
    }
}