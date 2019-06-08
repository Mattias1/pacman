#version 130

uniform mat4 modelMatrix;
uniform mat4 viewMatrix;
uniform mat4 projectionMatrix;

in vec3 v_position;
in vec3 v_normal;
in vec2 v_texcoord;
in vec4 v_color;

out vec3 p_normal;
out vec2 p_texcoord;
out vec4 p_color;

void main()
{
	// The ugly hack
	vec4 pos = modelMatrix * vec4(v_position, 1.0);
	if (pos.y > 0.7) {
		// Move the unwanted vertices (the ones above the horizontal 0.7-plane) to the center of the object, where we won't see them.
		pos = vec4(0.0);
	}

	// The rest of the shader
	gl_Position = projectionMatrix * viewMatrix * pos;
    
    p_normal = normalize(mat3(modelMatrix) * v_normal);
    p_texcoord = v_texcoord;
    p_color = v_color;
}