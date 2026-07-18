#nullable enable
using Godot;

namespace SecondOrderDynamics.Math;

public class SodVector3 : SecondOrderDynamics<Vector3> {
  public SodVector3(float freq = 1, float zeta = 1, float response = 1, Vector3 x0 = default)
    : base(freq, zeta, response, x0) {
  }

  public SodVector3(SecondOrderDynamics.SodParams @params, Vector3 x0 = default)
    : base(@params, x0) {
  }

  public SodVector3() : this(1) {
  }

  public override Vector3 UpdateInternal(float delta, Vector3 x, Vector3? xd = null) {
    if (Params is null) return Y;

    if (xd is null) {
      xd = (x - XPrev) / delta;
      XPrev = x;
    }

    float k2Stable = Mathf.Max(Params.K2, Mathf.Max(delta * delta / 2f + delta * Params.K1 / 2f, delta * Params.K1));
    Y += delta * Yd;
    Yd += delta * (x + Params.K3 * (Vector3)xd - Y - Params.K1 * Yd) / k2Stable;
    return Y;
  }

  public override bool IsValid(Vector3 value) {
    return value.IsFinite();
  }
}