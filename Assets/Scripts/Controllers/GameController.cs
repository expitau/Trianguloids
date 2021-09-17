using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UI;
//using UnityEditor;

public class GameController : MonoBehaviour
{

    public HubController Hub; //Hub reference
    public InputManager inputManager; //Input manager reference

    [Header("Map Controls")]
    public Tilemap map; //Tilemap reference
    public Tile tile; //Tile prefab
    public float cutoff; //Percentage of map that is filled
    public float scale; //Scale of noise
    public int chunkSize; //Map size

    [Header("Spawn Controls")]
    public float spawnRadius; //Maximum area to spawn new objects
    public float SpawnRate; //Rate at which objects spawn
    private float TimeToSpawn;
    public Transform spawnSource; //Center of area where objects can spawn
    public GameObject FloaterPrefab; //Floater prefab
    public GameObject EnemyPrefab; //Enemy prefab

    [Header("Score Controls")]
    public int score; //Score amt
    public int highscore; //Highscore amt
    public Text ScoreText; //Score reference
    public Text HighScoreText; //Highscore reference

    [Header("Prefabs")]
    public Animator loadingScreenAnimator; //Loading animator
    public Animator endScreenAnimator; //End card animator

    public GameObject ParticleSystemPrefab; //Particles

    public GameObject Bullet_Holder; //Bullet container
    public GameObject Object_Holder; //Object container

    public List<AudioSource> audioSources; //Audio sources (children)

    public bool isLoading = true; //Is the map generating
    private int loadAmt = 0; //Percentage of map loaded

    public bool SFX = false; //Sound effects enabled
    public bool Music = false; //Music enabled


    //On start
    void Start()
    {
        highscore = PlayerPrefs.GetInt("Highscore", 0);
        SFX = PlayerPrefs.GetInt("SFX", 1) == 1;
        Music = PlayerPrefs.GetInt("Music", 1) == 1;
        HighScoreText.text = "Highscore: " + highscore;
        Restart();
    }

    //Menu item to clear highscore
    //[MenuItem("PlayerPrefs/Clear")]
    private static void ClearPrefs()
    {
        PlayerPrefs.DeleteAll();
    }

    //On game over
    public void Die()
    {
        endScreenAnimator.SetBool("isDead", true);
        if (highscore < score)
        {
            highscore = score;
            HighScoreText.text = "Highscore: " + highscore;
        }
        PlayerPrefs.SetInt("Highscore", highscore);
        PlayClip(3);
    }

    public void AddScore(int amt)
    {
        score += amt;
        UpdateScore();
    }
    public void UpdateScore()
    {
        ScoreText.text = "Score: " + score;
        EnemyController.shootRate = 10 / Mathf.Max(score / 100, 1) + 0.75f;
    }

    void FixedUpdate()
    {
        if (!isLoading && (TimeToSpawn += Time.deltaTime) > SpawnRate)
        {
            Vector2 Location = Random.insideUnitCircle * spawnRadius + (Vector2)spawnSource.position;
            if (Physics2D.OverlapCircleAll(Location, 0.1f).Length == 0)
            {
                TimeToSpawn = 0;
                GameObject SpawnObject = (Random.Range(-100, score + 1000) > score ? FloaterPrefab : EnemyPrefab);
                GameObject obj = Instantiate(SpawnObject, Location, Quaternion.Euler(new Vector3(0f, 0f, Random.Range(0f, 360f))), Object_Holder.transform);
                if (obj.CompareTag("Triangle_Floater")) obj.GetComponent<Rigidbody2D>().angularVelocity = (Random.value - 0.5f) * 100f;
            }
        }
        if (isLoading)
        {
            loadingScreenAnimator.transform.GetChild(0).GetChild(1).GetComponent<RectTransform>().localScale = new Vector3(1f-(float)loadAmt/chunkSize, 1f, 1f);
            Hub.isDead = true;
        }

        loadingScreenAnimator.SetBool("isLoading",isLoading);
    }

    public void PlayClip(int clipID)
    {
        if(SFX) audioSources[clipID].Play();
    }

    public bool GetNoiseAt(int x, int y, float scale, float threshold, float seed)
    {
        return Mathf.PerlinNoise((float)(x) / scale + 4096f*seed, (float)(y) / scale + 4096f*seed) > threshold;
    }

    public IEnumerator RenderMap(float threshold, int size, float seed)
    {
        loadAmt = 0; 
        loadingScreenAnimator.transform.GetChild(0).GetChild(1).GetComponent<RectTransform>().localScale = new Vector3(1f, 1f, 1f);
        isLoading = true;
        inputManager.isEnabled = false;
        bool[,] coldata = new bool[3, size + 2];

        for(int i = 0; i < 3; i++)
        {
            for(int j = 0; j < size+2; j++)
            {
                coldata[i, j] = GetNoiseAt(i, j, scale, threshold, seed);
            }
            yield return null;
        }

        for (int i = 1; i < size + 1; i++)
        {
            for (int j = 1; j < size + 1; j++)
            {

                int c = 0;
                for (int a = 0; a < 9; a++) if (a != 4 && coldata[a % 3, j + a / 3 - 1]) c++;
                if (c != 4) coldata[1, j] = c > 4;
                map.SetTile(new Vector3Int(i - size / 2, j - size / 2, 0), coldata[1, j] ? tile : null);
                coldata[0, j - 1] = coldata[1, j - 1];
                coldata[1, j - 1] = coldata[2, j - 1];
                coldata[2, j - 1] = GetNoiseAt(i + 1, j - 1, scale, threshold, seed);
            }
            if (Time.realtimeSinceStartup - Time.fixedUnscaledTime > 0.1f) yield return null;
            loadAmt = i;
        }
        yield return null;
        int border = 30;
        for (int b = -size/2 - border + 1; b < size/2 + border; b++)
        {
            for(int c = 0; c < border; c++)
            {
                map.SetTile(new Vector3Int(b, c + size / 2, 0), tile);
                map.SetTile(new Vector3Int(b, -c - size / 2, 0), tile);
                map.SetTile(new Vector3Int(c + size / 2, b, 0), tile);
                map.SetTile(new Vector3Int(-c - size / 2, b, 0), tile);
            }
        }
        for(int b = -5; b < 5; b++)
        {
            for(int c = -5; c < 5; c++)
            {
                map.SetTile(new Vector3Int(b, c, 0), null);
            }
        }
        inputManager.isEnabled = true;
        isLoading = false;
        Hub.isDead = false;
        if (!transform.GetComponent<AudioSource>().isPlaying && Music) transform.GetComponent<AudioSource>().Play();
    }

    public void Restart()
    {
        StartCoroutine(RenderMap(cutoff, chunkSize, 2f*Random.value - 1f));
        StartCoroutine(beginRestartSequence());
    }
    private IEnumerator beginRestartSequence()
    {
        score = 0;
        UpdateScore();
        foreach (Transform x in Object_Holder.transform)
        {
            GameObject.Destroy(x.gameObject);
        }
        foreach (Transform x in Hub.transform.GetChild(0))
        {
            GameObject.Destroy(x.gameObject);
        }
        foreach (Transform x in Bullet_Holder.transform)
        {
            GameObject.Destroy(x.gameObject);
        }
        yield return null;
        GameObject.FindGameObjectWithTag("Camera").transform.position = new Vector3(0, 0, 0);
        Hub.transform.position = new Vector3(0f, 0f, 0f);
        Hub.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        Hub.GenerateTriangles();
        endScreenAnimator.SetBool("isDead", false);
    }

    public void LoadMenu()
    {
        SceneManager.LoadScene(0);
    }
    public void CreateParticles(Vector2 pos)
    {
        Instantiate(ParticleSystemPrefab, new Vector3(pos[0], pos[1], 0), Quaternion.identity, Bullet_Holder.transform);
    }
}
