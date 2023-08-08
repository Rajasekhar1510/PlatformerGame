using UnityEngine;

public class GroundChecker: MonoBehaviour 
{
    [SerializeField] float groundDistance = 0.08f;
    [SerializeField] LayerMask groundLayers;


    public bool isGrounded { get; private set; }

    private void Update()
    {
        isGrounded = Physics.SphereCast(transform.position, groundDistance, Vector3.down, out _, groundDistance, groundLayers);
    }
}
