using UnityEngine;

public class followPlayer : MonoBehaviour
{
    public Transform player;
    public float speed = 1;
    private Input input;
    private Vector2 leftR;
    private Vector2 rightR;


    private void Awake()
    {
        input = new Input();
        input.Gameplay.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        leftR = input.Gameplay.LeftRotation.ReadValue<Vector2>();
        rightR = input.Gameplay.RightRotation.ReadValue<Vector2>();
        
        float x = (leftR.x + rightR.x) / 2;
        float y = (leftR.y + rightR.y) / 2;

        Vector3 newPos =  new Vector3(player.position.x+x*10, player.position.y+y*10, -10f);
        transform.position = Vector3.Lerp(transform.position, newPos, speed*Time.deltaTime);
    }
}
