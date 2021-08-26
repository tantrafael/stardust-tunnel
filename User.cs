using UnityEngine;

//----------------------------------------------------------------------------
// User
//----------------------------------------------------------------------------
public class User
{
	private const float viewingDistance = 9.0f;
	private GameObject gameObject;
	private UserBehaviour behaviour;
	private float d;

	//------------------------------------------------------------------------
	public void Init()
	{
		gameObject = ( GameObject ) MonoBehaviour.Instantiate( Resources.Load( "Prefabs/TunnelUserPrefab" ) );
		behaviour = ( UserBehaviour ) gameObject.AddComponent( typeof( UserBehaviour ) );
	}

	//------------------------------------------------------------------------
	public Vector3 GetPosition()
	{
		return gameObject.transform.position;
	}

	//------------------------------------------------------------------------
	public Quaternion GetRotation()
	{
		return gameObject.transform.rotation;
	}

	//------------------------------------------------------------------------
	public float GetFov()
	{
		return gameObject.GetComponent<Camera>().fieldOfView;
	}

	//------------------------------------------------------------------------
	public void Slide( Vector3 position )
	{
		behaviour.Slide( position );
	}

	//------------------------------------------------------------------------
	public void View( Vector3 position )
	{
		Vector3 P;

		P = new Vector3();
		P.z = position.z - viewingDistance;
		Slide( P );
	}

	//------------------------------------------------------------------------
	public void SetInputHandling( bool value )
	{
		behaviour.SetInputHandling( value );
	}

	//------------------------------------------------------------------------
	public void Destroy()
	{
		if( behaviour != null )
		{
			GameObject.Destroy( behaviour );
			behaviour = null;
		}

		if( gameObject != null )
		{
			GameObject.Destroy( gameObject );
			gameObject = null;
		}
	}
}