using OpenTK;
using System;
using OpenTK.Graphics;
using Labs.Utility;
using OpenTK.Graphics.OpenGL;

namespace Labs.Lab2
{
    //L22T1 Rendered a small green square to start the camera lab
    //L22T2 Translated the square one unit to the right
    //L22T3 Reused a model to render two squares by changing the model matrix
    //L22T4 Two squares translated and rotated
    //L22T5 Set up vertex shader to use view matrix and set view matrix in new OnKeyPress function
    //L22T6 Refactors camera code and eliminated magic numbers
    //L22T7 Added a projection matrix to the shader to increase the size of the viewing volume
    //L22T8 Can change the viewport on resize, but the aspect ratio still isn’t right
    //L22T9 Created a perspective camera
    //L22T10 Created a user controlled perspective camera
    public class Lab2_2Window : GameWindow
    {
        public Lab2_2Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 2_2 Understanding the Camera",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        private int[] mVBO_IDs = new int[2];
        private int mVAO_ID;
        private ShaderUtility mShader;
        private ModelUtility mModel;
        private Matrix4 mView;

        protected override void OnLoad(EventArgs e)
        {
            // Set some GL state
            GL.ClearColor(Color4.DodgerBlue);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            mModel = ModelUtility.LoadModel(@"Utility/Models/lab22model.sjg");    
            mShader = new ShaderUtility(@"Lab2/Shaders/vLab22.vert", @"Lab2/Shaders/fSimple.frag");
            mView = Matrix4.CreateTranslation(0, 0, -2);

            //Vector3 eye = new Vector3(1.2f, 2.0f, -5.0f);
            //Vector3 lookAt = new Vector3(0, 0, 0);
            //Vector3 up = new Vector3(0, 1, 0);
            //mView = Matrix4.LookAt(eye, lookAt, up);

            GL.UseProgram(mShader.ShaderProgramID);
            int vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            int vColourLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vColour");

            int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 5);
            GL.UniformMatrix4(uProjectionLocation, true, ref projection);

            mVAO_ID = GL.GenVertexArray();
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);
            
            GL.BindVertexArray(mVAO_ID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mModel.Vertices.Length * sizeof(float)), mModel.Vertices, BufferUsageHint.StaticDraw);           
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[1]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mModel.Indices.Length * sizeof(float)), mModel.Indices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mModel.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mModel.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.EnableVertexAttribArray(vColourLocation);
            GL.VertexAttribPointer(vColourLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            GL.BindVertexArray(0);

            base.OnLoad(e);
            
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            
            GL.BindVertexArray(mVAO_ID);

            int uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            int uProjection = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");

            Matrix4 m1 = Matrix4.CreateRotationZ(0.8f) * Matrix4.CreateTranslation(0.5f, 0, 0);
            GL.UniformMatrix4(uModelLocation, true, ref m1);
            GL.UniformMatrix4(uView, true, ref mView);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            Matrix4 m2 = Matrix4.CreateRotationZ(0.8f) * Matrix4.CreateTranslation(-0.5f, 0.5f, 0f);
            GL.UniformMatrix4(uModelLocation, true, ref m2);

            GL.DrawElements(BeginMode.Triangles, mModel.Indices.Length, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            this.SwapBuffers();
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            float cameraSpeed = 0.01f;
            int cameraStationary = 0;

            if (e.KeyChar == 'a')
            {
                mView = mView * Matrix4.CreateTranslation(cameraSpeed, cameraStationary, cameraStationary);
                MoveCamera();
            }

            if (e.KeyChar == 'd')
            {
                mView = mView * Matrix4.CreateTranslation(-cameraSpeed, cameraStationary, cameraStationary);
                MoveCamera();
            }

            if (e.KeyChar == 'w')
            {
                mView = mView * Matrix4.CreateTranslation(cameraStationary, -cameraSpeed, cameraStationary);
                MoveCamera();
            }

            if (e.KeyChar == 's')
            {
                mView = mView * Matrix4.CreateTranslation(cameraStationary, cameraSpeed, cameraStationary);
                MoveCamera();
            }

            if (e.KeyChar == 'z')
            {
                mView = mView * Matrix4.CreateTranslation(cameraStationary, cameraStationary, cameraSpeed);
                MoveCamera();
            }

            if (e.KeyChar == 'x')
            {
                mView = mView * Matrix4.CreateTranslation(cameraStationary, cameraStationary, -cameraSpeed);
                MoveCamera();
            }
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (mShader != null)
            {
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                int windowHeight = this.ClientRectangle.Height;
                int windowWidth = this.ClientRectangle.Width;

                if (windowHeight > windowWidth)
                {
                    if (windowWidth < 1) { windowWidth = 1; }
                    float ratio = windowHeight / windowWidth;
                    Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 5);
                    GL.UniformMatrix4(uProjectionLocation, true, ref projection);
                }

                else
                {
                    if (windowHeight < 1) { windowHeight = 1; }
                    float ratio = windowWidth / windowHeight;
                    Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 5);
                    GL.UniformMatrix4(uProjectionLocation, true, ref projection);
                }
            }

            GL.Viewport(this.ClientRectangle);
        }

        private int MoveCamera()
        {
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);
            return uView;
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffers(mVBO_IDs.Length, mVBO_IDs);
            GL.DeleteVertexArray(mVAO_ID);
            mShader.Delete();
            base.OnUnload(e);
        }
    }
}
