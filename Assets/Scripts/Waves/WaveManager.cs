using System.Collections.Generic;
using UnityEngine;

namespace Waves
{
	public class WaveManager : MonoBehaviour
	{
		[Header("Waves")]
		[SerializeField] private List<WaveSO> Waves;
		[SerializeField] private float TimeToStartFirstWave;

		private List<WaveSO> _wavesCopy = new();
		private WaveSO _currentWave;
		private float _waveSpawnTimer;

		private void Start()
		{
			_wavesCopy = new List<WaveSO>(Waves);
			_currentWave = Waves[0];
			_waveSpawnTimer = TimeToStartFirstWave;
		}

		private void Update()
		{
			SpawnWaves();
		}

		private void SpawnWaves()
		{
			_waveSpawnTimer -= Time.deltaTime;
			if (_waveSpawnTimer > 0)
				return;


		}
	}
}
