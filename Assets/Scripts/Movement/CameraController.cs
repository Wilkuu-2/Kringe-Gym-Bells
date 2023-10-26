using UnityEngine;
using UnityEngine.InputSystem; 

public class CameraController : MonoBehaviour
{
    private Vector3 offset; 
    private Vector2 input; 
    public  Vector2 sensitivity = new Vector2(10f, 0.1f);
    public  float   y_min = -6;
    public  float   y_max =  6;
    public  Transform parent, lookpoint, rotation;
    public InputActionReference action; 
    
    // Start is called before the first frame update
    void Start()
    {
        offset = transform.position - parent.transform.position; 
    }

    // Update is called once per frame
    void Update()
    {
       input = action.action.ReadValue<Vector2>();
       transform.position = parent.transform.position + offset;

       lookpoint.localPosition = new Vector3(
                                lookpoint.localPosition.x, 
                                lookpoint.localPosition.y + input.y * sensitivity.y * Time.deltaTime,
                                lookpoint.localPosition.z);
       rotation.LookAt(lookpoint.position,Vector3.up); 
       rotation.eulerAngles = new Vector3(rotation.rotation.eulerAngles.x,rotation.eulerAngles.y,0);     

       transform.Rotate(Vector3.up, input.x * sensitivity.x * Time.deltaTime);

    }

}
