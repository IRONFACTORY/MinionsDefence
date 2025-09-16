using UnityEngine;

public enum BigPanelType { MAIN = 0, FIGHT };

public class UIManager : MonoBehaviour
{
    public static UIManager Inst;

    [SerializeField] RectTransform DisableRect;

    [SerializeField] private MainCostBox[] costBoxes;
    [SerializeField] private RectTransform[] bigPanels;

    [SerializeField] HeroPanel heroPanel;
    [SerializeField] PassivePanel passivePanel;
    [SerializeField] ShopPanel shopPanel;
    [SerializeField] StagePanel stagePanel;
    [SerializeField] TowerPanel towerPanel;

    private int PopUpIndex;
    DownButtonHead downButtonHead;
    public DownButtonHead GetDownButtonHead() => downButtonHead;

    void Awake()
    {
        Inst = this;
        downButtonHead = GetComponent<DownButtonHead>();
    }

    void Start()
    {
        Initialize();
        SetMain();
    }

    void Initialize()
    {
        for (int i = 0; i < costBoxes.Length; i++)
            costBoxes[i].Initialize((CostType)i);
        MainCamera.Inst.Initialize();
        StageManager.Inst.Initialize();
        FightPlayerController.Inst.Initialize();

        heroPanel.Initialize();
        passivePanel.Initialize();
        shopPanel.Initialize();
        stagePanel.Initialize();
        towerPanel.Initialize();
    }

    public MainPanelType GetPopUpPanelType() => (MainPanelType)PopUpIndex;
    public void SetMain()
    {
        downButtonHead.Initialize();
        SetBigPanel(BigPanelType.MAIN);
        SetDownToggle(DownButtonType.STAGE);
        MainCamera.Inst.SetMain();
    }

    public void SetFight()
    {
        SetBigPanel(BigPanelType.FIGHT);
        MainCamera.Inst.SetFight();
    }

    public void SetDownToggle(DownButtonType targetType) => downButtonHead.SetToggle(targetType);

    public void SetBigPanel(BigPanelType bigPanelType)
    {
        for (int i = 0; i < bigPanels.Length; i++)
        {
            bool thisIs = (int)bigPanelType == i;
            bigPanels[i].gameObject.SetActive(thisIs);
        }
    }

    public HeroPanel GetHeroPanel() => heroPanel;
    public PassivePanel GetPassivePanel() => passivePanel;
    public ShopPanel GetShopPanel() => shopPanel;
    public StagePanel GetStagePanel() => stagePanel;
    public TowerPanel GetTowerPanel() => towerPanel;

}