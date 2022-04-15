namespace RedHeart.Shaders
{
    public static class VertexShader
    {
        public static readonly string text = @"
            #version 330
            layout (location = 0) in vec3 vPos;
            layout (location = 1) in vec3 vNormal;

            uniform mat4 model;
            uniform mat4 view;
            uniform mat4 projection;
            
            out vec3 Normal;
            out vec3 FragPos;
            
            void main()
            {
                gl_Position = vec4(vPos, 1.0) * model * view * projection;
                FragPos = vec3(vec4(vPos, 1.0) * model);
                Normal = vNormal * mat3(transpose(inverse(model)));
            }
        ";
    }
}
