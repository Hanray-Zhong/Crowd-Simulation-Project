using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellController : MonoBehaviour
{
    [Header("管理的元胞链表")]
    public GameObject TargetGrid;
    public List<CellBase> l_cells = new List<CellBase>();
    [Header("初始化")]
    public bool InitiateComplete = false;
    public bool CellInitiateComplete = false;
    [Header("势能增加参数")]
    [Range(0, 1)]
    public float theta;
    [Header("行人数量对势能大小产生的影响")]
    public float tau;

    private void Update()
    {
        #region 初始化元胞控制器
        if (!InitiateComplete)
        {
            Initiate();
            InitiateComplete = true;
        }
        if (!InitiateComplete)
        {
            return;
        }
        #endregion
        #region 检查管理的所有元胞是否初始化完毕
        if (!CellInitiateComplete)
        {
            CellInitiateComplete = CheckCellInitiate();
            return;
        }
        #endregion
    }


    /// <summary>
    /// 初始化元胞管理器
    /// </summary>
    private void Initiate()
    {
        Transform[] targetGridChildren = TargetGrid.GetComponentsInChildren<Transform>();
        if (targetGridChildren != null)
        {
            foreach (Transform child in targetGridChildren)
            {
                CellBase childCell = child.gameObject.GetComponent<CellBase>();
                if (childCell != null)
                {
                    l_cells.Add(childCell);
                    childCell.BelongsCellController = this;
                }
            }
        }
    }

    /// <summary>
    /// 检查管理的元胞是否全部初始化完毕
    /// </summary>
    /// <returns></returns>
    private bool CheckCellInitiate()
    {
        foreach (CellBase cell in l_cells)
        {
            if (cell.InitiateComplete)
                continue;
            else
                return false;
        }
        return true;
    }

    /// <summary>
    /// 由 CTMController 调用，初始化管理的所有 Cell 的势能
    /// </summary>
    public void InitiatePotential()
    {
        int error = 0;

        // step1
        int layer = 1;
        int maxLayer = 1;

        foreach (CellBase cell in l_cells)
        {
            if (cell.gameObject.GetComponent<ExitCell>() != null)
            {
                cell.TempPotential = 1;
            }
            else
            {
                cell.TempPotential = 0;
            }
        }

        // step2 & step3
        while (true)
        {
            error++;
            if (error == 100000)
            {
                Debug.LogError("loop error.");
                return;
            }
            // step2: 
            foreach (CellBase cell in l_cells)
            {
                if (cell.TempPotential == layer)
                {
                    foreach (CellBase neighborCell in cell.l_neighborCells)
                    {
                        if (neighborCell.TempPotential == 0)
                        {
                            neighborCell.TempPotential = layer + 1;
                            if (neighborCell.TempPotential > maxLayer)
                            {
                                maxLayer = (int)neighborCell.TempPotential;
                            }
                        }
                        else
                        {
                            if (neighborCell.TempPotential > layer + 1)
                            {
                                neighborCell.TempPotential = layer + 1;
                            }
                            if (neighborCell.TempPotential > maxLayer)
                            {
                                maxLayer = (int)neighborCell.TempPotential;
                            }
                        }
                    }
                }
                else
                {
                    continue;
                }
            }
            // step3: 
            bool step3complete = true;
            foreach (CellBase cell in l_cells)
            {
                if (cell.TempPotential > 0)
                {
                    continue;
                }
                else
                {
                    step3complete = false;
                    layer++;
                    break;
                }
            }
            if (step3complete)
            {
                // step3完成，进入step4
                break;
            }
            else
            {
                // step3未完成，重复step2
                continue;
            }
        }

        CTMController.Instance.MaxPotentialLayer = maxLayer;

        // step4
        foreach (CellBase cell in l_cells)
        {
            if (cell.TempPotential == 1)
            {
                cell.Potential = 1;
            }
        }
        layer = 2;

        // step5 & step6
        error = 0;
        while (true)
        {
            error++;
            if (error == 10000)
            {
                Debug.LogError("loop error.");
                return;
            }
            // step5
            foreach (CellBase cell in l_cells)
            {
                if (cell.TempPotential == layer)
                {
                    int omega = 0;
                    float sum_l_neighborCellPotential = 0;
                    foreach (CellBase neighborCell in cell.l_neighborCells)
                    {
                        if (neighborCell.TempPotential == layer - 1)
                        {
                            omega++;
                            sum_l_neighborCellPotential += neighborCell.Potential;
                        }
                    }
                    if (omega == 1)
                    {
                        cell.Potential = sum_l_neighborCellPotential / omega + 1;
                    }
                    else
                    {
                        cell.Potential = sum_l_neighborCellPotential / omega + theta;
                    }
                }
            }
            // step6
            if (layer == maxLayer)
            {
                break;
            }
            else
            {
                layer++;
                continue;
            }
        }
    }

    /// <summary>
    /// 由 CTMController 调用，更新该元胞管理器中的元胞的广义势能
    /// </summary>
    public void UpdatePotential()
    {
        int error = 0;

        int m = 0;                                      // 将被分配势能的元胞数量
        int layer = 1;                                  // l
        List<CellBase> l_v = new List<CellBase>();      // V 集合

        // step 1:
        foreach (CellBase cell in l_cells)
        {
            if (cell.gameObject.GetComponent<ExitCell>() != null)
            {
                cell.Potential = 1;
                cell.U = 1;
                m++;
            }
            else
            {
                cell.Potential = 0;
                cell.U = 0;
            }
        }

        while (m != 0)
        {
            #region #invalid loop#
            error++;
            if (error == 10000)
            {
                Debug.LogError("loop error.");
                return;
            }
            #endregion

            // 清空 V 集合
            l_v.Clear();

            // step 3:
            foreach (CellBase cell in l_cells)
            {
                if (cell.U == 1 && cell.Potential <= layer)
                {
                    cell.U = 2;
                    l_v.Add(cell);
                }
            }
            m = m - l_v.Count;

            // step 4:
            foreach (CellBase cell in l_v)
            {
                foreach (CellBase neighborCell in cell.l_neighborCells)
                {
                    if (neighborCell.U == 0)
                    {
                        int count_psiCell = 0;
                        float sum_psiCell_Potential = 0;

                        neighborCell.U = 1;
                        m = m + 1;

                        foreach (CellBase psiCell in neighborCell.l_neighborCells)
                        {
                            if (psiCell.U == 2)
                            {
                                count_psiCell++;
                                sum_psiCell_Potential += psiCell.Potential;
                            }
                        }
                        if (count_psiCell == 1)
                        {
                            neighborCell.Potential = cell.Potential + 1 + tau * neighborCell.RealtimeN;
                        }
                        else if (count_psiCell > 1)
                        {
                            neighborCell.Potential = sum_psiCell_Potential / count_psiCell + theta + tau * neighborCell.RealtimeN;
                        }
                    }
                }
            }

            // step 5:
            layer = layer + 1;
        }

        CTMController.Instance.MaxPotentialLayer = layer;
    }

    /// <summary>
    /// 由 CTMController 调用，更新元胞内的行人
    /// </summary>
    public void UpdateCrowd()
    {
        foreach (CellBase cell in l_cells)
        {
            cell.CalculateOutputNumber();
        }
        foreach (CellBase cell in l_cells)
        {
            cell.CalculateInputNumber();
        }
        foreach (CellBase cell in l_cells)
        {
            cell.CalculateNewCrowdNumber();
        }
    }

    /// <summary>
    /// 由 CTMController 调用，计算管理的元胞的行人总和
    /// </summary>
    public float CaculateCrowdNumber()
    {
        float sum_realtimeN = 0;
        foreach (CellBase cell in l_cells)
        {
            sum_realtimeN += cell.RealtimeN;
        }
        return sum_realtimeN;
    }

    /// <summary>
    /// 重置 Cell
    /// </summary>
    public void ResetCells()
    {
        foreach (CellBase cell in l_cells)
        {
            if (cell.gameObject.GetComponent<ExitCell>() != null)
            {
                cell.RealtimeN = 0;
                cell.Potential = 1;
            }
            else
            {
                NormalCell normalCell = cell.gameObject.GetComponent<NormalCell>();
                if (normalCell.IsRandom)
                    cell.RealtimeN = Random.Range(0, cell.N);
                else
                    cell.RealtimeN = normalCell.DefaultNumber;
                cell.Potential = 0;
            }
            cell.Sum_yinput = 0;
            cell.Sum_youtput = 0;
        }
    }
}
