using System.Collections;
using UnityEngine;
using FistVR;
using System.Collections.Generic;

namespace Cityrobo
{
    public class PaperCartridgeRenderer : MonoBehaviour
    {
        public FVRFireArmChamber chamber;

        public Mesh cartridgeMesh;
        public Material cartridgeMaterial;

        //private bool _materialChanged = false;
        private GameObject _proxyGameObject;
        private MeshFilter _proxyMeshFilter;
        private MeshRenderer _proxyMeshRenderer;

        private static readonly Dictionary<FVRFireArmChamber, PaperCartridgeRenderer> _existingPaperCartridgeRenderers = new Dictionary<FVRFireArmChamber, PaperCartridgeRenderer>();
        private static readonly Dictionary<Mesh, GameObject> _paperCartridgeMeshProxyDictionary = new Dictionary<Mesh, GameObject>();

#if !(UNITY_EDITOR || UNITY_5 || DEBUG)
        public void Awake()
        {
            gameObject.SetActive(false);
            OpenScripts2.PaperCartridgeRenderer newComp = gameObject.AddComponent<OpenScripts2.PaperCartridgeRenderer>();
            newComp.Chamber = chamber;

            newComp.CartridgeMesh = cartridgeMesh;
            newComp.CartridgeMaterial = cartridgeMaterial;
            gameObject.SetActive(true);
            Destroy(this);

            //_existingPaperCartridgeRenderers.Add(chamber, this);

            //if (_paperCartridgeMeshProxyDictionary.TryGetValue(cartridgeMesh, out _proxyGameObject))
            //{
            //    _proxyMeshFilter = _proxyGameObject.GetComponent<MeshFilter>();
            //    _proxyMeshRenderer = _proxyGameObject.GetComponent<MeshRenderer>();
            //}
            //else
            //{
            //    _proxyGameObject = new GameObject("ProxyPaperCartridgeRenderer_" + cartridgeMesh.name);
            //    _proxyGameObject.SetActive(false);
            //    DontDestroyOnLoad(_proxyGameObject);

            //    _proxyMeshFilter = _proxyGameObject.AddComponent<MeshFilter>();
            //    _proxyMeshFilter.sharedMesh = cartridgeMesh;

            //    _proxyMeshRenderer = _proxyGameObject.AddComponent<MeshRenderer>();
            //    _proxyMeshRenderer.sharedMaterial = cartridgeMaterial;

            //    _paperCartridgeMeshProxyDictionary.Add(cartridgeMesh, _proxyGameObject);
            //}
        }

        //public void OnDestroy()
        //{
        //    _existingPaperCartridgeRenderers.Remove(chamber);
        //}

        //static PaperCartridgeRenderer()
        //{
        //    On.FistVR.FVRFireArmChamber.UpdateProxyDisplay += FVRFireArmChamber_UpdateProxyDisplay;
        //}

        //private static void FVRFireArmChamber_UpdateProxyDisplay(On.FistVR.FVRFireArmChamber.orig_UpdateProxyDisplay orig, FVRFireArmChamber self)
        //{
        //    if (_existingPaperCartridgeRenderers.TryGetValue(self, out PaperCartridgeRenderer paperCartridgeRenderer))
        //    {
        //        if (self.m_round == null)
        //        {
        //            self.ProxyMesh.mesh = null;
        //            self.ProxyRenderer.material = null;
        //            self.ProxyRenderer.enabled = false;
        //        }
        //        else
        //        {
        //            if (self.IsSpent)
        //            {
        //                if (self.m_round.FiredRenderer != null)
        //                {
        //                    self.ProxyMesh.mesh = self.m_round.FiredRenderer.GetComponent<MeshFilter>().sharedMesh;
        //                    self.ProxyRenderer.material = self.m_round.FiredRenderer.sharedMaterial;
        //                }
        //                else
        //                {
        //                    self.ProxyMesh.mesh = null;
        //                }
        //            }
        //            else
        //            {
        //                self.ProxyMesh.mesh = paperCartridgeRenderer._proxyMeshFilter.sharedMesh;
        //                self.ProxyRenderer.material = paperCartridgeRenderer._proxyMeshRenderer.sharedMaterial;
        //            }
        //            self.ProxyRenderer.enabled = true;
        //        }
        //    }
        //    else orig(self);
        //}

        //public void LateUpdate()
        //{
        //    if (!_materialChanged && chamber.m_round != null && !chamber.m_round.IsSpent && chamber.ProxyMesh.mesh != cartridgeMesh)
        //    {
        //        chamber.ProxyMesh.mesh = cartridgeMesh;
        //        chamber.ProxyRenderer.material = cartridgeMaterial;
        //        _materialChanged = true;
        //    }
        //    else if (_materialChanged && chamber.m_round == null)
        //    {
        //        _materialChanged = false;
        //    }
        //}
#endif
    }
}