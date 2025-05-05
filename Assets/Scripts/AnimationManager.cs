using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance;

    public GameObject animationPrefab; // Assign your prefab with the animation here

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void PlayAnimation(Vector3 position)
    {
        if (animationPrefab == null)
        {
            Debug.LogError("animationPrefab is null!");
            return;
        }

        GameObject animObj = Instantiate(animationPrefab, position, Quaternion.identity);

        Animator animator = animObj.GetComponentInChildren<Animator>();
        if (animator == null)
        {
            Debug.LogError("Animator not found on the prefab!");
            Destroy(animObj, 2f);
            return;
        }

        float animLength = animator.GetCurrentAnimatorStateInfo(0).length;
        Destroy(animObj, animLength);
    }
}
