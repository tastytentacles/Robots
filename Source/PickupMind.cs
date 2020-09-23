using Godot;
using System;

public class PickupMind : Area {
    [Export] public string tag;
    
    private CSGMesh mesh;

    public void steped_on(PhysicsBody b) {
        if (b is BotMind bm) {
            GD.Print(bm.have_list);

            // var first_tag = (string) this.GetGroups()[0]);
            if (!bm.have_list.ContainsKey(tag)) {
                bm.have_list.Add(tag, 1);
            } else {
                bm.have_list[tag] += 1;
            }

            GetParent().CallDeferred("remove_child", this);
            // GetParent().RemoveChild(this);
            QueueFree();
        }
    }

    public override void _Ready() {
        AddToGroup(tag);

        mesh = GetNode<CSGMesh>("CSGMesh");
    }

    public override void _Process(float delta) {
        mesh.RotateZ(2 * delta);
    }
}
