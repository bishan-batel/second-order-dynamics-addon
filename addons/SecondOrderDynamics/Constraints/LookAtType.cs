namespace SecondOrderDynamics.Constraints;

/// <summary>
///  Setting for whether a LookAtConstraint will focus on a Node or a Code-Specified Position
/// </summary>
public enum LookAtType {
  /// <summary>
  /// The look-at constraint will update to look at a Node2D or Node3D
  /// </summary>
  Node,

  /// <summary>
  /// The look-at constraint will update to look at a specified Vector2/3 position
  /// </summary>
  Position
}