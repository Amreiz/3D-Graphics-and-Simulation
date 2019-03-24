using System;
using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace ACW
{

    public class ACWWindow : GameWindow
    {
        public ACWWindow() : base(800, 600, GraphicsMode.Default, "Basic 3D Coursework",
                GameWindowFlags.Default, DisplayDevice.Default, 3, 3, GraphicsContextFlags.ForwardCompatible)
        {
        }

        private enum Mat
        {
            whitePlastic,
            blackRubber,
            jade,
            chrome,
            mirror
        }

        private enum CamType
        {
            topDown,
            FPS
        }

        private const float ToRadians = (float)(Math.PI / 180);
        private int[] mVBO_IDs = new int[9];
        private int[] mVAO_IDs = new int[5];
        private ShaderUtility mShader;
        private ModelUtility mCreatureModelUtility, mCylinderModelUtility;
        private CamType currentCamType;

        //parameters to feed into shader
        private Matrix4 mCreatureModel, mGroundModel, mCylinderModel, mWallModel, mWallModel2, mWallModel3, mWallModel4, mCubeModel, mCamera1, mCamera2; // local positions of individual instances of model
        private Matrix4 mView, mCreature, mGround, mCylinder, mWall, mProjection, mCube; // worldSpace positions used temporarily
        private float cameraSpeed, shininess;
        private Vector4 relativeLightPos, lightPos, lightPos2, lightPos3;
        private Vector3 ambientLight, diffuseLight, specularLight;
        private Vector3 ambientRef, diffuseRef, specularRef;

        //shader location variables
        int vPositionLocation, vNormalLocation, uModelLocation, uViewLocation, uProjectionLocation, uEyePositionLocation;
        int uLightPositionLocation, uAmbientLightLocation, uDiffuseLightLocation, uSpecularLightLocation;
        int uLightPositionLocation2, uAmbientLightLocation2, uDiffuseLightLocation2, uSpecularLightLocation2;
        int uLightPositionLocation3, uAmbientLightLocation3, uDiffuseLightLocation3, uSpecularLightLocation3;
        int uShininessLocation, uAmbientReflectivityLocation, uDiffuseReflectivityLocation, uSpecularReflectivityLocation;

        //textures
        int textureID, vTexCoords;

        protected override void OnLoad(EventArgs e)
        {
            // Set some GL state
            GL.ClearColor(Color4.White);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            cameraSpeed = 0.1f;

            #region  SHADER LOADING

            mShader = new ShaderUtility(@"ACW/Shaders/vertexShader.vert", @"ACW/Shaders/fragShaderMultiLight.frag");
            GL.UseProgram(mShader.ShaderProgramID);
            vNormalLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal");
            vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            vTexCoords = GL.GetAttribLocation(mShader.ShaderProgramID, "vTexCoords");

            //transformation matrices
            uModelLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            uViewLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
            uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");

            uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].Position");
            uAmbientLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].AmbientLight");
            uDiffuseLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].DiffuseLight");
            uSpecularLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[0].SpecularLight");

            uLightPositionLocation2 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].Position");
            uAmbientLightLocation2 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].AmbientLight");
            uDiffuseLightLocation2 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].DiffuseLight");
            uSpecularLightLocation2 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[1].SpecularLight");

            uLightPositionLocation3 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].Position");
            uAmbientLightLocation3 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].AmbientLight");
            uDiffuseLightLocation3 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].DiffuseLight");
            uSpecularLightLocation3 = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight[2].SpecularLight");

            //material properties
            uAmbientReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.AmbientReflectivity");
            uDiffuseReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.DiffuseReflectivity");
            uSpecularReflectivityLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.SpecularReflectivity");
            uShininessLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.Shininess");

            #endregion

            #region TEXTURE LOADING

            string textureFilepath = @"uvChecker.jpg";

            Bitmap TextureBitmap = new Bitmap(textureFilepath);

            TextureBitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);

            BitmapData TextureData = TextureBitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, TextureBitmap.Width,
                TextureBitmap.Height), ImageLockMode.ReadOnly,
                System.Drawing.Imaging.PixelFormat.Format32bppRgb);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.GenTextures(1, out textureID);
            GL.BindTexture(TextureTarget.Texture2D, textureID);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, TextureData.Width, TextureData.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, TextureData.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            TextureBitmap.UnlockBits(TextureData);


            int uTextureSamplerLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uTextureSampler");
            GL.Uniform1(uTextureSamplerLocation, 0);



            #endregion

            #region OBJECT LOADING

            GL.GenVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);

            #region PLANE LOADING

            float[] vertices = new float[] {-10, 0, -10,0,1,0,
                                             -10, 0, 10,0,1,0,
                                             10, 0, 10,0,1,0,
                                             10, 0, -10,0,1,0,};

            GL.BindVertexArray(mVAO_IDs[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            //--checking load
            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * sizeof(float) != size)
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");

            //--assigning values
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            #endregion

            #region CREATURE LOADING

            mCreatureModelUtility = ModelUtility.LoadModel(@"Utility/Models/Model.bin");

            GL.BindVertexArray(mVAO_IDs[1]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[1]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mCreatureModelUtility.Vertices.Length * sizeof(float)), mCreatureModelUtility.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[2]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mCreatureModelUtility.Indices.Length * sizeof(float)), mCreatureModelUtility.Indices, BufferUsageHint.StaticDraw);

            //--checking load
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCreatureModelUtility.Vertices.Length * sizeof(float) != size)
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCreatureModelUtility.Indices.Length * sizeof(float) != size)
                throw new ApplicationException("Index data not loaded onto graphics card correctly");

            //--assigning values
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            #endregion

            #region CYLINDER LOADING

            mCylinderModelUtility = ModelUtility.LoadModel(@"Utility/Models/cylinder.bin");

            GL.BindVertexArray(mVAO_IDs[2]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[3]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mCylinderModelUtility.Vertices.Length * sizeof(float)), mCylinderModelUtility.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[4]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mCylinderModelUtility.Indices.Length * sizeof(float)), mCylinderModelUtility.Indices, BufferUsageHint.StaticDraw);

            //--checking load
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCylinderModelUtility.Vertices.Length * sizeof(float) != size)
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCylinderModelUtility.Indices.Length * sizeof(float) != size)
                throw new ApplicationException("Index data not loaded onto graphics card correctly");

            //--assigning values
            GL.EnableVertexAttribArray(vPositionLocation);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            #endregion

            #region WALL
            float[] wallVertices = {-0.5f, -0.5f, -0.5f, -0.5f,
                                -0.25f, -0.5f, -0.25f, -0.5f,
                                0.0f, -0.5f, 0.0f, -0.5f,
                                0.25f, -0.5f, 0.25f, -0.5f,
                                0.5f, -0.5f, 0.5f, -0.5f,
                                -0.5f, 0.0f, -0.5f, 0.0f,
                                -0.25f, 0.0f,-0.25f, 0.0f,
                                0.0f, 0.0f, 0.0f, 0.0f,
                                0.25f, 0.0f, 0.25f, 0.0f,
                                0.5f, 0.0f, 0.5f, 0.0f,
                               -0.5f, 0.5f, -0.5f, 0.5f,
                                -0.25f, 0.5f,-0.25f, 0.5f,
                                0.0f, 0.5f, 0.0f, 0.5f,
                                0.25f, 0.5f, 0.25f, 0.5f,
                                0.5f, 0.5f, 0.5f, 0.5f,
                                };

            uint[] wallIndices = { 5, 0, 1,
                               5, 1, 6,
                               6, 1, 2,
                               6, 2, 7,
                               7, 2, 3,
                               7, 3, 8,
                               8, 3, 4,
                               8, 4, 9,
                               10, 5, 6,
                               10, 6, 11,
                               11, 6, 7,
                               11, 7, 12,
                               12, 7, 8,
                               12, 8, 13,
                               13, 8, 9,
                               13, 9, 14
                             };

            GL.BindVertexArray(mVAO_IDs[3]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[5]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(wallVertices.Length * sizeof(float)), wallVertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[6]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(wallIndices.Length * sizeof(uint)), wallIndices, BufferUsageHint.StaticDraw);
            
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (wallVertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (wallIndices.Length * sizeof(uint) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 0);

            GL.EnableVertexAttribArray(vTexCoords);
            GL.VertexAttribPointer(vTexCoords, 2, VertexAttribPointerType.Float, false, 4 * sizeof(float), 2 * sizeof(float));

            #endregion

            #region CUBE LOADING
            
            float[] cubeVertices = new float[] { -1.0f, -1.0f,  1.0f,//0
                                                  1.0f, -1.0f,  1.0f,//1
                                                  1.0f,  1.0f,  1.0f,//2
                                                 -1.0f,  1.0f,  1.0f,//3
                                                  1.0f, -1.0f, -1.0f,//4
                                                  1.0f,  1.0f, -1.0f,//5
                                                 -1.0f, -1.0f, -1.0f,//6
                                                 -1.0f,  1.0f, -1.0f,//7
                                                };

            int[] cubeIndices = {   0, 1, 2, 0, 2, 3, //front
                                    1, 4, 5, 1, 5, 2, //right
                                    4, 6, 7, 4, 7, 5, //back
                                    6, 0, 3, 6, 3, 7, //left
                                    3, 2, 5, 3, 5, 7, //upper
                                    6, 4, 1, 6, 1, 0}; //bottom

            GL.BindVertexArray(mVAO_IDs[4]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[7]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(cubeVertices.Length * sizeof(float)), cubeVertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[8]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(cubeIndices.Length * sizeof(uint)), cubeIndices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (cubeVertices.Length * sizeof(float) != size)
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (cubeIndices.Length * sizeof(uint) != size)
                throw new ApplicationException("Index data not loaded onto graphics card correctly");

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            #endregion

            GL.BindVertexArray(0);

            #endregion

            #region POSITIONING

            mCamera1 = Matrix4.CreateTranslation(0, -1.5f, -5);
            mCamera2 = Matrix4.CreateTranslation(0, 0, -20); 
            mCamera2 = RollModel(90 * ToRadians, mCamera2);

            currentCamType = CamType.FPS;
            mView = mCamera1;
            GL.UniformMatrix4(uViewLocation, true, ref mView);

            mGroundModel = Matrix4.CreateTranslation(0, 0, 0);

            mCreatureModel = Matrix4.CreateTranslation(0, 2, 0);
            mCreatureModel = RotateModel(-90 * ToRadians, mCreatureModel);

            mCylinderModel = Matrix4.CreateTranslation(0, 0, 0);

            mWallModel = Matrix4.CreateTranslation(0, 5, -10);
            mWallModel = ScaleModel(20f,10f,20f, mWallModel);

            mWallModel2 = Matrix4.CreateTranslation(10, 5, 0);
            mWallModel2 = ScaleModel(20f,10f,20f, mWallModel2);
            mWallModel2 = RotateModel(-90 * ToRadians, mWallModel2);

            mWallModel3 = Matrix4.CreateTranslation(-10, 5, 0);
            mWallModel3 = ScaleModel(20f,10f,20f, mWallModel3);
            mWallModel3 = RotateModel(90 * ToRadians, mWallModel3);
            
            mWallModel4 = Matrix4.CreateTranslation(0, 5, 10);
            mWallModel4 = ScaleModel(20f,10f,20f, mWallModel4);
            mWallModel4 = RotateModel(180 * ToRadians, mWallModel4);

            mCubeModel = Matrix4.CreateTranslation(3, 4, 0);
            mCubeModel = RollModel(45 * ToRadians, mCubeModel);
            mCubeModel = RotateModel(45 * ToRadians, mCubeModel);


            EyePositionUpdate();

            #endregion

            #region LIGHTING and MATERIALS

            lightPos = new Vector4(1, 2, 1, 1);
            lightPos2 = new Vector4(0, 2, 0, 1);
            lightPos3 = new Vector4(-1, 2, 1, 1);
            LightPositionUpdate();

            ambientLight = new Vector3(1.5f, 0.0f, 0.0f);
            diffuseLight = new Vector3(1.8f, 0.0f, 0.0f);
            specularLight = new Vector3(1.4f, 0.0f, 0.0f);
            SetLightParameters(1, ambientLight, diffuseLight, specularLight);

            ambientLight = new Vector3(0.0f, 1.5f, 0.0f);
            diffuseLight = new Vector3(0.0f, 1.8f, 0.0f);
            specularLight = new Vector3(0.0f, 1.4f, 0.0f);
            SetLightParameters(2, ambientLight, diffuseLight, specularLight);

            ambientLight = new Vector3(0.0f, 0.0f, 1.5f);
            diffuseLight = new Vector3(0.0f, 0.0f, 1.8f);
            specularLight = new Vector3(0.0f, 0.0f, 1.4f);
            SetLightParameters(3, ambientLight, diffuseLight, specularLight);

            ChangeMaterial(Mat.whitePlastic);

            #endregion
            
            base.OnLoad(e);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //draw floor
            ChangeMaterial(Mat.blackRubber);
            mGround = mGroundModel;
            GL.UniformMatrix4(uModelLocation, true, ref mGround);
            GL.BindVertexArray(mVAO_IDs[0]);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);
            
            //draw cylinder
            mCylinder = mCylinderModel * mGroundModel;
            GL.UniformMatrix4(uModelLocation, true, ref mCylinder);
            GL.BindVertexArray(mVAO_IDs[2]);
            GL.DrawElements(PrimitiveType.Triangles, mCylinderModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            //draw creature
            ChangeMaterial(Mat.whitePlastic);
            mCreature = mCreatureModel * mGroundModel;
            GL.UniformMatrix4(uModelLocation, true, ref mCreature);
            GL.BindVertexArray(mVAO_IDs[1]);
            GL.DrawElements(PrimitiveType.Triangles, mCreatureModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);
            
            ChangeMaterial(Mat.chrome);

            //back wall
            mWall = mWallModel * mGroundModel;
            GL.UniformMatrix4(uModelLocation, true, ref mWall);
            GL.BindVertexArray(mVAO_IDs[3]);
            GL.DrawElements(PrimitiveType.Triangles, 48, DrawElementsType.UnsignedInt, 0);
            
            //right wall
            mWall = mWallModel2 * mGroundModel;
            GL.UniformMatrix4(uModelLocation, true, ref mWall);
            GL.BindVertexArray(mVAO_IDs[3]);
            GL.DrawElements(PrimitiveType.Triangles, 48, DrawElementsType.UnsignedInt, 0);

            //left wall
            mWall = mWallModel3 * mGroundModel;
            GL.UniformMatrix4(uModelLocation, true, ref mWall);
            GL.BindVertexArray(mVAO_IDs[3]);
            GL.DrawElements(PrimitiveType.Triangles, 48, DrawElementsType.UnsignedInt, 0);

            //fourth wall
            mWall = mWallModel4  * mGroundModel;
            GL.UniformMatrix4(uModelLocation, true, ref mWall);
            GL.BindVertexArray(mVAO_IDs[3]);
            GL.DrawElements(PrimitiveType.Triangles, 48, DrawElementsType.UnsignedInt, 0);

            //cube 
            ChangeMaterial(Mat.mirror);
            
            mCubeModel = RollModel(3 * ToRadians, mCubeModel);
            mCubeModel = RotateModel(1 * ToRadians, mCubeModel);

            mCube = mCubeModel * mCreature;
            GL.UniformMatrix4(uModelLocation, true, ref mCube);
            GL.BindVertexArray(mVAO_IDs[4]);
            GL.DrawElements(PrimitiveType.Triangles, 36, DrawElementsType.UnsignedInt, 0);

            GL.BindVertexArray(0);
            this.SwapBuffers();
        }

        protected override void OnUnload(EventArgs e)
        {
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.DeleteBuffers(mVBO_IDs.Length, mVBO_IDs);
            GL.DeleteVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            mShader.Delete();
            base.OnUnload(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(this.ClientRectangle);
            if (mShader != null)
            {
                mProjection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 25);
                GL.UniformMatrix4(uProjectionLocation, true, ref mProjection);
            }
        }

        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);
            switch (e.KeyChar)
            {
                case 'a':
                    RotateCamera(-cameraSpeed, "Y");
                    break;

                case 'd':
                    RotateCamera(cameraSpeed, "Y");
                    break;

                case 'w':
                    MoveCamera(0, 0, cameraSpeed);
                    break;

                case 's':
                    MoveCamera(0, 0, -cameraSpeed);
                    break;

                case 'q':
                    RotateCamera(cameraSpeed, "Z");
                    break;

                case 'e':
                    RotateCamera(-cameraSpeed, "Z");
                    break;

                case 'z':
                    OrbitCamera(cameraSpeed);
                    break;

                case 'x':
                    OrbitCamera(-cameraSpeed);
                    break;

                case 'c':
                    mCreatureModel = RotateModel(cameraSpeed, mCreatureModel);
                    break;

                case 'v':
                    mCreatureModel = RotateModel(-cameraSpeed, mCreatureModel);
                    break;

                case 'p':
                    CameraChange();
                    break;
            }
        }

        private void MoveCamera(float x, float y, float z)
        {
            //updates model transforms
            mView = mView * Matrix4.CreateTranslation(x, y, z);
            GL.UniformMatrix4(uViewLocation, true, ref mView);

            LightPositionUpdate();
            EyePositionUpdate();
        }

        Matrix4 RotateModel(float value, Matrix4 mModel)
        {
            Vector3 t = mModel.ExtractTranslation();
            Matrix4 translation = Matrix4.CreateTranslation(t);
            Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
            return mModel * inverseTranslation * Matrix4.CreateRotationY(value) * translation;
        }

        Matrix4 RollModel(float value, Matrix4 mModel)
        {
            Vector3 t = mModel.ExtractTranslation();
            Matrix4 translation = Matrix4.CreateTranslation(t);
            Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
            return mModel * inverseTranslation * Matrix4.CreateRotationX(value) * translation;
        }

        Matrix4 ScaleModel(float value, Matrix4 mModel)
        {
            Vector3 t = mModel.ExtractTranslation();
            Matrix4 translation = Matrix4.CreateTranslation(t);
            Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
            return mModel * inverseTranslation * Matrix4.CreateScale(value) * translation;
        }

        Matrix4 ScaleModel(float x, float y, float z, Matrix4 mModel)
        {
            Vector3 t = mModel.ExtractTranslation();
            Matrix4 translation = Matrix4.CreateTranslation(t);
            Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
            return mModel * inverseTranslation * Matrix4.CreateScale(x,y,z) * translation;
        }

        private void OrbitCamera(float value)
        {
            //moves to centre, rotates, then back

            Vector3 t = mView.ExtractTranslation();
            //Quaternion r = mView.ExtractRotation();
            Matrix4 translation = Matrix4.CreateTranslation(t);
            Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
            mView = mView * inverseTranslation * Matrix4.CreateRotationY(value) * translation;
            GL.UniformMatrix4(uViewLocation, true, ref mView);

            LightPositionUpdate();
            EyePositionUpdate();
        }

        private void CameraChange()
        {
            if(currentCamType == CamType.FPS)
            {
                currentCamType = CamType.topDown;
                mView = mCamera2;
            }else if(currentCamType == CamType.topDown)
            {
                currentCamType = CamType.FPS;
                mView = mCamera1;
            }

            GL.UniformMatrix4(uViewLocation, true, ref mView);
            LightPositionUpdate();
            EyePositionUpdate();
        }

        private void RotateCamera(float value, string rotType)
        {
            //updates model transforms
            switch (rotType)
            {
                case "X":
                    mView = mView * Matrix4.CreateRotationX(value);
                    break;

                case "Y":
                    mView = mView * Matrix4.CreateRotationY(value);
                    break;

                case "Z":
                    mView = mView * Matrix4.CreateRotationX(value);
                    break;
            }
            GL.UniformMatrix4(uViewLocation, true, ref mView);

            LightPositionUpdate();
            EyePositionUpdate();
        }
        void EyePositionUpdate()
        {   //calculates camera position for light shader
            Vector3 invEyePosTranslation = mView.ExtractTranslation();
            Quaternion invEyePosRotation = mView.ExtractRotation();
            Vector3 invEyePos = Vector3.Transform(invEyePosTranslation, invEyePosRotation);
            Vector4 eyePosition = new Vector4(-invEyePos, 1);
            GL.Uniform4(uEyePositionLocation, eyePosition);
        }

        void LightPositionUpdate()
        {
            relativeLightPos = Vector4.Transform(lightPos, mView);
            GL.Uniform4(uLightPositionLocation, relativeLightPos);

            relativeLightPos = Vector4.Transform(lightPos2, mView);
            GL.Uniform4(uLightPositionLocation2, relativeLightPos);

            relativeLightPos = Vector4.Transform(lightPos3, mView);
            GL.Uniform4(uLightPositionLocation3, relativeLightPos);
        }

        void SetLightParameters(int i, Vector3 amb, Vector3 diff, Vector3 spec)
        {
            switch (i)
            {
                case 1:
                    GL.Uniform3(uAmbientLightLocation, amb);
                    GL.Uniform3(uDiffuseLightLocation, diff);
                    GL.Uniform3(uSpecularLightLocation, spec);
                    break;
                case 2:
                    GL.Uniform3(uAmbientLightLocation2, amb);
                    GL.Uniform3(uDiffuseLightLocation2, diff);
                    GL.Uniform3(uSpecularLightLocation2, spec);
                    break;
                case 3:
                    GL.Uniform3(uAmbientLightLocation3, amb);
                    GL.Uniform3(uDiffuseLightLocation3, diff);
                    GL.Uniform3(uSpecularLightLocation3, spec);
                    break;
            }
        }

        void ChangeMaterial(Mat material)
        {
            switch (material)
            {
                case Mat.whitePlastic:
                    ambientRef = new Vector3(0.2f, 0.2f, 0.2f);
                    diffuseRef = new Vector3(0.55f, 0.55f, 0.55f);
                    specularRef = new Vector3(0.7f, 0.7f, 0.7f);
                    shininess = 0.6f;
                    break;

                case Mat.blackRubber:
                    ambientRef = new Vector3(0.02f, 0.02f, 0.02f);
                    diffuseRef = new Vector3(0.01f, 0.01f, 0.01f);
                    specularRef = new Vector3(0.4f, 0.4f, 0.4f);
                    shininess = 0.6f;
                    break;

                case Mat.jade:
                    ambientRef = new Vector3(0.0215f, 0.1745f, 0.0215f);
                    diffuseRef = new Vector3(0.07568f, 0.61424f, 0.07568f);
                    specularRef = new Vector3(0.633f, 0.727811f, 0.633f);
                    shininess = 0.6f;
                    break;

                case Mat.chrome:
                    ambientRef = new Vector3(0.25f, 0.25f, 0.25f);
                    diffuseRef = new Vector3(0.4f, 0.4f, 0.4f);
                    specularRef = new Vector3(0.775f, 0.775f, 0.775f);
                    shininess = 0.6f;
                    break;

                case Mat.mirror:
                    ambientRef = new Vector3(1f, 1f, 1f);
                    diffuseRef = new Vector3(1f, 1f, 1f);
                    specularRef = new Vector3(1f, 1f, 1f);
                    shininess = 1f;
                    break;
            }

            GL.Uniform3(uAmbientReflectivityLocation, ambientRef);
            GL.Uniform3(uDiffuseReflectivityLocation, diffuseRef);
            GL.Uniform3(uSpecularReflectivityLocation, specularRef);
            GL.Uniform1(uShininessLocation, shininess);
        }
    }
}
