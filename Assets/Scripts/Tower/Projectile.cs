using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 10f;
    public int damage = 25;
    public GameObject target;
    public bool explodeOnImpact = false;
    
    [Header("Visual Effects")]
    public GameObject explosionEffect;
    public TrailRenderer trail;
    
    private TowerEffects towerEffects;
    private Vector3 targetPosition;
    private bool hasExploded = false;
    
    public void Initialize(GameObject targetEnemy, float projectileSpeed, int projectileDamage, TowerEffects effects)
    {
        target = targetEnemy;
        speed = projectileSpeed;
        damage = projectileDamage;
        towerEffects = effects;
        
        if (target != null)
        {
            targetPosition = target.transform.position;
            
            // Rotiraj projektil prema cilju
            Vector3 direction = (targetPosition - transform.position).normalized;
            transform.right = direction;
        }
        
        // Uništi projektil nakon 5 sekundi ako ne pogodi ništa
        Destroy(gameObject, 5f);
    }
    
    void Update()
    {
        if (hasExploded) return;
        
        // Ažuriraj target poziciju ako cilj još postoji
        if (target != null)
        {
            targetPosition = target.transform.position;
        }
        
        // Pomijeri projektil prema cilju
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        // Provjeri da li je dostigao cilj
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        if (distanceToTarget < 0.1f)
        {
            Hit();
        }
    }
    
    private void Hit()
    {
        if (hasExploded) return;
        hasExploded = true;
        
        // Primijeni damage na target ako još postoji
        if (target != null)
        {
            Enemy enemy = target.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }
        
        // Primijeni tower efekte
        if (towerEffects != null)
        {
            towerEffects.ApplyEffects(target, transform.position);
        }
        
        // Vizuelni efekat eksplozije
        if (explosionEffect != null)
        {
            GameObject explosion = Instantiate(explosionEffect, transform.position, Quaternion.identity);
            Destroy(explosion, 2f);
        }
        
        // Uništi projektil
        Destroy(gameObject);
    }
    
    void OnTriggerEnter2D(Collider2D collision)
    {
        // Alternativni način detekcije kolizije
        if (collision.CompareTag("Enemy") && collision.gameObject == target)
        {
            Hit();
        }
    }
}