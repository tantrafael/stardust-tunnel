using UnityEngine;
using System;

//----------------------------------------------------------------------------
// Interaction
//----------------------------------------------------------------------------
public class Interaction
{
	public delegate void ActivationHandler( Interaction interaction, bool activation );
	public event ActivationHandler Activation;

	public static int CALL  = 0;
	public static int SMS   = 1;
	public static int MAIL  = 2;
	public static int IM    = 3;
	public static int IMAGE = 4;

	public DateTime time
	{
		get
		{
			return dateTime;
		}
	}

	public Vector3 position
	{
		get
		{
			return gameObject.transform.position;
		}
	}

	private int id;
	private DateTime dateTime;
	private int type;
	private GameObject gameObject;
	private GameObject info;
	private InteractionBehaviour behaviour;
	private InteractionBehaviour.ActivationHandler activationHandler;

	//------------------------------------------------------------------------
	public Interaction( int id, DateTime time, int type )
	{
		this.type = type;
		this.id = id;
		dateTime = time;
		this.type = type;
	}

	//------------------------------------------------------------------------
	public void Embody( float a, float r, float z )
	{
		gameObject = ( GameObject ) MonoBehaviour.Instantiate( Resources.Load( "Prefabs/EmptyPrefab" ) );
		activationHandler = new InteractionBehaviour.ActivationHandler( HandleActivation );

		behaviour = ( InteractionBehaviour ) gameObject.AddComponent( typeof( InteractionBehaviour ) );
		behaviour.Init( a, r, z, type );
		behaviour.Activation += activationHandler;
	}

	//------------------------------------------------------------------------
	private void HandleActivation( bool value )
	{
		if( Activation != null )
		{
			Activation( this, value );
		}
	}

	//------------------------------------------------------------------------
	public void Activate()
	{
		if( behaviour )
		{
			behaviour.Activate();
		}
	}

	//------------------------------------------------------------------------
	public void Deactivate()
	{
		if( behaviour )
		{
			behaviour.Deactivate();
		}
	}

	//------------------------------------------------------------------------
	public void Focus()
	{
		if( behaviour )
		{
			behaviour.Focus();
		}
	}

	//------------------------------------------------------------------------
	public void Unfocus()
	{
		if( behaviour )
		{
			behaviour.Unfocus();
		}
	}

	//------------------------------------------------------------------------
	public void Turn( float angle )
	{
		if( behaviour )
		{
			behaviour.Turn( angle );
		}
	}

	//------------------------------------------------------------------------
	public void Destroy()
	{
		if( behaviour != null )
		{
			behaviour.Activation -= activationHandler;
			behaviour.Destroy();
			GameObject.Destroy( behaviour );
			behaviour = null;
		}

		activationHandler = null;

		if( gameObject != null )
		{
			GameObject.Destroy( gameObject );
			gameObject = null;
		}
	}
}