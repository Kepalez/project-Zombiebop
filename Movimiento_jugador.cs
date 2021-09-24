using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Movimiento_jugador : MonoBehaviour
{
    private bool tieneArma = false;
    private float valorTelonAlfa = 0;
    public float fuerzaSalto = 5f;
    public float Velocidad = 5f;
    public float recoilArma = 5f;
    public float magnitudReaccionDisparo;
    public float vidaMax = 5f;
    public float Vida = 5f;
    private bool enPiso;
    private float magnitudSacudida;
    Vector2 movimiento;
    Animator anim;
    Rigidbody2D rb;
    public Transform refPie;
    public Transform contarma;
    public Transform mira;
    public Transform refManoArma;
    public Transform refOjos;
    public Transform cabeza;
    public Transform canonPistola;
    public Transform contenedorVida;
    public GameObject particulasArma;
    public GameObject particulasSangre;
    public GameObject particulasMasSangre;
    public Transform camaraSacudir;
    public Image MascaraSangre;
    public Image Telon;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        int i = 0;
        foreach (RectTransform vidaIcono in contenedorVida)
        {
            i++;
            vidaIcono.anchoredPosition = new Vector2(i * 25, 0);
        }

        Telon.color = new Color(0, 0, 0, 1);
        valorTelonAlfa = 0;
    }

    void Update()
    {
        if (Vida <= 0) return;
        float velx = Input.GetAxis("Horizontal");
        anim.SetFloat("Velocidad", Mathf.Abs(velx));
        rb.velocity = new Vector2(velx * Velocidad, rb.velocity.y);
        enPiso = Physics2D.OverlapCircle(refPie.position, 1f, 1 << 8);
        anim.SetBool("Grounded", enPiso);

        //Salto
        if (Input.GetButtonDown("Jump") && enPiso)
        {
            rb.AddForce(new Vector2(0, fuerzaSalto), ForceMode2D.Impulse);
        }

        //Orientación personaje
        if(tieneArma)
        {
            if (mira.transform.position.x < transform.position.x) transform.localScale = new Vector3(-0.2f, 0.2f, 1f);
            if (mira.transform.position.x > transform.position.x) transform.localScale = new Vector3(0.2f, 0.2f, 1f);
            //Posicion mira y mouse
            mira.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
            if (Input.GetButtonDown("Fire1")) Disparar();
        }
        else
        {
            if (velx < 0) transform.localScale = new Vector3(-0.2f, 0.2f, 1f);
            if (velx > 0) transform.localScale = new Vector3(0.2f, 0.2f, 1f);
        }
        

        if (Input.GetKeyDown(KeyCode.P))
        {
            Vida = 1;
            RecibirMordida(new Vector2(0, 0));
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            valorTelonAlfa = 0;
        }

    }

    private void FixedUpdate()
    {
        if (magnitudSacudida > 0.1f)
        {
            camaraSacudir.rotation = Quaternion.Euler(0, 0, Random.Range(magnitudSacudida, -magnitudSacudida));
            magnitudSacudida *= 0.9f;
        }
        else
        {
            camaraSacudir.rotation = Quaternion.Euler(0, 0, 0);
        }
        ActualizarDisplay();

        float valorAlfa = Mathf.Lerp(Telon.color.a, valorTelonAlfa, 0.05f);
        Telon.color = new Color(0, 0, 0, valorAlfa);
        if(valorAlfa > 0.9f && valorTelonAlfa == 1)
        {
            SceneManager.LoadScene("Scenes/SampleScene");
        }
    }

    private void LateUpdate()
    {
        if (Vida <= 0) return;
        if (tieneArma)
        {
            refManoArma.position = mira.position;
            cabeza.up = refOjos.position - mira.position;
            contarma.up = contarma.position - mira.position;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Arma"))
        {
            tieneArma = true;
            Destroy(collision.gameObject);
            contarma.gameObject.SetActive(true);
        }
    }

    void Disparar()
    {
        Vector3 direccionArma = (mira.position - contarma.position).normalized;

        rb.AddForce(recoilArma * -direccionArma, ForceMode2D.Impulse);
        Instantiate(particulasArma, canonPistola.position, Quaternion.identity);
        SacudirCamara(recoilArma);
        RaycastHit2D hit = Physics2D.Raycast(contarma.position, direccionArma, 1000f, ~(1 << 10));
        if(hit.collider != null)
        {
            if(hit.collider.CompareTag("Zombie"))
            {
                hit.rigidbody.AddForce(magnitudReaccionDisparo * direccionArma, ForceMode2D.Impulse);
                Instantiate(particulasSangre, hit.point, Quaternion.identity);
            }
            if (hit.collider.CompareTag("CabezaZombie"))
            {
                Instantiate(particulasMasSangre, hit.point, Quaternion.identity);
                hit.transform.GetComponent<Zombie_Script>().MuerteZombie(direccionArma);
            }
        }

    }

    void SacudirCamara(float maximo)
    {
        magnitudSacudida = maximo;
    }

    public void RecibirMordida(Vector2 posicionMordida)
    {
        
        Instantiate(particulasSangre, posicionMordida, Quaternion.identity);
        Vida--;
        ActualizarDisplay();
        Debug.Log(Vida);
        if (Vida == 0)
        {
            tieneArma = false;
            anim.SetTrigger("Muere");
        }else
        {
            anim.SetTrigger("Mordido");
        }
    }

    void ActualizarDisplay()
    {
        float valorAlfa = 1 / vidaMax * (vidaMax - Vida);
        MascaraSangre.color = new Color(1, 1, 1, valorAlfa);

        int i = 0;
        Color colorVida;
        foreach (RectTransform vidaicono in contenedorVida)
        {
            i++;
            if (Vida >= i) colorVida = new Color(0f, 0.75f, 0.1f);
            else colorVida = new Color(0.85f, 0.15f, 0f);
            vidaicono.GetComponent<Image>().color = colorVida;
        }
    }
    
    public void IniciarFadeIn()
    {
        valorTelonAlfa = 1;
    }


}
