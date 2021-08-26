using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------
// Tunnel
//----------------------------------------------------------------------------
public class Tunnel
{
	public delegate void InteractionActivationHandler( Interaction interaction );
	public delegate void ContactImageClickHandler( ContactImage contactImage );
	public delegate void SearchGrabHandler( bool value );
	public event InteractionActivationHandler InteractionActivation;
	public event ContactImageClickHandler ContactImageClick;
	public event SearchGrabHandler SearchGrab;

	private const float TWO_PI = 2.0f * Mathf.PI;
	private const float LENGTH_SCALE = 5.0f;
	private const float START_OFFSET = 9.0f;
	private const float EXTRA_TIME = START_OFFSET / LENGTH_SCALE;
	private const float RADIUS = 5.0f;
	private const float INTERACTION_RADIUS = RADIUS - 0.7f;
	private const float ANGLE_INCREMENT = 0.15f * Mathf.PI;
	private const float LABEL_RADIUS = RADIUS - 0.1f;
	private const float LABEL_ANGLE = 1.5f * Mathf.PI;
	private const float LABEL_OFFSET = 0.5f;
	private const float LABEL_SIZE = 0.1f;
	private const float MAGNET_RADIUS = RADIUS - 0.6f;
	private const float LABEL_SPACE = 0.11f * Mathf.PI;
	private const float SEARCH_SPACE = 0.5f * Mathf.PI;

	private List<Interaction> interactions;
//	private List<DatabaseInteraction> databaseInteractions;
//	private List<GameObject> interactions;
	private List<GameObject> segmentLabels;
	private List<TunnelSearch> searches;
	private GameObject gameObject;
	private Interaction activeInteraction;
	private Interaction.ActivationHandler interactionActivationHandler;
	private ContactImage contactImage;
	private ContactImage.ClickHandler contactImageClickHandler;
	private TunnelSearch.GrabHandler searchGrabHandler;
	private TunnelSearch.ActivationHandler searchActivationHandler;
	private TunnelSearch.MovementHandler searchMovementHandler;
	private float length;
	private float timeSpan;
	private float angle;
	private DateTime startTime;
//	private bool populatingLabels;
	private bool populatingInteractions;
	private int interactionCounter;
	private float fakeReceiveImageTime;
	private Camera searchCamera;
	private Vector3 P;

	//------------------------------------------------------------------------
	public void Init( List<Interaction> interactions, Texture2D contactImageTexture )
//	public void Init( List<DatabaseInteraction> databaseInteractions, Texture2D contactImageTexture )
	{
		GameObject meshGameObject;
		GameObject segmentLabel;
		GameObject labelText;
		GameObject spotLight;
		MeshFilter meshFilter;
		DateTime latestTime;
		DateTime earliestTime;
		DateTime endTime;
		DateTime firstLabelTime;
		DateTime labelTime;
		Vector3 size;
		Vector3 scaleVector;
		Vector3 P;
		float effectiveTimeSpan;
		float offset;
		float t;
		TextMesh textMesh;
		Material wallMaterial;
		Material segmentMaterial;
		int difference;

		this.interactions = interactions;
	//	this.databaseInteractions = databaseInteractions;

		gameObject = ( GameObject ) MonoBehaviour.Instantiate( Resources.Load( "Prefabs/TunnelPrefab" ) );
		gameObject.layer = 12;

		meshGameObject = gameObject.transform.Find( "Mesh" ).gameObject;
		meshFilter = ( MeshFilter ) meshGameObject.GetComponent( typeof( MeshFilter ) );
		size = meshFilter.mesh.bounds.size;

		searchCamera = gameObject.transform.Find( "SearchCamera" ).gameObject.GetComponent<Camera>();

		latestTime = interactions[ 0 ].time;
		earliestTime = interactions[ interactions.Count - 1 ].time;
		startTime = DateTime.Today.AddDays( 1.0f + EXTRA_TIME );
		endTime = earliestTime.AddHours( -earliestTime.Hour ).AddMinutes( -earliestTime.Minute ).AddSeconds( -earliestTime.Second );
		timeSpan = ( float ) ( startTime - endTime ).TotalDays;
		length = LENGTH_SCALE * timeSpan;

		// Scale model
		scaleVector = new Vector3();
		scaleVector.x = scaleVector.z = 2.0f * RADIUS / size.x;
		scaleVector.y = length / size.y;
		meshGameObject.transform.localScale = scaleVector;

		// Place light
		spotLight = gameObject.transform.Find( "PointLight" ).gameObject;
		spotLight.transform.localPosition = new Vector3( 0.0f, 0.0f, length );

	/*
		// Segments
		offset = Mathf.Floor( EXTRA_TIME ) - EXTRA_TIME;
		segmentMaterial = meshGameObject.renderer.materials[ 1 ];
		segmentMaterial.mainTextureOffset = new Vector2( 0.0f, offset );
		segmentMaterial.mainTextureScale = new Vector2( 1.0f, timeSpan );
	*/

		// Labels
		segmentLabels = new List<GameObject>();
		labelTime = firstLabelTime = DateTime.Today.AddDays( 1.0f );
	//	labelTime = firstLabelTime = DateTime.Today;

	//	populatingLabels = true;

		while( labelTime > earliestTime )
		{
			t = ( ( float ) ( startTime - labelTime ).TotalDays + LABEL_OFFSET ) / timeSpan;
		//	t = ( ( float ) ( startTime - labelTime ).TotalDays ) / timeSpan;
	
			P = new Vector3();
			P.x = LABEL_RADIUS * Mathf.Cos( LABEL_ANGLE );
			P.y = LABEL_RADIUS * Mathf.Sin( LABEL_ANGLE );
			P.z = t * length;

			segmentLabel = ( GameObject ) MonoBehaviour.Instantiate( Resources.Load( "Prefabs/TextPrefab" ) );
			segmentLabel.transform.position = P;
			segmentLabel.transform.localScale = LABEL_SIZE * Vector3.one;
			textMesh = ( TextMesh ) segmentLabel.GetComponent( typeof( TextMesh ) );

			difference = ( int ) ( firstLabelTime - labelTime ).TotalDays;
			
			switch( difference )
			{
				case 0:
				{
					textMesh.text = "Today";
					break;
				}

				case 1:
				{
					textMesh.text = "Yesterday";
					break;
				}

				default:
				{
				//	textMesh.text = labelTime.ToString( "dddd MMM d yyyy" );
					textMesh.text = labelTime.AddDays( -1.0f ).ToString( "dddd MMM d yyyy" );
					break;
				}
			}

			segmentLabels.Add( segmentLabel );
			labelTime = labelTime.AddDays( -1.0f );
		}

		// Contact image
		contactImageClickHandler = new ContactImage.ClickHandler( HandleContactImageClick );
		contactImage = new ContactImage();
		contactImage.Init( length, contactImageTexture );
		contactImage.Click += contactImageClickHandler;

		// Interactions
		interactionCounter = 0;
		angle = 1.7f * Mathf.PI;
		interactionActivationHandler = new Interaction.ActivationHandler( HandleInteractionActivation );
		populatingInteractions = true;

		// Search
		searches = new List<TunnelSearch>();
		searchGrabHandler = new TunnelSearch.GrabHandler( HandleSearchGrab );
		searchActivationHandler = new TunnelSearch.ActivationHandler( DisplaySearch );
		searchMovementHandler   = new TunnelSearch.MovementHandler( HandleSearchMovement );
	}

	//------------------------------------------------------------------------
	public void AddInteraction( Interaction interaction )
//	public void AddInteraction( DatabaseInteraction databaseInteraction )
	{
		float t;
		Vector3 P;
	//	GameObject interaction;
		InteractionBehaviour behaviour;

		t = ( float ) ( startTime - interaction.time ).TotalDays / timeSpan;
		interaction.Embody( angle, INTERACTION_RADIUS, t * length );
		interaction.Activation += interactionActivationHandler;
	/*
		interaction = ( GameObject ) MonoBehaviour.Instantiate( Resources.Load( "Prefabs/EmptyPrefab" ) );
		behaviour = ( InteractionBehaviour ) gameObject.AddComponent( typeof( InteractionBehaviour ) );
		behaviour.Init( databaseInteraction, angle, INTERACTION_RADIUS, t * length );
	*/

		angle += ANGLE_INCREMENT;

		while( AngularDistance( angle, LABEL_ANGLE ) < LABEL_SPACE )
		{
			angle += ANGLE_INCREMENT;
		}
	}

	//------------------------------------------------------------------------
	public void HandleInteractionActivation( Interaction interaction, bool activation )
	{
		if( activation == true )
		{
			if( activeInteraction != null )
			{
				activeInteraction.Deactivate();
			}
	
			activeInteraction = interaction;
	
			if( InteractionActivation != null )
			{
				InteractionActivation( interaction );
			}
		}
		else
		{
			activeInteraction = null;
		}
	}

	//------------------------------------------------------------------------
	private void HandleContactImageClick( ContactImage contactImage )
	{
		if( ContactImageClick != null )
		{
			ContactImageClick( contactImage );
		}
	}

	//------------------------------------------------------------------------
	public void AddSearch( string searchString, Vector2 position )
	{
		Vector3 P;
		List<Interaction> result;
		TunnelSearch search;

		P = searchCamera.ScreenToWorldPoint( new Vector3( position.x, position.y, 0.0f ) );
		P.y = -P.y;
		P.z = 0.0f;

		result = new List<Interaction>();

		foreach( Interaction interaction in interactions )
		{
			if( UnityEngine.Random.value < 0.1f )
			{
				result.Add( interaction );
			}
		}

		search = new TunnelSearch( searchString, P, result );
		search.Grab += searchGrabHandler;
		search.Activation += searchActivationHandler;
		search.Movement += searchMovementHandler;
		searches.Add( search );

		DisplaySearch( search );
	}

	//------------------------------------------------------------------------
	public void DisplaySearch( TunnelSearch search )
	{
		List<Interaction> result;
		List<Interaction> remaining;
		float searchAngle;
		float a;

		result = search.GetResult();
		searchAngle = search.GetAngle();

		remaining = new List<Interaction>( interactions );

		foreach( Interaction interaction in result )
		{
			interaction.Turn( searchAngle );
			interaction.Focus();
			remaining.Remove( interaction );
		}

		a = searchAngle + 0.5f * Mathf.PI;

		while( AngularDistance( a, LABEL_ANGLE ) < LABEL_SPACE )
		{
			a += ANGLE_INCREMENT;
		}

		foreach( Interaction interaction in remaining )
		{
			interaction.Turn( a );
			interaction.Unfocus();

			a += ANGLE_INCREMENT;

			while( ( AngularDistance( a, searchAngle ) < SEARCH_SPACE ) || ( AngularDistance( a, LABEL_ANGLE ) < LABEL_SPACE ) )
			{
				a += ANGLE_INCREMENT;
			}
		}

		remaining.Clear();
	}

	//------------------------------------------------------------------------
	private void HandleSearchGrab( bool value )
	{
		if( SearchGrab != null )
		{
			SearchGrab( value );
		}
	}

	//------------------------------------------------------------------------
	private void HandleSearchMovement( TunnelSearch search, Vector3 movement )
	{
		P = searchCamera.ScreenToWorldPoint( Input.mousePosition );
		P.z = 0;
		search.SetPosition( P );
	}

	//------------------------------------------------------------------------
	private float AngularDistance( float a, float b )
	{
		float d;

		d = Mathf.Abs( a - b );

		while( d > TWO_PI )
		{
			d -= TWO_PI;
		}

		if( d > Mathf.PI )
		{
			d = TWO_PI - d;
		}

		return d;
	}

	//------------------------------------------------------------------------
	public bool IsPositionThroughStart( Vector3 position )
	{
		return ( position.z < -1.0f );
	}

	//------------------------------------------------------------------------
	public bool IsPositionThroughEnd( Vector3 position )
	{
		return ( position.z > length + 1.0f );
	}

	//------------------------------------------------------------------------
	public void Update()
	{
	/*
		if( populatingLabels )
		{
			AddLabel();
		}
	*/

		if( populatingInteractions )
		{
			fakeReceiveImageTime += Time.deltaTime;

			if( fakeReceiveImageTime >= 0.01f )
			{
				AddInteraction( interactions[ interactionCounter ] );
				interactionCounter++;
				fakeReceiveImageTime = 0.0f;

				if( interactionCounter == interactions.Count )
				{
					populatingInteractions = false;
				}
			}
		}
	}

	//------------------------------------------------------------------------
	public void Destroy()
	{
		foreach( TunnelSearch search in searches )
		{
			search.Grab -= searchGrabHandler;
			search.Activation -= searchActivationHandler;
			search.Movement -= searchMovementHandler;
			search.Destroy();
		}

		searchGrabHandler = null;
		searchActivationHandler = null;
		searchMovementHandler = null;
		searches.Clear();
		searches = null;

		if( interactions != null )
		{
			foreach( Interaction interaction in interactions )
			{
				interaction.Activation -= interactionActivationHandler;
				interaction.Destroy();
			}
	
			interactions.Clear();
			interactions = null;
		}

		interactionActivationHandler = null;

		if( contactImage != null )
		{
			contactImage.Click -= contactImageClickHandler;
			contactImage.Destroy();
			contactImage = null;
		}

		contactImageClickHandler = null;

		if( segmentLabels != null )
		{
			foreach( GameObject segmentLabel in segmentLabels )
			{
				GameObject.Destroy( segmentLabel );
			}
	
			segmentLabels.Clear();
			segmentLabels = null;
		}

		if( gameObject != null )
		{
			GameObject.Destroy( gameObject );
			gameObject = null;
		}
	}
}