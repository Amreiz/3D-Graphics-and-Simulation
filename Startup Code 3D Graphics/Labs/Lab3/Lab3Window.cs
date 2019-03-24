using Labs.Utility;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using System;

namespace Labs.Lab3
{
    //L3T1 Changed the clear colour to see some silhouettes
    //L3T2 Finished off first person style camera
    //L3T3 Added per vertex attribute for the normal and linked VBO data to it.
    //L3T5 Can rotate the sphere in sphere space (around the middle of the sphere)
    //L3T6 Made a directional light in the vertex shader
    //L3T8 Adjusted directional light for a rotated view matrix
    //L3T9 Implemented positional light
    //L3T10 Moved point lighting into the fragment shader

    public class Lab3Window : GameWindow
    {
        public Lab3Window()
            : base(
                800, // Width
                600, // Height
                GraphicsMode.Default,
                "Lab 3 Lighting and Material Properties",
                GameWindowFlags.Default,
                DisplayDevice.Default,
                3, // major
                3, // minor
                GraphicsContextFlags.ForwardCompatible
                )
        {
        }

        private enum Material
        {
            yellowRubber,
            gold,
            brass,
            chrome,
            emerald
        }

        private int[] mVBO_IDs = new int[5];
        private int[] mVAO_IDs = new int[3];
        private ShaderUtility mShader;
        private ModelUtility mArmadilloModelUtility;
        private ModelUtility mCylinderModelUtility;
        private Matrix4 mView, mArmadilloModel, mGroundModel, mCylinderModel;
        private Vector3 specularLight, diffuseLight, ambientLight, ambientReferance, diffuseReferance, specularReferance;
        private float shininess;

        int vPositionLocation, vNormalLocation, vAmbientLocation, vSpecularLocation, vDiffuseLocation, vShineLocation;
        int uEyePositionLocation, uSpecularLightLocation, uDiffuseLightLocation, uAmbientLightLocation, uLightPosition;

        protected override void OnLoad(EventArgs e)
        {
            // Set some GL state
            GL.ClearColor(Color4.ForestGreen);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);

            mShader = new ShaderUtility(@"Lab3/Shaders/vPassThrough.vert", @"Lab3/Shaders/vLighting.frag");
            GL.UseProgram(mShader.ShaderProgramID);

            // Transformation from the shaders
            vPositionLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vPosition");
            vNormalLocation = GL.GetAttribLocation(mShader.ShaderProgramID, "vNormal");
            uEyePositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uEyePosition");

            // Specular light from the shaders
            uSpecularLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.SpecularLight");
            uDiffuseLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.DiffuseLight");
            uAmbientLightLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.AmbientLight");

            // Material Properties
            vAmbientLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.AmbientReflectivity");
            vDiffuseLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.DiffuseReflectivity");
            vSpecularLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.SpecularReflectivity");
            vShineLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uMaterial.Shininess");

            

            // sets direction to be to the right shoulder

            //Vector3 normalisedLightDirection, lightDirection = new Vector3(-1, -1, -1);
            //Vector3.Normalize(ref lightDirection, out normalisedLightDirection);
            //GL.Uniform3(uLightPositionLocation, normalisedLightDirection);


            GL.GenVertexArrays(mVAO_IDs.Length, mVAO_IDs);
            GL.GenBuffers(mVBO_IDs.Length, mVBO_IDs);

            float[] vertices = new float[] {-10, 0, -10,0,1,0,
                                             -10, 0, 10,0,1,0,
                                             10, 0, 10,0,1,0,
                                             10, 0, -10,0,1,0,};

            GL.BindVertexArray(mVAO_IDs[0]);
            GL.BindBuffer(BufferTarget.ArrayBuffer, mVBO_IDs[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (vertices.Length * sizeof(float) != size)
            {
                throw new ApplicationException("Vertex data not loaded onto graphics card correctly");
            }


            GL.EnableVertexAttribArray(vPositionLocation);
            GL.EnableVertexAttribArray(vNormalLocation);
            GL.VertexAttribPointer(vPositionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);
            GL.VertexAttribPointer(vNormalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));


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

            // Loading in the cylinder

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

            //////////

            GL.BindVertexArray(0);

            mView = Matrix4.CreateTranslation(0, -1.5f, 0);
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);

            int uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLight.Position");
            Vector4 lightPosition = new Vector4(2, 4, -8.5f, 1);
            lightPosition = new Vector4(mView.ExtractTranslation(), 1);
            GL.Uniform4(uLightPositionLocation, lightPosition);

            mGroundModel = Matrix4.CreateTranslation(0, 0, -5f);
            mArmadilloModel = Matrix4.CreateTranslation(0, 3, -5f);
            mCylinderModel = Matrix4.CreateTranslation(0, 1, -5f);

            ambientLight = new Vector3(0.5f, 0.5f, 0.5f);
            GL.Uniform3(uAmbientLightLocation, ambientLight);

            diffuseLight = new Vector3(0.8f, 0.8f, 0.8f);
            GL.Uniform3(uDiffuseLightLocation, diffuseLight);

            specularLight = new Vector3(0.4f, 0.4f, 0.4f);
            GL.Uniform3(uSpecularLightLocation, specularLight);

            Matrix4 eyeLocation = Matrix4.CreateTranslation(mView.ExtractTranslation());
            GL.UniformMatrix4(uEyePositionLocation, true, ref eyeLocation);

            ApplyMaterial(Material.emerald);
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

        public delegate void RadianContainer(float radian);

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

        }

        void RotateModelX(float radianChange)
        {
            Vector3 t = mArmadilloModel.ExtractTranslation();
            Matrix4 translation = Matrix4.CreateTranslation(t);
            Matrix4 inverseTranslation = Matrix4.CreateTranslation(-t);
            mArmadilloModel = mArmadilloModel * inverseTranslation * Matrix4.CreateRotationY(radianChange) * translation;
        }
        void RotateCameraY(float radianChange)
        {
            mView *= Matrix4.CreateRotationX(radianChange);
            ChangeView();
            UpdateEyePosition();
        }

        void RotateCameraX(float radianChange)
        {
            mView *= Matrix4.CreateRotationY(radianChange);
            ChangeView();
            UpdateEyePosition();
        }

        void TranslateCamera(Vector3 positionChange)
        {
            mView *= Matrix4.CreateTranslation(positionChange);
            ChangeView();
            UpdateEyePosition();
        }

        void ChangeView()
        {
            int uView = GL.GetUniformLocation(mShader.ShaderProgramID, "uView");
            GL.UniformMatrix4(uView, true, ref mView);
        }

        void UpdateEyePosition()
        {
            Vector3 inverseTranslation = mView.ExtractTranslation();
            Quaternion inverseRotation = mView.ExtractRotation();
            Vector3 invertEyePosition = Vector3.Transform(inverseTranslation, inverseRotation);
            Vector4 eyePosition = new Vector4(-invertEyePosition, 1);
            GL.Uniform4(uEyePositionLocation, eyePosition);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            // floor rendering
            int uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref mGroundModel);


            GL.BindVertexArray(mVAO_IDs[0]);
            GL.DrawArrays(PrimitiveType.TriangleFan, 0, 4);

            // Armadillo rendering
            ApplyMaterial(Material.gold);
            Matrix4 m = mArmadilloModel * mGroundModel;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m);

            GL.BindVertexArray(mVAO_IDs[1]);
            GL.DrawElements(PrimitiveType.Triangles, mArmadilloModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            // Cylinder rendering
            ApplyMaterial(Material.yellowRubber);
            Matrix4 m2 = mCylinderModel * mGroundModel;
            uModel = GL.GetUniformLocation(mShader.ShaderProgramID, "uModel");
            GL.UniformMatrix4(uModel, true, ref m2);

            GL.BindVertexArray(mVAO_IDs[2]);
            GL.DrawElements(PrimitiveType.Triangles, mCylinderModelUtility.Indices.Length, DrawElementsType.UnsignedInt, 0);

            // Light rendering
            int uLightPositionLocation = GL.GetUniformLocation(mShader.ShaderProgramID, "uLightPosition");
            Vector4 lightPosition = Vector4.Transform(new Vector4(8, 1, -8.5f, 1), mView);
            GL.Uniform4(uLightPositionLocation, ref lightPosition);

            GL.BindVertexArray(0);
            this.SwapBuffers();
        }

        void ApplyMaterial(Material material)
        {
            if (material == Material.emerald)
            {
                ambientReferance = new Vector3(0.0215f, 0.1745f, 0.0215f);
                diffuseReferance = new Vector3(0.07568f, 0.61424f, 0.07568f);
                specularReferance = new Vector3(0.633f, 0.727811f, 0.633f);
                shininess = 0.6f;
            }

            else if(material == Material.yellowRubber)
            {
                ambientReferance = new Vector3(0.0f, 0.0f, 0.0f);
                diffuseReferance = new Vector3(0.5f, 0.5f, 0.0f);
                specularReferance = new Vector3(0.6f, 0.6f, 0.50f);
                shininess = 0.25f;
            }

            else if(material == Material.brass)
            {
                ambientReferance = new Vector3(0.329412f, 0.223529f, 0.027451f);
                diffuseReferance = new Vector3(0.780392f, 0.568627f, 0.113725f);
                specularReferance = new Vector3(0.992157f, 0.941176f, 0.807843f);
                shininess = 0.21794872f;
            }

            else if(material == Material.gold)
            {
                ambientReferance = new Vector3(0.24725f, 0.1995f, 0.0745f);
                diffuseReferance = new Vector3(0.75164f, 0.60648f, 0.22648f);
                specularReferance = new Vector3(0.628281f, 0.555802f, 0.366065f);
                shininess = 0.4f;
            }

            else if(material == Material.chrome)
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
    }
}
