using UnityEngine;

public class ItemOrientation : MonoBehaviour
{

    private Animator IrisAnimator;
    

    // Update is called once per frame
    void Awake()
    {
        IrisAnimator= transform.parent.GetComponent<Animator>();
    }
    void Update()
    {
        
        AnimatorStateInfo stateInfo = IrisAnimator.GetCurrentAnimatorStateInfo(0);

        SpriteRenderer Iris=transform.parent.GetComponent<SpriteRenderer>();
        SpriteRenderer item =GetComponent<SpriteRenderer>();

        if (Iris.flipX == true)
        {
            item.flipX=true;
        }
        else
        {
            item.flipX=false;
        }
        
    }
}
