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

    public void SpawnDamage(int amount, Vector3 worldPosition)
    {
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition) - transform.position;

        var dmg = Instantiate(_damagePrefab, transform);
        dmg.Show(amount, screenPos);
    }
}
