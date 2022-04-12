using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

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

            //КОРРЕКТНЫ
            layout(location = 0) in vec4 position;
            layout(location = 1) in vec3 normal;

            uniform mat4 scaleMatrix;

            //Выходные данные

            out vec3 frPos;
            out vec3 frNormal;

            void main(void)
            {
                vec4 worldCoords = scaleMatrix * position;

                gl_Position = worldCoords;
                
                frPos = vec3(worldCoords);
                frNormal = normal;
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
            ObjResource objRes = r.ReadObj("NiceHeart.obj");

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

            //Первым аргументом в шейдер по 4 float-а вершин (x,y,z,w) с шагом 7
            //без смещения от начала
            GL.VertexAttribPointer(positionLocation, 4, VertexAttribPointerType.Float,
                    false, 7 * sizeof(float), 0);

            //Вторым аргументом в шейдер по 3 float-а вершин (x,y,z,w)
            //с шагом 7 со смещением 4 от начала
            GL.VertexAttribPointer(normalLocation, 3, VertexAttribPointerType.Float,
                    false, 7 * sizeof(float), 4 * sizeof(float));
            //------------------------------------------------------

            //Создание объекта-буфера элементов для треугольников
            ElementsBufferObject = GL.GenBuffer();
            //Бинд EBO и копирование данных треугольников в буфер
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementsBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Elements.Length * sizeof(int), Elements, BufferUsageHint.StaticDraw);

            //Очистка цвета (установка серого)
            GL.ClearColor(0.15f, 0.15f, 0.15f, 0.0f);

            GL.Enable(EnableCap.CullFace);
            
            //Делает освещение интереснее
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
            // Изменение размера viewport для его соответствия размеру окна
            GL.Viewport(0, 0, Width, Height);
            base.OnResize(e);
        }

        bool upscaling = false;
        float iScale = 2000;

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //Уменьшение на 1
            if (!upscaling)
                iScale--;
            //Увеличение на 1
            else
                iScale++;
            if (iScale == 1700 || iScale == 2000)
                upscaling = !upscaling;

            float fScale = iScale / 2000.0f;

            //Очистка цветового буфера
            GL.Clear(ClearBufferMask.ColorBufferBit);

            //Бинд VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            //Бинд VAO
            GL.BindVertexArray(VertexArrayObject);
            //Бинд EBO
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementsBufferObject);

            //Создание матрицы масштабирования
            Matrix4 scale = Matrix4.CreateScale(fScale, fScale, fScale);
            //Привязка матрицы ко входу шейдерной программы
            int loc1 = GL.GetUniformLocation(ShaderProgram, "scaleMatrix");
            GL.UniformMatrix4(loc1, true, ref scale);

            //Создание вектора, задающего точку, откуда идёт освещение
            Vector3 lightPos = new Vector3(2.0f, 0f, 2.0f);
            //Привязка точки ко входу шейдерной программы
            int loc2 = GL.GetUniformLocation(ShaderProgram, "lightPos");
            GL.Uniform3(loc2, ref lightPos);

            //Использование ранее скомпилированной шейдерной программы
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
