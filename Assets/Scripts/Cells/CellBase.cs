using System.Collections.Generic;
using UnityEngine;

public class CellBase : MonoBehaviour
{
    [Header("元胞管理器")]
    public CellController BelongsCellController;
    [Header("相邻元胞的检索范围")]
    public float SearchRadius;
    public bool[] ConnectedEdges = new bool[6];
    [Header("相邻元胞链表")]
    public List<CellBase> l_neighborCells = new List<CellBase>();
    [Header("初始化")]
    public bool InitiateComplete = false;

    [Header("势能")]
    public float TempPotential;
    public float Potential;
    public int U;               // 是否被分配了广义势能
    [Header("流量")]
    public float Q;
    [Header("最大容纳人数")]
    public float N;
    [Header("实时人数")]
    public float RealtimeN;
    [Header("元胞人群传输量")]
    public Dictionary<CellBase, float> dic_w = new Dictionary<CellBase, float>();
    public Dictionary<CellBase, float> dic_y = new Dictionary<CellBase, float>();
    public Dictionary<CellBase, GameObject> dic_flowArrow = new Dictionary<CellBase, GameObject>();
    public GameObject FlowArrorPrefab;
    public float Sum_yinput;
    public float Sum_youtput;

    #region 组件
    private SpriteRenderer sr;
    #endregion

    #region 字段
    public int initiateTimes = 0;
    #endregion

    private void Start()
    {
        
        // Debug.Log("strat");
        // Debug.Log(transform.position);
        // Debug.Log("strat end");
        // Debug.LogError("Pause");
        sr = gameObject.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        #region 初始化元胞，将相邻元胞加入 l_neighborCells
        if (!InitiateComplete)
        {
            InitiateComplete = SearchNeighboringCells(ref initiateTimes);
        }
        #endregion
        if (!InitiateComplete)
            return;

        // 改变颜色
        MakeCrowdVisiable();
        // 显示人流
        ShowFlowArrow();
    }


    private void OnDrawGizmosSelected()
    {
        // 检索相邻元胞
        Gizmos.color = new Color(1, 0, 0);
        Gizmos.DrawWireSphere(transform.position, SearchRadius);
    }

    private void MakeCrowdVisiable()
    {
        if (CTMController.Instance.PotentialVisible)
        {
            float maxLayer = CTMController.Instance.MaxPotentialLayer;
            sr.color = new Color(1 - Potential / maxLayer, 1 - Potential / maxLayer, 1 - Potential / maxLayer);
        }
        else
            sr.color = new Color(1, 1 - RealtimeN / N, 1 - RealtimeN / N);
    }

    private void ShowFlowArrow()
    {
        foreach(KeyValuePair<CellBase, GameObject> flowArrowPair in dic_flowArrow)
        {
            SpriteRenderer arrow_sr = flowArrowPair.Value.GetComponent<SpriteRenderer>();
            if (Sum_yinput == 0)
            {
                arrow_sr.color = new Color(1, 1, 1, 0);
            }
            else
            {
                arrow_sr.color = new Color(1, 1, 1, dic_y[flowArrowPair.Key] / Q);
            }
        }
    }


    /// <summary>
    /// 检索相邻元胞，并将结果存入 l_neighborCells
    /// </summary>
    private bool SearchNeighboringCells(ref int times)
    {
        Collider2D[] cols = Physics2D.OverlapCircleAll(transform.position, SearchRadius, 1 << LayerMask.NameToLayer("NormalCell") | 1 << LayerMask.NameToLayer("ExitCell"));

        if (times == 5 || cols.Length == 0 || cols.Length > 7)
        {
            if (times == 5)
            {
                Debug.LogWarning(gameObject.GetInstanceID() + ": Initiate failed.");
            }
            times++;
            return false;
        }

        foreach (Collider2D col in cols)
        {
            CellBase neighbourCell = col.GetComponent<CellBase>();
            if (neighbourCell != null && neighbourCell != this)
            {
                // 判断两元胞是否连通
                Vector2 dir = col.gameObject.transform.position - transform.position;
                // 0
                if (dir.x < 0 && dir.y > 0)
                {
                    if (!ConnectedEdges[0] || !neighbourCell.ConnectedEdges[5])
                        continue;
                }
                // 2
                else if (dir.x < 0 && dir.y == 0)
                {
                    if (!ConnectedEdges[2] || !neighbourCell.ConnectedEdges[3])
                        continue;
                }
                // 4
                else if (dir.x < 0 && dir.y < 0)
                {
                    if (!ConnectedEdges[4] || !neighbourCell.ConnectedEdges[1])
                        continue;
                }
                // 1
                else if (dir.x > 0 && dir.y > 0)
                {
                    if (!ConnectedEdges[1] || !neighbourCell.ConnectedEdges[4])
                        continue;
                }
                // 3
                else if (dir.x > 0 && dir.y == 0)
                {
                    if (!ConnectedEdges[3] || !neighbourCell.ConnectedEdges[2])
                        continue;
                }
                // 5
                else if (dir.x > 0 && dir.y < 0)
                {
                    if (!ConnectedEdges[5] || !neighbourCell.ConnectedEdges[0])
                        continue;
                }

                l_neighborCells.Add(neighbourCell);
            }
        }
        
        return true;
    }

    /// <summary>
    /// 计算供给数量
    /// </summary>
    public virtual void CalculateOutputNumber()
    {
        // 出口元胞、普通元胞分别实现
    }

    /// <summary>
    /// 计算需求数量
    /// </summary>
    public virtual void CalculateInputNumber()
    {
        // 出口元胞、普通元胞分别实现
    }

    /// <summary>
    /// 计算元胞的人员传输数量
    /// </summary>
    public virtual void CalculateNewCrowdNumber()
    {
        float sum_yUpstream = 0;
        float sum_yDownstream = 0;
        foreach (CellBase cell in l_neighborCells)
        {
            if (cell.Potential > Potential)
            {
                if (dic_y.ContainsKey(cell))
                {
                    sum_yUpstream += dic_y[cell];
                }
            }
        }
        foreach (CellBase cell in l_neighborCells)
        {
            if (cell.Potential < Potential)
            {
                if (cell.dic_y.ContainsKey(this))
                {
                    sum_yDownstream += cell.dic_y[this];
                }
            }
        }
        // 处理出口元胞
        if (gameObject.GetComponent<ExitCell>() != null)
        {
            sum_yDownstream = Q;
        }

        RealtimeN = RealtimeN + sum_yUpstream - sum_yDownstream;
        if (RealtimeN < 0)
            RealtimeN = 0;
        if (RealtimeN > N)
            RealtimeN = N;

        Sum_yinput = sum_yUpstream;
        Sum_youtput = sum_yDownstream;
    }
}
