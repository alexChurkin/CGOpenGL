using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Input;

namespace OpenGLHeart
{
    sealed class Program : GameWindow
    {
        //Вершинный шейдер
        const string VertexShaderSource = @"
            #version 330

            //На вход - координаты вершины и нормали к ней
            //и матрица масштабирования
            //location - номер аргумента

            layout(location = 0) in vec3 position;
            layout(location = 1) in vec3 normal;

            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;

            //Выходные данные

            out vec3 frPos;
            out vec3 frNormal;

            void main(void)
            {
                gl_Position = vec4(position, 1.0) * model * view * projection;
                
                frPos = vec3(model * vec4(position, 1.0));

                //frNormal = normal;
                frNormal = mat3(transpose(inverse(model))) * normal;  
            }
        ";

        //Фрагментный шейдер. Реализует освещение (прожектор)
        const string FragmentShaderSource = @"
            #version 330

            //На вход нормаль к вершине и положение света

            in vec3 frPos;
            in vec3 frNormal;

            uniform vec3 lightPos;

            //На выход - итоговый цвет

            out vec4 finalColor;

            void main(void)
            {
                vec3 heartColor = vec3(1.0, 0.0, 0.0);
                vec3 lightColor = vec3(1.0, 1.0, 1.0);

                vec3 norm = normalize(frNormal);
                vec3 lightDir = normalize(lightPos - frPos);

                float diff = max(dot(norm, lightDir), 0.0);

                vec3 ambient = 0.12 * lightColor;
                vec3 diffuse = diff * lightColor;

                vec3 result = (ambient + diffuse) * heartColor;
                finalColor = vec4(result, 1.0);
            }
        ";

        //Все вершины с их нормалями в формате:
        //x1, y1, z1, w1, nx1, ny1, nz1,
        //x2, y2, z2, w2, nx2, ny2, nz2,
        //...........
        //xm, ym, zm, wm, nxn, nyn, nzn
        //Первые 4 числа - описание вершины, следующие 3 - нормали, и т. д.
        float[] VerticesWithNormals;

        //Треугольники, представленные номерами вершин каждый
        //в виде v11, v12, v13 <- первый треугольник
        //       v21, v22, v23 <- следующий
        //       ...........
        //       vk1, vk2, vk3 <- последний треугольник
        int[] Elements;

        int VertexShader;
        int FragmentShader;
        int ShaderProgram;
        int VertexBufferObject;
        int VertexArrayObject;
        int ElementsBufferObject;

        Program() : base(
            640, 480, new GraphicsMode(32, 24, 8, 8), "OpenTK Heart") { }

        protected override void OnLoad(EventArgs e)
        {
            ObjReader r = new ObjReader();
            //ObjResource objRes = r.ReadObj("NiceHeart.obj");
            ObjResource objRes = r.ReadObj(@"C:\Users\lxchu\Desktop\cube.obj");

            //Получение вершин в нужном для отрисовки формате
            VerticesWithNormals = objRes.ObtainVerticesWithNormals();
            //Получение треугольников, составленных из вершин
            Elements = objRes.triangles;

            //Загрузка исходного кода вершинного шейдера и его компиляция
            VertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(VertexShader, VertexShaderSource);
            GL.CompileShader(VertexShader);

            //Загрузка исходного кода фрагментного шейдера и его компиляция
            FragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(FragmentShader, FragmentShaderSource);
            GL.CompileShader(FragmentShader);

            //Создание шейдерной программы, связывание вершинного и
            //фрагментного шейдеров с программой и линковка программы
            ShaderProgram = GL.CreateProgram();
            GL.AttachShader(ShaderProgram, VertexShader);
            GL.AttachShader(ShaderProgram, FragmentShader);
            GL.LinkProgram(ShaderProgram);

            //Создание объекта-буфера вершин для данных вершин
            VertexBufferObject = GL.GenBuffer();
            //Бинд VBO и копирование данных вершин в буфер
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, VerticesWithNormals.Length * sizeof(float), VerticesWithNormals, BufferUsageHint.StaticDraw);

            //Получение положения position из программы
            var positionLocation = GL.GetAttribLocation(ShaderProgram, "position");
            var normalLocation = GL.GetAttribLocation(ShaderProgram, "normal");

            //Создание объекта массива вершин (VAO) для программы
            VertexArrayObject = GL.GenVertexArray();
            //Бинд VAO
            GL.BindVertexArray(VertexArrayObject);

            /* Указание OpenGL, как интерпретировать float массив вершин,
            т.к. там хранятся ещё и координаты векторов нормалей: */
            GL.EnableVertexAttribArray(positionLocation);
            GL.EnableVertexAttribArray(normalLocation);

            //Первым аргументом в шейдер по 3 float-а вершин (x,y,z) с шагом 6
            //без смещения от начала
            GL.VertexAttribPointer(positionLocation, 3, VertexAttribPointerType.Float,
                    false, 6 * sizeof(float), 0);

            //Вторым аргументом в шейдер по 3 float-а нормалей (x,y,z)
            //с шагом 6 со смещением 3 от начала
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float,
                    false, 6 * sizeof(float), 3 * sizeof(float));
            //------------------------------------------------------

            //Создание объекта-буфера элементов для треугольников
            ElementsBufferObject = GL.GenBuffer();
            //Бинд EBO и копирование данных треугольников в буфер
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementsBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Elements.Length * sizeof(int), Elements, BufferUsageHint.StaticDraw);

            //Установка серого фона
            GL.ClearColor(0.15f, 0.15f, 0.15f, 0.0f);

            //Настройка: отрисовывать только видимые грани
            GL.Enable(EnableCap.CullFace);

            //Настройка: проверка глубины
            GL.Enable(EnableCap.DepthTest);

            //Настройка: более реалистичное освещение
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlphaSaturate, BlendingFactorDest.One);
            base.OnLoad(e);
        }

        protected override void OnUnload(EventArgs e)
        {
            //Анбиндинг всех ресурсов установкой биндов в 0/null
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindVertexArray(0);
            GL.UseProgram(0);

            //Удаление всех ресурсов
            GL.DeleteBuffer(VertexBufferObject);
            GL.DeleteBuffer(ElementsBufferObject);
            GL.DeleteVertexArray(VertexArrayObject);
            GL.DeleteProgram(ShaderProgram);
            GL.DeleteShader(FragmentShader);
            GL.DeleteShader(VertexShader);

            base.OnUnload(e);
        }

        protected override void OnResize(EventArgs e)
        {
            //Установка размера viewport для его соответствия размеру окна
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }

        bool upscaling = false;
        int iScale = 2000;

        int iRot = 0;

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            KeyboardState input = Keyboard.GetState();
            if (input.IsKeyDown(Key.Escape))
                Exit();

            if (!upscaling)
                iScale--;
            else
                iScale++;

            if (iScale == 1700 || iScale == 2000)
                upscaling = !upscaling;

            float fScale = iScale / 2000.0f;

            iRot++;
            if (iRot == 14400) iRot = 0;

            //Очистка цветового буфера
            GL.Clear(ClearBufferMask.ColorBufferBit);
            //Очистка буфера глубины
            GL.Clear(ClearBufferMask.DepthBufferBit);

            //Бинд VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            //Бинд VAO
            GL.BindVertexArray(VertexArrayObject);
            //Бинд EBO
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementsBufferObject);

            //Создание матрицы масштабирования
            Matrix4 scale = Matrix4.CreateScale(fScale, fScale, fScale);
            Matrix4 rotation = Matrix4.CreateRotationY(MathHelper.DegreesToRadians(iRot / 40.0f));
            //Привязка матрицы периодического увеличения и поворота к шейдерной программе
            Matrix4 model = rotation * scale;
            int loc1 = GL.GetUniformLocation(ShaderProgram, "model");
            GL.UniformMatrix4(loc1, true, ref model);

            Vector3 position = new Vector3(0.0f, 0.0f, 3.0f);
            Vector3 front = new Vector3(0.0f, 0.0f, -1.0f);
            Vector3 up = new Vector3(0.0f, 1.0f, 0.0f);
            Matrix4 view = Matrix4.LookAt(position, position + front, up);

            int loc2 = GL.GetUniformLocation(ShaderProgram, "view");
            GL.UniformMatrix4(loc2, true, ref view);

            //Привязка матрицы проекции к шейдерной программе
            Matrix4 projection = Matrix4.CreatePerspectiveFieldOfView(
                MathHelper.DegreesToRadians(45.0f), Width / Height, 2.9f, 100.0f);
            int loc3 = GL.GetUniformLocation(ShaderProgram, "projection");
            GL.UniformMatrix4(loc3, true, ref projection);

            //Создание точки, откуда идёт освещение
            Vector3 lightPos = new Vector3(2.0f, -1.0f, 2.0f);
            //Привязка точки к шейдерной программе
            int loc4 = GL.GetUniformLocation(ShaderProgram, "lightPos");
            GL.Uniform3(loc4, ref lightPos);

            //Matrix4 viewMatrix = Matrix4.LookAt(new Vector3(0.0f, 0.0f, 0.9f),//откуда смотреть? (точка)
            //new Vector3(0.0f, 0.0f, 0.0f),//куда смотреть? (точка)
            //new Vector3(0.0f, 1f, 0.0f)); //вектор up (не трогать)
            //int loc3 = GL.GetUniformLocation(ShaderProgram, "viewMatrix");
            //GL.UniformMatrix4(loc3, true, ref viewMatrix);

            //Использование уже скомпилированной шейдерной программы
            GL.UseProgram(ShaderProgram);

            //Отрисовка треугольников
            GL.DrawElements(
                PrimitiveType.Triangles,
                Elements.Length,
                DrawElementsType.UnsignedInt, 0);

            //Смена местами front/back буферов, чтобы отобразить то,
            //что было только что срендерено на back буфере, "впереди"
            Context.SwapBuffers();

            base.OnRenderFrame(e);
        }

        [STAThread]
        static void Main()
        {
            var program = new Program();
            program.Run();
        }
    }
}
