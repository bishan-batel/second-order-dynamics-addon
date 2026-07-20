#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;

namespace SecondOrderDynamics.Constraints;

/// <summary>
/// A NON-PHYSICS constraint that will always rotate to face (Z-) a specified node or global position
/// </summary>
[GlobalClass, Tool, Icon("res://addons/SecondOrderDynamics/Icons/SodAnimator.svg")]
public partial class LookAtConstraint3D : Node3D {
  #region Exports

  /// <summary>
  /// Setting for whether to lock onto a node or specified position
  /// </summary>
  [Export]
  public LookAtType Type {
    set {
      _type = value;

      if (Active) {
        #if TOOLS
        if (!Engine.IsEditorHint() || RunInEditor) {
          LookAtTarget();
        }
        #else
        LookAtTarget();
        #endif
      }

      #if TOOLS
      UpdateConfigurationWarnings();
      NotifyPropertyListChanged();
      #endif
    }
    get => _type;
  }

  /// <summary>
  /// Position (in global space) to face towards
  /// </summary>
  [Export]
  public Vector3 LookAtPosition {
    set => _lookAtPosition = value;
    get => _lookAtPosition;
  }

  /// <summary>
  /// Target node to look at when <see cref="Type"/> is set to <see cref="LookAtType.Node"/>
  /// </summary>
  [Export]
  public Node3D? Target {
    set {
      _target = value;

      #if TOOLS
      UpdateConfigurationWarnings();
      #endif
    }
    get => _target;
  }

  /// <summary>
  /// Up direction, used to orient self
  /// </summary>
  [Export]
  public Vector3 UpDirection {
    set => _upDirection = value;
    get => _upDirection;
  }

  /// <summary>
  /// Should this constraint run while in the editor
  /// </summary>
  [ExportSubgroup("Update"), Export]
  public bool RunInEditor {
    set {
      _runInEditor = value;
      if (_runInEditor) UsePhysicsProcess = false;
      _updateProcessMode();

      #if TOOLS
      UpdateConfigurationWarnings();
      NotifyPropertyListChanged();
      #endif
    }
    get => _runInEditor;
  }

  /// <summary>
  /// Whether to update orientation on PhysicsProcess instead of Process
  /// </summary>
  [Export]
  public bool UsePhysicsProcess { set; get; }

  /// <summary>
  /// Whether it should run every frame / is running right now
  /// </summary>
  [Export]
  public bool Active {
    set {
      _active = value;

      if (!_active) return;

      #if TOOLS
      if (!Engine.IsEditorHint() || RunInEditor) {
        LookAtTarget();
      }
      #else
      LookAtTarget();
      #endif
    }
    get => _active;
  }

  #endregion

  LookAtType _type;
  Vector3 _lookAtPosition;
  Vector3 _upDirection = Vector3.Up;
  bool _runInEditor = true;
  bool _active = true;
  Node3D? _target;

  /// <summary>
  /// Default constructor
  /// </summary>
  public LookAtConstraint3D() {
    #if TOOLS
    RunInEditor = _runInEditor;
    #else
    _updateProcessMode();
    #endif
  }

  /// <summary>
  /// Manually triggers reorientation to the target position
  /// </summary>
  public void LookAtTarget() {
    Vector3 position = GetTargetPosition();
    if (position.IsEqualApprox(GlobalPosition)) return;
    LookAt(position, UpDirection);
  }

  /// <summary>
  /// Gets the current target position to look at this, note this is not the same as <see cref="LookAtPosition"/>
  /// </summary>
  /// <returns></returns>
  public Vector3 GetTargetPosition() {
    if (Type == LookAtType.Position) {
      return _lookAtPosition;
    }

    if (IsInstanceValid(Target) && Target is not null) {
      return Target.GlobalPosition;
    }

    return default;
  }

  void _updateProcessMode() {
    #if TOOLS
    if (Engine.IsEditorHint() && !RunInEditor) {
      SetProcess(false);
      SetPhysicsProcess(false);
      return;
    }
    #endif

    SetProcess(!UsePhysicsProcess);
    SetPhysicsProcess(UsePhysicsProcess);
  }

  /// <inheritdoc />
  public override void _Process(double delta) {
    base._Process(delta);

    #if TOOLS
    if (Engine.IsEditorHint() && !RunInEditor) return;
    #endif

    if (!UsePhysicsProcess) LookAtTarget();
  }

  /// <inheritdoc />
  public override void _PhysicsProcess(double delta) {
    base._PhysicsProcess(delta);

    #if TOOLS
    if (Engine.IsEditorHint() && !RunInEditor) return;
    #endif

    if (UsePhysicsProcess) LookAtTarget();
  }

  #if TOOLS
  /// <inheritdoc />
  /// <exception cref="ArgumentOutOfRangeException">Only occurs of <see cref="Type"/> is malformed</exception>
  public override void _ValidateProperty(Dictionary property) {
    StringName name = property["name"].AsStringName();


    if (RunInEditor && name == PropertyName.UsePhysicsProcess) {
      property["usage"] = (int)PropertyUsageFlags.NoEditor;
      return;
    }

    if (Type == LookAtType.Node) {
      if (name == PropertyName.LookAtPosition) {
        property["usage"] = (int)PropertyUsageFlags.NoEditor;
        return;
      }
    }
    else if (Type == LookAtType.Position) {
      if (name == PropertyName.Target) {
        property["usage"] = (int)PropertyUsageFlags.NoEditor;
        return;
      }
    }

    base._ValidateProperty(property);
  }

  /// <inheritdoc />
  public override string[] _GetConfigurationWarnings() {
    List<string> warnings = base._GetConfigurationWarnings()?.ToList() ?? [];

    if (Type == LookAtType.Node && !IsInstanceValid(Target)) {
      warnings.Add("Target must be set when Type is set to 'Node'");
    }

    return warnings.ToArray();
  }

  #endif
}