#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using SecondOrderDynamics.Math;

namespace SecondOrderDynamics.Animator;

[GlobalClass, Tool, Icon("res://addons/SecondOrderDynamics/Icons/SodAnimator.svg")]
public partial class SodAnimator : Node {
  public enum SupportedType : long {
    Float = Variant.Type.Float,
    Vector2 = Variant.Type.Vector2,
    Vector3 = Variant.Type.Vector3,
    Quaternion = Variant.Type.Quaternion,
  }

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

  [ExportSubgroup("Simulation")]
  [Export]
  public Variant TargetValue { set; get; }

  public Variant CurrentValue => _sod?.Y ?? default;

  SodVariant? _sod;

  [Export]
  public bool UsePhysicsProcess {
    set {
      _usePhysicsProcess = value;
      SetProcess(!_usePhysicsProcess);
      SetPhysicsProcess(_usePhysicsProcess);
    }
    get => _usePhysicsProcess;
  }

  [Export] public bool RunInEditor { set; get; }

  [Export] public bool IsRunning { set; get; } = true;

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

  [Export(PropertyHint.EnumSuggestion)] public StringName SetterProperty { set; get; }


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
  bool _usePhysicsProcess = true;
  SupportedType _valueType = SupportedType.Float;
  NodePath? _setterNode = null;

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

  public override void _Process(double delta) {
    if (_usePhysicsProcess) return;
    _update(delta);
  }

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

  public override string[] _GetConfigurationWarnings() {
    List<string> warnings = base._GetConfigurationWarnings()?.ToList() ?? [];

    if (Params is null) {
      warnings.Add("SodAnimator requires Params to be set to function");
    }

    return warnings.ToArray();
  }

  public override void _ValidateProperty(Dictionary property) {
    StringName name = property["name"].AsStringName();

    if (name == PropertyName.TargetValue) {
      property["type"] = (long)ValueType;
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

  public override bool _Set(StringName property, Variant value) {
    return base._Set(property, value);
  }

  public override Variant _Get(StringName property) {
    return base._Get(property);
  }
}