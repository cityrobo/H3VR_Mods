using System.Collections;
using UnityEngine;
using FistVR;

namespace Cityrobo
{
    public class PaperCartridgeRenderer : MonoBehaviour
    {
        public FVRFireArmChamber chamber;

        public Mesh cartridgeMesh;
        public Material cartridgeMaterial;

#if !(UNITY_EDITOR || UNITY_5)
        void LateUpdate()
        {
            if (chamber.m_round != null && !chamber.m_round.IsSpent && chamber.ProxyMesh.mesh != cartridgeMesh)
            {
                chamber.ProxyMesh.mesh = cartridgeMesh;
                chamber.ProxyRenderer.material = cartridgeMaterial;
            }
        }
#endif
    }
}