#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using SecondOrderDynamics.Math;

namespace SecondOrderDynamics.Animator;

/// <summary>
/// Base node of SOD Animators, this is used to simulate a second order system of
/// </summary>
[GlobalClass, Tool, Icon("res://addons/SecondOrderDynamics/Icons/SodAnimator.svg")]
public partial class SodAnimator : Node {
  /// <summary>
  /// A subset of <see cref="Variant.Type"/> to ones that support
  /// </summary>
  public enum SupportedType : long {
    /// <summary>
    /// <see cref="Variant.Type.Float"/>
    /// </summary>
    Float = Variant.Type.Float,

    /// <summary>
    /// <see cref="Variant.Type.Vector2"/>
    /// </summary>
    Vector2 = Variant.Type.Vector2,

    /// <summary>
    /// <see cref="Variant.Type.Vector3"/>
    /// </summary>
    Vector3 = Variant.Type.Vector3,

    /// <summary>
    /// <see cref="Variant.Type.Quaternion"/>
    /// </summary>
    Quaternion = Variant.Type.Quaternion,
  }

  /// <summary>
  /// The type this system is operating on, for ex. if you were to use this to animate position you would use <see cref="SupportedType.Vector2"/>
  /// </summary>
  /// <exception cref="ArgumentOutOfRangeException">This only throws if you give a malformed creation of <see cref="SupportedType"/></exception>
  [Export]
  public SupportedType ValueType {
    set {
      if (value != _valueType) {
        TargetValue = value switch {
          SupportedType.Float => 0,
          SupportedType.Vector2 => new Vector2(),
          SupportedType.Vector3 => new Vector3(),
          SupportedType.Quaternion => new Quaternion(),
          _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        };
      }

      _valueType = value;
#if TOOLS
      UpdateConfigurationWarnings();
      NotifyPropertyListChanged();
#endif
    }
    get => _valueType;
  }

  /// <summary>
  /// The target value for this simulation. Note that setting this value may update <see cref="ValueType"/> if the type is changed,
  /// ex. setting a TargetValue to a float and then to a Vector2 would change ValueType from SupportedType.Float -> SupportedType.Vector2.
  /// </summary>
  [ExportSubgroup("Simulation")]
  [Export]
  public Variant TargetValue {
    set {
      if (!SodVariant.IsValidType(value.VariantType)) {
        GD.PushError("[SodAnimator] The given TargetValue is not a supported type");
        return;
      }

      if (_targetValue.VariantType != value.VariantType) {
        _valueType = (SupportedType)(long)value.VariantType;
        _targetValue = value;

#if TOOLS
        UpdateConfigurationWarnings();
        NotifyPropertyListChanged();
#endif

        return;
      }

      _targetValue = value;
    }
    get => _targetValue;
  }

  /// <summary>
  /// Gets the current value of our system.
  /// </summary>
  public Variant CurrentValue => _sod?.Y ?? TargetValue;

  SodVariant? _sod;

  /// <summary>
  /// Whether to animate in PhysicsProcess rather than Process
  /// </summary>
  [Export]
  public bool UsePhysicsProcess {
    set {
      _usePhysicsProcess = value;
      _updateProcessMode();
    }
    get => _usePhysicsProcess;
  }

  /// <summary>
  /// Whether to simulate this system when running in-editor. Note if this can run in the editor this forces
  /// <see cref="UsePhysicsProcess"/> to be disabled.
  /// </summary>
  [Export]
  public bool RunInEditor {
    set {
      _runInEditor = value;
      if (_runInEditor) _usePhysicsProcess = false;

#if TOOLS
      NotifyPropertyListChanged();
#endif
    }
    get => _runInEditor;
  }

  /// <summary>
  /// Toggle for actively running the simulation or not.
  /// </summary>
  [Export]
  public bool IsRunning {
    set {
      _isRunning = value;
      SetProcess(!_isRunning);
      _updateProcessMode();
      SetPhysicsProcess(!_isRunning);
    }
    get => _isRunning;
  }

  /// <summary>
  /// Optional setting for updating a nodes property to match to with this system. For example if using this node to animate a node's rotation,
  /// Set <see cref="SetterNode"/> to the path to said node and set <see cref="SetterProperty"/> to the name of the property ("rotation" or "rotation_degrees")
  /// </summary>
  [ExportSubgroup("Remote Setter")]
  [Export]
  public NodePath? SetterNode {
    set {
      _setterNode = value;
#if TOOLS
      NotifyPropertyListChanged();
#endif
    }
    get => _setterNode;
  }

  /// <summary>
  /// Optional setting for use with <see cref="SetterNode"/>, see its documentation for full details.
  /// </summary>
  [Export(PropertyHint.EnumSuggestion)]
  public StringName? SetterProperty { set; get; }

  [Export]
  SodParams? Params {
    set {
      _params = value;
#if TOOLS
      UpdateConfigurationWarnings();
#endif
    }
    get => _params;
  }

  SodParams? _params;
  bool _usePhysicsProcess;
  SupportedType _valueType = SupportedType.Float;
  NodePath? _setterNode;
  Variant _targetValue = 0;
  bool _isRunning = true;
  bool _runInEditor;

  SodAnimator() {
    UsePhysicsProcess = _usePhysicsProcess;
  }

  /// <summary>
  /// Gets the interpolated property as its native type (Variant)
  /// </summary>
  /// <returns></returns>
  public Variant GetY() {
    return _sod?.Y ?? default;
  }

  /// <summary>
  /// Gets the interpolated property 'Y' as a given type
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <returns></returns>
  public T? GetYAs<T>() where T : class {
    return _sod?.Y as T;
  }

  /// <inheritdoc />
  public override void _Process(double delta) {
    if (_usePhysicsProcess) return;
    _update(delta);
  }

  /// <inheritdoc />
  public override void _PhysicsProcess(double delta) {
    if (!_usePhysicsProcess) return;
    _update(delta);
  }

  void _update(double delta) {
#if TOOLS
    if (Engine.IsEditorHint() && !RunInEditor) return;
#endif
    if (!IsRunning) return;
    if (_params is null) return;
    _sod ??= new SodVariant(_params, TargetValue);

    _sod.Params = _params;

    Variant value = _sod.Update((float)delta, TargetValue);

    if (SetterNode is not null) {
      GetNodeOrNull(SetterNode)?.Set(SetterProperty, value);
    }
  }

  /// <inheritdoc />
  public override string[] _GetConfigurationWarnings() {
    List<string> warnings = base._GetConfigurationWarnings()?.ToList() ?? [];

    if (Params is null) {
      warnings.Add("SodAnimator requires Params to be set to function");
    }

    return warnings.ToArray();
  }

  /// <inheritdoc />
  public override void _ValidateProperty(Dictionary property) {
    StringName name = property["name"].AsStringName();

    if (name == PropertyName.TargetValue) {
      property["type"] = (long)ValueType;
      return;
    }

    if (name == PropertyName.UsePhysicsProcess) {
      if (RunInEditor) {
        property["usage"] = (int)PropertyUsageFlags.NoEditor;
      }

      return;
    }

    if (name == PropertyName.SetterProperty) {
      Node? node = GetNodeOrNull(SetterNode);

      if (node is not null) {
        property["hint_string"] = string.Join(
          ",",
          node.GetPropertyList()
            .Select(x => x["name"]));
      }
      else {
        property["usage"] = (int)PropertyUsageFlags.NoEditor;
      }

      return;
    }

    base._ValidateProperty(property);
  }

  void _updateProcessMode() {
    SetProcess(!_usePhysicsProcess && IsRunning);
    SetPhysicsProcess(_usePhysicsProcess && IsRunning);
  }
}