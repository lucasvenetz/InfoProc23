using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public GameObject tilePrefab;
    public int seed = 12345;
    public int mapLength = 1000;
    public int minTileLength = 50;
    public int maxTileLength = 100;
    public int minGapLength = 10;
    public int maxGapLength = 20;
    public int maxHeightDifference = 10;

    private int lengthCache = 0;

    private int currentX = 0; 
    private int currentY = -50;

    public int[,] Record = new int[100,3];
    public int count;

    private int gapLength1;

    void Start()
    {
        Random.InitState(seed);
        GenerateMap();
    }

    void GenerateMap()
    {
        count = 0;
        while (currentX < mapLength)
        {
            int tileLength = Random.Range(minTileLength, maxTileLength);
            GenerateTile(tileLength);

            int gapLength = Random.Range(minGapLength, maxGapLength);
            currentX += gapLength;
            gapLength1 = gapLength;

            Record[count, 2] = currentY;

            int heightDifference = Random.Range(-maxHeightDifference, maxHeightDifference);
            currentY += heightDifference;
            count += 1;
        }

        currentX = 0;
        currentY = -50; 
        lengthCache = 0;

        while (currentX > -mapLength)
        {
            int tileLength = Random.Range(minTileLength, maxTileLength);
            GenerateTile(-tileLength);

            int gapLength = Random.Range(minGapLength, maxGapLength);
            currentX -= gapLength;
            gapLength1 = gapLength;

            int heightDifference = Random.Range(-maxHeightDifference, maxHeightDifference);
            currentY += heightDifference;
            Record[count, 2] = currentY;
            count += 1;
        }
    }

    void GenerateTile(int length)
    {
        currentX += length/2;

        if (length > 0){
            Record[count, 1] = currentX;
            Record[count, 0] = currentX-gapLength1;
        }
        else{
            Record[count, 1] = currentX+gapLength1;
            Record[count, 0] = currentX;
        }
        
        currentX += lengthCache/2;
        GameObject tile = Instantiate(tilePrefab, new Vector3(currentX, currentY, 0), Quaternion.identity);
        tile.transform.localScale = new Vector3(length, 80, 1); // 假设地块高度为10
        lengthCache = length;
    }
}