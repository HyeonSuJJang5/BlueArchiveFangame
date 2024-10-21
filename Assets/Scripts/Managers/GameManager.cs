using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Linq;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    [SerializeField]
    public GameObject Serving;

    [SerializeField]
    public GameObject Chef;

    [SerializeField]
    ChefManager chefManager; // ChefManager 스크립트 참조

    [SerializeField]
    ChairManager chairManager; // ChairManager 스크립트 참조

    [SerializeField]
    GuestRespawns customerRespawner; // CustomerRespawner 스크립트 참조

    [SerializeField]
    private ChefController chefController;

    [SerializeField]
    private GuestController guestController;


    // 요리 시간 감소 비율을 저장할 변수
    public float cookingTimeReduction = 0f; // 총 감소율 (0% ~ 50%)


    // 음식 가격 증가 비율을 저장할 변수
    private float foodPriceIncreasePercentage = 0f; // 총 증가율 (0% ~ 100%)


    // AudioManager 참조 추가
    [SerializeField]
    AudioManager audioManager;

    [SerializeField]
    private TextMeshProUGUI Text_Gold;
    [SerializeField]
    private long PlayerMoney = 0; // 점수 관리 변수

    [Header("Fried Rice Level System")]
    public int friedRiceLevel = 1;
    public Button friedRiceLevelUpButton;
    public Image friedRiceGaugeBar;
    public List<Image> friedRiceStarImages; // 10, 25, 50 레벨에 해당하는 별 이미지
    public TextMeshProUGUI friedRiceLevelUpButtonText;
    public TextMeshProUGUI friedRicePriceText; // 현재 판매 가격 표시 TextMeshProUGUI

    // 기존 가격 데이터 유지
    [SerializeField]
    private int[] friedRicePrices = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 12, 14, 16, 19, 22, 26, 31, 37, 44, 52, 62, 74, 88, 105, 126, 151, 181, 217, 260, 312, 374, 448, 537, 644, 772, 926, 1111, 1333, 1599, 1918, 2301, 2761, 3313, 3975, 4770, 5724, 6868, 8241, 9889, 11866 };

    // 새롭게 제공된 sale 데이터
    private int[] friedRiceSales = { 3, 4, 5, 6, 7, 8, 9, 10, 12, 25, 27, 29, 31, 33, 35, 37, 39, 42, 45, 48, 51, 55, 59, 63, 136, 146, 157, 169, 182, 196, 211, 227, 245, 264, 285, 307, 331, 357, 385, 415, 448, 483, 521, 562, 606, 654, 706, 762, 822, 1780 };

    [Header("라멘 레벨 시스템")]
    public int ramenLevel = 1;
    public Button ramenLevelUpButton;
    public Image ramenGaugeBar;
    public List<Image> ramenStarImages; // 10, 25, 50 레벨에 해당하는 별 이미지
    public TextMeshProUGUI ramenLevelUpButtonText;
    public TextMeshProUGUI ramenPriceText; // 현재 판매 가격 표시 TextMeshProUGUI

    // 기존 가격 데이터 유지
    [SerializeField]
    private int[] ramenPrices = { 40, 48, 57, 68, 81, 97, 116, 139, 166, 199, 238, 285, 342, 410, 492, 590, 708, 849, 1018, 1221, 1465, 1758, 2109, 2530, 3036, 3643, 4371, 5245, 6294, 7552, 9062, 10874, 13048, 15657, 18788, 22545, 27054, 32464, 38956, 46747, 56096, 67315, 80778, 96933, 116319, 139582, 167498, 200997, 241196, 289435 };

    // 새롭게 제공된 sale 데이터
    private int[] ramenSales = { 120, 129, 139, 150, 162, 174, 187, 201, 217, 468, 505, 545, 588, 635, 685, 739, 798, 861, 929, 1000, 1080, 1170, 1260, 1360, 2940, 3180, 3430, 3700, 4000, 4320, 4670, 5040, 5440, 5880, 6350, 6850, 7400, 7990, 8680, 9320, 10070, 10870, 11740, 12680, 13690, 14790, 15970, 17250, 18630, 40240 };

    [Header("스테이크 레벨 시스템")]
    public int steakLevel = 1;
    public Button steakLevelUpButton;
    public Image steakGaugeBar;
    public List<Image> steakStarImages; // 10, 25, 50 레벨에 해당하는 별 이미지
    public TextMeshProUGUI steakLevelUpButtonText;
    public TextMeshProUGUI steakPriceText; // 현재 판매 가격 표시 TextMeshProUGUI

    // 기존 가격 데이터 유지
    [SerializeField]
    private int[] steakPrices = { 2000, 2400, 2880, 3460, 4150, 4980, 5970, 7170, 8600, 10320, 12380, 14860, 17830, 21390, 25670, 30800, 36960, 44360, 53230, 63870, 76650, 91980, 110370, 132440, 158930, 190720, 228860, 274630, 329560, 395470, 474560, 569480, 683370, 820050, 984050, 1180860, 1417040, 1700440, 2040530, 2448640, 2938360, 3526040, 4231240, 5077490, 6092990, 7311590, 8773900, 10528680, 12634420, 15161300 };

    // 새롭게 제공된 sale 데이터
    private int[] steakSales = { 4800, 5180, 5600, 6050, 6530, 7050, 7610, 8220, 8880, 19180, 20710, 22370, 24160, 26090, 28180, 30430, 32870, 35500, 38340, 41400, 44710, 48290, 52150, 56330, 121660, 131390, 141910, 153260, 165520, 178760, 193060, 280500, 225180, 243200, 262650, 283660, 306360, 330860, 357330, 385920, 416790, 450140, 486150, 525040, 567040, 612400, 661390, 714300, 771450, 1670000 };

    [Header("패널들")]
    public GameObject friedRicePanel;
    public GameObject ramenPanel;
    public GameObject steakPanel;

    [Header("패널을 활성화 시키는 버튼")]
    public Button friedRiceButton;
    public Button ramenButton;
    public Button steakButton;

    [Header("패널을 비활성화 시키는 버튼")]
    public Button friedRiceCloseButton;
    public Button ramenCloseButton;
    public Button steakCloseButton;

    [Header("가게 관리")]
    public GameObject passivePanel; // 패시브 패널
    public Button passiveActiveButton; // 패시브 패널 활성화 버튼
    public Button passiveDeactivationButton; // 패시브 패널 비활성화 버튼
    public Button servingAddButton; // Serving 추가 버튼
    public Button chefAddButton; // Chef 추가 버튼
    public TextMeshProUGUI servingCostText; // Serving 추가에 필요한 비용 표시
    public TextMeshProUGUI chefCostText; // Chef 추가에 필요한 비용 표시
    private int servingCost = 200; // Serving 추가에 필요한 금액
    private int chefCost = 400; // Chef 추가에 필요한 금액

    private int steakUnloookCost = 1000; // 스테이크 해금에 필요한 금액 
    private int RamenUnlookCost = 150; // 라면 해금에 필요한 금액

    // 새로운 선택 버튼들
    public Button friedRiceSelectionButton;
    public Button ramenSelectionButton;
    public Button steakSelectionButton;

    // 각 요리의 UI 패널
    public GameObject friedRiceUI;
    public GameObject ramenUI;
    public GameObject steakUI;

    public GameObject backgroundPanel; // 전체 화면을 덮는 Panel

    // 가격 버튼 (UI에서의 구매 버튼)
    public Button friedRicePurchaseButton;
    public Button ramenPurchaseButton;
    public Button steakPurchaseButton;

    public bool steakOrderUnlook = false;
    public bool ramenOrderUnlook = false;

    [Header("스킬 업그레이드 UI")]
    [SerializeField]
    private GameObject Serving_UIListltem;
    [SerializeField]
    private GameObject Chef_UIListltem;
    [SerializeField]
    private GameObject firstCookingTime_UIListltem;
    [SerializeField]
    private GameObject secoundCookingTime_UIListltem;
    [SerializeField]
    private GameObject first_IncreaseFoodPrice_UIListltem;
    [SerializeField]
    private GameObject secound_IncreaseFoodPrice_UIListltem;
    [SerializeField]
    private GameObject third_IncreaseFoodPrice_UIListltem;
    [SerializeField]
    private GameObject fourth_IncreaseFoodPrice_UIListltem;
    [SerializeField]
    private GameObject firstSpeedPostion_UIListltem;
    [SerializeField]
    private GameObject secoundSpeedPostion_UIListltem;
    [SerializeField]
    private GameObject thirdSpeedPostion_UIListltem;

    [Header("스킬 업그레이드 버튼")]
    [SerializeField]
    private Button firstCookingTime_Button;
    [SerializeField]
    private Button secoundCookingTime_Button;
    [SerializeField]
    private Button first_IncreaseFoodPrice_Button;
    [SerializeField]
    private Button secound_IncreaseFoodPrice_Button;
    [SerializeField]
    private Button third_IncreaseFoodPrice_Button;
    [SerializeField]
    private Button fourth_IncreaseFoodPrice_Button;
    [SerializeField]
    private Button firstSpeedPostion_Button;
    [SerializeField]
    private Button secoundSpeedPostion_Button;
    [SerializeField]
    private Button thirdSpeedPostion_Button;

    [SerializeField]
    private GameObject NextStageButton;


    [SerializeField]
    private Image levelProgressImage; // Fill Image를 참조

    private int eventCount = 0; // 이벤트 카운터
    private const int maxEvents = 53; // 최대 이벤트 수

    public TextMeshProUGUI steakLevelText; // 스테이크 레벨 UI 요소
    public TextMeshProUGUI friedRiceLevelText; // 볶음밥 레벨 UI 요소
    public TextMeshProUGUI ramenLevelText; // 라면 레벨 UI 요소





    // Singleton 패턴을 위한 Instance 프로퍼티
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                // 씬에서 GameManager를 찾아 instance에 할당
                instance = FindObjectOfType<GameManager>();

                // 씬에 GameManager가 없을 경우 새로 생성
                if (instance == null)
                {
                    GameObject managerObject = new GameObject("GameManager");
                    instance = managerObject.AddComponent<GameManager>();
                }
            } 
            return instance;
        }
    }

    // 게임 초기화 로직 등을 추가할 수 있습니다.
    private void Awake()
    {
        Application.targetFrameRate = 60;


        return;
        // GameManager가 중복으로 생성되지 않도록 처리
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {

        // 씬 이름 확인
        string sceneName = SceneManager.GetActiveScene().name;

        // 씬 이름이 "First_GameScene"인 경우 MAX 레벨을 10으로 설정
        if (sceneName == "First_GameScene")
        {
            // 스테이크, 라면, 볶음밥 최대 레벨을 10으로 제한
            friedRicePrices = LimitArrayToMaxLevel(friedRicePrices, 10);
            friedRiceSales = LimitArrayToMaxLevel(friedRiceSales, 10);

            ramenPrices = LimitArrayToMaxLevel(ramenPrices, 10);
            ramenSales = LimitArrayToMaxLevel(ramenSales, 10);

            steakPrices = LimitArrayToMaxLevel(steakPrices, 10);
            steakSales = LimitArrayToMaxLevel(steakSales, 10);
        }

        chefController = FindObjectOfType<ChefController>();
        guestController = GetComponent<GuestController>();

        // 기존 요리 버튼 비활성화
        friedRiceButton.gameObject.SetActive(false);
        ramenButton.gameObject.SetActive(false);
        steakButton.gameObject.SetActive(false);

        // UI 비활성화 초기화
        friedRiceUI.SetActive(false);
        ramenUI.SetActive(false);
        steakUI.SetActive(false);

        // Panel에 EventTrigger 추가
        AddEventTrigger(backgroundPanel, EventTriggerType.PointerClick, OnBackgroundClick);

        // 버튼 클릭 이벤트 등록
        friedRiceSelectionButton.onClick.AddListener(ShowFriedRiceUI);
        ramenSelectionButton.onClick.AddListener(ShowRamenUI);
        steakSelectionButton.onClick.AddListener(ShowSteakUI);

        // 구매 버튼 클릭 이벤트 등록
        friedRicePurchaseButton.onClick.AddListener(() => CompletePurchase(friedRiceUI, friedRiceButton));
        ramenPurchaseButton.onClick.AddListener(() => CompletePurchase(ramenUI, ramenButton));
        steakPurchaseButton.onClick.AddListener(() => CompletePurchase(steakUI, steakButton));

        // 레벨 UI 업데이트
        UpdateSteakLevelUI();
        UpdateFriedRiceLevelUI();
        UpdateRamenLevelUI();

        // 레벨업 버튼 클릭 이벤트 연결
        steakLevelUpButton.onClick.AddListener(LevelUpSteak);
        friedRiceLevelUpButton.onClick.AddListener(LevelUpFriedRice);
        ramenLevelUpButton.onClick.AddListener(LevelUpRamen);

        // 새로운 버튼들에 패널 활성화 이벤트 연결
        friedRiceButton.onClick.AddListener(() => ActivatePanel(friedRicePanel));
        ramenButton.onClick.AddListener(() => ActivatePanel(ramenPanel));
        steakButton.onClick.AddListener(() => ActivatePanel(steakPanel));

        // 패널 비활성화 버튼 연결
        friedRiceCloseButton.onClick.AddListener(() => DeactivatePanel(friedRicePanel));
        ramenCloseButton.onClick.AddListener(() => DeactivatePanel(ramenPanel));
        steakCloseButton.onClick.AddListener(() => DeactivatePanel(steakPanel));

        // Passive 버튼 및 패널 활성화/비활성화 초기화
        passiveActiveButton.onClick.AddListener(ActivatePassivePanel);
        passiveDeactivationButton.onClick.AddListener(DeactivatePassivePanel);

        // Serving 및 Chef 추가 버튼 초기화
        servingAddButton.onClick.AddListener(AddServing);
        
        chefAddButton.onClick.AddListener(AddChef);

        // 비용 텍스트 초기화
        servingCostText.text = $"{servingCost}";
        chefCostText.text = $" {chefCost}";

        // Purchase 버튼 클릭 이벤트 등록
        friedRicePurchaseButton.onClick.AddListener(DeleteFriedRiceUI);
        ramenPurchaseButton.onClick.AddListener(DeleteRamenUI);
        steakPurchaseButton.onClick.AddListener(DeleteSteakUI);

        // 기존 버튼 이벤트 등록
        firstCookingTime_Button.onClick.AddListener(() => ReduceCookingTime(0.2f)); // 20% 감소
        secoundCookingTime_Button.onClick.AddListener(() => ReduceCookingTime(0.3f)); // 30% 감소

        first_IncreaseFoodPrice_Button.onClick.AddListener(() => IncreaseFoodPrices(0.1f));
        secound_IncreaseFoodPrice_Button.onClick.AddListener(() => IncreaseFoodPrices(0.2f));
        third_IncreaseFoodPrice_Button.onClick.AddListener(() => IncreaseFoodPrices(0.3f));
        fourth_IncreaseFoodPrice_Button.onClick.AddListener(() => IncreaseFoodPrices(0.4f));

        firstSpeedPostion_Button.onClick.AddListener(() => IncreaseMoveSpeed(0.1f));
        secoundSpeedPostion_Button.onClick.AddListener(() => IncreaseMoveSpeed(0.2f));
        thirdSpeedPostion_Button.onClick.AddListener(() => IncreaseMoveSpeed(0.2f));

        // 스킬 업그레이드 버튼에 클릭 이벤트 등록
        servingAddButton.onClick.AddListener(() => HandleSkillUpgrade(servingAddButton, Serving_UIListltem, 200));
        chefAddButton.onClick.AddListener(() => HandleSkillUpgrade(chefAddButton, Chef_UIListltem, 400));

        firstCookingTime_Button.onClick.AddListener(() => HandleSkillUpgrade(firstCookingTime_Button, firstCookingTime_UIListltem, 1000));
        secoundCookingTime_Button.onClick.AddListener(() => HandleSkillUpgrade(secoundCookingTime_Button, secoundCookingTime_UIListltem, 1600));

        first_IncreaseFoodPrice_Button.onClick.AddListener(() => HandleSkillUpgrade(first_IncreaseFoodPrice_Button, first_IncreaseFoodPrice_UIListltem, 1200));
        secound_IncreaseFoodPrice_Button.onClick.AddListener(() => HandleSkillUpgrade(secound_IncreaseFoodPrice_Button, secound_IncreaseFoodPrice_UIListltem, 1600));
        third_IncreaseFoodPrice_Button.onClick.AddListener(() => HandleSkillUpgrade(third_IncreaseFoodPrice_Button, third_IncreaseFoodPrice_UIListltem, 3000));
        fourth_IncreaseFoodPrice_Button.onClick.AddListener(() => HandleSkillUpgrade(fourth_IncreaseFoodPrice_Button, fourth_IncreaseFoodPrice_UIListltem, 4000));

        firstSpeedPostion_Button.onClick.AddListener(() => HandleSkillUpgrade(firstSpeedPostion_Button, firstSpeedPostion_UIListltem, 1300));
        secoundSpeedPostion_Button.onClick.AddListener(() => HandleSkillUpgrade(secoundSpeedPostion_Button, secoundSpeedPostion_UIListltem, 1600));
        thirdSpeedPostion_Button.onClick.AddListener(() => HandleSkillUpgrade(thirdSpeedPostion_Button, thirdSpeedPostion_UIListltem, 3000));

        // Fill Image 업데이트
        levelProgressImage.fillAmount = 0;
    }


    private void DisableUIItem(GameObject uiItem)
    {
        if (uiItem != null)
        {
            uiItem.SetActive(false); // UI 비활성화
        }
    }

    // 스킬 업그레이드 처리 메소드
    void HandleSkillUpgrade(Button button, GameObject uiListItem, int cost)
    {
        // 자산이 충분한지 확인
        if (!PlayerHasEnoughCurrency(cost))
        {
            Debug.Log("자산이 부족합니다. 스킬 업그레이드 불가.");
            return; // 자산 부족 시 아무 작업도 하지 않음
        }

        // 자산이 충분할 경우 자산 차감 및 스킬 업그레이드
        DeductCurrency(cost);

        // 스킬 업그레이드 후 UI 비활성화
        uiListItem.SetActive(false);

        eventCount++; // 이벤트 카운터 증가
        // Fill Image 업데이트
        UpdateFillImage();

        Debug.Log("스킬 업그레이드 완료: " + button.name);
    }


    // Fill Image 업데이트 메소드
    private void UpdateFillImage()
    {
        float fillAmount = (float)eventCount / maxEvents; // 0에서 1로 계산
        levelProgressImage.fillAmount = fillAmount;

        // 41회 이벤트가 발생했을 때 추가 작업 (예: 로그 출력)
        if (eventCount >= maxEvents)
        {
            Debug.Log("모든 스킬 업그레이드가 완료되었습니다!");

            NextStageButton.SetActive(true);
        }
    }

    public void NextStageGo()
    {
        SceneManager.LoadScene("Second_GameScene");
    }

    // 요리 시간 감소 함수
    private void ReduceCookingTime(float reductionPercentage)
    {
        // 기존 감소율에 추가 (최대 50%까지)
        cookingTimeReduction = Mathf.Clamp(cookingTimeReduction + reductionPercentage, 0f, 0.5f);

        Debug.Log($"현재 요리 시간 감소율: {cookingTimeReduction * 100}%");
    }

    // 요리 시간 감소 함수
    public void ReduceCookingTime(int cost, float reductionPercentage)
    {
        if (PlayerHasEnoughCurrency(cost))
        {
            DeductCurrency(cost);
            cookingTimeReduction += reductionPercentage;
            cookingTimeReduction = Mathf.Min(cookingTimeReduction, 0.5f); // 최대 50%로 제한

            chefController.UpdateCookingTimeReduction(cookingTimeReduction);
            Debug.Log($"요리 시간이 {cookingTimeReduction * 100}% 감소되었습니다.");
        }
        else
        {
            Debug.Log("골드가 부족합니다.");
        }
    }

    // 음식 가격 증가 함수
    void IncreaseFoodPrices(float increasePercentage)
    {
        foodPriceIncreasePercentage += increasePercentage;
        foodPriceIncreasePercentage = Mathf.Min(foodPriceIncreasePercentage, 1f); // 최대 100%로 제한

        UpdateFoodPrices();
        Debug.Log($"음식 가격이 {foodPriceIncreasePercentage * 100}% 증가되었습니다.");
    }

    // 음식 가격 업데이트 함수
    void UpdateFoodPrices()
    {
        for (int i = 0; i < friedRiceSales.Length; i++)
        {
            friedRiceSales[i] = Mathf.RoundToInt(friedRiceSales[i] * (1 + foodPriceIncreasePercentage));
        }

        for (int i = 0; i < ramenSales.Length; i++)
        {
            ramenSales[i] = Mathf.RoundToInt(ramenSales[i] * (1 + foodPriceIncreasePercentage));
        }

        for (int i = 0; i < steakSales.Length; i++)
        {
            steakSales[i] = Mathf.RoundToInt(steakSales[i] * (1 + foodPriceIncreasePercentage));
        }

        // 레벨 UI 업데이트
        UpdateFriedRiceLevelUI();
        UpdateRamenLevelUI();
        UpdateSteakLevelUI();
    }

    // 이동 속도 증가 함수
    void IncreaseMoveSpeed(float increasePercentage)
    {
       
        Debug.Log($"이동 속도가 {increasePercentage * 100}% 증가되었습니다.");
    }



    // 배열을 최대 레벨로 제한하는 함수
    private int[] LimitArrayToMaxLevel(int[] array, int maxLevel)
    {
        if (array.Length > maxLevel)
        {
            int[] limitedArray = new int[maxLevel];
            Array.Copy(array, limitedArray, maxLevel);
            return limitedArray;
        }
        return array;
    }


    // friedRice 관련 UI 삭제 및 버튼 활성화
    private void DeleteFriedRiceUI()
    {
        if (friedRiceUI != null)
        {
            friedRiceUI.gameObject.SetActive(false);
            
        }
        if (friedRiceSelectionButton != null)
        {
            
            friedRiceSelectionButton.gameObject.SetActive(false);
        }
     

        // friedRiceButton 활성화
        if (friedRiceButton != null)
        {
            friedRiceButton.gameObject.SetActive(true);
            customerRespawner.CreateCustomer();

        }



    }

    // ramen 관련 UI 삭제 및 버튼 활성화
    private void DeleteRamenUI()
    {
        if (PlayerMoney >= RamenUnlookCost)  // 스테이크 해금 금액이 충분한지 확인
        {
            if (ramenUI != null)
            {
                ramenUI.gameObject.SetActive(false);
            }
            if (ramenSelectionButton != null)
            {
                ramenSelectionButton.gameObject.SetActive(false);
            }

            // ramenButton 활성화
            if (ramenButton != null)
            {
                ramenButton.gameObject.SetActive(true);
            }

            // 해금 비용 차감
            DeductCurrency(RamenUnlookCost);

            ramenOrderUnlook = true;
        }
        else
        {
            Debug.Log("스테이크 해금 비용이 부족합니다.");
            ramenOrderUnlook = false;
        }


    }

    // steak 관련 UI 삭제 및 버튼 활성화
    private void DeleteSteakUI()
    {
        if (PlayerMoney >= steakUnloookCost)  // 볶음밥 해금 금액이 충분한지 확인
        {
            if (steakUI != null)
            {
                steakUI.gameObject.SetActive(false);
            }
            if (steakSelectionButton != null)
            {
                steakSelectionButton.gameObject.SetActive(false);
            }

            // steakButton 활성화
            if (steakButton != null)
            {
                steakButton.gameObject.SetActive(true);
            }

            // 해금 비용 차감
            DeductCurrency(steakUnloookCost);

            steakOrderUnlook = true;
        }
        else
        {
            Debug.Log("볶음밥 해금 비용이 부족합니다.");
            steakOrderUnlook = false;

        }


    }


    // EventTrigger에 클릭 이벤트 추가
    private void AddEventTrigger(GameObject obj, EventTriggerType type, System.Action<BaseEventData> action)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = obj.AddComponent<EventTrigger>();
        }

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener((eventData) => { action(eventData); });
        trigger.triggers.Add(entry);
    }

    // Panel 클릭 시 호출되는 메서드
    private void OnBackgroundClick(BaseEventData data)
    {
        // UI 비활성화 처리
        if (friedRiceUI.activeSelf && friedRiceUI != null)
        {
            friedRiceUI.SetActive(false);
        }

        if (ramenUI.activeSelf && ramenUI != null)
        {
            ramenUI.SetActive(false);
        }
        if (steakUI.activeSelf && steakUI != null)
        {
            steakUI.SetActive(false);
        }
    }


    // 볶음밥 UI 표시
    void ShowFriedRiceUI()
    {
        if(friedRiceUI.activeSelf == false)
            friedRiceUI.SetActive(true);  // UI 표시
        
    }

    // 라면 UI 표시
    void ShowRamenUI()
    {
        if (ramenUI.activeSelf == false)
            ramenUI.SetActive(true);  // UI 표시
       
    }

    // 스테이크 UI 표시
    void ShowSteakUI()
    {
        if (steakUI.activeSelf == false)
            steakUI.SetActive(true);  // UI 표시
       
    }

    void CompletePurchase(GameObject ui, Button originalButton)
    {
        ui.SetActive(false);  // UI 숨기기
        originalButton.gameObject.SetActive(true);  // 원래 버튼 활성화
    }

    void ActivatePassivePanel()
    {
        passivePanel.SetActive(true);
    }

    void DeactivatePassivePanel()
    {
        passivePanel.SetActive(false);
    }

    public static string FormatPrice(long amount)
    {
        string suffix = ""; // 기본 접미사 초기화
        double value = amount;

        // 1000원 이하일 경우 접미사 없음
        if (amount <= 1000)
        {
            return $"{amount}";
        }

        // 접미사 결정
        if (amount >= 1000000000000) // 1조 이상
        {
            value /= 1000000000000;
            suffix = "d";
        }
        else if (amount >= 1000000000) // 10억 이상
        {
            value /= 1000000000;
            suffix = "c";
        }
        else if (amount >= 1000000) // 100만 이상
        {
            value /= 1000000;
            suffix = "b";
        }
        else if (amount >= 1000) // 1000 이상
        {
            value /= 1000;
            suffix = "a";
        }

        // 소수점 2자리로 포맷하고 접미사 추가
        return $"{value:F2}{suffix}";
    }


    void AddServing()
    {
        if (PlayerMoney >= servingCost)
        {
            Serving.SetActive(true);
            DeductCurrency(servingCost);
        }
        else
        {
            Debug.Log("Not enough gold for Serving.");
        }
    }

    void AddChef()
    {
        if (PlayerMoney >= chefCost)
        {
            Chef.SetActive(true);
            DeductCurrency(chefCost);

            Debug.Log("새 쉐프 추가됨");
        }
        else
        {
            Debug.Log("Chef를 추가할 골드가 부족합니다.");
        }
    }

    public int GetSteakSalePrice()
    {
        if (steakLevel <= steakSales.Length)
            return steakSales[steakLevel - 1];
        return 0;
    }

    public int GetRamenSalePrice()
    {
        if (ramenLevel <= ramenSales.Length)
            return ramenSales[ramenLevel - 1];
        return 0;
    }

    public int GetFriedRiceSalePrice()
    {
        if (friedRiceLevel <= friedRiceSales.Length)
            return friedRiceSales[friedRiceLevel - 1];
        return 0;
    }


    void ActivatePanel(GameObject panel)
    {
        friedRicePanel.SetActive(false);
        ramenPanel.SetActive(false);
        steakPanel.SetActive(false);

        panel.SetActive(true);
    }

    void DeactivatePanel(GameObject panel)
    {
        panel.SetActive(false);
    }

    // 점수를 추가하는 메서드
    public void AddScore(int points)
    {
        PlayerMoney += points;
        Text_Gold.text = FormatPrice(PlayerMoney);  // 포맷팅 적용
        Debug.Log("점수 추가: " + points + ", 현재 점수: " + PlayerMoney);
    }



    // 스테이크 레벨 UI 업데이트
    void UpdateSteakLevelUI()
    {
        if (steakLevel < steakPrices.Length)
        {
            steakLevelUpButtonText.text = FormatPrice(steakPrices[steakLevel - 1]);
            steakPriceText.text = FormatPrice(steakSales[steakLevel - 1]);
            steakLevelText.text = "Lv." + steakLevel; // 레벨 텍스트 업데이트
        }
        else
        {
            steakLevelUpButtonText.text = "Max";
            steakLevelUpButton.interactable = false;
            steakLevelText.text = $"Lv.{steakLevel}"; // 최대 레벨 텍스트 업데이트
        }

        UpdateStarImages(steakLevel, steakStarImages);
        steakGaugeBar.fillAmount = (float)steakLevel / steakPrices.Length;

        // 이벤트 카운터 증가
        eventCount++;
        UpdateFillImage(); // Fill Image 업데이트
    }

    // 볶음밥 레벨 UI 업데이트
    void UpdateFriedRiceLevelUI()
    {
        if (friedRiceLevel < friedRicePrices.Length)
        {
            friedRiceLevelUpButtonText.text = FormatPrice(friedRicePrices[friedRiceLevel - 1]);
            friedRicePriceText.text = FormatPrice(friedRiceSales[friedRiceLevel - 1]);
            friedRiceLevelText.text = "Lv." + friedRiceLevel; // 레벨 텍스트 업데이트

        }
        else
        {
            friedRiceLevelUpButtonText.text = "Max";
            friedRiceLevelUpButton.interactable = false;
            friedRiceLevelText.text = $"Lv.{friedRiceLevel}"; // 최대 레벨 텍스트 업데이트

        }

        UpdateStarImages(friedRiceLevel, friedRiceStarImages);
        friedRiceGaugeBar.fillAmount = (float)friedRiceLevel / friedRicePrices.Length;

        // 이벤트 카운터 증가
        eventCount++;
        UpdateFillImage(); // Fill Image 업데이트
    }

    // 라면 레벨 UI 업데이트
    void UpdateRamenLevelUI()
    {
        if (ramenLevel < ramenPrices.Length)
        {
            ramenLevelUpButtonText.text = FormatPrice(ramenPrices[ramenLevel - 1]);
            ramenPriceText.text = FormatPrice(ramenSales[ramenLevel - 1]);
            ramenLevelText.text = "Lv." + ramenLevel; // 레벨 텍스트 업데이트
        }
        else
        {
            ramenLevelUpButtonText.text = "Max";
            ramenLevelUpButton.interactable = false;
            ramenLevelText.text = $"Lv.{ramenLevel}"; // 최대 레벨 텍스트 업데이트


        }

        UpdateStarImages(ramenLevel, ramenStarImages);
        ramenGaugeBar.fillAmount = (float)ramenLevel / ramenPrices.Length;

        // 이벤트 카운터 증가
        eventCount++;
        UpdateFillImage(); // Fill Image 업데이트
    }

    [Space(9), Header("음식 레벨의 별 활성화 레벨")]
    [SerializeField, Range(1, 2)] private int stage = 1;
    [System.Serializable] private class ActiveLevels
    {
        [Range(1, 2)] public int stage = 1;

        public List<int> targetLevels;

        public bool IsActive(int index, int currentLevel)
        {
            if (targetLevels == null || targetLevels.Count <= 0) return false;
            if (index >= targetLevels.Count) return false;

            return targetLevels[index] <= currentLevel;
        }
    }

    [SerializeField] private List<ActiveLevels> activeStarCondition; 


    // 별 이미지 업데이트 (레벨에 따라 별 활성화)
    void UpdateStarImages(int level, List<Image> starImages)
    {
        ActiveLevels target = activeStarCondition.FirstOrDefault(x => x.stage == stage);
        target ??= activeStarCondition.FirstOrDefault();

        for (int i = 0; i < starImages.Count; i++)
        {
            starImages[i].gameObject.SetActive(target.IsActive(i, level));
        }

        //if (level >= 3)
        //    starImages[0].gameObject.SetActive(true);
        //if (level >= 7)
        //    starImages[1].gameObject.SetActive(true);
        //if (level >= 10)
        //    starImages[2].gameObject.SetActive(true);
    }

    // 스테이크 레벨업
    void LevelUpSteak()
    {
        int nextPrice = steakPrices[steakLevel - 1];
        if (PlayerHasEnoughCurrency(nextPrice))
        {
            steakLevel++;
            DeductCurrency(nextPrice);
            UpdateSteakLevelUI();
        }
    }

    // 볶음밥 레벨업
    void LevelUpFriedRice()
    {
        int nextPrice = friedRicePrices[friedRiceLevel - 1];
        if (PlayerHasEnoughCurrency(nextPrice))
        {
            friedRiceLevel++;
            DeductCurrency(nextPrice);
            UpdateFriedRiceLevelUI();
        }
    }

    // 라면 레벨업
    void LevelUpRamen()
    {
        int nextPrice = ramenPrices[ramenLevel - 1];
        if (PlayerHasEnoughCurrency(nextPrice))
        {
            ramenLevel++;
            DeductCurrency(nextPrice);
            UpdateRamenLevelUI();
        }
    }

    // 플레이어 자산 확인
    bool PlayerHasEnoughCurrency(int amount)
    {
        return PlayerMoney >= amount;
    }

    // 플레이어 자산 차감
    void DeductCurrency(int amount)
    {
        PlayerMoney -= amount;
        Text_Gold.text = FormatPrice(PlayerMoney);  // 포맷팅 적용
        Debug.Log("자산 차감: " + amount + ", 현재 점수: " + FormatPrice(PlayerMoney));
    }
}