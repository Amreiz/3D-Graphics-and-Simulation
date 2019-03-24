using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;

namespace Labs.Lab2
{
    class Lab2_1Window : GameWindow
    {        
        private int[] mTriangleVertexBufferObjectIDArray = new int[2];
        private int[] mSquareVertexBufferObjectIDArray = new int[2];
        private int[] mVertexArrayObjectIDs = new int[2];
        private ShaderUtility mShader;

        public Lab2_1Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 2_1 Linking to Shaders and VAOs",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        // L21T1 Rendered a big black triangle!
        // L21T2 Set uniform variable in the fragment shader to colour all fragments red

        protected override void OnLoad(EventArgs e)
        {
            GL.ClearColor(Color4.CadetBlue);
            GL.Enable(EnableCap.DepthTest);

            float[] triangleVertices = new float[] 
            {
                -0.8f, 0.8f, 0.4f, 0.644f, 0.51f, 0.8f,
                -0.6f, -0.4f, 0.4f, 0.284f, 0.22f, 0.4f,
                 0.2f, 0.2f, 0.4f, 0.934f, 0.99f, 1.0f
            };

            uint[] triangleIndices = new uint[] 
            {
                0, 1, 2
            };

            float[] squareVertices = new float[] 
            {
                -0.2f, -0.4f, 0.2f, 0.378f, 1.0f, 0.8f,
                 0.8f, -0.4f, 0.2f, 0.642f, 0.45f, 0.2f,
                 0.8f, 0.6f, 0.2f, 0.198f, 0.7f, 0.3f,
                -0.2f, 0.6f, 0.2f, 0.843f, 0.9f, 0.345f
            };


            uint[] squareIndices = new uint[]
            {
                0,1,2,3
            };

            #region Shader Loading Code

            mShader = new ShaderUtility(@"Lab2/Shaders/vLab21.vert", @"Lab2/Shaders/fSimple.frag");
            GL.UseProgram(mShader.ShaderProgramID);
            #endregion

            int vColourLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vColour");
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            GL.EnableVertexAttribArray(vColourLocation);
            GL.EnableVertexAttribArray(vPositionLocation);

            GL.GenBuffers(2, mTriangleVertexBufferObjectIDArray);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mTriangleVertexBufferObjectIDArray[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(triangleVertices.Length * sizeof(float)), triangleVertices, BufferUsageHint.StaticDraw);

            int triangleSize;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out triangleSize);

            if (triangleVertices.Length * sizeof(float) != triangleSize)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mTriangleVertexBufferObjectIDArray[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(triangleIndices.Length * sizeof(int)), triangleIndices, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out triangleSize);

            if (triangleIndices.Length * sizeof(int) != triangleSize)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.GenVertexArrays(2, mVertexArrayObjectIDs);
            GL.BindVertexArray(mVertexArrayObjectIDs[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mTriangleVertexBufferObjectIDArray[0]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mTriangleVertexBufferObjectIDArray[1]);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.VertexAttribPointer(vColourLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(vColourLocation);
            GL.EnableVertexAttribArray(vPositionLocation);

            // L21T3 Added a blue square by creating new data and element arrays, linking the appropriate variables to the shader and changing the fragment shaders uniform colour variable
            // L21T5 Added depth by adding an extra vertex parameter, changing the shader variable and linking and enabling depth testing#
            // L21T6 Added per vertex colour to the vertex shader, passed it to the fragment shader and linked the data buffer to the shader program
            // L21T7 Changed vertex colours to see fragments colour values being blended together
            // L21T8 Made state changes easier using Vertex Array Objects
            // L21T9 

            int squareSize;    
            GL.GenBuffers(2, mSquareVertexBufferObjectIDArray);
            GL.BindVertexArray(mVertexArrayObjectIDs[1]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mSquareVertexBufferObjectIDArray[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(squareVertices.Length * sizeof(float)), squareVertices, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out squareSize);

            if(squareVertices.Length * sizeof(float) != squareSize)
            {
                throw new ApplicationException("Vertex data not loaded onto the graphics card correctly");
            }

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mSquareVertexBufferObjectIDArray[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(squareIndices.Length * sizeof(int)), squareIndices, BufferUsageHint.StaticDraw);
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out squareSize);

            if(squareIndices.Length * sizeof(int) != squareSize)
            {
                throw new ApplicationException("Index data not loaded onto the graphics card correctly");
            }

            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.VertexAttribPointer(vColourLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));
            GL.EnableVertexAttribArray(vColourLocation);
            GL.EnableVertexAttribArray(vPositionLocation);

            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            {
                base.OnRenderFrame(e);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.BindVertexArray(mVertexArrayObjectIDs[1]);
                GL.DrawElements(PrimitiveType.TriangleFan, 4, DrawElementsType.UnsignedInt, 0);

                GL.BindVertexArray(mVertexArrayObjectIDs[0]);
                GL.DrawElements(PrimitiveType.Triangles, 3, DrawElementsType.UnsignedInt, 0);

                GL.BindVertexArray(0);
                this.SwapBuffers();

            }
        }

        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            GL.BindVertexArray(0);
            GL.DeleteVertexArrays(2, mVertexArrayObjectIDs);
            GL.UseProgram(0);
            mShader.Delete();
        }
    }
}
