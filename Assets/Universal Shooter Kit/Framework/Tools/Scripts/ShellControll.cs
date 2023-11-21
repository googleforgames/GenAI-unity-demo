// GercStudio
// © 2018-2020

using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GercStudio.USK.Scripts
{

	public class ShellControll : MonoBehaviour
	{
		private AudioSource _audio;
		private Rigidbody _rigidbody;
		private float ShellSpeed;
		[HideInInspector] public Transform ShellPoint;

		void Start()
		{
			_rigidbody = gameObject.AddComponent<Rigidbody>();
			_audio = gameObject.AddComponent<AudioSource>();
			_audio.spatialBlend = 1;
			_audio.volume = 0.5f;
			_audio.minDistance = 1;
			_audio.maxDistance = 100;
			var collider = gameObject.AddComponent<BoxCollider>();
			collider.isTrigger = true;
			collider.size *= 5;
			transform.parent = null;

			_rigidbody.velocity = ShellPoint.forward * Random.Range(1f, 3f);
			_rigidbody.useGravity = true;
			_rigidbody.isKinematic = false;
			
			transform.RotateAround(transform.position, transform.right, Random.Range(-30,30));
			transform.RotateAround(transform.position, transform.up, Random.Range(-20,20));
		}
		

		private void OnTriggerEnter(Collider other)
		{
			if (other.GetComponent<Surface>())
			{
				var surface = other.GetComponent<Surface>();
				
				Destroy(gameObject.GetComponent<BoxCollider>());
				gameObject.GetComponent<Rigidbody>().isKinematic = false;

				StartCoroutine(RigidbodyTimeout());

				if(surface.ShellDropSounds.Count > 0)
					_audio.PlayOneShot(surface.ShellDropSounds[Random.Range(0, surface.ShellDropSounds.Count - 1)]);
				
				Destroy(gameObject, 10);
			}
		}

		IEnumerator RigidbodyTimeout()
		{
			yield return new WaitForSeconds(0.5f);
			gameObject.GetComponent<Rigidbody>().isKinematic = true;
			StopCoroutine(RigidbodyTimeout());
		}
	}

}


