using UnityEngine;

//----------------------------------------------------------------------------
// SpringSlider
//----------------------------------------------------------------------------
public class SpringSlider
{
	public delegate void FinishHandler();
	public event FinishHandler Finish;

	public float value
	{
		get
		{
			return p;
		}
	}

	protected bool active;
	protected float p;
	protected float v;
	protected float a;
	protected float f;
	protected float p1;
	protected float acceleration;
	protected float strength;
	protected float friction;
	protected float stopLimit;

	//------------------------------------------------------------------------
	public SpringSlider( float startValue, float acceleration, float strength, float friction, float stopLimit )
	{
		p = p1 = startValue;
		this.acceleration = acceleration;
		this.strength = strength;
		this.friction = friction;
		this.stopLimit = stopLimit;
	}

	//------------------------------------------------------------------------
	public virtual void Slide( float value )
	{
		f = 0.0f;
		p1 = value;
		active = true;
	}

	//------------------------------------------------------------------------
	public virtual void Update()
	{
		if( active )
		{
			if( f < 1.0f - acceleration )
			{
				f += acceleration;
			}
			else
			{
				f = 1.0f;
			}

			a = f * strength * ( p1 - p ) - friction * v;
			v += a;
			p += v;
	
			if( Mathf.Abs( a ) + Mathf.Abs( v ) <= stopLimit )
			{
				active = false;
				p = p1;
				f = a = v = 0;

				if( Finish != null )
				{
					Finish();
				}
			}
		}
	}
}