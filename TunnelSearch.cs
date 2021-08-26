using UnityEngine;
using System.Collections.Generic;

//----------------------------------------------------------------------------
// TunnelSearch
//----------------------------------------------------------------------------
public class TunnelSearch
{
	public delegate void GrabHandler( bool value );
	public delegate void ActivationHandler( TunnelSearch tunnelSearch );
	public delegate void MovementHandler( TunnelSearch tunnelSearch, Vector3 movement );
	public event GrabHandler Grab;
	public event ActivationHandler Activation;
	public event MovementHandler Movement;

	private List<Interaction> result;
	private GameObject gameObject;
	private TunnelSearchBehaviour behaviour;
	private TunnelSearchBehaviour.GrabHandler grabHandler;
	private TunnelSearchBehaviour.ActivationHandler activationHandler;
	private TunnelSearchBehaviour.MovementHandler movementHandler;

	//------------------------------------------------------------------------
	public TunnelSearch( string searchString, Vector3 position, List<Interaction> result )
	{
		this.result = result;

		grabHandler = new TunnelSearchBehaviour.GrabHandler( HandleGrab );
		activationHandler = new TunnelSearchBehaviour.ActivationHandler( HandleActivation );
		movementHandler = new TunnelSearchBehaviour.MovementHandler( HandleMovement );

		gameObject = ( GameObject ) MonoBehaviour.Instantiate( Resources.Load( "Prefabs/TunnelSearchPrefab" ) );
		behaviour = ( TunnelSearchBehaviour ) gameObject.AddComponent( typeof( TunnelSearchBehaviour ) );
		behaviour.Init( position, searchString );
		behaviour.Grab += grabHandler;
		behaviour.Activation += activationHandler;
		behaviour.Movement += movementHandler;
	}

	//------------------------------------------------------------------------
	public float GetAngle()
	{
		return Mathf.Atan2( gameObject.transform.position.y, gameObject.transform.position.x );
	}

	//------------------------------------------------------------------------
	public List<Interaction> GetResult()
	{
		return result;
	}

	//------------------------------------------------------------------------
	public void SetPosition( Vector3 position )
	{
		gameObject.transform.position = position;
	//	behaviour.SetPosition( position );
	}

	//------------------------------------------------------------------------
	private void HandleGrab( bool value )
	{
		if( Grab != null )
		{
			Grab( value );
		}
	}

	//------------------------------------------------------------------------
	private void HandleActivation()
	{
		if( Activation != null )
		{
			Activation( this );
		}
	}

	//------------------------------------------------------------------------
	private void HandleMovement( Vector3 movement )
	{
		if( Movement != null )
		{
			Movement( this, movement );
		}
	}

	//------------------------------------------------------------------------
	public void Destroy()
	{
		if( result != null )
		{
			result.Clear();
			result = null;
		}

		if( behaviour != null )
		{
			behaviour.Grab -= grabHandler;
			behaviour.Activation -= activationHandler;
			behaviour.Movement -= movementHandler;
			behaviour.Destroy();
			GameObject.Destroy( behaviour );
			behaviour = null;
		}

		grabHandler = null;
		activationHandler = null;
		movementHandler = null;

		if( gameObject != null )
		{
			GameObject.Destroy( gameObject );
			gameObject = null;
		}
	}
}