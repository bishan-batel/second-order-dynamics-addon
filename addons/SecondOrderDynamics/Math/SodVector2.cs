#nullable enable

using Godot;

namespace SecondOrderDynamics.Math;

public class SodVector2 : SecondOrderDynamics<Vector2> {
  public SodVector2(float freq = 1, float zeta = 1, float response = 1, Vector2 x0 = default)
    : base(freq, zeta, response, x0) {
  }

  public SodVector2(SecondOrderDynamics.SodParams @params, Vector2 x0 = default)
    : base(@params, x0) {
  }

  public SodVector2() : this(1) {
  }

  public override Vector2 UpdateInternal(float delta, Vector2 x, Vector2? xd = null) {
    if (Params is null) return Y;

    if (xd is null) {
      xd = (x - XPrev) / delta;
      XPrev = x;
    }

    float k2Stable = Mathf.Max(Params.K2, Mathf.Max(delta * delta / 2f + delta * Params.K1 / 2f, delta * Params.K1));
    Y += delta * Yd;
    Yd += delta * (x + Params.K3 * (Vector2)xd - Y - Params.K1 * Yd) / k2Stable;
    return Y;
  }

  public override bool IsValid(Vector2 value) {
    return value.IsFinite();
  }
}