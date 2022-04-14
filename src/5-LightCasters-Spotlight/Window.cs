using System;
using LearnOpenTK.Common;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;

namespace LearnOpenTK
{
    // This tutorial is split up into multiple different bits, one for each type of light.

    // The following is the code for the spotlight, the functionality is much the same as the point light except it
    // only shines in one direction, for this we need the angle between the spotlight direction and the lightDir
    // then we can check if that angle is within the cutoff of the spotlight, if it is we light it accordingly
    public class Window : GameWindow
    {
        private float[] _vertices;
        private int[] _elements;

        private int _vertexBufferObject;

        private int _vaoModel;

        private ShaderProgram shaderProgram;

        private Camera _camera;

        private bool _firstMove = true;

        private Vector2 _lastPos;

        int ElementsBufferObject;

        private Vector3 startCameraPos = new Vector3(0.0f, 0.0f, 1.2f);

        private Vector3 currentSpotlightPos = new Vector3(0.0f, 0.0f, 1.2f);
        private Vector3 currentSpotlightDir;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            ObjReader r = new ObjReader();
            ObjResource objRes = r.ReadObj("NiceHeart.obj");

            _vertices = objRes.ObtainVerticesWithNormals();
            _elements = objRes.triangles;

            GL.ClearColor(0.15f, 0.15f, 0.15f, 0.0f);

            GL.Enable(EnableCap.CullFace);
            GL.Enable(EnableCap.DepthTest);

            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertices.Length * sizeof(float), _vertices, BufferUsageHint.StaticDraw);

            shaderProgram = new ShaderProgram(
                "Shaders/vertex.txt", "Shaders/fragment.txt");

            _vaoModel = GL.GenVertexArray();
            GL.BindVertexArray(_vaoModel);

            var positionLocation = shaderProgram.GetAttribLocation("aPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            var normalLocation = shaderProgram.GetAttribLocation("aNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            //Создание объекта-буфера элементов для треугольников
            ElementsBufferObject = GL.GenBuffer();
            //Бинд EBO и копирование данных треугольников в буфер
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementsBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _elements.Length * sizeof(int), _elements, BufferUsageHint.StaticDraw);

            _camera = new Camera(startCameraPos, Size.X / (float)Size.Y);

            CursorGrabbed = true;
        }


        bool upscaling = false;
        int iScale = 2000;

        int iRot = 0;
        bool spotlightFixed = false;

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.BindVertexArray(_vaoModel);

            shaderProgram.Use();

            shaderProgram.SetMatrix4("view", _camera.GetViewMatrix());
            shaderProgram.SetMatrix4("projection", _camera.GetProjectionMatrix());

            shaderProgram.SetVector3("viewPos", _camera.Position);

            //shaderProgram.SetInt("material.diffuse", 0);
            //shaderProgram.SetInt("material.specular", 1);
            //shaderProgram.SetVector3("material.specular", new Vector3(0.5f, 0.5f, 0.5f));
            shaderProgram.SetFloat("material.shininess", 50.0f);

            if (spotlightFixed)
            {
                shaderProgram.SetVector3("light.position", currentSpotlightPos);
                shaderProgram.SetVector3("light.direction", currentSpotlightDir);
            }
            else
            {
                shaderProgram.SetVector3("light.position", _camera.Position);
                shaderProgram.SetVector3("light.direction", _camera.Front);
            }
            shaderProgram.SetFloat("light.cutOff", MathF.Cos(MathHelper.DegreesToRadians(12.5f)));
            shaderProgram.SetFloat("light.outerCutOff", MathF.Cos(MathHelper.DegreesToRadians(32.5f)));
            shaderProgram.SetFloat("light.constant", 1.0f);
            shaderProgram.SetFloat("light.linear", 0.09f);
            shaderProgram.SetFloat("light.quadratic", 0.032f);
            shaderProgram.SetVector3("light.ambient", new Vector3(0.2f));
            shaderProgram.SetVector3("light.diffuse", new Vector3(0.5f));
            shaderProgram.SetVector3("light.specular", new Vector3(1.0f));

            if (!upscaling)
                iScale--;
            else
                iScale++;

            if (iScale == 1700 || iScale == 2000)
                upscaling = !upscaling;
            float fScale = iScale / 2000.0f;

            iRot++;
            if (iRot == 14400) iRot = 0;

            // We want to draw the heart at their respective position
            // Then we translate said matrix by the cube position
            Matrix4 model = Matrix4.CreateTranslation(new Vector3(0.0f, 0.0f, 0.0f));
            // We then calculate the angle and rotate the model around an axis
            model *= Matrix4.CreateScale(fScale);
            model *= Matrix4.CreateRotationY(MathHelper.DegreesToRadians(iRot / 40.0f));

            // Remember to set the model at last so it can be used by opentk
            shaderProgram.SetMatrix4("model", model);

            GL.DrawElements(
                PrimitiveType.Triangles,
                _elements.Length,
                DrawElementsType.UnsignedInt, 0);

            SwapBuffers();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            base.OnUpdateFrame(e);

            if (!IsFocused)
            {
                return;
            }

            var input = KeyboardState;

            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.1f;

            if (input.IsKeyDown(Keys.W))
            {
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time;
            }
            if (input.IsKeyDown(Keys.S))
            {
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time;
            }
            if (input.IsKeyDown(Keys.A))
            {
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time;
            }
            if (input.IsKeyDown(Keys.D))
            {
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time;
            }
            if (input.IsKeyPressed(Keys.F))
            {
                spotlightFixed = !spotlightFixed;
                if (spotlightFixed)
                {
                    currentSpotlightPos = _camera.Position;
                    currentSpotlightDir = _camera.Front;
                }

            }

            var mouse = MouseState;

            if (_firstMove)
            {
                _lastPos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - _lastPos.X;
                var deltaY = mouse.Y - _lastPos.Y;
                _lastPos = new Vector2(mouse.X, mouse.Y);

                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity;
            }
        }

        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);

            _camera.Fov -= e.OffsetY;
        }

        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);

            GL.Viewport(0, 0, Size.X, Size.Y);
            _camera.AspectRatio = Size.X / (float)Size.Y;
        }
    }
}