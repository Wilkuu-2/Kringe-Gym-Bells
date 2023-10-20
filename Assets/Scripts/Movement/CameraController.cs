using UnityEngine;
using UnityEngine.InputSystem; 

[RequireComponent(typeof(PlayerInput))]
public class CameraController : MonoBehaviour
{
    private Vector3 offset; 
    private Vector2 input; 
    public  Vector2 sensitivity = new Vector2(10f, 0.1f);
    public  Transform parent, lookpoint;
    
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - parent.transform.position; 
    }

    // Update is called once per frame
    void Update()
    {
       transform.position = parent.transform.position + offset;

       lookpoint.localPosition = new Vector3(
                                lookpoint.localPosition.x, 
                                lookpoint.localPosition.y + input.y * sensitivity.y * Time.deltaTime,
                                lookpoint.localPosition.z);
        
       transform.Rotate(Vector3.up, input.x * sensitivity.x * Time.deltaTime);

    }

    public void OnLook(InputValue l) {
        input = l.Get<Vector2>();
    } 


}
