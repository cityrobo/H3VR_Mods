using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ThermalBody : MonoBehaviour {

	[Range(0.5f,8f)]
	public float ThermalDistribution = 4f;
	[Range(0f,1f)]
	public float MaximumTemperature = 1f;
	[Range(0.001f,1f)]
	public float MinimumTemperature = 0.001f;

	public bool isVariable = false;

	private List<Renderer> _childrenRenderers;
	private List<Material> origMaterialList;
	private List<Material> newMaterialsList;

	void OnEnable()
	{
		if (MinimumTemperature < 0.001f) MinimumTemperature = 0.001f;

		_childrenRenderers = new List<Renderer>(GetComponentsInChildren<Renderer>());
		if (origMaterialList == null) origMaterialList = new List<Material>();
		else origMaterialList.Clear();
		if (newMaterialsList == null) newMaterialsList = new List<Material>();
		else newMaterialsList.Clear();
		if (isVariable)
        {
			CopyMaterials();
        }
		UpdateSubMaterialProperties();
	}

	public void UpdateSubMaterialProperties()
	{
		if (_childrenRenderers.Count != 0)
		{
			foreach (var childrenRenderer in _childrenRenderers)
			{
				if (childrenRenderer.gameObject.GetComponent<ThermalBody>() == null || childrenRenderer.gameObject == this.gameObject)
				{
					var materials = childrenRenderer.sharedMaterials;
					if (materials.Length != 0)
					{
						foreach (var material in materials)
						{
							if (material != null)
							{
								if (MaximumTemperature > 0.3f)
								{
									material.SetOverrideTag("Thermal", "Hot");
								}
								else
								{
									material.SetOverrideTag("Thermal", "Cold");
								}

								material.SetFloat("_ThermalPowExponent", ThermalDistribution);
								material.SetFloat("_ThermalMax", MaximumTemperature);
								material.SetFloat("_ThermalMin", MinimumTemperature);
							}
						}
					}
				}
			}
		}
	}

	public void RestoreSubMaterialProperties()
	{
		if (_childrenRenderers.Count != 0)
		{
			foreach (var childrenRenderer in _childrenRenderers)
			{
				if (childrenRenderer.gameObject.GetComponent<ThermalBody>() == null || childrenRenderer.gameObject == this.gameObject)
				{
					var materials = childrenRenderer.sharedMaterials;
					if (materials.Length != 0)
					{
						foreach (var material in materials)
						{
							if (material != null) material.SetOverrideTag("Thermal", "");
						}
					}
				}
			}
		}
	}

	void OnDisable() {
		//Debug.Log(this.gameObject.name + " ThermalBody disabled and clearing materials.");

		if (isVariable)
        {
			//RestoreMaterials();
		}
		//RestoreSubMaterialProperties();
	}
	
	public void OnDestroy()
    {
        //Debug.Log(this.gameObject.name + " ThermalBody destroyed and clearing materials.");

        if (isVariable)
		{
			//RestoreMaterials();

			ClearMaterials();
		}
		//RestoreSubMaterialProperties();
	}

	void CopyMaterials()
	{
		if (_childrenRenderers.Count != 0)
		{
			foreach (var childrenRenderer in _childrenRenderers)
			{
				if (childrenRenderer.gameObject.GetComponent<ThermalBody>() == null || childrenRenderer.gameObject == this.gameObject)
				{
					var materials = childrenRenderer.sharedMaterials;

					for (int i = 0; i < materials.Length; i++)
					{
                        if (materials[i] != null)
                        {
							origMaterialList.Add(materials[i]);
						}
					}
                    for (int i = 0; i < childrenRenderer.sharedMaterials.Length; i++)
                    {
                        if (materials[i] != null)
                        {
							childrenRenderer.materials[i] = new Material(materials[i]);
							newMaterialsList.Add(childrenRenderer.materials[i]);
						}
					}
				}
			}
		}
	}

	void RestoreMaterials()
	{
		if (_childrenRenderers.Count != 0)
		{
			foreach (var childrenRenderer in _childrenRenderers)
			{
				if (childrenRenderer.gameObject.GetComponent<ThermalBody>() == null || childrenRenderer.gameObject == this.gameObject)
				{
					var materials = childrenRenderer.sharedMaterials;

					for (int i = 0; i < materials.Length; i++)
					{
                        if (materials[i] != null)
                        {
							childrenRenderer.sharedMaterials[i] = origMaterialList[i];
						}
					}
				}
			}
		}
		for (int i = 0; i < newMaterialsList.Count; i++)
		{
			Destroy(newMaterialsList[i]);
		}
		newMaterialsList.Clear();
	}

	void ClearMaterials()
    {
		for (int i = 0; i < newMaterialsList.Count; i++)
		{
			Destroy(newMaterialsList[i]);
		}
		newMaterialsList.Clear();
	}

#if UNITY_EDITOR
	private void Update()
	{
		//Enable quick changes in the editor
		UpdateSubMaterialProperties();
	}
#endif
}
