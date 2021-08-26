using UnityEngine;

//----------------------------------------------------------------------------
// ContactImageBehaviour
//----------------------------------------------------------------------------
public class ContactImageBehaviour : MonoBehaviour
{
	public delegate void ClickHandler();
	public event ClickHandler Click;

	private const float SIZE         = 7.0f;
	private const float Z0           = 3.2f;
	private const float DISTANCE     = 25.0f;
	private const float ACCELERATION = 0.05f;
	private const float STRENGTH     = 0.05f;
	private const float FRICTION     = 0.45f;
	private const float STOP_LIMIT   = 0.001f;

	private Vector3 P;
	private SpringSlider slider;

	//------------------------------------------------------------------------
	public void Awake()
	{
		transform.localScale = SIZE * Vector3.one;
	}

	//------------------------------------------------------------------------
	public void Init( float tunnelLength, Texture2D texture )
	{
		GetComponent<Renderer>().material.color = Color.white;
		GetComponent<Renderer>().material.shader = Shader.Find( "Transparent/Diffuse" );
		GetComponent<Renderer>().material.mainTexture = texture;

		slider = new SpringSlider( Z0, ACCELERATION, STRENGTH, FRICTION, STOP_LIMIT );
		slider.Slide( tunnelLength + DISTANCE );

		Update();
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
	public void FixedUpdate()
	{
		slider.Update();
	}

	//------------------------------------------------------------------------
	public void Update()
	{
		P.z = slider.value;
		transform.position = P;
	}

	//------------------------------------------------------------------------
	public void Destroy()
	{
		slider = null;
	}
}