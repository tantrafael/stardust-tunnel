using UnityEngine;

//----------------------------------------------------------------------------
// InteractionThumbBehaviour
//----------------------------------------------------------------------------
public class InteractionThumbBehaviour : MonoBehaviour
{
	public delegate void ClickHandler();
	public event ClickHandler Click;

	private Color focusColor;
	private Color unfocusColor;

	//------------------------------------------------------------------------
	public void Init( Texture2D texture )
	{
		focusColor = Color.white;
		unfocusColor = new Color( 0.25f, 0.25f, 0.25f );

		GetComponent<Renderer>().material.color = focusColor;
		GetComponent<Renderer>().material.shader = Shader.Find( "Transparent/Diffuse" );
		GetComponent<Renderer>().material.mainTexture = texture;
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
	public void Focus()
	{
		GetComponent<Renderer>().material.color = focusColor;
	}

	//------------------------------------------------------------------------
	public void Unfocus()
	{
		GetComponent<Renderer>().material.color = unfocusColor;
	}

	//------------------------------------------------------------------------
	public void Destroy()
	{}
}