@tool
extends Node3D

@export var ray_length: float = 2.0;
@export var ankle_height: float = 0.01
@export var max_foot_distance: float = 0.5;
@export var speed: float = 1.0; 

@export var legs: Array[SpiderLeg] = [
	$"Skeleton3D/Front Left",
	$"Skeleton3D/Front Right",
	$"Skeleton3D/Hind Left",
	$"Skeleton3D/Hind Right"
] 

@onready var camera: Camera3D = $Camera3D

func _process(_delta: float) -> void:
	if Engine.is_editor_hint():
		_solve_legs()
		return
	
func _physics_process(delta: float) -> void:
	if Engine.is_editor_hint():
		return
		
	_solve_legs()
	
	var direction := Input.get_vector("left", "right", "back","forward").normalized()
	self.global_position += global_basis.orthonormalized() * (Vector3.FORWARD * direction.y + Vector3.RIGHT * direction.x) * delta * speed
	self.rotate_y(Input.get_axis("rotate_right", "rotate_left") * delta * 1.0)
		
func _solve_legs() -> void:
	for leg in legs:
		if leg != null:
			_solve_foot(leg);



func _get_target_foot_pos(leg: SpiderLeg) -> Vector3:
	if not leg.can_move():
		return leg.target.global_position
	
	var from: Vector3 = leg.global_position;
	var to: Vector3 = from + Vector3.DOWN * ray_length;
	
	if Engine.is_editor_hint():
		return from + Vector3.DOWN * 1;
	
	var space := get_world_3d().direct_space_state;
	var query := PhysicsRayQueryParameters3D.create(from, to)
	query.collide_with_areas = false;
	
	var hit := space.intersect_ray(query)
	
	if hit:
		return hit.position + Vector3.UP * ankle_height;
		
	return leg.target.global_position;

func _solve_foot(leg: SpiderLeg) -> void:
	var foot_pos := _get_target_foot_pos(leg)
	
	if leg.target.global_position.distance_to(foot_pos) > max_foot_distance:
		leg.target.global_position = foot_pos
