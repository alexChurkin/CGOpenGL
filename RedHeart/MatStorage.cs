using OpenTK.Mathematics;

namespace RedHeart
{
    public struct Material
    {
        public readonly string Name;
        public readonly Vector3 Ambient;
        public readonly Vector3 Diffuse;
        public readonly Vector3 Specular;
        public readonly float Shininess;

        public Material(
            string name, 
            Vector3 ambient,
            Vector3 diffuse,
            Vector3 specular,
            float shininess)
        {
            Name = name;
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
            Shininess = shininess;
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public static class MatStorage
    {
        public static Material[] materials = new Material[] {
            new Material(
                "Red Heart",
                new Vector3(1.0f, 0.2f, 0.4f),
                new Vector3(1.0f, 0.2f, 0.4f),
                new Vector3(0.8f),
                48.0f
            ),
            new Material(
                "Some Orange",
                new Vector3(1.0f, 0.5f, 0.31f),
                new Vector3(1.0f, 0.5f, 0.31f),
                new Vector3(0.5f, 0.5f, 0.5f),
                32.0f
            ),
            new Material(
                "Green Emerald",
                new Vector3(0.0215f, 0.1745f, 0.0215f),
                new Vector3(0.07568f, 0.61424f, 0.07568f),
                new Vector3(0.633f, 0.727811f, 0.633f),
                0.6f * 128.0f
            ),
            new Material(
                "Cyan Plastic",
                new Vector3(0.0f, 0.1f, 0.06f),
                new Vector3(0.0f, 0.50980392f, 0.50980392f),
                new Vector3(0.50196078f, 0.50196078f, 0.50196078f),
                0.25f * 128.0f
            ),
            new Material(
                "Green Rubber",
                new Vector3(0.0f, 0.05f, 0.0f),
                new Vector3(0.4f, 0.5f, 0.4f),
                new Vector3(0.04f, 0.7f, 0.04f),
                0.078125f * 128.0f
            ),
            new Material(
                "Bronze",
                new Vector3(0.2125f, 0.1275f, 0.054f),
                new Vector3(0.714f, 0.4284f, 0.18144f),
                new Vector3(0.393548f, 0.271906f, 0.166721f),
                0.2f * 128.0f
            ),
            new Material(
                "Silver",
                new Vector3(0.19225f),
                new Vector3(0.50754f),
                new Vector3(0.508273f),
                0.4f * 128.0f
            ),
            new Material(
                "Black Rubber",
                new Vector3(0.02f),
                new Vector3(0.01f),
                new Vector3(0.4f),
                0.078125f * 128.0f
            )
    };

        public static int MaterialsCount
        {
            get { return materials.Length; }
        }
    }
}