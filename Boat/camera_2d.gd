extends Camera2D

var zoom_target: Vector2 = zoom
var zoom_smoothing_speed: float = 1.0
var zoom_threshold: float = 0.001

func set_zoom_smooth(new_target_zoom):
	if new_target_zoom is float:
		zoom_target = Vector2(new_target_zoom, new_target_zoom)
	elif new_target_zoom is Vector2:
		zoom_target = new_target_zoom
	else:
		printerr("Error: Invalid zoom target type. Use Vector2 or float.")

func _process(delta: float) -> void:
	var zoom_difference = zoom_target - zoom
	
	if zoom_difference.length() > zoom_threshold:
		var c = zoom_smoothing_speed * delta
		zoom += zoom_difference * c
	else:
		zoom = zoom_target
