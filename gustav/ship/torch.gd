extends PointLight2D
class_name Torch

## A torch. It light. Yai.

@onready var torch: PointLight2D = $"."
@onready var animation_player: AnimationPlayer = $Sprite2D/AnimationPlayer

var noise = FastNoiseLite.new()
var time = 0.0

enum State { lit, unlit }

@export var _current_state = State.lit
@export var speed = 1
@export var amplitude = 0.7
var start_energy

func _ready() -> void:
	noise.seed = randi()
	noise.frequency = 0.5
	start_energy = energy
	if _current_state == State.unlit:
		turn_off(true)
	else:
		turn_on(true)

func _process(delta: float) -> void:
	if(_current_state == State.unlit):
		return
	time += delta * speed
	var n = noise.get_noise_1d(time)
	energy = start_energy + (n * amplitude)

func turn_on(force = false) -> bool:
	if(_current_state == State.lit and not force):
		return false
	animation_player.play("torch")
	_current_state = State.lit
	return true
	
func turn_off(force = false) -> bool:
	if(_current_state == State.unlit and not force):
		return false
	animation_player.play("unlit")
	energy = 0
	_current_state = State.unlit
	return true
