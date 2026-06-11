using UnityEngine;

public class PlayerInput : MonoBehaviour
{

    // void Awake()
    // {
    //     DontDestroyOnLoad(this.gameObject);
    // }
    public float HorizontalInput { get; private set; }
    public bool JumpPressed { get; private set; }
    public bool InteractPressed { get; private set; }
    public bool JumpReleased { get; private set; }
    public bool RollPressed { get; private set; }
    public bool LightAttackPressed {get; private set;}
    public bool HeavyAttackPressed {get; private set;}


    private void Update()
    {
        InteractPressed = Input.GetKeyDown(KeyCode.E);
        JumpPressed = Input.GetButtonDown("Jump");
        JumpReleased = Input.GetButtonUp("Jump");
        RollPressed = Input.GetKeyDown(KeyCode.LeftShift);
        LightAttackPressed = Input.GetMouseButtonDown(0);
        HeavyAttackPressed = Input.GetMouseButtonDown(1);

        HorizontalInput = Input.GetAxisRaw("Horizontal");
    }
    public void ConsumeRoll()
    {
        RollPressed = false;
    }
}