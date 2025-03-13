using UnityEngine;
using System.Collections.Generic;

public class BattleGridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridSize; // Define o tamanho do grid por time (ex: 1vs1 = 1, 3vs3 = 3)
    public GameObject gridPrefab; // Prefab da grid
    
    [Header("Spacing Settings")]
    public float horizontalSpacing = 2.5f; // Espaçamento horizontal entre grids
    public float verticalSpacing = 2.0f; // Espaçamento vertical entre linhas
    public float teamDistance = 6.0f; // Distância entre os times
    public float gridHeight = 1.01f; // Altura dos grids

    private List<GameObject> battleGrid = new List<GameObject>();
    private Dictionary<int, List<Vector2>> formationLayouts = new Dictionary<int, List<Vector2>>();

    void Awake()
    {
        // Define as formações em coordenadas relativas para cada tamanho
        // Cada Vector2 representa uma posição (x, z) relativa
        
        // Formação 1v1
        formationLayouts[1] = new List<Vector2> { 
            new Vector2(0, 0) 
        };
        
        // Formação 3v3
        formationLayouts[3] = new List<Vector2> { 
            new Vector2(-0.92f, 0), new Vector2(1, 0),  // Linha da frente
            new Vector2(0, 1)                           // Linha da final
        };
        
        // Formação 6v6
        formationLayouts[6] = new List<Vector2> {
            new Vector2(-0.92f, 0), new Vector2(1, 0), new Vector2(2.92f, 0),   // Linha da frente
            new Vector2(0.04f, 1), new Vector2(1.97f, 1),              // Linha do meio
            new Vector2(1, 2.045f)                                          // Linha da final
        };
        
        // Formação 10v10
        formationLayouts[10] = new List<Vector2> {
            new Vector2(-2.92f, 0), new Vector2(-0.92f, 0), new Vector2(1, 0), new Vector2(2.92f, 0),     // Linha de trás
            new Vector2(-1.97f, 1), new Vector2(0, 1), new Vector2(1.97f, 1),                           // Linha do meio
            new Vector2(-0.97f, 2), new Vector2(0.97f, 2),                                              // Terceira linha
            new Vector2(0, 3.045f)                                                                            // Linha da frente
        };
    }

    void Start()
    {
        GenerateGrid(gridSize);
    }

    public void GenerateGrid(int size)
    {
        ClearGrid();

        // Verifica se temos uma formação definida para esse tamanho
        if (!formationLayouts.ContainsKey(size))
        {
            Debug.LogError($"Formação para tamanho {size} não definida!");
            return;
        }

        // Gera grids para o primeiro time (posição Z negativa)
        GenerateTeamFormation(size, -1);
        
        // Gera grids para o segundo time (posição Z positiva)
        GenerateTeamFormation(size, 1);
    }

    private void GenerateTeamFormation(int size, int teamDirection)
    {
        float zOffset = teamDirection * teamDistance / 2;
        
        // Usa a formação pré-definida para este tamanho
        foreach (Vector2 relativePos in formationLayouts[size])
        {
            // Multiplica pelas configurações de espaçamento atuais
            float xPos = relativePos.x * horizontalSpacing;
            float zPos = zOffset + teamDirection * relativePos.y * verticalSpacing;
            
            // Cria a grid na posição calculada
            CreateGridAtPosition(new Vector3(xPos, gridHeight, zPos));
        }
    }

    private void CreateGridAtPosition(Vector3 position)
    {
        GameObject newGrid = Instantiate(gridPrefab, position, Quaternion.identity);
        newGrid.transform.parent = transform; // Organiza como filhos do BattleGridManager
        battleGrid.Add(newGrid);
    }

    private void ClearGrid()
    {
        foreach (GameObject grid in battleGrid)
        {
            Destroy(grid);
        }
        battleGrid.Clear();
    }
}
