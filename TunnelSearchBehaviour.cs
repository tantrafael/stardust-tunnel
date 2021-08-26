using UnityEngine;

//----------------------------------------------------------------------------
// TunnelSearchBehaviour
//----------------------------------------------------------------------------
public class TunnelSearchBehaviour : MonoBehaviour
{
	public delegate void GrabHandler( bool value );
	public delegate void ActivationHandler();
	public delegate void MovementHandler( Vector3 movement );
	public event GrabHandler Grab;
	public event ActivationHandler Activation;
	public event MovementHandler Movement;

//	private const int   LAYER = 15;
//	private const float SIZE  = 1.0f;

	private TextMesh textMesh;
	private Vector3 P;

	//------------------------------------------------------------------------
	public void Awake()
	{
		GameObject icon;
		GameObject text;

	/*
		gameObject.layer = LAYER;
		gameObject.transform.localScale = SIZE * Vector3.one;
	*/

		icon = transform.Find( "Icon" ).gameObject;
		text = transform.Find( "Text" ).gameObject;

		icon.GetComponent<Renderer>().material.color = Color.white;
		icon.GetComponent<Renderer>().material.shader = Shader.Find( "Transparent/Diffuse" );
		icon.GetComponent<Renderer>().material.mainTexture = ( Texture2D ) Resources.Load( "Textures/magnet_search" );;

		textMesh = ( TextMesh ) text.GetComponent( typeof( TextMesh ) );
	}

	//------------------------------------------------------------------------
	public void Init( Vector3 position, string searchString )
	{
		textMesh.text = searchString;
		gameObject.transform.position = position;
	}

	//------------------------------------------------------------------------
	public void OnMouseDown()
	{
		P = Input.mousePosition;

		if( Grab != null )
		{
			Grab( true );
		}

		if( Activation != null )
		{
			Activation();
		}
	}

	//------------------------------------------------------------------------
	public void OnMouseDrag()
	{
		if( Input.mousePosition != P )
		{
			if( Activation != null )
			{
				Activation();
			}

			if( Movement != null )
			{
				Movement( Input.mousePosition - P );
			}

			P = Input.mousePosition;
		}
	}

	//------------------------------------------------------------------------
	public void OnMouseUp()
	{
		if( Grab != null )
		{
			Grab( false );
		}
	}

/*
	//------------------------------------------------------------------------
	public void SetPosition( Vector3 position )
	{
		gameObject.transform.position = position;
	}
*/

	//------------------------------------------------------------------------
	public void Destroy()
	{
		textMesh = null;
	}
}