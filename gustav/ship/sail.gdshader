shader_type canvas_item;
uniform float sway_speed = 2.0;
uniform float sway_amount = 0.02;

uniform float wave_speed = 1.0;
uniform float wave_size = 0.01;

void vertex() {
	float sway = sin(TIME * sway_speed) * sway_amount;
	float wave = sin(TIME * wave_speed) * wave_size;
	
	VERTEX.x += sway * (1.0 - UV.y) * 100.0;
	VERTEX.y += wave * (1.0 - UV.y) * 100.0;
}

void fragment() {
	// Called for every pixel the material is visible on.
}

//void light() {
//	// Called for every pixel for every light affecting the CanvasItem.
//	// Uncomment to replace the default light processing function with this one.
//}
