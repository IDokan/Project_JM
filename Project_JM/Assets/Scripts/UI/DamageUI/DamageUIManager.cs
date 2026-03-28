using UnityEngine;

public class DamageUIManager : MonoBehaviour
{
    public static DamageUIManager Instance;

    [SerializeField] protected DamageUI _damagePrefab;

    protected void Awake()
    {
        Instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public DamageUI SpawnDamage(int amount, AttackContext context, bool isCritical, float sizeMultiplier = 1f)
    {
        var dmg = Instantiate(_damagePrefab, transform);
        dmg.Show(amount, context, isCritical, sizeMultiplier);

        return dmg;
    }
}
