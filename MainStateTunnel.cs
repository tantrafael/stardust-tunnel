#define NAVIGATION_PACKAGE
#define GALAXY_PACKAGE
#define BREADCRUMB_PACKAGE
#define DATABASE_PACKAGE
#define GUI_PACKAGE

using UnityEngine;
using System.Collections;

public class MainStateTunnel : MainState
{
	//------------------------------------------------------------------------
	public void Init(int startContactId, int startPlanetNodeIndex, int endContactId)
	{
		m_startContactId = startContactId;
		m_startPlanetNodeIndex = startPlanetNodeIndex;
		m_endContactId = endContactId;

#if DATABASE_PACKAGE
		m_databaseTunnelSearchReceivedHandler = new Database.TunnelSearchReceivedHandler(OnDatabaseTunnelSearchReceived);
#endif

#if NAVIGATION_PACKAGE
		m_navigationManagerBarInputHandler = new NavigationManagerBarInputHandler(OnNavigationManagerBarInput);
#endif

#if GUI_PACKAGE
		m_hotboxSearchHandler = new GuiManager.SearchHandler( HandleHotboxSearch );
#endif
	}

	//------------------------------------------------------------------------
	public override void Enter()
	{
		m_user = new User();
		m_user.Init();

		m_interactionActivationHandler = new TunnelManager.InteractionActivationHandler( HandleInteractionActivation );
		m_contactImageClickHandler = new TunnelManager.ContactImageClickHandler( HandleContactImageClick );
		m_searchGrabHandler = new TunnelManager.SearchGrabHandler( HandleSearchGrab );

		m_tunnelManager = new TunnelManager();
		m_tunnelManager.Init( m_endContactId );
		m_tunnelManager.InteractionActivation += m_interactionActivationHandler;
		m_tunnelManager.ContactImageClick += m_contactImageClickHandler;
		m_tunnelManager.SearchGrab += m_searchGrabHandler;

		Input.ResetInputAxes();

	//	Main.GetSingleton().GetWebInterface().GetTunnel( 1 );

	/*
		RenderSettings.fogColor = Color.red;
		RenderSettings.fogDensity = 0.01f;
		RenderSettings.fog = true;
	*/

#if DATABASE_PACKAGE
		Main.GetSingleton().GetDatabase().m_tunnelReceivedHandlers += m_databaseTunnelSearchReceivedHandler;
#endif

#if NAVIGATION_PACKAGE
		Main.GetSingleton().GetNavigationManager().m_barInputHandlers += m_navigationManagerBarInputHandler;
#endif

#if GUI_PACKAGE
		Main.GetSingleton().GetGuiManager().Search += m_hotboxSearchHandler;
#endif
	}

	//------------------------------------------------------------------------
	public override void Leave()
	{
#if DATABASE_PACKAGE
		Main.GetSingleton().GetDatabase().m_tunnelReceivedHandlers -= m_databaseTunnelSearchReceivedHandler;
#endif

#if NAVIGATION_PACKAGE
		Main.GetSingleton().GetNavigationManager().m_barInputHandlers -= m_navigationManagerBarInputHandler;
		Main.GetSingleton().GetNavigationManager().AbortDraggingInput();
#endif

		m_user.Destroy();
		m_user = null;

		m_tunnelManager.InteractionActivation -= m_interactionActivationHandler;
		m_tunnelManager.ContactImageClick -= m_contactImageClickHandler;
		m_tunnelManager.SearchGrab -= m_searchGrabHandler;
		m_tunnelManager.Destroy();
		m_tunnelManager = null;
		m_interactionActivationHandler = null;
		m_contactImageClickHandler = null;
		m_searchGrabHandler = null;

		RenderSettings.fog = false;
	}

	//------------------------------------------------------------------------
	public override Vector3 GetCameraPosition()
	{
		return m_user.GetPosition();
	}

	//------------------------------------------------------------------------
	public override Quaternion GetCameraRotation()
	{
		return m_user.GetRotation();
	}

	//------------------------------------------------------------------------
	public override float GetCameraFov()
	{
		return m_user.GetFov();
	}

	//------------------------------------------------------------------------
	public override bool SwitchToGalaxy(int galaxyId)
	{
		Debug.Log("MainStateTunnel.SwitchToGalaxy(" + galaxyId + ")");

		m_user.Slide(new Vector3(0.0f, 0.0f, -10.0f));

		m_leaveMode = LEAVE_MODE_SWITCH_TO_GALAXY;
		m_leaveSwitchGalaxyId = galaxyId;

		return true;
	}

	//------------------------------------------------------------------------
	public override bool SwitchToPlanet(int galaxyId)
	{
		Debug.Log("MainStateTunnel.SwitchToPlanet(" + galaxyId + ")");

		m_user.Slide(new Vector3(0.0f, 0.0f, -10.0f));

		m_leaveMode = LEAVE_MODE_SWITCH_TO_PLANET;
		m_leaveSwitchGalaxyId = galaxyId;

		return true;
	}

	//------------------------------------------------------------------------
	public override bool SwitchToPlanetNode(int galaxyId, int planetNodeIndex)
	{
		Debug.Log("MainStateTunnel.SwitchToPlanetNode(" + galaxyId + "," + planetNodeIndex + ")");

		if (m_startContactId == galaxyId && m_startPlanetNodeIndex == planetNodeIndex)
			return true;

		m_user.Slide(new Vector3(0.0f, 0.0f, -10.0f));

		m_leaveMode = LEAVE_MODE_SWITCH_TO_PLANET_NODE;
		m_leaveSwitchGalaxyId = galaxyId;
		m_leaveSwitchPlanetNodeIndex = planetNodeIndex;

		return true;
	}

	//------------------------------------------------------------------------
	public override void Update()
	{
		bool leave = false;
		int leaveContactId = -1;

		Vector3 userPosition = m_user.GetPosition();
		if (m_tunnelManager.IsPositionThroughStart(userPosition))
		{
			WebInterface.DebugString("Through tunnel start");
			leave = true;
			leaveContactId = m_startContactId;
		}
		else if (m_tunnelManager.IsPositionThroughEnd(userPosition))
		{
			WebInterface.DebugString("Through tunnel end");
			leave = true;
			leaveContactId = m_endContactId;
		}

		m_tunnelManager.Update();

		if (leave)
		{
#if GALAXY_PACKAGE
			MainStateGalaxy mainStateGalaxy = new MainStateGalaxy();
			if (m_leaveMode == LEAVE_MODE_NORMAL)
				mainStateGalaxy.Init(MainStateGalaxy.START_MODE_PLANET, leaveContactId, MainStateGalaxy.START_SWITCH_NONE, -1, -1);
			else if (m_leaveMode == LEAVE_MODE_SWITCH_TO_GALAXY)
				mainStateGalaxy.Init(MainStateGalaxy.START_MODE_PLANET, leaveContactId, MainStateGalaxy.START_SWITCH_GALAXY, m_leaveSwitchGalaxyId, -1);
			else if (m_leaveMode == LEAVE_MODE_SWITCH_TO_PLANET)
				mainStateGalaxy.Init(MainStateGalaxy.START_MODE_PLANET, leaveContactId, MainStateGalaxy.START_SWITCH_PLANET, m_leaveSwitchGalaxyId, -1);
			else if (m_leaveMode == LEAVE_MODE_SWITCH_TO_PLANET_NODE)
				mainStateGalaxy.Init(MainStateGalaxy.START_MODE_PLANET, leaveContactId, MainStateGalaxy.START_SWITCH_PLANET_NODE, m_leaveSwitchGalaxyId, m_leaveSwitchPlanetNodeIndex);
			if (Main.GetSingleton().SwitchMainState(mainStateGalaxy))
			{
				if (m_leaveMode == LEAVE_MODE_NORMAL)
				{
#if BREADCRUMB_PACKAGE
					Breadcrumb breadcrumb = new Breadcrumb();
					breadcrumb.Init(Breadcrumb.BREADCRUMB_TYPE_PLANET, "Planet " + leaveContactId, leaveContactId);
					Main.GetSingleton().GetBreadcrumbManager().AddBreadcrumb(breadcrumb);
#endif
				}
			}
#endif
		}
	}

	//------------------------------------------------------------------------
	private void HandleInteractionActivation( Interaction interaction )
	{
		m_user.View( interaction.position );
	}

	//------------------------------------------------------------------------
	private void HandleContactImageClick( ContactImage contactImage )
	{
		m_user.View( contactImage.position );
	}

	//------------------------------------------------------------------------
	private void HandleSearchGrab( bool value )
	{
		m_user.SetInputHandling( !value );
	}

#if DATABASE_PACKAGE
	//------------------------------------------------------------------------
	private void OnDatabaseTunnelSearchReceived(Database database, int id)
	{
		Debug.Log("MainStateGalaxy.OnDatabaseTunnelSearchReceived(" + id + ")");

	/*
		GalaxyManager galaxyManager = Main.GetSingleton().GetGalaxyManager();
		Galaxy selectedGalaxy = galaxyManager.GetGalaxyByGalaxyId(m_selectedGalaxyId);
		if (selectedGalaxy != null)
		{
			DatabaseTunnelSearch databaseTunnelSearch = Main.GetSingleton().GetDatabase().GetTunnelSearchById(id, true);
			selectedGalaxy.AddTunnelSearch(databaseTunnelSearch);
		}
	*/
	}
#endif

#if NAVIGATION_PACKAGE
	//------------------------------------------------------------------------
	private void OnNavigationManagerBarInput(string name)
	{
		Debug.Log("MainStateTunnel.OnNavigationManagerBarInput(" + name + ")");
		switch (name)
		{
			case "Galaxy":
				Main.GetSingleton().SwitchToGalaxy(Main.GetSingleton().GetHomePlanetId(), true);
				break;
			case "Planet":
				Main.GetSingleton().SwitchToPlanet(Main.GetSingleton().GetHomePlanetId(), true);
				break;
		}
	}
#endif

#if GUI_PACKAGE
	//------------------------------------------------------------------------
	private void HandleHotboxSearch( string searchString, Vector2 point )
	{
		if( m_tunnelManager != null && searchString != "" )
		{
			m_tunnelManager.AddSearch( searchString, point + searchPointCorrection );
		}
	}
#endif

	private const int LEAVE_MODE_NORMAL = 0;
	private const int LEAVE_MODE_SWITCH_TO_GALAXY = 1;
	private const int LEAVE_MODE_SWITCH_TO_PLANET = 2;
	private const int LEAVE_MODE_SWITCH_TO_PLANET_NODE = 3;

	private int m_startContactId = -1;
	private int m_startPlanetNodeIndex = -1;
	private int m_endContactId = -1;

	private int m_leaveMode = LEAVE_MODE_NORMAL;
	private int m_leaveSwitchGalaxyId = -1;
	private int m_leaveSwitchPlanetNodeIndex = -1;

	private User m_user = null;
	private TunnelManager m_tunnelManager = null;

	private TunnelManager.InteractionActivationHandler m_interactionActivationHandler = null;
	private TunnelManager.ContactImageClickHandler m_contactImageClickHandler = null;
	private TunnelManager.SearchGrabHandler m_searchGrabHandler = null;

	private Vector2 searchPointCorrection = new Vector2( 0.0f, -90.0f );

#if DATABASE_PACKAGE
	private Database.TunnelSearchReceivedHandler m_databaseTunnelSearchReceivedHandler = null;
#endif

#if NAVIGATION_PACKAGE
	private NavigationManagerBarInputHandler m_navigationManagerBarInputHandler = null;
#endif

#if GUI_PACKAGE
	private GuiManager.SearchHandler m_hotboxSearchHandler = null;
#endif
}
