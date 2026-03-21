extends Node2D
class_name Ship
@onready var _animation_player: AnimationPlayer = $AnimationPlayer
@onready var _bubles_l: GPUParticles2D = $"bubbles/bubles2-l"
@onready var _bubles_r: GPUParticles2D = $"bubbles/bubles-r"
@onready var _sail: Sprite2D = $sail
@onready var _mast: Sprite2D = $mast
@onready var _torches: Node2D = $torches

## Ohoy Mate'y. A ship. Aye.
##
## A ship you can drop into the scene and call functions on.
## Mainly visual stuff.

enum SailsState {
	NO_SAILS, FULL_SAILS
}
enum SailPace {
	STILL, SLOW, QUICK
}
enum LightsState {
	on, off
}

var _bubbles_emitting = {
	SailPace.STILL: false,
	SailPace.SLOW: true,
	SailPace.QUICK: true,
}
var _bubbles_amount = {
	SailPace.STILL: 1,
	SailPace.SLOW: 20,
	SailPace.QUICK: 200,
}
@onready var _mast_shader_params = {
	SailPace.STILL: {
		"sway_speed": 2.0,
		"sway_amount": 0.005,
		"wave_speed": 1.0,
		"wave_size": 0.01,
	},
	SailPace.SLOW: {
		"sway_speed": 2.0,
		"sway_amount": 0.01,
		"wave_speed": 1.3,
		"wave_size": 0.01,
	},
	SailPace.QUICK: {
		"sway_speed": 3.0,
		"sway_amount": 0.01,
		"wave_speed": 2.0,
		"wave_size": 0.02,
	}
}
@onready var _sail_shader_params = {
	SailPace.STILL: {
		"sway_speed": 2.0,
		"sway_amount": 0.05,
		"wave_speed": 1.0,
		"wave_size": 0.01,
	},
	SailPace.SLOW: {
		"sway_speed": 2.0,
		"sway_amount": 0.05,
		"wave_speed": 1.3,
		"wave_size": 0.01,
	},
	SailPace.QUICK: {
		"sway_speed": 3.0,
		"sway_amount": 0.07,
		"wave_speed": 2.0,
		"wave_size": 0.02,
	}
}


@export var _current_sails_state: SailsState = SailsState.NO_SAILS;
@export var _current_sail_pace: SailPace = SailPace.STILL;
@export var _current_light_state: LightsState = LightsState.on;

func _ready() -> void:
	if(_current_sails_state == SailsState.NO_SAILS):
		_animation_player.play("sails-up")
	else:
		_animation_player.play_backwards("sails-up")
	set_sail_pace(_current_sail_pace, true)
	set_lights(_current_light_state, true)

func set_lights(state: LightsState, force = false):
	var children = _torches.get_children() as Array[Torch]
	for child in children:
		if(state == LightsState.off):
			child.turn_off(force)
		else:
			child.turn_on(force)
	
## Sets mast movement and bubbles
func set_sail_pace(pace: SailPace, force: bool = false) -> bool:
	if(pace == _current_sails_state and not force): 
		return false
	_bubles_l.emitting = _bubbles_emitting[pace]
	_bubles_r.emitting = _bubbles_emitting[pace]
	_bubles_l.amount = _bubbles_amount[pace]
	_bubles_r.amount = _bubbles_amount[pace]
	
	var _sm = _sail.material as ShaderMaterial
	var _sail_params = _sail_shader_params[pace]
	_sm.set_shader_parameter("sway_speed", _sail_params.sway_speed)
	_sm.set_shader_parameter("sway_amount", _sail_params.sway_amount)
	_sm.set_shader_parameter("wave_speed", _sail_params.wave_speed)
	_sm.set_shader_parameter("wave_size", _sail_params.wave_size)
	
	var _mm = _mast.material as ShaderMaterial
	var _mast_params = _mast_shader_params[pace]
	_mm.set_shader_parameter("sway_speed", _mast_params.sway_speed)
	_mm.set_shader_parameter("sway_amount", _mast_params.sway_amount)
	_mm.set_shader_parameter("wave_speed", _mast_params.wave_speed)
	_mm.set_shader_parameter("wave_size", _mast_params.wave_size)
	
	_current_sail_pace = pace
	return true

func raise_sails() -> bool:
	if _current_sails_state == SailsState.NO_SAILS:
		return false
	_animation_player.play("sails-up")
	return true

func drop_sails() -> bool:
	if _current_sails_state == SailsState.FULL_SAILS:
		return false
	_animation_player.play_backwards("sails-up")
	return true
