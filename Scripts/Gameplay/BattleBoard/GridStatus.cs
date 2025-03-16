using UnityEngine;

// Este script deve ser adicionado ao prefab da grid
public class GridStatus : MonoBehaviour
{
    // Cores para os diferentes estados da grid
    [Header("Grid Colors")]
    public Color emptyColor = Color.green;
    public Color occupiedColor = Color.red;
    
    [Header("Grid Status")]
    [SerializeField] private bool isOccupied = false;
    [SerializeField] private int creatureCount = 0;
    
    // Referência ao renderer do plano
    private Renderer planeRenderer;
    private Material planeMaterial;

    [Header("Debug Options")]
    public bool showDebugLogs = false; // Desativado por padrão
    public float checkInterval = 1f; // Intervalo em segundos para verificação contínua
    private float nextCheckTime;

    void Awake()
    {
        // Configura o material e o renderer
        SetupRenderer();
        
        // Verifica configuração do collider
        SetupCollider();
    }

    void Start()
    {
        // Verifica inicialmente se já existe alguma criatura sobre a grid
        CheckForInitialCreatures();
        nextCheckTime = Time.time + checkInterval;
    }
    
    void Update()
    {
        // Verificação periódica para garantir que o estado esteja correto
        if (Time.time > nextCheckTime)
        {
            CheckForCreatures();
            nextCheckTime = Time.time + checkInterval;
        }
    }

    private void SetupRenderer()
    {
        // Obtém o renderer do objeto
        planeRenderer = GetComponent<Renderer>();
        
        if (planeRenderer != null)
        {
            // Cria uma instância do material para que possamos modificá-lo
            planeMaterial = new Material(planeRenderer.material);
            planeRenderer.material = planeMaterial;
            
            // Define a cor inicial (provisoriamente verde, será atualizada após a verificação)
            planeMaterial.color = emptyColor;
        }
        else
        {
            Debug.LogError($"Grid {gameObject.name} não possui componente Renderer!");
        }
    }

    private void SetupCollider()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider == null)
        {
            // Adiciona um collider se não existir
            boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.isTrigger = true;
            boxCollider.size = new Vector3(1f, 0.5f, 1f); // Aumentado em Y para melhor detecção
        }
        else if (!boxCollider.isTrigger)
        {
            boxCollider.isTrigger = true;
        }
    }

    private void CheckForInitialCreatures()
    {
        // Primeira verificação usando OverlapBox
        CheckForCreatures();
        
        // Segunda verificação com raycasting (mais confiável para alguns casos)
        RaycastCheck();
    }
    
    private void RaycastCheck()
    {
        // Lança um raio para cima para verificar se há uma criatura acima
        RaycastHit[] hits = Physics.RaycastAll(
            transform.position, 
            Vector3.up, 
            2.0f // Distância do raio
        );
        
        foreach (RaycastHit hit in hits)
        {
            // Se encontrou uma criatura acima da grid
            if (hit.collider.CompareTag("Creature"))
            {
                creatureCount = 1;
                isOccupied = true;
                UpdateGridColor();
                return;
            }
        }
    }

    private void CheckForCreatures()
    {
        // Usa OverlapBox com um box maior para melhor detecção
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        
        if (boxCollider != null)
        {
            // Usa um box maior para melhor detecção
            Vector3 boxCenter = transform.position + new Vector3(0, 0.5f, 0); // Centro acima da grid
            Vector3 boxExtents = new Vector3(0.5f, 1.0f, 0.5f); // Box mais alto para detecção
            
            // Realiza a verificação de sobreposição
            Collider[] overlappingColliders = Physics.OverlapBox(
                boxCenter,
                boxExtents,
                transform.rotation
            );
            
            bool foundCreature = false;
            
            // Verifica cada collider encontrado
            foreach (Collider col in overlappingColliders)
            {
                if (col.gameObject != gameObject && col.CompareTag("Creature"))
                {
                    // Encontrou uma criatura sobre a grid
                    foundCreature = true;
                    break;
                }
            }
            
            // Atualiza o estado com base na verificação
            if (foundCreature != isOccupied)
            {
                isOccupied = foundCreature;
                creatureCount = foundCreature ? 1 : 0;
                UpdateGridColor();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Creature"))
        {
            creatureCount++;
            isOccupied = true;
            UpdateGridColor();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Creature"))
        {
            creatureCount--;
            
            if (creatureCount <= 0)
            {
                creatureCount = 0;
                isOccupied = false;
                UpdateGridColor();
            }
        }
    }
    
    // Atualiza a cor do grid com base no estado atual
    private void UpdateGridColor()
    {
        if (planeMaterial != null)
        {
            Color targetColor = isOccupied ? occupiedColor : emptyColor;
            planeMaterial.color = targetColor;
            
            // Para Shader Graph e URP também tenta estas propriedades:
            planeMaterial.SetColor("_BaseColor", targetColor);
            planeMaterial.SetColor("_Color", targetColor);
        }
    }
    
    // Para teste manual - você pode chamar isso de outro script ou botão
    public void ToggleColor()
    {
        isOccupied = !isOccupied;
        UpdateGridColor();
    }
    
    // Para verificação manual através de outro script
    public void ForceCheck()
    {
        CheckForCreatures();
    }
    
    // Getter público para verificar se a grid está ocupada
    public bool IsOccupied()
    {
        return isOccupied;
    }
    
    // Método para visualização em tempo de edição
    private void OnValidate()
    {
        // Atualiza visualmente no editor quando valores são alterados
        if (planeRenderer != null && planeMaterial != null)
        {
            planeMaterial.color = isOccupied ? occupiedColor : emptyColor;
        }
    }
    
    // Visualização do collider na cena para debug
    private void OnDrawGizmos()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
        {
            Gizmos.color = isOccupied ? Color.red : Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);
            
            // Área de detecção expandida removida
        }
    }
}