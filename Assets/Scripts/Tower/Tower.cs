using UnityEngine;

public class Tower : MonoBehaviour
{
    [Header("Tower Stats")]
    public float Range = 8f;
    public float FireRate = 1f;
    public int Damage = 25;

    [Header("Targeting Mode")]
    public bool First = true;
    public bool Last = false;
    public bool Strongest = false;

    [NonSerialized]
    public GameObject Target;
    private float CoolDown = 0f;

    void Update()
    {
        if(Target)
        {
            if(CoolDown >= FireRate)
            {
                transform.right = Target.transform.position - transform.position;
                Target.GetComponent<Enemy>().TakeDamage(Damage);
                CoolDown = 0f;
            }
            else
            {
                CoolDown += 1f * Time.deltaTime;
            }
        }
    }
    
}
