using UnityEngine;
using System.Collections;
using DG.Tweening; // Importe o namespace do DOTween

public class CreatureMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float hoverHeight = 0.3f; // Altura que a carta sobe ao ser selecionada
    public float moveDuration = 0.5f; // Duração das animações em segundos
    public float selectDuration = 0.2f; // Duração da animação de seleção
    public int teamId = 0; // 0 = Player 1, 1 = Player 2, etc.
    
    [Header("Creature Stats")]
    [Tooltip("Número de casas que a criatura pode se mover por turno")]
    [Range(1, 5)]
    public int moveRange = 2;
    [Tooltip("Se a criatura já se moveu neste turno")]
    public bool hasMoved = false;
    
    [Header("Animation Settings")]
    public Ease hoverEase = Ease.OutBack; // Tipo de efeito para hover
    public Ease moveEase = Ease.InOutQuad; // Tipo de efeito para movimento
    public float rotationAmount = 5f; // Quantidade de rotação ao ser selecionado
    
    // Estado e referências
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool isSelected = false;
    private GridStatus currentGrid;
    
    // Cache de componentes
    private Collider creatureCollider;
    
    // Para visualização das grids válidas
    private Color validMoveHighlightColor = new Color(0, 1, 0, 0.5f);
    private Color invalidMoveHighlightColor = new Color(1, 0, 0, 0.5f);
    private GridStatus[] allGrids;
    private GridStatus[] validMoveGrids;
    
    void Start()
    {
        // Inicializa a posição original
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        
        // Obtém o collider da criatura
        creatureCollider = GetComponent<Collider>();
        
        // Certifica-se que a tag está correta
        if (tag != "Creature")
        {
            Debug.LogWarning($"Criatura {gameObject.name} não possui a tag 'Creature'. Movimentação pode não funcionar corretamente.");
            tag = "Creature";
        }
        
        // Encontra a grid inicial
        FindCurrentGrid();
        
        // Aplica efeito de entrada
        PlaySpawnAnimation();
        
        // Encontra todas as grids no cenário para uso posterior
        allGrids = FindObjectsOfType<GridStatus>();
    }
    
    private void PlaySpawnAnimation()
    {
        // Começa a carta fora de visão (acima) e a deixa cair suavemente
        Vector3 startPos = transform.position + Vector3.up * 3f;
        transform.position = startPos;
        
        // Animação de queda com ressalto
        transform.DOMove(originalPosition, 0.8f)
            .SetEase(Ease.OutBounce)
            .SetDelay(Random.Range(0.1f, 0.3f)); // Delay aleatório para não ser tudo junto
        
        // Adiciona rotação inicial e retorna ao normal
        transform.rotation = originalRotation * Quaternion.Euler(Random.Range(-30f, 30f), Random.Range(-30f, 30f), Random.Range(-30f, 30f));
        transform.DORotateQuaternion(originalRotation, 0.8f)
            .SetEase(Ease.OutElastic);
    }
    
    void Update()
    {
        // Trata clique do mouse para seleção da criatura
        if (Input.GetMouseButtonDown(0))
        {
            HandleSelection();
        }
        
        // Debug: permitir redefinir o movimento com a tecla 'R'
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetMovement();
        }
        
        // Debug: aumentar moveRange com '+' 
        if (Input.GetKeyDown(KeyCode.Plus) || Input.GetKeyDown(KeyCode.KeypadPlus))
        {
            moveRange = Mathf.Min(moveRange + 1, 5);
            Debug.Log($"Alcance de movimento aumentado para: {moveRange}");
            
            // Atualiza highlight se selecionado
            if (isSelected)
            {
                HighlightValidMoveGrids(true);
            }
        }
        
        // Debug: diminuir moveRange com '-'
        if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus))
        {
            moveRange = Mathf.Max(moveRange - 1, 1);
            Debug.Log($"Alcance de movimento reduzido para: {moveRange}");
            
            // Atualiza highlight se selecionado
            if (isSelected)
            {
                HighlightValidMoveGrids(true);
            }
        }
    }
    
    public void ResetMovement()
    {
        hasMoved = false;
        Debug.Log($"Movimento da criatura {gameObject.name} foi redefinido.");
    }
    
    private void HandleSelection()
    {
        // Cria um raio a partir da posição do mouse
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            // Se clicou nesta criatura
            if (hit.collider.gameObject == gameObject)
            {
                // Só permite seleção se ainda não se moveu neste turno
                if (!hasMoved)
                {
                    ToggleSelection();
                }
                else
                {
                    Debug.Log($"Esta criatura já se moveu neste turno.");
                    
                    // Pequena animação de feedback
                    PlayDeniedAnimation();
                }
            }
            // Se clicou em uma grid e esta criatura está selecionada
            else if (isSelected && hit.collider.CompareTag("Grid"))
            {
                // Tenta mover para a grid clicada
                GridStatus targetGrid = hit.collider.GetComponent<GridStatus>();
                if (targetGrid != null)
                {
                    TryMoveToGrid(targetGrid);
                }
            }
            // Se clicou em outra coisa enquanto esta criatura estava selecionada
            else if (isSelected)
            {
                // Cancela a seleção
                SetSelected(false);
            }
        }
    }
    
    private void ToggleSelection()
    {
        SetSelected(!isSelected);
    }
    
    private void SetSelected(bool selected)
    {
        // Se o estado não mudou, não faz nada
        if (isSelected == selected) return;
        
        isSelected = selected;
        
        // Cancela animações anteriores
        DOTween.Kill(transform);
        
        if (isSelected)
        {
            // Guarda a posição original
            originalPosition = transform.position;
            originalRotation = transform.rotation;
            
            // Animação de levantamento com efeito de ressalto
            transform.DOMove(new Vector3(
                transform.position.x,
                transform.position.y + hoverHeight,
                transform.position.z
            ), selectDuration).SetEase(hoverEase);
            
            // Adiciona uma pequena rotação para dar mais vida
            transform.DORotate(new Vector3(rotationAmount, 0, 0), selectDuration)
                .SetEase(Ease.OutBack);
            
            // Opcional: efeito de escala
            transform.DOScale(transform.localScale * 1.05f, selectDuration)
                .SetEase(Ease.OutBack);
            
            // Destaca as grids para onde a criatura pode se mover
            HighlightValidMoveGrids(true);
        }
        else
        {
            // Animação de retorno à posição original
            transform.DOMove(originalPosition, selectDuration)
                .SetEase(Ease.OutQuad);
            
            // Retorna à rotação original
            transform.DORotateQuaternion(originalRotation, selectDuration)
                .SetEase(Ease.OutQuad);
            
            // Retorna à escala original
            transform.DOScale(Vector3.one, selectDuration)
                .SetEase(Ease.OutQuad);
            
            // Remove o destaque das grids
            HighlightValidMoveGrids(false);
        }
    }
    
    private void HighlightValidMoveGrids(bool highlight)
    {
        // Se não temos a grid atual, não podemos calcular o alcance
        if (currentGrid == null) return;
        
        // Inicializa array para armazenar as grids válidas
        validMoveGrids = new GridStatus[0];
        
        // Para cada grid no cenário
        foreach (GridStatus grid in allGrids)
        {
            // Calcula a distância em grids (usando distância de Manhattan para movimento em grid)
            int distance = CalculateGridDistance(currentGrid, grid);
            
            // Se está dentro do alcance e não é a grid atual
            if (distance > 0 && distance <= moveRange && grid != currentGrid)
            {
                // Opcional: verificar se o caminho está livre (linha de visão)
                
                if (highlight)
                {
                    // Se a grid não está ocupada, destaca em verde
                    if (!grid.IsOccupied())
                    {
                        // Armazena esta grid como uma grid válida para movimento
                        System.Array.Resize(ref validMoveGrids, validMoveGrids.Length + 1);
                        validMoveGrids[validMoveGrids.Length - 1] = grid;
                        
                        // Destaca a grid
                        HighlightGrid(grid, validMoveHighlightColor);
                    }
                    else
                    {
                        // Se está ocupada, verifica se é inimigo (para possível ataque)
                        bool isEnemy = IsEnemyOnGrid(grid);
                        if (isEnemy)
                        {
                            // Destaca em vermelho para ataques
                            HighlightGrid(grid, invalidMoveHighlightColor);
                        }
                    }
                }
                else
                {
                    // Remove o destaque
                    RemoveGridHighlight(grid);
                }
            }
        }
    }
    
    private bool IsEnemyOnGrid(GridStatus grid)
    {
        // Encontra qualquer criatura na grid
        Collider[] colliders = Physics.OverlapBox(
            grid.transform.position + new Vector3(0, 0.5f, 0),
            new Vector3(0.4f, 0.5f, 0.4f)
        );
        
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Creature") && col.gameObject != gameObject)
            {
                CreatureMovement otherCreature = col.GetComponent<CreatureMovement>();
                if (otherCreature != null && otherCreature.teamId != this.teamId)
                {
                    return true; // É um inimigo
                }
            }
        }
        
        return false; // Não encontrou inimigo
    }
    
    private void HighlightGrid(GridStatus grid, Color highlightColor)
    {
        Renderer renderer = grid.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            // Salva a cor original (opcional)
            // originalColors[grid] = renderer.material.color;
            
            // Aplica a cor de destaque
            renderer.material.color = highlightColor;
        }
    }
    
    private void RemoveGridHighlight(GridStatus grid)
    {
        // Restaura a cor com base no estado de ocupação
        if (grid != null)
        {
            Renderer renderer = grid.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                // Usa o método do próprio GridStatus para atualizar a cor
                grid.ForceCheck(); // Isso vai restaurar a cor com base na ocupação
            }
        }
    }
    
    private int CalculateGridDistance(GridStatus from, GridStatus to)
    {
        // Calcula a distância em unidades de grid (não em unidades do mundo)
        // Usa a distância de Manhattan que é mais apropriada para movimento em grid
        
        // Primeiro, obtém as posições no mundo
        Vector3 fromPos = from.transform.position;
        Vector3 toPos = to.transform.position;
        
        // Converte para coordenadas de grid aproximadas
        // Isso assume que suas grids estão em um layout regular
        float gridSizeX = 1.0f; // Ajuste com base no seu espaçamento de grid real
        float gridSizeZ = 1.0f;
        
        // Tenta obter o espaçamento real, se disponível
        BattleGridManager gridManager = FindObjectOfType<BattleGridManager>();
        if (gridManager != null)
        {
            gridSizeX = gridManager.horizontalSpacing;
            gridSizeZ = gridManager.verticalSpacing;
        }
        
        // Calcula diferença em grids
        int xDiff = Mathf.RoundToInt(Mathf.Abs(fromPos.x - toPos.x) / gridSizeX);
        int zDiff = Mathf.RoundToInt(Mathf.Abs(fromPos.z - toPos.z) / gridSizeZ);
        
        // Distância de Manhattan = soma das diferenças absolutas
        return xDiff + zDiff;
    }
    
    private void TryMoveToGrid(GridStatus targetGrid)
    {
        // Verifica se a grid está no alcance de movimento
        int distance = CalculateGridDistance(currentGrid, targetGrid);
        
        if (distance <= 0 || distance > moveRange)
        {
            Debug.Log($"Grid fora do alcance de movimento ({distance} casas, máximo é {moveRange})");
            PlayDeniedAnimation();
            return;
        }
        
        // Verifica se a grid está ocupada
        if (!targetGrid.IsOccupied())
        {
            // Grid vazia - move a criatura
            MoveToPosition(targetGrid.transform.position + new Vector3(0, 0.1f, 0));
            
            // Marca que a criatura já se moveu neste turno
            hasMoved = true;
        }
        else
        {
            // Grid ocupada - verifica quem está na grid
            CheckGridOccupant(targetGrid);
        }
    }
    
    private void CheckGridOccupant(GridStatus targetGrid)
    {
        // Usa Physics.OverlapBox para encontrar o ocupante da grid
        Collider[] colliders = Physics.OverlapBox(
            targetGrid.transform.position + new Vector3(0, 0.5f, 0),
            new Vector3(0.4f, 0.5f, 0.4f)
        );
        
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Creature") && col.gameObject != gameObject)
            {
                // Encontrou uma criatura
                CreatureMovement otherCreature = col.GetComponent<CreatureMovement>();
                
                if (otherCreature != null)
                {
                    // Verifica se é inimigo ou aliado
                    if (otherCreature.teamId != this.teamId)
                    {
                        // É um inimigo - inicia batalha
                        Debug.Log($"Batalha iniciada entre {gameObject.name} e {otherCreature.gameObject.name}!");
                        
                        // Animação de "ataque"
                        PlayBattleAnimation(otherCreature);
                        
                        // Marca que a criatura já se moveu/agiu neste turno
                        hasMoved = true;
                    }
                    else
                    {
                        // É um aliado - não faz nada
                        Debug.Log($"Não é possível mover para uma grid ocupada por um aliado.");
                        
                        // Pequena animação de "negação"
                        PlayDeniedAnimation();
                    }
                }
                
                // Cancela a seleção, independentemente do resultado
                SetSelected(false);
                return;
            }
        }
    }
    
    private void PlayBattleAnimation(CreatureMovement opponent)
    {
        // Sequência de ataque
        Sequence battleSequence = DOTween.Sequence();
        
        // Movimento rápido em direção ao oponente
        Vector3 opponentPosition = opponent.transform.position;
        Vector3 midPoint = (transform.position + opponentPosition) / 2;
        Vector3 attackPosition = midPoint + (opponentPosition - transform.position).normalized * 0.5f;
        
        battleSequence.Append(transform.DOMove(attackPosition, 0.2f).SetEase(Ease.InQuad));
        battleSequence.Append(transform.DOShakePosition(0.3f, 0.2f, 10, 90, false, true));
        battleSequence.Append(transform.DOMove(originalPosition, 0.3f).SetEase(Ease.OutQuad));
        
        // Reproduz a sequência
        battleSequence.Play();
    }
    
    private void PlayDeniedAnimation()
    {
        // Animação de shake para indicar que o movimento não é permitido
        transform.DOShakeRotation(0.5f, 10, 10, 90, true)
            .OnComplete(() => {
                // Retorna à rotação original
                transform.DORotateQuaternion(originalRotation, 0.3f);
            });
    }
    
    private void MoveToPosition(Vector3 targetPosition)
    {
        // Desativa a seleção
        isSelected = false;
        
        // Remove o destaque das grids
        HighlightValidMoveGrids(false);
        
        // Guarda posição final para referência
        Vector3 finalPosition = new Vector3(
            targetPosition.x,
            targetPosition.y,
            targetPosition.z
        );
        
        // Cancela qualquer animação em andamento
        DOTween.Kill(transform);
        
        // Cria sequência de movimento
        Sequence moveSequence = DOTween.Sequence();
        
        // Primeiro movimento: mantém a altura e vai para a nova posição X,Z
        Vector3 midPosition = new Vector3(
            targetPosition.x, 
            transform.position.y, 
            targetPosition.z
        );
        
        moveSequence.Append(
            transform.DOMove(midPosition, moveDuration * 0.6f)
            .SetEase(moveEase)
        );
        
        // Segundo movimento: desce para a posição final
        moveSequence.Append(
            transform.DOMove(finalPosition, moveDuration * 0.4f)
            .SetEase(Ease.OutBounce)
        );
        
        // Adiciona rotação suave durante o movimento
        transform.DORotate(new Vector3(5, Random.Range(-5f, 5f), Random.Range(-3f, 3f)), moveDuration * 0.6f)
            .SetEase(Ease.InOutSine)
            .OnComplete(() => {
                // Retorna à rotação normal
                transform.DORotateQuaternion(originalRotation, moveDuration * 0.4f);
            });
        
        // Começa a sequência
        moveSequence.Play().OnComplete(() => {
            // Após o movimento, atualiza a posição original
            originalPosition = finalPosition;
            
            // Atualiza a grid atual
            FindCurrentGrid();
        });
    }
    
    private void FindCurrentGrid()
    {
        // Lança um raio para baixo para encontrar a grid sob a criatura
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 2.0f))
        {
            GridStatus grid = hit.collider.GetComponent<GridStatus>();
            if (grid != null)
            {
                currentGrid = grid;
            }
        }
    }
    
    // Método público para verificar se esta criatura é selecionável
    public bool IsSelectable()
    {
        // Verifica se a criatura já se moveu neste turno
        return !hasMoved;
    }
    
    // Método público para batalha
    public void InitiateBattle(CreatureMovement opponent)
    {
        // Implemente a lógica de batalha ou chame seu sistema de batalha
        PlayBattleAnimation(opponent);
    }
    
    // Chamado quando o objeto é destruído ou desabilitado
    private void OnDisable()
    {
        // Mata todas as animações em andamento
        DOTween.Kill(transform);
        
        // Remove destaques das grids
        if (isSelected && allGrids != null)
        {
            foreach (GridStatus grid in allGrids)
            {
                RemoveGridHighlight(grid);
            }
        }
    }
}