#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MazurkaGameKit.Rope2D
{
    public partial class SwingDecorativeObject
    {
        [SerializeField] private bool simulate;
        [SerializeField] private Transform swingObjectModel;
        [SerializeField] private Transform ropeFolder;
        [SerializeField] private SpriteRenderer swingObjectSpriteRenderer;
        [SerializeField] private Sprite usedSprite;
        [SerializeField] private Sprite chainSprite;
        [SerializeField] private float _chainWidth = 0.5f;

        public float ChainWidth
        {
            get => _chainWidth;
            set
            {
                _chainWidth = value;
                chainSpriteRenderer.transform.localScale = Vector3.one * _chainWidth;
            }
        }
        
        public float SwingObjectSize => swingObjectModel.localScale.x;
        public float Distance => Vector2.Distance(GetRootAnchorPosition, GetSwingObjectPosition);
        public void CheckRope()
        {
            CreateHangedObject();
            CreateRope();
            
            SetSwingObjectPosition(GetSwingObjectPosition);
            EditorUtility.SetDirty(gameObject);
        }
            
        //CREATE
        private void CreateHangedObject()
        {
            swingObjectBody = transform.Find("_SwingObject");

            if (swingObjectBody == null)
            {
                swingObjectBody = new GameObject("_SwingObject").transform;
                swingObjectBody.SetParent(transform);
                swingObjectBody.localPosition = Vector3.down * 10f;
            }
            
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
                usedSprite = Resources.Load<Sprite>("Rope2D/S_Lamp");
                SetSprite();
            }
            else
            {
                usedSprite = swingObjectSpriteRenderer.sprite;
            }
        }
        private void CreateRope()
        {
            ropeFolder = swingObjectBody.Find("_Rope");

            if (ropeFolder == null)
            {
                ropeFolder = new GameObject("_Rope").transform;
                ropeFolder.SetParent(swingObjectBody);
                ropeFolder.localPosition = Vector3.zero;
            }

            
            chainSpriteRenderer = ropeFolder.GetComponentInChildren<SpriteRenderer>(true);
            if (chainSpriteRenderer == null)
            {
                _chainWidth = 0.5f;
                chainSprite = Resources.Load<Sprite>("Rope2D/S_Chain");
                                
                GameObject ropeChain = new GameObject("_Chain");
                ropeChain.transform.SetParent(ropeFolder);
                ropeChain.transform.localPosition = Vector3.zero;
                ropeChain.transform.localEulerAngles = Vector3.zero;
                chainSpriteRenderer = ropeChain.AddComponent<SpriteRenderer>();
                SetChainSprite();
                chainSpriteRenderer.transform.localScale = Vector3.one * _chainWidth;
                chainSpriteRenderer.drawMode = SpriteDrawMode.Tiled;
            }
            else
            {
                _chainWidth = chainSpriteRenderer.transform.localScale.x;
                chainSprite = chainSpriteRenderer.sprite;
            }
            
            UpdateRope();
        }
        
        
        //UTILS
        public void UpdateRope()
        {
            chainSpriteRenderer.size = new Vector2(chainSpriteRenderer.sprite.bounds.size.x, Distance / chainSpriteRenderer.transform.localScale.y);
        }
        public void SetSwingObjectPosition(Vector3 position)
        {
            swingObjectBody.transform.position = position;
            UpdateRope();
        }
        public void SetSprite() => swingObjectSpriteRenderer.sprite = usedSprite;
        public void SetChainSprite() => chainSpriteRenderer.sprite = chainSprite;
        public void ScaleSprite(float newScale) =>  swingObjectModel.localScale = Vector3.one * newScale;
        
    }
}

#endif
