using UnityEngine;

public class door : MonoBehaviour
{
    [SerializeField] private float finalYRotation;

    private float initialYRotation;
    private bool isRotated = false;
    [SerializeField] private bool playerInCollider = false;
    [SerializeField] private AudioSource opening;
    [SerializeField] private AudioSource closing;

    // Start is called before the first frame update
    void Start()
    {
        initialYRotation = transform.eulerAngles.y;

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C) && (playerInCollider == true))
        {
            if (isRotated)
            {
                Rotation(initialYRotation);
                opening.gameObject.GetComponent<AudioSource>().enabled = false;
                closing.gameObject.GetComponent<AudioSource>().enabled = true;
                closing.Play();
            }
            else
            {
                Rotation(finalYRotation);
                closing.gameObject.GetComponent<AudioSource>().enabled = false;
                opening.gameObject.GetComponent<AudioSource>().enabled = true;
                opening.Play();
            }
            isRotated = !isRotated;


        }
    }

    public void Rotation(float yRotation)
    {
        Vector3 currentRotation = transform.eulerAngles;

        currentRotation.y = yRotation;

        transform.eulerAngles = currentRotation;
    }
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInCollider = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInCollider = false;
        }
    }
}
