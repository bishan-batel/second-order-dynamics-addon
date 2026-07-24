@tool
class_name SpiderLeg
extends FABRIK3D

@onready
var target: Marker3D = $Target

@onready
var sod: SodRemoteTransform3D = $SodRemoteTransform3D

@export var adjacent_legs: Array[SpiderLeg]

func can_move() -> bool:
	if sod.global_position.distance_to(target.global_position) > 3:
		return true;
		
	if not is_grounded():
		return false
		
	for leg in adjacent_legs:
		if !leg.is_grounded():
			return false 
	return true

func is_grounded() ->  bool:
	return sod.global_position.distance_to(target.global_position) < 0.1
