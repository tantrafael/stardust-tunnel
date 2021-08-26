using UnityEngine;

//----------------------------------------------------------------------------
// ContactImage
//----------------------------------------------------------------------------
public class ContactImage
{
	public delegate void ClickHandler( ContactImage contactImage );
	public event ClickHandler Click;

	public Vector3 position
	{
		get
		{
			return gameObject.transform.position;
		}
	}

	private GameObject gameObject;
	private ContactImageBehaviour behaviour;
	private ContactImageBehaviour.ClickHandler clickHandler;

	//------------------------------------------------------------------------
	public void Init( float tunnelLength, Texture2D texture )
	{
		gameObject = ( GameObject ) MonoBehaviour.Instantiate( Resources.Load( "Prefabs/TilePrefab" ) );
		gameObject.AddComponent( typeof( ContactImageBehaviour ) );
		gameObject.layer = 16;

		clickHandler = new ContactImageBehaviour.ClickHandler( HandleClick );
		behaviour = ( ContactImageBehaviour ) gameObject.GetComponent( typeof( ContactImageBehaviour ) );
		behaviour.Init( tunnelLength, texture );
		behaviour.Click += clickHandler;
	}

	//------------------------------------------------------------------------
	private void HandleClick()
	{
		if( Click != null )
		{
			Click( this );
		}
	}

	//------------------------------------------------------------------------
	public void Destroy()
	{
		if( behaviour != null )
		{
			behaviour.Click -= clickHandler;
			behaviour.Destroy();
			GameObject.Destroy( behaviour );
			behaviour = null;
		}

		clickHandler = null;

		if( gameObject != null )
		{
			GameObject.Destroy( gameObject );
			gameObject = null;
		}
	}
}