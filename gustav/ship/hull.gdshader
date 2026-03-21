shader_type canvas_item;
uniform float wave_speed = 1.0;
uniform float wave_size = 0.02;

uniform sampler2D noise_img : repeat_enable;
uniform float edge_size = 2.;

void vertex() {
	float wave = sin(TIME * wave_speed) * wave_size;
	VERTEX.y += wave * (1.0 - UV.y) * 100.0;
}

void fragment() {
    // 1. Get the base texture (your water)
    vec4 text = texture(TEXTURE, UV);
    vec2 size = TEXTURE_PIXEL_SIZE * edge_size;
	
	float outline = 
		texture(TEXTURE, UV - vec2(size.x, 0)).a
		+ texture(TEXTURE, UV + vec2(size.x, 0)).a
		+ texture(TEXTURE, UV - vec2(0, size.y)).a
		+ texture(TEXTURE, UV + vec2(0, size.y)).a
	;
	outline = min(1, outline);
	float edge = outline - text.a;
	float noise_value_1 = texture(noise_img, UV + TIME * 0.1).r;
	float noise_value_2 = texture(noise_img, UV - TIME * 0.1).r;
	float noise_value = (noise_value_1 + noise_value_2) / 2.0;
	float foam = smoothstep(0.4, 0.5, noise_value);
	
    COLOR = mix(text, vec4(1), edge * foam);
}
