#nullable enable

using Godot;

namespace SecondOrderDynamics.Math;

/// <summary>
/// Second Order System operating on a single number
/// </summary>
public class SodFloat : SecondOrderDynamics<float> {
  public SodFloat(float freq = 1, float zeta = 1, float response = 1, float x0 = 0)
    : base(freq, zeta, response, x0) {
  }

  public SodFloat(SecondOrderDynamics.SodParams @params, float x0 = 0)
    : base(@params, x0) {
  }

  public override float UpdateInternal(float delta, float x, float? xd = null) {
    if (Params is null) return Y;

    if (xd is null) {
      xd = (x - XPrev) / delta;
      XPrev = x;
    }

    float k2Stable = Mathf.Max(Params.K2, Mathf.Max(delta * delta / 2f + delta * Params.K1 / 2f, delta * Params.K1));
    Y += delta * Yd;
    Yd += delta * (x + Params.K3 * (float)xd - Y - Params.K1 * Yd) / k2Stable;
    return Y;
  }

  public override bool IsValid(float value) {
    return float.IsFinite(value);
  }
}