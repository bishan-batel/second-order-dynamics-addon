#if TOOLS
#nullable enable

using Godot;

namespace SecondOrderDynamics;

[Tool]
public partial class SecondOrderDynamicsPlugin : EditorPlugin {
  public static SecondOrderDynamicsPlugin Singleton { private set; get; } = null!;

  Editor.SodInspectorPlugin _inspectorPlugin = null!;

  public override void _EnterTree() {
    Singleton = this;

    _inspectorPlugin = new Editor.SodInspectorPlugin();
    AddInspectorPlugin(_inspectorPlugin);
  }

  public override void _ExitTree() {
    RemoveInspectorPlugin(_inspectorPlugin);
  }
}
#endif