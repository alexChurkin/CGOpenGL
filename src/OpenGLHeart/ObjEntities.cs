namespace OpenGLHeart
{
    //Вершина
    public class Vertex
    {
        public float x { get; set; }
        public float y { get; set; }
        public float z { get; set; }
        public float w { get; set; }

        public Vertex(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Vertex(float x, float y, float z)
            : this(x, y, z, 1.0f) { }

        public override string ToString()
        {
            return $"({x}, {y}, {z}, {w})";
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
