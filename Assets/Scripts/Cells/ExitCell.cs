using UnityEngine;

public class ExitCell : CellBase
{
    private void Awake()
    {
        TempPotential = 1;
    }

    public override void CalculateOutputNumber()
    {
        base.CalculateOutputNumber();
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
