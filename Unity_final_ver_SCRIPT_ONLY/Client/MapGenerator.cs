using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public GameObject tilePrefab; // 长方体地块的预制体
    public int seed = 12345; // 随机种子
    public bool isStart;
    public int mapLength = 1000; // 地图总长度
    public int minTileLength = 50; // 地块最小长度
    public int maxTileLength = 100; // 地块最大长度
    public int minGapLength = 10; // 空隙最小长度
    public int maxGapLength = 20; // 空隙最大长度
    public int maxHeightDifference = 10; // 最大高度差

    private int lengthCache = 0;

    private int currentX = 0; // 当前生成位置的X坐标
    private int currentY = -50; // 当前生成位置的Y坐标

    void Update(){
        if (isStart){
            Random.InitState(seed); // 初始化随机种子
            GenerateMap();
            isStart = false;
        }
    }

    void GenerateMap()
    {
        while (currentX < mapLength)
        {
            int tileLength = Random.Range(minTileLength, maxTileLength);
            GenerateTile(tileLength);

            int gapLength = Random.Range(minGapLength, maxGapLength);
            currentX += gapLength;

            int heightDifference = Random.Range(-maxHeightDifference, maxHeightDifference);
            currentY += heightDifference;
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

            int heightDifference = Random.Range(-maxHeightDifference, maxHeightDifference);
            currentY += heightDifference;
        }
    }

    void GenerateTile(int length)
    {
        currentX += length/2 + lengthCache/2;
        GameObject tile = Instantiate(tilePrefab, new Vector3(currentX, currentY, 0), Quaternion.identity);
        tile.transform.localScale = new Vector3(length, 80, 1); // 假设地块高度为10
        lengthCache = length;
    }
}