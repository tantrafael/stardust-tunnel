#define NAVIGATION_PACKAGE

using UnityEngine;
using System.Collections;

//----------------------------------------------------------------------------
// UserBehaviour
//----------------------------------------------------------------------------
public class UserBehaviour : MonoBehaviour
{
	private const float slideFactor  = 220.0f;
	private const float forceFactor  = 500.0f;
	private const float dragFactor   = 300.0f;
	private const float scrollFactor = 75.0f;
	private const float stopLimit    = 0.01f;

	private bool slide;
	private bool handleInput;
	private Vector3 P;
	private Vector3 D;
	private Vector3 F;

	//------------------------------------------------------------------------
	public void Start()
	{
	//	Camera imageCamera;

		slide = false;
		handleInput = true;

	//	imageCamera = transform.Find( "ImageCamera" ).gameObject.camera;
	//	imageCamera.cullingMask = 12;
	}

	//------------------------------------------------------------------------
	public void Slide( Vector3 position )
	{
		P = position;
		slide = true;
	}

	//------------------------------------------------------------------------
	public void SetInputHandling( bool value )
	{
		handleInput = value;
	}

	//------------------------------------------------------------------------
	public void FixedUpdate()
	{
		if( slide )
		{
			D = P - transform.position;
	
			if( D.sqrMagnitude + GetComponent<Rigidbody>().velocity.sqrMagnitude > stopLimit )
			{
				F = slideFactor * D;
				GetComponent<Rigidbody>().AddForce( F );
			}
			else
			{
				slide = false;
			}
		}
	}

	//-----------------------------------------------------------------------------
	public void LateUpdate()
	{
		if( handleInput && !slide )
		{
#if NAVIGATION_PACKAGE
			NavigationManager navigationManager = Main.GetSingleton().GetNavigationManager();
			Vector2 navigationJoystickInput = navigationManager.GetJoystickInput();
			Vector2 navigationDraggingInput = navigationManager.GetDraggingInput();
			float navigationWheelInput = navigationManager.GetWheelInput();
#else
			Vector2 navigationJoystickInput = Vector2.zero;
			Vector2 navigationDraggingInput = Vector2.zero;
			float navigationWheelInput = 0;
#endif
	
			float verticalInput = 3.0f * Input.GetAxis( "Vertical" ) + navigationJoystickInput.y - dragFactor * navigationDraggingInput.y + scrollFactor * navigationWheelInput;
	
			if( verticalInput != 0.0f )
			{
				slide = false;
				F.z = forceFactor * verticalInput;
				GetComponent<Rigidbody>().AddForce( F );
			}
		}
	}
}