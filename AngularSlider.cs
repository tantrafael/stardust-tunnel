using UnityEngine;

//----------------------------------------------------------------------------
// AngularSlider
//----------------------------------------------------------------------------
public class AngularSlider : SpringSlider
{
	private const float TWO_PI = 2.0f * Mathf.PI;

	private float d = 0.0f;

	//------------------------------------------------------------------------
	public AngularSlider( float startValue, float acceleration, float strength, float friction, float stopLimit ) : base( startValue, acceleration, strength, friction, stopLimit )
	{
		p = p1 = Deperiodize( startValue );
	}

	//------------------------------------------------------------------------
	public override void Slide( float value )
	{
		base.Slide( Deperiodize( value ) );
	}

	//------------------------------------------------------------------------
	public override void Update()
	{
		if( active )
		{
			d = p1 - p;

			if( d < -Mathf.PI )
			{
				d += TWO_PI;
			}

			if( d > Mathf.PI )
			{
				d -= TWO_PI;
			}

			if( f < 1.0f )
			{
				f += acceleration;
			}
			else
			{
				f = 1.0f;
			}

			a = f * strength * d - friction * v;
			v += a;
			p += v;

			if( Mathf.Abs( a ) + Mathf.Abs( v ) <= stopLimit )
			{
				active = false;
				p = p1;
				a = v = 0;

			/*
				if( base.Finish != null )
				{
					base.Finish();
				}
			*/
			}

		/*
			if( p > TWO_PI )
			{
				p -= TWO_PI; 
			}

			if( p < 0 )
			{
				p += TWO_PI; 
			}
		*/

			Deperiodize( p );
		}
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
}