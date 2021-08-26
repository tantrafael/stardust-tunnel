using UnityEngine;
using System;
using System.Collections.Generic;

//----------------------------------------------------------------------------
// TunnelManager
//----------------------------------------------------------------------------
public class TunnelManager
{
	public delegate void InteractionActivationHandler( Interaction interaction );
	public delegate void ContactImageClickHandler( ContactImage contactImage );
	public delegate void SearchGrabHandler( bool value );
	public event InteractionActivationHandler InteractionActivation;
	public event ContactImageClickHandler ContactImageClick;
	public event SearchGrabHandler SearchGrab;

	private const float TIME_FACTOR = 8.0f;

	private int nrInteractionTypes;
	private Tunnel tunnel;
	private Tunnel.InteractionActivationHandler interactionActivationHandler;
	private Tunnel.ContactImageClickHandler contactImageClickHandler;
	private Tunnel.SearchGrabHandler searchGrabHandler;

	//------------------------------------------------------------------------
	public void Init( int contactId )
	{
		Database database;
		DatabaseContact databaseContact;
		Texture2D contactImageTexture;
		List<Interaction> interactions;
	//	List<DatabaseInteraction> databaseInteractions;
		int N;
		int id;
		int type;
		DateTime time;
		Interaction interaction;
	//	DatabaseInteraction databaseInteraction;

		database = Main.GetSingleton().GetDatabase();
		databaseContact = database.GetContactById( contactId, true );
		contactImageTexture = databaseContact.GetTexture();

		interactions = new List<Interaction>();
		nrInteractionTypes = 5;
		time = DateTime.Now.AddHours( -TIME_FACTOR * UnityEngine.Random.value );
		N = Mathf.FloorToInt( 100.0f + UnityEngine.Random.value * 100.0f );

		for( uint i = 0; i < N; i++ )
		{
			id = Mathf.FloorToInt( UnityEngine.Random.value * 1000.0f );

			if( UnityEngine.Random.value < 0.667f )
			{
				type = Interaction.IMAGE;
			}
			else
			{
				type = Mathf.FloorToInt( UnityEngine.Random.value * ( nrInteractionTypes - 1 ) );
			}

			interaction = new Interaction( id, time, type );
			interactions.Add( interaction );
			time = time.AddHours( -TIME_FACTOR * UnityEngine.Random.value );
		}

		interactionActivationHandler = new Tunnel.InteractionActivationHandler( HandleInteractionActivation );
		contactImageClickHandler = new Tunnel.ContactImageClickHandler( HandleContactImageClick );
		searchGrabHandler = new Tunnel.SearchGrabHandler( HandleSearchGrab );

		tunnel = new Tunnel();
		tunnel.Init( interactions, contactImageTexture );
		tunnel.InteractionActivation += interactionActivationHandler;
		tunnel.ContactImageClick += contactImageClickHandler;
		tunnel.SearchGrab += searchGrabHandler;
	}

	//------------------------------------------------------------------------
	public void AddSearch( string searchString, Vector2 point )
	{
		tunnel.AddSearch( searchString, point );
	}

	//------------------------------------------------------------------------
	private void HandleInteractionActivation( Interaction interaction )
	{
		if( InteractionActivation != null )
		{
			InteractionActivation( interaction );
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
	private void HandleSearchGrab( bool value )
	{
		if( SearchGrab != null )
		{
			SearchGrab( value );
		}
	}

	//------------------------------------------------------------------------
	public bool IsPositionThroughStart( Vector3 position )
	{
		return tunnel.IsPositionThroughStart( position );
	}

	//------------------------------------------------------------------------
	public bool IsPositionThroughEnd( Vector3 position )
	{
		return tunnel.IsPositionThroughEnd( position );
	}

	//------------------------------------------------------------------------
	public void Update()
	{
		if( tunnel != null )
		{
			tunnel.Update();
		}
	}

	//------------------------------------------------------------------------
	public void Destroy()
	{
		tunnel.InteractionActivation -= interactionActivationHandler;
		tunnel.ContactImageClick -= contactImageClickHandler;
		tunnel.SearchGrab -= searchGrabHandler;
		interactionActivationHandler = null;
		contactImageClickHandler = null;
		searchGrabHandler = null;
		tunnel.Destroy();
		tunnel = null;
	}
}