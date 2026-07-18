#nullable enable

using Godot;

namespace SecondOrderDynamics;

[GlobalClass, Tool, Icon("res://addons/SecondOrderDynamics/Icon/SodParams.svg")]
public partial class SodParams : Resource {
  public float K1 { private set; get; }
  public float K2 { private set; get; }
  public float K3 { private set; get; }

  float _zeta = 1.0f, _freq = 1.0f, _response = 0.1f;

  [Export(PropertyHint.Range, "0,10,0.001,or_greater")]
  public float Frequency {
    set {
      _freq = value;

      _updateConstants();
    }
    get => _freq;
  }

  [Export(PropertyHint.Range, "0,5,0.001,or_greater")]
  public float Zeta {
    set {
      _zeta = value;
      _updateConstants();
    }
    get => _zeta;
  }

  [Export]
  public float Response {
    set {
      _response = value;
      _updateConstants();
    }
    get => _response;
  }


  public SodParams(float freq = 1, float zeta = 1, float response = 1) {
    SetDirectly(freq, zeta, response);
  }

  public SodParams() : this(1) {
  }

  public void SetDirectly(float freq, float zeta, float resp) {
    _freq = freq;
    _zeta = zeta;
    _response = resp;
    _updateConstants();
  }

  void _updateConstants() {
    const float tau = Mathf.Pi * 2f;
    K1 = Zeta / (Mathf.Pi * Frequency);
    K2 = 1f / (tau * Frequency * tau * Frequency);
    K3 = Response * Zeta / (tau * Frequency);
  }
}