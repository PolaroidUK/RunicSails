extends Node2D
class_name SeaEnvironment

@onready var _rain: GPUParticles2D = $Weather/Rain
@onready var _wind: GPUParticles2D = $Weather/Wind
@onready var canvas_modulate: CanvasModulate = $CanvasModulate

enum WindState { NO_WIND, STRONG_WIND }
enum RainState { NO_RAIN, STRONG_RAIN }
enum TimeState { NIGHT, MORNING }

@export var emission_box_size: Vector2 = Vector2(400, 100)
@export var _current_wind = WindState.STRONG_WIND
@export var _current_rain = RainState.STRONG_RAIN
@export var _current_time = TimeState.NIGHT

var wind_enable = {
	WindState.NO_WIND: false,
	WindState.STRONG_WIND: true,
}

var rain_enable = {
	RainState.NO_RAIN: false,
	RainState.STRONG_RAIN: true,
}
var time_color = {
	TimeState.NIGHT: Color(0.105, 0.253, 0.34, 1.0),
	TimeState.MORNING: Color(0.862, 0.631, 0.38, 1.0)
}

func _ready() -> void:
	var r = _rain.process_material as ParticleProcessMaterial
	r.emission_box_extents = Vector3(emission_box_size.x, emission_box_size.y, 1)
	var w = _wind.process_material as ParticleProcessMaterial
	w.emission_box_extents = Vector3(emission_box_size.x, emission_box_size.y, 1)
	canvas_modulate.color = time_color[_current_time]

func set_time(time: TimeState) -> bool:
	if(_current_time == time):
		return false
	_current_time = time
	canvas_modulate.color = time_color[time]
	return true
	

func set_wind(wind: WindState) -> bool:
	if(_current_wind == wind):
		return false
	_current_wind = wind
	_wind.visible = wind_enable[wind]
	return true
	
func set_rain(rain: RainState) -> bool:
	if(_current_rain == rain):
		return false
	_current_rain = rain
	_rain.visible = rain_enable[rain]
	return true
