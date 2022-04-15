using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.GraphicsLibraryFramework;
using OpenTK.Windowing.Desktop;
using RedHeart.ObjImport;
using RedHeart.Shaders;

namespace RedHeart
{
    public class Window : GameWindow
    {
        private float[] _verticesAndNormals;
        private int[] _elements;

        private int _vertexArrayObject;
        private int _vertexBufferObject;
        private int _elementsBufferObject;

        private ShaderProgram shaderProgram;

        //Состояние мыши
        private bool _firstMove = true;
        private Vector2 _lastMousePos;

        //Состояние камеры
        private Camera _camera;

        //Переменные состояния:
        //Для масштабирования сердца
        private float _scale = 1.0f;
        private float _scaleSpeed = 0.3f;
        private bool _isUpscaling = false;
        //И для поворота
        private float _rotationDegrees = 0.0f;

        //Отвязанность прожектора от камеры
        private bool _spotlightFixed = false;

        //Начальное положение камеры в мире
        private Vector3 _startCameraPos = new Vector3(0.0f, 0.0f, 1.2f);
        //Текущее положение 
        private Vector3 _currentSpotlightPos = new Vector3(0.0f, 0.0f, 1.2f);
        private Vector3 _currentSpotlightDir;

        public Window(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
        }

        protected override void OnLoad()
        {
            base.OnLoad();

            //Импорт 3D-модели сердца из файла
            ObjReader r = new ObjReader();
            GL3DModel objRes = r.ReadObj("NiceHeart.obj");

            //Получение массива вершин/нормалей
            //и массива номеров вершин треугольников
            _verticesAndNormals = objRes.GetVerticesWithNormals();
            _elements = objRes.triangles;

            //Сборка шейдерной программы (компиляция и линковка двух шейдеров)
            shaderProgram = new ShaderProgram(
                VertexShader.text, FragmentShader.text);

            //Создание Vertex Array Object и его привязка
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);

            //Создание объекта буфера вершин/нормалей, его привязка и заполнение
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, _verticesAndNormals.Length * sizeof(float),
                _verticesAndNormals, BufferUsageHint.StaticDraw);

            //Указание OpenGL, где искать вершины в буфере вершин/нормалей
            var positionLocation = shaderProgram.GetAttribLocation("vPos");
            GL.EnableVertexAttribArray(positionLocation);
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 0);

            //Указание OpenGL, где искать нормали в буфере вершин/нормалей
            var normalLocation = shaderProgram.GetAttribLocation("vNormal");
            GL.EnableVertexAttribArray(normalLocation);
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float, false, 6 * sizeof(float), 3 * sizeof(float));

            //Создание, привязка и заполнение объекта-буфера элементов для треугольников
            _elementsBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _elementsBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _elements.Length * sizeof(int), _elements, BufferUsageHint.StaticDraw);

            //Установка серого фона
            GL.ClearColor(0.15f, 0.15f, 0.15f, 0.0f);
            //Включение отрисовки только видимого...
            GL.Enable(EnableCap.CullFace);
            //и теста глубины во избежание наложений
            GL.Enable(EnableCap.DepthTest);

            //Установка стартового положения камеры
            _camera = new Camera(_startCameraPos, Size.X / (float)Size.Y);

            //Захват курсора
            CursorGrabbed = true;
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            base.OnRenderFrame(e);

            //Очистка буферов цвета и глубины
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            //Привязка буфера вершин
            GL.BindVertexArray(_vertexArrayObject);
            //Указание использовать данную шейдерную программу
            shaderProgram.Use();

            //Привязка входных данных через uniform-механизм
            shaderProgram.SetMatrix4("view", _camera.GetViewMatrix());
            shaderProgram.SetMatrix4("projection", _camera.GetProjectionMatrix());
            shaderProgram.SetVector3("viewPos", _camera.Position);

            //shaderProgram.SetInt("material.diffuse", 0);
            //shaderProgram.SetInt("material.specular", 1);
            //shaderProgram.SetVector3("material.specular", new Vector3(0.5f, 0.5f, 0.5f));
            shaderProgram.SetFloat("material.shininess", 50.0f);
            if (_spotlightFixed)
            {
                shaderProgram.SetVector3("light.position", _currentSpotlightPos);
                shaderProgram.SetVector3("light.direction", _currentSpotlightDir);
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

            //Матрица модели - матрица для сердца в начале координат с заданным масштабированием и поворотом
            Matrix4 model =
                Matrix4.CreateScale(_scale) * 
                Matrix4.CreateRotationY(MathHelper.DegreesToRadians(_rotationDegrees));
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
                return;

            var input = KeyboardState;

            //Закрытие окна на Esc
            if (input.IsKeyDown(Keys.Escape))
                Close();

            //Обновление значений масштаба и поворота
            if (_scale <= 0.8f)
                _isUpscaling = true;
            if (_scale >= 0.999999f)
                _isUpscaling = false;

            if (_isUpscaling)
                _scale += _scaleSpeed * (float)e.Time;
            else
                _scale -= _scaleSpeed * (float)e.Time;

            _rotationDegrees += 10f * (float)e.Time;
            if (_rotationDegrees >= 359.999) _rotationDegrees = 0.0f;

            //Обработка нажатий клавиш
            //(в том числе вычисление нового положения камеры перед следующим кадром)
            const float cameraSpeed = 1.5f;
            const float sensitivity = 0.1f;

            if (input.IsKeyDown(Keys.W))
                _camera.Position += _camera.Front * cameraSpeed * (float)e.Time;
            if (input.IsKeyDown(Keys.S))
                _camera.Position -= _camera.Front * cameraSpeed * (float)e.Time;
            if (input.IsKeyDown(Keys.A))
                _camera.Position -= _camera.Right * cameraSpeed * (float)e.Time;
            if (input.IsKeyDown(Keys.D))
                _camera.Position += _camera.Right * cameraSpeed * (float)e.Time;
            if (input.IsKeyPressed(Keys.F))
            {
                _spotlightFixed = !_spotlightFixed;
                if (_spotlightFixed)
                {
                    _currentSpotlightPos = _camera.Position;
                    _currentSpotlightDir = _camera.Front;
                }
            }

            //Обновление направления камеры исходя из передвижения мыши
            var mouse = MouseState;

            if (_firstMove)
            {
                _lastMousePos = new Vector2(mouse.X, mouse.Y);
                _firstMove = false;
            }
            else
            {
                var deltaX = mouse.X - _lastMousePos.X;
                var deltaY = mouse.Y - _lastMousePos.Y;
                _lastMousePos = new Vector2(mouse.X, mouse.Y);

                _camera.Yaw += deltaX * sensitivity;
                _camera.Pitch -= deltaY * sensitivity;
            }
        }

        //Обновление поля зрения камеры по колесу мыши
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            base.OnMouseWheel(e);
            _camera.Fov -= 2 * e.OffsetY;
        }

        //Обновление размеров области видимости при изменении размеров окна
        protected override void OnResize(ResizeEventArgs e)
        {
            base.OnResize(e);
            GL.Viewport(0, 0, Size.X, Size.Y);
            _camera.AspectRatio = Size.X / (float)Size.Y;
        }
    }
}