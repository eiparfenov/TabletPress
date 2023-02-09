using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class PlayerMovement:MonoBehaviour
{
    [SerializeField] private SteamVR_Action_Vector2 joystick;
    [SerializeField] private float speed;
    private void Update()
    {
        var direction = Player.instance.hmdTransform.TransformDirection(
            new Vector3(joystick.GetAxis(SteamVR_Input_Sources.LeftHand).x, 
                0,
                joystick.GetAxis(SteamVR_Input_Sources.LeftHand).y)
            );
        transform.position += direction * speed * Time.deltaTime;
    }
}