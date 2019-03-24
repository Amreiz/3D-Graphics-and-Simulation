using System;
using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace Labs.Lab1
{
    public class Lab1Window : GameWindow
    {
        private int[] mVertexBufferObjectIDArray = new int[2];
        private ShaderUtility mShader;

        public Lab1Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 1 Hello, Triangle",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color4.Gold);

            GL.Enable(EnableCap.CullFace);

            /*
            float[] vertices = new float[] {-0.4f, 0.0f,
                                             0.4f, 0.0f,
                                             0.0f, 0.6f,
                                            -0.8f,-0.6f,
                                             0.0f,-0.6f,
                                             0.8f,-0.6f};
            */

            /*
            float[] vertices = new float[]
            {
                // chimney

                0.0f, 0.8f, // 0
                0.0f, 1.0f, // 1
               -0.2f, 1.0f, // 2
               -0.2f, 0.8f, // 3

               // right roof

               0.8f, 0.4f, // 4
               0.4f, 0.8f, // 5
               0.4f, 0.4f, // 6

               // middle roof
              -0.4f, 0.4f, // 7
              -0.4f, 0.8f, // 8
              -0.8f, 0.4f, // 9
               0.6f, 0.0f, // 10
               0.6f, 0.4f, // 11
               0.0f, 0.4f, // 12
               0.0f, 0.0f, // 13
              -0.4f, 0.0f, // 14
              -0.6f, 0.0f, // 15
              -0.6f, 0.4f, // 16
               0.4f, 0.0f, // 17
               0.4f,-0.4f, // 18
               0.6f,-0.4f, // 19
               0.2f, 0.0f, // 20
               0.2f,-0.4f, // 21
              -0.6f,-0.4f, // 22


            };

            int[] indices = new int[] 
            {
                // chimney
                0, 1, 2,
                0, 2, 3,

                // right roof triangle
                6, 4, 5,
                
                // middle roof triangle
                7, 6, 5,
                7, 5, 8,

                // left roof triangle
                7, 8, 9,

                // right of the window
                10, 11, 12,
                13, 10, 12,

                // left of the window
                15, 14, 7,
                15, 7, 16,

                // right of the door
                17, 18, 10,
                18, 19, 10,

                // left of the door
                22, 21, 20,
                22, 20, 15,


            };   // when drawing your stuff make sure you draw anti clockwise

            */

            float[] vertices = new float[]
            {
                0.0f, 0.8f,
                0.8f, 0.4f,
                0.6f,-0.6f,
               -0.6f,-0.6f,
               -0.8f, 0.4f,
            };

            /*
            uint[] indices = new uint[]
            {
                0,2,1,
                0,3,2,
                0,4,3,
            };
            */

            uint[] indices = new uint[]
            {
                0,4,3,0,3,2,0,2,1
                
            };

            //L1T2 Enabled Back Face Culling and Fixed Triangle Winding.
            //L1T3 Drew a square by adding additional vertices to the vertices array, and adjusting the DrawArrays call.
            //L1T5 Drew a TriForce Symbol by editing the vertices array and adjusting the DrawArrays call. Also modified the fragment shader to colour all fragments yellow.
            //L1T6 Converted TriForce Symbol to use element array buffers.
            //L1T7 Drew a house by using 0.2f as the base coordinate
            //L1T8 Drew a pentagon using DrawPrimitives.Triangles
            //L1T9 Drew a pentagon using DrawPrimitives.TriangleFan using 45 % fewer indices!
            //L1T10 Drew a pentagon using DrawPrimitive.TriangleStrip

            GL.GenBuffers(2, mVertexBufferObjectIDArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw); // second parameter is the number of bytes we want to copy on the graphics card

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);

            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVertexBufferObjectIDArray[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Length * sizeof(uint)),
            indices, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out
            size);

            if (indices.Length * sizeof(uint) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            #region Shader Loading Code - Can be ignored for now

            mShader = new ShaderUtility( @"Lab1/Shaders/vSimple.vert", @"Lab1/Shaders/fSimple.frag");

            #endregion

            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVertexBufferObjectIDArray[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVertexBufferObjectIDArray[1]);

            // shader linking goes here
            #region Shader linking code - can be ignored for now

            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 2, VertexAttribPointerType.Float, false, 2 * sizeof(float), 0);

            #endregion

            //L1T1 Changed the clear colour and drew my first triangle!

            GL.DrawElements(PrimitiveType.TriangleStrip, 9, DrawElementsType.UnsignedInt, 0);

            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            GL.DeleteBuffers(2, mVertexBufferObjectIDArray);
            GL.UseProgram(0);
            mShader.Delete();
        }
    }
}
