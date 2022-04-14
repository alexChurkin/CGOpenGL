namespace OpenGLHeart
{
    //Вершина
    public class Vertex
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public Vertex(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }
    }

    //Нормаль
    public class Normal
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }

        public Normal(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString()
        {
            return $"({x}, {y}, {z})";
        }
    }
}
