#if TOOLS
#nullable enable

using Godot;
using SecondOrderDynamics.Math;

namespace SecondOrderDynamics.Editor;

[Tool]
public partial class SodInspectorPlugin : EditorInspectorPlugin {
  public override bool _CanHandle(GodotObject @object) {
    return true;
  }

  public override bool _ParseProperty(
    GodotObject @object,
    Variant.Type type,
    string name,
    PropertyHint hintType,
    string hintString,
    PropertyUsageFlags usageFlags,
    bool wide
  ) {
    if (type != Variant.Type.Object) return false;

    if (@object.Get(name).AsGodotObject() is SodParams) {
      AddCustomControl(new SodSingleEditor(@object, name));
    }
    else if (hintType == PropertyHint.ResourceType && hintString == nameof(SodParams)) {
      AddCustomControl(new SodSingleEditor(@object, name));
    }


    return false;
  }
}
#endif