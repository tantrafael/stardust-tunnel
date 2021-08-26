using UnityEngine;
using System;

//----------------------------------------------------------------------------
// InteractionBehaviour
//----------------------------------------------------------------------------
public class InteractionBehaviour : MonoBehaviour
{
	public delegate void ActivationHandler( bool activation );
	public event ActivationHandler Activation;

	private const float TWO_PI = 2.0f * Mathf.PI;
	private const float CLOSING_DISTANCE  = 6.25f;
	private const float ANGULAR_STRENGTH  = 0.078f;
	private const float RADIAL_STRENGTH   = 0.12f;
	private const float AXIAL_STRENGTH    = 0.078f;
	private const float FLIP_STRENGTH     = 0.078f;
	private const float ACCELERATION      = 0.2f;
	private const float FRICTION          = 0.45f;
	private const float STOP_LIMIT        = 0.0001f;
	private const float INTRO_ANGLE       = -0.35f;
	private const float INTRO_RADIUS      = -0.5f;
	private const float ACTIVATION_ANGLE  = 0.25f * Mathf.PI;
	private const int ACTIVATING   = 0;
	private const int ACTIVATED    = 1;
	private const int DEACTIVATING = 2;
	private const int DEACTIVATED  = 3;
	private const int BACK_LAYER   = 15;
	private const int FRONT_LAYER  = 16;

	private float a0;
	private float a1;
	private float r0;
	private float r1;
	private float z0;
	private float z1;
	private float w0;
	private float w1;
	private float a;
	private float r;
	private float z;
	private float w;
	private Vector3 P;
	private Quaternion R;
	private SpringSlider angularSlider;
	private SpringSlider radialSlider;
	private SpringSlider axialSlider;
	private SpringSlider flipSlider;
	private SpringSlider.FinishHandler finishHandler;
	private int type;
	private int state;
	private GameObject thumb;
	private GameObject info;
	private InteractionThumbBehaviour thumbBehaviour;
	private InteractionInfoBehaviour  infoBehaviour;
	private InteractionThumbBehaviour.ClickHandler thumbClickHandler;
	private InteractionInfoBehaviour.ClickHandler  infoClickHandler;
	private Texture2D thumbTexture;

	//------------------------------------------------------------------------
	public void Init( float a, float r, float z, int type )
	{
		int N;
		int imageNr;
		string url;
		string imageName;
		float startAngle;
		float startRadius;

		this.type = type;

		url = "Images/";

		if( type == Interaction.IMAGE )
		{
			url += "Images/";

			N = 47;
			imageNr = 1 + ( int ) Mathf.Floor( N * UnityEngine.Random.value );
			imageName = Convert.ToString( imageNr );
	
			if( imageNr < 10 )
			{
				imageName = "0" + imageName;
			}
	
			if( imageNr < 100 )
			{
				imageName = "0" + imageName;
			}
		}
		else
		{
			url += "Actions/";
			imageName = Convert.ToString( type );
		}

		url += imageName;
		thumbTexture = ( Texture2D ) Resources.Load( url );

		state = DEACTIVATED;

		a0 = Deperiodize( a );
		a1 = a0 + ACTIVATION_ANGLE;

		if( a1 > Mathf.PI )
		{
			a1 += TWO_PI;
		}

		if( a1 > Mathf.PI )
		{
			a1 -= TWO_PI;
		}

		r0 = r;
		r1 = 0;
		z0 = z;
		z1 = z0 - CLOSING_DISTANCE;

		w0 = 0;
		w1 = Mathf.PI;

		startAngle  = a0 + INTRO_ANGLE;
		startRadius = r0 + INTRO_RADIUS;

		angularSlider = new AngularSlider( startAngle,  ACCELERATION, ANGULAR_STRENGTH, FRICTION, STOP_LIMIT );
		radialSlider  = new SpringSlider ( startRadius, ACCELERATION, RADIAL_STRENGTH,  FRICTION, STOP_LIMIT );
		axialSlider   = new SpringSlider ( z0,          ACCELERATION, AXIAL_STRENGTH,   FRICTION, STOP_LIMIT );
		flipSlider    = new SpringSlider ( w0,          ACCELERATION, FLIP_STRENGTH,    FRICTION, STOP_LIMIT );

		finishHandler = new SpringSlider.FinishHandler( HandleFinish );
		axialSlider.Finish += finishHandler;

		thumb = ( GameObject ) MonoBehaviour.Instantiate( Resources.Load( "Prefabs/TilePrefab" ) );
		thumb.transform.parent = this.transform;
		thumb.layer = BACK_LAYER;

		thumbBehaviour = ( InteractionThumbBehaviour ) thumb.AddComponent( typeof( InteractionThumbBehaviour ) );
		thumbBehaviour.Init( thumbTexture );
		thumbClickHandler = new InteractionThumbBehaviour.ClickHandler( HandleThumbClick );
		thumbBehaviour.Click += thumbClickHandler;

		Update();

		angularSlider.Slide( a0 );
		radialSlider.Slide( r0 );
	}

	//------------------------------------------------------------------------
	private float Deperiodize( float angle )
	{
		while( angle > TWO_PI )
		{
			angle -= TWO_PI;
		}

		while( angle < 0 )
		{
			angle += TWO_PI;
		}

		return angle;
	}

	//------------------------------------------------------------------------
	private void HandleThumbClick()
	{
		if( state == DEACTIVATED || state == DEACTIVATING )
		{
			Activate();
		}
		else
		{
			Deactivate();
		}
	}

	//------------------------------------------------------------------------
	private void HandleInfoClick()
	{
		Deactivate();
	}

	//------------------------------------------------------------------------
	private void HandleFinish()
	{
		if( state == ACTIVATING )
		{
			state = ACTIVATED;
		}

		if( state == DEACTIVATING )
		{
			state = DEACTIVATED;
			DetachInfo();
		}
	}

	//------------------------------------------------------------------------
	public void Activate()
	{
		state = ACTIVATING;

		angularSlider.Slide( a1 );
		radialSlider.Slide( r1 );
		axialSlider.Slide( z1 );

		if( type == Interaction.IMAGE )
		{
			thumb.layer = FRONT_LAYER;
		}
		else
		{
			flipSlider.Slide( w1 );
			AttachInfo();
		}

		if( Activation != null )
		{
			Activation( true );
		}
	}

	//------------------------------------------------------------------------
	public void Deactivate()
	{
		state = DEACTIVATING;

		angularSlider.Slide( a0 );
		radialSlider.Slide( r0 );
		axialSlider.Slide( z0 );

		if( type == Interaction.IMAGE )
		{
			thumb.layer = BACK_LAYER;
		}
		else
		{
			flipSlider.Slide( w0 );
			infoBehaviour.Deactivate();
		}

		if( Activation != null )
		{
			Activation( false );
		}
	}

	//------------------------------------------------------------------------
	private void AttachInfo()
	{
		string prefabPath;
		Type behaviourType;
	//	Texture2D texture;

		if( info == null )
		{
			prefabPath = "Prefabs/TilePrefab";
			behaviourType = typeof( InteractionInfoBehaviour );
	
		/*
			if( type == Interaction.IMAGE )
			{
				prefabPath += "TilePrefab";
				behaviourType = typeof( InteractionInfoBehaviour );
				texture = thumbTexture;
			}
			else
			{
				prefabPath += "TilePrefab";
				behaviourType = typeof( MobileInteractionInfoBehaviour );
				texture = ( Texture2D ) Resources.Load( "Textures/MobileInteractionInfo" );
			}
		*/
	
			info = ( GameObject ) MonoBehaviour.Instantiate( Resources.Load( prefabPath ) );
			info.transform.position = this.transform.position;
			info.transform.rotation = this.transform.rotation;
			info.transform.Rotate( 0, Mathf.PI * Mathf.Rad2Deg, 0 );
			info.transform.parent = this.transform;
			info.layer = FRONT_LAYER;
	
			infoBehaviour = ( InteractionInfoBehaviour ) info.AddComponent( behaviourType );
		//	infoBehaviour.Init( texture );
			infoBehaviour.Init( type );
			infoClickHandler = new InteractionInfoBehaviour.ClickHandler( HandleInfoClick );
			infoBehaviour.Click += infoClickHandler;
		}
	}

	//------------------------------------------------------------------------
	private void DetachInfo()
	{
		if( infoBehaviour )
		{
			infoBehaviour.Click -= infoClickHandler;
			infoClickHandler = null;
			infoBehaviour.Destroy();
			GameObject.Destroy( infoBehaviour );
		}

		if( info )
		{
			GameObject.Destroy( info );
			info = null;
		}
	}

	//------------------------------------------------------------------------
	public void Focus()
	{
		thumbBehaviour.Focus();
	}

	//------------------------------------------------------------------------
	public void Unfocus()
	{
		thumbBehaviour.Unfocus();
	}

	//------------------------------------------------------------------------
	public void Turn( float angle )
	{
		a0 = angle;
		a1 = a0 + ACTIVATION_ANGLE;
		angularSlider.Slide( angle );
	}

	//------------------------------------------------------------------------
	public void FixedUpdate()
	{
		angularSlider.Update();
		radialSlider.Update();
		axialSlider.Update();
		flipSlider.Update();
	}

	//------------------------------------------------------------------------
	public void Update()
	{
		a = angularSlider.value;
		r = radialSlider.value;
		z = axialSlider.value;
		w = flipSlider.value;

		P.x = r * Mathf.Cos( a );
		P.y = r * Mathf.Sin( a );
		P.z = z;

		R = Quaternion.Euler( 0, w * Mathf.Rad2Deg, 0 );

		transform.position = P;
		transform.rotation = R;
	}

	//------------------------------------------------------------------------
	public void Destroy()
	{
		if( thumbBehaviour != null )
		{
			thumbBehaviour.Click -= thumbClickHandler;
			thumbBehaviour.Destroy();
		}

		if( infoBehaviour != null )
		{
			infoBehaviour.Click -= infoClickHandler;
			infoBehaviour.Destroy();
		}
		
		if( thumb != null )
		{
			GameObject.Destroy( thumb );
		}

		if( info != null )
		{
			GameObject.Destroy( info );
		}

		thumbClickHandler = null;
		infoClickHandler  = null;

		axialSlider.Finish -= finishHandler;
		finishHandler = null;

		angularSlider = null;
		radialSlider  = null;
		axialSlider   = null;
	}
}