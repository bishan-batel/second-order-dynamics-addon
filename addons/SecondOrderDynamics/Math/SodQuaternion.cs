#nullable enable

using Godot;

namespace SecondOrderDynamics.Math;

/// <summary>
/// Second Order System operating on a quaternion
/// </summary>
public class SodQuaternion : SecondOrderDynamics<Quaternion> {
  public SodQuaternion(float freq = 1, float zeta = 1, float response = 1, Quaternion x0 = default)
    : base(freq, zeta, response, x0) {
  }

  public SodQuaternion(SecondOrderDynamics.SodParams @params, Quaternion x0 = default)
    : base(@params, x0) {
  }

  public SodQuaternion() : this(1) {
  }

  /// <inheritdoc />
  public override Quaternion UpdateInternal(float delta, Quaternion x, Quaternion? xd = null) {
    if (Params is null) return Y;

    if (xd is null) {
      xd = (x - XPrev) / delta;
      XPrev = x;
    }

    float k2Stable = Mathf.Max(Params.K2, Mathf.Max(delta * delta / 2f + delta * Params.K1 / 2f, delta * Params.K1));
    Y += delta * Yd;
    Yd += delta * (x + Params.K3 * (Quaternion)xd - Y - Params.K1 * Yd) / k2Stable;
    return Y;
  }

  /// <inheritdoc />
  public override bool IsValid(Quaternion value) {
    return value.IsFinite();
  }
}