using OpenTK.Mathematics;
using System;

namespace RedHeart
{
    public class Camera
    {
        //Векторы, задающие направления осей координат для камеры
        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        //Угол поворота камеры вокруг оси X (в радианах)
        private float _pitch;
        //Угол поворота камеры вокруг оси Y (в радианах)
        //(без -2pi/2 камера при запуске была бы повёрнута вправо на 90 градусов)
        private float _yaw = -MathHelper.PiOver2;

        //Угол обзора камеры (в радианах)
        private float _fov = MathHelper.PiOver2;

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
        }

        //Позиция камеры (точка)
        public Vector3 Position { get; set; }

        //Соотношение сторон окна, используемое для расчёта projection-матрицы
        public float AspectRatio { private get; set; }

        public Vector3 Front => _front;
        public Vector3 Up => _up;
        public Vector3 Right => _right;


        /* Удобные свойства в градусах: */

        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                //Вычисление угла наклона камеры в радианах
                //и его присваивание переменной _pitch
                //Значение ужимается в диапазон от -89 до 89 для предотвращения странных багов.
                var angle = MathHelper.Clamp(value, -89f, 89f);
                _pitch = MathHelper.DegreesToRadians(angle);
                UpdateVectors();
            }
        }

        public float Yaw
        {
            get => MathHelper.RadiansToDegrees(_yaw);
            set
            {
                _yaw = MathHelper.DegreesToRadians(value);
                UpdateVectors();
            }
        }

        //Угол обзора - (вертикальный) угол обзора камеры.
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        //Получение матрицы отображения с помощью функции LookAt
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + _front, _up);
        }

        //Получение матрицы проекции
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
        }

        //Обновляет вектора направления камеры
        private void UpdateVectors()
        {
            //Вычислим вектор "вперёд" для камеры
            _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            _front.Y = MathF.Sin(_pitch);
            _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);
            _front = Vector3.Normalize(_front);

            //Также вычислим векторы "вправо" и "вверх" через векторное произведение
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }
}