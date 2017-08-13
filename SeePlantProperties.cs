using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeePlantProperties : MonoBehaviour {

	public float bioMass = 0f;
	public int idNum = -1;
	public float xSize = 0f;
	public float xSizeCalc = 0f;
	public float ySize = 0f;
	public float ySizeCalc = 0f;

	// Use this for initialization
	void Start () {
		
	}

	// Update is called once per frame
	void Update () {

	}

	public void updateYSize(){
		try{
			ySize = GetComponent<MeshRenderer>().bounds.extents.y;
		}
		catch {
			ySize = GetComponentInChildren<MeshRenderer>().bounds.extents.y;
		}
	}

	public void updateXSize(){
		try{
			xSize = GetComponent<MeshRenderer>().bounds.extents.x;
		}
		catch {
			xSize = GetComponentInChildren<MeshRenderer>().bounds.extents.x;
		}
	}
}
