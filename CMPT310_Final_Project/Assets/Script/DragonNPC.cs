using UnityEngine;

public class DragonNPC : MonoBehaviour
{

    public Transform target;

    private Rigidbody rb;

    public float walkSpeed = 1;
    // public ForceMode walkForceMode;
    private Vector3 dirToGo;

    // private Vector3 m_StartingPos;
    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // m_StartingPos = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate()
    {
    }

    public void SetRandomWalkSpeed()
    {
        walkSpeed = Random.Range(1f, 7f);
    }
}
