using UnityEngine;
using System.Collections.Generic;

public class BattleGridManager : MonoBehaviour
{
    public int gridSize; // Define o tamanho do grid por time (ex: 1vs1 = 1, 3vs3 = 3)
    public GameObject gridPrefab; // Prefab da grid
    public float horizontalSpacing = 2.5f; // Espaçamento horizontal entre grids
    public float verticalSpacing = 2.0f; // Espaçamento vertical entre linhas
    public float teamDistance = 6.0f; // Distância entre os times

    private List<GameObject> battleGrid = new List<GameObject>();

    void Start()
    {
        GenerateGrid(gridSize);
    }

    public void GenerateGrid(int size)
    {
        ClearGrid();

        // Gera grids para o primeiro time (posição Z negativa)
        GenerateTeamFormation(size, -1);
        
        // Gera grids para o segundo time (posição Z positiva)
        GenerateTeamFormation(size, 1);
    }

    private void GenerateTeamFormation(int size, int teamDirection)
    {
        if (size == 1)
        {
            // Caso 1v1, apenas uma grid por time
            Vector3 position = new Vector3(0, 1.1f, teamDirection * teamDistance / 2);
            CreateGridAtPosition(position);
            return;
        }

        // Para formações mais complexas (3v3, 6v6, 10v10)
        float zOffset = teamDirection * teamDistance / 2;
        
        if (size == 3)
        {
            // Formação para 3v3 (formato de árvore)
            // Linha de trás
            CreateGridAtPosition(new Vector3(0, 1.1f, zOffset));
            
            // Linha do meio
            float middleRowZ = zOffset + teamDirection * verticalSpacing;
            CreateGridAtPosition(new Vector3(-horizontalSpacing, 1.1f, middleRowZ));
            CreateGridAtPosition(new Vector3(horizontalSpacing, 1.1f, middleRowZ));
            
            // Linha da frente
            float frontRowZ = zOffset + teamDirection * verticalSpacing * 2;
            CreateGridAtPosition(new Vector3(0, 1.1f, frontRowZ));
        }
        else if (size == 6)
        {
            // Implementar formação para 6v6
            // Linha de trás
            CreateGridAtPosition(new Vector3(-horizontalSpacing, 1.1f, zOffset));
            CreateGridAtPosition(new Vector3(horizontalSpacing, 1.1f, zOffset));
            
            // Linha do meio
            float middleRowZ = zOffset + teamDirection * verticalSpacing;
            CreateGridAtPosition(new Vector3(-horizontalSpacing * 2, 1.1f, middleRowZ));
            CreateGridAtPosition(new Vector3(0, 1.1f, middleRowZ));
            CreateGridAtPosition(new Vector3(horizontalSpacing * 2, 1.1f, middleRowZ));
            
            // Linha da frente
            float frontRowZ = zOffset + teamDirection * verticalSpacing * 2;
            CreateGridAtPosition(new Vector3(-horizontalSpacing, 1.1f, frontRowZ));
            CreateGridAtPosition(new Vector3(horizontalSpacing, 1.1f, frontRowZ));
        }
        else if (size == 10)
        {
            // Implementar formação para 10v10 
            // Primeira linha (4 grids)
            CreateGridAtPosition(new Vector3(-horizontalSpacing * 1.5f, 1.1f, zOffset));
            CreateGridAtPosition(new Vector3(-horizontalSpacing * 0.5f, 1.1f, zOffset));
            CreateGridAtPosition(new Vector3(horizontalSpacing * 0.5f, 1.1f, zOffset));
            CreateGridAtPosition(new Vector3(horizontalSpacing * 1.5f, 1.1f, zOffset));
            
            // Segunda linha (3 grids)
            float secondRowZ = zOffset + teamDirection * verticalSpacing;
            CreateGridAtPosition(new Vector3(-horizontalSpacing, 1.1f, secondRowZ));
            CreateGridAtPosition(new Vector3(0, 1.1f, secondRowZ));
            CreateGridAtPosition(new Vector3(horizontalSpacing, 1.1f, secondRowZ));
            
            // Terceira linha (2 grids)
            float thirdRowZ = zOffset + teamDirection * verticalSpacing * 2;
            CreateGridAtPosition(new Vector3(-horizontalSpacing * 0.5f, 1.1f, thirdRowZ));
            CreateGridAtPosition(new Vector3(horizontalSpacing * 0.5f, 1.1f, thirdRowZ));
            
            // Quarta linha (frente - 1 grid)
            float frontRowZ = zOffset + teamDirection * verticalSpacing * 3;
            CreateGridAtPosition(new Vector3(0, 1.1f, frontRowZ));
        }
        // Você pode adicionar mais padrões para outros tamanhos se necessário
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
