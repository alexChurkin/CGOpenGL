using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL4;

namespace BasicTriangle
{
    sealed class Program : GameWindow
    {
        //Простейший вершинный шейдер. Just passes through the position vector.
        const string VertexShaderSource = @"
            #version 330

            layout(location = 0) in vec4 position;

            void main(void)
            {
                gl_Position = position;
            }
        ";

        //Простой фрагментный шейдер. Просто постоянный красный цвет
        const string FragmentShaderSource = @"
            #version 330

            out vec4 outputColor;

            void main(void)
            {
                outputColor = vec4(1, 0.0, 0.0, 1.0);
            }
        ";
        //Точки сердца в нормализованных координатах (x, y, z от -1 до 1)
        //x1, y1, z1, w1,
        //x2, y2, z2, w2,
        //...........
        //xm, ym, zm, wm
        float[] Points;
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
            ObjResource objRes = r.ReadObj("Heart3DModel.obj");

            //Vertex[] vertices = o.vertices;
            //TODO Использовать позже
            //Normal[] normals = objRes.normals;

            //Получение вершин в нужном для отрисовки формате
            Points = objRes.GetFloatPoints();
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
            GL.BufferData(BufferTarget.ArrayBuffer, Points.Length * sizeof(float), Points, BufferUsageHint.StaticDraw);

            //Получение position location из программы
            var positionLocation = GL.GetAttribLocation(ShaderProgram, "position");

            //Создание объекта массива вершин (VAO) для программы
            VertexArrayObject = GL.GenVertexArray();
            //Бинд VAO и установка атрибута position
            GL.BindVertexArray(VertexArrayObject);
            GL.VertexAttribPointer(positionLocation, 4, VertexAttribPointerType.Float, false, 0, 0);
            GL.EnableVertexAttribArray(positionLocation);

            //Создание объекта-буфера элементов для треугольников
            ElementsBufferObject = GL.GenBuffer();
            //Бинд EBO и копирование данных треугольников в буфер
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementsBufferObject);
            GL.BufferData(BufferTarget.ElementArrayBuffer, Elements.Length * sizeof(int), Elements, BufferUsageHint.StaticDraw);

            // Очистка цвета (установка синего)
            GL.ClearColor(0.0f, 0.0f, 0.5f, 0.0f);

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

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            //Очистка цветового буфера
            GL.Clear(ClearBufferMask.ColorBufferBit);

            //Бинд VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, VertexBufferObject);
            //Бинд VAO
            GL.BindVertexArray(VertexArrayObject);
            //Бинд EBO
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ElementsBufferObject);
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
