namespace RedHeart.Shaders
{
    public static class FragmentShader
    {
        public static readonly string text = @"
            #version 330
            
            struct Material {
                vec3 diffuse;
                vec3 specular;
                float shininess;
            };

            //The spotlight is some pointlight when we only want to show the light within a certain angle.
            //That angle is the cutoff, the outercutoff is used to make a more smooth border to the spotlight.
            struct Light {
                vec3 position;
                vec3 direction;
                float cutOff;
                float outerCutOff;
            
                vec3 ambient;
                vec3 diffuse;
                vec3 specular;
            
                float constant;
                float linear;
                float quadratic;
            };
            
            uniform Light light;
            uniform Material material;
            uniform vec3 viewPos;
            
            in vec3 FragPos;
            in vec3 Normal;

            out vec4 FragColor;
            
            void main()
            {
                vec3 heartColor = vec3(1.0, 0.2, 0.4);
            
                vec3 ambient = light.ambient * heartColor;
            
                //Diffuse
                vec3 norm = normalize(Normal);
                vec3 lightDir = normalize(light.position - FragPos);
                float diff = max(dot(norm, lightDir), 0.0);
                vec3 diffuse = light.diffuse * diff * heartColor;
            
                //Specular
                vec3 viewDir = normalize(viewPos - FragPos);
                vec3 reflectDir = reflect(-lightDir, norm);
                float spec = pow(max(dot(viewDir, reflectDir), 0.0), material.shininess);
                vec3 specular = light.specular * spec * heartColor;
            
                //Attenuation
                float distance    = length(light.position - FragPos);
                float attenuation = 1.0 / (light.constant + light.linear * distance
                                        + light.quadratic * (distance * distance));
            
                //Spotlight intensity
                //This is how we calculate the spotlight
                float theta     = dot(lightDir, normalize(-light.direction));
                float epsilon   = light.cutOff - light.outerCutOff;
                float intensity = clamp((theta - light.outerCutOff) / epsilon, 0.0, 1.0); //The intensity, is the lights intensity on a given fragment,
                                                                                            //this is used to make the smooth border.    
                //When applying the spotlight intensity we want to multiply it.
                ambient  *= attenuation; //Remember the ambient is where the light dosen't hit, this means the spotlight shouldn't be applied
                diffuse  *= attenuation * intensity;
                specular *= attenuation * intensity;
            
                //Setting up our final color of this fragment
                vec3 result = ambient + diffuse + specular;
                FragColor = vec4(result, 1.0);
            }
        ";
    }
}
