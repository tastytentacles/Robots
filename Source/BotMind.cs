using Godot;
using System;

using System.Collections.Generic;



public class BotMind : KinematicBody {
    private int mode = 0;
    private float tick = 0;
    private bool move_flag;
    private Random rng;
    
    private Control mini_gui;
    private Navigation nav;
    private ImmediateGeometry debug;
    private Label text;
    private Spatial goals;
    private int step_back_mode;
    private Vector3 step_back_normal;

    private List<Vector3> target_path = new List<Vector3>();
    private Vector3 target_point = new Vector3();

    public Dictionary<string, int> have_list = new Dictionary<String, int>();


    public void pressed(
            Camera cam,
            InputEvent e,
            Vector2 pos,
            Vector3 norm,
            CollisionShape box) {

        if (e is InputEventMouseButton em) {
            if (em.Pressed) {
                InputEventKey clear_others = new InputEventKey();
                clear_others.Scancode = (uint) KeyList.Escape;
                Input.ParseInputEvent(clear_others);
                
                mini_gui.Visible = true;
                mini_gui.RectPosition = GetViewport().GetMousePosition() - mini_gui.RectSize / 2f;
            }
        }
    }


    // button functions
    public void button_null() {
        mode = 0;
    }

    public void button_roam() {
        if (mode != 1) {
            mode = 1; // roam state
        } else {
            mode = 0;
        }
    }

    public void button_go_get() {
        if (mode != 2) {
            mode = 2; // go get state
        } else {
            mode = 0;
        }
    }


    // state functions
    public void roam() {        
        var point = nav.GetClosestPoint(
            new Vector3(
                (float) rng.NextDouble() * 40 - 20,
                (float) rng.NextDouble() * 40 - 20,
                (float) rng.NextDouble() * 40 - 20)
        );
        var path = nav.GetSimplePath(Translation, point);

        GD.Print(path.Length);
        debug.Clear();
        debug.Begin(Mesh.PrimitiveType.LineStrip);
        foreach (Vector3 n in path) {
            debug.AddVertex(n);
        }
        debug.End();

        target_path = new List<Vector3>(path);
        move_flag = true;
    }

    public void go_get(string trg_tag = "star") {
        var look = new List<Spatial>();
        
        foreach (Spatial n in goals.GetChildren()) {
            if (n.IsInGroup(trg_tag)) {
                look.Add(n);
            }
        }

        if (look.Count == 0) {
            mode = 1;
            return;
        }

        var pick = look[(int) Mathf.Floor((float) rng.NextDouble() * look.Count)];
        var point = nav.GetClosestPoint(pick.GlobalTransform.origin);
        var path = nav.GetSimplePath(Translation, point);

        GD.Print(path.Length);
        debug.Clear();
        debug.Begin(Mesh.PrimitiveType.LineStrip);
        foreach (Vector3 p in path) {
            debug.AddVertex(p);
        }
        debug.End();

        target_path = new List<Vector3>(path);
        move_flag = true;
    }

    public void step_back() {
        if (step_back_normal == null) {
            mode = 0;
            return;
        }

        var point = nav.GetClosestPoint(GlobalTransform.origin - step_back_normal);
        var path = nav.GetSimplePath(Translation, point);

        GD.Print(path.Length);
        debug.Clear();
        debug.Begin(Mesh.PrimitiveType.LineStrip);
        foreach (Vector3 p in path) {
            debug.AddVertex(p);
        }
        debug.End();

        target_path = new List<Vector3>(path);
        move_flag = true;
    }

    // logic functions
    public void drop_path(KinematicCollision hit_data = null) {
        target_point = Translation;
        target_path = new List<Vector3>();

        var hit_obj = hit_data.Collider as Spatial;
        step_back_normal = (hit_obj.GlobalTransform.origin - GlobalTransform.origin).Normalized();
        step_back_mode = mode;
        mode = 3;

        debug.Clear();

        GD.Print("impact while moving");
        if (hit_data != null) {
            GD.Print("data: " + hit_data.ToString());
        }
    }

    // core logic
    public override void _Ready() {
        mini_gui = GetNode<Control>("bot_GUI");
        mini_gui.Visible = false;
        
        text = GetNode<Label>("bot_GUI/box_text");

        nav = GetTree().Root.GetNode<Navigation>("Spatial/Navigation");
        debug = GetNode<ImmediateGeometry>("debug_line");
        RemoveChild(debug);
        GetTree().Root.CallDeferred("add_child", debug);

        goals = GetTree().Root.GetNode<Spatial>("Spatial/goals");

        target_point = Translation;
        rng = new Random();
    }

    public override void _Input(InputEvent e) {
        if (e is InputEventKey eb) {
            if (eb.Scancode == (int) KeyList.Escape) {
                mini_gui.Visible = false;
            }
        }
    }

    public override void _Process(float delta) {
        if (!move_flag) {
            tick += delta;
        }

        text.Text = "mode: " + mode;

        if (tick > 3) {
            switch (mode) {
                case 1:
                    roam();
                    break;

                case 2:
                    go_get();
                    break;

                case 3:
                    step_back();
                    break;

                default:
                    break;
            }

            tick = 0;
        }
    }

    public override void _PhysicsProcess(float delta) {
        if (Translation.DistanceTo(target_point) > 0.1f) {
            var step = (target_point - Translation).Normalized() * 4 * delta;
            var hit = MoveAndCollide(step);

            if (hit != null) {
                drop_path(hit);
            } 
        } else {
            if (target_path.Count > 0) {
                target_point = target_path[0];
                target_path.RemoveAt(0);

                if (target_path.Count == 0 && mode == 3) {
                    mode = step_back_mode;
                }
            } else if (move_flag) {
                move_flag = false;
                // if (mode == 3) {
                //     mode = step_back_mode;
                // }
            }
        }
    }
}
