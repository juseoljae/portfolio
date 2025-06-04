using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CStaticAvatar
{
	public GameObject AvatarOBj;
	public Animator AvatarAnimator;
	public SkinnedMeshRenderer FaceMesh;
	public int BlendShapeCount;

	public CStaticAvatar(GameObject avatarObj, Animator anim)
	{
		AvatarOBj = avatarObj;
		AvatarAnimator = anim;

        SetFaceMesh();
	}

	private void SetFaceMesh()
	{
		if (AvatarOBj != null)
		{
			GameObject faceObj = AvatarOBj.transform.Find( "Basic_Face" ).gameObject;
			if (faceObj != null)
			{
				FaceMesh = faceObj.GetComponent<SkinnedMeshRenderer>();
				if (FaceMesh != null)
				{
					Mesh mesh = FaceMesh.sharedMesh;
					if (mesh)
					{
						BlendShapeCount = mesh.blendShapeCount;
					}
				}
            }
		}
    }

	public void InitAvatarBlendShape()
    {
        for (int i = 0; i < BlendShapeCount; ++i)
        {
			if (FaceMesh != null)
			{
				FaceMesh.SetBlendShapeWeight( i, 0 );
			}
        }
    }
}
