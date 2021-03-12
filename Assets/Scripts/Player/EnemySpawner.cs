using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemy;
    public GameObject waveUI;
    public TextMeshProUGUI waveUItext;
    public TextMeshProUGUI enemyCountText;
    public TextMeshProUGUI wavesText;
    public Map map;
    public GameObject endScreen;

    int wave = 0;
    int startingEnemyCount;
    public int enemies = 0;
    bool spawning;
    bool finalWave = false;

    void Start()
    {
        SetStartingCount();
        StartCoroutine(Wave());
    }

    void Update()
    {
        if(DataContainer.Instance.isPlaying)
        {
            enemyCountText.text = "Total enemy: " + enemies;
            wavesText.text = "Wave: " + wave;

            if (wave > 0 && enemies == 0 && wave < DataContainer.Instance.enemyWaves && !spawning)
            {
                StartCoroutine(Wave());
            }
        }

        if(wave == DataContainer.Instance.enemyWaves && enemies == 0 && finalWave)
        {
            string completion = "Victory!";
            StartCoroutine(EndGame(completion));
            wavesText.text = "Wave: " + 0;
        }

        if(DataContainer.Instance.playerHealth == 0)
        {
            string completion = "Defeat!";
            StartCoroutine(EndGame(completion));
            wavesText.text = "Wave: " + 0;
        }
    }

    void SetStartingCount()
    {
        switch (DataContainer.Instance.difficulty)
        {
            case 0:
                startingEnemyCount = 1;
                break;
            case 1:
                startingEnemyCount = 4;
                break;
            case 2:
                startingEnemyCount = 5;
                break;
        }
    }

    public void CountDownEnemy()
    {
        enemies -= 1;
    }

    IEnumerator Wave()
    {
        spawning = true;
        yield return new WaitForSeconds(5f);

        if (wave < DataContainer.Instance.enemyWaves)
        {
            wave += 1;
        }

        waveUI.SetActive(true);
        waveUItext.text = "Wave: " + wave;

        yield return new WaitForSeconds(3f);
        waveUI.SetActive(false);

        int spawnCount = wave * startingEnemyCount;
        enemies = spawnCount;

        while (spawnCount > 0)
        {
            Transform center = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
            int randomAngle = Random.Range(0, 360);
            Quaternion centerRotation = Quaternion.Euler(0, randomAngle, 0);
            Vector3 distanceFromCenter = new Vector3Int(0, 0, -50);
            Vector3 position = centerRotation * distanceFromCenter + center.position;

            if(map.ValidateSpawnPont(position))
            {
                Vector3Int spawnPosition = map.GetSpawnPosition(position);
                Instantiate(enemy, spawnPosition, Quaternion.identity);

                spawnCount -= 1;
            }
            yield return new WaitForSeconds(3f);
        }

        if(wave == DataContainer.Instance.enemyWaves)
        {
            finalWave = true;
        }

        spawning = false;
    }

    IEnumerator EndGame(string completion)
    {
        DataContainer.Instance.isPlaying = false;
        waveUItext.text = completion;
        waveUI.SetActive(true);
        yield return new WaitForSeconds(3f);
        endScreen.SetActive(true);
        Cursor.visible = true;
    }
}
