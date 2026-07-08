using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public float zPos;

    public string itemTag;

    public SpriteRenderer icon_Open;
    public SpriteRenderer icon_Close;

    [SerializeField] private float normalScale = 1f;
    [SerializeField] private float hoverScale = 1.1f;

    [SerializeField] private float normalGlow = 0f;
    [SerializeField] private float hoverGlow = 2f;

    [SerializeField] private float speed = 8f;

    protected virtual void Start()
    {
        icon_Open.material.SetColor("_OutlineColor", new Color(1, 1, 0, 0));
        icon_Close.material.SetColor("_OutlineColor", new Color(1, 1, 0, 0));

        icon_Close.material.SetFloat("_Scale", normalScale);
        icon_Close.material.SetFloat("_GlowPower", normalGlow);
    }

    protected virtual void OnDisable()
    {
        OnPonterExit();
    }

    public void BeginDrag(Vector3 mouseWorld)
    {
        icon_Close.GetComponent<Rigidbody2D>().bodyType =
            RigidbodyType2D.Kinematic;

        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, zPos);

        if (icon_Open.gameObject.activeSelf)
        {
            icon_Close.transform.position = mouseWorld;
            icon_Close.transform.localPosition = new Vector3(icon_Close.transform.localPosition.x, icon_Close.transform.localPosition.y, 0f);
        }
        else if (icon_Close.gameObject.activeSelf)
        {
            icon_Open.transform.position = mouseWorld;
            icon_Open.transform.localPosition = new Vector3(icon_Open.transform.localPosition.x, icon_Open.transform.localPosition.y, 0f);
        }
    }

    public void Drag(Vector3 targetPos, bool isWorkspace)
    {
        targetPos.z = zPos;
        transform.position = targetPos;

        if (isWorkspace)
        {
            icon_Open.gameObject.SetActive(true);
            icon_Close.gameObject.SetActive(false);
            icon_Close.transform.localEulerAngles = Vector3.zero;
        }
        else
        {
            icon_Open.gameObject.SetActive(false);
            icon_Close.gameObject.SetActive(true);
        }
    }

    public virtual void EndDrag()
    {
        if (icon_Close.gameObject.activeSelf)
        {
            icon_Close.GetComponent<Rigidbody2D>().bodyType =
                RigidbodyType2D.Dynamic;
        }
        else
        {
            icon_Close.GetComponent<Rigidbody2D>().bodyType =
                RigidbodyType2D.Kinematic;
        }
    }

    public void OnPonterEnter()
    {
        icon_Close.material.SetColor("_OutlineColor", new Color(1, 1, 0.25f, 1));

        icon_Close.material.SetFloat("_Scale", hoverScale);
        icon_Close.material.SetFloat("_GlowPower", hoverGlow);
    }

    public void OnPonterExit()
    {
        icon_Close.material.SetColor("_OutlineColor", new Color(1, 1, 0, 0));

        icon_Close.material.SetFloat("_Scale", normalScale);
        icon_Close.material.SetFloat("_GlowPower", normalGlow);
    }

    public void ResetChildPosition()
    {
        icon_Open.transform.localPosition = Vector3.zero;
        icon_Close.transform.localPosition = Vector3.zero;
    }
}