using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Firework : MonoBehaviour
{
    GameObject flare;
    GameObject sparkles;
    public bool stopFun = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void Play()
    {
        stopFun = false;
        StartCoroutine(BANGPARAPARA());
    }

    IEnumerator BANGPARAPARA()
    {
        float size = Random.Range(1f, 3f);
        GameObject f = Instantiate(flare, transform);
        ParticleSystem p = f.GetComponent<ParticleSystem>();
        p.startSize = size;
        p.Play();
        yield return new WaitForSeconds(2f);
        Destroy(f);

        GameObject s = Instantiate(sparkles, transform);
        p = s.GetComponent<ParticleSystem>();
        p.startSize = size;
        p.Play();
        yield return new WaitForSeconds(Random.Range(3f, 6f));
        Destroy(s);
        if (!stopFun)
            BANGPARAPARA();
    }
}
