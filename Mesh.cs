﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using Lotus.ECS;

namespace Lotus
{
    public abstract class Mesh
    {

        public abstract void RenGen(); // here you would program the GL to render the object at Vector3.Zero and as Quaternion.Identity for the rotation.
        public abstract Bounds GetBounds(); //Returns the Axis-Aligned Bounding-Box corners of the mesh, for culling purposes

        private Matrix4 viewMatrix;
        private Matrix4 normalMatrix;

        public Vector3 ToWorldPoint(Vector3 p) {
            return Vector3.Transform(p, viewMatrix);
        }

        public Vector3 ToWorldNormal(Vector3 n) {
            return Vector3.Transform(n, normalMatrix);
        }

        protected Color4 baseColor;

        public Color4 GetColor(Vector3 normal, Vector3 vertex) {
            if (Camera.Current.UseLighting.Value) {
                return Light.GetColor(ToWorldNormal(normal), ToWorldPoint(vertex), baseColor);
            }
            else {
                return baseColor;
            }
        }

        public void DrawVertex(Vector3 vertex, Vector3 normal) {
            GL.Color4(GetColor(normal, vertex));
            GL.Vertex3(vertex);
        }

        public virtual void Update() { }

        public void Draw(Matrix4 viewMatrix, Matrix4 normalMatrix, Color4 baseColor)
        {
            GL.PushMatrix();
            this.viewMatrix = viewMatrix;
            this.normalMatrix = normalMatrix;
            this.baseColor = baseColor;
            GL.MultMatrix(ref viewMatrix);
            RenGen();
            GL.PopMatrix();
        }
    }
}
