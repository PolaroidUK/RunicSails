shader_type canvas_item;
uniform float speed_u = 1.0;
uniform float speed_v = 1.0;
uniform float speed2_u = 1.0;
uniform float speed2_v = 1.0;

uniform float tiling = 1.0;

uniform sampler2D noise_img : repeat_enable;

varying vec4 modulate;

void vertex() {
	modulate = COLOR;
}

void fragment() {
    vec2 uv = UV;
	uv = uv * tiling;

    // Calculate the scrolled UV position
    float scrolled_v = mod(uv.y - TIME * speed_v, 1);
	float scrolled_u = mod(uv.x - TIME * speed_u, 1);
	float scrolled2_v = mod(uv.y - TIME * speed2_v, 1);
	float scrolled2_u = mod(uv.x - TIME * speed2_u, 1);

    // If the UV is beyond the visible range, wrap it around to the top
    if (scrolled_v < 0.0) {
        scrolled_v += 1.0;
    }
	if (scrolled_u < 0.0) {
        scrolled_u += 1.0;
    }
	if (scrolled2_v < 0.0) {
        scrolled2_v += 1.0;
    }
	if (scrolled2_u < 0.0) {
        scrolled2_u += 1.0;
    }

    // Sample the texture using the scrolled UV
	vec4 color = texture(TEXTURE, vec2(scrolled_u, scrolled_v)) * texture(noise_img, vec2(scrolled2_u, scrolled2_v));
	color.a = max(max(color.r, color.g), color.b) * color.a;
    COLOR = modulate * color;
}
