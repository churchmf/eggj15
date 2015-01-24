using UnityEngine;
using System.Collections;

//http://createathingunity.blogspot.co.uk/
public class Movement : MonoBehaviour
{
		int moveSpeed = 8;
		public bool haveControl = false;
	
		void FixedUpdate ()
		{
				if (haveControl) {
						float vert = Input.GetAxis ("Vertical");
						float horiz = Input.GetAxis ("Horizontal");
						
						if (Network.isServer) {
								movePlayer (vert, horiz);
						} else {
								networkView.RPC ("movePlayer", RPCMode.Server, vert, horiz);
						}
				}
		}
	
		[RPC]
		void movePlayer (float vert, float horiz)
		{
				Vector3 newVelocity = (transform.right * horiz * moveSpeed) + (transform.forward * vert * moveSpeed);
				Vector3 myVelocity = rigidbody.velocity;
				myVelocity.x = newVelocity.x;
				myVelocity.z = newVelocity.z;

				rigidbody.velocity = myVelocity;
				networkView.RPC ("updatePlayer", RPCMode.OthersBuffered, transform.position);
		}
		[RPC]
		void updatePlayer (Vector3 playerPos)
		{
				transform.position = playerPos;
		}
}