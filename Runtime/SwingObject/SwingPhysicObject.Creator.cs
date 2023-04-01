#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

namespace MazurkaGameKit.Rope2D
{
    public partial class SwingPhysicObject 
    {

        [SerializeField] private bool useMaxDistanceOnly = true;
        [SerializeField] private Sprite usedSprite;
        [SerializeField] private Rope2D_RopePreset ropePreset;
        [SerializeField] private Sprite chainRopeSprite;
        [SerializeField] private BoxCollider2D swingObjectCollider;
        [SerializeField] private SpriteRenderer swingObjectSpriteRenderer;
        [SerializeField] private float _chainWidth = 0.5f;
        
        [SerializeField] private Transform swingObjectTransform;
        [SerializeField] private Transform swingObjectModel;

        public bool UseMaxDistanceOnly => useMaxDistanceOnly;
        public float SwingObjectSize => swingObjectModel.localScale.x;
        public float ChainWidth
        {
            get => chainRope.transform.localScale.x;
            set
            {
                _chainWidth = value;
                chainRope.transform.localScale = Vector3.one * _chainWidth;
            }
        }

        
        public void CheckRope()
        {
            transform.position = new Vector3(transform.position.x, transform.position.y, 0);

            CreateCorePhysic();
            
            CreateSwingObject();

            CreateRopes();
   

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
            
            SetSwingObjectPosition(GetSwingObjectPosition);
            
            EditorUtility.SetDirty(gameObject);
        }
        

        //CREATE 
        private void CreateRopes()
        {
            Transform chain = swingObjectTransform.Find("_Chain");

            if (chain == null)
            {
                chainRopeSprite = Resources.Load<Sprite>("Rope2D/S_Chain");
                _chainWidth = 0.5f;
                CreateSpriteRope();
            }
            else
            {
                if (chain.TryGetComponent(out chainRope))
                {
                    _chainWidth = chainRope.transform.localScale.x;
                    chainRopeSprite = chainRope.sprite;
                }
                else
                {
                    DestroyImmediate(chain.gameObject);
                    chainRopeSprite = Resources.Load<Sprite>("Rope2D/S_Chain");
                    _chainWidth = 0.5f;
                    CreateSpriteRope();
                }
            }
            
            physicRope = transform.GetComponentInChildren<Rope2D>(true);
            if (physicRope == null)
            {
                ropePreset = Rope2D.GetDefaultPreset;
                CreatePhysicRope();
            }
            else
            {
                ropePreset = physicRope.Preset;
            }
        }
        private void CreatePhysicRope()
        {
            if (physicRope == null)
            {
                physicRope = Rope2D.CreateRope(dJoin.connectedBody.transform,swingObjectTransform, ropePreset, transform, true, -1f); // = -1.5f tension to simulate hard tension on the rope
                physicRope.gameObject.name = "_PhysicRope";
            }
            
            physicRope.SetRopePreset(ropePreset);
            
            if (useMaxDistanceOnly)
            {
                dJoin.maxDistanceOnly = true;
                UpdateRope();
            }
        }
        private void CreateSpriteRope()
        {
            if (chainRope == null)
            {   
                //CREATE
                GameObject ropeChain = new GameObject("_Chain");
                ropeChain.transform.SetParent(swingObjectTransform);
                ropeChain.transform.localPosition = Vector3.zero;
                ropeChain.transform.localEulerAngles = Vector3.zero;

                chainRope = ropeChain.AddComponent<SpriteRenderer>();
            }
         
            chainRope.sprite = chainRopeSprite;
            chainRope.drawMode = SpriteDrawMode.Tiled;
            chainRope.transform.localScale = Vector3.one * _chainWidth;

            
            if (!useMaxDistanceOnly)
            {
                dJoin.maxDistanceOnly = false;
                UpdateRope();
            }
        }
        private void CreateSwingObject()
        {
            swingObjectModel = swingObjectTransform.Find("_Model");

            if (swingObjectModel == null)
            {
                swingObjectModel = new GameObject("_Model").transform;
                swingObjectModel.SetParent(swingObjectTransform);
                swingObjectModel.localPosition = Vector3.zero;
            }
            
            swingObjectCollider = swingObjectModel.GetComponentInChildren<BoxCollider2D>(true);

            if (swingObjectCollider == null)
            {
                Transform col = new GameObject("_Collider").transform;
                col.SetParent(swingObjectModel);
                col.localPosition = Vector3.zero;
                swingObjectCollider = col.gameObject.AddComponent<BoxCollider2D>();
            }
            

            swingObjectSpriteRenderer = swingObjectModel.GetComponentInChildren<SpriteRenderer>(true);

            if (swingObjectSpriteRenderer == null)
            {
                Transform sp = new GameObject("_Sprite").transform;
                sp.SetParent(swingObjectModel);
                sp.localPosition = Vector3.zero;
                swingObjectSpriteRenderer = sp.gameObject.AddComponent<SpriteRenderer>();

                usedSprite = Resources.Load<Sprite>("Rope2D/S_Lamp");
                SetSprite();
            }
            else
            {
                usedSprite = swingObjectSpriteRenderer.sprite;
            }
        }
        private void CreateCorePhysic()
        {
            rootBody = GetComponent<Rigidbody2D>();
            if (rootBody == null)
            {
                rootBody = gameObject.AddComponent<Rigidbody2D>();
            }
            rootBody.bodyType = RigidbodyType2D.Static;
            

            
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

            if (swingObjectBody == null)
            {
                swingObjectBody = dJoin.attachedRigidbody;
                swingObjectBody.bodyType = RigidbodyType2D.Dynamic;
                swingObjectBody.gravityScale = 1f;
                swingObjectBody.angularDrag = 3f;
                swingObjectBody.drag = 0.3f;
            }
 
            
            if (dJoin.connectedBody == null)
            {
                dJoin.connectedBody = rootBody;
            }
        }

        
        //EDITOR UTILS
        public void UpdateRope()
        {
            if(useMaxDistanceOnly) physicRope.DrawRopeInEditor();
            else chainRope.size = new Vector2(chainRope.sprite.bounds.size.x, (Vector2.Distance(dJoin.connectedBody.position, swingObjectBody.transform.position)) / chainRope.transform.localScale.y);
        }
        public void SetSprite()
        {
            if (swingObjectSpriteRenderer.sprite == usedSprite)
                return;

            swingObjectSpriteRenderer.sprite = usedSprite;
            ResizeCollider();
            EditorUtility.SetDirty(gameObject);
        }

        public void SetChainSprite() => chainRope.sprite = chainRopeSprite;
        public void SetRopePreset() => physicRope.SetRopePreset(ropePreset);
        public void ScaleSprite(float newScale)
        {
            swingObjectModel.localScale = Vector3.one * newScale;
            EditorUtility.SetDirty(gameObject);
        }
        public void ResizeCollider()
        {
            if (swingObjectSpriteRenderer.sprite != null)
            {
                swingObjectCollider.size = swingObjectSpriteRenderer.sprite.bounds.size;
                swingObjectCollider.offset = swingObjectSpriteRenderer.transform.localPosition + swingObjectSpriteRenderer.sprite.bounds.center;
                EditorUtility.SetDirty(gameObject);
            }
        }
        public void SetSwingObjectPosition(Vector3 position)
        {
            swingObjectBody.transform.position = position;
            dJoin.distance = Vector2.Distance(GetRootAnchorPosition,GetSwingObjectPosition);
            UpdateRope();
        }
        public void ChangeMaxDistanceOnly()
        {
            useMaxDistanceOnly = !useMaxDistanceOnly;
            
            if (useMaxDistanceOnly)
            {
                dJoin.maxDistanceOnly = true;
                chainRope.gameObject.SetActive(false);
                CreatePhysicRope();
                physicRope.gameObject.SetActive(true);
                EditorUtility.SetDirty(gameObject);
            }
            else
            {
                dJoin.maxDistanceOnly = false;
                physicRope.gameObject.SetActive(false);
                CreateSpriteRope();
                chainRope.gameObject.SetActive(true);
                EditorUtility.SetDirty(gameObject);
            }
        }
        public void ResetRootPosition()
        {
            transform.position =dJoin.connectedBody.position + dJoin.connectedAnchor;
            EditorUtility.SetDirty(gameObject);
        }
    }
}
#endif