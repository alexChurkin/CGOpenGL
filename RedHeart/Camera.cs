using OpenTK.Mathematics;
using System;

namespace RedHeart
{
    public class Camera
    {
        //�������, �������� ����������� ������������ ��������� ������
        private Vector3 _front = -Vector3.UnitZ;
        private Vector3 _up = Vector3.UnitY;
        private Vector3 _right = Vector3.UnitX;

        //���� �������� ������ ������ ��� X (� ��������)
        private float _pitch;
        //���� �������� ������ ������ ��� Y (� ��������)
        //(��� -2pi/2 ������ ��� ������� ���� �� �������� ������ �� 90 ��������)
        private float _yaw = -MathHelper.PiOver2;

        //���� ������ ������ (� ��������)
        private float _fov = MathHelper.PiOver2;

        public Camera(Vector3 position, float aspectRatio)
        {
            Position = position;
            AspectRatio = aspectRatio;
        }

        //������� ������ (�����)
        public Vector3 Position { get; set; }

        //����������� ������ ����, ������������ ��� ������� projection-�������
        public float AspectRatio { private get; set; }

        public Vector3 Front => _front;
        public Vector3 Up => _up;
        public Vector3 Right => _right;


        /* ������� �������� � ��������: */

        public float Pitch
        {
            get => MathHelper.RadiansToDegrees(_pitch);
            set
            {
                //���������� ���� ������� ������ � ��������
                //� ��� ������������ ���������� _pitch
                //�������� ��������� � �������� �� -89 �� 89 ��� �������������� �������� �����.
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

        //���� ������ - (������������) ���� ������ ������.
        public float Fov
        {
            get => MathHelper.RadiansToDegrees(_fov);
            set
            {
                var angle = MathHelper.Clamp(value, 1f, 90f);
                _fov = MathHelper.DegreesToRadians(angle);
            }
        }

        // Get the view matrix using the amazing LookAt function described more in depth on the web tutorials
        public Matrix4 GetViewMatrix()
        {
            return Matrix4.LookAt(Position, Position + _front, _up);
        }

        // Get the projection matrix using the same method we have used up until this point
        public Matrix4 GetProjectionMatrix()
        {
            return Matrix4.CreatePerspectiveFieldOfView(_fov, AspectRatio, 0.01f, 100f);
        }

        // This function is going to update the direction vertices using some of the math learned in the web tutorials.
        private void UpdateVectors()
        {
            //�������� ������ "�����" ��� ������
            _front.X = MathF.Cos(_pitch) * MathF.Cos(_yaw);
            _front.Y = MathF.Sin(_pitch);
            _front.Z = MathF.Cos(_pitch) * MathF.Sin(_yaw);
            _front = Vector3.Normalize(_front);

            //����� �������� ������� "������" � "�����" ����� ��������� ������������
            _right = Vector3.Normalize(Vector3.Cross(_front, Vector3.UnitY));
            _up = Vector3.Normalize(Vector3.Cross(_right, _front));
        }
    }
}