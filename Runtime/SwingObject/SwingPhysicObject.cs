using UnityEngine;

namespace MazurkaGameKit.Rope2D
{
    public class SwingPhysicObject : MonoBehaviour
    {
        [SerializeField] private bool swingOnStart;
        [SerializeField] private float swingOnStartForce = 5f;
        [SerializeField] private int swingOnStartSign = 1;
        

        
        public Vector3 GetRootAnchorPosition => rootBody.position;
        public Vector3 GetSwingObjectPosition => swingObjectBody.position;
        public Rigidbody2D GetSwingObjectBody => swingObjectBody;
        public SpriteRenderer GetSwingObjectSpriteRenderer => swingObjectSpriteRenderer;


        
        [HideInInspector] public Rigidbody2D swingObjectBody;
        [HideInInspector] public Rigidbody2D rootBody;
        [HideInInspector] public DistanceJoint2D dJoin;
        [HideInInspector] public BoxCollider2D swingObjectCollider;
        [HideInInspector] public SpriteRenderer swingObjectSpriteRenderer;
        [HideInInspector] public Rope2D physicRope;
        [HideInInspector] public SpriteRenderer chainRope;

#if UNITY_EDITOR
        
        [HideInInspector] public Transform swingObjectTransform;
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

        public Rope2D_RopePreset TryGetRopePreset()
        {
            if (physicRope != null)
                return physicRope.Preset;

            return null;
        }
        

        //INIT 
        public void CreateRope()
        {
            transform.position = new Vector3(transform.position.x, 1, 0);
            
            //BODY && JOIN
            dJoin = GetComponentInChildren<DistanceJoint2D>(true);
            
            if (dJoin == null)
            {
                swingObjectTransform = transform.Find("_SwingObject");

                if (swingObjectTransform == null)
                {
                    swingObjectTransform = new GameObject("_SwingObject").transform;
                    swingObjectTransform.SetParent(transform);
                    swingObjectTransform.localPosition = Vector3.down * 10f;

                    dJoin = swingObjectTransform.gameObject.AddComponent<DistanceJoint2D>();
                }
            }
            else
            {
                swingObjectTransform = dJoin.transform;
            }
            
            swingObjectBody = dJoin.attachedRigidbody;
            swingObjectBody.bodyType = RigidbodyType2D.Dynamic;
            
            if (dJoin.connectedBody == null)
            {
                dJoin.connectedBody = rootBody;
            }

            //SWING OBJECT
            swingObjectModel = swingObjectTransform.Find("_Model");

            if (swingObjectModel == null)
            {
                swingObjectModel = new GameObject("_Model").transform;
                swingObjectModel.SetParent(swingObjectTransform);
                swingObjectModel.localPosition = Vector3.zero;
            }

            swingObjectSpriteRenderer = swingObjectModel.GetComponentInChildren<SpriteRenderer>(true);

            if (swingObjectSpriteRenderer == null)
            {
                Transform sp = new GameObject("_Sprite").transform;
                sp.SetParent(swingObjectModel);
                sp.localPosition = Vector3.zero;
                swingObjectSpriteRenderer = sp.gameObject.AddComponent<SpriteRenderer>();
                Debug.Log(Resources.Load<Sprite>("Rope2D/S_Lamp").name);
                swingObjectSpriteRenderer.sprite = Resources.Load<Sprite>("Rope2D/S_Lamp");
            }
            
            swingObjectCollider = swingObjectModel.GetComponentInChildren<BoxCollider2D>(true);

            if (swingObjectCollider == null)
            {
                Transform col = new GameObject("_Collider").transform;
                col.SetParent(swingObjectModel);
                col.localPosition = Vector3.zero;
                swingObjectCollider = col.gameObject.AddComponent<BoxCollider2D>();
            }

        
            
            //ANCHOR BODY
            rootBody = GetComponent<Rigidbody2D>();

            if (rootBody == null)
            {
                rootBody = gameObject.AddComponent<Rigidbody2D>();
            }
            
            rootBody.bodyType = RigidbodyType2D.Static;


            //ROPE
            ropeFolder = swingObjectTransform.Find("_Rope");

            if (ropeFolder == null)
            {
                ropeFolder = new GameObject("_Rope").transform;
                ropeFolder.SetParent(swingObjectTransform);
                ropeFolder.localPosition = Vector3.zero;
            }

            chainRope = ropeFolder.GetComponentInChildren<SpriteRenderer>(true);

            if (chainRope == null)
            {
                CreateSpriteRope(Resources.Load<Sprite>("Rope2D/S_Chain"),1f);
            }
            
            physicRope = ropeFolder.GetComponentInChildren<Rope2D>(true);

            if (physicRope == null)
            {
                CreatePhysicRope(Resources.Load<Rope2D_RopePreset>("Rope2D/DefaultRope2D"));
            }


            if (dJoin.maxDistanceOnly)
            {
                physicRope.gameObject.SetActive(true);
                chainRope.gameObject.SetActive(false);
            }
            else
            {
                physicRope.gameObject.SetActive(false);
                chainRope.gameObject.SetActive(true);
            }
            
            //INIT THINGS
            SetSwingObjectPosition(GetSwingObjectPosition);
        }
        


        //ROPE 
        public void CreatePhysicRope(Rope2D_RopePreset ropePreset)
        {
            //CLEAR
            if (physicRope == null)
            {
                //CREATE
                physicRope = Rope2D.CreateRope(dJoin.connectedBody.transform, ropeFolder, ropePreset, ropeFolder, true, 0f);
                physicRope.gameObject.name = "_Rope";
            }
            
            physicRope.SetRopePreset(ropePreset);
            dJoin.maxDistanceOnly = true;

            UpdateRope();
            
            UnityEditor.EditorUtility.SetDirty(gameObject);
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

            dJoin.maxDistanceOnly = false;

            UpdateRope();
            
            UnityEditor.EditorUtility.SetDirty(gameObject);
        }
        public void UpdateRope()
        {
            if(dJoin.maxDistanceOnly)
            {
                physicRope.DrawRopeInEditor();
            }
            else
            {
                chainRope.size = new Vector2(chainRope.sprite.bounds.size.x, (Vector2.Distance(dJoin.connectedBody.position, swingObjectBody.transform.position)) / chainRope.transform.localScale.y);
            }
        }

        
        //SWING OBJECT
        public void SetSprite(Sprite newSprite)
        {
            if (swingObjectSpriteRenderer.sprite == newSprite)
                return;

            swingObjectSpriteRenderer.sprite = newSprite;
            ResizeCollider();
        }
        public void ScaleSprite(float newScale)
        {
            swingObjectModel.localScale = Vector3.one * newScale;
        }
        public void ResizeCollider()
        {
            if (swingObjectCollider != null && swingObjectSpriteRenderer != null && swingObjectSpriteRenderer.sprite != null)
            {
                swingObjectCollider.size = swingObjectSpriteRenderer.sprite.bounds.size;
                swingObjectCollider.offset = swingObjectSpriteRenderer.transform.localPosition + swingObjectSpriteRenderer.sprite.bounds.center;
            }
        }


        //SWING
        public void SetSwingObjectPosition(Vector3 position)
        {
            swingObjectBody.transform.position = position;
            dJoin.distance = Vector2.Distance(GetRootAnchorPosition,GetSwingObjectPosition);
            UpdateRope();
        }
        
#endif
        
        private void Start()
        {
            if(swingOnStart)
            {
                Swing(swingOnStartSign, swingOnStartForce);
            }
        }

        private void Update()
        {
            if(!dJoin.maxDistanceOnly)
            {
                chainRope.transform.rotation = LookAt2D((dJoin.connectedBody.position  - swingObjectBody.position).normalized);
            }
        }
        
        private Quaternion LookAt2D(Vector2 dir)
        {
            var angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            return Quaternion.AngleAxis(angle + -90f, Vector3.forward);
        }
        
        public void Swing(int sign, float force)
        {
            swingObjectBody.AddForce(Vector2.right * sign * force * swingObjectBody.mass, ForceMode2D.Impulse);
        }
    }
}

