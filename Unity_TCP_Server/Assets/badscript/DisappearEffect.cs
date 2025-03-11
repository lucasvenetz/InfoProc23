using UnityEngine;

public class DisappearEffect : MonoBehaviour
{
    public float disappearDuration = 2f;

    private Renderer objectRenderer; 
    public Renderer objectRenderer2; 
    public Renderer objectRenderer3; 
    public Renderer objectRenderer4; 
    public Renderer objectRenderer5; 
    private Material material; 
    private Material material2; 
    private Material material3; 
    private Material material4; 
    private Material material5; 
    private Color C1;
    public bool isFade = false;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        material = objectRenderer.material;
        material2 = objectRenderer2.material;
        material3 = objectRenderer3.material;
        material4 = objectRenderer4.material;
        material5 = objectRenderer5.material;
        C1 = material.color;
    }

    public void fade(){
        isFade = true;
    }

    public void reborn(){
        isFade = false;
    }

    void Update()
    {
        if (isFade)
        {
            StartCoroutine(Disappear());
        }
        else{
            material.color = C1;
            material2.color = C1;
            material3.color = C1;
            material4.color = C1;
            material5.color = C1;
        }
    }

    System.Collections.IEnumerator Disappear()
    {
        float elapsedTime = 0f;
        Color color = material.color;

        while (elapsedTime < disappearDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / disappearDuration); 
            color.a = alpha;
            material.color = color;
            material2.color = color;
            material3.color = color;
            material4.color = color;
            material5.color = color;
            yield return null;
        }
    }
}