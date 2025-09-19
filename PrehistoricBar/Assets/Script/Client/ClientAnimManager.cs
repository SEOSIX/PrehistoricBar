using Unity.VisualScripting;
using UnityEngine;

public class ClientAnimManager : MonoBehaviour
{
    public Animation animation;
    public Animator animator;

    private void Start()
    {
        animator.SetTrigger("trgEnter");
    }
    
    public void LeaveBar()
    {
        animator.SetTrigger("trgExit");
    }

    public void OutOfTheBar()
    {
        Destroy(gameObject);
    }

    public void ServeCocktail(bool validate)
    {
        if (validate) animator.SetTrigger("trgValidate");
        else animator.SetTrigger("trgRefuse");
    }
}
