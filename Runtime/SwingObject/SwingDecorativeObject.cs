using UnityEngine;

namespace MazurkaGameKit.Rope2D
{
   [ExecuteAlways]
public class SwingDecorativeObject : MonoBehaviour
{
    [SerializeField] private bool isConstant;
    [SerializeField, Range(1f, 45f)] private float windAmplitude = 1f;
    [SerializeField, Range(0.01f, 5f)] private float windFrequency = 0.5f;
    [SerializeField, HideInInspector] private int windSeed;
    


    public Vector3 GetRootAnchorPosition => transform.position;
    public Vector3 GetSwingObjectPosition => swingObjectBody.position;
    public SpriteRenderer GetSwingObjectSpriteRenderer => swingObjectSpriteRenderer;


        
    [HideInInspector] public Transform swingObjectBody;
    [HideInInspector] public SpriteRenderer swingObjectSpriteRenderer;
    [HideInInspector] public SpriteRenderer chainRope;
    
#if UNITY_EDITOR
    [HideInInspector] public bool simulate;
    [HideInInspector] public Transform swingObjectModel;
    [HideInInspector] public Transform ropeFolder;
    
    public Sprite TryGetRopeSprite()
    {
        if (chainRope != null)
            return chainRope.sprite;

        return null;
    }
        
    public float TryGetRopeSpriteWidth()
    {
        if (chainRope != null)
            return chainRope.transform.localScale.x;

        return 1f;
    }

    public void SetRopeSpriteWidth(float newWidth)
    {
        if (chainRope != null)
        {
            chainRope.transform.localScale = Vector3.one * newWidth;
        }
    }
    
    public void CreateRope()
        {
            //BODY && JOIN

            swingObjectBody = transform.Find("_SwingObject");

            if (swingObjectBody == null)
            {
                swingObjectBody = new GameObject("_SwingObject").transform;
                swingObjectBody.SetParent(transform);
                swingObjectBody.localPosition = Vector3.down * 10f;
            }

            //SWING OBJECT
            swingObjectModel = swingObjectBody.Find("_Model");

            if (swingObjectModel == null)
            {
                swingObjectModel = new GameObject("_Model").transform;
                swingObjectModel.SetParent(swingObjectBody);
                swingObjectModel.localPosition = Vector3.zero;
            }

            swingObjectSpriteRenderer = swingObjectModel.GetComponentInChildren<SpriteRenderer>(true);

            if (swingObjectSpriteRenderer == null)
            {
                Transform sp = new GameObject("_Sprite").transform;
                sp.SetParent(swingObjectModel);
                sp.localPosition = Vector3.zero;
                swingObjectSpriteRenderer = sp.gameObject.AddComponent<SpriteRenderer>();
                swingObjectSpriteRenderer.sprite = Resources.Load<Sprite>("Rope2D/S_Lamp");
            }


            if (swingObjectSpriteRenderer == null)
            {
                Transform col = new GameObject("_Collider").transform;
                col.SetParent(swingObjectModel);
                col.localPosition = Vector3.zero;

            }

            //ROPE
            ropeFolder = swingObjectBody.Find("_Rope");

            if (ropeFolder == null)
            {
                ropeFolder = new GameObject("_Rope").transform;
                ropeFolder.SetParent(swingObjectBody);
                ropeFolder.localPosition = Vector3.zero;
            }

            chainRope = ropeFolder.GetComponentInChildren<SpriteRenderer>(true);

            if (chainRope == null)
            {
                CreateSpriteRope(Resources.Load<Sprite>("Rope2D/S_Chain"),1f);
            }

            //INIT THINGS
            SetSwingObjectPosition(GetSwingObjectPosition);
        }
        
    
    public void CreateSpriteRope(Sprite chainSprite, float chainSpriteSize)
    {
        if (chainRope == null)
        {   
            //CREATE
            GameObject ropeChain = new GameObject("_Chain");
            ropeChain.transform.SetParent(ropeFolder);
            ropeChain.transform.localPosition = Vector3.zero;
            ropeChain.transform.localEulerAngles = Vector3.zero;

            chainRope =  ropeChain.AddComponent<SpriteRenderer>();
        }
         
        chainRope.sprite = chainSprite;
        chainRope.drawMode = SpriteDrawMode.Tiled;
        chainRope.transform.localScale = Vector3.one * chainSpriteSize;

        UpdateRope();
    }
    public void UpdateRope()
    {
        chainRope.size = new Vector2(chainRope.sprite.bounds.size.x, (Vector2.Distance(GetRootAnchorPosition, swingObjectBody.transform.position)) / chainRope.transform.localScale.y);
    }
    
    
    
    //SWING OBJECT
    public void SetSprite(Sprite newSprite)
    {
        if (swingObjectSpriteRenderer.sprite == newSprite)
            return;

        swingObjectSpriteRenderer.sprite = newSprite;
    }
    public void ScaleSprite(float newScale)
    {
        swingObjectModel.localScale = Vector3.one * newScale;
    }
    
    
    //SWING
    public void SetSwingObjectPosition(Vector3 position)
    {
        swingObjectBody.transform.position = position;
        UpdateRope();
    }

#endif
    
    
    
    private void Start()
    {
        NewSeed();
        swingObjectBody.transform.localPosition = new Vector3(0, swingObjectBody.transform.localPosition.y, swingObjectBody.transform.localPosition.z);
    }

    public void NewSeed()
    {
        windSeed = UnityEngine.Random.Range(0, 500000);
    }

    private void Update()
    {
        #if UNITY_EDITOR

        if (!Application.isPlaying && !simulate)
            return;
        
        #endif
        
        if (isConstant)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, (windAmplitude * Mathf.Sin((Time.time + windSeed) * Mathf.PI * windFrequency)));
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 0f, (windAmplitude * Mathf.PerlinNoise((Time.time + windSeed) * windFrequency, 0f)) - (windAmplitude / 2));
        }    
        
        chainRope.transform.rotation = LookAt2D((GetRootAnchorPosition - GetSwingObjectPosition).normalized);
    }

    private Quaternion LookAt2D(Vector2 dir)
    {
        var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        return Quaternion.AngleAxis(angle + -90f, Vector3.forward);
    }

    public void ResetSwingObject()
    {
        swingObjectBody.localPosition = Vector3.down * Vector2.Distance(GetRootAnchorPosition, GetSwingObjectPosition);
        transform.eulerAngles = Vector3.zero;
        swingObjectBody.eulerAngles = Vector3.zero;
        chainRope.transform.rotation = LookAt2D((GetRootAnchorPosition - GetSwingObjectPosition).normalized);
    }
}
 
}

