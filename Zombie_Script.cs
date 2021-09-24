using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Zombie_Script : MonoBehaviour
{
    Rigidbody2D rb;
    public float umbralVelocidad;
    public float velocidad = 5f;

    int direccion = 1; //orientacion del zombie
    public float rango; //rango del patrullaje
    float limiteRangoIzq; //patrullaje izq
    float limiteRangoDer; //patrullaje der
    
    public float fuerzaCabeza = 5f; //La fuerza con la que sale volando

    public float distAtaque = 1f;
    public float entradaZonaPersecucion = 5f;
    public float salidaZonaPersecucion = 7f;

    public Animator anim;
    public Transform jugadorTR;
    public GameObject prefabMuerte;

    bool mordidaValida = false;
    
    float distJugador;
    public enum Comportamiento {Pasivo, Persecucion, Ataque};
    Comportamiento compZombie = Comportamiento.Pasivo;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        limiteRangoDer = transform.position.x + rango;
        limiteRangoIzq = transform.position.x - rango;
    }

    // Update is called once per frame
    void Update()
    {
        distJugador = Mathf.Abs(jugadorTR.position.x - transform.position.x);
        if (rb.velocity.magnitude < umbralVelocidad)
        {
            switch (compZombie)
            {
                case Comportamiento.Pasivo:
                    //Patrullando en su zona
                    if (transform.position.x > limiteRangoDer) direccion = -1;
                    if (transform.position.x < limiteRangoIzq) direccion = 1;
                    rb.velocity = new Vector2(velocidad * direccion, rb.velocity.y);
                    anim.speed = 1f;
                    //Cambio de comportamiento
                    if (distJugador < entradaZonaPersecucion) compZombie = Comportamiento.Persecucion;
                    break;
                case Comportamiento.Persecucion:
                    //Persigue al jugador (se mueve 50% mas rapido)
                    if (jugadorTR.position.x > transform.position.x) direccion = 1;
                    if (jugadorTR.position.x < transform.position.x) direccion = -1;
                    rb.velocity = new Vector2(velocidad * direccion * 1.5f, rb.velocity.y);
                    anim.speed = 1.5f;
                    //Cambio de comportamiento
                    //Pasivo
                    if (distJugador > salidaZonaPersecucion) compZombie = Comportamiento.Pasivo;
                    //Ataque
                    if (distJugador < distAtaque) compZombie = Comportamiento.Ataque;
                    break;
                case Comportamiento.Ataque:
                    anim.SetTrigger("Morder");
                    //Mira al jugador
                    if (jugadorTR.position.x > transform.position.x) direccion = 1;
                    if (jugadorTR.position.x < transform.position.x) direccion = -1;
                    anim.speed = 1f;
                    //Cambio de comportamiento
                    //Persecucion
                    if (distJugador > distAtaque)
                    {
                        compZombie = Comportamiento.Persecucion;
                        anim.ResetTrigger("Morder");
                    }
                    break;
            }
        }
        //Cambio de orientacion
        transform.localScale = new Vector3(0.2f * direccion, 0.2f, 1);

    }

    public void MuerteZombie(Vector3 direccion)
    {
        GameObject Muerto = Instantiate(prefabMuerte, transform.position, transform.rotation);
        Muerto.transform.localScale = transform.localScale;
        Muerto.transform.GetChild(0).GetComponent<Rigidbody2D>().AddForce(direccion * fuerzaCabeza, ForceMode2D.Impulse);
        Muerto.transform.GetChild(1).GetComponent<Rigidbody2D>().AddForce(direccion * (fuerzaCabeza/2), ForceMode2D.Impulse);
        Muerto.transform.GetChild(0).GetComponent<Rigidbody2D>().AddTorque(Random.Range(0f, 7f), ForceMode2D.Impulse);
        Muerto.transform.GetChild(1).GetComponent<Rigidbody2D>().AddTorque(Random.Range(0f, 7f), ForceMode2D.Impulse);
        Destroy(gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Jugador") && mordidaValida)
        {
            mordidaValida = false;
            jugadorTR.GetComponent <Movimiento_jugador>().RecibirMordida(collision.contacts[0].point);
        }
    }

    public void MordidaValida_inicio()
    {
        mordidaValida = true;
    }

    public void MordidaValida_fin()
    {
        mordidaValida = false;
    }
}
