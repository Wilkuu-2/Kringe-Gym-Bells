using UnityEngine;

namespace SRLegacy {
    public class RelativeFollower : MonoBehaviour
    {
#region fields
    Vector3 offset;
    [SerializeField] Transform target;
#endregion

        // Start is called before the first frame update
        void Start()
        {
            offset = transform.position - target.position;
        }

        // Update is called once per frame
        void Update()
        {
            transform.position = target.position + offset;
        }
    }
}
