using UnityEngine;
using System.Collections.Generic;

public class NormalCell : CellBase
{
    [Header("是否随机人数")]
    public bool IsRandom;
    public float DefaultNumber;

    private void Awake()
    {
        TempPotential = 0;
        if (IsRandom)
            RealtimeN = Random.Range(0, N);
    }

    public override void CalculateOutputNumber()
    {
        base.CalculateOutputNumber();
        float sum_downstream = 0;
        foreach (CellBase cell in l_neighborCells)
        {
            if (cell.Potential < Potential)
            {
                sum_downstream += (Potential - cell.Potential) * (cell.N - cell.RealtimeN);
            }
        }
        // 对其每个下游元胞进行供给
        foreach (CellBase cell in l_neighborCells)
        {
            if (cell.Potential < Potential)
            {
                // 分流比
                float D;
                float w;
                if (sum_downstream != 0)
                {
                    D = (Potential - cell.Potential) * (cell.N - cell.RealtimeN) / sum_downstream;
                    w = Mathf.Min(D * RealtimeN, Q);
                }
                else
                    w = 0;

                if (!dic_w.ContainsKey(cell))
                    dic_w.Add(cell, w);
                else
                    dic_w[cell] = w;
            }
        }
    }
    public override void CalculateInputNumber()
    {
        base.CalculateInputNumber();
        float sum_upstream = 0;
        // 对其每个上游元胞供给求和
        foreach (CellBase cell in l_neighborCells)
        {
            if (cell.Potential > Potential)
            {
                if (cell.dic_w.ContainsKey(this))
                    sum_upstream += cell.dic_w[this];
            }
        }

        // 对其每个上游元胞求需求量y
        foreach (CellBase cell in l_neighborCells)
        {
            if (cell.Potential > Potential)
            {
                if (cell.dic_w.ContainsKey(this))
                {
                    float y;
                    if (sum_upstream != 0)
                        y = Mathf.Min(cell.dic_w[this], (N - RealtimeN) * cell.dic_w[this] / sum_upstream);
                    else
                        y = cell.dic_w[this];

                    if (!dic_y.ContainsKey(cell))
                    {
                        dic_y.Add(cell, y);

                        // 可视化人流
                        if (FlowArrorPrefab != null)
                        {
                            // Debug.Log("there is a flow arrow.");
                            GameObject flowArrow = Instantiate(FlowArrorPrefab, (gameObject.transform.position + cell.gameObject.transform.position) / 2, Quaternion.identity, gameObject.transform);
                            flowArrow.transform.up = (gameObject.transform.position - cell.transform.position).normalized;
                            dic_flowArrow.Add(cell, flowArrow);
                        }
                    }
                    else
                        dic_y[cell] = y;
                }
            }
        }
    }
}
