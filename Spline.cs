﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Lotus {
    public class Spline : Mesh {

        public List<Point> Points = new List<Point>();

        public Spline(params Point[] points) {
            Points.AddRange(points);
        }

        public class Point {
            public Vector3 Position;
            public Vector3 LeftControl;
            public Vector3 RightControl;

            public Point(Vector3 pos) {
                Position = pos;
            }

            public Point(Vector3 pos, Vector3 left, Vector3 right)
                : this(pos) {
                LeftControl = left;
                RightControl = right;
            }
        }

        public Vector3 Evaluate(Point A, Point B, float t) {
            float a = t;
            float b = 1f - a;
            return A.Position * a * a * a + (A.Position + A.RightControl) * 3 * a * a * b + (B.Position + B.LeftControl) * 3 * a * b * b + B.Position * b * b * b;
        }

        const int SEGMENTS = 24;

        public override void RenGen() {
            GL.Begin(PrimitiveType.Polygon);
            GL.Color4(Color4.White);
            for (int i = 0; i < Points.Count; i++) {
                float step = 1f/SEGMENTS;
                for (float t = 0f; t <= 1.0001f; t += step) {
                    GL.Vertex3(Evaluate(Points[i], Points[(i + 1) % Points.Count], t) - Vector3.UnitZ * 2f);
                }
            }
            GL.End();
            GL.Begin(PrimitiveType.Lines);
            GL.Color4(Color4.Yellow);
            for (int i = 0; i < Points.Count; i++) {
                GL.Vertex3(Points[i].Position);
                GL.Vertex3(Points[i].Position + Points[i].LeftControl);
                GL.Vertex3(Points[i].Position);
                GL.Vertex3(Points[i].Position + Points[i].RightControl);
            }
            GL.End();
            GL.PointSize(4f);
            GL.Begin(PrimitiveType.Points);
            GL.Color4(Color4.Yellow);
            for (int i = 0; i < Points.Count; i++) {
                GL.Vertex3(Points[i].Position + Points[i].LeftControl);
                GL.Vertex3(Points[i].Position + Points[i].RightControl);
            }
            GL.End();
            GL.PointSize(8f);
            GL.Begin(PrimitiveType.Points);
            GL.Color4(Color4.Cyan);
            for (int i = 0; i < Points.Count; i++) {
                GL.Vertex3(Points[i].Position);
            }
            GL.End();
        }
    }
}