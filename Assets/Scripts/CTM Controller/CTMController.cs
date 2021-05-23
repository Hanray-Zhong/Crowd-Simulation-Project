using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CTMController : MonoBehaviour
{
    private static CTMController instance;
    public static CTMController Instance
    {
        get
        {
            if (instance == null)
            {
                instance = GameObject.FindGameObjectWithTag("CTM Controller").GetComponent<CTMController>();
            }
            return instance;
        }
    }

    [Header("元胞管理器链表")]
    public List<CellController> l_cellController = new List<CellController>();
    public bool InitiateCellControllerComplete = false;
    [Header("初始化势能")]
    public bool InitiatePotentialComplete = false;
    public float MaxPotentialLayer;
    [Header("人群仿真模拟运行状态")]
    public bool Start;
    public bool PotentialVisible;
    [Header("UI")]
    public Text CrowdNumber;
    public Text ThetaText;
    public Text TauText;
    public Button InitiateButton;
    public Button StartButton;
    public Button PauseButton;
    public Toggle PotentialVisibleToggle;

    private void FixedUpdate()
    {
        if (!InitiateCellControllerComplete || !InitiatePotentialComplete)
        {
            // Debug.Log("Don't Initiate.");
            StartButton.interactable = false;
            PauseButton.interactable = false;
            return;
        }
        else
        {
            StartButton.interactable = true;
            PauseButton.interactable = true;
        }
        #region 更新行人
        if (Start)
        {
            UpdatePotential();
            UpdateCrowd();
        }
        #endregion
    }

    private void Update()
    {
        #region UI
        CaculateCrowdNumber();
        UpdateTheta();
        UpdateTau();
        MakePotentialVisible();
        #endregion

        #region 检查所有元胞管理器是否全部初始化
        if (!InitiateCellControllerComplete)
        {
            InitiateCellControllerComplete = CheckCellControllerInitiate();
        }
        if (!InitiateCellControllerComplete)
            return;
        #endregion

        #region 检测初始化势能
        if (!InitiatePotentialComplete)
            return;
        #endregion
    }


    /// <summary>
    /// 检查所有 CellController 是否初始化完毕
    /// </summary>
    /// <returns></returns>
    private bool CheckCellControllerInitiate()
    {
        foreach (CellController cellController in l_cellController)
        {
            if (cellController.InitiateComplete && cellController.CellInitiateComplete)
                continue;
            else
                return false;
        }
        return true;
    }

    /// <summary>
    /// 初始化所有 Cell 的势能
    /// </summary>
    private bool InitiatePotential()
    {
        foreach (CellController cellController in l_cellController)
        {
            cellController.InitiatePotential();
        }
        return true;
    }

    /// <summary>
    /// 更新元胞广义势能
    /// </summary>
    private void UpdatePotential()
    {
        foreach (CellController cellController in l_cellController)
        {
            cellController.UpdatePotential();
        }
    }

    /// <summary>
    /// 更新元胞内的行人
    /// </summary>
    private void UpdateCrowd()
    {
        // Debug.Log(Time.fixedTime);
        foreach (CellController cellController in l_cellController)
        {
            cellController.UpdateCrowd();
        }
    }

    /// <summary>
    /// 计算所有元胞内的人数总和
    /// </summary>
    private void CaculateCrowdNumber()
    {
        float sum = 0;
        foreach (CellController cellController in l_cellController)
        {
            sum += cellController.CaculateCrowdNumber();
        }
        CrowdNumber.text = "总人数： " + sum + " ";
    }

    /// <summary>
    /// 更新 θ 值
    /// </summary>
    private void UpdateTheta()
    {
        foreach (CellController cellController in l_cellController)
        {
            try
            {
                cellController.theta = Mathf.Clamp(Convert.ToSingle(ThetaText.text), 0, 1);
                ThetaText.text = cellController.theta.ToString();
            }
            catch
            {
                cellController.theta = 0.95f;
            }
        }
    }

    /// <summary>
    /// 更新 τ 值
    /// </summary>
    private void UpdateTau()
    {
        foreach (CellController cellController in l_cellController)
        {
            try
            {
                cellController.tau = Convert.ToSingle(TauText.text);
            }
            catch
            {
                cellController.tau = 0;
            }
        }
    }

    /// <summary>
    /// 是否显示势能图
    /// </summary>
    private void MakePotentialVisible()
    {
        PotentialVisible = PotentialVisibleToggle.isOn;
    }

    /// <summary>
    /// 初始化 CTM 势能
    /// </summary>
    public void InitiateCTMPotential()
    {
        if (InitiateCellControllerComplete)
        {
            InitiatePotentialComplete = InitiatePotential();
        }
        else
        {
            Debug.LogError("Don't Initiate CellController");
        }
    }

    /// <summary>
    /// 开始更新行人人群
    /// </summary>
    public void StartUpdateCrowd()
    {
        InitiateButton.interactable = false;
        Start = true;
    }

    /// <summary>
    /// 停止更新行人人群
    /// </summary>
    public void StopUpdateCrowd()
    {
        Start = false;
    }

    /// <summary>
    /// 重置 CTM 状态
    /// </summary>
    public void ResetCTM()
    {
        InitiatePotentialComplete = false;
        Start = false;
        InitiateButton.interactable = true;
        foreach (CellController cellController in l_cellController)
        {
            cellController.ResetCells();
        }
    }
}
