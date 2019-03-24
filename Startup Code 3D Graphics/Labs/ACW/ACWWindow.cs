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
        public ACWWindow() : base(800, 600, GraphicsMode.Default, "3D Coursework",
                GameWindowFlags.Default, DisplayDevice.Default, 3, 3, GraphicsContextFlags.ForwardCompatible)
        {
        }

        // Enum to define materials needed.

        private enum Material
        {
            yellowRubber,
            gold,
            brass,
            chrome,
            emerald
        }

        // VBO's and VAO's intialised assigned to the amount need to generate buffers on the graphics card

        private int[] mVBO_IDs = new int[8];
        private int[] mVAO_IDs = new int[4];

        // Shader and Model Utilitys for loading in our models, Matrices to define position transformation, vectors for lighting
        private ShaderUtility mShader;
        private ModelUtility mArmadilloModelUtility, mCylinderModelUtility;
        private Matrix4 mView, mArmadilloModel, mGroundModel, mCylinderModel, mCubeModel;
        private Vector3 specularLight, diffuseLight, ambientLight, ambientReferance, diffuseReferance, specularReferance;

        // variables to determine the vertex locations
        private float shininess;
        int vPositionLocation, vNormalLocation, vAmbientLocation, vSpecularLocation, vDiffuseLocation, vShineLocation;
        int uEyePositionLocation, uSpecularLightLocation, uDiffuseLightLocation, uAmbientLightLocation;
        private int u2SpecularLightLocation;
        private int u2DiffuseLightLocation;
        private int u2AmbientLightLocation;

        // vertices that draw the floor
        #region VERTICES
        float[] floorVertices = new float[] 
                                            {
                                             -10, 0,-10,0,1,0,  // Vertex 0
                                             -10, 0, 10,0,1,0,  // Vertex 1
                                              10, 0, 10,0,1,0,  // Vertex 2
                                              10, 0,-10,0,1,0,  // Vertex 3
                                            };


        float[] cubeVertices = new float[] 
                                            {
                                                 -2.0f, -2.0f,  2.0f,   // Vertex 0
                                                  2.0f, -2.0f,  2.0f,   // Vertex 1
                                                  2.0f,  2.0f,  2.0f,   // Vertex 2
                                                 -2.0f,  2.0f,  2.0f,   // Vertex 3
                                                  2.0f, -2.0f, -2.0f,   // Vertex 4
                                                  2.0f,  2.0f, -2.0f,   // Vertex 5
                                                 -2.0f, -2.0f, -2.0f,   // Vertex 6
                                                 -2.0f,  2.0f, -2.0f,   // Vertex 7
                                            };

        uint[] cubeIndices = new uint[]
                                        {
                                            0, 1, 2, 0, 2, 3, // frontface of cube
                                            4, 6, 7, 4, 7, 5, // backface of cube
                                            1, 4, 5, 1, 5, 2, // rightface of cube
                                            6, 0, 3, 6, 3, 7, // leftface of cube
                                            3, 2, 5, 3, 5, 7, // topface of cube
                                            6, 4, 1, 6, 1, 0  // bottomface of cube
                                        };
        #endregion
        protected override void OnLoad(EventArgs e)
        {
            // Set base GL states, background colour, depth test, culling to draw anti-clockwise
            GL.ClearColor(Color4.Sienna);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            // Links the vertex and fragment shaders to mShader 
            mShader = new ShaderUtility(@"ACW/Shaders/vPassThrough.vert", @"ACW/Shaders/vLighting.frag");
            GL.UseProgram(mShader.ShaderProgramID);

            // Transformation from the shaders
            vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            vNormalLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal");
            uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");

            // Specular light from the shaders
            uSpecularLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.SpecularLight");
            uDiffuseLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.DiffuseLight");
            uAmbientLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.AmbientLight");

            u2SpecularLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.SpecularLight");
            u2DiffuseLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.DiffuseLight");
            u2AmbientLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.AmbientLight");

            // Material Properties
            vAmbientLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.AmbientReflectivity");
            vDiffuseLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.DiffuseReflectivity");
            vSpecularLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.SpecularReflectivity");
            vShineLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.Shininess");

            // sets direction to be to the right shoulder

            GL.GenVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);

            // Binds floor vertices data to the GPU using VBO's and VAO's

            GL.BindVertexArray(mVAO_IDs[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(floorVertices.Length * sizeof(float)), floorVertices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (floorVertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }


            GL.EnableVertexAttribArray(vPositionLocation);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));


            // Binds armadillo vertices data to the GPU using VBO's and VAO's

            #region Loading in the armadillo

            mArmadilloModelUtility = ModelUtility.LoadModel(@"Utility/Models/model.bin");

            GL.BindVertexArray(mVAO_IDs[1]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[1]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mArmadilloModelUtility.Vertices.Length * sizeof(float)), mArmadilloModelUtility.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[2]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mArmadilloModelUtility.Indices.Length * sizeof(float)), mArmadilloModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mArmadilloModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mArmadilloModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            #endregion

            #region Loading in the cylinder

            mCylinderModelUtility = ModelUtility.LoadModel(@"Utility/Models/cylinder.bin");

            GL.BindVertexArray(mVAO_IDs[2]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[3]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mCylinderModelUtility.Vertices.Length * sizeof(float)), mCylinderModelUtility.Vertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[4]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(mCylinderModelUtility.Indices.Length * sizeof(float)), mCylinderModelUtility.Indices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCylinderModelUtility.Vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (mCylinderModelUtility.Indices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Index data not loaded onto graphics card correctly");
            }

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            #endregion done

            #region Loading in the cube

            GL.BindVertexArray(mVAO_IDs[3]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[6]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(cubeVertices.Length * sizeof(float)), cubeVertices, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, mVBO_IDs[7]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(cubeIndices.Length * sizeof(uint)), cubeIndices, BufferUsageHint.StaticDraw);

            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (cubeVertices.Length * sizeof(float) != size)
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");

            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (cubeIndices.Length * sizeof(uint) != size)
                throw new ApplicationException("Index data not loaded onto graphics card correctly");

            GL.EnableVertexAttribArray(vPositionLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);

            #endregion done

            GL.BindVertexArray(0);


            mView = Matrix4.CreateTranslation(0, -1.5f, 0);
            UpdateEyePosition();
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);



            int uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.Position");
            Vector4 lightPosition = new Vector4(2, 4, -8.5f, 1);
            lightPosition = new Vector4(mView.ExtractTranslation(), 1);
            GL.Uniform4(uLightPositionLocation, lightPosition);

            // Initial position of the models
            mGroundModel = Matrix4.CreateTranslation(0, 0, -6.5f);
            mArmadilloModel = Matrix4.CreateTranslation(0, 3, -5.5f);
            mCylinderModel = Matrix4.CreateTranslation(0, 1, -5.5f);
            mCubeModel = Matrix4.CreateTranslation(0, 0, -10.5f);

            // Initialisating lighting

            ambientLight = new Vector3(0.8f, 0.8f, 0.8f);
            GL.Uniform3(uAmbientLightLocation, ambientLight);
            diffuseLight = new Vector3(0.6f, 0.6f, 0.6f);
            GL.Uniform3(uDiffuseLightLocation, diffuseLight);
            specularLight = new Vector3(0.78f, 0.78f, 0.78f);
            GL.Uniform3(uSpecularLightLocation, specularLight);

            // Initial position of the eye
            Matrix4 eyeLocation = Matrix4.CreateTranslation(mView.ExtractTranslation());
            GL.UniformMatrix4(uEyePositionLocation, true, ref eyeLocation);

            base.OnLoad(e);
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(this.ClientRectangle);
            if (mShader != null)
            {
                int uProjectionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uProjection");
                Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(1, (float)ClientRectangle.Width / ClientRectangle.Height, 0.5f, 25);
                GL.UniformMatrix4(uProjectionLocation, true, ref projection);
            }
        }
        /// <summary>
        /// This method allows me to move the camera in worldspace to look around my scene
        /// </summary>
        /// <param name="e">On keypress corresponding character will move camera </param>
        protected override void OnKeyPress(KeyPressEventArgs e)
        {
            base.OnKeyPress(e);

            // moves camera forward
            if (e.KeyChar == 'w')
                TranslateCamera(new Vector3(0.0f, 0.0f, 0.05f));

            // camera looks to the left
            if (e.KeyChar == 'a')
                RotateCameraX(-0.025f);            

            // moves camera backwards
            if (e.KeyChar == 's')
                TranslateCamera(new Vector3(0.0f, 0.0f, -0.05f));

            // camera looks to the right
            if (e.KeyChar == 'd')
                RotateCameraX(0.025f);

            // camera does a front flip
            if (e.KeyChar == 'r')
                RotateCameraY(0.025f);

            // camera does a back flip
            if (e.KeyChar == 'f')
                RotateCameraY(-0.025f);

            // rotate the model to the left
            if (e.KeyChar == 'c')
                RotateModelX(-0.025f);

            // rotate the model to the right
            if (e.KeyChar == 'v')
                RotateModelX(0.025f);

            // changes the camera to a birds eye view perspective
            if (e.KeyChar == 'b')
                BirdsEyeView(1.5708f);

            // changes the camera back to the main starting point
            if (e.KeyChar == 'm')
                MainView(0);
        }

        /// <summary>
        /// Rotates the armadillo on the X axis
        /// </summary>
        /// <param name="radianChange">radianChange allows passing of rotation parameters where the method is called </param>
        protected void RotateModelX(float radianChange)
        {
            Vector3 t = mArmadilloModel.ExtractTranslation();
            Matrix4 translation = Matrix4.CreateTranslation(t);
            Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
            mArmadilloModel = mArmadilloModel * inverseTranslation * Matrix4.CreateRotationY(radianChange) * translation;
        }

        /// <summary>
        /// Rotates the camera on the Y axis
        /// </summary>
        /// <param name="radianChange">radianChange allows passing of rotation parameters where the method is called </param>
        protected void RotateCameraY(float radianChange)
        {
            mView *= Matrix4.CreateRotationX(radianChange);
            ChangeView();
            UpdateEyePosition();
        }

        /// <summary>
        /// Translates to a birds eye view of the whole scene
        /// </summary>
        /// <param name="radianChange">radianChange allows passing of rotation parameters where the method is called </param>
        protected void BirdsEyeView(float radianChange)
        {
            mView = Matrix4.CreateRotationX(radianChange) * Matrix4.CreateTranslation(0 , -12, -15f);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);
            ChangeView();
            UpdateEyePosition();
        }

        /// <summary>
        /// Translates to the main starting view of the whole scene
        /// </summary>
        /// <param name="radianChange">radianChange allows passing of rotation parameters where the method is called </param>
        protected void MainView(float radianChange)
        {
            mView = Matrix4.CreateRotationX(0) * Matrix4.CreateTranslation(0, -1.5f, 0);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);
            ChangeView();
            UpdateEyePosition();
        }

        /// <summary>
        /// Allows rotation of the Camera on the X axis
        /// </summary>
        /// <param name="radianChange"></param>
        protected void RotateCameraX(float radianChange)
        {
            mView *= Matrix4.CreateRotationY(radianChange);
            ChangeView();
            UpdateEyePosition();
        }

        /// <summary>
        /// Translates the Camera to a certain position
        /// </summary>
        /// <param name="positionChange"> Vector position to specify translation on the x,y,z axis</param>
        protected void TranslateCamera(Vector3 positionChange)
        {
            mView *= Matrix4.CreateTranslation(positionChange);
            ChangeView();
            UpdateEyePosition();
        }

        /// <summary>
        /// Changes the view for the camera
        /// </summary>
        protected void ChangeView()
        {
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);
        }

        /// <summary>
        /// Animation for the cube so it Pans around the Y axis
        /// </summary>
        /// <param name="e">Per frame per second. Make a rotation </param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            mCubeModel *= Matrix4.CreateRotationY(-0.025f);
            base.OnUpdateFrame(e);
        }

        /// <summary>
        /// Update the Eye position per translation made so objects are in view
        /// </summary>
        protected void UpdateEyePosition()
        {
            Vector3 inverseTranslation = mView.ExtractTranslation();
            Quaternion inverseRotation = mView.ExtractRotation();
            Vector3 invertEyePosition = Vector3.Transform(inverseTranslation, inverseRotation);
            Vector4 eyePosition = new Vector4(-invertEyePosition, 1);
            GL.Uniform4(uEyePositionLocation, eyePosition);
        }

        /// <summary>
        /// Passes in the enum made at the top to generate materials referancing ambience, diffusing and specular lighting
        /// </summary>
        /// <param name="material"></param>
        void ApplyMaterial(Material material)
        {
            if (material == Material.emerald)
            {
                ambientReferance = new Vector3(0.0215f, 0.1745f, 0.0215f);
                diffuseReferance = new Vector3(0.07568f, 0.61424f, 0.07568f);
                specularReferance = new Vector3(0.633f, 0.727811f, 0.633f);
                shininess = 0.6f;
            }

            else if (material == Material.yellowRubber)
            {
                ambientReferance = new Vector3(0.0f, 0.0f, 0.0f);
                diffuseReferance = new Vector3(0.5f, 0.5f, 0.0f);
                specularReferance = new Vector3(0.6f, 0.6f, 0.50f);
                shininess = 0.25f;
            }

            else if (material == Material.brass)
            {
                ambientReferance = new Vector3(0.329412f, 0.223529f, 0.027451f);
                diffuseReferance = new Vector3(0.780392f, 0.568627f, 0.113725f);
                specularReferance = new Vector3(0.992157f, 0.941176f, 0.807843f);
                shininess = 0.21794872f;
            }

            else if (material == Material.gold)
            {
                ambientReferance = new Vector3(0.24725f, 0.1995f, 0.0745f);
                diffuseReferance = new Vector3(0.75164f, 0.60648f, 0.22648f);
                specularReferance = new Vector3(0.628281f, 0.555802f, 0.366065f);
                shininess = 0.4f;
            }

            else if (material == Material.chrome)
            {
                ambientReferance = new Vector3(0.25f, 0.25f, 0.25f);
                diffuseReferance = new Vector3(0.4f, 0.4f, 0.4f);
                specularReferance = new Vector3(0.774597f, 0.774597f, 0.774597f);
                shininess = 0.6f;
            }

            GL.Uniform3(vAmbientLocation, ambientReferance);
            GL.Uniform3(vDiffuseLocation, diffuseReferance);
            GL.Uniform3(vSpecularLocation, specularReferance);
            GL.Uniform1(vShineLocation, shininess * 128);
        }

        /// <summary>
        /// This renders all of the models and lighting
        /// </summary>
        /// <param name="e"></param>

        #region Rendering

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // floor rendering
            ApplyMaterial(Material.chrome);
            int uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref mGroundModel);
            GL.BindVertexArray(mVAO_IDs[0]);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            // Armadillo rendering
            ApplyMaterial(Material.emerald);
            Matrix4 m = mArmadilloModel * mGroundModel;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m);
            GL.BindVertexArray(mVAO_IDs[1]);
            GL.DrawElements(PrimitiveType.Triangles, mArmadilloModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            // Cylinder rendering
            ApplyMaterial(Material.brass);
            Matrix4 m2 = mCylinderModel * mGroundModel;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m2);
            GL.BindVertexArray(mVAO_IDs[2]);
            GL.DrawElements(PrimitiveType.Triangles, mCylinderModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            // Cube rendering
            ApplyMaterial(Material.gold);
            Matrix4 m3 = mCubeModel * mGroundModel;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m3);
            GL.BindVertexArray(mVAO_IDs[3]);
            GL.DrawElements(PrimitiveType.Triangles, cubeIndices.Length, DrawElementsType.UnsignedInt, 0);

            // Light rendering
            int uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLightPosition");
            Vector4 lightPosition = Vector4.Transform(new Vector4(8, 1, -8.5f, 1), mView);
            GL.Uniform4(uLightPositionLocation, ref lightPosition);
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

        #endregion
    }
}
