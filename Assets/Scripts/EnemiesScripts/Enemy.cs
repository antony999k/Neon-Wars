﻿using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField]
    protected bool useInspectorParams;

    [Header("Enemy Status")]
    public float speed;
    public int enemyScore;
    private bool isRespawning;
    public float respawnTime;

    [Header("Enemy Editors")]
    public GameObject explotion;
    public GameObject floatingPoints;
    public GameObject floatingMultiplier;

    protected Rigidbody2D rb;
    private Transform playerTarget;
    private Vector2 movement;
    private PolygonCollider2D pCollider;

    protected virtual void Start()
    {
        //Obtiene el colider del objeto y lo desabilita
        pCollider = GetComponent<PolygonCollider2D>();
        pCollider.isTrigger = true;
        //Esta reapareciando cuando se iniciañliza el enemigo
        isRespawning = true;
        //Añade enemigo al contador global
        GameManager.instance.aliveEnemies += 1;
        rb = GetComponent<Rigidbody2D>();
        playerTarget = GameObject.FindGameObjectWithTag("Player").transform;
    }

    protected virtual void Update()
    {
        Vector3 direction = playerTarget.position - transform.position;
        direction.Normalize();
        movement = direction;

        RespawnTimer();
    }

    protected virtual void FixedUpdate()
    {
        MoveEnemy(movement);
    }

    void MoveEnemy(Vector2 direction)
    {
        if (!isRespawning)
        {
            rb.MovePosition((Vector2)transform.position + (direction * speed * Time.deltaTime));
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Bullet" && isRespawning)
        {
            DestroyEnemy(true);
            SendToGameManager();
        } 
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Bullet" || (collision.gameObject.tag == "Player" && GameManager.instance.hasShieldInmunity))
        {
            if (GameManager.instance.hasShieldInmunity)
            {
                SoundManager.instance.PlaySound("ShieldHitEnemy");
            }

            DestroyEnemy(true);
            SendToGameManager();
        }
    }

    void RespawnTimer()
    {
        if (respawnTime <= 0)
        {
            pCollider.isTrigger = false;
            isRespawning = false;
        }
        else
        {
            respawnTime -= Time.deltaTime;
        }
    }

    public virtual void DestroyEnemy(bool destroyByPlayer = false)
    {
        //Explotion particle system play
        GameObject cloneExplotion = Instantiate(explotion, transform.position, Quaternion.identity);
        Destroy(cloneExplotion, 0.8f);

        //Intancia el score que da el enemigo si fue destruido por el jugador
        if (destroyByPlayer)
        {
            //AddComponentMenu sound
            SoundManager.instance.PlaySound("EnemyExplode");
            //Add Floating points
            PopFloatingPoints();
            if (GameManager.instance.multiplayerChanged)
            {
                PopMultiplierPoints();
            }
        }

        Destroy(gameObject);
    }

    private void PopFloatingPoints()
    {
        GameObject cloneFloatingPoints = Instantiate(floatingPoints, transform.position, Quaternion.identity);
        int totalEnemyScore = enemyScore * GameManager.instance.multiplier;
        cloneFloatingPoints.transform.GetChild(0).GetComponent<TextMeshPro>().SetText(totalEnemyScore.ToString());
        Destroy(cloneFloatingPoints, 1f);
    }

    private void PopMultiplierPoints()
    {
        // Play Multiplier sound
        SoundManager.instance.PlaySound("Multiplier");
        //Instanciate the UI
        GameObject cloneFloatingPoints = Instantiate(floatingMultiplier, new Vector3(transform.position.x, transform.position.y - 0.2f, transform.position.z), Quaternion.identity);
        cloneFloatingPoints.transform.GetChild(0).GetComponent<TextMeshPro>().SetText("MULTIPLIER X" + GameManager.instance.multiplier.ToString());
        Destroy(cloneFloatingPoints, 1.3f);
        GameManager.instance.multiplayerChanged = false;
    }

    void SendToGameManager()
    {
        //Add points to player score
        GameManager.instance.aliveEnemies -= 1;
        GameManager.instance.score += enemyScore * GameManager.instance.multiplier;
        GameManager.instance.scoreToMultiplier += enemyScore * GameManager.instance.multiplier;
    }
}
