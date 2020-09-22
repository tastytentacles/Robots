using Godot;
using System;

public class BotMind : KinematicBody {
    private int mode = 0;
    private float tick = 0;
    private Random rng;
    
    private Control mini_gui;
    private Label text;

    private Vector3 target_point = new Vector3();

    public void pressed(
            Camera cam,
            InputEvent e,
            Vector2 pos,
            Vector3 norm,
            CollisionShape box) {

        if (e is InputEventMouseButton em) {
            if (em.Pressed) {
                mini_gui.Visible = true;
                mini_gui.RectPosition = GetViewport().GetMousePosition() - new Vector2(64, 64);
            }
        }
    }

    // public void b_l() {
    //     target_point += Vector3.Left;
    // }

    // public void b_r() {
    //     target_point += Vector3.Right;
    // }

    // public void b_u() {
    //     target_point += Vector3.Forward;
    // }

    // public void b_d() {
    //     target_point += Vector3.Back;
    // }

    public void button_null() {
        mode = 0;
    }

    public void button_roam() {
        if (mode == 0) {
            mode = 1; // roam state
        } else {
            mode = 0;
        }
    }

    public void roam() {
        var flip = Mathf.Floor((float) rng.NextDouble() * 4);

        switch (flip) {
            case 0:
                target_point += Vector3.Forward;
                break;
            
            case 1:
                target_point += Vector3.Left;
                break;
            
            case 2:
                target_point += Vector3.Back;
                break;

            case 3:
                target_point += Vector3.Right;
                break;
            
            default:
                break;
        }
    }

    public override void _Ready() {
        mini_gui = GetNode<Control>("bot_GUI");
        text = GetNode<Label>("bot_GUI/box_text");
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
        tick += delta;

        text.Text = "mode: " + mode;

        if (tick > 2.5) {
            switch (mode) {
                case 1:
                    roam();
                    break;

                default:
                    break;
            }

            tick = 0;
        }
    }

    public override void _PhysicsProcess(float delta) {
        if (Translation.DistanceTo(target_point) > 0.1f) {
            var hit = MoveAndCollide(target_point - Translation);

            if (hit != null) {
                target_point = Translation;
            } 
        }
    }
}
