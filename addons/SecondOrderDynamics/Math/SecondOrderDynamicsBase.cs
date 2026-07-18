using System;
using System.Diagnostics.CodeAnalysis;
using Godot;

#nullable enable

namespace SecondOrderDynamics.Math;

[SuppressMessage("ReSharper", "MemberCanBeProtected.Global")]
public abstract class SecondOrderDynamicsBase {
  [Export] public SodParams? Params { set; get; }

  public abstract bool IsValidTypeErased(object? obj);

  public abstract object? UpdateTypeErased(float delta, object x, object? xd = null);
}