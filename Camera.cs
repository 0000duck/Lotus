﻿using System;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics.OpenGL;

namespace Lotus {
	public class Camera {

		public static Camera Main; //Shortcut to the first camera registered, which should be the 'scene' view
        public static Camera Current; //The camera most recently set up with .Draw()
        Matrix4 projectionMatrix; //The Matrix that determines whether the camera is orthographic, perspective, etc.
		public Vector3 Position; //The position in 3D space that the camera occupies
        public Quaternion Rotation; //The quaternion rotation of the camera, applied in YXZ order

        //TODO: move camera controls to separate class
        public bool FreelookEnabled;
		public float MoveSpeed = 10f; //How fast the freelook camera moves around
        public float RotateSpeed = 0.005f; //How fast the freelook camera rotates

        public readonly bool IsOrthographic; //Whether this camera is orthographic; cannot be changed after initialization
        public readonly bool UseAlphaBlend; //Whether to use simple alpha blending for transparency
        public readonly bool UseLighting; //Whether to use a directional light and normal shading

        public bool IsPerspective { //Whether this camera uses perspective projection
            get { return !IsOrthographic;  }
        }

		public Camera(float width, float height, bool ortho, bool blend, bool light) { //Creates a new camera, using the width and height of the screen and whether it is orthographic
            IsOrthographic = ortho;
            UseAlphaBlend = blend;
            UseLighting = light;
            if (Main == null) Main = this; //If this is the first created camera, designate it as the Main camera
            ResetProjectionMatrix(width, height);

		}

        public void ResetProjectionMatrix(float width, float height) {
            if (IsOrthographic) {
                projectionMatrix = Matrix4.CreateOrthographicOffCenter(0f, width, height, 0f, 0.1f, 256f);
            }
            else {
                projectionMatrix = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(60f), width / height, 0.1f, 256f);
                projectionMatrix *= Matrix4.CreateScale(-1f, -1f, 1f); //Invert X and Y to match screen coordinates
            }
        }

        public Matrix4 ProjectionMatrix { //The projection matrix used to create ortho/perspective rendering
            get {
                return projectionMatrix;
            }
        }

		public Matrix4 ViewMatrix { //The final view matrix used to draw the world
			get {
				return RotationMatrix * TranslationMatrix;
			}
		}

		public Matrix4 TranslationMatrix { //A matrix of the current position
			get {
				return Matrix4.CreateTranslation(Position);
			}
		}

		public Matrix4 RotationMatrix { //A matrix of the current rotation
			get {
				return Matrix4.CreateRotationZ(Rotation.Z) * Matrix4.CreateRotationX(Rotation.X) * Matrix4.CreateRotationY(Rotation.Y);
			}
		}

		public Vector3 Forward { //The direction the camera is facing in worldspace
			get {
				return Vector3.TransformPosition(-Vector3.UnitZ, RotationMatrix);
			}
		}

		public Vector3 Right { //The direction to the right of the camera in worldspace
			get {
				return Vector3.TransformPosition(-Vector3.UnitX, RotationMatrix);
			}
		}

		public Vector3 Up { //The direction to the top of the camera in worldspace
			get {
				return Vector3.TransformPosition(-Vector3.UnitY, RotationMatrix);
			}
		}

		public void Move(float x, float y, float z) {
			var rot = RotationMatrix;
			Position -= Vector3.TransformPosition(Vector3.UnitX, rot) * x;
			Position -= Vector3.TransformPosition(Vector3.UnitY, rot) * y;
			Position -= Vector3.TransformPosition(Vector3.UnitZ, rot) * z;
		}

		public void Rotate(float x, float y, float z) {
            Rotation -= Quaternion.FromAxisAngle(Vector3.UnitX, x); //Yaw
            Rotation -= Quaternion.FromAxisAngle(Vector3.UnitY, y); //Pitch
            Rotation -= Quaternion.FromAxisAngle(Vector3.UnitZ, z); //Roll
		}

		Vector2 lastMousePos = new Vector2();

		public void Update(Window game, float dt) {
            if (FreelookEnabled) {
                float amt = dt * MoveSpeed;
                if (Input.IsDown(Key.W)) Move(0f, 0f, amt);
                if (Input.IsDown(Key.S)) Move(0f, 0f, -amt);
                if (Input.IsDown(Key.A)) Move(-amt, 0, 0f);
                if (Input.IsDown(Key.D)) Move(amt, 0, 0f);
                if (Input.IsDown(Key.Q)) Move(0f, amt, 0f);
                if (Input.IsDown(Key.E)) Move(0f, -amt, 0f);

                if (!game.CursorVisible && game.Focused) {
                    //game.Title = "" + MathHelper.RadiansToDegrees(Rotation.X) + ", " + MathHelper.RadiansToDegrees(Rotation.Y) + ", " + MathHelper.RadiansToDegrees(Rotation.Z);
                    Vector2 delta = lastMousePos - new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
                    Rotate(delta.Y*RotateSpeed, delta.X*RotateSpeed, 0f); //Flipped because moving the mouse horizontally actually rotates on the Y axis, etc.
                    Mouse.SetPosition(game.Bounds.Left + game.Bounds.Width / 2, game.Bounds.Top + game.Bounds.Height / 2);
                }
                lastMousePos = new Vector2(Mouse.GetState().X, Mouse.GetState().Y);
            }
		}

		public void Begin() {
            Current = this;
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadMatrix(ref projectionMatrix);
			GL.MatrixMode(MatrixMode.Modelview);
            GL.PushMatrix();
            GL.LoadIdentity();
			var viewMatrix = ViewMatrix;
			viewMatrix.Invert();
			GL.LoadMatrix(ref viewMatrix);
            //GL.Enable(EnableCap.Normalize);
            if (UseAlphaBlend) {
                GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
                GL.Enable(EnableCap.Blend);
            }
            if (UseLighting) {
                GL.Enable(EnableCap.CullFace);
                GL.Begin(PrimitiveType.Lines);
                GL.Color3(1f, 1f, 0f);
                GL.Vertex3(Vector3.Zero);
                Vector3 lightDir = new Vector3((float)Math.Cos(Window.Time), (float)Math.Sin(Window.Time), 0f);
                //lightDir.Normalize();
                GL.Vertex3(lightDir * 100f);
                GL.End();
                GL.Enable(EnableCap.Lighting);
                GL.Enable(EnableCap.Light0);
                GL.CullFace(CullFaceMode.Front);
                GL.FrontFace(FrontFaceDirection.Ccw);

                GL.LightModel(LightModelParameter.LightModelTwoSide, 0);
                GL.Enable(EnableCap.ColorMaterial);
                GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.Diffuse);
                GL.Light(LightName.Light0, LightParameter.Diffuse, new Vector4(1f, 1f, 0f, 1f));
                GL.Light(LightName.Light0, LightParameter.Position, new Vector4(lightDir.X, lightDir.Y, lightDir.Z, 0f));
                GL.Light(LightName.Light0, LightParameter.Ambient, new Vector4(0.0f, 0.0f, 0.0f, 1f));
            }
			//GL.Ortho(-game.Width / 32.0, game.Width / 32.0, -game.Height / 32.0, game.Height / 32.0, 0.0, 4.0);
		}

        public void End() {
            GL.PopMatrix();
            if (UseAlphaBlend) GL.Disable(EnableCap.Blend);
            if (UseLighting) {
                GL.Disable(EnableCap.Lighting);
                GL.Disable(EnableCap.Light0);
                GL.Disable(EnableCap.ColorMaterial);
                GL.Disable(EnableCap.CullFace);
            }
            //GL.Disable(EnableCap.Normalize);
        }
	}
}

