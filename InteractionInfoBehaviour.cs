using UnityEngine;
using System.Collections;

//----------------------------------------------------------------------------
// InteractionInfoBehaviour
//----------------------------------------------------------------------------
public class InteractionInfoBehaviour : MonoBehaviour
{
	public delegate void ClickHandler();
	public event ClickHandler Click;

	private const float DELAY = 0.15f;

	private GameObject container;
	private GameObject symbol;
	private GameObject content;
	private Animation animation;
	private float scale = 0.8f;
	private Vector3 P0 = new Vector3( 0.0f, 0.1f, -0.87f );
	private Vector3 R0 = new Vector3( 0.5f * Mathf.PI * Mathf.Rad2Deg, 0, 0 );

	//------------------------------------------------------------------------
	virtual public void Init( int type )
	{
		string path;

		path = "Prefabs/";

		switch( type )
		{
			case 0:
			{
				path += "MobilePrefab";
				P0.x = -0.025f;
				P0.y = 0.13f;
				break;
			}

			case 3:
			{
				path += "ChatPrefab";
				break;
			}

			default:
			{
				path += "MailPrefab";
				break;
			}
		}

		GetComponent<Renderer>().material.color = Color.white;
		GetComponent<Renderer>().material.shader = Shader.Find( "Transparent/Diffuse" );
		GetComponent<Renderer>().material.mainTexture = ( Texture2D ) Resources.Load( "Textures/InteractionInfo" );

		container = ( GameObject ) MonoBehaviour.Instantiate( Resources.Load( "Prefabs/EmptyPrefab" ) );
		container.transform.position = transform.position + P0;
		container.transform.localScale = scale * Vector3.one;
		container.transform.parent = transform;
		container.transform.rotation = Quaternion.Euler( R0 );

		symbol = ( GameObject ) MonoBehaviour.Instantiate( Resources.Load( path ) );
		symbol.transform.parent = container.transform;
		symbol.layer = 16;
		symbol.GetComponent<Renderer>().enabled = false;

		animation = ( Animation ) symbol.GetComponent( "Animation" );
		StartCoroutine( "DelayedStart" );
	}

	//------------------------------------------------------------------------
	public void OnMouseDown()
	{
		if( Click != null )
		{
			Click();
		}
	}

	//------------------------------------------------------------------------
	private IEnumerator DelayedStart()
	{
		yield return new WaitForSeconds( DELAY );
		symbol.GetComponent<Renderer>().enabled = true;
		animation.Play();
	}

	//------------------------------------------------------------------------
	virtual public void Deactivate()
	{
		if( symbol != null )
		{
			GameObject.Destroy( symbol );
			symbol = null;
			content = null;
			animation = null;
		}
	}

	//------------------------------------------------------------------------
	virtual public void Destroy()
	{
		Deactivate();
	}
}