using Godot;
using System;

public class GUIMind : Control {
    public void mimic_esc() {
        var e = new InputEventKey();
        e.Scancode = (uint) KeyList.Escape;
        Input.ParseInputEvent(e);
    }
}
